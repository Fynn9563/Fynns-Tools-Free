using System;
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
            }, true));
            return false;
        }
    }
}
