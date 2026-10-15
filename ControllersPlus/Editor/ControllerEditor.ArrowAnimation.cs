using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private static double nextArrowRepaint;
        private static readonly HashSet<(Vector3, Vector3)> selectedArrowEdges = new HashSet<(Vector3, Vector3)>();

        private static void RefreshSelectedArrowAnimations()
        {
            selectedArrowEdges.Clear();
            var selection = UnityAnimatorBridge.RefTests.DefinePolicy()?.edgeSelection;
            if (selection == null || selection.Count == 0) return;
            var edges = UnityAnimatorBridge.RefTests.DestroyPolicy()?.ToArray();
            if (edges == null) return;
            foreach (int index in selection)
            {
                if (index < 0 || index >= edges.Length) continue;
                var points = InitAnnotation(edges[index]);
                if (points != null && points.Length >= 2) selectedArrowEdges.Add((points[0], points[1]));
            }
        }

        internal static void UpdateArrowAnimation()
        {
            if (!_ExpressionVisitor || EditorApplication.timeSinceStartup < nextArrowRepaint) return;
            _ExpressionVisitor = false;
            nextArrowRepaint = EditorApplication.timeSinceStartup + 1.0 / 30.0;
            var animator = QueryAnnotation();
            if (animator) animator.Repaint();
        }
    }
}
