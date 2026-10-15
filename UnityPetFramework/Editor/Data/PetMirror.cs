namespace FynnsTools.UnityPetFramework
{
	internal enum PetDrawnFacing
	{
		SameAsPet = 0,
		Right = 1,
		Left = 2
	}

	internal static class PetMirror
	{
		public static bool ArtFacesRight(PetDefinition definition, PetClipDefinition clip)
		{
			if (clip != null && clip.drawnFacing != (int)PetDrawnFacing.SameAsPet)
			{
				return clip.drawnFacing == (int)PetDrawnFacing.Right;
			}

			return definition == null || definition.artFacesRight;
		}

		public static bool ShouldMirror(PetDefinition definition, PetClipDefinition clip, PetFrameRef frame, PetFacing facing)
		{
			bool facingMirror = false;

			if (clip == null || clip.allowFlip)
			{
				facingMirror = (facing == PetFacing.Right) != ArtFacesRight(definition, clip);
			}

			return frame.flip ^ facingMirror;
		}
	}
}
