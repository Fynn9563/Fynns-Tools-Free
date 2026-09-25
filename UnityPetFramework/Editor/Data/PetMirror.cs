namespace FynnsTools.UnityPetFramework
{
	// Which way a pet is drawn, per animation.
	internal enum PetDrawnFacing
	{
		SameAsPet = 0,
		Right = 1,
		Left = 2
	}

	// The whole of the left and right question, in one place.
	internal static class PetMirror
	{
		// Which way this animation's artwork is drawn.
		public static bool ArtFacesRight(PetDefinition definition, PetClipDefinition clip)
		{
			if (clip != null && clip.drawnFacing != (int)PetDrawnFacing.SameAsPet)
			{
				return clip.drawnFacing == (int)PetDrawnFacing.Right;
			}

			return definition == null || definition.artFacesRight;
		}

		// Whether to draw this frame mirrored.
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
