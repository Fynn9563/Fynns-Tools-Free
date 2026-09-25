using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Loads and caches the little pictures shown beside each pet.
	internal static class PetThumbnails
	{
		// Anything larger is a pet portrait rather than a list icon.
		private const int MaxSize = 512;

		private static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
		private static bool _hooked;

		public static Texture2D Get(string absolutePath)
		{
			if (string.IsNullOrEmpty(absolutePath))
			{
				return null;
			}

			EnsureHooked();

			if (_cache.TryGetValue(absolutePath, out Texture2D cached))
			{
				// A null entry is a remembered failure.
				return cached;
			}

			Texture2D loaded = Load(absolutePath);
			_cache[absolutePath] = loaded;
			return loaded;
		}

		public static void Clear()
		{
			foreach (KeyValuePair<string, Texture2D> entry in _cache)
			{
				if (entry.Value != null)
				{
					Object.DestroyImmediate(entry.Value);
				}
			}

			_cache.Clear();
		}

		private static Texture2D Load(string absolutePath)
		{
			if (!File.Exists(absolutePath))
			{
				return null;
			}

			try
			{
				byte[] bytes = File.ReadAllBytes(absolutePath);

				Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
				{
					hideFlags = HideFlags.HideAndDontSave,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};

				if (!texture.LoadImage(bytes, false) || texture.width > MaxSize || texture.height > MaxSize)
				{
					Object.DestroyImmediate(texture);
					return null;
				}

				return texture;
			}
			catch (System.Exception)
			{
				return null;
			}
		}

		private static void EnsureHooked()
		{
			if (_hooked)
			{
				return;
			}

			_hooked = true;
			AssemblyReloadEvents.beforeAssemblyReload += Clear;
		}
	}
}
