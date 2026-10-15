using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal static class PetIconRenderer
	{
		private const int MaxAtlasPixels = 16 * 1024 * 1024;

		private sealed class Entry
		{
			public Texture2D Atlas;
			public List<PetSpriteFrame> Frames = new List<PetSpriteFrame>();
			public float Fps = 6f;
			public bool Loop = true;
			public bool Failed;
		}

		private static readonly Dictionary<string, Entry> _cache = new Dictionary<string, Entry>();
		private static bool _hooked;

		public static bool AnyAnimated { get; private set; }

		public static void BeginFrame()
		{
			AnyAnimated = false;
		}

		public static bool Draw(Rect area, PetLibrary.Entry pet)
		{
			Entry entry = Resolve(pet);
			if (entry == null || entry.Failed || entry.Atlas == null || entry.Frames.Count == 0)
			{
				return false;
			}

			PetSpriteFrame frame = entry.Frames[FrameIndex(entry)];
			if (entry.Frames.Count > 1)
			{
				AnyAnimated = true;
			}

			DrawFrame(area, entry.Atlas, frame);
			return true;
		}

		public static void Clear()
		{
			foreach (KeyValuePair<string, Entry> entry in _cache)
			{
				if (entry.Value?.Atlas != null)
				{
					UnityEngine.Object.DestroyImmediate(entry.Value.Atlas);
				}
			}

			_cache.Clear();
		}

		private static int FrameIndex(Entry entry)
		{
			if (entry.Frames.Count <= 1)
			{
				return 0;
			}

			double elapsed = EditorApplication.timeSinceStartup * Mathf.Clamp(entry.Fps, 1f, 60f);
			int index = (int)(elapsed % entry.Frames.Count);

			// Hold the last frame for one-shot clips.
			return entry.Loop ? index : Mathf.Min(entry.Frames.Count - 1, index);
		}

		private static void DrawFrame(Rect area, Texture2D atlas, PetSpriteFrame frame)
		{
			float scale = Mathf.Min(area.width / frame.width, area.height / frame.height);
			float width = frame.width * scale;
			float height = frame.height * scale;

			Rect box = new Rect(
				area.x + (area.width - width) * 0.5f,
				area.y + (area.height - height) * 0.5f,
				width,
				height);

			// Atlas pixels are bottom-up; frame rectangles are top-down.
			Rect uv = new Rect(
				frame.x / (float)atlas.width,
				1f - (frame.y + frame.height) / (float)atlas.height,
				frame.width / (float)atlas.width,
				frame.height / (float)atlas.height);

			GUI.DrawTextureWithTexCoords(box, atlas, uv, true);
		}

		private static Entry Resolve(PetLibrary.Entry pet)
		{
			if (pet == null || string.IsNullOrEmpty(pet.DefinitionPath))
			{
				return null;
			}

			EnsureHooked();

			if (_cache.TryGetValue(pet.DefinitionPath, out Entry cached))
			{
				return cached;
			}

			Entry built = Load(pet.DefinitionPath);
			_cache[pet.DefinitionPath] = built;
			return built;
		}

		private static Entry Load(string definitionPath)
		{
			Entry entry = new Entry { Failed = true };

			if (!PetDefinitionSerializer.TryReadDefinition(definitionPath, out PetDefinition definition, out _))
			{
				return entry;
			}

			if (definition.icon == null || definition.icon.FrameCount == 0 || !definition.HasRectangles)
			{
				return entry;
			}

			string atlasPath = PetDefinitionSerializer.SafeCombine(Path.GetDirectoryName(definitionPath), definition.atlas);
			if (atlasPath == null || !File.Exists(atlasPath))
			{
				return entry;
			}

			try
			{
				Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
				{
					hideFlags = HideFlags.HideAndDontSave,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};

				if (!texture.LoadImage(File.ReadAllBytes(atlasPath), false) ||
					(long)texture.width * texture.height > MaxAtlasPixels)
				{
					UnityEngine.Object.DestroyImmediate(texture);
					return entry;
				}

				entry.Atlas = texture;
				entry.Fps = definition.icon.fps;
				entry.Loop = definition.icon.loop;

				foreach (PetFrameRef reference in definition.icon.frames)
				{
					if (reference.cell >= 0 && reference.cell < definition.spriteFrames.Count)
					{
						entry.Frames.Add(definition.spriteFrames[reference.cell]);
					}
				}

				entry.Failed = entry.Frames.Count == 0;
				return entry;
			}
			catch (Exception)
			{
				return entry;
			}
		}

		private static void EnsureHooked()
		{
			if (_hooked)
			{
				return;
			}

			_hooked = true;

			// Destroy owned textures before domain reload.
			AssemblyReloadEvents.beforeAssemblyReload += Clear;
		}
	}
}
