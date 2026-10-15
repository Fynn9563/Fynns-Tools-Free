#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.VRCSDKPlus
{
	// Patch reporting while the renderer context is still available.
	internal sealed class ShaderErrorSelectPatch : VRCSdkPatch
	{
		private const string ReportMethodName = "OnGUIError";

		private const string MessageMarker = "unsupported shader '";

		// Shader names can contain apostrophes.
		private const string MessageTail = "'. You can only use";

		public override string Id => "ShaderErrorSelect";

		public override string Title => "Select the object using an unsupported shader";

		public override string Description =>
			"Makes the Select button on the SDK's \"unsupported shader\" build error pick the objects that " +
			"actually use the shader, instead of the avatar root. These errors only appear once the SDK is " +
			"on Android.";

		protected override string CheckAvailability()
		{
			if (!VRCSdkReflection.IsAvatarsSdkInstalled)
			{
				return "The VRChat Avatars SDK was not found in this project, and this patch only changes an avatar build error.";
			}

			return ResolveReportMethod() == null
				? "The SDK's build report method was not found. The installed SDK version has probably moved VRCSdkControlPanel.OnGUIError."
				: null;
		}

		protected override void ApplyPatches(Harmony harmony, List<MethodBase> patched)
		{
			MethodInfo target = ResolveReportMethod();
			harmony.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(ShaderErrorSelectPatch), nameof(Prefix))));
			patched.Add(target);
		}

		private static MethodInfo ResolveReportMethod()
		{
			Type controlPanel = VRCSdkReflection.ControlPanelType;
			return controlPanel == null
				? null
				: AccessTools.Method(controlPanel, ReportMethodName,
					new[] { typeof(UnityEngine.Object), typeof(string), typeof(Action), typeof(Action) });
		}

		// Harmony binds these parameters by name.
		private static void Prefix(UnityEngine.Object subject, string output, ref Action show)
		{
			string shaderName = ExtractShaderName(output);
			if (string.IsNullOrEmpty(shaderName))
			{
				return;
			}

			GameObject root = RootOf(subject);
			if (root == null)
			{
				return;
			}

			Action original = show;
			show = () =>
			{
				// Cached alert actions must respect later preference changes.
				if (VRCSdkPatchesSettings.IsEnabled("ShaderErrorSelect")) SelectShaderUsers(root, shaderName, original);
				else original?.Invoke();
			};
		}

		private static void SelectShaderUsers(GameObject root, string shaderName, Action fallback)
		{
			GameObject[] users = root == null ? null : FindShaderUsers(root, shaderName);

			// Use the native selection action if materials changed.
			if (users == null || users.Length == 0)
			{
				fallback?.Invoke();
				return;
			}

			Selection.objects = users;
			EditorGUIUtility.PingObject(users[0]);
		}

		private static GameObject[] FindShaderUsers(GameObject root, string shaderName)
		{
			List<GameObject> found = new List<GameObject>();

			foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
			{
				if (renderer == null || found.Contains(renderer.gameObject))
				{
					continue;
				}

				foreach (Material material in renderer.sharedMaterials)
				{
					if (material == null || material.shader == null || material.shader.name != shaderName)
					{
						continue;
					}

					found.Add(renderer.gameObject);
					break;
				}
			}

			return found.ToArray();
		}

		private static string ExtractShaderName(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return null;
			}

			int start = message.IndexOf(MessageMarker, StringComparison.OrdinalIgnoreCase);
			if (start < 0)
			{
				return null;
			}

			start += MessageMarker.Length;

			int end = message.IndexOf(MessageTail, start, StringComparison.OrdinalIgnoreCase);
			if (end < 0)
			{
				end = message.IndexOf('\'', start);
			}

			return end <= start ? null : message.Substring(start, end - start);
		}

		private static GameObject RootOf(UnityEngine.Object subject)
		{
			if (subject is GameObject gameObject)
			{
				return gameObject;
			}

			return subject is Component component ? component.gameObject : null;
		}
	}
}
#endif
