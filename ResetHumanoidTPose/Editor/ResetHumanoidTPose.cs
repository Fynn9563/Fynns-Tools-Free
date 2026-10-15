using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ResetHumanoid
{
	public class ResetHumanoid : EditorWindow
	{
		[SerializeField] private Animator ani;
		[SerializeField] private bool full;
		[SerializeField] private bool pos = true, rot = true, scale = true;
		private string status;
		private MessageType statusType;

		[MenuItem("Tools/Fynn's Tools/Reset Humanoid", false, 32)]
		private static void showWindow()
		{
			var window = GetWindow<ResetHumanoid>(false, "Reset Humanoid", true);
			window.ani = SelectedAnimator();
			window.status = null;
		}

		private static Animator SelectedAnimator()
		{
			var selected = Selection.activeGameObject;
			if (!selected || !selected.scene.IsValid()) return null;
			var parent = selected.GetComponentInParent<Animator>(true);
			if (parent) return parent;
			var children = selected.GetComponentsInChildren<Animator>(true);
			return children.Length == 1 ? children[0] : null;
		}

		private void OnSelectionChange()
		{
			ani = SelectedAnimator();
			status = null;
			Repaint();
		}

		private static string Validation(Animator target)
		{
			if (!target) return "Select an avatar in the Hierarchy or assign its Animator.";
			if (!target.gameObject.scene.IsValid() || EditorUtility.IsPersistent(target)) return "Choose an avatar instance in a scene.";
			if (!target.avatar || !target.avatar.isValid || !target.avatar.isHuman) return "The Animator needs a valid humanoid Avatar.";
			if (EditorApplication.isPlayingOrWillChangePlaymode) return "Exit Play Mode before resetting the pose.";
			return null;
		}

		private void ApplyReset()
		{
			try
			{
				int count = ResetPoseCount(ani, full, pos, rot, scale);
				status = count == 0 ? "The selected transforms already match the reset pose." : "Reset " + count + " transform(s). Undo is available with Ctrl+Z.";
				statusType = MessageType.Info;
			}
			catch (Exception error) { status = error.Message; statusType = MessageType.Warning; }
		}

		private void OnGUI()
		{
			float previousLabelWidth = EditorGUIUtility.labelWidth;
			try
			{
				EditorGUIUtility.labelWidth = 60;
				EditorGUI.BeginChangeCheck();
				ani = (Animator)EditorGUILayout.ObjectField("Avatar", ani, typeof(Animator), true);
				using (new EditorGUILayout.HorizontalScope())
				{
					pos = EditorGUILayout.ToggleLeft("Position", pos);
					rot = EditorGUILayout.ToggleLeft("Rotation", rot);
					scale = EditorGUILayout.ToggleLeft("Scale", scale);
				}
				full = EditorGUILayout.ToggleLeft(new GUIContent("Include non-humanoid bones", "Also reset transforms stored in the imported model's Avatar skeleton, such as extra bones. Objects added afterwards are excluded."), full);
				if (EditorGUI.EndChangeCheck()) status = null;
				var issue = Validation(ani);
				if (issue != null) EditorGUILayout.HelpBox(issue, MessageType.Info);
				else if (!pos && !rot && !scale) EditorGUILayout.HelpBox("Choose Position, Rotation or Scale to reset.", MessageType.Info);
				using (new EditorGUI.DisabledScope(issue != null || (!pos && !rot && !scale)))
					if (GUILayout.Button("Reset")) ApplyReset();
				if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, statusType);
			}
			finally { EditorGUIUtility.labelWidth = previousLabelWidth; }
		}

		public static void ResetPose(Animator ani, bool fullReset = false, bool position = true, bool rotation = true, bool scale = true)
		{
			var issue = Validation(ani);
			if (issue != null) { Debug.LogWarning(issue); return; }
			ResetPoseCount(ani, fullReset, position, rotation, scale);
		}

		private static int ResetPoseCount(Animator ani, bool fullReset, bool position, bool rotation, bool scale)
		{
			var issue = Validation(ani);
			if (issue != null) throw new InvalidOperationException(issue);
			if (!position && !rotation && !scale) return 0;
			SerializedObject sAvi = new SerializedObject(ani.avatar);
			SerializedProperty humanIds = sAvi.FindProperty("m_Avatar.m_Human.data.m_Skeleton.data.m_ID");
			SerializedProperty allIds = sAvi.FindProperty("m_Avatar.m_AvatarSkeleton.data.m_ID");
			SerializedProperty defaultPose = sAvi.FindProperty("m_Avatar.m_DefaultPose.data.m_X");
			SerializedProperty tos = sAvi.FindProperty("m_TOS");
			if (humanIds == null || allIds == null || defaultPose == null || tos == null)
				throw new NotSupportedException("Unity did not expose this Avatar's reset pose. No transforms were changed.");

			Dictionary<long, int> idToElem = new Dictionary<long, int>();
			Dictionary<int, TransformData> elemToTransform = new Dictionary<int, TransformData>();
			Dictionary<long, string> IdToPath = new Dictionary<long, string>();

			for (int i = 0; i < allIds.arraySize; i++)
				idToElem.Add(allIds.GetArrayElementAtIndex(i).longValue, i);

			for (int i = 0; i < defaultPose.arraySize; i++)
				elemToTransform.Add(i, new TransformData(defaultPose.GetArrayElementAtIndex(i)));

			for (int i = 0; i < tos.arraySize; i++)
			{
				SerializedProperty currProp = tos.GetArrayElementAtIndex(i);
				IdToPath.Add(currProp.FindPropertyRelative("first").longValue, currProp.FindPropertyRelative("second").stringValue);
			}

			int count = 0;
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Reset Humanoid Pose");
			Undo.RegisterFullObjectHierarchyUndo(ani.gameObject, "Reset Humanoid Pose");
			try
			{
				Action<Transform, TransformData> applyTransform = (transform, data) =>
				{
					if (transform)
					{
						bool changed = (position && !transform.localPosition.Equals(data.pos))
							|| (rotation && !transform.localRotation.Equals(data.rot))
							|| (scale && !transform.localScale.Equals(data.scale));
						if (!changed) return;
						if (position)
							transform.localPosition = data.pos;
						if (rotation)
							transform.localRotation = data.rot;
						if (scale)
							transform.localScale = data.scale;
						PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
						count++;
					}
				};

				if (!fullReset)
				{
					for (int i = 0; i < humanIds.arraySize; i++)
					{
						Transform myBone = ani.transform.Find(IdToPath[humanIds.GetArrayElementAtIndex(i).longValue]);
						TransformData data = elemToTransform[idToElem[humanIds.GetArrayElementAtIndex(i).longValue]];
						applyTransform(myBone, data);
					}
				}
				else
				{
					for (int i = 0; i < allIds.arraySize; i++)
					{
						Transform myBone = ani.transform.Find(IdToPath[allIds.GetArrayElementAtIndex(i).longValue]);
						TransformData data = elemToTransform[idToElem[allIds.GetArrayElementAtIndex(i).longValue]];
						applyTransform(myBone, data);
					}
				}
			}
			catch
			{
				Undo.RevertAllDownToGroup(undoGroup);
				throw;
			}
			finally
			{
				Undo.CollapseUndoOperations(undoGroup);
				Undo.IncrementCurrentGroup();
			}
			return count;
		}

		struct TransformData
		{
			public Vector3 pos;
			public Quaternion rot;
			public Vector3 scale;

			public TransformData(SerializedProperty t)
			{
				SerializedProperty tProp = t.FindPropertyRelative("t");
				SerializedProperty qProp = t.FindPropertyRelative("q");
				SerializedProperty sProp = t.FindPropertyRelative("s");
				pos = new Vector3(tProp.FindPropertyRelative("x").floatValue, tProp.FindPropertyRelative("y").floatValue, tProp.FindPropertyRelative("z").floatValue);
				rot = new Quaternion(qProp.FindPropertyRelative("x").floatValue, qProp.FindPropertyRelative("y").floatValue, qProp.FindPropertyRelative("z").floatValue, qProp.FindPropertyRelative("w").floatValue);
				scale = new Vector3(sProp.FindPropertyRelative("x").floatValue, sProp.FindPropertyRelative("y").floatValue, sProp.FindPropertyRelative("z").floatValue);
			}
		}
	}
}
