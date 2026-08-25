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
	// The "+" button under Excluded Blend Shapes opens a ListSelectPopupContent, which
	// never overrides PopupWindowContent.GetWindowSize and so inherits Unity's fixed
	// 200 by 200 default. Every row is a BlendShape, and BlendShape.ToString returns
	// "<hierarchy path>.<blend shape name>", so on any real avatar the path fills the
	// popup before the name starts and the part worth reading is the part cut off.
	//
	// Patching the base GetWindowSize is narrow rather than broad, because a class that
	// overrides it never calls it. Only content that took Unity's default is reachable
	// here, the result is filtered down to FaceEmo's popup, and it is only ever grown.
	internal sealed class BlendShapePopupSizePatch : FaceEmoPatch
	{
		private const string PopupTypeName = "Suzuryg.FaceEmo.Detail.View.Element.ListSelectPopupContent`1";
		private const string TreeViewFieldName = "_listTreeView";
		private const string ContentsFieldName = "_contents";

		// Row foldout indent, the vertical scrollbar, the popup's own 5px margins, and
		// slack so the longest name does not sit flush against the scrollbar.
		private const float RowDecorationWidth = 60;

		// The popup gives its list everything except the OK and Cancel row: 25px of
		// button plus four 5px margins.
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

		// Compared as an open generic type name so nothing has to be resolved or loaded,
		// and so the check holds for every T the popup is used with.
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

				// TreeView already knows how tall its rows add up to, so the popup does not
				// have to guess a row height that FaceEmo might have changed.
				float rowsHeight = treeView is TreeView tree ? tree.totalHeight : 0;
				return new Vector2(widest + RowDecorationWidth, rowsHeight + ButtonRowHeight);
			}
			catch (Exception exception)
			{
				// This runs inside Unity's own popup layout, so throwing here would break the
				// window rather than the patch. Falling back to zero leaves Unity's default
				// size in place, which is the unpatched behaviour.
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
