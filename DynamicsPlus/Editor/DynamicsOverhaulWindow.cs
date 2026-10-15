#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using UnityEditor;
using UnityEngine;

namespace FynnsToolkit.Dynamics
{
    internal sealed class DynamicsOverhaulWindow : EditorWindow
    {
        private Vector2 scroll;

        [MenuItem("Tools/Fynn's Tools/Dynamics+", false, 30)]
        internal static void OpenMain() { DynamicsEditorWindow.OpenMain(); }

        internal static void Open() { DynamicsEditorWindow.OpenSettings(); }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Components")) DynamicsEditorWindow.OpenFor(null);
                if (GUILayout.Button("Organize")) DynamicsEditorWindow.OpenManagement(false);
                if (GUILayout.Button("Remove")) DynamicsEditorWindow.OpenManagement(true);
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DynamicsSettings.instance.DrawCosmetic();
            EditorGUILayout.EndScrollView();
            DynamicsInfo.DrawCredit();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("⋮", EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Reset Settings"), false, () =>
                    {
                        if (!EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel")) return;
                        DynamicsSettings.instance.ResetSettings();
                    });
                    menu.AddItem(new GUIContent("Component Browser"), false, () => DynamicsEditorWindow.OpenFor(null));
                    menu.AddItem(new GUIContent("Physbone Tester"), DynamicsPreview.Active != null, () =>
                    {
                        if (DynamicsPreview.Active != null) { DynamicsPreview.Active.Dispose(); return; }
                        try { new DynamicsPreview().Start(null); }
                        catch (System.Exception error) { EditorUtility.DisplayDialog("Physbone Tester", error.Message, "OK"); }
                    });
                    menu.ShowAsContext();
                }
            }
        }
    }
}
#endif
