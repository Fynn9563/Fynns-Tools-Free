#if VRC_SDK_VRCSDK3
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FynnsTools.FaceEmoPatches
{
	// Grow FaceEmo's default popup to fit blendshape paths.
	internal sealed class BlendShapePopupSizePatch : FaceEmoPatch
	{
		private const string PopupTypeName = "Suzuryg.FaceEmo.Detail.View.Element.ListSelectPopupContent`1";
		private const string TreeViewFieldName = "_listTreeView";
		private const string ContentsFieldName = "_contents";

		// Include indent, scrollbar and popup margins.
		private const float RowDecorationWidth = 60;

		private const float ButtonRowHeight = 45;

		private const float MaxWidth = 900;
		private const float MaxHeight = 600;

		private static bool _measureFailureLogged;

		public override string Id => "BlendShapePopupSize";

		public override string Title => "Fit the blend shape picker to its contents";

		public override string Description =>
			"Sizes the blend shape selection popup to the longest entry in it, instead of Unity's fixed " +
			"200 by 200, so long blend shape names stay readable while you pick one.";

		protected override string CheckAvailability()
		{
			if (AccessTools.TypeByName(PopupTypeName) == null)
			{
				return "FaceEmo's selection popup was not found. The installed version has probably moved ListSelectPopupContent.";
			}

			return ResolveWindowSizeMethod() == null
				? "Unity's PopupWindowContent.GetWindowSize was not found, so the popup cannot be resized."
				: null;
		}

		protected override void ApplyPatches(Harmony harmony, List<MethodBase> patched)
		{
			MethodInfo target = ResolveWindowSizeMethod();
			harmony.Patch(target, postfix: new HarmonyMethod(AccessTools.Method(typeof(BlendShapePopupSizePatch), nameof(Postfix))));
			patched.Add(target);
		}

		private static MethodInfo ResolveWindowSizeMethod()
		{
			return AccessTools.Method(typeof(PopupWindowContent), nameof(PopupWindowContent.GetWindowSize));
		}

		private static void Postfix(PopupWindowContent __instance, ref Vector2 __result)
		{
			if (!IsListSelectPopup(__instance))
			{
				return;
			}

			Vector2 fitted = MeasureContents(__instance);
			if (fitted == Vector2.zero)
			{
				return;
			}

			__result = new Vector2(
				Mathf.Min(Mathf.Max(__result.x, fitted.x), MaxWidth),
				Mathf.Min(Mathf.Max(__result.y, fitted.y), MaxHeight));
		}

		// Match the open generic name without loading FaceEmo types.
		private static bool IsListSelectPopup(PopupWindowContent content)
		{
			Type type = content?.GetType();
			if (type == null || !type.IsGenericType)
			{
				return false;
			}

			return type.GetGenericTypeDefinition().FullName == PopupTypeName;
		}

		private static Vector2 MeasureContents(PopupWindowContent content)
		{
			try
			{
				object treeView = AccessTools.Field(content.GetType(), TreeViewFieldName)?.GetValue(content);
				if (treeView == null)
				{
					return Vector2.zero;
				}

				if (!(AccessTools.Field(treeView.GetType(), ContentsFieldName)?.GetValue(treeView) is IEnumerable items))
				{
					return Vector2.zero;
				}

				float widest = 0;
				GUIContent measured = new GUIContent();
				foreach (object item in items)
				{
					measured.text = item?.ToString();
					if (string.IsNullOrEmpty(measured.text))
					{
						continue;
					}

					widest = Mathf.Max(widest, EditorStyles.label.CalcSize(measured).x);
				}

				if (widest <= 0)
				{
					return Vector2.zero;
				}

				// Use TreeView's measured height.
				float rowsHeight = treeView is TreeView tree ? tree.totalHeight : 0;
				return new Vector2(widest + RowDecorationWidth, rowsHeight + ButtonRowHeight);
			}
			catch (Exception exception)
			{
				// Keep Unity's default size if reflection fails.
				if (!_measureFailureLogged)
				{
					_measureFailureLogged = true;
					Debug.LogWarning($"{FaceEmoPatchRunner.LogPrefix} Could not measure the blend shape picker, so it keeps its default size.\n{exception}");
				}

				return Vector2.zero;
			}
		}
	}
}
#endif
