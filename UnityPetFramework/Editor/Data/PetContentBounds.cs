using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Works out how much of a cell a pet actually draws in.
	internal static class PetContentBounds
	{
		// Recalculates and writes the result onto the definition.
		public static bool Recalculate(PetDefinition definition, PetAtlas atlas, out string error)
		{
			if (definition == null || atlas == null)
			{
				error = "There is no pet to measure.";
				return false;
			}

			List<int> used = definition.UsedCells();
			if (definition.HasRectangles)
			{
				var canvas = PetFrameGeometry.Canvas(definition);
				definition.contentLeft = 0; definition.contentTop = 0;
				definition.contentWidth = canvas.width; definition.contentHeight = canvas.height;
				error = used.Count == 0 ? "Assign some animation frames first." : null;
				return error == null;
			}
			if (used.Count == 0)
			{
				error = "This pet has no frames yet, so there is nothing to measure.";
				return false;
			}

			RectInt bounds = atlas.ComputeContentBounds(used);
			if (bounds.width <= 0 || bounds.height <= 0)
			{
				error = "Nothing is drawn in any of the frames this pet uses.";
				return false;
			}

			definition.contentLeft = bounds.x;
			definition.contentTop = bounds.y;
			definition.contentWidth = bounds.width;
			definition.contentHeight = bounds.height;

			error = null;
			return true;
		}

		// True when the stored box no longer matches the frames in use.
		public static bool IsStale(PetDefinition definition, PetAtlas atlas)
		{
			if (definition == null || atlas == null)
			{
				return false;
			}

			List<int> used = definition.UsedCells();
			if (used.Count == 0)
			{
				return false;
			}

			RectInt bounds = atlas.ComputeContentBounds(used);
			return bounds.x != definition.contentLeft
				|| bounds.y != definition.contentTop
				|| bounds.width != definition.contentWidth
				|| bounds.height != definition.contentHeight;
		}
	}
}
