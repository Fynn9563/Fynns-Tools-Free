using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private static bool ClipMenuPrefix(object __instance, Rect position)
        {
            if (!ControllerEditorExtraSettings.Current.motionSearch) return true;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var state = __instance.GetType().GetField("state", flags)?.GetValue(__instance);
            if (state == null) return true;
            var clipProperty = state.GetType().GetProperty("activeAnimationClip", flags);
            if (clipProperty == null || !clipProperty.CanWrite) return true;
            PopupWindow.Show(position, new ControllerMotionPicker((motion, record) =>
            {
                if (!(motion is AnimationClip clip)) return;
                clipProperty.SetValue(state, clip, null);
                if (record && (bool)(state.GetType().GetProperty("canRecord", flags)?.GetValue(state, null) ?? false))
                    state.GetType().GetMethod("StartRecording", flags)?.Invoke(state, null);
                FindInitializer()?.Repaint();
            }, true, AnimationMenuMotions(state)));
            return false;
        }

        internal static IEnumerable<Motion> AnimationMenuMotions(object state)
        {
            if (state == null) return Array.Empty<Motion>();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var root = state.GetType().GetProperty("activeRootGameObject", flags)?.GetValue(state, null) as GameObject;
            if (!root) root = state.GetType().GetProperty("activeGameObject", flags)?.GetValue(state, null) as GameObject;
            return ControllerEditorInspection.AvatarMotions(root);
        }
    }
}
