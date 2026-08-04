#if NDMF && VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace FynnsTools.GoGoLocoPoseChanger
{
	[CustomEditor(typeof(GoGoLocoPoseChangerComponent))]
	public class GoGoLocoPoseChangerEditor : Editor
	{
		private const string CreditUrl = "https://github.com/Fynn9563";

		private ObjectField _afkField;
		private ObjectField _afkInitField;
		private ObjectField _afkLoopField;
		private ObjectField _afkStopField;
		private Toggle _isExtended;

		public override VisualElement CreateInspectorGUI()
		{
			VisualElement visualElement = new VisualElement();
			string uxmlPath = GoGoLocoPoseChangerPaths.InspectorUxmlPath;
			VisualTreeAsset uxml = string.IsNullOrEmpty(uxmlPath)
				? null
				: AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			if (uxml != null)
			{
				uxml.CloneTree(visualElement);
				visualElement.Q<Label>("title").text = string.Format(visualElement.Q<Label>("title").text, GoGoLocoPoseChangerPlugin.Version);
				AvatarCheck(visualElement, "av");
				visualElement.Q<VisualElement>("bi").Add(new HelpBox("Changes are automatically applied at build time.", HelpBoxMessageType.Info));
				_afkField = visualElement.Q<ObjectField>("afk");
				_afkInitField = visualElement.Q<ObjectField>("afkInit");
				_afkLoopField = visualElement.Q<ObjectField>("afkLoop");
				_afkStopField = visualElement.Q<ObjectField>("afkStop");
				_isExtended = visualElement.Q<Toggle>("isExtended");
				Label credit = visualElement.Q<Label>("credit");
				if (credit != null)
				{
					credit.RegisterCallback<ClickEvent>(_ => Application.OpenURL(CreditUrl));
				}
				_afkInitField.style.display = DisplayStyle.None;
				_afkLoopField.style.display = DisplayStyle.None;
				_afkStopField.style.display = DisplayStyle.None;
				_isExtended.RegisterValueChangedCallback(delegate(ChangeEvent<bool> evt)
				{
					if (evt.newValue)
					{
						_afkField.style.display = DisplayStyle.None;
						_afkField.value = null;
						_afkInitField.style.display = DisplayStyle.Flex;
						_afkLoopField.style.display = DisplayStyle.Flex;
						_afkStopField.style.display = DisplayStyle.Flex;
					}
					else
					{
						_afkField.style.display = DisplayStyle.Flex;
						_afkInitField.style.display = DisplayStyle.None;
						_afkLoopField.style.display = DisplayStyle.None;
						_afkStopField.style.display = DisplayStyle.None;
						_afkInitField.value = null;
						_afkLoopField.value = null;
						_afkStopField.value = null;
					}
				});
			}
			return visualElement;
		}

		private bool AvatarCheck(VisualElement root, string id)
		{
			VisualElement visualElement = root.Q<VisualElement>(id);
			if (visualElement == null)
			{
				return false;
			}
			VRCAvatarDescriptor avatar = AvatarRoot();
			if (avatar == null)
			{
				visualElement.Add(new IMGUIContainer
				{
					onGUIHandler = delegate
					{
						EditorGUILayout.ObjectField("Target Avatar", null, typeof(GameObject), true);
						EditorGUILayout.Space();
						EditorGUILayout.HelpBox("Error: Target avatar not found.\nPlease place this object under an avatar.", MessageType.Error);
						EditorGUILayout.Space();
					}
				});
				return false;
			}
			visualElement.Add(new IMGUIContainer
			{
				onGUIHandler = delegate
				{
					EditorGUILayout.ObjectField("Target Avatar", avatar.gameObject, typeof(GameObject), true);
					EditorGUILayout.Space();
				}
			});
			return true;
		}

		private VRCAvatarDescriptor AvatarRoot()
		{
			GameObject gameObject = ((Component)target).gameObject;
			VRCAvatarDescriptor vRCAvatarDescriptor = null;
			while (gameObject != null)
			{
				vRCAvatarDescriptor = gameObject.GetComponent<VRCAvatarDescriptor>();
				if (vRCAvatarDescriptor != null || gameObject.transform.parent == null)
				{
					break;
				}
				gameObject = gameObject.transform.parent.gameObject;
			}
			return vRCAvatarDescriptor;
		}
	}
}
#endif
