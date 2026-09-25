using System;
using System.Collections.Generic;

namespace FynnsTools.UnityPetFramework
{
	internal static class PetDefinitionValidator
	{
		public static bool Validate(PetDefinition d, int atlasWidth, int atlasHeight, out string error)
		{
			var errors = new List<string>();
			if (d == null || d.schemaVersion != UnityPetFrameworkInfo.PetSchemaVersion)
			{
				error = "This tool requires pet schema " + UnityPetFrameworkInfo.PetSchemaVersion + ".";
				return false;
			}
			if (string.IsNullOrWhiteSpace(d.id) || string.IsNullOrWhiteSpace(d.displayName)) errors.Add("Give the pet an id and name.");
			if (!SafeImage(d.atlas)) errors.Add("The atlas must be a PNG file name.");
			if (!string.IsNullOrEmpty(d.thumbnail) && !SafeImage(d.thumbnail)) errors.Add("Invalid thumbnail file name.");
			if (!Finite(d.defaultScale) || d.defaultScale < .05f || d.defaultScale > 4) errors.Add("Default scale must be between 0.05 and 4.");
			if (d.HasRectangles)
			{
				if (d.spriteFrames.Count > 4096) errors.Add("At most 4096 atlas frames are supported.");
				foreach (var f in d.spriteFrames)
				{
					if (f == null || f.x < 0 || f.y < 0 || f.width < 1 || f.height < 1 ||
						(long)f.x + f.width > (atlasWidth > 0 ? atlasWidth : 16384) ||
						(long)f.y + f.height > (atlasHeight > 0 ? atlasHeight : 16384) ||
						!Finite(f.pivotX) || !Finite(f.pivotY) || f.pivotX < 0 || f.pivotX > f.width || f.pivotY < 0 || f.pivotY > f.height)
					{ errors.Add("A frame rectangle or anchor is outside its image/frame."); break; }
				}
			}
			else
			{
				if (d.columns < 1 || d.rows < 1 || (long)d.columns * d.rows > 4096 || d.cellWidth < 1 || d.cellHeight < 1 ||
					(long)d.columns * d.cellWidth > 16384 || (long)d.rows * d.cellHeight > 16384)
					errors.Add("Invalid frame grid.");
				if (atlasWidth > 0 && ((long)d.columns * d.cellWidth != atlasWidth || (long)d.rows * d.cellHeight != atlasHeight)) errors.Add("The grid does not match the image.");
				if (d.contentWidth <= 0 || d.contentHeight <= 0 || d.contentLeft < 0 || d.contentTop < 0 ||
					(long)d.contentLeft + d.contentWidth > d.cellWidth || (long)d.contentTop + d.contentHeight > d.cellHeight) errors.Add("Invalid drawn area.");
			}
			if (d.clips == null || d.clips.Count == 0 || d.clips.Count > 128) errors.Add("A pet needs between 1 and 128 animations.");
			var names = new HashSet<string>(StringComparer.Ordinal);
			foreach (var clip in d.AllSequences())
			{
				// Named, and with the reason.
			if (string.IsNullOrWhiteSpace(clip.name)) errors.Add("An animation has no name.");
			else if (clip.name.Length > 80) errors.Add("The animation name '" + clip.name.Substring(0, 40) + "...' is longer than 80 characters.");
			else if (!names.Add(clip.name)) errors.Add("There is more than one animation called '" + clip.name + "'.");
				if (!Finite(clip.fps) || clip.fps < 1 || clip.fps > 60) errors.Add("Animation FPS must be between 1 and 60.");
			if (clip.drawnFacing < 0 || clip.drawnFacing > 2) errors.Add("An animation has an unknown drawn facing.");
			if (!Finite(clip.holdSeconds) || clip.holdSeconds < 0 || clip.holdSeconds > 10) errors.Add("An animation hold must be between 0 and 10 seconds.");
				if (clip.frames == null || clip.frames.Count > 1024) { errors.Add("Invalid animation frame list (maximum 1024)."); continue; }
				foreach (var f in clip.frames) if (f.cell < 0 || f.cell >= d.CellCount) { errors.Add("An animation refers to a missing atlas frame."); break; }
				if (!string.IsNullOrEmpty(clip.sound) && (!PetPackageFormat.IsSafeEntryName(clip.sound) || !clip.sound.StartsWith("sounds/", StringComparison.Ordinal) || !clip.sound.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))) errors.Add("Sounds must be WAV files in sounds/.");
			}
			if (d.icon != null && d.icon.name != PetClipSlots.PetIcon)
			errors.Add("The icon sequence is named '" + (d.icon.name ?? "") + "' and has " + d.icon.FrameCount + " frames; it must be named " + PetClipSlots.PetIcon + ".");
			if (d.clips != null && d.clips.Exists(c => c == null || c.name == PetClipSlots.PetIcon)) errors.Add("Invalid animation; Pet Icon belongs in the icon field.");
			if (!d.HasClip(d.ResolveBehaviour("Idle"))) errors.Add("Assign a primary idle animation (Stand 1 by default).");
			var mapped = new HashSet<string>();
			if (d.behaviours != null)
			{
				if (d.behaviours.Count > 64) errors.Add("Too many behaviour mappings.");
				foreach (var m in d.behaviours)
					if (m == null || Array.IndexOf(PetBehaviours.Names, m.behaviour) < 0 || !mapped.Add(m.behaviour) ||
						(!string.IsNullOrEmpty(m.animation) && (m.animation == PetClipSlots.PetIcon || !d.HasClip(m.animation)))) errors.Add("A behaviour mapping is invalid or names an empty animation.");
			}
			error = errors.Count == 0 ? null : "This pet cannot be used:\n" + string.Join("\n", errors);
			return errors.Count == 0;
		}
		private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
		private static bool SafeImage(string name) => !string.IsNullOrEmpty(name) && name.IndexOf('/') < 0 &&
			PetPackageFormat.IsSafeEntryName(name) && name.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
	}
}
