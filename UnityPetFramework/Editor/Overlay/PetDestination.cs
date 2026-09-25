using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Somewhere the pet has been told to walk to, and the region it is in.
	internal readonly struct PetDestination
	{
		public PetDestination(Vector2Int feet, RectInt region)
		{
			Feet = feet;
			Region = region;
			IsSet = true;
		}

		public Vector2Int Feet { get; }

		public RectInt Region { get; }

		public bool IsSet { get; }
	}
}
