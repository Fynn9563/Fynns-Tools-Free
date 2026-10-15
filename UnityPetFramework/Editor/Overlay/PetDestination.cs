using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
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
