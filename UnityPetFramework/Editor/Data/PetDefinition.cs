using System;
using System.Collections.Generic;

#pragma warning disable 0649

namespace FynnsTools.UnityPetFramework
{
	[Serializable]
	internal sealed class PetDefinition
	{
		public int schemaVersion;

		public string frameworkVersion;

		public string id;
		public string displayName;
		public string author;
		public string version;
		public string license;

		public string atlas;
		public string thumbnail;

		public int columns;
		public int rows;
		public int cellWidth;
		public int cellHeight;

		// Schema 2 uses top-left pixel rectangles and local anchors.
		public List<PetSpriteFrame> spriteFrames = new List<PetSpriteFrame>();
		public PetClipDefinition icon;
		public List<PetBehaviourMapping> behaviours = new List<PetBehaviourMapping>();
		public bool allowPickup = true;
		public bool physicalJump = true;
		public bool HasRectangles => spriteFrames != null && spriteFrames.Count > 0;

		public bool artFacesRight = true;

		public int contentLeft;
		public int contentTop;
		public int contentWidth;
		public int contentHeight;

		public float defaultScale = 1f;

		public List<PetClipDefinition> clips = new List<PetClipDefinition>();

		public int CellCount => HasRectangles ? spriteFrames.Count : columns * rows;

		// Restore the default icon when JSON omits it.
		public PetDefinition Normalise()
		{
			clips?.RemoveAll(clip => clip == null);
			behaviours?.RemoveAll(mapping => mapping == null);
			spriteFrames?.RemoveAll(frame => frame == null);

			if (icon == null) return this;

			if (icon.FrameCount == 0)
			{
				icon = null;
				return this;
			}

			icon.name = PetClipSlots.PetIcon;
			return this;
		}

		public IEnumerable<PetClipDefinition> AllSequences()
		{
			if (clips != null) foreach (var clip in clips) if (clip != null) yield return clip;
			if (icon != null) yield return icon;
		}

		public string ResolveBehaviour(string behaviour)
		{
			if (behaviours != null) foreach (var mapping in behaviours)
				if (mapping != null && mapping.behaviour == behaviour) return mapping.animation;
			return PetBehaviours.DefaultAnimation(this, behaviour);
		}

		public List<int> UsedCells()
		{
			List<int> used = new List<int>();
			HashSet<int> seen = new HashSet<int>();

			if (clips == null)
			{
				return used;
			}

			foreach (PetClipDefinition clip in AllSequences())
			{
				if (clip?.frames == null)
				{
					continue;
				}

				foreach (PetFrameRef frame in clip.frames)
				{
					if (seen.Add(frame.cell))
					{
						used.Add(frame.cell);
					}
				}
			}

			return used;
		}

		public int GroundBaseline => contentTop + contentHeight;

		public PetClipDefinition FindClip(string clipName)
		{
			if (clipName == PetClipSlots.PetIcon) return icon;
			if (clips == null || string.IsNullOrEmpty(clipName))
			{
				return null;
			}

			foreach (PetClipDefinition clip in clips)
			{
				if (clip != null && string.Equals(clip.name, clipName, StringComparison.Ordinal))
				{
					return clip;
				}
			}

			return null;
		}

		public bool HasClip(string clipName)
		{
			PetClipDefinition clip = FindClip(clipName);
			return clip != null && clip.FrameCount > 0;
		}
	}

	[Serializable]
	internal sealed class PetSpriteFrame
	{
		public int x, y, width, height;
		public float pivotX, pivotY;
		public UnityEngine.RectInt Rect => new UnityEngine.RectInt(x, y, width, height);
		public PetSpriteFrame Copy() => (PetSpriteFrame)MemberwiseClone();
	}

	[Serializable]
	internal sealed class PetBehaviourMapping
	{
		public string behaviour;
		public string animation;
	}

	[Serializable]
	internal sealed class PetClipDefinition
	{
		public string name;

		public List<PetFrameRef> frames = new List<PetFrameRef>();

		public float fps = 7f;
		public bool loop = true;

		public bool allowFlip = true;

		public int drawnFacing;

		public float holdSeconds;

		public string sound;

		public int FrameCount => frames?.Count ?? 0;
	}

	[Serializable]
	public struct PetFrameRef
	{
		public int cell;

		public bool flip;

		public PetFrameRef(int cell, bool flip)
		{
			this.cell = cell;
			this.flip = flip;
		}
	}
}
