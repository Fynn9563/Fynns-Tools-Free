using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The only place that knows the overlay has more than one implementation.
	internal static class PetOverlayFactory
	{
		public const string NotWindowsReason =
			"The pet is only shown on Windows. Everything else in this tool, including building and sharing pets, works the same everywhere.";

		public static bool IsSupportedPlatform
		{
			get
			{
#if UNITY_EDITOR_WIN
				return true;
#else
				return false;
#endif
			}
		}

		public static IPetHost CreateHost()
		{
#if UNITY_EDITOR_WIN
			return new EditorHostWindow();
#else
			return new NullPetHost();
#endif
		}

		// Returns something usable either way.
		public static IPetOverlay CreateOverlay(IPetHost host, Vector2Int frameSize)
		{
#if UNITY_EDITOR_WIN
			if (!(host is EditorHostWindow editorHost))
			{
				return new NullPetOverlay("The pet could not work out where the Unity window is.");
			}

			LayeredPetOverlay overlay = LayeredPetOverlay.TryCreate(editorHost.Owner, frameSize, out string error);
			return overlay != null ? (IPetOverlay)overlay : new NullPetOverlay(error);
#else
			return new NullPetOverlay(NotWindowsReason);
#endif
		}

		// Destroys any pet window left behind by a previous domain.
		public static int SweepOrphans()
		{
#if UNITY_EDITOR_WIN
			return LayeredPetOverlay.SweepOrphans();
#else
			return 0;
#endif
		}
	}
}
