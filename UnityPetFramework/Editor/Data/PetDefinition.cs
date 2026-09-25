using System;
using System.Collections.Generic;

// Every field below is written by JsonUtility through reflection.
#pragma warning disable 0649

namespace FynnsTools.UnityPetFramework
{
	// What a pet is, on disk.
	[Serializable]
	internal sealed class PetDefinition
	{
		// The format contract.
		public int schemaVersion;

		// Which build of the tool wrote the file.
		public string frameworkVersion;

		public string id;
		public string displayName;
		public string author;
		public string version;
		public string license;

		// File names relative to the pet folder, never paths.
		public string atlas;
		public string thumbnail;

		public int columns;
		public int rows;
		public int cellWidth;
		public int cellHeight;

		// Schema 2: top-left pixel rectangles and local, top-left pixel anchors.
		public List<PetSpriteFrame> spriteFrames = new List<PetSpriteFrame>();
		public PetClipDefinition icon;
		public List<PetBehaviourMapping> behaviours = new List<PetBehaviourMapping>();
		public bool allowPickup = true;
		public bool physicalJump = true;
		public bool HasRectangles => spriteFrames != null && spriteFrames.Count > 0;

		// Which way the artwork is drawn facing.
		public bool artFacesRight = true;

		// The drawn part of a cell.
		public int contentLeft;
		public int contentTop;
		public int contentWidth;
		public int contentHeight;

		public float defaultScale = 1f;

		// One entry per slot in PetClipSlots.
		public List<PetClipDefinition> clips = new List<PetClipDefinition>();

		public int CellCount => HasRectangles ? spriteFrames.Count : columns * rows;

		// Undoes what JsonUtility does to an absent icon.
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

			// The name is structural rather than a choice: the icon is whatever is in this field.
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

		// Every cell any animation uses, once each, in first use order.
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

		// The row the pet stands on, in cell-local pixels.
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

		// True when the pet actually has something to play for a slot.
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
		// One of the names in PetClipSlots.
		public string name;

		// The frames in the order they play, which is the order they were clicked.
		public List<PetFrameRef> frames = new List<PetFrameRef>();

		public float fps = 7f;
		public bool loop = true;

		// Whether the renderer may mirror this animation to face the other way.
		public bool allowFlip = true;

		// Which way this animation's artwork is drawn, when it disagrees with the pet.
		public int drawnFacing;

		// Seconds to stay on the last frame after this animation finishes.
		public float holdSeconds;

		// Optional, relative to the pet folder, e.g.
		public string sound;

		public int FrameCount => frames?.Count ?? 0;
	}

	// One frame: which cell of the sheet, and whether the creator chose to mirror it by hand.
	[Serializable]
	public struct PetFrameRef
	{
		// Flat index into the sheet: row * columns + column.
		public int cell;

		// Drawn mirrored left to right.
		public bool flip;

		public PetFrameRef(int cell, bool flip)
		{
			this.cell = cell;
			this.flip = flip;
		}
	}
}
