using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// A small energy readout in the corner of the Unity window.
	internal sealed class PetEnergyBar : IDisposable
	{
		private const int BarWidth = 96;
		private const int BarHeight = 10;
		private const int MarginPixels = 12;

		// Redrawn only when the reading actually changes by something visible.
		private const float RedrawThreshold = 0.01f;

		private static readonly Color32 Background = new Color32(20, 20, 20, 200);
		private static readonly Color32 Border = new Color32(0, 0, 0, 220);
		private static readonly Color32 FullColour = new Color32(94, 240, 47, 235);
		private static readonly Color32 LowColour = new Color32(255, 194, 8, 235);

		private readonly byte[] _pixels = new byte[BarWidth * BarHeight * 4];

		private IPetOverlay _overlay;
		private float _lastDrawn = -1f;

		public bool IsAvailable => _overlay != null && _overlay.IsAvailable;

		public void EnsureCreated(IPetHost host)
		{
			if (_overlay != null)
			{
				return;
			}

			_overlay = PetOverlayFactory.CreateOverlay(host, new Vector2Int(BarWidth, BarHeight));

			// Never in the way.
			_overlay.SetClickThrough(true);
		}

		public void Update(IPetHost host, float energy, bool visible)
		{
			if (_overlay == null || !_overlay.IsAvailable)
			{
				return;
			}

			if (!visible)
			{
				_overlay.IsVisible = false;
				return;
			}

			if (Mathf.Abs(energy - _lastDrawn) >= RedrawThreshold || _lastDrawn < 0f)
			{
				Fill(energy);
				_lastDrawn = energy;
			}

			RectInt bounds = host.HostBoundsPx;
			Vector2Int position = new Vector2Int(
				bounds.xMin + MarginPixels,
				bounds.yMax - MarginPixels - BarHeight);

			_overlay.Present(_pixels, position);
			_overlay.IsVisible = true;
		}

		public void Dispose()
		{
			_overlay?.Dispose();
			_overlay = null;
		}

		// Premultiplied BGRA, bottom-up, same as every other frame the overlay is given.
		private void Fill(float energy)
		{
			int filled = Mathf.RoundToInt(Mathf.Clamp01(energy) * (BarWidth - 2));
			Color32 fill = energy <= UnityPetFrameworkSettings.SleepThreshold + 0.1f ? LowColour : FullColour;

			for (int y = 0; y < BarHeight; y++)
			{
				bool edgeRow = y == 0 || y == BarHeight - 1;

				for (int x = 0; x < BarWidth; x++)
				{
					bool edge = edgeRow || x == 0 || x == BarWidth - 1;
					Color32 colour = edge
						? Border
						: x - 1 < filled ? fill : Background;

					int offset = (y * BarWidth + x) * 4;
					int alpha = colour.a;
					_pixels[offset + 0] = (byte)(colour.b * alpha / 255);
					_pixels[offset + 1] = (byte)(colour.g * alpha / 255);
					_pixels[offset + 2] = (byte)(colour.r * alpha / 255);
					_pixels[offset + 3] = colour.a;
				}
			}
		}
	}
}
