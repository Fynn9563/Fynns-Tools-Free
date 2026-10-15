using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal static class PetEditorEvents
	{
		private const double ErrorReactionCooldownSeconds = 5d;

		private const string CompileFailedKey = "FynnsTools.UnityPetFramework.Session.CompileFailed";
		private const string PendingReactionKey = "FynnsTools.UnityPetFramework.Session.PendingReaction";

		private static bool _subscribed;
		private static double _lastErrorReaction;

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

		// Carry pending reactions across domain reload.
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

			// Handle failed compilation without expecting a domain reload.
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
