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
		[MenuItem(ToolsMenu, false, 20)]
		private static void AddPrefab()
		{
			VRCAvatarDescriptor avatar = Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>();
			if (avatar == null)
			{
				EditorUtility.DisplayDialog("GoGoLoco Pose Changer", "Avatar not found.\nPlease select an object under your avatar.", "OK");
				return;
			}

			// Only the first component on an avatar is read at build time, so a second
			// would be silently ignored. Reveal the existing one instead of adding it.
			// GameObject menu commands run once per selected object, so this also keeps
			// a multi-object selection from creating a pile of instances.
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

			// The prefab only carries the component with default values, so a bare
			// GameObject holding that component is an equivalent starting point.
			GameObject fallback = new GameObject("GoGoLoco Pose Changer");
			fallback.transform.SetParent(avatar.transform, false);
			fallback.AddComponent<GoGoLocoPoseChangerComponent>();
			Undo.RegisterCreatedObjectUndo(fallback, "Add GoGoLoco Pose Changer");
			Selection.activeGameObject = fallback;
		}

		[MenuItem(GameObjectMenu, true)]
		[MenuItem(ToolsMenu, true)]
		private static bool Validate()
		{
			return Selection.activeGameObject != null;
		}
	}
}
#endif
