#if NDMF && VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace FynnsTools.GoGoLocoPoseChanger
{
	public static class GoGoLocoPoseChangerMenu
	{
		private const string GameObjectMenu = "GameObject/Fynn's Tools/GoGoLoco Pose Changer/Add Prefab";
		private const string ToolsMenu = "Tools/Fynn's Tools/GoGoLoco Pose Changer/Add Prefab";

		[MenuItem(GameObjectMenu, false, 20)]
		[MenuItem(ToolsMenu, false, 13)]
		private static void AddPrefab()
		{
			VRCAvatarDescriptor avatar = Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>();
			if (avatar == null)
			{
				EditorUtility.DisplayDialog("GoGoLoco Pose Changer", "Avatar not found.\nPlease select an object under your avatar.", "OK");
				return;
			}

			// Reuse the existing component; the build reads only the first.
			GoGoLocoPoseChangerComponent existing = avatar.GetComponentInChildren<GoGoLocoPoseChangerComponent>(true);
			if (existing != null)
			{
				Selection.activeGameObject = existing.gameObject;
				EditorGUIUtility.PingObject(existing.gameObject);
				return;
			}

			string prefabPath = GoGoLocoPoseChangerPaths.PrefabPath;
			GameObject prefab = string.IsNullOrEmpty(prefabPath)
				? null
				: AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			if (prefab != null)
			{
				GameObject instance = PrefabUtility.InstantiatePrefab(prefab, avatar.transform) as GameObject;
				Undo.RegisterCreatedObjectUndo(instance, "Add GoGoLoco Pose Changer");
				Selection.activeGameObject = instance;
				return;
			}

			// Use a bare component when the default-only prefab is unavailable.
			GameObject fallback = new GameObject("GoGoLoco Pose Changer");
			fallback.transform.SetParent(avatar.transform, false);
			fallback.AddComponent<GoGoLocoPoseChangerComponent>();
			Undo.RegisterCreatedObjectUndo(fallback, "Add GoGoLoco Pose Changer");
			Selection.activeGameObject = fallback;
		}

		[MenuItem(GameObjectMenu, true)]
		[MenuItem(ToolsMenu, true, 13)]
		private static bool Validate()
		{
			return Selection.activeGameObject != null;
		}
	}
}
#endif
