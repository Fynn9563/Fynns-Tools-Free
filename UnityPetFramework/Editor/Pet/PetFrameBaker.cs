using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Turns a pet's frames into the exact bytes the overlay hands to Windows.
	internal sealed class PetFrameBaker
	{
		private readonly Dictionary<long, byte[]> _cache = new Dictionary<long, byte[]>();

		private PetAtlas _atlas;
		private PetDefinition _definition;
		private RectInt _trim;
		private float _scale = 1f;
		private bool _artFacesRight = true;

		// The size every baked frame comes out at, and therefore the size of the overlay window.
		public Vector2Int FrameSize { get; private set; }

		// Where the pet's feet are within a baked frame, measured from its top.
		public int GroundOffset { get; private set; }

		public bool IsReady => _atlas != null && FrameSize.x > 0 && FrameSize.y > 0;

		public void SetPet(PetAtlas atlas, PetDefinition definition, float scale)
		{
			_cache.Clear();
			_atlas = atlas;
			_definition = definition;
			_scale = Mathf.Max(0.05f, scale);
			_artFacesRight = definition?.artFacesRight ?? true;

			if (atlas == null || definition == null)
			{
				FrameSize = Vector2Int.zero;
				return;
			}

			_trim = definition.HasRectangles ? PetFrameGeometry.Canvas(definition) : new RectInt(
				Mathf.Clamp(definition.contentLeft, 0, atlas.CellWidth),
				Mathf.Clamp(definition.contentTop, 0, atlas.CellHeight),
				Mathf.Clamp(definition.contentWidth, 1, atlas.CellWidth),
				Mathf.Clamp(definition.contentHeight, 1, atlas.CellHeight));

			FrameSize = new Vector2Int(
				Mathf.Max(1, Mathf.RoundToInt(_trim.width * _scale)),
				Mathf.Max(1, Mathf.RoundToInt(_trim.height * _scale)));

			GroundOffset = definition.HasRectangles ? Mathf.RoundToInt(-_trim.y * _scale) : FrameSize.y;
		}

		// Bottom-up premultiplied BGRA, FrameSize worth of it.
		public byte[] Bake(PetFrameRef frame, PetClipDefinition clip, PetFacing facing)
		{
			if (!IsReady || frame.cell < 0 || !_atlas.IsValidCell(frame.cell))
			{
				return null;
			}

			bool mirrored = ShouldMirror(frame, clip, facing);

			long key = ((long)frame.cell << 1) | (mirrored ? 1L : 0L);
			if (_cache.TryGetValue(key, out byte[] cached))
			{
				return cached;
			}

			byte[] baked = BakeUncached(frame.cell, mirrored);
			_cache[key] = baked;
			return baked;
		}

		public bool ShouldMirror(PetFrameRef frame, PetClipDefinition clip, PetFacing facing)
		{
			return PetMirror.ShouldMirror(_definition, clip, frame, facing);
		}

		private byte[] BakeUncached(int cell, bool mirrored)
		{
			if (_definition.HasRectangles) return BakeRectangle(cell, mirrored);
			int width = FrameSize.x;
			int height = FrameSize.y;
			byte[] pixels = new byte[width * height * 4];

			for (int y = 0; y < height; y++)
			{
				// The DIB is bottom-up and the trim rect is top-down.
				int sourceY = _trim.y + Mathf.Min(_trim.height - 1, (int)((height - 1 - y) / _scale));
				int rowStart = y * width * 4;

				for (int x = 0; x < width; x++)
				{
					int localX = Mathf.Min(_trim.width - 1, (int)(x / _scale));

					// Mirroring is a read backwards across the trimmed area.
					int sourceX = _trim.x + (mirrored ? _trim.width - 1 - localX : localX);

					Color32 source = _atlas.GetPixel(cell, sourceX, sourceY);
					int offset = rowStart + x * 4;

					// Premultiply, then write in BGRA order.
					int alpha = source.a;
					pixels[offset + 0] = (byte)(source.b * alpha / 255);
					pixels[offset + 1] = (byte)(source.g * alpha / 255);
					pixels[offset + 2] = (byte)(source.r * alpha / 255);
					pixels[offset + 3] = source.a;
				}
			}

			return pixels;
		}

		private byte[] BakeRectangle(int cell, bool mirrored)
		{
			var f = _definition.spriteFrames[cell];
			var pixels = new byte[FrameSize.x * FrameSize.y * 4];
			float pivotX = mirrored ? f.width - f.pivotX : f.pivotX;
			for (int y = 0; y < FrameSize.y; y++)
			{
				int sy = Mathf.FloorToInt((FrameSize.y - 1 - y + .5f) / _scale + _trim.y + f.pivotY);
				if (sy < 0 || sy >= f.height) continue;
				for (int x = 0; x < FrameSize.x; x++)
				{
					int sx = Mathf.FloorToInt((x + .5f) / _scale + _trim.x + pivotX);
					if (sx < 0 || sx >= f.width) continue;
					if (mirrored) sx = f.width - 1 - sx;
					var color = _atlas.GetPixel(cell, sx, sy);
					int offset = (y * FrameSize.x + x) * 4;
					pixels[offset] = (byte)(color.b * color.a / 255);
					pixels[offset + 1] = (byte)(color.g * color.a / 255);
					pixels[offset + 2] = (byte)(color.r * color.a / 255);
					pixels[offset + 3] = color.a;
				}
			}
			return pixels;
		}
	}
}
