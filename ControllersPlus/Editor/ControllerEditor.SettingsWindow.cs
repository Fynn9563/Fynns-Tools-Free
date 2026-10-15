using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.Compilation;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        internal class ControllerEditorSettingsWindow : EditorWindow
        {
            internal enum NodeColor
            {
                Grey,
                Blue,
                Aqua,
                Green,
                Yellow,
                Orange,
                Red
            }

            internal static Animator m_AdvisorMapper;
            internal static bool _CallbackMapper;
            private static int indexerMapper;
            private static readonly string[] m_IssuerMapper;
            private static int _PrototypeMapper;
            private static readonly string[] _RuleMapper;
            private static readonly string[] m_SingletonMapper;
            private static SerializedObject _FactoryMapper;
            private static SerializedProperty m_Name;
            private static SerializedProperty _AccountMapper;
            private static SerializedProperty m_RefMapper;
            private static SerializedProperty m_StatusMapper;
            private static SerializedProperty _TokenMapper;
            private static SerializedProperty _CodeMapper;
            private static SerializedProperty _DicMapper;
            private static SerializedProperty invocationMapper;
            private static SerializedProperty roleMapper;
            private static SerializedProperty paramMapper;
            private static SerializedProperty modelMapper;
            private static SerializedProperty tokenizerMapper;
            private static SerializedProperty _DecoratorMapper;
            private static SerializedProperty _ComparatorMapper;
            private static SerializedProperty m_ExceptionMapper;
            private static SerializedProperty objectMapper;
            private static SerializedObject _UtilsMapper;
            private static SerializedProperty _ValMapper;
            private static SerializedProperty valueMapper;
            private static SerializedProperty _MerchantMapper;
            private static SerializedProperty m_AuthenticationMapper;
            private static SerializedProperty reponseMapper;
            private static SerializedProperty m_PoolMapper;
            private static SerializedProperty _ParameterMapper;
            private static SerializedProperty _ComposerMapper;
            private static SerializedProperty repositoryMapper;
            private static SerializedProperty _MappingMapper;
            private static bool _BaseMapper;
            private static Vector2 containerMapper;
            private static bool _ClassMapper;
            private static bool mockMapper;
            private static bool instanceMapper;
            private static bool m_FieldMapper;
            private static bool _AttributeMapper;
            private static bool _ClientMapper;
            private static bool configMapper;
            private static bool m_DescriptorMapper;
            private static bool templateMapper;
            private static bool m_MessageMapper;
            private static bool collectionMapper;
            private static bool _ParserMapper;
            [SpecialName]
            internal static bool PushTests()
            {
                return EditorGUIUtility.isProSkin;
            }

            [MenuItem("Tools/Fynn's Tools/Controllers+ Settings", false, 14)]
            internal static void CalcTests()
            {
                EditorWindow.GetWindow<ControllerEditorSettingsWindow>(utility: false, "Controllers+ Settings", focus: true);
            }

            private void OnGUI()
            {
                if (!OrderVisitor(this))
                {
                    return;
                }

                containerMapper = EditorGUILayout.BeginScrollView(containerMapper);
                indexerMapper = GUILayout.Toolbar(indexerMapper, m_IssuerMapper, "toolbarbutton");
                switch (indexerMapper)
                {
                    case 0:
                        IncludeTests();
                        break;
                    case 1:
                        RunTests();
                        break;
                    case 2:
                        ControllerEditorExtraSettings.Draw();
                        break;
                }

                ControllerEditorUtility.MapQueue();
                RevertAnnotation();
                DefineVisitor();
                EditorGUILayout.EndScrollView();
            }

            private static void IncludeTests()
            {
                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(GUI.skin.box);
                try
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        _ClassMapper = EditorGUILayout.Foldout(_ClassMapper, "Animation Window");
                        GUILayout.FlexibleSpace();
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, AddInitializer().aw_active, Color.green, Color.grey);
                        try
                        {
                            AddInitializer().aw_active.ExcludeDefinition(ControllerEditorUtility.InvokeQueue(AddInitializer().aw_active, AddInitializer().aw_active ? "Enabled" : "Disabled"));
                        }
                        finally
                        {
                            if (templateThread != null)
                            {
                                ((IDisposable)templateThread).Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if (horizontalScope != null)
                        {
                            ((IDisposable)horizontalScope).Dispose();
                        }
                    }

                    if (_ClassMapper)
                    {
                        using (new EditorGUI.DisabledScope(!AddInitializer().aw_active))
                        {
                            IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                            try
                            {
                                horizontalScope = new GUILayout.HorizontalScope();
                                try
                                {
                                    EditorGUI.BeginChangeCheck();
                                    AddInitializer().aw_enableOverride.QueryDefinition(new GUIContent("Overriding", "Allows you to explicitly set the controller for selecting clips, and explicitly set the root to change what the paths are relative to."), null);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        TestInitializer(null);
                                        _AlgoVisitor = null;
                                        m_VisitorVisitor = false;
                                    }

                                    AddInitializer().aw_enablePropertyEditing.QueryDefinition(new GUIContent("Edit Property", "Allows you to drag and drop objects to properties and to edit the properties of the curves with right-click context menu."), null);
                                }
                                finally
                                {
                                    if (horizontalScope != null)
                                    {
                                        ((IDisposable)horizontalScope).Dispose();
                                    }
                                }

                                horizontalScope = new GUILayout.HorizontalScope();
                                try
                                {
                                    AddInitializer().aw_enableGameObjectDND.QueryDefinition(new GUIContent("Drag & Drop", "Allows you to drag and drop GameObjects to the animation window to add them as a new curve."), null);
                                    AddInitializer().aw_autoSwitchClip.QueryDefinition(new GUIContent("Auto-Switch Clip", "Automatically switch the clip in the animation window when selecting a state."), null);
                                }
                                finally
                                {
                                    if (horizontalScope != null)
                                    {
                                        ((IDisposable)horizontalScope).Dispose();
                                    }
                                }

                                horizontalScope = new GUILayout.HorizontalScope();
                                try
                                {
                                    AddInitializer().aw_warnPropertyMerge.QueryDefinition(new GUIContent("Property Merge Log", "Warn in the console when merging properties through property modification."), null);
                                }
                                finally
                                {
                                    if (horizontalScope != null)
                                    {
                                        ((IDisposable)horizontalScope).Dispose();
                                    }
                                }
                            }
                            finally
                            {
                                if (schemaThread != null)
                                {
                                    ((IDisposable)schemaThread).Dispose();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }

                verticalScope = new GUILayout.VerticalScope(GUI.skin.box);
                try
                {
                    FoldoutScope publisherThread = new FoldoutScope(ref mockMapper, "Animator Window");
                    try
                    {
                        if (!mockMapper)
                        {
                            return;
                        }

                        GUILayout.VerticalScope verticalScope2 = new GUILayout.VerticalScope(GUI.skin.box);
                        try
                        {
                            FoldoutScope publisherThread2 = new FoldoutScope(ref instanceMapper, "Layers");
                            try
                            {
                                if (instanceMapper)
                                {
                                    AddInitializer().categoryBaseName.ViewDefinition("Uncategorized Name".CreateResolver("Name of the category for layers without a category."), true, true, null);
                                    AddInitializer().categoryDelimiter.ViewDefinition("Category Delimiter".CreateResolver("The character used to separate categories in the layer view."), true, true, null);
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        AddInitializer().displayCategoryView.QueryDefinition("Category View".CreateResolver("Displays options to view layers in categories."), null);
                                        AddInitializer().displayLayerCompactView.QueryDefinition("Compact View".CreateResolver("Displays a button to view layers in a compact manner."), null);
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        AddInitializer().displayLayerIndex.QueryDefinition(new GUIContent("Layer Index", "Shows a small number on the layer's GUI for the layer's index in the list of layers."), null);
                                        AddInitializer().autoFrameLayer.QueryDefinition(new GUIContent("Auto-Frame Layer", "Upon selecting a layer, automatically frame the statemachine. Behaviour is similar to pressing 'A' after clicking the graph."), null);
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (publisherThread2 != null)
                                {
                                    ((IDisposable)publisherThread2).Dispose();
                                }
                            }
                        }
                        finally
                        {
                            if (verticalScope2 != null)
                            {
                                ((IDisposable)verticalScope2).Dispose();
                            }
                        }

                        verticalScope2 = new GUILayout.VerticalScope(GUI.skin.box);
                        try
                        {
                            FoldoutScope publisherThread2 = new FoldoutScope(ref m_FieldMapper, "Parameters");
                            try
                            {
                                if (m_FieldMapper)
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        _AttributeMapper = EditorGUILayout.Foldout(_AttributeMapper, "Type Indicator");
                                        GUILayout.FlexibleSpace();
                                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, AddInitializer().displayParameterType, Color.green, Color.grey);
                                        try
                                        {
                                            AddInitializer().displayParameterType.ExcludeDefinition(ControllerEditorUtility.InvokeQueue(AddInitializer().displayParameterType, AddInitializer().displayParameterType ? "Enabled" : "Disabled"));
                                        }
                                        finally
                                        {
                                            if (templateThread != null)
                                            {
                                                ((IDisposable)templateThread).Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!AddInitializer().displayParameterType))
                                    {
                                        if (_AttributeMapper)
                                        {
                                            IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                                            try
                                            {
                                                AddInitializer().capitalParameterIndicator.QueryDefinition(new GUIContent("Capital Letters", "Changes 'f' to 'F' and 'i' to 'I'"), null);
                                                AddInitializer().parameterLabelFontStyle.ValidateDefinition<FontStyle>(new GUIContent("Font style", "The font style of the parameter indicators."), ismap: false, null, Array.Empty<GUILayoutOption>());
                                                AddInitializer().parameterLabelColor.FillDefinition(new GUIContent("Font Color", "The color of the parameter indicators. Supports Alpha."), true);
                                            }
                                            finally
                                            {
                                                if (schemaThread != null)
                                                {
                                                    ((IDisposable)schemaThread).Dispose();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (publisherThread2 != null)
                                {
                                    ((IDisposable)publisherThread2).Dispose();
                                }
                            }
                        }
                        finally
                        {
                            if (verticalScope2 != null)
                            {
                                ((IDisposable)verticalScope2).Dispose();
                            }
                        }

                        verticalScope2 = new GUILayout.VerticalScope(GUI.skin.box);
                        try
                        {
                            FoldoutScope publisherThread2 = new FoldoutScope(ref configMapper, "Transitions");
                            try
                            {
                                if (configMapper)
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        GUILayout.VerticalScope verticalScope3 = new GUILayout.VerticalScope();
                                        try
                                        {
                                            AddInitializer().autoReverseModes.QueryDefinition(new GUIContent("Auto Reverse Mode", "Reverse Transitions should also reverse the condition modes"), null);
                                            AddInitializer().animateInboundEdges.QueryDefinition("Animate In Transitions".CreateResolver("Incoming transitions to selected states get animated."), null);
                                        }
                                        finally
                                        {
                                            if (verticalScope3 != null)
                                            {
                                                ((IDisposable)verticalScope3).Dispose();
                                            }
                                        }

                                        verticalScope3 = new GUILayout.VerticalScope();
                                        try
                                        {
                                            AddInitializer().reverseModifiesValues.QueryDefinition(new GUIContent("Reverse Adjusts Values", "Reversing a condition will also modify its values appropriately. Hold CTRL to temporarily flip this setting while reversing"), null);
                                            AddInitializer().animateOutboundEdges.QueryDefinition("Animate Out Transitions".CreateResolver("Outgoing transitions from selected states get animated."), null);
                                        }
                                        finally
                                        {
                                            if (verticalScope3 != null)
                                            {
                                                ((IDisposable)verticalScope3).Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    AddInitializer().arrowLerpRatio.RemoveDefinition("Arrow Location".CreateResolver("Where the arrow exists on transitions."), -1f, 1f, true);
                                }
                            }
                            finally
                            {
                                if (publisherThread2 != null)
                                {
                                    ((IDisposable)publisherThread2).Dispose();
                                }
                            }
                        }
                        finally
                        {
                            if (verticalScope2 != null)
                            {
                                ((IDisposable)verticalScope2).Dispose();
                            }
                        }

                        verticalScope2 = new GUILayout.VerticalScope(GUI.skin.box);
                        try
                        {
                            FoldoutScope publisherThread2 = new FoldoutScope(ref _ClientMapper, "Nodes");
                            try
                            {
                                if (_ClientMapper)
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        AddInitializer().switchDoubleClick.QueryDefinition(new GUIContent("Alternate Double Click", "Switch Double click's behaviour on states. Ctrl Double Click will do the other behaviour"), null);
                                        AddInitializer().stateCosmetics.CustomizeDefinition<ControllerEditorSettings.StateCosmeticOptions>("State Extras", hasreg: true, null, Array.Empty<GUILayoutOption>());
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (publisherThread2 != null)
                                {
                                    ((IDisposable)publisherThread2).Dispose();
                                }
                            }
                        }
                        finally
                        {
                            if (verticalScope2 != null)
                            {
                                ((IDisposable)verticalScope2).Dispose();
                            }
                        }

                        verticalScope2 = new GUILayout.VerticalScope(GUI.skin.box);
                        try
                        {
                            FoldoutScope publisherThread2 = new FoldoutScope(ref collectionMapper, "Colors");
                            try
                            {
                                if (!collectionMapper)
                                {
                                    return;
                                }

                                GUILayout.VerticalScope verticalScope3 = new GUILayout.VerticalScope(GUI.skin.box);
                                try
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        _ParserMapper = EditorGUILayout.Foldout(_ParserMapper, "Transition Colors");
                                        GUILayout.FlexibleSpace();
                                        bool flag = AddInitializer().cosmeticTransitionsActive.FindDefinition();
                                        string map = (flag ? "Enabled" : "Disabled");
                                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, flag, Color.green, Color.grey);
                                        try
                                        {
                                            AddInitializer().cosmeticTransitionsActive.ExcludeDefinition(ControllerEditorUtility.InvokeQueue(flag, map));
                                        }
                                        finally
                                        {
                                            if (templateThread != null)
                                            {
                                                ((IDisposable)templateThread).Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!AddInitializer().cosmeticTransitionsActive))
                                    {
                                        if (_ParserMapper)
                                        {
                                            IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                                            try
                                            {
                                                AddInitializer().normalTransitionColor.VerifyDefinition("Normal Transition", true);
                                                AddInitializer().entryTransitionColor.VerifyDefinition("Entry Transition", true);
                                                AddInitializer().selectedTransitionColor.VerifyDefinition("Selected Transition", true);
                                                AddInitializer().baseTransitionColor.VerifyDefinition("Base Transition", true);
                                            }
                                            finally
                                            {
                                                if (schemaThread != null)
                                                {
                                                    ((IDisposable)schemaThread).Dispose();
                                                }
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    if (verticalScope3 != null)
                                    {
                                        ((IDisposable)verticalScope3).Dispose();
                                    }
                                }

                                verticalScope3 = new GUILayout.VerticalScope(GUI.skin.box);
                                try
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        m_DescriptorMapper = EditorGUILayout.Foldout(m_DescriptorMapper, "Graph Colors");
                                        GUILayout.FlexibleSpace();
                                        bool flag2 = AddInitializer().cosmeticGraphActive.FindDefinition();
                                        string map2 = (flag2 ? "Enabled" : "Disabled");
                                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, flag2, Color.green, Color.grey);
                                        try
                                        {
                                            ControllerEditorSettings.WorkerAlgo workerAlgo = new ControllerEditorSettings.WorkerAlgo(SortAlgo);
                                            try
                                            {
                                                AddInitializer().cosmeticGraphActive.ExcludeDefinition(ControllerEditorUtility.InvokeQueue(flag2, map2));
                                            }
                                            finally
                                            {
                                                if (workerAlgo != null)
                                                {
                                                    ((IDisposable)workerAlgo).Dispose();
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (templateThread != null)
                                            {
                                                ((IDisposable)templateThread).Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!AddInitializer().cosmeticGraphActive))
                                    {
                                        if (m_DescriptorMapper)
                                        {
                                            IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                                            try
                                            {
                                                horizontalScope = new GUILayout.HorizontalScope();
                                                try
                                                {
                                                    if (!AddInitializer().graphBackgroundIsTexture)
                                                    {
                                                        AddInitializer().gridBackgroundColor.VerifyDefinition("Background", false);
                                                    }
                                                    else
                                                    {
                                                        AddInitializer().graphBackgroundTexture.CheckDefinition("Background", false, GUILayout.Height(17f), GUILayout.ExpandWidth(expand: true));
                                                    }

                                                    AddInitializer().graphBackgroundIsTexture.ExcludeDefinition(ControllerEditorUtility.ExcludeQueue(AddInitializer().graphBackgroundIsTexture, new GUIContent("T", "Use Texture"), GUI.skin.button, GUILayout.Width(18f), GUILayout.Height(18f)));
                                                    if (ControllerEditorUtility.CallQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor))
                                                    {
                                                        if ((bool)AddInitializer().graphBackgroundIsTexture)
                                                        {
                                                            AddInitializer().graphBackgroundTexture.Reset();
                                                        }
                                                        else
                                                        {
                                                            AddInitializer().gridBackgroundColor.Reset();
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    if (horizontalScope != null)
                                                    {
                                                        ((IDisposable)horizontalScope).Dispose();
                                                    }
                                                }

                                                if (PushTests())
                                                {
                                                    AddInitializer().gridMinorDarkColor.VerifyDefinition("Minor Line", true);
                                                    AddInitializer().gridMajorDarkColor.VerifyDefinition("Major Line", true);
                                                }
                                                else
                                                {
                                                    AddInitializer().gridMinorLightColor.VerifyDefinition("Minor Line", true);
                                                    AddInitializer().gridMajorLightColor.VerifyDefinition("Major Line", true);
                                                }
                                            }
                                            finally
                                            {
                                                if (schemaThread != null)
                                                {
                                                    ((IDisposable)schemaThread).Dispose();
                                                }
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    if (verticalScope3 != null)
                                    {
                                        ((IDisposable)verticalScope3).Dispose();
                                    }
                                }

                                verticalScope3 = new GUILayout.VerticalScope(GUI.skin.box);
                                try
                                {
                                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                                    try
                                    {
                                        templateMapper = EditorGUILayout.Foldout(templateMapper, "Node Colors");
                                        GUILayout.FlexibleSpace();
                                        bool flag3 = AddInitializer().cosmeticNodesActive.FindDefinition();
                                        string map3 = (flag3 ? "Enabled" : "Disabled");
                                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, flag3, Color.green, Color.grey);
                                        try
                                        {
                                            AddInitializer().cosmeticNodesActive.ExcludeDefinition(ControllerEditorUtility.InvokeQueue(flag3, map3));
                                        }
                                        finally
                                        {
                                            if (templateThread != null)
                                            {
                                                ((IDisposable)templateThread).Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (horizontalScope != null)
                                        {
                                            ((IDisposable)horizontalScope).Dispose();
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!AddInitializer().cosmeticNodesActive))
                                    {
                                        if (!templateMapper)
                                        {
                                            return;
                                        }

                                        IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                                        try
                                        {
                                            NewTests(AddInitializer().normalStateNodeColor, "State Node");
                                            NewTests(AddInitializer().machineStateNodeColor, "Machine Node");
                                            NewTests(AddInitializer().defaultStateNodeColor, "Default Node");
                                            NewTests(AddInitializer().anyStateNodeColor, "AnyState Node");
                                            NewTests(AddInitializer().entryStateNodeColor, "Entry Node");
                                            NewTests(AddInitializer().exitStateNodeColor, "Exit Node");
                                        }
                                        finally
                                        {
                                            if (schemaThread != null)
                                            {
                                                ((IDisposable)schemaThread).Dispose();
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    if (verticalScope3 != null)
                                    {
                                        ((IDisposable)verticalScope3).Dispose();
                                    }
                                }
                            }
                            finally
                            {
                                if (publisherThread2 != null)
                                {
                                    ((IDisposable)publisherThread2).Dispose();
                                }
                            }
                        }
                        finally
                        {
                            if (verticalScope2 != null)
                            {
                                ((IDisposable)verticalScope2).Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if (publisherThread != null)
                        {
                            ((IDisposable)publisherThread).Dispose();
                        }
                    }
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }
            }

            private static void RunTests()
            {
                _PrototypeMapper = GUILayout.Toolbar(_PrototypeMapper, _RuleMapper, "toolbarbutton");
                ControllerEditorUtility.MapQueue();
                int prototypeMapper = _PrototypeMapper;
                switch (prototypeMapper)
                {
                    case 0:
                        CloneTests();
                        break;
                    case 2:
                        ReflectTests();
                        break;
                    case 1:
                        LoginTests();
                        break;
                }
            }

            private static void CloneTests()
            {
                _UtilsMapper.Update();
                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    GUILayout.FlexibleSpace();
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_BridgeProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        if (_ObserverAnnotation == null)
                        {
                            _ObserverAnnotation = new AnimatorStateTransition();
                        }

                        CustomizeAlgo(AddInitializer().defaultTransition, _ObserverAnnotation);
                    }

                    using (new EditorGUI.DisabledScope(!_ObserverAnnotation))
                    {
                        if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_StrategyProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            Undo.RecordObject(AddInitializer().defaultTransition, "PasteSettings");
                            CustomizeAlgo(_ObserverAnnotation, AddInitializer().defaultTransition);
                        }
                    }

                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_CustomerProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        if (EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                        {
                            AddInitializer().defaultTransition = new AnimatorStateTransition();
                            DeleteTests();
                            ControllerEditorSettings.SetupDefinition();
                        }
                    }
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.PropertyField(m_PoolMapper);
                    using (new EditorGUI.DisabledScope(!m_PoolMapper.boolValue))
                    {
                        EditorGUILayout.PropertyField(reponseMapper);
                    }
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.PropertyField(_ParameterMapper);
                    EditorGUILayout.PropertyField(_MerchantMapper);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                EditorGUILayout.PropertyField(m_AuthenticationMapper);
                EditorGUILayout.PropertyField(_ComposerMapper);
                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.PropertyField(repositoryMapper);
                    EditorGUILayout.PropertyField(valueMapper);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.PropertyField(_MappingMapper);
                    EditorGUILayout.PropertyField(_ValMapper);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                bool hasModifiedProperties = _UtilsMapper.hasModifiedProperties;
                _UtilsMapper.ApplyModifiedProperties();
                if (hasModifiedProperties)
                {
                    ControllerEditorSettings.SetupDefinition();
                }
            }

            private static void LoginTests()
            {
                _FactoryMapper.Update();
                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    GUILayout.Label(ControllerEditorUtility.DestroyError()._DatabaseProcessor, GUILayout.Width(35f), GUILayout.Height(35f));
                    GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope();
                    try
                    {
                        EditorGUILayout.PropertyField(m_Name, new GUIContent(string.Empty));
                        GUILayout.HorizontalScope horizontalScope2 = new GUILayout.HorizontalScope();
                        try
                        {
                            EditorGUIUtility.labelWidth = 35f;
                            EditorGUILayout.PropertyField(tokenizerMapper);
                            EditorGUIUtility.labelWidth = 0f;
                            if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_CustomerProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)) && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                            {
                                AddInitializer().defaultState = new AnimatorState{name = "New State"};
                                CreateTests();
                                ControllerEditorSettings.SetupDefinition();
                            }
                        }
                        finally
                        {
                            if (horizontalScope2 != null)
                            {
                                ((IDisposable)horizontalScope2).Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if (verticalScope != null)
                        {
                            ((IDisposable)verticalScope).Dispose();
                        }
                    }
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(modelMapper);
                EditorGUILayout.PropertyField(_AccountMapper);
                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUI.indentLevel++;
                    using (new EditorGUI.DisabledScope(!_DicMapper.boolValue))
                    {
                        _DecoratorMapper.stringValue = EditorGUILayout.TextField("Multiplier", _DecoratorMapper.stringValue, "textfielddropdowntext");
                    }

                    EditorGUI.indentLevel--;
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
                    }

                    _DicMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", _DicMapper.boolValue, GUILayout.Width(90f));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    if (!paramMapper.boolValue)
                    {
                        GUILayout.Label("Normalized Time");
                    }
                    else
                    {
                        objectMapper.stringValue = EditorGUILayout.TextField("Normalized Time", objectMapper.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(disabled: true))
                        {
                            EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }

                    paramMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", paramMapper.boolValue, GUILayout.Width(90f));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    if (!invocationMapper.boolValue)
                    {
                        EditorGUILayout.PropertyField(_CodeMapper);
                    }
                    else
                    {
                        _ComparatorMapper.stringValue = EditorGUILayout.TextField("Mirror", _ComparatorMapper.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(disabled: true))
                        {
                            EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }

                    invocationMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", invocationMapper.boolValue, GUILayout.Width(90f));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    if (!roleMapper.boolValue)
                    {
                        m_RefMapper.floatValue = EditorGUILayout.Slider("Cycle Offset", m_RefMapper.floatValue, 0f, 1f);
                    }
                    else
                    {
                        m_ExceptionMapper.stringValue = EditorGUILayout.TextField("Cycle Offset", m_ExceptionMapper.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(disabled: true))
                        {
                            EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }

                    roleMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", roleMapper.boolValue, GUILayout.Width(90f));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.PropertyField(_TokenMapper, new GUIContent("Write Defaults"));
                    EditorGUILayout.PropertyField(m_StatusMapper, new GUIContent("Foot IK"));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                bool hasModifiedProperties = _FactoryMapper.hasModifiedProperties;
                _FactoryMapper.ApplyModifiedProperties();
                if (hasModifiedProperties)
                {
                    ControllerEditorSettings.SetupDefinition();
                }
            }

            private static void ReflectTests()
            {
                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
                try
                {
                    m_AdvisorMapper = m_AdvisorMapper.CountPredicate(new GUIContent("Targeted Animator", "The Animator that should be targeted by default when building Masks"), true);
                    _CallbackMapper = ControllerEditorUtility.FindQueue(_CallbackMapper, new GUIContent("Always Use"), GUILayout.Width(85f));
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(GUI.skin.box);
                try
                {
                    m_MessageMapper = EditorGUILayout.Foldout(m_MessageMapper, "Default Layer Options");
                    if (m_MessageMapper)
                    {
                        IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                        try
                        {
                            AddInitializer().defaultLayerWeight.FlushDefinition(EditorGUILayout.Slider("Default Layer Weight", AddInitializer().defaultLayerWeight, 0f, 1f));
                            AddInitializer().defaultLayerMask.CheckDefinition("Default Layer Mask", false);
                            EditorGUILayout.HorizontalScope horizontalScope2 = new EditorGUILayout.HorizontalScope(GUI.skin.box);
                            try
                            {
                                AddInitializer().defaultEntryPosition.CloneDefinition("Entry Position");
                            }
                            finally
                            {
                                if (horizontalScope2 != null)
                                {
                                    ((IDisposable)horizontalScope2).Dispose();
                                }
                            }

                            horizontalScope2 = new EditorGUILayout.HorizontalScope(GUI.skin.box);
                            try
                            {
                                AddInitializer().defaultAnyPosition.CloneDefinition("AnyState Position");
                            }
                            finally
                            {
                                if (horizontalScope2 != null)
                                {
                                    ((IDisposable)horizontalScope2).Dispose();
                                }
                            }

                            horizontalScope2 = new EditorGUILayout.HorizontalScope(GUI.skin.box);
                            try
                            {
                                AddInitializer().defaultExitPosition.CloneDefinition("Exit Position");
                            }
                            finally
                            {
                                if (horizontalScope2 != null)
                                {
                                    ((IDisposable)horizontalScope2).Dispose();
                                }
                            }

                            using (new EditorGUI.DisabledScope(!RevertMapper()))
                            {
                                if (ControllerEditorUtility.DisableQueue("Sample From Active StateMachine"))
                                {
                                    AddInitializer().defaultEntryPosition.CreateDefinition(RevertMapper().entryPosition);
                                    AddInitializer().defaultAnyPosition.CreateDefinition(RevertMapper().anyStatePosition);
                                    AddInitializer().defaultExitPosition.CreateDefinition(RevertMapper().exitPosition);
                                }
                            }
                        }
                        finally
                        {
                            if (schemaThread != null)
                            {
                                ((IDisposable)schemaThread).Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }

                string value = ControllerEditorUtility.EnableRules(AddInitializer().saveFolder, "Generated Assets Path");
                if (!string.IsNullOrEmpty(value))
                {
                    AddInitializer().saveFolder.ResolveDefinition(value);
                }
            }

            private void OnEnable()
            {
                titleContent = new GUIContent("Controllers+ Settings");
                while (!_BaseMapper)
                {
                    int num = 4;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 2:
                                _BaseMapper = true;
                                num = 3;
                                continue;
                            case 0:
                                return;
                            case 3:
                                return;
                            case 5:
                                break;
                            case 6:
                                DeleteTests();
                                num = 1;
                                continue;
                            case 1:
                                CreateTests();
                                num = 2;
                                continue;
                            case 4:
                                goto case 6;
                        }

                        break;
                    }
                }
            }

            internal static void DeleteTests()
            {
                while (true)
                {
                    int num;
                    if (!(AddInitializer().defaultTransition == null))
                    {
                        num = 12;
                        goto IL_001d;
                    }

                    goto IL_01d3;
                    IL_001d:
                        while (true)
                        {
                            switch (num)
                            {
                                case 16:
                                    reponseMapper = _UtilsMapper.FindProperty("m_ExitTime");
                                    num = 8;
                                    continue;
                                case 13:
                                    break;
                                case 10:
                                    return;
                                case 6:
                                    goto IL_00d6;
                                default:
                                    valueMapper = _UtilsMapper.FindProperty("m_Mute");
                                    num = 4;
                                    continue;
                                case 2:
                                case 12:
                                    _UtilsMapper = new SerializedObject(AddInitializer().defaultTransition);
                                    num = 14;
                                    continue;
                                case 4:
                                    _MerchantMapper = _UtilsMapper.FindProperty("m_TransitionDuration");
                                    num = 5;
                                    continue;
                                case 9:
                                    _ComposerMapper = _UtilsMapper.FindProperty("m_InterruptionSource");
                                    num = 7;
                                    continue;
                                case 5:
                                    m_AuthenticationMapper = _UtilsMapper.FindProperty("m_TransitionOffset");
                                    num = 16;
                                    continue;
                                case 8:
                                    m_PoolMapper = _UtilsMapper.FindProperty("m_HasExitTime");
                                    num = 3;
                                    continue;
                                case 11:
                                    goto IL_01d3;
                                case 14:
                                    _ValMapper = _UtilsMapper.FindProperty("m_Solo");
                                    num = 0;
                                    continue;
                                case 7:
                                    repositoryMapper = _UtilsMapper.FindProperty("m_OrderedInterruption");
                                    num = 1;
                                    continue;
                                case 3:
                                    _ParameterMapper = _UtilsMapper.FindProperty("m_HasFixedDuration");
                                    num = 9;
                                    continue;
                                case 1:
                                case 15:
                                    _MappingMapper = _UtilsMapper.FindProperty("m_CanTransitionToSelf");
                                    num = 10;
                                    continue;
                            }

                            break;
                        }

                    continue;
                    IL_01d3:
                        ;
                    goto IL_00d6;
                    IL_00d6:
                        AddInitializer().defaultTransition = new AnimatorStateTransition();
                    num = 2;
                    goto IL_001d;
                }
            }

            internal static void CreateTests()
            {
                if (AddInitializer().defaultState == null)
                {
                    AddInitializer().defaultState = new AnimatorState{name = "New State"};
                }

                _FactoryMapper = new SerializedObject(AddInitializer().defaultState);
                m_Name = _FactoryMapper.FindProperty("m_Name");
                if (m_Name != null && (bool)AddInitializer().requiresStateRename)
                {
                    m_Name.stringValue = "New State";
                    AddInitializer().requiresStateRename.ExcludeDefinition(excludeparam: false);
                    _FactoryMapper.ApplyModifiedPropertiesWithoutUndo();
                }

                _AccountMapper = _FactoryMapper.FindProperty("m_Speed");
                m_RefMapper = _FactoryMapper.FindProperty("m_CycleOffset");
                m_StatusMapper = _FactoryMapper.FindProperty("m_IKOnFeet");
                _TokenMapper = _FactoryMapper.FindProperty("m_WriteDefaultValues");
                _CodeMapper = _FactoryMapper.FindProperty("m_Mirror");
                _DicMapper = _FactoryMapper.FindProperty("m_SpeedParameterActive");
                invocationMapper = _FactoryMapper.FindProperty("m_MirrorParameterActive");
                roleMapper = _FactoryMapper.FindProperty("m_CycleOffsetParameterActive");
                paramMapper = _FactoryMapper.FindProperty("m_TimeParameterActive");
                modelMapper = _FactoryMapper.FindProperty("m_Motion");
                tokenizerMapper = _FactoryMapper.FindProperty("m_Tag");
                _DecoratorMapper = _FactoryMapper.FindProperty("m_SpeedParameter");
                _ComparatorMapper = _FactoryMapper.FindProperty("m_MirrorParameter");
                m_ExceptionMapper = _FactoryMapper.FindProperty("m_CycleOffsetParameter");
                objectMapper = _FactoryMapper.FindProperty("m_TimeParameter");
            }

            static ControllerEditorSettingsWindow()
            {
                while (true)
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 5:
                                m_SingletonMapper = Array.Empty<string>();
                                num = 0;
                                continue;
                            case 2:
                                break;
                            case 4:
                                return;
                            case 1:
                                m_IssuerMapper = new string[3]{"Behaviours & Cosmetics", "Defaults", "Additional Features"};
                                num = 6;
                                continue;
                            case 6:
                                _RuleMapper = new string[3]{"Transition", "State", "Other"};
                                num = 3;
                                continue;
                            case 3:
                                goto case 5;
                        }

                        break;
                    }
                }
            }

            [CompilerGenerated]
            internal static void NewTests(ControllerEditorSettings.FloatSetting config, string attr)
            {
                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    config.FlushDefinition((float)(NodeColor)(object)EditorGUILayout.EnumPopup(attr, (NodeColor)config.ResetDefinition()));
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 3:
                                if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor, ControllerEditorUtility.CalcError().m_ServiceProcessor, GUILayout.Width(18f), GUILayout.Height(18f)))
                                {
                                    num = 2;
                                    continue;
                                }

                                return;
                            case 1:
                                return;
                            case 4:
                                break;
                            case 2:
                                break;
                        }

                        config.Reset();
                        num = 0;
                    }
                }
                finally
                {
                    int num2;
                    if (horizontalScope == null)
                    {
                        num2 = 3;
                        goto IL_0178;
                    }

                    goto IL_0208;
                    IL_01d4:
                        ((IDisposable)horizontalScope).Dispose();
                    num2 = 2;
                    goto IL_0178;
                    IL_0178:
                        switch (num2)
                        {
                            case 4:
                                break;
                            case 3:
                                break;
                            case 1:
                                goto IL_01d4;
                            case 2:
                                break;
                            default:
                                goto IL_0208;
                        }

                    goto end_IL_0163;
                    IL_0208:
                        ;
                    goto IL_01d4;
                    end_IL_0163:
                        ;
                }
            }
        }
    }
}
