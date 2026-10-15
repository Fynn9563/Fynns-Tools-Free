using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.NoAutoGame
{
    [InitializeOnLoad]
    internal static class NoAutoGameService
    {
        internal const string KeepFocusKey = "FynnsTools.NoAutoGame.KeepFocus";
        internal const string CloseNewKey = "FynnsTools.NoAutoGame.CloseNew";
        private const string SessionKey = "FynnsTools.NoAutoGame.PlayEntry";
        private static readonly Type GameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        private static double applyAt;
        private static bool scheduled;
        private static Entry entry;

        [Serializable]
        private sealed class Entry
        {
            public int focus;
            public int[] games;
            public bool keepFocus;
            public bool closeNew;
        }

        static NoAutoGameService()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += StopCallback;
            EditorApplication.quitting += Cancel;
            if (EditorApplication.isPlaying) Schedule();
        }

        internal static bool KeepFocus { get { return EditorPrefs.GetBool(KeepFocusKey, true); } }
        internal static bool CloseNew { get { return EditorPrefs.GetBool(CloseNewKey, true); } }

        internal static void SetOptions(bool keepFocus, bool closeNew)
        {
            EditorPrefs.SetBool(KeepFocusKey, keepFocus);
            EditorPrefs.SetBool(CloseNewKey, closeNew);
            Cancel();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    Remember();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Schedule();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    Cancel();
                    break;
            }
        }

        private static void Remember()
        {
            Cancel();
            if ((!KeepFocus && !CloseNew) || GameViewType == null) return;
            var games = Resources.FindObjectsOfTypeAll(GameViewType);
            var ids = new int[games.Length];
            for (int i = 0; i < games.Length; i++) ids[i] = games[i].GetInstanceID();
            var focused = EditorWindow.focusedWindow;
            entry = new Entry { focus = focused ? focused.GetInstanceID() : 0,
                games = ids, keepFocus = KeepFocus, closeNew = CloseNew };
            SessionState.SetString(SessionKey, JsonUtility.ToJson(entry));
        }

        private static void Schedule()
        {
            StopCallback();
            var json = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(json) || GameViewType == null) return;
            try { entry = JsonUtility.FromJson<Entry>(json); }
            catch (ArgumentException) { Cancel(); return; }
            if (entry == null || entry.games == null) { Cancel(); return; }
            applyAt = EditorApplication.timeSinceStartup + 0.15;
            scheduled = true;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (!scheduled || !EditorApplication.isPlaying) { Cancel(); return; }
            var focused = EditorWindow.focusedWindow;
            if (focused && !IsGameView(focused) && focused.GetInstanceID() != entry.focus)
            { Cancel(); return; }
            if (EditorApplication.timeSinceStartup < applyAt) return;
            Apply();
        }

        private static bool IsGameView(EditorWindow window)
        {
            return window && GameViewType != null && GameViewType.IsInstanceOfType(window);
        }

        private static void Apply()
        {
            var saved = entry;
            Cancel(); 
            if (saved == null || saved.games == null || GameViewType == null) return;
            var previous = EditorUtility.InstanceIDToObject(saved.focus) as EditorWindow;
            var focused = EditorWindow.focusedWindow;
            bool restore = saved.keepFocus && IsGameView(focused) && previous && !IsGameView(previous);
            if (saved.closeNew)
            {
                var existing = new HashSet<int>(saved.games);
                foreach (var obj in Resources.FindObjectsOfTypeAll(GameViewType))
                {
                    var game = obj as EditorWindow;
                    if (game && !existing.Contains(game.GetInstanceID())) game.Close();
                }
            }
            if (restore && previous) previous.Focus();
        }

        private static void StopCallback()
        {
            EditorApplication.update -= Tick;
            scheduled = false;
        }

        private static void Cancel()
        {
            StopCallback();
            entry = null;
            SessionState.EraseString(SessionKey);
        }
    }

    internal sealed class NoAutoGameWindow : EditorWindow
    {
        [MenuItem("Tools/Fynn's Tools/No Auto Game", false, 98)]
        private static void Open()
        {
            var window = GetWindow<NoAutoGameWindow>("No Auto Game");
            window.titleContent = new GUIContent("No Auto Game");
            window.minSize = new Vector2(340, 145);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Play Mode", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool keep = EditorGUILayout.ToggleLeft(new GUIContent("Keep focus on the current tab",
                "Return to your previous editor tab if Unity focuses Game when Play starts."), NoAutoGameService.KeepFocus);
            bool close = EditorGUILayout.ToggleLeft(new GUIContent("Close newly opened Game tabs",
                "Close Game tabs created at Play entry. Existing Game tabs stay open."), NoAutoGameService.CloseNew);
            if (EditorGUI.EndChangeCheck()) NoAutoGameService.SetOptions(keep, close);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Created by Fynn9563", EditorStyles.linkLabel))
                Application.OpenURL("https://github.com/Fynn9563");
            GUILayout.Space(8);
        }
    }
}
