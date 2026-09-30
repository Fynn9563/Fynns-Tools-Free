using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	// Deletes the ClientSimStorage folder next to Assets after play mode ends.
	[InitializeOnLoad]
	internal static class ClientSimStorage
	{
		private const string LogPrefix = "[ClientSim Storage Cleaner] ";
		private const string FolderName = "ClientSimStorage";

		// ClientSim can still be writing a file when play mode ends, so the delete is retried.
		private const int MaxAttempts = 12;
		private const double SecondsBetweenAttempts = 0.25;

		private static int _attempts;
		private static double _nextAttemptAt;

		public static string FolderPath
		{
			get { return Path.Combine(Path.GetDirectoryName(Application.dataPath), FolderName); }
		}

		public static bool Exists
		{
			get { return Directory.Exists(FolderPath); }
		}

		static ClientSimStorage()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state != PlayModeStateChange.EnteredEditMode) return;
			if (!ClientSimStorageCleanerSettings.Enabled) return;
			if (!Exists) return;

			_attempts = 0;
			_nextAttemptAt = EditorApplication.timeSinceStartup;
			EditorApplication.update -= TryClear;
			EditorApplication.update += TryClear;
		}

		private static void TryClear()
		{
			if (EditorApplication.timeSinceStartup < _nextAttemptAt) return;

			_attempts++;
			if (Clear(out string error))
			{
				EditorApplication.update -= TryClear;
				return;
			}

			if (_attempts >= MaxAttempts)
			{
				EditorApplication.update -= TryClear;
				Debug.LogWarning(LogPrefix + "Could not delete " + FolderPath + ": " + error
					+ ". Use Tools > Fynn's Tools > ClientSim Storage Cleaner > Clear Now once ClientSim has finished writing.");
				return;
			}

			_nextAttemptAt = EditorApplication.timeSinceStartup + SecondsBetweenAttempts;
		}

		// True when the folder is gone.
		public static bool Clear(out string error)
		{
			error = null;
			string path = FolderPath;
			if (!Directory.Exists(path)) return true;

			try
			{
				Directory.Delete(path, true);
			}
			catch (IOException exception)
			{
				error = exception.Message;
				return false;
			}
			catch (UnauthorizedAccessException exception)
			{
				error = exception.Message;
				return false;
			}

			Debug.Log(LogPrefix + "Deleted " + path);
			return true;
		}
	}
}
