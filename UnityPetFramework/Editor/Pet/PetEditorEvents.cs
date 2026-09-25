using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Turns things happening in the editor into things the pet does.
	internal static class PetEditorEvents
	{
		// An error loop can produce hundreds of messages a second.
		private const double ErrorReactionCooldownSeconds = 5d;

		private const string CompileFailedKey = "FynnsTools.UnityPetFramework.Session.CompileFailed";
		private const string PendingReactionKey = "FynnsTools.UnityPetFramework.Session.PendingReaction";

		private static bool _subscribed;
		private static double _lastErrorReaction;

		// Raised with the state the pet should move to.
		public static event Action<PetState> Reaction;

		public static void Subscribe()
		{
			if (_subscribed)
			{
				return;
			}

			_subscribed = true;

			CompilationPipeline.compilationStarted += OnCompilationStarted;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			Application.logMessageReceived += OnLogMessage;
		}

		public static void Unsubscribe()
		{
			if (!_subscribed)
			{
				return;
			}

			_subscribed = false;

			CompilationPipeline.compilationStarted -= OnCompilationStarted;
			CompilationPipeline.compilationFinished -= OnCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompiled;
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			Application.logMessageReceived -= OnLogMessage;
		}

		// Whatever the pet was supposed to react to but could not.
		public static bool TryTakePendingReaction(out PetState state)
		{
			state = PetState.Idle;

			string stored = SessionState.GetString(PendingReactionKey, "");
			if (string.IsNullOrEmpty(stored))
			{
				return false;
			}

			SessionState.EraseString(PendingReactionKey);
			return Enum.TryParse(stored, out state);
		}

		// Replays a reaction that was stored before a reload.
		public static void RaisePending(PetState state)
		{
			Raise(state);
		}

		public static void ClearPending()
		{
			SessionState.EraseString(PendingReactionKey);
			SessionState.EraseBool(CompileFailedKey);
		}

		private static void Raise(PetState state)
		{
			if (!UnityPetFrameworkSettings.ReactToEditorEvents)
			{
				return;
			}

			Reaction?.Invoke(state);
		}

		// Stored rather than raised, for a reaction that belongs to the pet on the far side of a reload.
		private static void Defer(PetState state)
		{
			if (UnityPetFrameworkSettings.ReactToEditorEvents)
			{
				SessionState.SetString(PendingReactionKey, state.ToString());
			}
		}

		private static void OnCompilationStarted(object context)
		{
			SessionState.SetBool(CompileFailedKey, false);
			Raise(PetState.Think);
		}

		// Fired per assembly.
		private static void OnAssemblyCompiled(string assemblyPath, CompilerMessage[] messages)
		{
			foreach (CompilerMessage message in messages)
			{
				if (message.type == CompilerMessageType.Error)
				{
					SessionState.SetBool(CompileFailedKey, true);
					return;
				}
			}
		}

		private static void OnCompilationFinished(object context)
		{
			bool failed = SessionState.GetBool(CompileFailedKey, false);
			SessionState.EraseBool(CompileFailedKey);

			PetState outcome = failed ? PetState.Failure : PetState.Happy;

			// A failed compile leaves the domain alone, so the pet is still here and can react now.
			if (failed)
			{
				Raise(outcome);
				return;
			}

			Defer(outcome);
		}

		private static void OnPlayModeChanged(PlayModeStateChange change)
		{
			switch (change)
			{
				case PlayModeStateChange.EnteredPlayMode:
					Raise(PetState.Celebrate);
					break;

				case PlayModeStateChange.EnteredEditMode:
					Raise(PetState.Happy);
					break;
			}
		}

		private static void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
			{
				return;
			}

			// Compiler errors arrive through the Console too.
			if (EditorApplication.isCompiling)
			{
				return;
			}

			double now = EditorApplication.timeSinceStartup;
			if (now - _lastErrorReaction < ErrorReactionCooldownSeconds)
			{
				return;
			}

			_lastErrorReaction = now;
			Raise(PetState.Alert);
		}
	}
}
