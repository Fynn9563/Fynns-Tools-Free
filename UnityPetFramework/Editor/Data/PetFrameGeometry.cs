using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal static class PetFrameGeometry
	{
		// Symmetric horizontal envelope keeps the same desktop anchor when flipped.
		public static RectInt Canvas(PetDefinition definition, PetClipDefinition only = null)
		{
			float half = 1, above = 1, below = 0;
			IEnumerable<PetClipDefinition> clips = only != null ? new[] { only } : definition.clips;
			foreach (var clip in clips)
			{
				if (clip?.frames == null) continue;
				foreach (var reference in clip.frames)
				{
					if (reference.cell < 0 || reference.cell >= definition.spriteFrames.Count) continue;
					var f = definition.spriteFrames[reference.cell];
					half = Mathf.Max(half, Mathf.Max(f.pivotX, f.width - f.pivotX));
					above = Mathf.Max(above, f.pivotY); below = Mathf.Max(below, f.height - f.pivotY);
				}
			}
			int h = Mathf.CeilToInt(half), a = Mathf.CeilToInt(above);
			return new RectInt(-h, -a, h * 2, a + Mathf.CeilToInt(below));
		}

		public static Rect DrawRect(Rect area, RectInt canvas, PetSpriteFrame frame, bool flip, float padding = 8)
		{
			float scale = Mathf.Min(Mathf.Max(1, area.width - padding * 2) / canvas.width, Mathf.Max(1, area.height - padding * 2) / canvas.height);
			float left = area.center.x - canvas.width * scale * .5f;
			float top = area.center.y - canvas.height * scale * .5f;
			float pivotX = flip ? frame.width - frame.pivotX : frame.pivotX;
			return new Rect(left + (-canvas.x - pivotX) * scale, top + (-canvas.y - frame.pivotY) * scale, frame.width * scale, frame.height * scale);
		}
	}
}
