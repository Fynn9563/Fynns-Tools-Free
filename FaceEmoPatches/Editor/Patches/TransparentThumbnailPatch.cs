#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FynnsTools.FaceEmoPatches
{
	// FaceEmo renders every thumbnail through DrawingUtility.GetRenderedTexture, which
	// reads the camera into a TextureFormat.RGB24 texture. RGB24 has no alpha channel,
	// and the camera is left on its default Skybox clear, so the result is opaque twice
	// over. The generated expression menu icons are those same textures, which is why
	// they arrive with a solid backdrop behind the face.
	//
	// The replacement below is the original method with two changes: an RGBA32
	// destination, and a solid colour clear with zero alpha. Nothing downstream needed
	// patching, because FaceEmo already treats these textures as alpha capable. Its
	// gamma shader returns col with only col.rgb touched, and the menu icon padding step
	// builds an ARGB32 texture and copies pixels through GetPixel. Both carry alpha
	// through untouched, they were simply never handed any.
	internal sealed class TransparentThumbnailPatch : FaceEmoPatch
	{
		private const string DrawingUtilityTypeName = "Suzuryg.FaceEmo.Detail.Drawing.DrawingUtility";
		private const string RenderMethodName = "GetRenderedTexture";

		public override string Id => "TransparentThumbnails";

		public override string Title => "Transparent thumbnail backgrounds";

		public override string Description =>
			"Renders FaceEmo thumbnails onto a transparent background instead of an opaque one, " +
			"so the expression menu icons it generates keep their alpha.";

		protected override string CheckAvailability()
		{
			return ResolveRenderMethod() == null
				? "FaceEmo's thumbnail renderer was not found. The installed version has probably moved DrawingUtility.GetRenderedTexture."
				: null;
		}

		protected override void ApplyPatches(Harmony harmony, List<MethodBase> patched)
		{
			MethodInfo target = ResolveRenderMethod();
			harmony.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(TransparentThumbnailPatch), nameof(Prefix))));
			patched.Add(target);
		}

		private static MethodInfo ResolveRenderMethod()
		{
			Type drawingUtility = AccessTools.TypeByName(DrawingUtilityTypeName);
			return drawingUtility == null
				? null
				: AccessTools.Method(drawingUtility, RenderMethodName, new[] { typeof(int), typeof(int), typeof(Camera) });
		}

		// Returning false skips the original entirely and makes __result its return value.
		// The original is short and self contained, so replacing it outright is easier to
		// follow than rewriting its instructions to swap a texture format and a clear mode.
		private static bool Prefix(int width, int height, Camera camera, ref Texture2D __result)
		{
			__result = RenderWithAlpha(width, height, camera);
			return false;
		}

		private static Texture2D RenderWithAlpha(int width, int height, Camera camera)
		{
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
			RenderTexture renderTexture = RenderTexture.GetTemporary(width, height,
				format: RenderTextureFormat.ARGB32, readWrite: RenderTextureReadWrite.sRGB, depthBuffer: 24, antiAliasing: 8);

			// The camera belongs to FaceEmo's preview scene and is reused for every
			// thumbnail in a batch, so each of these has to go back the way it was found.
			CameraClearFlags clearFlagsCache = camera.clearFlags;
			Color backgroundCache = camera.backgroundColor;
			RenderTexture targetTextureCache = camera.targetTexture;
			float aspectCache = camera.aspect;
			RenderTexture activeRenderTextureCache = RenderTexture.active;
			try
			{
				renderTexture.wrapMode = TextureWrapMode.Clamp;
				renderTexture.filterMode = FilterMode.Bilinear;

				camera.clearFlags = CameraClearFlags.SolidColor;
				camera.backgroundColor = new Color(0, 0, 0, 0);
				camera.targetTexture = renderTexture;
				camera.aspect = (float)renderTexture.width / renderTexture.height;
				camera.Render();

				RenderTexture.active = renderTexture;
				texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, recalculateMipMaps: false);
				texture.Apply();
			}
			finally
			{
				RenderTexture.active = activeRenderTextureCache;
				camera.aspect = aspectCache;
				camera.targetTexture = targetTextureCache;
				camera.backgroundColor = backgroundCache;
				camera.clearFlags = clearFlagsCache;
				RenderTexture.ReleaseTemporary(renderTexture);
			}

			return texture;
		}
	}
}
#endif
