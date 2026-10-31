using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	[InitializeOnLoad]
	internal static class ClientSimReloadProtection
	{
		private const string LogPrefix = "[ClientSim Storage Cleaner] ";
		private static bool _ownsReloadLock;

		static ClientSimReloadProtection()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorApplication.quitting += ReleaseReloadLock;
			if (EditorApplication.isPlaying) ProtectRunningSession();
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode) ProtectRunningSession();
			// ClientSim teardown must finish before assemblies can reload.
			else if (state == PlayModeStateChange.EnteredEditMode) ReleaseReloadLock();
		}

		private static void ProtectRunningSession()
		{
			if (_ownsReloadLock || !ClientSimStorageCleanerSettings.ProtectAssemblyReloads || !IsClientSimEnabled()) return;
			EditorApplication.LockReloadAssemblies();
			_ownsReloadLock = true;
			EditorApplication.update += ReleaseAfterCancelledTransition;
		}

		private static bool IsClientSimEnabled()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type settings = assembly.GetType("VRC.SDK3.ClientSim.ClientSimSettings", false);
				if (settings == null) continue;
				try
				{
					PropertyInfo instance = settings.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
					FieldInfo enabled = settings.GetField("enableClientSim", BindingFlags.Public | BindingFlags.Instance);
					if (instance == null || enabled == null || enabled.FieldType != typeof(bool))
					{
						Debug.LogWarning(LogPrefix + "ClientSim settings layout has changed; assembly reload protection was skipped.");
						return false;
					}
					object value = instance.GetValue(null);
					return value != null && (bool)enabled.GetValue(value);
				}
				catch (Exception exception)
				{
					Debug.LogWarning(LogPrefix + "Could not read ClientSim settings for assembly reload protection: " + exception);
					return false;
				}
			}
			return false;
		}

		private static void ReleaseAfterCancelledTransition()
		{
			if (!ClientSimStorageCleanerSettings.ProtectAssemblyReloads ||
				(!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode))
				ReleaseReloadLock();
		}

		private static void ReleaseReloadLock()
		{
			if (!_ownsReloadLock) return;
			EditorApplication.UnlockReloadAssemblies();
			_ownsReloadLock = false;
			EditorApplication.update -= ReleaseAfterCancelledTransition;
		}
	}
}
