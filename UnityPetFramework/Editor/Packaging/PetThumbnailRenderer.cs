using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The still picture a pet is shown by, taken from the first frame of its Pet Icon animation.
	internal static class PetThumbnailRenderer
	{
		private const int MaxSide = 512;

		// Returns null when the pet has no icon.
		public static byte[] Render(PetAtlas atlas, PetDefinition definition)
		{
			if (atlas == null || definition?.icon == null || definition.icon.FrameCount == 0)
			{
				return null;
			}

			int cell = definition.icon.frames[0].cell;
			if (!atlas.IsValidCell(cell) || !definition.HasRectangles)
			{
				return null;
			}

			PetSpriteFrame frame = definition.spriteFrames[cell];
			if (frame.width < 1 || frame.height < 1 || frame.width > MaxSide || frame.height > MaxSide)
			{
				return null;
			}

			Texture2D texture = new Texture2D(frame.width, frame.height, TextureFormat.RGBA32, false, false)
			{
				hideFlags = HideFlags.HideAndDontSave
			};

			try
			{
				Color32[] pixels = new Color32[frame.width * frame.height];

				for (int y = 0; y < frame.height; y++)
				{
					// GetPixel counts down from the top of the cell.
					int row = (frame.height - 1 - y) * frame.width;
					for (int x = 0; x < frame.width; x++)
					{
						pixels[row + x] = atlas.GetPixel(cell, x, y);
					}
				}

				texture.SetPixels32(pixels);
				texture.Apply(false, false);
				return texture.EncodeToPNG();
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}
	}
}
