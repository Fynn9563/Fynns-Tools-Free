using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Finds the Scene and Game views, and is the only place in the tool that touches Unity internals.
	internal static class EditorViewLocator
	{
		private static bool _probed;
		private static System.Type _gameViewType;
		private static FieldInfo _parentField;
		private static PropertyInfo _actualViewProperty;
		private static bool _warned;

		// False when Unity has moved something.
		public static bool IsAvailable
		{
			get
			{
				Probe();
				return _gameViewType != null;
			}
		}

		// Every Scene and Game view currently on screen, in Unity's point space.
		public static void CollectVisibleViews(List<Rect> results)
		{
			results.Clear();
			Probe();

			foreach (SceneView sceneView in SceneView.sceneViews)
			{
				if (sceneView != null && IsShowing(sceneView))
				{
					results.Add(sceneView.position);
				}
			}

			if (_gameViewType == null)
			{
				return;
			}

			foreach (Object candidate in Resources.FindObjectsOfTypeAll(_gameViewType))
			{
				if (candidate is EditorWindow window && IsShowing(window))
				{
					results.Add(window.position);
				}
			}
		}

		// A docked window that is not the tab on top still reports a perfectly good position.
		private static bool IsShowing(EditorWindow window)
		{
			if (window == null)
			{
				return false;
			}

			if (_parentField == null || _actualViewProperty == null)
			{
				// Cannot tell, so assume showing.
				return true;
			}

			object parent = _parentField.GetValue(window);
			if (parent == null)
			{
				return false;
			}

			return ReferenceEquals(_actualViewProperty.GetValue(parent, null), window);
		}

		private static void Probe()
		{
			if (_probed)
			{
				return;
			}

			_probed = true;

			// Present under this exact name in both 2022.3 and Unity 6, which is why one lookup covers both.
			_gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");

			_parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
			if (_parentField != null)
			{
				_actualViewProperty = _parentField.FieldType.GetProperty(
					"actualView",
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}

			if (_gameViewType != null || _warned)
			{
				return;
			}

			_warned = true;
			Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix +
				"The Game View could not be found in this version of Unity, so Safe UI mode will keep the pet to the Scene View only.");
		}
	}
}
