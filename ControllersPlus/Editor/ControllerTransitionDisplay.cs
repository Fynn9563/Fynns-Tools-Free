using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerTransitionDisplay
    {
        internal static string Condition(AnimatorCondition condition, AnimatorController controller)
        {
            string name = condition.parameter;
            var parameter = controller ? controller.parameters.FirstOrDefault(item => item.name == name) : null;
            bool missing = parameter == null && !ControllerParameterTools.Builtins.ContainsKey(name);
            if (missing) return "Missing: " + name;
            var type = parameter != null ? parameter.type : ControllerParameterTools.Builtins[name];
            if (!ControllerConditionEditing.Compatible(condition.mode, type)) return "Invalid: " + name;
            if (float.IsNaN(condition.threshold) || float.IsInfinity(condition.threshold)) return "Invalid threshold: " + name;
            string value = condition.threshold.ToString("0.###", CultureInfo.InvariantCulture);
            if (ControllerEditorExtraSettings.Current.compactVrcComparisons && (name == "GestureLeft" || name == "GestureRight") &&
                (condition.mode == AnimatorConditionMode.Equals || condition.mode == AnimatorConditionMode.NotEqual) && condition.threshold >= 0 && condition.threshold <= 7 && Mathf.Approximately(condition.threshold, Mathf.Round(condition.threshold)))
                value = new[] { "Neutral", "Fist", "Open", "Point", "Peace", "Rock", "Gun", "Thumbs Up" }[(int)condition.threshold];
            switch (condition.mode)
            {
                case AnimatorConditionMode.If: return name;
                case AnimatorConditionMode.IfNot: return "!" + name;
                case AnimatorConditionMode.Equals: return name + " = " + value;
                case AnimatorConditionMode.NotEqual: return name + " ≠ " + value;
                case AnimatorConditionMode.Greater:
                    if (parameter != null && parameter.type == AnimatorControllerParameterType.Int && ControllerEditorExtraSettings.Current.inclusiveIntComparisons)
                        return name + " ≥ " + (Math.Floor(condition.threshold) + 1).ToString(CultureInfo.InvariantCulture);
                    return name + " > " + value;
                case AnimatorConditionMode.Less:
                    if (parameter != null && parameter.type == AnimatorControllerParameterType.Int && ControllerEditorExtraSettings.Current.inclusiveIntComparisons)
                        return name + " ≤ " + (Math.Ceiling(condition.threshold) - 1).ToString(CultureInfo.InvariantCulture);
                    return name + " < " + value;
                default: return name;
            }
        }
        internal static string Label(AnimatorTransitionBase transition, AnimatorController controller)
        {
            if (transition.conditions.Length > 0) return string.Join(" ∧ ", transition.conditions.Select(condition => Condition(condition, controller)));
            if (transition is AnimatorStateTransition state && state.hasExitTime)
                return Mathf.Approximately(state.exitTime, 1) ? "End of Motion" : "After " + (state.exitTime * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";
            return "Always";
        }
        internal static bool IsInstant(AnimatorTransitionBase transition)
        { return transition is AnimatorStateTransition state && !state.hasExitTime && state.duration <= 0; }
    }
    internal sealed partial class ControllerEditor
    {
        private static void DrawEdgeExtrasPrefix(Edge edge, ref Color color, bool viewHasLiveLinkExactEdge)
        {
            if (edge == null || viewHasLiveLinkExactEdge) return;
            var transitions = new UnityAnimatorBridge.BaseTests(edge).FindSerializer().Select(item => item.itemTests).Where(value => value).ToArray();
            if (transitions.Length == 0 || transitions.Any(value => value.solo || value.mute)) return;
            var graph = UnityAnimatorBridge.RefTests.CancelPolicy();
            var selected = UnityAnimatorBridge.RefTests.DefinePolicy()?.edgeSelection;
            if (graph && selected != null && selected.Contains(graph.edges.IndexOf(edge))) return;
            if (ControllerEditorExtraSettings.Current.instantTransitionHighlight && transitions.All(ControllerTransitionDisplay.IsInstant)) color = new Color(0.1f, 0.9f, 1f);
            foreach (var rule in ControllerEditorExtraSettings.Current.transitionColours.Where(rule => !string.IsNullOrEmpty(rule.text)))
                if (transitions.Any(transition => transition.name.IndexOf(rule.text, StringComparison.OrdinalIgnoreCase) >= 0)) { color = rule.colour; break; }
        }
        private static void DrawEdgeExtrasPost(Edge edge)
        {
            if (Event.current.type != EventType.Repaint || edge == null) return;
            var controller = LogoutMapper();
            var transitions = new UnityAnimatorBridge.BaseTests(edge).FindSerializer().Select(item => item.itemTests).Where(value => value).ToArray();
            if (transitions.Length == 0) return;
            string text = string.Join(" OR ", transitions.Select(transition => ControllerTransitionDisplay.Label(transition, controller)).Distinct());
            bool invalid = text.Contains("Missing: ") || text.Contains("Invalid: ") || text.Contains("Invalid threshold: ");
            if (!invalid && !ControllerEditorExtraSettings.Current.transitionLabels) return;
            var points = InitAnnotation(edge);
            if (points == null || points.Length < 2 || Vector3.Distance(points[0], points[1]) < 25) return;
            Vector2 center = ((Vector2)points[0] + (Vector2)points[1]) * 0.5f;
            var style = new GUIStyle(EditorStyles.miniLabel) { fontSize = ControllerEditorExtraSettings.Current.transitionLabelSize, alignment = TextAnchor.MiddleCenter };
            var content = new GUIContent(text, text);
            float width = Mathf.Min(260, style.CalcSize(content).x + 8);
            if (style.CalcSize(content).x > 252) content.text = text.Substring(0, Mathf.Min(text.Length, 42)) + "…";
            var rect = new Rect(center.x - width / 2, center.y - 17, width, 17);
            Matrix4x4 matrix = GUI.matrix; Color colour = GUI.contentColor;
            try
            {
                if (!ControllerEditorExtraSettings.Current.horizontalTransitionLabels)
                {
                    Vector2 direction = (Vector2)(points[1] - points[0]);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    if (angle > 90 || angle < -90) angle += 180;
                    GUIUtility.RotateAroundPivot(angle, center);
                }
                if (ControllerEditorExtraSettings.Current.transitionLabelOutline)
                {
                    GUI.contentColor = Color.black;
                    foreach (var offset in new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down })
                        GUI.Label(new Rect(rect.position + offset, rect.size), content, style);
                }
                GUI.contentColor = invalid ? new Color(1f, 0.45f, 0.35f) : Color.white;
                GUI.Label(rect, content, style);
            }
            finally { GUI.matrix = matrix; GUI.contentColor = colour; }
        }
    }
}
