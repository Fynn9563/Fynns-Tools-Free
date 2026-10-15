using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class ControllerMotionPicker : PopupWindowContent
    {
        private static Motion[] index;
        static ControllerMotionPicker() { EditorApplication.projectChanged += () => index = null; }
        private readonly Action<Motion, bool> choose;
        private readonly Motion[] motions;
        private Motion[] matches;
        private string query;
        private int selected;
        private Vector2 scroll;
        private bool focus = true;
        internal ControllerMotionPicker(Action<Motion, bool> choose, bool clipsOnly = false)
        {
            this.choose = choose;
            query = SessionState.GetString("FynnsTools.ControllerEditor.MotionSearch", "");
            if (index == null) index = AssetDatabase.FindAssets("t:Motion").Select(AssetDatabase.GUIDToAssetPath).Distinct()
                .SelectMany(path => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Motion>()).Where(motion => motion)
                .OrderBy(motion => motion.name, StringComparer.OrdinalIgnoreCase).ToArray();
            motions = index.Where(motion => motion && (!clipsOnly || motion is AnimationClip)).ToArray();
            Filter();
        }
        internal static int Score(string name, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return 0;
            name = name.ToLowerInvariant(); query = query.ToLowerInvariant().Replace(" ", "");
            int cursor = 0, score = 0, previous = -2;
            foreach (char character in query)
            {
                int found = name.IndexOf(character, cursor);
                if (found < 0) return int.MinValue;
                score += found == previous + 1 ? 12 : 2;
                if (found == 0 || !char.IsLetterOrDigit(name[found - 1])) score += 8;
                score -= found - cursor;
                previous = found; cursor = found + 1;
            }
            return score - name.Length / 4;
        }
        private void Filter()
        {
            matches = motions.Select(motion => new { motion, score = Score(motion.name, query) })
                .Where(item => item.score != int.MinValue).OrderByDescending(item => item.score)
                .ThenBy(item => item.motion.name, StringComparer.OrdinalIgnoreCase).Select(item => item.motion).ToArray();
            selected = 0; scroll = Vector2.zero;
        }
        public override Vector2 GetWindowSize() { return new Vector2(430, 420); }
        public override void OnGUI(Rect rect)
        {
            var current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape) { editorWindow.Close(); current.Use(); return; }
                if (current.keyCode == KeyCode.UpArrow || current.keyCode == KeyCode.DownArrow)
                {
                    selected = Mathf.Clamp(selected + (current.keyCode == KeyCode.DownArrow ? 1 : -1), 0, Mathf.Max(0, matches.Length - 1));
                    scroll.y = Mathf.Max(0, selected * 38 - 170); current.Use(); editorWindow.Repaint();
                }
                if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && matches.Length > 0)
                { Accept(matches[selected], current.control || current.command); current.Use(); return; }
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.SetNextControlName("MotionSearch");
                EditorGUI.BeginChangeCheck();
                query = GUILayout.TextField(query, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) { SessionState.SetString("FynnsTools.ControllerEditor.MotionSearch", query); Filter(); }
                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25))) CreateClip();
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int index = 0; index < matches.Length; index++)
            {
                var motion = matches[index];
                Rect row = GUILayoutUtility.GetRect(1, 38, GUILayout.ExpandWidth(true));
                if (index == selected) EditorGUI.DrawRect(row, new Color(0.15f, 0.35f, 0.5f, 0.65f));
                GUI.Label(new Rect(row.x + 5, row.y, row.width - 10, 19), EditorGUIUtility.ObjectContent(motion, motion.GetType()));
                GUI.Label(new Rect(row.x + 25, row.y + 18, row.width - 30, 17), AssetDatabase.GetAssetPath(motion), EditorStyles.miniLabel);
                if (current.type == EventType.MouseDown && row.Contains(current.mousePosition))
                { selected = index; Accept(motion, current.button == 1 || current.control || current.command); current.Use(); break; }
            }
            if (matches.Length == 0) EditorGUILayout.LabelField("No matching motions");
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField("Enter: select   Ctrl+Enter or right-click: record", EditorStyles.miniLabel);
            if (focus && current.type == EventType.Repaint) { focus = false; GUI.FocusControl("MotionSearch"); }
        }
        private void Accept(Motion motion, bool record) { choose(motion, record); editorWindow.Close(); }
        private void CreateClip()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Animation", string.IsNullOrWhiteSpace(query) ? "Animation" : query, "anim", "Save the animation clip");
            if (string.IsNullOrEmpty(path)) return;
            var clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(path));
            Undo.RegisterCreatedObjectUndo(clip, "Create Animation");
            Accept(clip, false);
        }
    }
    internal sealed partial class ControllerEditor
    {
        private static void SearchStateMotion(AnimatorState state, Rect anchor)
        {
            if (!state) return;
            var states = m_AlgoAnnotation.Contains(state) ? m_AlgoAnnotation.Where(value => value).ToArray() : new[] { state };
            PopupWindow.Show(anchor, new ControllerMotionPicker((motion, record) =>
            {
                Undo.RecordObjects(states, "Set State Motion");
                foreach (var target in states) { target.motion = motion; EditorUtility.SetDirty(target); }
                if (record && motion is AnimationClip clip)
                {
#if FYNNS_CONTROLLER_EDITOR_AVATARS
                    var avatar = ControllerParameterTools.Avatar(LogoutMapper());
                    if (avatar) Selection.activeGameObject = avatar.gameObject;
#endif
                    var window = EditorWindow.GetWindow<AnimationWindow>();
                    window.animationClip = clip;
                    if (window.canRecord) window.recording = true;
                    else window.ShowNotification(new GUIContent("Select an avatar object to record this clip."));
                }
                PatchAlgo();
            }));
        }
    }
}
