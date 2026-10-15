#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    [InitializeOnLoad]
    internal static class DynamicsInspectorTools
    {
        internal static bool Registered { get; private set; }
        static DynamicsInspectorTools() { EditorApplication.delayCall += Register; }
        internal static void Register()
        {
            try
            {
                bool rebuild = !Registered;
                var registry = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes", true);
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                registry.GetMethod("FindCustomEditorTypeByType", flags).Invoke(null, new object[] { typeof(VRCPhysBone), false });
                foreach (string name in new[] { "kSCustomEditors", "kSCustomMultiEditors" })
                {
                    var cache = (IDictionary)registry.GetField(name, flags).GetValue(null);
                    foreach (Type type in new[] { typeof(VRCPhysBone), typeof(VRCPhysBoneCollider), typeof(VRCContactReceiver), typeof(VRCContactSender) })
                    {
                        var entries = cache[type] as IList;
                        var entry = entries?.Cast<object>().FirstOrDefault(e =>
                            typeof(DynamicsComponentEditor).IsAssignableFrom((Type)e.GetType().GetField("m_InspectorType").GetValue(e)));
                        if (entry == null) throw new InvalidOperationException("Missing Dynamics+ editor for " + type.Name);
                        rebuild |= entries.IndexOf(entry) != 0;
                        entries.Remove(entry);
                        entries.Insert(0, entry);
                    }
                }
                Registered = true;
                if (rebuild)
                    foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>().Where(value => value.GetType().FullName == "UnityEditor.InspectorWindow"))
                    {
                        var tracker = window.GetType().GetProperty("tracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(window) as ActiveEditorTracker;
                        tracker?.ForceRebuild();
                        window.Repaint();
                    }
            }
            catch (Exception error)
            {
                Registered = false;
                Debug.LogWarning("Dynamics+ inspector registration is unavailable: " + error.GetBaseException().Message);
            }
        }
    }

    [CustomEditor(typeof(VRCPhysBone)), CanEditMultipleObjects]
    internal sealed class DynamicsPhysBoneEditor : DynamicsComponentEditor { }
    [CustomEditor(typeof(VRCPhysBoneCollider)), CanEditMultipleObjects]
    internal sealed class DynamicsColliderEditor : DynamicsComponentEditor { }
    [CustomEditor(typeof(VRCContactReceiver)), CanEditMultipleObjects]
    internal sealed class DynamicsReceiverEditor : DynamicsComponentEditor { }
    [CustomEditor(typeof(VRCContactSender)), CanEditMultipleObjects]
    internal sealed class DynamicsSenderEditor : DynamicsComponentEditor { }

    internal abstract class DynamicsComponentEditor : UnityEditor.Editor
    {
        private static readonly DynamicsSceneTools tools = new DynamicsSceneTools();
        private static GUIStyle editButtonStyle;
        private static Component sceneTarget;
        private static bool hidesNativeTools;
        private static bool previousNativeToolsHidden;
        private Component[] sceneComponents = Array.Empty<Component>();
        private readonly Dictionary<string, AnimBool> foldouts = new Dictionary<string, AnimBool>();
        private readonly HashSet<string> drawn = new HashSet<string>();
        private readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();
        private UnityEditor.Editor sdkEditor;
        private UnityEditor.Editor previewEditor;
        private bool sdkView;
        private Component Component => target as Component;
        private Component[] Components => targets.OfType<Component>().ToArray();

        static DynamicsComponentEditor()
        {
            EditorApplication.update += RefreshNativeTools;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreNativeTools;
            EditorApplication.quitting += RestoreNativeTools;
        }

        private static void RefreshNativeTools()
        {
            bool editing = (DynamicsEditorWindow.EditingScene || tools.mode != DynamicsSceneTools.Mode.None && sceneTarget != null &&
                Selection.gameObjects.Contains(sceneTarget.gameObject) && DynamicsSettings.instance.sceneTools &&
                DynamicsSettings.instance.InspectorEnabled(sceneTarget)) &&
                DynamicsPreview.Active == null && !EditorApplication.isPlayingOrWillChangePlaymode;
            if (!editing) { RestoreNativeTools(); return; }
            if (!hidesNativeTools) { previousNativeToolsHidden = Tools.hidden; hidesNativeTools = true; }
            Tools.hidden = true;
        }

        private static void RestoreNativeTools()
        {
            if (!hidesNativeTools) return;
            Tools.hidden = previousNativeToolsHidden;
            hidesNativeTools = false;
        }

        protected virtual void OnEnable() { sceneComponents = Components; SceneView.duringSceneGui += PreviewSceneGUI; }

        protected virtual void OnDisable()
        {
            SceneView.duringSceneGui -= PreviewSceneGUI;
            foreach (var value in foldouts.Values) value.valueChanged.RemoveListener(Repaint);
            if (sdkEditor != null) DestroyImmediate(sdkEditor);
            if (previewEditor != null) DestroyImmediate(previewEditor);
        }

        public override bool RequiresConstantRepaint() => EditorApplication.isPlaying || DynamicsPreview.Active != null;

        public override void OnInspectorGUI()
        {
            if (Component == null) return;
            var preview = DynamicsPreview.Active;
            if (preview != null && !preview.ContainsCopy(Component))
            {
                var mapped = Components.Select(component => preview.CopyOf(component)).ToArray();
                if (mapped.All(value => value is Component))
                {
                    CreateCachedEditor(mapped, GetType(), ref previewEditor);
                    previewEditor.OnInspectorGUI();
                    return;
                }
                EditorGUILayout.HelpBox("Stop testing to edit components outside this preview.", MessageType.Info);
                return;
            }
            if (!DynamicsSettings.instance.InspectorEnabled(Component) || sdkView)
            {
                var sdkType = DynamicsSdkEditors.TypeFor(Component);
                if (sdkType != null) CreateCachedEditor(targets, sdkType, ref sdkEditor);
                if (sdkEditor != null) sdkEditor.OnInspectorGUI();
                else DrawDefaultInspector();
                if (DynamicsSettings.instance.InspectorEnabled(Component) && GUILayout.Button("Dynamics+ Editor")) sdkView = false;
                return;
            }
            serializedObject.Update();
            drawn.Clear();
            drawn.Add("m_Script");
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160;
            try
            {
                if (Component is VRCPhysBoneBase || Component is VRCPhysBoneColliderBase)
                    DrawTestControls();
                if (Component is VRCPhysBoneBase)
                {
                    DrawBone();
                }
                else
                {
                    EditorGUIUtility.labelWidth = 253;
                    DrawContactOrCollider();
                }
                DrawAdditional();
                if (serializedObject.ApplyModifiedProperties() && DynamicsPreview.Active != null)
                    DynamicsPreview.Active.RefreshConfiguration();
            }
            finally { EditorGUIUtility.labelWidth = previousLabelWidth; }
            if (GUILayout.Button("SDK Editor", EditorStyles.miniButton))
            {
                sdkView = true;
                tools.mode = DynamicsSceneTools.Mode.None;
                RestoreNativeTools();
                SceneView.RepaintAll();
            }
            DynamicsInfo.DrawCredit();
        }

        private void DrawTestControls()
        {
            var preview = DynamicsPreview.Active;
            if (preview == null)
            {
                if (GUILayout.Button("Test PhysBones in Scene")) StartTest();
                return;
            }
            bool stop;
            bool restart;
            bool apply;
            using (new EditorGUILayout.HorizontalScope())
            {
                var background = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 0.25f, 0.25f);
                stop = GUILayout.Button("Stop Testing - ESC / Enter");
                GUI.backgroundColor = new Color(0.25f, 0.65f, 1);
                restart = GUILayout.Button("Restart", GUILayout.Width(50));
                GUI.backgroundColor = background;
                using (new EditorGUI.DisabledScope(!preview.HasChanges(Component)))
                    apply = GUILayout.Button("Apply Changes", GUILayout.Width(95));
            }
            if (apply) preview.Apply(Component);
            if (restart) { preview.Restart(); GUIUtility.ExitGUI(); }
            if (stop) { preview.Dispose(); GUIUtility.ExitGUI(); }
        }

        private void StartTest()
        {
            try
            {
                if (DynamicsPreview.Active != null) return;
                tools.mode = DynamicsSceneTools.Mode.None;
                RestoreNativeTools();
                var preview = new DynamicsPreview();
                preview.Start(null);
                var copy = preview.CopyOf(Component);
                if (copy != null) Selection.activeObject = copy;
            }
            catch (Exception error) { EditorUtility.DisplayDialog("Physbone Tester", error.Message, "OK"); }
        }

        private void DrawBone()
        {
            Section("Transforms", () =>
            {
                Root();
                Field("endpointPosition", "End Position", DynamicsSceneTools.Mode.EndPosition);
                Fields("multiChildType", "ignoreOtherPhysBones", "ignoreTransforms");
            });
            Section("Forces", () =>
            {
                drawn.Add("integrationType");
                Field("pull");
                Field("spring", serializedObject.FindProperty("integrationType")?.enumValueIndex == 1 ? "Momentum" : "Spring");
                var integration = serializedObject.FindProperty("integrationType");
                if (integration == null || integration.hasMultipleDifferentValues || integration.enumValueIndex == 1) Field("stiffness");
                Fields("immobile", "gravity", "gravityFalloff", "immobileType");
            }, () =>
            {
                var integration = serializedObject.FindProperty("integrationType");
                if (integration == null) return;
                EditorGUI.showMixedValue = integration.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                bool advanced = StateToggle(integration.enumValueIndex == 1, "Advanced", 70);
                if (EditorGUI.EndChangeCheck()) integration.enumValueIndex = advanced ? 1 : 0;
                EditorGUI.showMixedValue = false;
            });
            Section("Limits", () =>
            {
                Field("limitType", "Type");
                var limit = serializedObject.FindProperty("limitType");
                if (limit != null && !limit.hasMultipleDifferentValues && limit.enumNames[limit.enumValueIndex] == "None") return;
                Field("maxAngleX");
                if (limit == null || limit.hasMultipleDifferentValues || limit.enumNames[limit.enumValueIndex] == "Polar") Field("maxAngleZ");
                Field("limitRotation");
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Limit Rotation Curves", GUILayout.MinWidth(EditorGUIUtility.labelWidth - 4));
                    foreach (string axis in new[] { "X", "Y", "Z" })
                    {
                        GUILayout.Label(axis, GUILayout.Width(10));
                        var curve = serializedObject.FindProperty("limitRotation" + axis + "Curve");
                        if (curve != null && drawn.Add(curve.propertyPath)) DrawCurve(curve);
                    }
                }
            });
            Section("Collisions", () =>
            {
                Fields("radius", "allowCollision", "collisionFilter", "colliders");
            });
            Section("Grab & Pose", () => Fields("allowGrabbing", "grabFilter", "allowPosing", "poseFilter", "grabMovement", "snapToHand"));
            Section("Stretch & Squish", () => Fields("stretchMotion", "maxStretch", "maxSquish"));
            Section("Options", () =>
            {
                Fields("version", "isAnimated", "resetWhenDisabled");
                if (!DynamicsSdkEditors.IsWorld(Component)) { Field("parameter"); DynamicsAvatarBridge.Draw(Component); }
                ParameterReadouts();
            });
            Section("Gizmos", DrawGizmos, DrawGizmoHeader);
        }

        private void DrawContactOrCollider()
        {
            Section("Shape", () =>
            {
                bool box = Component is ContactBase contact && contact.shapeType == ContactBase.ShapeType.Box;
                bool capsule = Component is ContactBase shape ? shape.shapeType == ContactBase.ShapeType.Capsule :
                    ((VRCPhysBoneColliderBase)Component).shapeType == VRCPhysBoneColliderBase.ShapeType.Capsule;
                bool plane = Component is VRCPhysBoneColliderBase planeCollider && planeCollider.shapeType == VRCPhysBoneColliderBase.ShapeType.Plane;
                using (new EditorGUILayout.HorizontalScope())
                {
                    Field("shapeType", "Type");
                    var inside = serializedObject.FindProperty("insideBounds");
                    if (inside != null && !plane)
                    {
                        drawn.Add(inside.propertyPath);
                        EditorGUI.showMixedValue = inside.hasMultipleDifferentValues;
                        EditorGUI.BeginChangeCheck();
                        bool outside = StateToggle(!inside.boolValue, inside.boolValue ? "Inside Bounds" : "Outside Bounds", 100);
                        if (EditorGUI.EndChangeCheck()) inside.boolValue = !outside;
                        EditorGUI.showMixedValue = false;
                    }
                }
                Root();
                EditorGUILayout.Space(4);
                if (box) Field("size", "Size", DynamicsSceneTools.Mode.Shape, DynamicsSceneTools.ShapeControl.Size);
                else if (!plane) Field("radius", "Radius", DynamicsSceneTools.Mode.Shape, DynamicsSceneTools.ShapeControl.Radius);
                if (capsule) Field("height", "Height", DynamicsSceneTools.Mode.Shape, DynamicsSceneTools.ShapeControl.Height);
                Field("position", "Position", DynamicsSceneTools.Mode.Shape, DynamicsSceneTools.ShapeControl.Position);
                if (capsule || plane || box)
                    Field("rotation", "Rotation", DynamicsSceneTools.Mode.Shape, DynamicsSceneTools.ShapeControl.Rotation);
                Field("bonesAsSpheres");
            });
            if (Component is VRCContactReceiver)
                Section("Receiver", () =>
                {
                    if (!DynamicsSdkEditors.IsWorld(Component))
                    {
                        Fields("receiverType", "parameter");
                        var receiver = serializedObject.FindProperty("receiverType");
                        if (receiver.hasMultipleDifferentValues || receiver.enumValueIndex == 1) Field("minVelocity");
                        DynamicsAvatarBridge.Draw(Component);
                        ParameterReadouts();
                    }
                    else EditorGUILayout.LabelField("Receivers in worlds trigger Udon contact events.", EditorStyles.wordWrappedMiniLabel);
                    if (((ContactBase)Component).shapeType == ContactBase.ShapeType.Box) Field("useFaceProximity");
                });
            if (Component is ContactBase)
                Section("Filtering", () =>
                {
                    if (DynamicsSdkEditors.IsWorld(Component)) Field("contentTypes");
                    else Fields("allowSelf", "allowOthers", "localOnly");
                    Field("collisionTags");
                    TagMenu();
                });
            else Section("Global Collision", () =>
            {
                Field("globalCollision");
                var collider = (VRCPhysBoneColliderBase)Component;
                if (collider.globalCollision == VRCPhysBoneBase.AdvancedBool.Other)
                    Field("globalCollisionFlags", "Content Types");
                if (collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Plane &&
                    collider.globalCollision != VRCPhysBoneBase.AdvancedBool.False)
                    EditorGUILayout.HelpBox("Global collision is not supported on Plane colliders.", MessageType.Warning);
            });
        }

        private void ParameterReadouts()
        {
            if (!EditorApplication.isPlaying && !(DynamicsPreview.Active?.ContainsCopy(Component) ?? false)) return;
            using (new EditorGUI.DisabledScope(true))
                foreach (string name in Component is VRCPhysBoneBase ?
                    new[] { "param_IsGrabbedValue", "param_IsPosedValue", "param_AngleValue", "param_StretchValue", "param_SquishValue" } : new[] { "paramValue" })
                {
                    var field = Component.GetType().GetField(name);
                    if (field == null) continue;
                    object value = field.GetValue(Component);
                    string label = name.Replace("param_", "").Replace("Value", "");
                    if (value is bool flag) EditorGUILayout.Toggle(label, flag);
                    else if (value is float number) EditorGUILayout.FloatField(label, number);
                }
        }

        private void TagMenu()
        {
            if (!GUILayout.Button("Choose Collision Tag", EditorStyles.miniButton)) return;
            string[] defaults = { "Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL", "FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR", "FingerMiddleR", "FingerRingR", "FingerLittleR" };
            var menu = new GenericMenu();
            foreach (string tag in defaults) AddTagChoice(menu, "Default/", tag);
            foreach (string tag in Resources.FindObjectsOfTypeAll<ContactBase>().Where(c => c.gameObject.scene.IsValid() && !EditorUtility.IsPersistent(c))
                .SelectMany(c => c.collisionTags ?? new List<string>()).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t))
                AddTagChoice(menu, "Scene/", tag);
            menu.ShowAsContext();
        }

        private void AddTagChoice(GenericMenu menu, string path, string tag)
        {
            var components = Components.OfType<ContactBase>().ToArray();
            bool present = components.All(c => c.collisionTags != null && c.collisionTags.Contains(tag));
            if (!present && components.Any(c => c.collisionTags != null && !c.collisionTags.Contains(tag) && c.collisionTags.Count >= ContactBase.MAX_COLLISION_TAGS))
            {
                menu.AddDisabledItem(new GUIContent(path + tag));
                return;
            }
            menu.AddItem(new GUIContent(path + tag), present, () => DynamicsOperations.Change(components, "Choose collision tag", component =>
            {
                var contact = (ContactBase)component;
                var tags = new List<string>(contact.collisionTags ?? new List<string>());
                if (present) tags.RemoveAll(value => value == tag);
                else if (!tags.Contains(tag)) tags.Add(tag);
                contact.collisionTags = tags;
            }));
        }

        private void DrawGizmoHeader()
        {
            var settings = DynamicsSettings.instance;
            var local = serializedObject.FindProperty("showGizmos");
            EditorGUI.BeginChangeCheck();
            bool shown = StateToggle(settings.globalGizmos ? settings.gizmosActive : local.boolValue, "Show Gizmos", 90);
            settings.globalGizmos = StateToggle(settings.globalGizmos, "Global Setting", 100);
            if (settings.globalGizmos) settings.gizmosActive = shown;
            else local.boolValue = shown;
            if (EditorGUI.EndChangeCheck()) settings.SaveGizmos();
        }

        private void DrawGizmos()
        {
            var settings = DynamicsSettings.instance;
            EditorGUI.BeginChangeCheck();
            if (settings.globalGizmos)
            {
                settings.boneOpacity = EditorGUILayout.Slider("Bone Opacity", settings.boneOpacity, 0, 1);
                settings.limitOpacity = EditorGUILayout.Slider("Limit Opacity", settings.limitOpacity, 0, 1);
            }
            if (EditorGUI.EndChangeCheck()) settings.SaveGizmos();
            if (settings.globalGizmos)
            {
                Set("showGizmos", settings.gizmosActive);
                Set("boneOpacity", settings.boneOpacity);
                Set("limitOpacity", settings.limitOpacity);
            }
            else Fields("boneOpacity", "limitOpacity");
        }

        private void Set(string name, object value)
        {
            var property = serializedObject.FindProperty(name);
            if (property == null) return;
            if (value is bool flag && (property.hasMultipleDifferentValues || property.boolValue != flag)) property.boolValue = flag;
            else if (value is float number && (property.hasMultipleDifferentValues || !Mathf.Approximately(property.floatValue, number))) property.floatValue = number;
        }

        private void Root()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Field("rootTransform", "Root");
                if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(20)))
                {
                    serializedObject.ApplyModifiedProperties();
                    DynamicsOperations.Change(Components, "Set dynamics root to self", component =>
                    {
                        var value = new SerializedObject(component);
                        value.FindProperty("rootTransform").objectReferenceValue = component.transform;
                        value.ApplyModifiedProperties();
                    });
                    serializedObject.Update();
                }
            }
        }

        private void Fields(params string[] names) { foreach (string name in names) Field(name); }
        private void Field(string name, string label = null, DynamicsSceneTools.Mode mode = DynamicsSceneTools.Mode.None,
            DynamicsSceneTools.ShapeControl shape = DynamicsSceneTools.ShapeControl.All)
        {
            var property = serializedObject.FindProperty(name);
            if (property == null || !drawn.Add(name)) return;
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                DrawList(property, label ?? property.displayName);
                return;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                string curveName = name.StartsWith("limitRotation.", StringComparison.Ordinal) ?
                    "limitRotation" + name.Substring(name.Length - 1).ToUpperInvariant() + "Curve" : name + "Curve";
                var curve = serializedObject.FindProperty(curveName);
                EditorGUI.BeginChangeCheck();
                if (property.propertyType == SerializedPropertyType.Quaternion)
                {
                    bool mixed = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    Vector3 angles = EditorGUILayout.Vector3Field(label ?? property.displayName, property.quaternionValue.eulerAngles);
                    if (EditorGUI.EndChangeCheck()) property.quaternionValue = Quaternion.Euler(angles);
                    EditorGUI.showMixedValue = mixed;
                }
                else EditorGUILayout.PropertyField(property, new GUIContent(label ?? property.displayName,
                    DynamicsSettings.instance.tooltips ? property.tooltip : ""), true);
                if (EditorGUI.EndChangeCheck() && property.propertyType == SerializedPropertyType.Float &&
                    (name == "radius" || name == "height" || name == "maxStretch")) property.floatValue = Mathf.Max(0, property.floatValue);
                if (curve != null && drawn.Add(curve.propertyPath))
                {
                    DrawCurve(curve);
                }
                bool numeric = Component is VRCPhysBoneBase && property.propertyType == SerializedPropertyType.Float && DynamicsSceneTools.CanEditProperty(name);
                if (mode != DynamicsSceneTools.Mode.None || numeric)
                    if (SceneEditButton(numeric ? tools.IsEditingProperty(name) : tools.mode == mode &&
                        (mode != DynamicsSceneTools.Mode.Shape || (tools.shapeControl & shape) != 0)))
                    {
                        sceneTarget = Component;
                        if (numeric) tools.EditProperty(name);
                        else if (mode == DynamicsSceneTools.Mode.Shape)
                        {
                            if (tools.mode != mode) { tools.mode = mode; tools.shapeControl = shape; }
                            else
                            {
                                tools.shapeControl ^= shape;
                                if (tools.shapeControl == DynamicsSceneTools.ShapeControl.None) tools.mode = DynamicsSceneTools.Mode.None;
                            }
                        }
                        else tools.mode = tools.mode == mode ? DynamicsSceneTools.Mode.None : mode;
                        SceneView.RepaintAll();
                    }
            }
        }

        private static void DrawCurve(SerializedProperty curve)
        {
            EditorGUILayout.CurveField(curve, Color.cyan, new Rect(0, 0, 1, 1), GUIContent.none, GUILayout.Width(85));
            if (!curve.hasMultipleDifferentValues && curve.animationCurveValue.length < 2)
                GUI.Label(GUILayoutUtility.GetLastRect(), "///////////////////////////////", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button(new GUIContent("X", "Clear curve"), EditorStyles.miniLabel, GUILayout.Width(14)))
                curve.animationCurveValue = new AnimationCurve();
        }

        private bool SceneEditButton(bool active)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = active && sceneTarget == Component ? new Color(0.56f, 0.94f, 0.47f) : new Color(1, 0.25f, 0.25f);
            try
            {
                var icon = new GUIContent(EditorGUIUtility.IconContent("editicon.sml")) { tooltip = "Edit through the scene view" };
                if (editButtonStyle == null) editButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(1, 1, 1, 1), margin = new RectOffset(0, 0, 2, 0)
                };
                return GUILayout.Button(icon, editButtonStyle, GUILayout.Width(18), GUILayout.Height(18));
            }
            finally { GUI.backgroundColor = previous; }
        }

        private static bool StateToggle(bool active, string label, float width)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = active ? new Color(0.56f, 0.94f, 0.47f) : new Color(1, 0.25f, 0.25f);
            try { return GUILayout.Toggle(active, label, EditorStyles.miniButton, GUILayout.Width(width)); }
            finally { GUI.backgroundColor = previous; }
        }

        private void DrawList(SerializedProperty property, string label)
        {
            if (!lists.TryGetValue(property.propertyPath, out var list))
            {
                list = new ReorderableList(serializedObject, property.Copy(), true, true, true, true);
                list.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, label);
                    if (property.name != "ignoreTransforms" && property.name != "colliders") return;
                    float right = rect.xMax;
                    if (property.name == "ignoreTransforms")
                    {
                        rect.x = right - 114;
                        rect.width = 70;
                        if (GUI.Button(rect, "Optimize", EditorStyles.miniButton))
                        {
                            serializedObject.ApplyModifiedProperties();
                            DynamicsOperations.OptimizeIgnores(Components.OfType<VRCPhysBoneBase>());
                            serializedObject.Update();
                        }
                    }
                    rect.x = right - 40;
                    rect.width = 18;
                    ListTool(rect, "Select", property.name == "ignoreTransforms" ? DynamicsSceneTools.Mode.IgnoreSelection : DynamicsSceneTools.Mode.CollisionSelection);
                    rect.x += 20;
                    ListTool(rect, "Copy", property.name == "ignoreTransforms" ? DynamicsSceneTools.Mode.IgnoreCopy : DynamicsSceneTools.Mode.CollisionCopy);
                };
                if (property.name == "ignoreTransforms" || property.name == "colliders")
                    list.drawNoneElementCallback = rect => GUI.Label(rect, "[Drag And Drop Or Click Here]",
                        new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontStyle = FontStyle.Italic });
                list.onCanAddCallback = value => property.name != "collisionTags" || value.serializedProperty.arraySize < ContactBase.MAX_COLLISION_TAGS;
                list.elementHeightCallback = index => index < 0 || index >= list.serializedProperty.arraySize ? EditorGUIUtility.singleLineHeight + 4 :
                    EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true) + 4;
                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.y += 2;
                    rect.height -= 4;
                    EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
                };
                list.onAddCallback = value =>
                {
                    var array = value.serializedProperty;
                    int index = array.arraySize++;
                    var element = array.GetArrayElementAtIndex(index);
                    if (element.propertyType == SerializedPropertyType.ObjectReference) element.objectReferenceValue = null;
                    else if (element.propertyType == SerializedPropertyType.String) element.stringValue = "";
                    value.index = index;
                };
                list.onRemoveCallback = value =>
                {
                    var array = value.serializedProperty;
                    if (value.index < 0 || value.index >= array.arraySize) return;
                    int count = array.arraySize;
                    array.DeleteArrayElementAtIndex(value.index);
                    if (array.arraySize == count) array.DeleteArrayElementAtIndex(value.index);
                    value.index = Mathf.Min(value.index, array.arraySize - 1);
                };
                lists.Add(property.propertyPath, list);
            }
            list.DoLayoutList();
            Rect area = GUILayoutUtility.GetLastRect();
            if (property.name != "ignoreTransforms" && property.name != "colliders") return;
            if (!area.Contains(Event.current.mousePosition)) return;
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                var values = DragAndDrop.objectReferences.Select(value =>
                {
                    var gameObject = value as GameObject;
                    if (property.name == "ignoreTransforms") return value as Transform ?? gameObject?.transform;
                    return (UnityEngine.Object)(value as VRCPhysBoneColliderBase ?? gameObject?.GetComponent<VRCPhysBoneColliderBase>());
                }).Where(value => value != null).Distinct().ToArray();
                if (values.Length == 0) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (Event.current.type == EventType.DragPerform)
                {
                    foreach (var value in values)
                        if (!Enumerable.Range(0, property.arraySize).Any(index => property.GetArrayElementAtIndex(index).objectReferenceValue == value))
                            property.GetArrayElementAtIndex(property.arraySize++).objectReferenceValue = value;
                    DragAndDrop.AcceptDrag();
                }
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && property.arraySize == 0 &&
                Event.current.mousePosition.y > area.y + list.headerHeight && Event.current.mousePosition.y < area.yMax - list.footerHeight)
            {
                property.arraySize = 1;
                property.GetArrayElementAtIndex(0).objectReferenceValue = null;
                Event.current.Use();
            }
        }

        private void ListTool(Rect rect, string label, DynamicsSceneTools.Mode mode)
        {
            Color previous = GUI.backgroundColor;
            bool active = tools.mode == mode && sceneTarget == Component;
            GUI.backgroundColor = active ? new Color(0.56f, 0.94f, 0.47f) : new Color(1, 0.25f, 0.25f);
            var icon = new GUIContent(EditorGUIUtility.IconContent(label == "Copy" ? "Selectable Icon" : "eyeDropper.Large"))
            {
                tooltip = label == "Copy" ? "Copy from another PhysBone" : "Select through the scene view"
            };
            EditorGUI.BeginChangeCheck();
            bool enabled;
            try { enabled = GUI.Toggle(rect, active, icon, editButtonStyle ?? EditorStyles.miniButton); }
            finally { GUI.backgroundColor = previous; }
            if (!EditorGUI.EndChangeCheck()) return;
            sceneTarget = Component;
            tools.mode = enabled ? mode : DynamicsSceneTools.Mode.None;
            SceneView.RepaintAll();
        }

        private void Section(string label, Action draw, Action header = null)
        {
            string key = "FynnsADO.Foldout." + Component.GetType().Name + "." + label;
            if (!foldouts.TryGetValue(label, out var value))
            {
                value = new AnimBool(SessionState.GetBool(key, false));
                value.valueChanged.AddListener(Repaint);
                foldouts.Add(label, value);
            }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                value.target = SessionState.GetBool(key, value.target);
                using (new EditorGUILayout.HorizontalScope())
                {
                    var style = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 14, padding = new RectOffset(6, 0, 0, 0) };
                    value.target = GUILayout.Toggle(value.target, label, style, GUILayout.Height(22));
                    header?.Invoke();
                }
                SessionState.SetBool(key, value.target);
                if (EditorGUILayout.BeginFadeGroup(DynamicsSettings.instance.animatedFoldouts ? value.faded : value.target ? 1 : 0)) draw();
                EditorGUILayout.EndFadeGroup();
            }
        }

        private void DrawAdditional()
        {
            var property = serializedObject.GetIterator();
            bool enter = true;
            var additional = new List<string>();
            var known = KnownFields(Component);
            while (property.NextVisible(enter))
            {
                enter = false;
                if (!known.Contains(property.name) && !property.name.StartsWith("foldout_", StringComparison.Ordinal) &&
                    !property.name.StartsWith("m_", StringComparison.Ordinal)) additional.Add(property.name);
            }
            if (additional.Count > 0) Section("SDK Settings", () => Fields(additional.ToArray()));
        }

        internal static HashSet<string> KnownFields(Component component)
        {
            string names = component is VRCPhysBoneBase ?
                "m_Script version rootTransform endpointPosition multiChildType ignoreOtherPhysBones ignoreTransforms integrationType pull spring stiffness gravity gravityFalloff immobileType immobile limitType maxAngleX maxAngleZ limitRotation limitRotationXCurve limitRotationYCurve limitRotationZCurve radius allowCollision collisionFilter colliders stretchMotion maxStretch maxSquish allowGrabbing grabFilter allowPosing poseFilter grabMovement snapToHand isAnimated resetWhenDisabled parameter showGizmos boneOpacity limitOpacity" :
                "m_Script rootTransform shapeType position rotation radius height size bonesAsSpheres insideBounds receiverType parameter minVelocity useFaceProximity allowSelf allowOthers localOnly collisionTags contentTypes globalCollision globalCollisionAllowSelf globalCollisionAllowOthers globalCollisionFlags";
            var result = new HashSet<string>(names.Split(' '));
            foreach (string name in result.ToArray()) result.Add(name + "Curve");
            return result;
        }

        protected virtual void OnSceneGUI()
        {
            if (DynamicsEditorWindow.OwnsScene(SceneView.currentDrawingSceneView)) return;
            if (Component == null || EditorUtility.IsPersistent(Component)) return;
            if (sceneTarget == null || !Selection.gameObjects.Contains(sceneTarget.gameObject))
                sceneTarget = Selection.activeGameObject?.GetComponents<Component>().FirstOrDefault(value =>
                    value is VRCPhysBoneBase || value is VRCPhysBoneColliderBase || value is ContactBase);
            if (DynamicsPreview.Active != null && sceneTarget != null && !DynamicsPreview.Active.ContainsCopy(sceneTarget))
                sceneTarget = DynamicsPreview.Active.CopyOf(sceneTarget) as Component ?? sceneTarget;
            if (sceneTarget != Component) return;
            if (!DynamicsSettings.instance.InspectorEnabled(Component) || sdkView)
            {
                var sdkType = DynamicsSdkEditors.TypeFor(Component);
                if (sdkType == null) return;
                CreateCachedEditor(sceneComponents.Cast<UnityEngine.Object>().ToArray(), sdkType, ref sdkEditor);
                sdkEditor?.GetType().GetMethod("OnSceneGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(sdkEditor, null);
                return;
            }
            var preview = DynamicsPreview.Active;
            if (preview != null && (!preview.ContainsCopy(Component) || !preview.IsView(SceneView.currentDrawingSceneView))) return;
            tools.Draw(Component.transform.root.gameObject, Component, sceneComponents, component => Selection.activeObject = component);
        }

        private void PreviewSceneGUI(SceneView view)
        {
            var preview = DynamicsPreview.Active;
            if (preview == null || !preview.IsView(view) || Component == null) return;
            if (preview.ContainsCopy(Component)) OnSceneGUI();
            else if (previewEditor is DynamicsComponentEditor editor && editor.Component != null)
                editor.OnSceneGUI();
        }
    }
}
#endif
