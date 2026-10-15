#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;

namespace FynnsToolkit.Dynamics
{
    internal sealed class DynamicsSceneTools
    {
        internal enum Mode { None, EndPosition, IgnoreSelection, IgnoreCopy, CollisionSelection, CollisionCopy, PropertyEdit, Shape }
        [Flags]
        internal enum ShapeControl { None = 0, Position = 1, Rotation = 2, Radius = 4, Height = 8, Size = 16, All = 31 }
        internal ShapeControl shapeControl = ShapeControl.All;
        private static readonly string[] Names = { "None", "End Position Edit", "Ignore Selection", "Ignore Copy", "Collision Selection", "Collision Copy", "Property Edit", "Shape" };
        private static readonly string[] Properties = { "pull", "spring", "stiffness", "immobile", "gravity", "gravityFalloff", "radius", "maxAngleX", "maxAngleZ", "limitRotation.x", "limitRotation.y", "limitRotation.z", "stretchMotion", "maxStretch", "maxSquish", "grabMovement" };
        internal Mode mode;
        private int propertyIndex;
        private bool editCurve;
        private string error;
        private static GUIStyle overlayTitle;
        private static GUIStyle overlayTip;
        private int draggingOverlay;
        private Vector2 dragPosition;

        internal static bool CanEditProperty(string name) => Array.IndexOf(Properties, name) >= 0;
        internal bool IsEditingProperty(string name) => mode == Mode.PropertyEdit && Properties[propertyIndex] == name;
        internal void EditProperty(string name)
        {
            int index = Array.IndexOf(Properties, name);
            if (index < 0) return;
            mode = mode == Mode.PropertyEdit && propertyIndex == index ? Mode.None : Mode.PropertyEdit;
            propertyIndex = index;
        }

        internal void DrawControls(Component active = null)
        {
            var previous = mode;
            if (active != null && !(active is VRCPhysBoneBase))
                mode = GUILayout.Toolbar(mode == Mode.Shape ? 1 : 0, new[] { "None", "Shape" }) == 1 ? Mode.Shape : Mode.None;
            else mode = (Mode)GUILayout.SelectionGrid((int)mode, active == null ? Names : Names.Take(7).ToArray(), 2);
            if (mode == Mode.Shape)
            {
                if (previous != mode) shapeControl = ShapeControl.All;
                foreach (ShapeControl control in ShapeControls(active))
                {
                    bool enabled = GUILayout.Toggle((shapeControl & control) != 0, control.ToString(), EditorStyles.miniButton);
                    if (enabled) shapeControl |= control;
                    else shapeControl &= ~control;
                }
                if (shapeControl == ShapeControl.None) mode = Mode.None;
            }
            if (mode == Mode.PropertyEdit)
            {
                propertyIndex = EditorGUILayout.Popup("Property", propertyIndex, Properties);
                editCurve = EditorGUILayout.Toggle("Edit curve", editCurve);
                if (DynamicsSettings.instance.tooltips)
                    EditorGUILayout.LabelField("Alt: curve for one bone, target only for multiple bones. Shift: same value.", EditorStyles.wordWrappedMiniLabel);
            }
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
        }

        internal void Draw(GameObject root, Component active, IReadOnlyList<Component> selection, Action<Component> choose)
        {
            if (root == null || active == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var settings = DynamicsSettings.instance;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return))
            {
                mode = Mode.None;
                if (draggingOverlay != 0) { draggingOverlay = 0; GUIUtility.hotControl = 0; }
                Event.current.Use();
            }
            if (!settings.sceneTools) return;
            if (mode == Mode.Shape)
            {
                shapeControl &= ShapeControls(active).Aggregate(ShapeControl.None, (flags, control) => flags | control);
                if (shapeControl == ShapeControl.None) mode = Mode.None;
            }
            var bones = selection.OfType<VRCPhysBoneBase>().ToArray();
            try
            {
                using (new Handles.DrawingScope(settings.selectedColor))
                {
                    if (settings.showBoneChains)
                        foreach (var bone in bones) DrawChain(bone);
                    switch (mode)
                    {
                        case Mode.EndPosition: foreach (var bone in bones) DrawEndpoint(bone, bones); break;
                        case Mode.IgnoreSelection: DrawIgnoreSelection(bones); break;
                        case Mode.CollisionSelection: DrawColliderSelection(root, bones); break;
                        case Mode.IgnoreCopy:
                        case Mode.CollisionCopy: DrawCopy(root, bones); break;
                        case Mode.PropertyEdit: DrawProperty(active as VRCPhysBoneBase, bones); break;
                        case Mode.Shape: DrawShape(active, selection); break;
                    }
                }
                if (settings.ignoreSceneClicks && mode != Mode.None && Event.current.type == EventType.Layout && !Event.current.alt)
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                DrawOverlays(active, selection.Count);
                error = null;
            }
            catch (ExitGUIException) { throw; }
            catch (Exception exception) { error = exception.Message; mode = Mode.None; Debug.LogException(exception); }
        }

        private void DrawOverlays(Component active, int count)
        {
            var settings = DynamicsSettings.instance;
            if (overlayTitle == null)
            {
                overlayTitle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                overlayTip = new GUIStyle(overlayTitle);
            }
            Handles.BeginGUI();
            try
            {
                if (active is VRCPhysBoneBase && settings.sceneOverlay && (settings.toolOverlayAlwaysVisible || mode != Mode.None))
                {
                    Rect area = OverlayRect(settings.toolOverlayPosition, new Vector2(260, 34));
                    OverlayBackground(area);
                    Rect row = new Rect(area.x + 5, area.y + 7, area.width - 10, 20);
                    var picking = EditorGUIUtility.IconContent(settings.ignoreSceneClicks ? "d_scenepicking_notpickable@2x" : "d_scenepicking_pickable_hover@2x");
                    picking = new GUIContent(picking) { tooltip = "Toggle scene clicks while editing" };
                    using (new EditorGUI.DisabledScope(mode == Mode.None))
                        if (GUI.Button(new Rect(row.x, row.y, 18, 18), picking, GUIStyle.none))
                        {
                            settings.ignoreSceneClicks = !settings.ignoreSceneClicks;
                            settings.SaveLayout();
                        }
                    Rect title = new Rect(row.x + 22, row.y, 75, 18);
                    GUI.Label(title, "Dynamics+:", overlayTitle);
                    OverlayDock(title, false);
                    if (active is VRCPhysBoneBase)
                        mode = (Mode)EditorGUI.Popup(new Rect(title.xMax + 2, row.y, row.width - 124, 18), Mathf.Min((int)mode, 6), Names.Take(7).ToArray());
                    else mode = EditorGUI.Popup(new Rect(title.xMax + 2, row.y, row.width - 124, 18), mode == Mode.Shape ? 1 : 0, new[] { "None", "Shape" }) == 1 ? Mode.Shape : Mode.None;
                    OverlaySettings(new Rect(area.xMax - 24, row.y, 18, 18));
                    Intercept(area);
                }
                if (settings.editingOverlay && mode != Mode.None)
                {
                    string title = (count > 1 ? "Multi-" : "") + EditingTitle();
                    var tips = new List<string>();
                    if (settings.tooltips && mode != Mode.Shape)
                    {
                        tips.Add("Press Enter or Escape to exit");
                        if (mode == Mode.PropertyEdit)
                        {
                            if (count > 1) tips.Add("Hold Alt to edit the target physbone only");
                            else if (new SerializedObject(active).FindProperty(CurveName(Properties[propertyIndex])) != null)
                                tips.Add("Hold Alt to edit the curve");
                            if (count > 1) tips.Add("Hold Shift to set the physbones to the same value");
                        }
                    }
                    float width = mode == Mode.Shape ? 200 : count > 1 ? 340 : 240;
                    float height = 30 + tips.Sum(tip => overlayTip.CalcHeight(new GUIContent(tip), width - 12) + 4);
                    if (mode == Mode.Shape) height += ShapeControls(active).Length * 20;
                    Rect area = OverlayRect(settings.editingOverlayPosition, new Vector2(width, height));
                    OverlayBackground(area);
                    Rect header = new Rect(area.x + 5, area.y + 4, area.width - 30, 20);
                    if (mode == Mode.PropertyEdit)
                    {
                        string[] names = Properties.Select(PropertyLabel).ToArray();
                        float popupWidth = Mathf.Clamp(EditorStyles.popup.CalcSize(new GUIContent(names[propertyIndex])).x + 10, 46, 125);
                        float titleWidth = overlayTitle.CalcSize(new GUIContent(title)).x;
                        float left = header.center.x - (titleWidth + popupWidth + 4) * 0.5f;
                        Rect label = new Rect(left, header.y, titleWidth, 20);
                        GUI.Label(label, title, overlayTitle);
                        OverlayDock(label, true);
                        propertyIndex = EditorGUI.Popup(new Rect(label.xMax + 4, header.y + 1, popupWidth, 18), propertyIndex, names);
                    }
                    else { GUI.Label(header, title, overlayTitle); OverlayDock(header, true); }
                    OverlaySettings(new Rect(area.xMax - 22, header.y + 1, 18, 18));
                    EditorGUI.DrawRect(new Rect(area.x + 4, area.y + 27, area.width - 8, 1), new Color(0.5f, 0.5f, 0.5f, 0.35f));
                    float y = area.y + 30;
                    foreach (string tip in tips)
                    {
                        float line = overlayTip.CalcHeight(new GUIContent(tip), width - 12);
                        GUI.Label(new Rect(area.x + 6, y, width - 12, line), tip, overlayTip);
                        y += line + 4;
                    }
                    if (mode == Mode.Shape)
                        foreach (ShapeControl control in ShapeControls(active))
                        {
                            bool current = (shapeControl & control) != 0;
                            Color background = GUI.backgroundColor;
                            GUI.backgroundColor = current ? new Color(0.56f, 0.94f, 0.47f) : new Color(1, 0.25f, 0.25f);
                            bool enabled = GUI.Toggle(new Rect(area.x + 6, y, width - 12, 18), current, control.ToString(), EditorStyles.miniButton);
                            GUI.backgroundColor = background;
                            if (enabled) shapeControl |= control; else shapeControl &= ~control;
                            y += 20;
                        }
                    if (mode == Mode.Shape && shapeControl == ShapeControl.None) mode = Mode.None;
                    Intercept(area);
                }
                DrawDockingGrid();
            }
            finally { Handles.EndGUI(); }
        }

        private string EditingTitle()
        {
            switch (mode)
            {
                case Mode.EndPosition: return "Editing End Position";
                case Mode.IgnoreSelection: return "Selecting Ignore Transforms";
                case Mode.IgnoreCopy: return "Copying Ignore Transforms";
                case Mode.CollisionSelection: return "Selecting Colliders";
                case Mode.CollisionCopy: return "Copying Colliders";
                case Mode.PropertyEdit: return "Editing:";
                default: return "Editing";
            }
        }

        internal void DrawTestingOverlay(Action restart, Action apply, Action stop, bool hasChanges)
        {
            var settings = DynamicsSettings.instance;
            if (!settings.editingOverlay) return;
            if (overlayTitle == null)
                overlayTitle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            if (overlayTip == null) overlayTip = new GUIStyle(overlayTitle);
            bool restartClicked;
            bool stopClicked;
            bool applyClicked;
            Handles.BeginGUI();
            try
            {
                Rect area = OverlayRect(settings.editingOverlayPosition, new Vector2(200, 100));
                OverlayBackground(area);
                Rect title = new Rect(area.x + 25, area.y + 4, area.width - 50, 20);
                GUI.Label(title, new GUIContent("Testing", "Drag the move or rotation handles to test motion. Ctrl-drag to grab; Shift-release to pose."), overlayTitle);
                OverlayDock(title, true);
                var color = GUI.color;
                GUI.color = settings.hideToolsDuringTesting ? settings.activeColor : settings.inactiveColor;
                var tools = new GUIContent(EditorGUIUtility.IconContent("d_CustomTool@2x")) { tooltip = "Toggle native tools during testing" };
                if (GUI.Button(new Rect(area.x + 6, title.y + 1, 18, 18), tools, GUIStyle.none))
                {
                    settings.hideToolsDuringTesting = !settings.hideToolsDuringTesting;
                    settings.SaveSettings();
                }
                GUI.color = color;
                OverlaySettings(new Rect(area.xMax - 22, title.y + 1, 18, 18));
                var background = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 0.25f, 0.25f);
                stopClicked = GUI.Button(new Rect(area.x + 6, area.y + 30, area.width - 12, 18), "Stop Testing", EditorStyles.miniButton);
                GUI.backgroundColor = new Color(0.25f, 0.65f, 1);
                restartClicked = GUI.Button(new Rect(area.x + 6, area.y + 50, area.width - 12, 18), "Restart", EditorStyles.miniButton);
                GUI.backgroundColor = background;
                using (new EditorGUI.DisabledScope(!hasChanges))
                    applyClicked = GUI.Button(new Rect(area.x + 6, area.y + 70, area.width - 12, 18), "Apply All Changes", EditorStyles.miniButton);
                Intercept(area);
                DrawDockingGrid();
            }
            finally { Handles.EndGUI(); }
            if (applyClicked) apply();
            if (restartClicked) { restart(); GUIUtility.ExitGUI(); }
            if (stopClicked) { stop(); GUIUtility.ExitGUI(); }
        }

        internal void CancelOverlayDrag()
        {
            if (draggingOverlay == 0) return;
            draggingOverlay = 0;
            GUIUtility.hotControl = 0;
        }

        private static string PropertyLabel(string name) => name.StartsWith("limitRotation.", StringComparison.Ordinal)
            ? "Limit Rotation " + name.Substring(name.Length - 1).ToUpperInvariant() : ObjectNames.NicifyVariableName(name);

        private static string CurveName(string name) => name.StartsWith("limitRotation.", StringComparison.Ordinal)
            ? "limitRotation" + name.Substring(name.Length - 1).ToUpperInvariant() + "Curve" : name + "Curve";

        internal static ShapeControl[] ShapeControls(Component component)
        {
            var controls = new List<ShapeControl> { ShapeControl.Position };
            if (component is ContactBase contact)
            {
                if (contact.shapeType != ContactBase.ShapeType.Sphere) controls.Add(ShapeControl.Rotation);
                if (contact.shapeType == ContactBase.ShapeType.Box) controls.Add(ShapeControl.Size);
                else
                {
                    controls.Add(ShapeControl.Radius);
                    if (contact.shapeType == ContactBase.ShapeType.Capsule) controls.Add(ShapeControl.Height);
                }
            }
            else if (component is VRCPhysBoneColliderBase collider)
            {
                if (collider.shapeType != VRCPhysBoneColliderBase.ShapeType.Sphere) controls.Add(ShapeControl.Rotation);
                if (collider.shapeType != VRCPhysBoneColliderBase.ShapeType.Plane) controls.Add(ShapeControl.Radius);
                if (collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Capsule) controls.Add(ShapeControl.Height);
            }
            return new[] { ShapeControl.Size, ShapeControl.Radius, ShapeControl.Height, ShapeControl.Position, ShapeControl.Rotation }
                .Where(controls.Contains).ToArray();
        }

        private static void OverlayBackground(Rect area)
        {
            if (Event.current.type != EventType.Repaint) return;
            GUI.DrawTexture(area, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.03f, 0.03f, 0.03f, 0.5f), 0, 8);
            GUI.DrawTexture(area, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.137f, 0.137f, 0.137f, 0.5f), 2, 8);
        }

        private static void OverlaySettings(Rect rect)
        {
            var icon = new GUIContent(EditorGUIUtility.IconContent("settings")) { tooltip = "Dynamics+ settings" };
            if (GUI.Button(rect, icon, GUIStyle.none)) DynamicsOverhaulWindow.Open();
        }

        private void OverlayDock(Rect title, bool editing)
        {
            int id = GUIUtility.GetControlID(editing ? 73112 : 73111, FocusType.Passive);
            EditorGUIUtility.AddCursorRect(title, MouseCursor.Pan);
            var current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && title.Contains(current.mousePosition))
            {
                draggingOverlay = editing ? 2 : 1;
                dragPosition = current.mousePosition;
                GUIUtility.hotControl = id;
                current.Use();
            }
            if (GUIUtility.hotControl != id || draggingOverlay == 0) return;
            if (current.type == EventType.MouseDrag)
            {
                dragPosition = current.mousePosition;
                current.Use();
                SceneView.RepaintAll();
            }
            if (current.type == EventType.MouseUp && current.button == 0)
            {
                var anchor = DockAnchor(current.mousePosition);
                if (editing) DynamicsSettings.instance.editingOverlayPosition = anchor;
                else DynamicsSettings.instance.toolOverlayPosition = anchor;
                draggingOverlay = 0;
                GUIUtility.hotControl = 0;
                DynamicsSettings.instance.SaveLayout();
                current.Use();
            }
        }

        private static DynamicsSettings.SceneAnchor DockAnchor(Vector2 point)
        {
            var view = SceneView.currentDrawingSceneView;
            Vector2 size = view == null ? new Vector2(800, 600) : view.position.size - new Vector2(0, 46);
            int column = Mathf.Clamp((int)(point.x / (size.x / 3)), 0, 2);
            int row = Mathf.Clamp((int)(point.y / (size.y / 3)), 0, 2);
            return new[] { DynamicsSettings.SceneAnchor.TopLeft, DynamicsSettings.SceneAnchor.Top, DynamicsSettings.SceneAnchor.TopRight,
                DynamicsSettings.SceneAnchor.Left, DynamicsSettings.SceneAnchor.Middle, DynamicsSettings.SceneAnchor.Right,
                DynamicsSettings.SceneAnchor.BottomLeft, DynamicsSettings.SceneAnchor.Bottom, DynamicsSettings.SceneAnchor.BottomRight }[row * 3 + column];
        }

        private void DrawDockingGrid()
        {
            if (draggingOverlay == 0 || Event.current.type != EventType.Repaint) return;
            var view = SceneView.currentDrawingSceneView;
            Vector2 size = view == null ? new Vector2(800, 600) : view.position.size - new Vector2(0, 46);
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                {
                    Rect cell = new Rect(column * size.x / 3 + 2, row * size.y / 3 + 2, size.x / 3 - 4, size.y / 3 - 4);
                    var color = cell.Contains(dragPosition) ? new Color(1, 0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    GUI.DrawTexture(cell, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 3, 8);
                }
        }

        internal static Rect OverlayRect(DynamicsSettings.SceneAnchor anchor, Vector2 size)
        {
            var view = SceneView.currentDrawingSceneView;
            Vector2 available = view == null ? new Vector2(800, 600) : view.position.size;
            float x = (available.x - size.x) * 0.5f;
            float y = (available.y - size.y) * 0.5f;
            if (anchor == DynamicsSettings.SceneAnchor.Left || anchor == DynamicsSettings.SceneAnchor.TopLeft || anchor == DynamicsSettings.SceneAnchor.BottomLeft) x = 12;
            if (anchor == DynamicsSettings.SceneAnchor.Right || anchor == DynamicsSettings.SceneAnchor.TopRight || anchor == DynamicsSettings.SceneAnchor.BottomRight) x = available.x - size.x - 12;
            if (anchor == DynamicsSettings.SceneAnchor.Top || anchor == DynamicsSettings.SceneAnchor.TopLeft || anchor == DynamicsSettings.SceneAnchor.TopRight) y = 36;
            if (anchor == DynamicsSettings.SceneAnchor.Bottom || anchor == DynamicsSettings.SceneAnchor.BottomLeft || anchor == DynamicsSettings.SceneAnchor.BottomRight) y = available.y - size.y - 48;
            return new Rect(Mathf.Max(12, x), Mathf.Max(36, y), size.x, size.y);
        }

        private static void Intercept(Rect area)
        {
            if (!DynamicsSettings.instance.overlayInterceptsClicks || !area.Contains(Event.current.mousePosition)) return;
            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.ScrollWheel) Event.current.Use();
        }

        private static void Label(Vector3 position, string text, float offset = 0)
        {
            Vector3 point = HandleUtility.WorldToGUIPointWithDepth(position);
            if (point.z <= 0) return;
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = DynamicsSettings.instance.labelColor;
            var content = new GUIContent(text);
            float width = style.CalcSize(content).x;
            Handles.BeginGUI();
            try { GUI.Label(new Rect(point.x - width * 0.5f, point.y - offset * 500 / point.z - 20, width, 20), content, style); }
            finally { Handles.EndGUI(); }
        }

        internal static Transform[] Endpoints(VRCPhysBoneBase bone)
        {
            var transforms = DynamicsOperations.AffectedTransforms(bone);
            var parents = new HashSet<Transform>(transforms.Select(t => t.parent));
            return transforms.Where(t => !parents.Contains(t)).ToArray();
        }

        private static void DrawChain(VRCPhysBoneBase bone)
        {
            var settings = DynamicsSettings.instance;
            if (settings.globalGizmos ? !settings.gizmosActive : !bone.showGizmos) return;
            Color color = settings.generalColor;
            color.a *= settings.globalGizmos ? settings.boneOpacity : bone.boneOpacity;
            using (new Handles.DrawingScope(color))
            {
                var affected = DynamicsOperations.AffectedTransforms(bone);
                var set = new HashSet<Transform>(affected);
                foreach (var transform in affected)
                {
                    if (transform.parent != null && set.Contains(transform.parent)) Handles.DrawLine(transform.parent.position, transform.position);
                    if (!affected.Any(t => t.parent == transform) && bone.endpointPosition != Vector3.zero)
                        Handles.DrawLine(transform.position, transform.TransformPoint(bone.endpointPosition));
                }
            }
        }

        private static void DrawEndpoint(VRCPhysBoneBase bone, VRCPhysBoneBase[] bones)
        {
            var endpoints = Endpoints(bone);
            foreach (var transform in endpoints)
            {
                Vector3 previous = transform.TransformPoint(bone.endpointPosition);
                EditorGUI.BeginChangeCheck();
                Vector3 next = Handles.PositionHandle(previous, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 delta = transform.InverseTransformPoint(next) - bone.endpointPosition;
                    DynamicsOperations.Change(bones, "Edit PhysBone endpoint", component =>
                        ((VRCPhysBoneBase)component).endpointPosition += delta);
                }
            }
        }

        private static void DrawIgnoreSelection(VRCPhysBoneBase[] bones)
        {
            var candidates = bones.SelectMany(b => b.GetRootTransform().GetComponentsInChildren<Transform>(true)).Distinct();
            foreach (var transform in candidates)
            {
                int count = bones.Count(b => b.ignoreTransforms != null && b.ignoreTransforms.Contains(transform));
                using (new Handles.DrawingScope(count == bones.Length ? DynamicsSettings.instance.activeColor : count == 0 ? DynamicsSettings.instance.inactiveColor : DynamicsSettings.instance.mixedColor))
                {
                    float size = 0.25f * DynamicsSettings.instance.handleSizeMultiplier;
                    if (Handles.Button(transform.position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                        DynamicsOperations.ToggleIgnore(bones, transform);
                    if (DynamicsSettings.instance.sceneLabels) Label(transform.position, transform.name, size + 0.01f);
                }
            }
        }

        private static void DrawColliderSelection(GameObject root, VRCPhysBoneBase[] bones)
        {
            if (bones.Length == 0) return;
            var candidates = Resources.FindObjectsOfTypeAll<VRCPhysBoneColliderBase>()
                .Where(c => c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(c) &&
                    (DynamicsPreview.Active != null ? DynamicsPreview.Active.ContainsCopy(c) : !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(c.gameObject.scene)));
            foreach (var collider in candidates)
            {
                int count = bones.Count(b => b.colliders != null && b.colliders.Contains(collider));
                Vector3 center = collider.GetRootTransform().TransformPoint(collider.position);
                float size = 0.05f * DynamicsSettings.instance.handleSizeMultiplier;
                using (new Handles.DrawingScope(count == bones.Length ? DynamicsSettings.instance.activeColor : count == 0 ? DynamicsSettings.instance.inactiveColor : DynamicsSettings.instance.mixedColor))
                {
                    if (Handles.Button(center, Quaternion.identity, size, size, Handles.SphereHandleCap))
                        DynamicsOperations.ToggleCollider(bones, collider);
                    if (DynamicsSettings.instance.sceneLabels) Label(center, collider.gameObject.name, size + 0.01f);
                }
            }
        }

        private void DrawCopy(GameObject root, VRCPhysBoneBase[] destinations)
        {
            if (destinations.Length == 0) return;
            foreach (var source in root.GetComponentsInChildren<VRCPhysBoneBase>(true))
            {
                Vector3 position = source.GetRootTransform().position;
                float size = 0.25f * DynamicsSettings.instance.handleSizeMultiplier;
                if (Handles.Button(position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    DynamicsOperations.Change(destinations, mode == Mode.IgnoreCopy ? "Copy ignored transforms" : "Copy PhysBone colliders", component =>
                    {
                        var destination = (VRCPhysBoneBase)component;
                        if (mode == Mode.IgnoreCopy) destination.ignoreTransforms = new List<Transform>(source.ignoreTransforms ?? new List<Transform>());
                        else destination.colliders = new List<VRCPhysBoneColliderBase>(source.colliders ?? new List<VRCPhysBoneColliderBase>());
                    });
                }
                if (DynamicsSettings.instance.sceneLabels) Label(position, source.gameObject.name, size + 0.01f);
            }
        }

        private void DrawProperty(VRCPhysBoneBase active, VRCPhysBoneBase[] bones)
        {
            if (active == null) return;
            string name = Properties[propertyIndex];
            var serialized = new SerializedObject(active);
            var property = serialized.FindProperty(name);
            if (property == null || property.propertyType != SerializedPropertyType.Float) return;
            string curveName = CurveName(name);
            bool curve = serialized.FindProperty(curveName) != null && (editCurve || bones.Length <= 1 && Event.current.alt);
            DrawPropertyHandles(active, bones, name, curveName, curve);
        }

        private static void DrawPropertyHandles(VRCPhysBoneBase active, VRCPhysBoneBase[] bones, string name, string curveName, bool curveEdit)
        {
            var visible = bones.Length > 1 && Event.current.alt ? new[] { active } : bones;
            foreach (var bone in visible)
            {
                var serialized = new SerializedObject(bone);
                var field = serialized.FindProperty(name);
                if (field == null) continue;
                float previous = field.floatValue;
                var animation = serialized.FindProperty(curveName)?.animationCurveValue;
                var transforms = DynamicsOperations.AffectedTransforms(bone);
                if (transforms.Count == 0) continue;
                var depths = new Dictionary<Transform, int>();
                foreach (var transform in transforms) depths[transform] = transform.parent != null && depths.TryGetValue(transform.parent, out var depth) ? depth + 1 : 0;
                int maximum = Mathf.Max(1, depths.Values.Max() + (bone.endpointPosition == Vector3.zero ? 0 : 1));
                var samples = transforms.Select(t => new PropertySample { transform = t, position = t.position, depth = depths[t] }).ToList();
                if (bone.endpointPosition != Vector3.zero)
                    samples.AddRange(Endpoints(bone).Select(t => new PropertySample { transform = t, position = t.TransformPoint(bone.endpointPosition), depth = depths[t] + 1 }));
                var graph = new Dictionary<Transform, Vector3>();
                foreach (var sample in samples)
                {
                    var transform = sample.transform;
                    float time = (float)sample.depth / maximum;
                    float factor = animation != null && animation.length >= 2 ? animation.Evaluate(time) : 1f;
                    float value = previous * factor;
                    EditorGUI.BeginChangeCheck();
                    float next;
                    var modifiers = Event.current.modifiers;
                    Event.current.modifiers &= ~EventModifiers.Alt;
                    try
                    {
                        if (name == "radius")
                        {
                            Vector3 scale = transform.lossyScale;
                            float multiplier = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z), 0.0001f);
                            using (new Handles.DrawingScope(DynamicsSettings.instance.generalColor))
                                next = Handles.RadiusHandle(transform.rotation, sample.position, value * multiplier) / multiplier;
                        }
                        else
                        {
                            float multiplier = DynamicsSettings.instance.handleSizeMultiplier;
                            float range = name.StartsWith("limitRotation.", StringComparison.Ordinal) ? 360 : name == "maxAngleX" ? 180 : name == "maxAngleZ" ? 90 : 1;
                            Vector3 position = sample.position + Vector3.up * (value * multiplier / range);
                            float size = Mathf.Clamp(HandleUtility.GetHandleSize(bone.GetRootTransform().position) * 0.05f * multiplier, 0.02f * multiplier, 2 * multiplier);
                            using (new Handles.DrawingScope(DynamicsSettings.instance.generalColor))
                            {
                                Handles.DrawDottedLine(sample.position, position, 5);
                                if (graph.TryGetValue(transform, out var previousPoint)) Handles.DrawAAPolyLine(3 * multiplier, previousPoint, position);
                                else if (transform.parent != null && graph.TryGetValue(transform.parent, out var parentPoint)) Handles.DrawAAPolyLine(3 * multiplier, parentPoint, position);
                                graph[transform] = position;
                                Label(position, value.ToString("F2"), size + 0.01f);
                                Vector3 moved = Handles.Slider(position, Vector3.up, size, Handles.DotHandleCap, 0);
                                next = value + (moved.y - position.y) * range / multiplier;
                            }
                        }
                    }
                    finally { Event.current.modifiers = modifiers; }
                    if (!EditorGUI.EndChangeCheck()) continue;
                    if (curveEdit)
                    {
                        DynamicsOperations.Change(new[] { bone }, "Edit PhysBone curve", component =>
                        {
                            var target = new SerializedObject(component);
                            var scalar = target.FindProperty(name);
                            var curve = target.FindProperty(curveName);
                            if (curve == null) return;
                            float baseline = scalar.floatValue;
                            curve.animationCurveValue = CurveAtValue(curve.animationCurveValue, time, ClampProperty(name, next), ref baseline);
                            scalar.floatValue = baseline;
                            target.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        float scalar = Mathf.Abs(factor) > 0.0001f ? next / factor : next;
                        var affected = bones.Length > 1 && Event.current.alt ? new[] { bone } : bones;
                        bool absolute = Event.current.shift;
                        DynamicsOperations.Change(affected, "Edit PhysBone " + name, component =>
                        {
                            var target = new SerializedObject(component);
                            var destination = target.FindProperty(name);
                            destination.floatValue = ClampProperty(name, absolute || component == bone ? scalar : destination.floatValue + scalar - previous);
                            target.ApplyModifiedProperties();
                        });
                    }
                    return;
                }
            }
        }

        private struct PropertySample
        {
            internal Transform transform;
            internal Vector3 position;
            internal int depth;
        }

        private static float ClampProperty(string name, float value)
        {
            if (name.StartsWith("limitRotation.", StringComparison.Ordinal)) return Mathf.Clamp(value, 0, 360);
            if (name == "radius" || name == "maxStretch") return Mathf.Max(0, value);
            var range = typeof(VRCPhysBoneBase).GetField(name)?.GetCustomAttributes(typeof(RangeAttribute), true).FirstOrDefault() as RangeAttribute;
            return range == null ? value : Mathf.Clamp(value, range.min, range.max);
        }

        internal static AnimationCurve CurveAtValue(AnimationCurve original, float time, float value, ref float baseline)
        {
            var curve = original != null && original.length >= 2 ? new AnimationCurve(original.keys) : AnimationCurve.Constant(0, 1, 1);
            if (Mathf.Abs(baseline) < 0.0001f)
            {
                baseline = Mathf.Abs(value) < 0.0001f ? 1 : value;
                for (int i = 0; i < curve.length; i++) curve.MoveKey(i, new Keyframe(curve[i].time, 0));
            }
            float factor = value / baseline;
            int index = Array.FindIndex(curve.keys, key => Mathf.Abs(key.time - time) < 0.0001f);
            if (index < 0) curve.AddKey(time, factor);
            else curve.MoveKey(index, new Keyframe(time, factor));
            float maximum = Mathf.Max(1, curve.keys.Max(key => Mathf.Abs(key.value)));
            if (maximum > 1)
            {
                baseline *= maximum;
                for (int i = 0; i < curve.length; i++)
                {
                    var key = curve[i];
                    key.value /= maximum;
                    key.inTangent /= maximum;
                    key.outTangent /= maximum;
                    curve.MoveKey(i, key);
                }
            }
            curve.preWrapMode = original == null ? WrapMode.ClampForever : original.preWrapMode;
            curve.postWrapMode = original == null ? WrapMode.ClampForever : original.postWrapMode;
            return curve;
        }

        private void DrawShape(Component component, IReadOnlyList<Component> selection)
        {
            bool isContact = component is ContactBase;
            if (!isContact && !(component is VRCPhysBoneColliderBase)) return;
            var contact = component as ContactBase;
            var collider = component as VRCPhysBoneColliderBase;
            Transform root = DynamicsOperations.Root(component);
            Vector3 position = isContact ? contact.position : collider.position;
            Quaternion rotation = isContact ? contact.rotation : collider.rotation;
            float radius = isContact ? contact.radius : collider.radius;
            float height = isContact ? contact.height : collider.height;
            bool capsule = isContact ? contact.shapeType == ContactBase.ShapeType.Capsule : collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Capsule;
            bool box = isContact && contact.shapeType == ContactBase.ShapeType.Box;
            bool plane = !isContact && collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Plane;
            using (new Handles.DrawingScope(root.localToWorldMatrix))
            {
                EditorGUI.BeginChangeCheck();
                Vector3 nextPosition = (shapeControl & ShapeControl.Position) != 0 ? Handles.PositionHandle(position, rotation) : position;
                Quaternion nextRotation = (shapeControl & ShapeControl.Rotation) != 0 ? Handles.RotationHandle(rotation, position) : rotation;
                float nextRadius = radius;
                if (!plane && !box && (shapeControl & ShapeControl.Radius) != 0)
                {
                    if (capsule)
                    {
                        Vector3 offset = rotation * Vector3.up * Mathf.Max(0, height * 0.5f - radius);
                        float top = Handles.RadiusHandle(rotation, position + offset, radius, true);
                        float bottom = Handles.RadiusHandle(rotation, position - offset, radius, true);
                        nextRadius = Mathf.Max(0, top != radius ? top : bottom);
                    }
                    else nextRadius = Mathf.Max(0, Handles.RadiusHandle(rotation, position, radius));
                }
                float nextHeight = height;
                Vector3 nextSize = box ? ((shapeControl & ShapeControl.Size) != 0 ? Handles.ScaleHandle(contact.size, position, rotation, HandleUtility.GetHandleSize(root.TransformPoint(position))) : contact.size) : Vector3.zero;
                if (capsule && (shapeControl & ShapeControl.Height) != 0)
                {
                    Vector3 axis = rotation * Vector3.up;
                    Vector3 tip = position + axis * height * 0.5f;
                    Vector3 lowerTip = position - axis * height * 0.5f;
                    Vector3 nextTip;
                    Vector3 nextLowerTip;
                    using (new Handles.DrawingScope(Matrix4x4.identity))
                    {
                        Vector3 worldAxis = root.TransformVector(axis).normalized;
                        nextTip = root.InverseTransformPoint(Handles.Slider(root.TransformPoint(tip), worldAxis));
                        nextLowerTip = root.InverseTransformPoint(Handles.Slider(root.TransformPoint(lowerTip), -worldAxis));
                    }
                    nextHeight = Mathf.Max(0f, height + (nextTip != tip ? Vector3.Dot(nextTip - tip, axis) * 2f :
                        Vector3.Dot(lowerTip - nextLowerTip, axis) * 2f));
                    Handles.DrawLine(lowerTip, tip);
                }
                if (capsule)
                {
                    Vector3 offset = rotation * Vector3.up * Mathf.Max(0, height * 0.5f - radius);
                    Vector3 up = rotation * Vector3.up;
                    Vector3 right = rotation * Vector3.right;
                    Vector3 forward = rotation * Vector3.forward;
                    Handles.DrawWireDisc(position + offset, up, radius);
                    Handles.DrawWireDisc(position - offset, up, radius);
                    Handles.DrawWireArc(position + offset, forward, right, 180, radius);
                    Handles.DrawWireArc(position - offset, forward, right, -180, radius);
                    Handles.DrawWireArc(position + offset, right, -forward, 180, radius);
                    Handles.DrawWireArc(position - offset, right, -forward, -180, radius);
                    foreach (Vector3 direction in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
                    {
                        Vector3 edge = rotation * direction * radius;
                        Handles.DrawLine(position + offset + edge, position - offset + edge);
                    }
                }
                if (box)
                    using (new Handles.DrawingScope(root.localToWorldMatrix * Matrix4x4.TRS(position, rotation, Vector3.one)))
                        Handles.DrawWireCube(Vector3.zero, contact.size);
                if (plane) Handles.DrawWireDisc(position, rotation * Vector3.up, Mathf.Max(radius, 0.2f));
                if (!EditorGUI.EndChangeCheck()) return;
                Vector3 movement = root.TransformVector(nextPosition - position);
                Quaternion turn = root.rotation * nextRotation * Quaternion.Inverse(root.rotation * rotation);
                Vector3 sizeDelta = box ? nextSize - contact.size : Vector3.zero;
                ApplyShapeChange(component, selection, movement, turn, nextRadius - radius, nextHeight - height, sizeDelta, box);
            }
        }

        internal static void ApplyShapeChange(Component component, IReadOnlyList<Component> selection, Vector3 movement, Quaternion turn,
            float radiusChange, float heightChange, Vector3 sizeChange, bool box)
        {
            DynamicsOperations.Change(selection.Where(c => c != null && c.GetType() == component.GetType()), "Edit dynamics shape", target =>
            {
                Transform targetRoot = DynamicsOperations.Root(target);
                Vector3 delta = targetRoot.InverseTransformVector(movement);
                Quaternion localTurn = Quaternion.Inverse(targetRoot.rotation) * turn * targetRoot.rotation;
                if (target is ContactBase destination)
                {
                    destination.position += delta; destination.rotation = localTurn * destination.rotation;
                    destination.radius = Mathf.Max(0, destination.radius + radiusChange);
                    destination.height = Mathf.Max(0, destination.height + heightChange);
                    if (box)
                    {
                        Vector3 size = destination.size + sizeChange;
                        destination.size = new Vector3(Mathf.Max(0, size.x), Mathf.Max(0, size.y), Mathf.Max(0, size.z));
                    }
                }
                else
                {
                    var destinationCollider = (VRCPhysBoneColliderBase)target;
                    destinationCollider.position += delta; destinationCollider.rotation = localTurn * destinationCollider.rotation;
                    destinationCollider.radius = Mathf.Max(0, destinationCollider.radius + radiusChange);
                    destinationCollider.height = Mathf.Max(0, destinationCollider.height + heightChange);
                }
            });
        }
    }
}
#endif
