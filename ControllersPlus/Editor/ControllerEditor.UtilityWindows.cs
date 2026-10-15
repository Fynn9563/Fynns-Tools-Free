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
        internal class MotionRenameWindow : EditorWindow
        {
            public List<Motion> _ManagerMapper = new List<Motion>();
            public string _ItemMapper = "";
            private bool _SpecificationMapper = true;
            public void OnGUI()
            {
                bool flag = false;
                string text = "Rename Field";
                GUI.SetNextControlName(text);
                _ItemMapper = EditorGUILayout.TextField(_ItemMapper);
                if (_SpecificationMapper)
                {
                    _SpecificationMapper = false;
                    GUI.FocusControl(text);
                }

                Event current = Event.current;
                if (current.isKey && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == text)
                {
                    flag = true;
                    current.Use();
                }

                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        Close();
                    }

                    flag |= GUILayout.Button("Ok");
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                if (!flag)
                {
                    return;
                }

                UnityEngine.Object[] objectsToUndo;
                UnityEngine.Object[] array = (objectsToUndo = _ManagerMapper.Where((Motion m) => m != null).Distinct().ToArray());
                Undo.RecordObjects(objectsToUndo, "Rename motion");
                StringBuilder stringBuilder = new StringBuilder();
                Motion[] array2 = (Motion[])array;
                foreach (Motion motion in array2)
                {
                    if (MotionAssetOperations.ResetReg(motion))
                    {
                        motion.name = _ItemMapper;
                    }
                    else if (motion.name != _ItemMapper)
                    {
                        string text2 = motion.name;
                        string assetPath = AssetDatabase.GetAssetPath(motion);
                        string text3 = MotionAssetOperations.FlushReg(assetPath, _ItemMapper);
                        if (_ItemMapper != text3)
                        {
                            stringBuilder.AppendLine(text2 + " -> " + text3);
                        }

                        AssetDatabase.RenameAsset(assetPath, text3);
                    }

                    EditorUtility.SetDirty(motion);
                    MotionAssetOperations.InstantiateReg();
                }

                Close();
                if (stringBuilder.Length > 0)
                {
                    EditorUtility.DisplayDialog("Motion Rename", $"The following clips are not embedded and have been renamed accordingly:\n{stringBuilder}", "Ok");
                }
            }

            public void OnLostFocus()
            {
                Close();
            }
        }

        internal class ParameterRenameWindow : CustomUtilityWindow<ParameterRenameWindow>
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass10_0
            {
                public bool producerMapper;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass11_0
            {
                public int publisherMapper;
                public ParameterRenameWindow m_ConfigurationMapper;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass11_1
            {
                public string m_ProcMapper;
                internal bool PrepareTests(UnityEngine.AnimatorControllerParameter p2)
                {
                    return p2.name != m_ProcMapper;
                }
            }

            private static readonly string[] m_BroadcasterMapper = new string[3]{"No Change", "Force On", "Force Off"};
            internal UnityEditor.Animations.AnimatorController m_ProxyMapper;
            internal UnityEditor.Animations.AnimatorController _StructMapper;
            internal bool serviceMapper = true;
            private int stateMapper;
            internal (UnityEngine.AnimatorControllerParameter, string)[] globalMapper;
            internal override string title
            {
                get
                {
                    return "Parameter Rename";
                }
            }

            internal static ParameterRenameWindow ResolveTests(UnityEditor.Animations.AnimatorController def, UnityEditor.Animations.AnimatorController vis, bool acceptutil)
            {
                ParameterRenameWindow parameterRenameWindow = CustomUtilityWindow<ParameterRenameWindow>.CloneHelper();
                parameterRenameWindow.m_ProxyMapper = def;
                parameterRenameWindow._StructMapper = vis;
                parameterRenameWindow.serviceMapper = acceptutil;
                UnityEngine.AnimatorControllerParameter[] array = def.parameters.Where((UnityEngine.AnimatorControllerParameter p) => !ControllerEditorUtility.m_AdapterProcessor.Contains(p.name)).ToArray();
                int num = array.Length;
                parameterRenameWindow.globalMapper = new (UnityEngine.AnimatorControllerParameter, string)[num];
                for (int num2 = 0; num2 < num; num2++)
                {
                    parameterRenameWindow.globalMapper[num2] = (array[num2], array[num2].name);
                }

                if (acceptutil)
                {
                    for (int num3 = 0; num3 < num; num3++)
                    {
                        parameterRenameWindow.globalMapper[num3].Item2 = parameterRenameWindow.ListTests(parameterRenameWindow.globalMapper[num3].Item2, num3);
                    }
                }

                return parameterRenameWindow;
            }

            internal override void OnCustomGUI()
            {
                if (globalMapper == null)
                {
                    Close();
                    return;
                }

                m_DicPolicy = true;
                EditorGUI.BeginChangeCheck();
                GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, serviceMapper, Color.green, Color.grey);
                try
                {
                    serviceMapper = ControllerEditorUtility.AddQueue(serviceMapper, "Unique Parameters", GUI.skin.button);
                }
                finally
                {
                    if (templateThread != null)
                    {
                        ((IDisposable)templateThread).Dispose();
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < globalMapper.Length; i++)
                    {
                        globalMapper[i].Item2 = ListTests(globalMapper[i].Item2, i);
                    }
                }

                int num = 0;
                while (num < globalMapper.Length)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(EditorStyles.helpBox);
                    try
                    {
                        GUILayout.Label(new GUIContent(CloneAnnotation(globalMapper[num].Item1.name, 9, 5), globalMapper[num].Item1.name), GUILayout.Width(125f));
                        EditorGUI.BeginChangeCheck();
                        globalMapper[num].Item2 = EditorGUILayout.TextField(globalMapper[num].Item2);
                        if (EditorGUI.EndChangeCheck() && serviceMapper)
                        {
                            globalMapper[num].Item2 = ListTests(globalMapper[num].Item2, num);
                        }

                        if (string.IsNullOrEmpty(globalMapper[num].Item2))
                        {
                            m_DicPolicy = false;
                            GUILayout.Label(new GUIContent(ControllerEditorUtility.DestroyError().issuerProcessor.CompareHelper(), "Parameter must not be empty"), ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.ExpandWidth(expand: false));
                        }
                    }
                    finally
                    {
                        if (horizontalScope != null)
                        {
                            ((IDisposable)horizontalScope).Dispose();
                        }
                    }

                    num++;
                }

                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox);
                try
                {
                    stateMapper = EditorGUILayout.Popup("Write Defaults", stateMapper, m_BroadcasterMapper);
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }
            }

            internal override void OnCustomConfirm()
            {
                _003C_003Ec__DisplayClass10_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass10_0();
                UnityEditor.Animations.AnimatorControllerLayer[] array = CompareAlgo(m_ProxyMapper, _StructMapper, globalMapper);
                if (stateMapper == 0)
                {
                    return;
                }

                CS_0024_003C_003E8__locals2.producerMapper = stateMapper == 1;
                UnityEditor.Animations.AnimatorControllerLayer[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i].stateMachine.AssetPredicate(delegate (AnimatorState s)
                    {
                        s.writeDefaultValues = CS_0024_003C_003E8__locals2.producerMapper;
                        s.PrintPredicate();
                    });
                }
            }

            private string ListTests(string key, int remove_COUNTERAt)
            {
                _003C_003Ec__DisplayClass11_0 CS_0024_003C_003E8__locals6 = new _003C_003Ec__DisplayClass11_0();
                CS_0024_003C_003E8__locals6.publisherMapper = remove_COUNTERAt;
                CS_0024_003C_003E8__locals6.m_ConfigurationMapper = this;
                string text = key;
                string text2;
                do
                {
                    text2 = text;
                    text = MapAnnotation(text, delegate (string s)
                    {
                        int num = 0;
                        while (true)
                        {
                            if (num >= CS_0024_003C_003E8__locals6.m_ConfigurationMapper.globalMapper.Length)
                            {
                                return true;
                            }

                            if (num != CS_0024_003C_003E8__locals6.publisherMapper && CS_0024_003C_003E8__locals6.m_ConfigurationMapper.globalMapper[num].Item2 == s)
                            {
                                break;
                            }

                            num++;
                        }

                        return false;
                    });
                    text = MapAnnotation(text, delegate (string s)
                    {
                        _003C_003Ec__DisplayClass11_1 _003C_003Ec__DisplayClass11_ = new _003C_003Ec__DisplayClass11_1();
                        _003C_003Ec__DisplayClass11_.m_ProcMapper = s;
                        return CS_0024_003C_003E8__locals6.m_ConfigurationMapper._StructMapper.parameters.All(_003C_003Ec__DisplayClass11_.PrepareTests);
                    });
                }
                while (text2 != text);
                return text;
            }

            internal Vector2 FillTests()
            {
                return new Vector2(350f, 60f + (float)globalMapper.Length * (EditorGUIUtility.singleLineHeight + 7f));
            }
        }

        internal class QuickToggleWindow : CustomUtilityWindow<QuickToggleWindow>
        {
            [Serializable]
            [CompilerGenerated]
            internal sealed class _003C_003Ec
            {
                public static readonly _003C_003Ec observerInitializer = new _003C_003Ec();
                public static Func<AnimatorState, bool> serverInitializer;
                public static Func<GameObject, AnimatorTreeView> m_ThreadInitializer;
                public static Action<SearchPopup<string>.UtilsServer> policyInitializer;
                public static Func<string, string, bool> m_SerializerInitializer;
                public static Func<Component, Type> m_PageInitializer;
                public static Action<SearchPopup<Type>.UtilsServer> _ResolverInitializer;
                public static Func<Type, object[]> predicateInitializer;
                public static Func<GameObject, AnimatorTreeView> rulesInitializer;
                public static Func<AnimatorState, bool> queueInitializer;
                public static Func<AnimatorState, bool> m_ErrorInitializer;
                public static Func<bool, bool> setterInitializer;
                public static Func<bool, bool> _ConnectionInitializer;
                internal static _003C_003Ec FillInfo;
                internal void OrderProperty(SearchPopup<string>.UtilsServer i)
                {
                    GUILayout.Label(i._ValueServer);
                }

                internal bool CompareProperty(string p, string s)
                {
                    return p.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                internal Type SetProperty(Component c)
                {
                    return c.GetType();
                }

                internal void PostProperty(SearchPopup<Type>.UtilsServer item)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(EditorStyles.helpBox);
                    try
                    {
                        GUILayout.Label((GUIContent)item.CloneConnection(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }
                    finally
                    {
                        if (horizontalScope != null)
                        {
                            ((IDisposable)horizontalScope).Dispose();
                        }
                    }
                }

                internal object[] SetupProperty(Type type)
                {
                    return new object[1]{new GUIContent(image: EditorGUIUtility.ObjectContent(null, type).image ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image, text: type.Name, tooltip: type.AssemblyQualifiedName)};
                }

                internal AnimatorTreeView EnableProperty(GameObject o)
                {
                    return new AnimatorTreeView(o);
                }
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass18_0
            {
                public QuickToggleWindow m_ContextInitializer;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass18_1
            {
                public AnimatorTreeView m_RecordInitializer;
                public int _HelperInitializer;
                public _003C_003Ec__DisplayClass18_0 m_ConsumerInitializer;
                internal void CountProperty(int i, string s)
                {
                    while (true)
                    {
                        m_RecordInitializer._ConsumerProperty = i;
                        int num = 0;
                        while (true)
                        {
                            switch (num)
                            {
                                case 4:
                                    return;
                                case 3:
                                    return;
                                case 1:
                                    break;
                                case 2:
                                    goto IL_00a4;
                                default:
                                    goto IL_00a4;
                            }

                            break;
                            IL_00a4:
                                m_ConsumerInitializer.m_ContextInitializer.Repaint();
                            num = 4;
                        }
                    }
                }

                internal void DisableProperty(int i, Type _)
                {
                    m_RecordInitializer.OrderPage(i - 1);
                    m_ConsumerInitializer.m_ContextInitializer.Repaint();
                }

                internal void InsertProperty(IEnumerable<GameObject> ie)
                {
                    m_ConsumerInitializer.m_ContextInitializer.annotationInitializer.InsertRange(_HelperInitializer, ie.Where((GameObject o) => o != m_RecordInitializer.ManageSerializer()).Select(_003C_003Ec.observerInitializer.EnableProperty));
                }

                internal void QueryProperty(float y)
                {
                    if (y > 0f)
                    {
                        m_RecordInitializer.CheckSerializer(!AddInitializer().advancedQuickToggle);
                    }
                    else
                    {
                        m_RecordInitializer.StopSerializer(!AddInitializer().advancedQuickToggle);
                    }

                    m_ConsumerInitializer.m_ContextInitializer.Repaint();
                }
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass18_2
            {
                public SearchPopup<Type>.UtilsServer interpreterInitializer;
                internal bool AddProperty(Type vt)
                {
                    return interpreterInitializer._ValueServer.InstantiateResolver(vt);
                }
            }

            private Transform m_WrapperInitializer;
            private List<AnimatorTreeView> annotationInitializer;
            private List<AnimatorState> _VisitorInitializer;
            private InlineReorderableList<AnimatorTreeView> _AlgoInitializer;
            private static readonly Color[] _MapperInitializer;
            private int _InitializerInitializer;
            private int definitionInitializer;
            private bool[] regInitializer;
            private bool testsInitializer;
            private bool propertyInitializer;
            private static readonly GUIContent[] _ProcessorInitializer;
            internal override string title
            {
                get
                {
                    return "CEditor QuickToggle";
                }
            }

            [SpecialName]
            private static bool RegisterTests()
            {
                return AddInitializer().advancedQuickToggle;
            }

            [SpecialName]
            private static void LogoutTests(bool loadinit)
            {
                AddInitializer().advancedQuickToggle.ExcludeDefinition(loadinit);
            }

            [SpecialName]
            private static bool InterruptTests()
            {
                return AddInitializer().mergeQuickToggle;
            }

            [SpecialName]
            private static void ManageTests(bool appendasset)
            {
                AddInitializer().mergeQuickToggle.ExcludeDefinition(appendasset);
            }

            internal static QuickToggleWindow AssetTests(List<AnimatorState> i, Transform second, List<GameObject> template)
            {
                _003C_003Ec__DisplayClass18_0 CS_0024_003C_003E8__locals23 = new _003C_003Ec__DisplayClass18_0();
                CS_0024_003C_003E8__locals23.m_ContextInitializer = CustomUtilityWindow<QuickToggleWindow>.CloneHelper();
                CS_0024_003C_003E8__locals23.m_ContextInitializer._VisitorInitializer = i;
                CS_0024_003C_003E8__locals23.m_ContextInitializer.regInitializer = new bool[i.Count];
                AnimatorState defaultState = ControllerEditorSettings.CallDefinition().defaultState;
                Motion motion = ((defaultState != null) ? defaultState.motion : null);
                if (InterruptTests())
                {
                    for (int j = 0; j < i.Count; j++)
                    {
                        AnimatorState animatorState = CS_0024_003C_003E8__locals23.m_ContextInitializer._VisitorInitializer[j];
                        CS_0024_003C_003E8__locals23.m_ContextInitializer.regInitializer[j] = animatorState == null || animatorState.motion != motion;
                    }
                }

                CS_0024_003C_003E8__locals23.m_ContextInitializer.definitionInitializer = i.Count((AnimatorState s) => s.motion as AnimationClip);
                CS_0024_003C_003E8__locals23.m_ContextInitializer.testsInitializer = CS_0024_003C_003E8__locals23.m_ContextInitializer.definitionInitializer > 0;
                CS_0024_003C_003E8__locals23.m_ContextInitializer.m_WrapperInitializer = second;
                CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer = new List<AnimatorTreeView>(template.Select((GameObject o) => new AnimatorTreeView(o)));
                CS_0024_003C_003E8__locals23.m_ContextInitializer._AlgoInitializer = new InlineReorderableList<AnimatorTreeView>(() =>
                {
                    CS_0024_003C_003E8__locals23.m_ContextInitializer._AlgoInitializer.LogoutThread("Target GameObjects", "The GameObjects that will be animated by the animation clip");
                    GUILayout.FlexibleSpace();
                    if (CS_0024_003C_003E8__locals23.m_ContextInitializer.testsInitializer && ControllerEditorUtility.RestartQueue(InterruptTests() ? ControllerEditorUtility.DestroyError().m_ExpressionProcessor : ControllerEditorUtility.DestroyError().systemProcessor, ControllerEditorUtility.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        ManageTests(!InterruptTests());
                    }

                    if (ControllerEditorUtility.RestartQueue(RegisterTests() ? ControllerEditorUtility.DestroyError().productProcessor : ControllerEditorUtility.DestroyError().candidateProcessor, ControllerEditorUtility.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        LogoutTests(!RegisterTests());
                    }

                    CS_0024_003C_003E8__locals23.m_ContextInitializer._AlgoInitializer.PatchThread(rejectv: false, writeattr: false);
                }, CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer, delegate
                {
                    CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer.Add(new AnimatorTreeView());
                }, (Rect rect, int index, bool active, bool focused) =>
                {
                    _003C_003Ec__DisplayClass18_1 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_1();
                    _003C_003Ec__DisplayClass18_.m_ConsumerInitializer = CS_0024_003C_003E8__locals23;
                    _003C_003Ec__DisplayClass18_._HelperInitializer = index;
                    _003C_003Ec__DisplayClass18_.m_RecordInitializer = CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer[_003C_003Ec__DisplayClass18_._HelperInitializer];
                    Rect rect2 = rect.SortResolver(RegisterTests() ? 45 : 80, isfield: false, -1f, iscont3: false, isattr4: false);
                    Rect item = new Rect(rect2)
                    {width = 20f, x = rect2.x + rect2.width - 20f};
                    if (!RegisterTests())
                    {
                        Rect rect3 = rect.SortResolver(20f, isfield: false, 80f);
                        string text = (_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer() ? "On" : "Off");
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, _003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer(), Color.green, Color.red);
                        try
                        {
                            if (GUI.Button(rect3, text))
                            {
                                _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer()) ? 1 : 0);
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
                    else
                    {
                        Rect rect4 = rect.SortResolver(40f, isfield: false, 45f, iscont3: false, isattr4: false);
                        Rect res = rect.SortResolver(15f, isfield: false, 85f);
                        rect4.height = 20f;
                        bool flag = _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty.Length != 0;
                        using (new EditorGUI.DisabledScope(!flag))
                        {
                            if (EditorGUI.DropdownButton(rect4, new GUIContent(_003C_003Ec__DisplayClass18_.m_RecordInitializer.SetPage()), FocusType.Keyboard, EditorStyles.toolbarDropDown))
                            {
                                SearchPopup<string> statusServer = new SearchPopup<string>("Property", _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty, _003C_003Ec.observerInitializer.OrderProperty, _003C_003Ec__DisplayClass18_.CountProperty);
                                statusServer.GetConnection(_003C_003Ec.observerInitializer.CompareProperty);
                                statusServer.RunConnection(rect4);
                            }
                        }

                        _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = EditorGUI.FloatField(res, _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty);
                        ControllerEditorUtility.FlushQueue(rect4, "Property", 180f, 15f, stripresult3: false);
                        ControllerEditorUtility.FlushQueue(res, "Value", 50f, 0f, stripresult3: false);
                        if (!flag)
                        {
                            ControllerEditorUtility.FlushQueue(rect4, "No Valid Properties", 145f);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer()))
                    {
                        if (GUI.Button(item, GUIContent.none, GUIStyle.none))
                        {
                            if (Event.current.button != 0)
                            {
                                _003C_003Ec__DisplayClass18_.m_RecordInitializer.StopSerializer(!AddInitializer().advancedQuickToggle);
                            }
                            else
                            {
                                SearchPopup<Type> statusServer2 = new SearchPopup<Type>("Target Type", new Type[1]{typeof(GameObject)}.Concat(_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_SetterProperty.Select(_003C_003Ec.observerInitializer.SetProperty)).Distinct().ToList(), _003C_003Ec.observerInitializer.PostProperty, _003C_003Ec__DisplayClass18_.DisableProperty);
                                statusServer2.IncludeConnection(_003C_003Ec.observerInitializer.SetupProperty);
                                if (!AddInitializer().advancedQuickToggle)
                                {
                                    SearchPopup<Type>.UtilsServer[] dicServer = statusServer2.m_DicServer;
                                    for (int k = 0; k < dicServer.Length; k++)
                                    {
                                        _003C_003Ec__DisplayClass18_2 _003C_003Ec__DisplayClass18_2 = new _003C_003Ec__DisplayClass18_2();
                                        _003C_003Ec__DisplayClass18_2.interpreterInitializer = dicServer[k];
                                        if (!AnimatorTreeView._InterpreterProperty.Any(_003C_003Ec__DisplayClass18_2.AddProperty))
                                        {
                                            _003C_003Ec__DisplayClass18_2.interpreterInitializer.authenticationServer = false;
                                        }
                                    }
                                }

                                statusServer2.RunConnection(item);
                            }

                            Event.current.Use();
                        }
                    }

                    UnityEngine.Object contextProperty = _003C_003Ec__DisplayClass18_.m_RecordInitializer.contextProperty;
                    EditorGUI.BeginChangeCheck();
                    contextProperty = EditorGUI.ObjectField(rect2, contextProperty, typeof(GameObject), allowSceneObjects: true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (!contextProperty)
                        {
                            _003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(null);
                        }
                        else if (!(contextProperty is GameObject def))
                        {
                            if (contextProperty is Component component)
                            {
                                _003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(component.gameObject);
                            }
                        }
                        else
                        {
                            _003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(def);
                        }
                    }

                    if ((bool)_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer())
                    {
                        EditorGUI.DropdownButton(item, GUIContent.none, FocusType.Passive, ControllerEditorUtility.CalcError().descriptorProcessor);
                    }

                    ControllerEditorUtility.FlushQueue(rect2, "Target", 200f, 20f, stripresult3: false);
                    ControllerEditorUtility.AwakeRules<GameObject>(rect2, _003C_003Ec__DisplayClass18_.InsertProperty);
                    ControllerEditorUtility.ResetQueue(rect2, _003C_003Ec__DisplayClass18_.QueryProperty);
                });
                CS_0024_003C_003E8__locals23.m_ContextInitializer._AlgoInitializer.m_ErrorTests = true;
                CS_0024_003C_003E8__locals23.m_ContextInitializer.ChangeTests();
                if (i.All((AnimatorState s) => s.name.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    using List<AnimatorTreeView>.Enumerator enumerator = CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.m_AdapterProperty = 0f;
                    }
                }
                else if (i.All((AnimatorState s) => s.name.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    foreach (AnimatorTreeView item2 in CS_0024_003C_003E8__locals23.m_ContextInitializer.annotationInitializer)
                    {
                        item2.m_AdapterProperty = 1f;
                    }
                }

                return CS_0024_003C_003E8__locals23.m_ContextInitializer;
            }

            internal override void OnCustomGUI()
            {
                if (_AlgoInitializer == null)
                {
                    Close();
                    return;
                }

                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox);
                try
                {
                    m_WrapperInitializer = (Transform)EditorGUILayout.ObjectField(_ProcessorInitializer[0], m_WrapperInitializer, typeof(Transform), true);
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }

                _AlgoInitializer.SortThread();
                if (!testsInitializer)
                {
                    return;
                }

                verticalScope = new GUILayout.VerticalScope(EditorStyles.helpBox);
                try
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        propertyInitializer = EditorGUILayout.Foldout(propertyInitializer, new GUIContent($"Existing Clips ({definitionInitializer})"));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent(ControllerEditorUtility.DestroyError()._AccountProcessor.CompareHelper(), "Merge: Adds the properties to the existing clips on states. Creates a new clip if no clip exists.\n\nReplace: Replaces the existing clips on states with new clips and adds the properties to them."), GUILayout.Width(14f), GUILayout.Height(18f));
                        string res = ((_InitializerInitializer == 0) ? "Merge" : ((_InitializerInitializer == 1) ? "Replace" : "Mixed"));
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, _InitializerInitializer, _MapperInitializer[0], _MapperInitializer[1], _MapperInitializer[2]);
                        try
                        {
                            if (ControllerEditorUtility.DisableQueue(res))
                            {
                                switch (_InitializerInitializer)
                                {
                                    case 0:
                                    {
                                        _InitializerInitializer = 1;
                                        for (int j = 0; j < regInitializer.Length; j++)
                                        {
                                            regInitializer[j] = false;
                                        }

                                        break;
                                    }

                                    case 1:
                                    case 2:
                                    {
                                        _InitializerInitializer = 0;
                                        for (int i = 0; i < regInitializer.Length; i++)
                                        {
                                            regInitializer[i] = true;
                                        }

                                        break;
                                    }
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

                    if (!propertyInitializer)
                    {
                        return;
                    }

                    IndentedLayoutScope schemaThread = new IndentedLayoutScope();
                    try
                    {
                        int num = 0;
                        while (num < _VisitorInitializer.Count)
                        {
                            AnimatorState animatorState = _VisitorInitializer[num];
                            if ((bool)animatorState)
                            {
                                AnimationClip animationClip = animatorState.motion as AnimationClip;
                                if ((bool)animationClip)
                                {
                                    horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
                                    try
                                    {
                                        GUILayout.Label(animationClip.name);
                                        GUILayout.FlexibleSpace();
                                        string res2 = (regInitializer[num] ? "Merge" : "Replace");
                                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, regInitializer[num], _MapperInitializer[0], _MapperInitializer[1]);
                                        try
                                        {
                                            if (ControllerEditorUtility.DisableQueue(res2))
                                            {
                                                regInitializer[num] = !regInitializer[num];
                                                ChangeTests();
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
                                }
                            }

                            num++;
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
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }
            }

            internal override void OnCustomConfirm()
            {
                if (RateAnnotation(!m_WrapperInitializer, "No Root Set!"))
                {
                    return;
                }

                List<AnimationClip> list = new List<AnimationClip>();
                int num = 0;
                AnimationClip[] array2;
                int num2;
                while (true)
                {
                    if (num >= _VisitorInitializer.Count)
                    {
                        UnityEngine.Object[] objectsToUndo;
                        UnityEngine.Object[] array = (objectsToUndo = list.Distinct().ToArray());
                        Undo.RecordObjects(objectsToUndo, "Set Quick Toggle Curve");
                        array2 = (AnimationClip[])array;
                        num2 = 0;
                        break;
                    }

                    AnimatorState animatorState = _VisitorInitializer[num];
                    Motion motion = animatorState.motion;
                    if (regInitializer[num] || !RateAnnotation(motion is UnityEditor.Animations.BlendTree, "State " + animatorState.name + " has a Blendtree motion. Can't automatically merge."))
                    {
                        AnimationClip animationClip = motion as AnimationClip;
                        if (!animationClip || !regInitializer[num])
                        {
                            Undo.RecordObject(animatorState, "Set Quick Toggle Curve");
                            string i = $"{AddInitializer().saveFolder}/Animation Clips/{LogoutMapper().name}";
                            animationClip = new AnimationClip();
                            string path = ControllerEditorUtility.AwakeList(i, animatorState.name + ".anim", writestate: true);
                            AssetDatabase.CreateAsset(animationClip, path);
                            animatorState.motion = animationClip;
                            EditorUtility.SetDirty(animatorState);
                        }

                        list.Add(animationClip);
                        num++;
                        continue;
                    }

                    return;
                }

                while (num2 < array2.Length)
                {
                    AnimationClip animationClip2 = array2[num2];
                    foreach (AnimatorTreeView item in annotationInitializer)
                    {
                        if (item.RegisterSerializer())
                        {
                            animationClip2.SetCurve(AnimationUtility.CalculateTransformPath(item.ManageSerializer().transform, m_WrapperInitializer), item._RecordProperty, item.SetPage(), ControllerEditorUtility.InvokePredicate(AnimationUtility.TangentMode.Linear, (0f, item.m_AdapterProperty), (animationClip2.FindPredicate(), item.m_AdapterProperty)));
                        }
                    }

                    num2++;
                }
            }

            internal Vector2 UpdateTests()
            {
                return new Vector2(370f, 48 + 22 * Mathf.Max(1, annotationInitializer.Count) + 28 + ((!string.IsNullOrEmpty(invocationPolicy)) ? 38 : 0) + (testsInitializer ? 32 : 0));
            }

            internal void ChangeTests()
            {
                _InitializerInitializer = ((!regInitializer.All((bool b) => b)) ? (regInitializer.All((bool b) => !b) ? 1 : 2) : 0);
            }

            internal void SortTests(Vector2 instance)
            {
                LoginHelper(instance, UpdateTests());
            }

            static QuickToggleWindow()
            {
                while (true)
                {
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                                break;
                            default:
                                goto IL_0050;
                            case 2:
                                _ProcessorInitializer = new GUIContent[4]{new GUIContent("Root", "Relative path root of the animation"), new GUIContent("Target", "Target GameObject or GameObject containing target Component"), new GUIContent("Component Index", "Which component to toggle. -1 is GameObject. 0 is Transform (Not toggleable)"), new GUIContent("Enabled", "What the toggled state is when animated")};
                                return;
                        }

                        break;
                        IL_0050:
                            _MapperInitializer = new Color[3]{Color.green, Color.cyan, Color.yellow};
                        num = 2;
                    }
                }
            }
        }
    }
}
