#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using UnityEditor;
using UnityEngine;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsInfo
    {
        private const string CreditUrl = "https://github.com/Fynn9563";
        private static GUIStyle creditStyle;

        internal static void DrawCredit()
        {
            if (creditStyle == null)
            {
                creditStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, alignment = TextAnchor.MiddleRight };
                var color = creditStyle.normal.textColor;
                color.a = 0.6f;
                creditStyle.normal.textColor = color;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                var credit = new GUIContent("Created by Fynn9563", CreditUrl);
                if (GUILayout.Button(credit, creditStyle, GUILayout.Width(120), GUILayout.Height(18)))
                    Application.OpenURL(CreditUrl);
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            }
        }
    }
}
#endif
