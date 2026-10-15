using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	[InitializeOnLoad]
	internal static class PetOverlayHost
	{
		private const string SessionFeetKey = "FynnsTools.UnityPetFramework.Session.Feet";

		private const int MaxStartupAttempts = 60;

		private const float MaxDeltaSeconds = 0.25f;

		private static IPetOverlay _overlay;
		private static IPetHost _host;
		private static PetController _controller;
		private static PetEnergyBar _energyBar;

		private static double _lastTick;
		private static int _startupAttempts;
		private static bool _running;
		private static string _lastError;

		private static bool _suppressPersist;

		static PetOverlayHost()
		{
			int swept = PetOverlayFactory.SweepOrphans();
			if (swept > 0)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix +
					"Cleared " + swept + " leftover pet window(s) from before the last script reload.");
			}

			AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
			EditorApplication.quitting += Shutdown;

			EditorApplication.delayCall += StartIfEnabled;
		}

		public static bool IsRunning => _running;

		public static string StatusMessage
		{
			get
			{
				if (!PetOverlayFactory.IsSupportedPlatform)
				{
					return PetOverlayFactory.NotWindowsReason;
				}

				if (!string.IsNullOrEmpty(_lastError))
				{
					return _lastError;
				}

				return _running ? null : "The pet is switched off.";
			}
		}

		public static float Energy => _controller?.Energy ?? UnityPetFrameworkSettings.Energy;

		public static void SetEnabled(bool enabled)
		{
			UnityPetFrameworkSettings.Enabled = enabled;

			if (enabled)
			{
				Start();
			}
			else
			{
				Shutdown();
			}
		}

		public static void Restart()
		{
			if (!UnityPetFrameworkSettings.Enabled)
			{
				return;
			}

			Shutdown();
			Start();
		}

		public static void ResetEverything()
		{
			bool wasEnabled = UnityPetFrameworkSettings.Enabled;

			_suppressPersist = true;
			try
			{
				Shutdown();
			}
			finally
			{
				_suppressPersist = false;
			}

			UnityPetFrameworkSettings.ResetAll();
			SessionState.EraseString(SessionFeetKey);
			PetEditorEvents.ClearPending();

			if (wasEnabled)
			{
				Start();
			}
		}

		public static void RefreshMovementArea()
		{
			if (_running && _controller != null && _host != null)
			{
				_controller.RefreshSafeArea(_host);
			}
		}

		public static void ApplyClickThrough()
		{
			_overlay?.SetClickThrough(UnityPetFrameworkSettings.ClickThrough);
		}

		private static void StartIfEnabled()
		{
			if (UnityPetFrameworkSettings.Enabled)
			{
				Start();
			}
		}

		private static void Start()
		{
			if (_running)
			{
				return;
			}

			_lastError = null;

			if (!PetOverlayFactory.IsSupportedPlatform)
			{
				_lastError = PetOverlayFactory.NotWindowsReason;
				return;
			}

			_host = PetOverlayFactory.CreateHost();
			_host.Poll();

			if (!_host.IsReady)
			{
				if (++_startupAttempts <= MaxStartupAttempts)
				{
					EditorApplication.delayCall += Start;
					return;
				}

				_lastError = "Unity's main window could not be found, so the pet cannot be placed over it.";
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + _lastError);
				return;
			}

			_startupAttempts = 0;

			if (!TryLoadPet(out PetAtlas atlas, out PetDefinition definition, out string petError))
			{
				_lastError = petError;
				return;
			}

			if (UnityPetFrameworkSettings.ScaleInitialisedFor != definition.id)
			{
				UnityPetFrameworkSettings.Scale = definition.defaultScale > 0f ? definition.defaultScale : 1f;
				UnityPetFrameworkSettings.ScaleInitialisedFor = definition.id;
			}

			_controller = new PetController();
			_controller.SetPet(atlas, definition, UnityPetFrameworkSettings.Scale);

			LoadSounds(definition, Path.GetDirectoryName(PetLibrary.ResolveSelectedDefinitionPath()));

			_overlay = PetOverlayFactory.CreateOverlay(_host, _controller.FrameSize);
			if (!_overlay.IsAvailable)
			{
				_lastError = _overlay.UnavailableReason;
				Shutdown();
				return;
			}

			_overlay.SetClickThrough(UnityPetFrameworkSettings.ClickThrough);

			_energyBar = new PetEnergyBar();
			_energyBar.EnsureCreated(_host);
			_controller.RefreshSafeArea(_host);
			_controller.Place(RestorePosition(_host, _controller.FrameSize));

			PetEditorEvents.Subscribe();

			_lastTick = EditorApplication.timeSinceStartup;
			_running = true;
			EditorApplication.update += Tick;
			if (PetEditorEvents.TryTakePendingReaction(out PetState pending))
			{
				EditorApplication.delayCall += () => PetEditorEvents.RaisePending(pending);
			}
		}
		private static void Shutdown()
		{
			if (!_running && _overlay == null && _controller == null)
			{
				return;
			}

			_running = false;

			try
			{
				EditorApplication.update -= Tick;
				PetEditorEvents.Unsubscribe();
				PetAudio.StopAll();
				PetAudio.ReleaseAll();
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Detaching the pet's editor hooks failed: " + exception.Message);
			}

			try
			{
				if (_controller != null && !_suppressPersist)
				{
					UnityPetFrameworkSettings.Energy = _controller.Energy;
					SessionState.SetString(SessionFeetKey, _controller.Feet.x + "," + _controller.Feet.y);
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Saving the pet's state failed: " + exception.Message);
			}

			try
			{
				_energyBar?.Dispose();
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Closing the energy bar failed: " + exception.Message);
			}
			finally
			{
				_energyBar = null;
			}

			try
			{
				_overlay?.Dispose();
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Closing the pet window failed: " + exception.Message);
			}
			finally
			{
				_overlay = null;
			}

			try
			{
				_controller?.Dispose();
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Releasing the pet failed: " + exception.Message);
			}
			finally
			{
				_controller = null;
				_host = null;
			}
		}

		private static void Tick()
		{
			if (!_running || _overlay == null || _controller == null)
			{
				return;
			}

			double now = EditorApplication.timeSinceStartup;

			// Clamp elapsed time after imports and compilation.
			float delta = Mathf.Clamp((float)(now - _lastTick), 0f, MaxDeltaSeconds);
			_lastTick = now;

			try
			{
				_overlay.Pump();
				_host.Poll();

				if (!_host.ShouldShowPet)
				{
					_overlay.IsVisible = false;
					_energyBar?.Update(_host, _controller.Energy, false);
					return;
				}

				_controller.Tick(delta, _overlay.SamplePointer(), _host, now, _overlay.DoubleClickSeconds);

				byte[] frame = _controller.CurrentFrame;
				if (frame == null)
				{
					return;
				}

				_overlay.Present(frame, _controller.TopLeft);
				_overlay.IsVisible = true;

				_energyBar?.Update(_host, _controller.Energy, UnityPetFrameworkSettings.ShowEnergyBar);
			}
			catch (Exception exception)
			{
				// Stop a failed tick from throwing every frame.
				_lastError = "The pet hit a problem and has been switched off: " + exception.Message;
				Debug.LogError(UnityPetFrameworkInfo.LogPrefix + _lastError + "\n" + exception);
				Shutdown();
			}
		}

		private static void LoadSounds(PetDefinition definition, string petFolder)
		{
			PetAudio.ReleaseAll();

			if (string.IsNullOrEmpty(petFolder))
			{
				return;
			}

			foreach (PetClipDefinition clip in definition.clips)
			{
				if (string.IsNullOrEmpty(clip.sound))
				{
					continue;
				}

				string path = PetDefinitionSerializer.SafeCombineRelative(petFolder, clip.sound);
				if (path != null && File.Exists(path))
				{
					PetAudio.Register(clip.sound, File.ReadAllBytes(path));
				}
			}
		}

		private static bool TryLoadPet(out PetAtlas atlas, out PetDefinition definition, out string error)
		{
			string path = PetLibrary.ResolveSelectedDefinitionPath();
			if (string.IsNullOrEmpty(path))
			{
				atlas = null;
				definition = null;
				error = "No pet could be found to show.";
				return false;
			}

			return PetDefinitionSerializer.TryLoad(path, out definition, out atlas, out error);
		}

		private static Vector2Int RestorePosition(IPetHost host, Vector2Int frameSize)
		{
			string stored = SessionState.GetString(SessionFeetKey, "");
			string[] parts = stored.Split(',');

			if (parts.Length == 2 &&
				int.TryParse(parts[0], out int x) &&
				int.TryParse(parts[1], out int y))
			{
				return new Vector2Int(x, y);
			}

			RectInt bounds = host.HostBoundsPx;
			return new Vector2Int(bounds.xMin + frameSize.x, bounds.yMax - 8);
		}
	}
}
