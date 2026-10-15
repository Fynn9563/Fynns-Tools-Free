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
    internal sealed partial class ControllerEditor : EditorWindow, IHasCustomMenu
    {
        private static EditorWindow m_Base;
        private static UnityEditor.Animations.AnimatorController _Container;
        private static AnimatorStateMachine @class;
        private static AnimatorStateMachine _Mock;
        private static Vector2 field;
        private static int _Attribute;
        private static bool _Client;
        private static bool m_Config;
        private static bool _Descriptor;
        private static bool m_Template;
        private static bool m_Message;
        private static bool collection;
        private static bool m_Parser;
        private static bool _Manager;
        private static bool specification = true;
        private static string[] _Method;
        private static string[] _Schema;
        private static string[] broadcaster;
        private static string[] _Proxy;
        private static StateMachineBehaviour[] @struct;
        private static ControllerAssetInventory _Service;
        private static AnimatorStateTransition[] global;
        private static SerializedObject task;
        private static readonly List<UnityAnimatorBridge.TokenizerTests> _Process;
        private static readonly ConcurrentBag<UnityAnimatorBridge.BaseTests> _Producer;
        private static List<AnimatorTransitionBase> _Iterator;
        private static SerializedObject _Publisher;
        private static SerializedObject m_Proc;
        private static AnimatorStateMachine[] m_WrapperAnnotation;
        private static AnimatorStateMachine[] _AnnotationAnnotation;
        private static List<AnimatorState> _VisitorAnnotation;
        private static List<AnimatorState> m_AlgoAnnotation;
        private static List<UnityAnimatorBridge.TokenizerTests> m_MapperAnnotation;
        private static ConcurrentBag<UnityAnimatorBridge.BaseTests> _InitializerAnnotation;
        private static List<UnityAnimatorBridge.InstanceTests> _DefinitionAnnotation;
        private static AnimatorTransitionBase[] regAnnotation;
        private static List<AnimatorStateTransition> _TestsAnnotation;
        private static List<AnimatorTransitionBase> propertyAnnotation;
        private static UnityAnimatorBridge.InstanceTests _ProcessorAnnotation;
        private static AnimatorStateTransition _ObserverAnnotation;
        private static List<UnityAnimatorBridge.InstanceTests> serverAnnotation;
        private static List<AnimatorCondition> m_ThreadAnnotation;
        private static readonly Dictionary<string, GUIStyle> m_PolicyAnnotation;
        private static readonly HashSet<string> m_SerializerAnnotation;
        private static readonly List<string> _PageAnnotation;
        private static Dictionary<string, GUIStyle> m_ResolverAnnotation;
        private static GUIStyle predicateAnnotation;
        private static TrackingControlEditor rulesAnnotation;
        private static List<AlgoAlgo> _QueueAnnotation;
        private static ReorderableList _ErrorAnnotation;
        private static List<SerializedPropertyGroup.BridgeProperty> setterAnnotation;
        private static bool m_ConnectionAnnotation;
        private static FieldInfo contextAnnotation;
        private static FieldInfo recordAnnotation;
        private static PropertyInfo helperAnnotation;
        private static PropertyInfo m_ConsumerAnnotation;
        private static MethodInfo _AdapterAnnotation;
        private static MethodInfo interpreterAnnotation;
        private static string m_BridgeAnnotation;
        private static string strategyAnnotation;
        private static string m_CustomerAnnotation;
        private static readonly Type[] infoAnnotation;
        private static readonly AnimBool factoryAnnotation;
        private static readonly AnimBool _AccountAnnotation;
        private static readonly ToolVersion m_RefAnnotation;
        private static readonly (string, string)[] statusAnnotation;
        private static List<GlobalVisitor> _TokenAnnotation;
        private static List<GlobalVisitor> m_CodeAnnotation;
        private static List<GlobalVisitor> m_DicAnnotation;
        private static ReorderableList invocationAnnotation;
        private static ReorderableList _RoleAnnotation;
        private static ReorderableList m_ParamAnnotation;
        private static int _ModelAnnotation;
        private static SerializedProperty m_TokenizerAnnotation;
        private static SerializedProperty decoratorAnnotation;
        private static SerializedProperty m_ComparatorAnnotation;
        private static SerializedProperty m_ExceptionAnnotation;
        private static SerializedProperty objectAnnotation;
        private static SerializedProperty _UtilsAnnotation;
        private static SerializedProperty valAnnotation;
        private static SerializedProperty m_ValueAnnotation;
        private static SerializedProperty _MerchantAnnotation;
        private static SerializedProperty m_AuthenticationAnnotation;
        private static SerializedProperty m_ReponseAnnotation;
        private static SerializedProperty _PoolAnnotation;
        private static SerializedProperty m_ParameterAnnotation;
        private static SerializedProperty _ComposerAnnotation;
        private static SerializedProperty _RepositoryAnnotation;
        private static SerializedProperty m_MappingAnnotation;
        private static bool baseAnnotation;
        private static bool containerAnnotation;
        private static bool m_ClassAnnotation;
        private static SerializedProperty _MockAnnotation;
        private static SerializedProperty instanceAnnotation;
        private static SerializedProperty _FieldAnnotation;
        private static SerializedProperty attributeAnnotation;
        private static SerializedProperty m_ClientAnnotation;
        private static SerializedProperty configAnnotation;
        private static SerializedProperty m_DescriptorAnnotation;
        private static SerializedProperty _TemplateAnnotation;
        private static SerializedProperty _MessageAnnotation;
        private static SerializedProperty m_CollectionAnnotation;
        private static bool parserAnnotation;
        private static Type[] m_ManagerAnnotation;
        private static readonly Dictionary<Type, string[]> m_ItemAnnotation;
        private static readonly Dictionary<Shader, string[]> _SpecificationAnnotation;
        private static UnityEditor.Animations.AnimatorController m_MethodAnnotation;
        private static string _SchemaAnnotation;
        private static string m_BroadcasterAnnotation;
        private static string proxyAnnotation;
        private static bool structAnnotation;
        private static bool serviceAnnotation;
        private static string _StateAnnotation;
        private static ControllerAction m_GlobalAnnotation;
        private static ActionMode _TaskAnnotation;
        private static MoveMode m_ProcessAnnotation;
        private MoveDestination producerAnnotation;
        private static EditorWindow _IteratorAnnotation;
        private static GameObject m_PublisherAnnotation;
        private static Animator m_ConfigurationAnnotation;
        private static bool wrapperVisitor;
        private static UnityEditor.Animations.AnimatorController annotationVisitor;
        private static bool m_VisitorVisitor;
        private static GameObject _AlgoVisitor;
        private static Type mapperVisitor;
        private static Type _InitializerVisitor;
        private static Type definitionVisitor;
        private static Type _RegVisitor;
        private static Type _TestsVisitor;
        private static Type _PropertyVisitor;
        private static PropertyInfo m_ProcessorVisitor;
        private static PropertyInfo _ObserverVisitor;
        private static PropertyInfo serverVisitor;
        private static PropertyInfo m_ThreadVisitor;
        private static PropertyInfo m_PolicyVisitor;
        private static FieldInfo m_SerializerVisitor;
        private static bool pageVisitor;
        private static List<object> resolverVisitor;
        private static Type rulesVisitor;
        private static Type m_QueueVisitor;
        private static Type errorVisitor;
        private static FieldInfo m_SetterVisitor;
        private static Texture2D connectionVisitor;
        private static readonly HashSet<Vector3> m_ContextVisitor;
        private static bool m_RecordVisitor;
        private static bool m_HelperVisitor;
        private static Type _ConsumerVisitor;
        private static Type m_AdapterVisitor;
        private static Type m_InterpreterVisitor;
        private static Type _WatcherVisitor;
        private static Type m_CandidateVisitor;
        private static Type m_ProductVisitor;
        private static bool _ExpressionVisitor;
        private static bool _SystemVisitor;
        private static MethodInfo workerVisitor;
        private static bool filterVisitor;
        private static object m_StubVisitor;
        private static Type _ReaderVisitor;
        private static Type _BridgeVisitor;
        private static FieldInfo m_StrategyVisitor;
        private static MethodInfo _CustomerVisitor;
        private static MethodInfo m_DatabaseVisitor;
        private static MethodInfo _ExporterVisitor;
        private static FieldInfo m_IdentifierVisitor;
        private static FieldInfo m_AttrVisitor;
        private static FieldInfo m_DispatcherVisitor;
        private static MethodInfo registryVisitor;
        private static bool _TagVisitor;
        private static Vector2 importerVisitor;
        private static UnityEditor.Animations.AnimatorController[] requestVisitor;
        private static string[] m_PrinterVisitor;
        private static LayerViewViewType writerVisitor;
        private static ImporterMapper m_ParamsVisitor;
        private static ImporterMapper listenerVisitor;
        private static ReorderableList m_GetterVisitor;
        private static ReorderableList m_InterceptorVisitor;
        private static string[] creatorVisitor;
        private static bool eventVisitor;
        private static bool _InfoVisitor;
        private static ReorderableList.ElementCallbackDelegate _FacadeVisitor;
        private static ReorderableList.SelectCallbackDelegate _AdvisorVisitor;
        private static ReorderableList.SelectCallbackDelegate _CallbackVisitor;
        private static AnimationWindowBridge indexerVisitor;
        private static AnimationWindowBridge _IssuerVisitor;
        private static MethodInfo m_PrototypeVisitor;
        private static ConstructorInfo m_RuleVisitor;
        private static GenericMenu m_SingletonVisitor;
        private static Type _FactoryVisitor;
        private static Type _AccountVisitor;
        private static Type _RefVisitor;
        private static Type _StatusVisitor;
        private static FieldInfo tokenVisitor;
        private static FieldInfo codeVisitor;
        private static FieldInfo m_DicVisitor;
        private static PropertyInfo _InvocationVisitor;
        private static MethodInfo roleVisitor;
        private static MethodInfo paramVisitor;
        private static MethodInfo modelVisitor;
        private static MethodInfo tokenizerVisitor;
        private static MethodInfo m_DecoratorVisitor;
        private static readonly MethodInfo m_ComparatorVisitor;
        private static Node exceptionVisitor;
        private static bool m_ObjectVisitor;
        private static bool m_UtilsVisitor;
        private static bool _ValVisitor;
        private static bool valueVisitor;
        private static AnimatorStateTransition merchantVisitor;
        private static AnimatorState m_AuthenticationVisitor;
        private static Vector2 _ReponseVisitor;
        private static bool poolVisitor;
        private static AnimatorState parameterVisitor;
        private static UnityEngine.AnimatorControllerParameter[] m_ComposerVisitor;
        private static FieldInfo _RepositoryVisitor;
        private static Type mappingVisitor;
        private static Vector2 m_ContainerVisitor;
        private static object classVisitor;
        private static int _MockVisitor;
        private static bool _InstanceVisitor;
        private static bool _FieldVisitor;
        private static bool _AttributeVisitor;
        private static bool clientVisitor;
        private static int m_ConfigVisitor;
        private static bool m_DescriptorVisitor;
        private static AnimatorState templateVisitor;
        private static UnityEditor.Animations.AnimatorController m_MessageVisitor;
        private static UnityEditor.Animations.AnimatorControllerLayer collectionVisitor;
        private static MethodInfo _ParserVisitor;
        private static MethodInfo _ManagerVisitor;
        private static MethodInfo m_ItemVisitor;
        private static MethodInfo m_SpecificationVisitor;
        private static void PatchWrapper()
        {
            while (m_Base != null)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            return;
                        case 4:
                            break;
                        case 2:
                            return;
                        default:
                            goto IL_0079;
                        case 3:
                            goto IL_0079;
                    }

                    break;
                    IL_0079:
                        m_Base.Repaint();
                    num = 1;
                }
            }
        }

        [SpecialName]
        internal static UnityEditor.Animations.AnimatorController LogoutMapper()
        {
            while (true)
            {
                int num;
                if ((bool)_Container)
                {
                    num = 2;
                    goto IL_001d;
                }

                goto IL_00ed;
                IL_001d:
                    switch (num)
                    {
                        case 3:
                            break;
                        case 1:
                        case 2:
                        case 5:
                            return _Container;
                        case 4:
                            goto IL_00cc;
                        default:
                            goto IL_00ed;
                    }

                continue;
                IL_00ed:
                    ;
                goto IL_00cc;
                IL_00cc:
                    InstantiateAnnotation();
                num = 1;
                goto IL_001d;
            }
        }

        [SpecialName]
        internal static void PatchMapper(UnityEditor.Animations.AnimatorController v)
        {
            while (_Container != v)
            {
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
                        case 5:
                            _Container = v;
                            num = 2;
                            continue;
                        case 2:
                            CollectAnnotation();
                            num = 3;
                            continue;
                        default:
                            goto case 5;
                    }

                    break;
                }
            }
        }

        [SpecialName]
        private static AnimatorStateMachine ManageMapper()
        {
            while (!@class)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            DefineAnnotation();
                            num = 0;
                            continue;
                        case 4:
                            goto end_IL_001d;
                        case 3:
                            goto case 1;
                    }

                    goto end_IL_00a3;
                    continue;
                    end_IL_001d:
                        break;
                }

                continue;
                end_IL_00a3:
                    break;
            }

            return @class;
        }

        [SpecialName]
        private static void PrintMapper(AnimatorStateMachine var1)
        {
            while (@class != var1)
            {
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            return;
                        case 1:
                            break;
                        case 2:
                            @class = var1;
                            num = 5;
                            continue;
                        case 4:
                            return;
                        case 5:
                            PushAnnotation();
                            num = 4;
                            continue;
                        default:
                            goto case 2;
                    }

                    break;
                }
            }
        }

        [SpecialName]
        private static AnimatorStateMachine RevertMapper()
        {
            while (!_Mock)
            {
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            break;
                        default:
                            goto end_IL_003b;
                        case 4:
                            RemoveAnnotation();
                            num = 1;
                            continue;
                        case 2:
                            goto case 4;
                    }

                    break;
                }

                continue;
                end_IL_003b:
                    break;
            }

            return _Mock;
        }

        [SpecialName]
        private static void OrderInitializer(AnimatorStateMachine instance)
        {
            while (true)
            {
                int num;
                if (!(_Mock != instance))
                {
                    num = 5;
                    goto IL_001d;
                }

                goto IL_00ee;
                IL_001d:
                    switch (num)
                    {
                        default:
                            return;
                        case 1:
                            return;
                        case 2:
                            break;
                        case 5:
                            return;
                        case 4:
                            goto IL_00b1;
                        case 6:
                            continue;
                        case 3:
                            goto IL_00ee;
                    }

                goto IL_0052;
                IL_00ee:
                    ;
                goto IL_0052;
                IL_0052:
                    _Mock = instance;
                goto IL_00b1;
                IL_00b1:
                    ViewAnnotation();
                num = 0;
                goto IL_001d;
            }
        }

        [SpecialName]
        private static bool SetInitializer()
        {
            return _ProcessorAnnotation.itemTests != null;
        }

        [MenuItem("Tools/Fynn's Tools/Controllers+ %t", false, 13)]
        internal static void InterruptWrapper()
        {
            EditorWindow.GetWindow<ControllerEditor>(utility: false, "Controllers+", focus: true).titleContent.image = EditorGUIUtility.IconContent("d_EditCollider").image;
        }

        private void OnGUI()
        {
            if (!OrderVisitor(this))
            {
                return;
            }

            Event current = Event.current;
            if ((current.keyCode != KeyCode.Return && current.keyCode != KeyCode.KeypadEnter) || current.type != EventType.KeyDown)
            {
                _Attribute = 0;
                ScrollViewScope specificationThread = new ScrollViewScope(ref field);
                try
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        ControllerEditorSettings.BoolSetting[] array = new ControllerEditorSettings.BoolSetting[3]{AddInitializer().editingTransitions, AddInitializer().editingStates, AddInitializer().editingController};
                        string[] array2 = new string[3]{"Transitions", "States", "Controller"};
                        int i = 0;
                        for (; i < 3; i++)
                        {
                            EditorGUI.BeginChangeCheck();
                            array[i].ExcludeDefinition(ControllerEditorUtility.AddQueue(array[i], array2[i], EditorStyles.toolbarButton));
                            if (EditorGUI.EndChangeCheck())
                            {
                                switch (i)
                                {
                                    case 0:
                                        parserAnnotation = AddInitializer().editingTransitions;
                                        break;
                                    case 1:
                                        _Descriptor = AddInitializer().editingStates;
                                        break;
                                }
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

                    EditorGUI.BeginDisabledGroup(!LogoutMapper());
                    GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(ControllerEditorUtility.CalcError()._WrapperObserver);
                    try
                    {
                        horizontalScope = new GUILayout.HorizontalScope();
                        try
                        {
                            GUILayout.Space(18f);
                            if (RevertMapper() != null)
                            {
                                GUILayout.Label(RevertMapper().name, ControllerEditorUtility.CalcError()._ClassProcessor, GUILayout.ExpandWidth(expand: true));
                            }
                            else
                            {
                                GUILayout.Label("No Active Machine", ControllerEditorUtility.CalcError()._ClassProcessor, GUILayout.ExpandWidth(expand: true));
                            }

                            EditorGUI.EndDisabledGroup();
                            if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError()._DicProcessor, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)) && EditorUtility.DisplayDialog("Instructions", "Open Controllers+'s Online Manual?", "Open", "Cancel"))
                            {
                                Application.OpenURL("https://notes.sleightly.dev/ceditor");
                            }

                            if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError()._RequestProcessor, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)))
                            {
                                ControllerEditorSettingsWindow.CalcTests();
                            }

                            EditorGUI.BeginDisabledGroup(!LogoutMapper());
                        }
                        finally
                        {
                            if (horizontalScope != null)
                            {
                                ((IDisposable)horizontalScope).Dispose();
                            }
                        }

                        if ((bool)ManageMapper())
                        {
                            if (_Method.Length != 0)
                            {
                                horizontalScope = new GUILayout.HorizontalScope();
                                try
                                {
                                    GUILayout.FlexibleSpace();
                                    for (int j = 0; j < _Method.Length; j++)
                                    {
                                        GUILayout.Label(_Method[j], "AssetLabel");
                                    }

                                    GUILayout.FlexibleSpace();
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
                    }
                    finally
                    {
                        if (verticalScope != null)
                        {
                            ((IDisposable)verticalScope).Dispose();
                        }
                    }

                    EditorGUI.EndDisabledGroup();
                    ControllerEditorUtility.MapQueue();
                    ReflectVisitor();
                    IncludeAnnotation(parserAnnotation && (_Descriptor || AddInitializer().editingController.FindDefinition()));
                    DestroyVisitor();
                    IncludeAnnotation(_Descriptor && AddInitializer().editingController.FindDefinition());
                    ValidateVisitor();
                    DrawInspection();
                    DefineVisitor();
                    EditorPatchRegistry.LoginReg();
                    return;
                }
                finally
                {
                    if (specificationThread != null)
                    {
                        ((IDisposable)specificationThread).Dispose();
                    }
                }
            }

            GUI.FocusControl(null);
            Repaint();
        }

        private static void ManageWrapper()
        {
            if ((bool)AddInitializer().aw_active && (bool)AddInitializer().aw_autoSwitchClip && Selection.activeObject is AnimatorState { motion: AnimationClip motion })
            {
                StartInitializer(motion);
            }

            _IssuerVisitor.MapSerializer(isconfig: true);
            m_MapperAnnotation = UnityAnimatorBridge.RefTests.CalcPolicy()?.ToList() ?? _Process;
            _InitializerAnnotation = UnityAnimatorBridge.RefTests.RunPolicy() ?? _Producer;
            _DefinitionAnnotation = new List<UnityAnimatorBridge.InstanceTests>();
            IEnumerator<UnityAnimatorBridge.BaseTests> enumerator = _InitializerAnnotation.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UnityAnimatorBridge.BaseTests current = enumerator.Current;
                    _DefinitionAnnotation.AddRange(current.FindSerializer());
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            _Manager = (m_Parser = false);
            bool flag = !m_ClassAnnotation && m_MapperAnnotation.Any((UnityAnimatorBridge.TokenizerTests nw) => nw.FillPolicy() == UnityAnimatorBridge.RefTests.FlushPolicy().FillPolicy());
            bool flag2 = !m_ClassAnnotation && m_MapperAnnotation.Any((UnityAnimatorBridge.TokenizerTests nw) => nw.FillPolicy() == UnityAnimatorBridge.RefTests.MapPolicy().FillPolicy());
            m_Message |= flag;
            m_Template |= flag2;
            collection = m_MapperAnnotation.Any((UnityAnimatorBridge.TokenizerTests nw) => nw.FillPolicy() == UnityAnimatorBridge.RefTests.CalculatePolicy().FillPolicy());
            bool flag3 = false;
            IEnumerator<UnityEngine.Object> enumerator2 = Selection.objects.CloneRules().GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    UnityEngine.Object current2 = enumerator2.Current;
                    if (SetInitializer())
                    {
                        flag3 |= current2 == _ProcessorAnnotation.itemTests;
                    }

                    Type type = current2.GetType();
                    if (!m_Parser && type == typeof(AnimatorStateTransition))
                    {
                        m_Parser = true;
                    }
                    else if (!_Manager && type == typeof(AnimatorTransition))
                    {
                        _Manager = true;
                    }

                    if (m_Parser && _Manager && (!SetInitializer() || flag3))
                    {
                        break;
                    }
                }
            }
            finally
            {
                if (enumerator2 != null)
                {
                    enumerator2.Dispose();
                }
            }

            if (!m_ClassAnnotation)
            {
                if (!flag2)
                {
                    m_Template = false;
                }

                if (!flag)
                {
                    m_Message = false;
                }
            }

            if (!flag3)
            {
                _ProcessorAnnotation = default(UnityAnimatorBridge.InstanceTests);
            }

            if (UnityAnimatorBridge.RefTests.CalculatePolicy() != null && UnityAnimatorBridge.RefTests.CalculatePolicy().FillPolicy() != null)
            {
                regAnnotation = UnityAnimatorBridge.RefTests.CalculatePolicy().ChangePolicy().ToArray();
            }

            AnimatorTransitionBase[] filtered = Selection.GetFiltered<AnimatorTransitionBase>(SelectionMode.Editable);
            propertyAnnotation = _DefinitionAnnotation.Select((UnityAnimatorBridge.InstanceTests t) => t.itemTests).ToList();
            _TokenAnnotation = AssetVisitor(propertyAnnotation);
            AnimatorState[] filtered2 = Selection.GetFiltered<AnimatorState>(SelectionMode.Editable);
            m_AlgoAnnotation = filtered2.ToList();
            object publisher;
            if (m_AlgoAnnotation.Count <= 0)
            {
                publisher = null;
            }
            else
            {
                UnityEngine.Object[] objs = filtered2;
                publisher = new SerializedObject(objs);
            }

            _Publisher = (SerializedObject)publisher;
            m_WrapperAnnotation = Selection.GetFiltered<AnimatorStateMachine>(SelectionMode.Editable);
            if (_DefinitionAnnotation.Count == 0)
            {
                m_CodeAnnotation.Clear();
                _Iterator.Clear();
            }
            else
            {
                List<GlobalVisitor> list = new List<GlobalVisitor>();
                AnimatorTransitionBase[] array = _Iterator.Where((AnimatorTransitionBase t) => !propertyAnnotation.Contains(t)).ToArray();
                AnimatorTransitionBase[] array2 = array;
                for (int num = 0; num < array2.Length; num++)
                {
                    _003C_003Ec__DisplayClass59_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass59_0();
                    CS_0024_003C_003E8__locals2.processReg = array2[num];
                    list.AddRange(m_CodeAnnotation.Where((GlobalVisitor c) => c._ProducerVisitor[0].Item1 == CS_0024_003C_003E8__locals2.processReg));
                }

                _Iterator = _Iterator.Except(array).ToList();
                m_CodeAnnotation = m_CodeAnnotation.Except(list).ToList();
                IEnumerator<AnimatorTransitionBase> enumerator3 = propertyAnnotation.Where((AnimatorTransitionBase t) => !_Iterator.Contains(t)).GetEnumerator();
                try
                {
                    while (enumerator3.MoveNext())
                    {
                        AnimatorTransitionBase current3 = enumerator3.Current;
                        _Iterator.Add(current3);
                        for (int num2 = 0; num2 < current3.conditions.Length; num2++)
                        {
                            m_CodeAnnotation.Add(new GlobalVisitor(current3, num2));
                        }
                    }
                }
                finally
                {
                    if (enumerator3 != null)
                    {
                        enumerator3.Dispose();
                    }
                }
            }

            MapVisitor();
            _TestsAnnotation = Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.Editable).ToList();
            RunAlgo();
            CalculateAnnotation();
            if (!containerAnnotation && !baseAnnotation)
            {
                serverAnnotation = _DefinitionAnnotation.ToList();
            }

            if (!m_ClassAnnotation)
            {
                _VisitorAnnotation = m_AlgoAnnotation;
                _AnnotationAnnotation = m_WrapperAnnotation;
            }

            ConnectAnnotation();
            if (SerializedPropertyGroup.InvokePage())
            {
                CallAnnotation();
                MoveAnnotation();
            }

            parserAnnotation = (bool)AddInitializer().editingTransitions || filtered.Length != 0;
            _Descriptor = (bool)AddInitializer().editingStates || m_AlgoAnnotation.Count > 0;
            PatchWrapper();
        }

        private void OnFocus()
        {
            ManageWrapper();
        }

        private void PrintWrapper()
        {
            UpdateVisitor();
            Repaint();
        }

        private void OnDisable()
        {
            m_ClassAnnotation = false;
            containerAnnotation = false;
            baseAnnotation = false;
            Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Controllers+", EditorGUIUtility.IconContent("d_EditCollider").image);
            m_Base = this;
            Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
            Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
            EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(SortAlgo));
            EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(SortAlgo));
            if (global == null)
            {
                global = new AnimatorStateTransition[2]{new AnimatorStateTransition{name = "a", canTransitionToSelf = false, duration = 0f, exitTime = 0f, hasExitTime = false, hasFixedDuration = false, interruptionSource = TransitionInterruptionSource.None, mute = false, offset = 0f, orderedInterruption = false, solo = false}, new AnimatorStateTransition{name = "b", canTransitionToSelf = true, duration = 1f, exitTime = 1f, hasExitTime = true, hasFixedDuration = true, interruptionSource = TransitionInterruptionSource.Destination, mute = true, offset = 1f, orderedInterruption = true, solo = true}};
                UnityEngine.Object[] objs = global;
                task = new SerializedObject(objs);
            }

            m_Proc = task;
            CalculateAnnotation();
            MapVisitor();
            CancelAnnotation();
        }

        private static void RevertWrapper()
        {
            Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(ManageWrapper));
            Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(ManageWrapper));
            OrderAnnotation();
            CompareAnnotation();
            RegisterAdditionalGraphExtensions();
            ControllerEditorUtility.DisableRules(SetAnnotation);
            ManageWrapper();
            PublishAnnotation();
            _SystemVisitor = true;
        }

        private static void OrderAnnotation()
        {
            try
            {
                RegisterAlgo();
                ResetMapper();
                ManageAlgo();
                {
                    ValidateMapper();
                    FillAlgo();
                    InstantiateMapper();
                    PrepareAlgo();
                }
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                throw;
            }
        }

        private static void CompareAnnotation()
        {
            ForgotAlgo();
            CustomizeMapper();
            PrintAlgo();
            FlushMapper();
            {
                RemoveMapper();
                AssetAlgo();
                LogoutAlgo();
                AwakeMapper();
            }
        }

        private static void SetAnnotation()
        {
            _003C_003Ec__DisplayClass73_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass73_0();
            if (m_ResolverAnnotation == null)
            {
                m_ResolverAnnotation = (Dictionary<string, GUIStyle>)ControllerEditorUtility.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").RestartList("m_NodeStyleCache").GetValue(null);
            }

            m_ResolverAnnotation.Clear();
            m_PolicyAnnotation.Clear();
            m_SerializerAnnotation.Clear();
            CS_0024_003C_003E8__locals15.m_ObserverTests = new GUIStyle[4]{"flow node 0", "flow node 0 on", "flow node 5", "flow node 5 on"};
            predicateAnnotation = new GUIStyle(CS_0024_003C_003E8__locals15.m_ObserverTests[0])
            {fixedWidth = 200f, fixedHeight = 40f};
            CS_0024_003C_003E8__locals15._ServerTests = new Texture2D(1, 64, TextureFormat.RGBA32, mipChain: false)
            {filterMode = FilterMode.Bilinear};
            for (int i = 0; i < 64; i++)
            {
                Color color = ((i > 40) ? new Color(0.961f, 0.655f, 0.176f) : new Color(0.973f, 0.757f, 0.267f));
                CS_0024_003C_003E8__locals15._ServerTests.SetPixel(0, i, color);
                CS_0024_003C_003E8__locals15._ServerTests.Apply();
            }

            CS_0024_003C_003E8__locals15.AssetThread("ce_Note Sticky", delegate (GUIStyle s)
            {
                GUIStyle gUIStyle = new GUIStyle(s)
                {alignment = TextAnchor.UpperLeft, clipping = TextClipping.Overflow};
                gUIStyle.fontStyle = FontStyle.Bold;
                gUIStyle.overflow = new RectOffset();
                gUIStyle.contentOffset = default(Vector2);
                gUIStyle.padding = new RectOffset(2, 2, 20, 2);
                gUIStyle.wordWrap = true;
                gUIStyle.fixedHeight = 100f;
                gUIStyle.fixedWidth = 100f;
                gUIStyle.normal.scaledBackgrounds = new Texture2D[1]{CS_0024_003C_003E8__locals15._ServerTests};
                gUIStyle.normal.textColor = Color.black;
                return gUIStyle;
            }, isCosmeticOnlyStyle: true);
            CS_0024_003C_003E8__locals15.AssetThread("ce_Note", delegate (GUIStyle s)
            {
                GUIStyle gUIStyle = new GUIStyle(s);
                gUIStyle.alignment = TextAnchor.UpperLeft;
                gUIStyle.clipping = TextClipping.Overflow;
                gUIStyle.fontStyle = FontStyle.Bold;
                gUIStyle.overflow = new RectOffset();
                gUIStyle.contentOffset = default(Vector2);
                gUIStyle.padding = new RectOffset(2, 2, 2, 2);
                gUIStyle.wordWrap = true;
                gUIStyle.fixedHeight = 100f;
                gUIStyle.fixedWidth = 100f;
                gUIStyle.normal.scaledBackgrounds = new Texture2D[1]{ControllerEditorUtility.LoginList(Color.black)};
                return gUIStyle;
            }, isCosmeticOnlyStyle: true);
            CS_0024_003C_003E8__locals15.AssetThread("ce_Note Big", delegate (GUIStyle s)
            {
                GUIStyle gUIStyle = new GUIStyle(s);
                gUIStyle.alignment = TextAnchor.UpperLeft;
                gUIStyle.clipping = TextClipping.Overflow;
                gUIStyle.fontStyle = FontStyle.Bold;
                gUIStyle.overflow = new RectOffset();
                gUIStyle.contentOffset = default(Vector2);
                gUIStyle.padding = new RectOffset(2, 2, 2, 2);
                gUIStyle.wordWrap = true;
                gUIStyle.fixedHeight = 200f;
                gUIStyle.fixedWidth = 200f;
                gUIStyle.normal.scaledBackgrounds = new Texture2D[1]{ControllerEditorUtility.LoginList(Color.black)};
                return gUIStyle;
            }, isCosmeticOnlyStyle: true);
            CS_0024_003C_003E8__locals15.AssetThread("ce_Mini", (GUIStyle s) => new GUIStyle(s)
            {fixedHeight = 40f, fixedWidth = 100f});
            CS_0024_003C_003E8__locals15.AssetThread("ce_Square", (GUIStyle s) => new GUIStyle(s)
            {fixedHeight = 40f, fixedWidth = 40f}, isCosmeticOnlyStyle: true);
            CS_0024_003C_003E8__locals15.AssetThread("ce_Square Big", (GUIStyle s) => new GUIStyle(s)
            {fixedHeight = 80f, fixedWidth = 80f, alignment = TextAnchor.MiddleCenter});
            CS_0024_003C_003E8__locals15.AssetThread("ce_Big", (GUIStyle s) => new GUIStyle(s)
            {fixedHeight = 80f, fixedWidth = 400f, fontSize = 20});
            CS_0024_003C_003E8__locals15.AssetThread("ce_Tiny", (GUIStyle s) => new GUIStyle(s)
            {fixedWidth = 20f, fixedHeight = 20f, clipping = TextClipping.Clip}, isCosmeticOnlyStyle: true, hiddenFromList: true);
            CS_0024_003C_003E8__locals15.AssetThread("ce_Dot", (GUIStyle s) => new GUIStyle(s)
            {fixedWidth = 10f, fixedHeight = 10f, clipping = TextClipping.Clip}, isCosmeticOnlyStyle: true, hiddenFromList: true);
            _PageAnnotation.Sort();
            _SystemVisitor = true;
        }

        private static void EnableAnnotation(string spec, GUIStyle vis, bool isrule, bool validatesecond2)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                bool flag = i > 1;
                bool flag2 = i % 2 == 1;
                if (!(flag ^ validatesecond2) && !(flag2 ^ isrule))
                {
                    int num = (flag ? 5 : 0);
                    stringBuilder.Clear();
                    stringBuilder.Append($"flow {spec} {num}");
                    if (flag2)
                    {
                        stringBuilder.Append(" on");
                    }

                    m_ResolverAnnotation[stringBuilder.ToString()] = vis;
                }
            }
        }

        private static void PublishAnnotation()
        {
            while (m_InterceptorVisitor != null)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 4:
                            break;
                        case 0:
                            return;
                        case 2:
                            goto IL_005e;
                        case 1:
                            return;
                        case 3:
                            goto IL_005e;
                    }

                    break;
                    IL_005e:
                        m_InterceptorVisitor.elementHeight = CloneInitializer();
                    num = 1;
                }
            }
        }

        public void AddItemsToMenu(GenericMenu i)
        {
            i.AddItem(new GUIContent("Instructions"), on: false, () =>
            {
                Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
            });
            i.AddSeparator(string.Empty);
            i.AddItem(new GUIContent("Legacy Dropdown"), AddInitializer().useLegacyDropdown, delegate
            {
                AddInitializer().useLegacyDropdown.InsertDefinition();
            });
            i.AddSeparator(string.Empty);
            i.AddItem(new GUIContent("Settings"), on: false, ControllerEditorSettingsWindow.CalcTests);
            i.AddSeparator(string.Empty);
            i.AddItem(new GUIContent("Expand Tabs"), on: false, delegate
            {
                GetAnnotation(updatereference: true);
            });
            i.AddItem(new GUIContent("Collapse Tabs"), on: false, delegate
            {
                GetAnnotation(updatereference: false);
            });
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu i)
        {
            this.AddItemsToMenu(i);
        }

        private static List<AlgoAlgo> PopAnnotation(SerializedPropertyGroup.BridgeProperty init, List<AlgoAlgo> cust = null)
        {
            List<AlgoAlgo> list = cust;
            if (list == null)
            {
                list = new List<AlgoAlgo>();
                for (int i = 0; i < init.m_DatabaseProperty.Count; i++)
                {
                    list.Add(new AlgoAlgo(init, i));
                }
            }
            else
            {
                for (int j = 0; j < init.m_DatabaseProperty.Count; j++)
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (!list[k]._MapperAlgo && ComputeAnnotation(init.m_DatabaseProperty[j], list[k].m_InitializerAlgo))
                        {
                            list[k].RegisterInitializer(init, j);
                            break;
                        }
                    }
                }

                list = list.Where((AlgoAlgo s) => s._MapperAlgo).ToList();
                for (int num = 0; num < list.Count; num++)
                {
                    list[num]._MapperAlgo = false;
                }
            }

            return list;
        }

        private static bool ComputeAnnotation(SerializedPropertyGroup.BridgeProperty.AttrProperty i, SerializedPropertyGroup.BridgeProperty.AttrProperty pol)
        {
            if (!LogoutMapper())
            {
                return false;
            }

            if (i.ResetPage() != pol.ResetPage())
            {
                return false;
            }

            if (i.CreatePage() != pol.CreatePage())
            {
                return false;
            }

            int second;
            UnityEngine.AnimatorControllerParameterType type = ResetAnnotation(i.ResetPage(), out second).type;
            if (i.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set || i.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Add)
            {
                if (type == UnityEngine.AnimatorControllerParameterType.Trigger)
                {
                    return true;
                }

                if (i.ValidatePage() != pol.ValidatePage())
                {
                    return false;
                }
            }
            else if (type != UnityEngine.AnimatorControllerParameterType.Bool && type != UnityEngine.AnimatorControllerParameterType.Trigger)
            {
                if (i.IncludePage() != pol.IncludePage() || i.LoginPage() != pol.LoginPage())
                {
                    return false;
                }
            }
            else if (i.DestroyPage() != pol.DestroyPage())
            {
                return false;
            }

            return true;
        }

        private static void MoveAnnotation()
        {
            m_ConnectionAnnotation = true;
            List<StateMachineBehaviour> list = new List<StateMachineBehaviour>();
            foreach (AnimatorState item in m_AlgoAnnotation)
            {
                bool flag = false;
                StateMachineBehaviour[] behaviours = item.behaviours;
                foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
                {
                    if (stateMachineBehaviour.GetType() == SerializedPropertyGroup.QueryPage())
                    {
                        flag = true;
                        list.Add(stateMachineBehaviour);
                    }
                }

                if (flag)
                {
                    continue;
                }

                m_ConnectionAnnotation = false;
                break;
            }

            int connectionAnnotation;
            if (m_ConnectionAnnotation)
            {
                connectionAnnotation = ((list.Count > 0) ? 1 : 0);
            }
            else
            {
                connectionAnnotation = 0;
            }

            m_ConnectionAnnotation = (byte)connectionAnnotation != 0;
            if (m_ConnectionAnnotation)
            {
                rulesAnnotation = new TrackingControlEditor(list.ToArray());
            }
        }

        private static void ConcatAnnotation()
        {
            foreach (AnimatorState item in m_AlgoAnnotation)
            {
                if (item.behaviours.All((StateMachineBehaviour b) => b.GetType() != SerializedPropertyGroup.QueryPage()))
                {
                    item.AddStateMachineBehaviour(SerializedPropertyGroup.QueryPage());
                }
            }

            MoveAnnotation();
        }

        private static void CallAnnotation()
        {
            _QueueAnnotation = null;
            setterAnnotation.Clear();
            int i = 0;
            for (; i < m_AlgoAnnotation.Count; i++)
            {
                bool flag = false;
                for (int j = 0; j < m_AlgoAnnotation[i].behaviours.Length; j++)
                {
                    if (m_AlgoAnnotation[i].behaviours[j].GetType() == SerializedPropertyGroup.InsertPage())
                    {
                        SerializedPropertyGroup.BridgeProperty bridgeProperty = new SerializedPropertyGroup.BridgeProperty(m_AlgoAnnotation[i].behaviours[j]);
                        setterAnnotation.Add(bridgeProperty);
                        _QueueAnnotation = PopAnnotation(bridgeProperty, _QueueAnnotation);
                        flag = true;
                    }
                }

                if (!flag)
                {
                    if (_QueueAnnotation != null)
                    {
                        _QueueAnnotation.Clear();
                    }

                    break;
                }
            }

            if (_QueueAnnotation == null)
            {
                _QueueAnnotation = new List<AlgoAlgo>();
            }

            CancelAnnotation();
        }

        private static void CancelAnnotation()
        {
            _ErrorAnnotation = new ReorderableList(_QueueAnnotation, typeof(AlgoAlgo), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
            {drawElementCallback = CountAnnotation, drawHeaderCallback = DisableAnnotation, onAddCallback = InsertAnnotation};
        }

        private static void CountAnnotation(Rect ident, int cust_end, bool rejectserv, bool acceptvar12)
        {
            if (!LogoutMapper())
            {
            }
            else
            {
                if (cust_end >= _QueueAnnotation.Count || cust_end < 0)
                {
                    return;
                }

                SerializedPropertyGroup.BridgeProperty.AttrProperty initializerAlgo = _QueueAnnotation[cust_end].m_InitializerAlgo;
                int second;
                UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(initializerAlgo.ResetPage(), out second);
                Rect source = new Rect(ident);
                Rect rect = new Rect(ident.width - 29f, ident.y + 2f, 32f, 18f);
                source.width -= 42f;
                source.y += 2f;
                source.width /= 2f;
                EditorGUI.BeginChangeCheck();
                bool flag = false;
                if (animatorControllerParameter == null)
                {
                    EditorGUI.BeginChangeCheck();
                    int firstsize = EditorGUI.Popup(source, -1, _Schema);
                    if (EditorGUI.EndChangeCheck())
                    {
                        initializerAlgo.FlushPage(TestAnnotation(firstsize));
                    }

                    GUI.Label(new Rect(source)
                    {width = source.width - 5f, x = source.x + 5f}, initializerAlgo.ResetPage(), "minilabel");
                    source.x += source.width + 3f;
                    GUI.Label(source, "Parameter not found in Controller!");
                    source = new Rect(ident)
                    {x = ident.x + ident.width - 67f, y = ident.y + 2f, width = 40f, height = EditorGUIUtility.singleLineHeight};
                    int num = -1;
                    EditorGUI.BeginChangeCheck();
                    num = (int)(UnityEngine.AnimatorControllerParameterType)(object)EditorGUI.EnumPopup(source, (UnityEngine.AnimatorControllerParameterType)num);
                    source.x += 4f;
                    GUI.Label(source, "Add");
                    if (EditorGUI.EndChangeCheck())
                    {
                        string text = (string.IsNullOrEmpty(initializerAlgo.ResetPage()) ? "New Parameter" : initializerAlgo.ResetPage());
                        LogoutMapper().AddParameter(text, (UnityEngine.AnimatorControllerParameterType)num);
                        List<string> list = _Schema.ToList();
                        list.Add(text);
                        _Schema = list.ToArray();
                        initializerAlgo.FlushPage(text);
                        flag = true;
                    }
                }
                else
                {
                    initializerAlgo.FlushPage(TestAnnotation(EditorGUI.Popup(source, second, _Schema)));
                    source.x += source.width;
                    if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger && initializerAlgo.CreatePage() != SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set && (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Float || animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Int) && initializerAlgo.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Random)
                    {
                        source.width /= 3f;
                    }
                    else if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger || initializerAlgo.CreatePage() != SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set)
                    {
                        source.width /= 2f;
                    }

                    initializerAlgo.NewPage((SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType)(object)EditorGUI.EnumPopup(selected: (Enum)((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool && animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger) ? ((object)(VRCFullOptions)initializerAlgo.CreatePage()) : ((object)(VRCHalfOptions)initializerAlgo.CreatePage())), position: source));
                    source.x += source.width;
                    source.height -= 5f;
                    if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Bool || animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger)
                    {
                        if (initializerAlgo.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Add)
                        {
                            initializerAlgo.NewPage(SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set);
                        }

                        if (initializerAlgo.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set)
                        {
                            initializerAlgo.CustomizePage(Mathf.Clamp((int)initializerAlgo.ValidatePage(), 0, 1));
                            Enum selected = ((initializerAlgo.ValidatePage() == 1f) ? BoolModes.True : BoolModes.False);
                            if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
                            {
                                initializerAlgo.CustomizePage(((BoolModes)(object)EditorGUI.EnumPopup(source, selected) == BoolModes.True) ? 1 : 0);
                            }
                        }
                        else
                        {
                            EditorGUIUtility.labelWidth = 50f;
                            source.width -= 17f;
                            initializerAlgo.GetPage(EditorGUI.Slider(source, "Chance", initializerAlgo.DestroyPage() * 100f, 0f, 100f) / 100f);
                            source.x += source.width;
                            GUI.Label(source, "%", "boldlabel");
                            EditorGUIUtility.labelWidth = 0f;
                        }
                    }
                    else
                    {
                        bool flag2 = initializerAlgo.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Add;
                        if (initializerAlgo.CreatePage() == SerializedPropertyGroup.BridgeProperty.AttrProperty.ChangeType.Set || flag2)
                        {
                            EditorGUIUtility.labelWidth = 37f;
                            if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Int)
                            {
                                initializerAlgo.CustomizePage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Value"), (int)initializerAlgo.ValidatePage()), flag2 ? (-255) : 0, 255));
                            }
                            else
                            {
                                initializerAlgo.CustomizePage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Value"), initializerAlgo.ValidatePage()), -1f, 1f));
                            }

                            EditorGUIUtility.labelWidth = 0f;
                        }
                        else
                        {
                            EditorGUIUtility.labelWidth = 27f;
                            if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Int)
                            {
                                initializerAlgo.RunPage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Min"), (int)initializerAlgo.IncludePage()), 0, 255));
                                source.x += source.width;
                                initializerAlgo.ReflectPage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Max"), (int)initializerAlgo.LoginPage()), initializerAlgo.IncludePage(), 255f));
                            }
                            else
                            {
                                initializerAlgo.RunPage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Min"), initializerAlgo.IncludePage()), -1f, 1f));
                                source.x += source.width;
                                initializerAlgo.ReflectPage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Max"), initializerAlgo.LoginPage()), initializerAlgo.IncludePage(), 1f));
                            }

                            EditorGUIUtility.labelWidth = 0f;
                        }
                    }
                }

                if (EditorGUI.EndChangeCheck() || flag)
                {
                    _QueueAnnotation[cust_end].LogoutInitializer(initializerAlgo);
                    _QueueAnnotation[cust_end].m_InitializerAlgo = initializerAlgo;
                }

                if (GUI.Button(rect, ControllerEditorUtility.DestroyError()._TagProcessor, ControllerEditorUtility.CalcError()._TemplateProcessor))
                {
                    _QueueAnnotation[cust_end].PatchInitializer();
                    _QueueAnnotation.RemoveAt(cust_end);
                    CallAnnotation();
                }
            }
        }

        private static void DisableAnnotation(Rect spec)
        {
            GUI.Label(spec, "Shared VRCParameter Drivers");
            Rect rect = new Rect(spec);
            rect.x += rect.width - 80f;
            rect.width -= rect.x + 2f;
            int num = 0;
            foreach (SerializedPropertyGroup.BridgeProperty item in setterAnnotation)
            {
                switch (num)
                {
                    case 3:
                        if (!item.StartPage())
                        {
                            num = 2;
                        }

                        break;
                    case 1:
                        if (item.StartPage())
                        {
                            num = 2;
                        }

                        break;
                    case 0:
                        if (item.StartPage())
                        {
                            num = 3;
                        }
                        else if (!item.StartPage())
                        {
                            num = 1;
                        }

                        break;
                }

                if (num == 2)
                {
                    break;
                }
            }

            GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, num, Color.grey, Color.red, Color.yellow, Color.green);
            try
            {
                if (!GUI.Button(rect, "Local Only"))
                {
                    return;
                }

                switch (num)
                {
                    case 1:
                    case 2:
                    {
                        using List<SerializedPropertyGroup.BridgeProperty>.Enumerator enumerator = setterAnnotation.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.ReadPage(instanceinstall: true);
                        }

                        break;
                    }

                    case 3:
                    {
                        using List<SerializedPropertyGroup.BridgeProperty>.Enumerator enumerator = setterAnnotation.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.ReadPage(instanceinstall: false);
                        }

                        break;
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

        private static void InsertAnnotation(ReorderableList spec)
        {
            SerializedPropertyGroup.BridgeProperty.AttrProperty attrProperty = null;
            string def = "";
            if (_QueueAnnotation.Count > 0)
            {
                attrProperty = _QueueAnnotation.Last().m_InitializerAlgo;
            }
            else if ((bool)LogoutMapper() && LogoutMapper().parameters.Length != 0)
            {
                def = TestAnnotation(0);
            }

            for (int i = 0; i < m_AlgoAnnotation.Count; i++)
            {
                SerializedPropertyGroup.BridgeProperty bridgeProperty = null;
                for (int j = 0; j < m_AlgoAnnotation[i].behaviours.Length; j++)
                {
                    if (m_AlgoAnnotation[i].behaviours[j].GetType() == SerializedPropertyGroup.InsertPage())
                    {
                        bridgeProperty = new SerializedPropertyGroup.BridgeProperty(m_AlgoAnnotation[i].behaviours[j]);
                        break;
                    }
                }

                if (bridgeProperty == null)
                {
                    StateMachineBehaviour stateMachineBehaviour = (StateMachineBehaviour)ScriptableObject.CreateInstance(SerializedPropertyGroup.InsertPage());
                    stateMachineBehaviour.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(stateMachineBehaviour, LogoutMapper());
                    StateMachineBehaviour[] array = m_AlgoAnnotation[i].behaviours;
                    ArrayUtility.Add(ref array, stateMachineBehaviour);
                    m_AlgoAnnotation[i].behaviours = array;
                    bridgeProperty = new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour);
                }

                SerializedPropertyGroup.BridgeProperty.AttrProperty attrProperty2 = bridgeProperty.VisitPage();
                if (attrProperty == null)
                {
                    attrProperty2.FlushPage(def);
                }
                else
                {
                    RestartAnnotation(attrProperty, attrProperty2);
                }
            }

            CallAnnotation();
            PatchWrapper();
        }

        private static void RestartAnnotation(SerializedPropertyGroup.BridgeProperty.AttrProperty def, SerializedPropertyGroup.BridgeProperty.AttrProperty vis)
        {
            vis.InstantiatePage(validatekey: true);
            vis.GetPage(def.DestroyPage());
            vis.FlushPage(def.ResetPage());
            vis.CustomizePage(def.ValidatePage());
            vis.NewPage(def.CreatePage());
            vis.RunPage(def.IncludePage());
            vis.ReflectPage(def.LoginPage());
            vis.InstantiatePage(validatekey: false);
        }

        [SpecialName]
        private static int SetupInitializer()
        {
            return (int)m_ConsumerAnnotation.GetValue(ReadAnnotation());
        }

        [SpecialName]
        private static void EnableInitializer(int length_setup)
        {
            m_ConsumerAnnotation.SetValue(ReadAnnotation(), length_setup);
        }

        private static EditorWindow QueryAnnotation()
        {
            return UnityAnimatorBridge.RefTests.PopPolicy();
        }

        private static bool AddAnnotation()
        {
            while (true)
            {
                EditorWindow editorWindow = QueryAnnotation();
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            break;
                        case 2:
                            if (editorWindow != null)
                            {
                                goto IL_007a;
                            }

                            goto case 4;
                        default:
                            return (bool)helperAnnotation.GetValue(editorWindow);
                        case 4:
                            return false;
                        case 1:
                            goto default;
                    }

                    break;
                    IL_007a:
                        num = 1;
                }
            }
        }

        private static Animator InvokeAnnotation()
        {
            while (true)
            {
                EditorWindow editorWindow = QueryAnnotation();
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return null;
                        case 1:
                            if (editorWindow == null)
                            {
                                goto IL_00ac;
                            }

                            return (Animator)recordAnnotation.GetValue(editorWindow);
                        case 3:
                            break;
                        case 2:
                            goto case 1;
                    }

                    break;
                    IL_00ac:
                        num = 4;
                }
            }
        }

        private static object FindAnnotation()
        {
            object obj = QueryAnnotation();
            if (obj != null)
            {
                return _AdapterAnnotation.Invoke(obj, null);
            }

            return null;
        }

        private static object ExcludeAnnotation()
        {
            return Traverse.Create(FindAnnotation()).Property("edgeGUI").GetValue();
        }

        private static Vector3[] InitAnnotation(Edge last)
        {
            return (Vector3[])interpreterAnnotation.Invoke(ExcludeAnnotation(), new object[1]{last});
        }

        private static object VisitAnnotation()
        {
            if ((bool)LogoutMapper())
            {
                if ((object)QueryAnnotation() != null)
                {
                    return UnityAnimatorBridge.RefTests.InitPolicy();
                }

                return null;
            }

            return null;
        }

        private static void DefineAnnotation()
        {
            while (true)
            {
                int num;
                if (!LogoutMapper())
                {
                    num = 0;
                    goto IL_001d;
                }

                goto IL_005a;
                IL_001d:
                    switch (num)
                    {
                        case 2:
                            break;
                        case 4:
                            return;
                        case 1:
                            continue;
                        case 3:
                            return;
                        default:
                            return;
                    }

                goto IL_005a;
                IL_005a:
                    PrintMapper(UnityAnimatorBridge.RefTests.RemovePolicy());
                num = 3;
                goto IL_001d;
            }
        }

        private static object StartAnnotation()
        {
            object obj = VisitAnnotation();
            if (obj == null)
            {
                return null;
            }

            return _WatcherVisitor.GetMethod("get_stateMachineGraph", BindingFlags.Instance | BindingFlags.Public)?.Invoke(obj, null);
        }

        private static object ReadAnnotation()
        {
            EditorWindow editorWindow = QueryAnnotation();
            if (editorWindow == null)
            {
                return null;
            }

            object obj = contextAnnotation.GetValue(editorWindow);
            if (obj == null)
            {
                contextAnnotation.SetValue(editorWindow, obj = Activator.CreateInstance(_ReaderVisitor));
            }

            return obj;
        }

        private static void RemoveAnnotation()
        {
            OrderInitializer(UnityAnimatorBridge.RefTests.ReadPolicy());
        }

        private static void InstantiateAnnotation()
        {
            while (true)
            {
                int num;
                if ((object)QueryAnnotation() == null)
                {
                    num = 3;
                    goto IL_001d;
                }

                goto IL_003b;
                IL_001d:
                    switch (num)
                    {
                        default:
                            return;
                        case 2:
                            break;
                        case 4:
                            continue;
                        case 0:
                            return;
                        case 1:
                            return;
                        case 3:
                            return;
                    }

                goto IL_003b;
                IL_003b:
                    PatchMapper(UnityAnimatorBridge.RefTests.ConcatPolicy());
                num = 1;
                goto IL_001d;
            }
        }

        private static UnityEngine.AnimatorControllerParameter ResetAnnotation(string spec, out int second)
        {
            _003C_003Ec__DisplayClass121_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass121_0();
            CS_0024_003C_003E8__locals2._DecoratorDefinition = spec;
            if ((bool)LogoutMapper())
            {
                UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
                if (!parameters.ExcludeResolver((UnityEngine.AnimatorControllerParameter p) => p.name == CS_0024_003C_003E8__locals2._DecoratorDefinition, out second))
                {
                    return null;
                }

                return parameters[second];
            }

            second = -1;
            return null;
        }

        private static void FlushAnnotation()
        {
            _Method = (
                from t in ManageMapper().anyStateTransitions
                where t.isExit
                select t.name).ToArray();
        }

        private static void ConnectAnnotation()
        {
            if (!LogoutMapper())
            {
                return;
            }

            UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
            _Schema = new string[parameters.Length];
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                _Schema[i] = parameters[i].name;
                if (parameters[i].type != UnityEngine.AnimatorControllerParameterType.Bool)
                {
                    if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Float)
                    {
                        list.Add(parameters[i].name);
                    }
                }
                else
                {
                    list2.Add(parameters[i].name);
                }
            }

            _Proxy = list.ToArray();
            broadcaster = list2.ToArray();
        }

        private static void CalculateAnnotation()
        {
            while (true)
            {
                LoginVisitor();
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 3:
                            return;
                        case 4:
                            goto IL_004a;
                        case 2:
                            break;
                        case 1:
                            goto IL_004a;
                    }

                    break;
                    IL_004a:
                        PushVisitor();
                    num = 0;
                }
            }
        }

        private static string TestAnnotation(int firstsize)
        {
            return LogoutMapper().parameters[firstsize].name;
        }

        private static string MapAnnotation(string ident, Func<string, bool> caller)
        {
            if (!ControllerEditorUtility.m_AdapterProcessor.Contains(ident))
            {
                return ControllerEditorUtility.SortRules(ident, caller);
            }

            return ident;
        }

        private static void ValidateAnnotation(Action<object[]> key)
        {
            QuickInputWindow quickInputWindow = QuickInputWindow.CreateHelper("Animator QuickInput", new QuickInputWindow.FieldType[3]{QuickInputWindow.FieldType.Object, QuickInputWindow.FieldType.ToggleGroup, QuickInputWindow.FieldType.ToggleGroup}, new GUIContent[3]{new GUIContent("Target Animator"), new GUIContent("Use Once"), new GUIContent("Always Use")}, key, delegate (object[] arr)
            {
                bool[] array = new bool[arr.Length];
                array[0] = arr[0] == null;
                return array;
            });
            quickInputWindow.NewHelper(0, (!ControllerEditorSettingsWindow.m_AdvisorMapper) ? UnityEngine.Object.FindObjectsOfType<Animator>().FirstOrDefault((Animator a) => a.avatar) : ControllerEditorSettingsWindow.m_AdvisorMapper);
            quickInputWindow.PushHelper(0, typeof(Animator));
            quickInputWindow.NewHelper(1, true);
            quickInputWindow.comparatorPolicy = new bool[3]{false, true, false};
            quickInputWindow.CollectHelper(m_ContainerVisitor);
        }

        internal static void CustomizeAnnotation(string config)
        {
            FindVisitor(config);
        }

        internal static bool RateAnnotation(bool isi, string selection)
        {
            return FindVisitor(selection, CustomLogType.Warning, isi);
        }

        internal static void DestroyAnnotation(string task)
        {
            FindVisitor(task, CustomLogType.Error);
        }

        private static void GetAnnotation(bool updatereference)
        {
            ControllerEditorSettings.WorkerAlgo workerAlgo = new ControllerEditorSettings.WorkerAlgo();
            try
            {
                ControllerEditorSettings.BoolSetting editingTransitions = AddInitializer().editingTransitions;
                ControllerEditorSettings.BoolSetting editingStates = AddInitializer().editingStates;
                ControllerEditorSettings.BoolSetting editingController = AddInitializer().editingController;
                ControllerEditorSettings.BoolSetting showTransitionSettings = AddInitializer().showTransitionSettings;
                ControllerEditorSettings.BoolSetting showTransitionConditions = AddInitializer().showTransitionConditions;
                ControllerEditorSettings.BoolSetting showTransitionsCount = AddInitializer().showTransitionsCount;
                ControllerEditorSettings.BoolSetting showStateCount = AddInitializer().showStateCount;
                ControllerEditorSettings.BoolSetting showStateSettings = AddInitializer().showStateSettings;
                ControllerEditorSettings.BoolSetting showVRCDrivers = AddInitializer().showVRCDrivers;
                bool flag;
                AddInitializer().showVRCTracking.ExcludeDefinition(flag = updatereference);
                bool flag2;
                showVRCDrivers.ExcludeDefinition(flag2 = flag);
                bool flag3;
                showStateSettings.ExcludeDefinition(flag3 = flag2);
                bool flag4;
                showStateCount.ExcludeDefinition(flag4 = flag3);
                bool flag5;
                showTransitionsCount.ExcludeDefinition(flag5 = flag4);
                bool flag6;
                showTransitionConditions.ExcludeDefinition(flag6 = flag5);
                bool flag7;
                showTransitionSettings.ExcludeDefinition(flag7 = flag6);
                bool flag8;
                editingController.ExcludeDefinition(flag8 = flag7);
                bool excludeparam;
                editingStates.ExcludeDefinition(excludeparam = flag8);
                editingTransitions.ExcludeDefinition(excludeparam);
            }
            finally
            {
                if (workerAlgo != null)
                {
                    ((IDisposable)workerAlgo).Dispose();
                }
            }
        }

        internal static void CalcAnnotation(Rect param, string second)
        {
            if (!AddInitializer().displayParameterType)
            {
                return;
            }

            GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, AddInitializer().parameterLabelColor);
            try
            {
                ControllerEditorUtility.ConnectQueue(param, second, overridethird: true, 0f, 1f, isparam4: false, ControllerEditorSettings.m_AdapterAlgo);
            }
            finally
            {
                if (templateThread != null)
                {
                    ((IDisposable)templateThread).Dispose();
                }
            }
        }

        internal static void IncludeAnnotation(bool calci)
        {
            while (calci)
            {
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            return;
                        case 1:
                            break;
                        case 4:
                            return;
                        case 2:
                            goto IL_009a;
                        default:
                            goto IL_009a;
                    }

                    break;
                    IL_009a:
                        ControllerEditorUtility.MapQueue();
                    num = 4;
                }
            }
        }

        private static string RunAnnotation(string i, int indexOfpred)
        {
            if (i.Length <= indexOfpred)
            {
                return i;
            }

            return i.Substring(0, indexOfpred - 3) + "...";
        }

        private static string CloneAnnotation(string setup, int endcont, int tag_Z)
        {
            if (setup.Length <= endcont + tag_Z + 3)
            {
                return setup;
            }

            return setup.Substring(0, endcont) + "..." + setup.Substring(setup.Length - tag_Z, tag_Z);
        }

        private static void LoginAnnotation(float asset = 8f)
        {
            GUILayout.Label(string.Empty, new GUIStyle(GUI.skin.verticalSlider)
            {margin = new RectOffset(), padding = new RectOffset(), stretchHeight = true}, GUILayout.Width(asset));
        }

        private static void ReflectAnnotation(string value)
        {
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(ControllerEditorUtility.CalcError()._WrapperObserver);
            try
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(value, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }
        }

        private static void DeleteAnnotation(Action def, string token, ControllerEditorSettings.BoolSetting res, bool iscont2, int visitor3counter)
        {
            if ((bool)res)
            {
                EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope();
                try
                {
                    EditorGUILayout.VerticalScope verticalScope = new EditorGUILayout.VerticalScope(iscont2 ? GUI.skin.box : GUIStyle.none);
                    try
                    {
                        def();
                    }
                    finally
                    {
                        if (verticalScope != null)
                        {
                            ((IDisposable)verticalScope).Dispose();
                        }
                    }

                    string text = $"CollapsePart{visitor3counter}";
                    ControllerEditorUtility.CompareQueue(text, GUILayoutUtility.GetLastRect().height, EventType.Repaint);
                    if (ControllerEditorUtility.DisableQueue(string.Empty, GUILayout.Height(ControllerEditorUtility.OrderQueue(text, 0f)), GUILayout.Width(7f)))
                    {
                        res.InsertDefinition();
                    }

                    return;
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            }

            if (ControllerEditorUtility.CountQueue(token, EditorStyles.toolbarButton))
            {
                res.InsertDefinition();
            }
        }

        private static bool CreateAnnotation(UnityEngine.Object asset, params GUILayoutOption[] options)
        {
            return NewAnnotation(asset, string.Empty, options);
        }

        private static bool NewAnnotation(UnityEngine.Object v, string map = "", params GUILayoutOption[] options)
        {
            bool flag = default(bool);
            while (true)
            {
                int num;
                if (!string.IsNullOrEmpty(map))
                {
                    num = 1;
                    goto IL_001d;
                }

                goto IL_011e;
                IL_001d:
                    switch (num)
                    {
                        case 2:
                            continue;
                        case 1:
                        case 4:
                            flag = Selection.activeObject == v;
                            goto case 3;
                        case 5:
                            goto IL_011e;
                        case 3:
                        case 6:
                        {
                            bool num2 = ControllerEditorUtility.CountQueue(map, flag ? ControllerEditorUtility.CalcError().m_MockProcessor : GUI.skin.label, options);
                            if (num2)
                            {
                                Selection.activeObject = (flag ? null : v);
                            }

                            return num2;
                        }
                    }

                goto IL_007b;
                IL_011e:
                    ;
                goto IL_007b;
                IL_007b:
                    map = v.name;
                num = 4;
                goto IL_001d;
            }
        }

        private static void PushAnnotation()
        {
            FlushAnnotation();
        }

        private static void ViewAnnotation()
        {
            RestartVisitor();
        }

        private static void CollectAnnotation()
        {
            DisableMapper();
        }

        private static void RevertAnnotation()
        {
        }

        private static bool OrderVisitor(EditorWindow value = null, float map = 0f)
        {
            return true;
        }

        private static void RestartVisitor()
        {
            Type[] array = infoAnnotation;
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(array[i]);
                foreach (UnityEngine.Object obj in array2)
                {
                    if (!(obj is EditorWindow editorWindow))
                    {
                        if (obj is Editor editor)
                        {
                            editor.Repaint();
                        }
                    }
                    else
                    {
                        editorWindow.Repaint();
                    }
                }
            }
        }

        internal static bool FindVisitor(string param, CustomLogType selection = CustomLogType.Regular, bool setrule = true)
        {
            if (setrule)
            {
                string message = "<color=#" + ColorUtility.ToHtmlStringRGB(selection switch
                {
                    CustomLogType.Warning => ControllerEditorUtility._WrapperProcessor,
                    CustomLogType.Regular => ControllerEditorUtility.configurationProperty,
                    _ => ControllerEditorUtility._ProcProperty,
                }) + ">[Controllers+]</color> " + param.Replace("\\n", "\n");
                switch (selection)
                {
                    case CustomLogType.Warning:
                        UnityEngine.Debug.LogWarning(message);
                        break;
                    case CustomLogType.Error:
                        UnityEngine.Debug.LogError(message);
                        break;
                    case CustomLogType.Regular:
                        UnityEngine.Debug.Log(message);
                        break;
                }
            }

            return setrule;
        }

        private static void DefineVisitor(Action item = null, Action<GenericMenu> cont = null)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    ReadVisitor(cont);
                item?.Invoke();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Created by Fynn9563", "Open Fynn9563 on GitHub"), EditorStyles.miniLabel, GUILayout.ExpandWidth(false)))
                    Application.OpenURL("https://github.com/Fynn9563");
            }
        }

        private static void ReadVisitor(Action<GenericMenu> init = null)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Settings"), false, ControllerEditorSettingsWindow.CalcTests);
            menu.ShowAsContext();
        }

        [SpecialName]
        private static ControllerEditorSettings AddInitializer()
        {
            return ControllerEditorSettings.CallDefinition();
        }

        private static void CalculateVisitor(Rect spec)
        {
            EditorGUI.DisabledScope disabledScope;
            GuiEnabledScope annotationPolicy;
            if (_ProcessorAnnotation.itemTests == null)
            {
                annotationPolicy = new GuiEnabledScope(iskey: false);
                try
                {
                    Rect item = new Rect(spec);
                    item.width = 16f;
                    item.x = spec.x - 3f;
                    item.y = spec.y + 2f;
                    if (ControllerEditorUtility.QueryQueue(item, ControllerEditorUtility.DestroyError().dispatcherProcessor, GUIStyle.none))
                    {
                        specification = !specification;
                        MapVisitor();
                    }
                }
                finally
                {
                    if (annotationPolicy != null)
                    {
                        ((IDisposable)annotationPolicy).Dispose();
                    }
                }

                spec.x += 12f;
                GUI.Label(spec, specification ? "Shared Conditions" : "All Conditions");
                spec.x -= 12f;
            }
            else
            {
                Rect item2 = new Rect(spec);
                item2.width = 18f;
                using (new EditorGUI.DisabledScope(_ProcessorAnnotation.itemTests.conditions.Length == 0))
                {
                    if (ControllerEditorUtility.QueryQueue(item2, ControllerEditorUtility.DestroyError().m_BridgeProcessor, GUI.skin.label))
                    {
                        SortVisitor();
                    }
                }

                item2.x += 20f;
                disabledScope = new EditorGUI.DisabledScope(m_ThreadAnnotation.Count == 0);
                try
                {
                    if (ControllerEditorUtility.QueryQueue(item2, ControllerEditorUtility.DestroyError().m_StrategyProcessor, GUI.skin.label))
                    {
                        RegisterVisitor();
                    }
                }
                finally
                {
                    ((IDisposable)disabledScope /*cast due to .constrained prefix*/).Dispose();
                }

                item2.x += 20f;
                item2.width = spec.width - 40f;
                GUI.Label(item2, _ProcessorAnnotation.VisitSerializer() + "'s Conditions");
            }

            Rect item3 = new Rect(spec);
            item3.x += 95f;
            if (specification)
            {
                item3.x += 29f;
            }

            item3.width = 18f;
            if (!_ProcessorAnnotation.itemTests)
            {
                disabledScope = new EditorGUI.DisabledScope((specification && _TokenAnnotation.Count == 0) || (!specification && m_CodeAnnotation.Count == 0));
                try
                {
                    if (ControllerEditorUtility.QueryQueue(item3, ControllerEditorUtility.DestroyError().m_BridgeProcessor, GUI.skin.label))
                    {
                        SortVisitor();
                    }
                }
                finally
                {
                    ((IDisposable)disabledScope /*cast due to .constrained prefix*/).Dispose();
                }

                item3.x += 20f;
                using (new EditorGUI.DisabledScope(m_ThreadAnnotation.Count == 0))
                {
                    if (ControllerEditorUtility.QueryQueue(item3, ControllerEditorUtility.DestroyError().m_StrategyProcessor, GUI.skin.label))
                    {
                        RegisterVisitor();
                    }
                }
            }
            else
            {
                item3.x += 20f;
            }

            item3 = new Rect(spec);
            item3.y += 2f;
            item3.x += spec.width / 2f + spec.width / 8f - 25f;
            item3.width = 15f;
            EditorGUI.BeginDisabledGroup((!_ProcessorAnnotation.itemTests && ((specification && _TokenAnnotation.Count == 0) || (!specification && m_CodeAnnotation.Count == 0))) || (_ProcessorAnnotation.itemTests != null && _ProcessorAnnotation.itemTests.conditions.Length < 1));
            if (ControllerEditorUtility.QueryQueue(item3, ControllerEditorUtility.DestroyError().m_ExporterProcessor, GUIStyle.none))
            {
                if (_ProcessorAnnotation.itemTests != null)
                {
                    using (List<GlobalVisitor>.Enumerator enumerator = m_DicAnnotation.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GlobalVisitor current = enumerator.Current;
                            AnimatorCondition animatorCondition = ResolveAlgo(current.m_ProcessVisitor);
                            current.ResolveInitializer(animatorCondition);
                            current.m_ProcessVisitor = animatorCondition;
                        }
                    }

                    ChangeVisitor();
                    _TokenAnnotation = AssetVisitor(propertyAnnotation);
                }
                else if (specification)
                {
                    using (List<GlobalVisitor>.Enumerator enumerator = _TokenAnnotation.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.ForgotInitializer();
                        }
                    }

                    ChangeVisitor();
                }
                else
                {
                    using (List<GlobalVisitor>.Enumerator enumerator = m_CodeAnnotation.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.ForgotInitializer();
                        }
                    }

                    UpdateVisitor();
                }
            }

            EditorGUI.EndDisabledGroup();
            spec.x += spec.width - 52f;
            spec.width = 15f;
            annotationPolicy = new GuiEnabledScope(iskey: false);
            try
            {
                if (ControllerEditorUtility.QueryQueue(spec, new GUIContent(ControllerEditorUtility.DestroyError()._RequestProcessor)
                {tooltip = "Toggles custom matching options"}, GUIStyle.none))
                {
                    AddInitializer().showMatchingOptions.InsertDefinition();
                }
            }
            finally
            {
                if (annotationPolicy != null)
                {
                    ((IDisposable)annotationPolicy).Dispose();
                }
            }

            spec.x += 17f;
            using (new EditorGUI.DisabledScope(_ProcessorAnnotation.itemTests))
            {
                if (ControllerEditorUtility.QueryQueue(spec, ControllerEditorUtility.DestroyError()._AttrProcessor, GUIStyle.none))
                {
                    ConnectAlgo();
                }
            }

            spec.x += 17f;
            if (ControllerEditorUtility.QueryQueue(spec, ControllerEditorUtility.DestroyError()._IdentifierProcessor, GUIStyle.none))
            {
                ViewAlgo();
            }
        }

        private static void TestVisitor(Rect def, int boffset, bool isconsumer, bool dotoken2)
        {
            _003C_003Ec__DisplayClass285_0 CS_0024_003C_003E8__locals58 = new _003C_003Ec__DisplayClass285_0();
            if (!LogoutMapper())
            {
                return;
            }

            List<GlobalVisitor> list = StopVisitor();
            if (boffset >= list.Count || boffset < 0)
            {
                return;
            }

            GlobalVisitor globalVisitor = list[boffset];
            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition = globalVisitor.m_ProcessVisitor;
            int second;
            UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter, out second);
            bool flag = false;
            bool flag2 = false;
            bool num = animatorControllerParameter == null;
            CS_0024_003C_003E8__locals58.publisherDefinition = $"ConditionParameterField{boffset}";
            CS_0024_003C_003E8__locals58._IteratorDefinition = GUI.GetNameOfFocusedControl() == CS_0024_003C_003E8__locals58.publisherDefinition;
            Rect source = new Rect(def.width - 22f, def.y + 2f, 32f, 18f);
            Rect source2 = (num ? new Rect(source)
            {width = 60f, x = source.x - 60f} : Rect.zero);
            Rect def2 = new Rect(def.x, def.y + 2f, def.width - source2.width - 40f, EditorGUIUtility.singleLineHeight);
            if (num)
            {
                Rect rect = def2.SortResolver(50f);
                Rect rect2 = new Rect(rect);
                rect2.x = rect.x + 3f;
                rect2.width = rect.width - 3f;
                Rect rect3 = rect2;
                def2.SortResolver(3f, isfield: true);
                Rect rect4 = def2.SortResolver(100f);
                CS_0024_003C_003E8__locals58.CompareServer(rect);
                if (!CS_0024_003C_003E8__locals58._IteratorDefinition)
                {
                    MixedValueScope managerThread = new MixedValueScope(globalVisitor.iteratorVisitor[0]);
                    try
                    {
                        EditorGUI.BeginChangeCheck();
                        int firstsize = EditorGUI.Popup(rect, -1, _Schema);
                        if (EditorGUI.EndChangeCheck())
                        {
                            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter = TestAnnotation(firstsize);
                            CS_0024_003C_003E8__locals58.m_ProcDefinition = true;
                        }
                    }
                    finally
                    {
                        if (managerThread != null)
                        {
                            ((IDisposable)managerThread).Dispose();
                        }
                    }

                    GUI.Label(rect3, CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter);
                }

                GUI.Label(rect4, "Parameter not found in Controller!");
                int num2 = -1;
                EditorGUI.BeginChangeCheck();
                num2 = (int)(UnityEngine.AnimatorControllerParameterType)(object)EditorGUI.EnumPopup(source2, (UnityEngine.AnimatorControllerParameterType)num2);
                rect2 = new Rect(source2);
                rect2.x = source2.x + 3f;
                GUI.Label(rect2, "Add");
                if (EditorGUI.EndChangeCheck())
                {
                    string text = (string.IsNullOrEmpty(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter) ? "New Parameter" : CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter);
                    LogoutMapper().AddParameter(text, (UnityEngine.AnimatorControllerParameterType)num2);
                    ArrayUtility.Add(ref _Schema, text);
                    CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter = text;
                    CS_0024_003C_003E8__locals58.m_ProcDefinition = true;
                }
            }
            else
            {
                Rect rect5 = def2.SortResolver(20f, isfield: true);
                Rect rect6 = def2.SortResolver((animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger) ? 100 : 50);
                Rect rect7 = new Rect(rect6)
                {width = 20f, x = rect6.x + rect6.width - 40f};
                Rect rect8 = def2.SortResolver((animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Bool) ? 100 : 50);
                Rect rect9 = def2.SortResolver(100f);
                if (GUI.Button(rect5, ControllerEditorUtility.DestroyError().m_ValProcessor, ControllerEditorUtility.CalcError().configProcessor))
                {
                    _003C_003Ec__DisplayClass285_1 CS_0024_003C_003E8__locals52 = new _003C_003Ec__DisplayClass285_1();
                    IEnumerable<IEnumerable<AnimatorStateTransition>> first = RevertMapper().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition, c, forcetag: true))));
                    CS_0024_003C_003E8__locals52.mapperReg = new List<AnimatorTransitionBase>();
                    first.InvokeResolver(delegate (IEnumerable<AnimatorStateTransition> e)
                    {
                        CS_0024_003C_003E8__locals52.mapperReg.AddRange(e);
                    });
                    CS_0024_003C_003E8__locals52.mapperReg.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition, c, forcetag: true))));
                    CS_0024_003C_003E8__locals52.mapperReg.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition, c, forcetag: true))));
                    Selection.objects = Selection.objects.Concat(CS_0024_003C_003E8__locals52.mapperReg).Distinct().ToArray();
                }

                if (GUI.Button(rect7, GUIContent.none, GUIStyle.none))
                {
                    _003C_003Ec__DisplayClass285_2 CS_0024_003C_003E8__locals59 = new _003C_003Ec__DisplayClass285_2();
                    CS_0024_003C_003E8__locals59.initializerReg = CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter;
                    IEnumerable<IEnumerable<AnimatorStateTransition>> first2 = RevertMapper().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == CS_0024_003C_003E8__locals59.initializerReg)));
                    CS_0024_003C_003E8__locals59.m_DefinitionReg = new List<AnimatorTransitionBase>();
                    first2.InvokeResolver(delegate (IEnumerable<AnimatorStateTransition> e)
                    {
                        CS_0024_003C_003E8__locals59.m_DefinitionReg.AddRange(e);
                    });
                    CS_0024_003C_003E8__locals59.m_DefinitionReg.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == CS_0024_003C_003E8__locals59.initializerReg)));
                    CS_0024_003C_003E8__locals59.m_DefinitionReg.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == CS_0024_003C_003E8__locals59.initializerReg)));
                    Selection.objects = Selection.objects.Concat(CS_0024_003C_003E8__locals59.m_DefinitionReg).Distinct().ToArray();
                }

                CS_0024_003C_003E8__locals58.CompareServer(rect6);
                if (!CS_0024_003C_003E8__locals58.m_ProcDefinition && !CS_0024_003C_003E8__locals58._IteratorDefinition)
                {
                    MixedValueScope managerThread = new MixedValueScope(globalVisitor.iteratorVisitor[0]);
                    try
                    {
                        if (!AddInitializer().useLegacyDropdown)
                        {
                            object[] parameters = new object[3]{rect6, second, _Schema};
                            int num3 = (int)_ManagerVisitor.Invoke(null, parameters);
                            if (num3 != second)
                            {
                                CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter = TestAnnotation(num3);
                                CS_0024_003C_003E8__locals58.m_ProcDefinition = true;
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter = TestAnnotation(EditorGUI.Popup(rect6, second, _Schema));
                            if (EditorGUI.EndChangeCheck())
                            {
                                CS_0024_003C_003E8__locals58.m_ProcDefinition = true;
                            }
                        }
                    }
                    finally
                    {
                        if (managerThread != null)
                        {
                            ((IDisposable)managerThread).Dispose();
                        }
                    }
                }

                GUI.Label(rect7, ControllerEditorUtility.DestroyError().m_ValProcessor, GUIStyle.none);
                if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
                {
                    MixedValueScope managerThread = new MixedValueScope(globalVisitor.iteratorVisitor[1]);
                    try
                    {
                        EditorGUI.BeginChangeCheck();
                        Enum selected;
                        if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Int)
                        {
                            selected = (IntModes)CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode;
                            selected = EditorGUI.EnumPopup(rect8, selected);
                        }
                        else if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool)
                        {
                            selected = (FloatModes)CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode;
                            selected = EditorGUI.EnumPopup(rect8, selected);
                        }
                        else
                        {
                            selected = (BoolModes)CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode;
                            selected = EditorGUI.EnumPopup(rect8, selected);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode = (AnimatorConditionMode)(object)selected;
                            flag = true;
                        }
                    }
                    finally
                    {
                        if (managerThread != null)
                        {
                            ((IDisposable)managerThread).Dispose();
                        }
                    }

                    managerThread = new MixedValueScope(globalVisitor.iteratorVisitor[2]);
                    try
                    {
                        EditorGUI.BeginChangeCheck();
                        switch (animatorControllerParameter.type)
                        {
                            case UnityEngine.AnimatorControllerParameterType.Int:
                                GUI.SetNextControlName("Threshold" + _Attribute);
                                _Attribute++;
                                CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.threshold = EditorGUI.IntField(rect9, (int)CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.threshold);
                                break;
                            case UnityEngine.AnimatorControllerParameterType.Float:
                                GUI.SetNextControlName("Threshold" + _Attribute);
                                _Attribute++;
                                CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.threshold = EditorGUI.FloatField(rect9, CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.threshold);
                                break;
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            flag2 = true;
                        }
                    }
                    finally
                    {
                        if (managerThread != null)
                        {
                            ((IDisposable)managerThread).Dispose();
                        }
                    }
                }

                if (animatorControllerParameter.type > UnityEngine.AnimatorControllerParameterType.Int)
                {
                    if (CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode > AnimatorConditionMode.IfNot)
                    {
                        CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode = AnimatorConditionMode.If;
                        flag = true;
                        goto IL_17fc;
                    }
                }

                switch (animatorControllerParameter.type)
                {
                    case UnityEngine.AnimatorControllerParameterType.Float:
                        if (((int)CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode).GetResolver(3, 5))
                        {
                            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode = AnimatorConditionMode.Greater;
                            flag = true;
                        }

                        break;
                    case UnityEngine.AnimatorControllerParameterType.Int:
                        if (CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode < AnimatorConditionMode.Greater)
                        {
                            CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode = AnimatorConditionMode.Equals;
                            flag = true;
                        }

                        break;
                }
            }

            goto IL_17fc;
            IL_17fc:
                ;
            if (CS_0024_003C_003E8__locals58.m_ProcDefinition)
            {
                globalVisitor.VerifyInitializer(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.parameter);
            }

            if (flag)
            {
                globalVisitor.FillInitializer(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.mode);
            }

            if (flag2)
            {
                globalVisitor.WriteInitializer(CS_0024_003C_003E8__locals58.m_ConfigurationDefinition.threshold);
            }

            if (!GUI.Button(source, ControllerEditorUtility.DestroyError()._TagProcessor, ControllerEditorUtility.CalcError()._TemplateProcessor))
            {
                return;
            }

            globalVisitor.StopInitializer();
            if (_ProcessorAnnotation.itemTests == null)
            {
                if (specification)
                {
                    UpdateVisitor();
                    return;
                }

                m_CodeAnnotation.RemoveAt(boffset);
                MapVisitor();
            }
        }

        private static void MapVisitor()
        {
            if (_ProcessorAnnotation.itemTests != null)
            {
                m_DicAnnotation = PrepareVisitor(_ProcessorAnnotation.itemTests);
                m_ParamAnnotation = new ReorderableList(m_DicAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
                {drawElementCallback = TestVisitor, drawHeaderCallback = CalculateVisitor, onAddCallback = FillVisitor};
            }
            else if (specification)
            {
                invocationAnnotation = new ReorderableList(_TokenAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
                {drawElementCallback = TestVisitor, drawHeaderCallback = CalculateVisitor, onAddCallback = FillVisitor};
            }
            else
            {
                _RoleAnnotation = new ReorderableList(m_CodeAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, _DefinitionAnnotation.Count == 1, displayRemoveButton: false)
                {drawElementCallback = TestVisitor, drawHeaderCallback = CalculateVisitor, onAddCallback = FillVisitor};
            }
        }

        private void ValidateVisitor()
        {
            if (!AddInitializer().editingController)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(LogoutMapper() == null))
            {
                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(GUI.skin.box);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    bool flag4 = false;
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        m_GlobalAnnotation = (ControllerAction)(object)EditorGUILayout.EnumPopup(m_GlobalAnnotation, GUILayout.Width(ControllerEditorUtility.PostQueue(m_GlobalAnnotation) + 28f));
                        switch (m_GlobalAnnotation)
                        {
                            case ControllerAction.TagCurrentLayerWith:
                                flag3 = true;
                                break;
                            case ControllerAction.RemoveTag:
                                flag = true;
                                flag4 = true;
                                break;
                            case ControllerAction.RemoveLayersWithTag:
                                flag3 = true;
                                break;
                            case ControllerAction.RemoveParameter:
                                flag = true;
                                flag4 = true;
                                break;
                            case ControllerAction.ReplaceParameter:
                                flag = true;
                                flag2 = true;
                                flag4 = true;
                                break;
                        }

                        if (flag4 && _TaskAnnotation == ActionMode.LayersTaggedWith)
                        {
                            flag3 = true;
                        }

                        if (flag)
                        {
                            EditorGUIUtility.labelWidth = 40f;
                            _SchemaAnnotation = EditorGUILayout.TextField("", _SchemaAnnotation, "textfielddropdowntext");
                            EditorGUIUtility.labelWidth = 0f;
                            int selectedIndex = -1;
                            EditorGUI.BeginChangeCheck();
                            selectedIndex = EditorGUILayout.Popup(selectedIndex, _Schema ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                _SchemaAnnotation = _Schema[selectedIndex];
                            }
                        }

                        if (flag3 && _TaskAnnotation != ActionMode.LayersTaggedWith)
                        {
                            proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
                        }

                        if (flag2)
                        {
                            GUILayout.Label("With", GUILayout.Width(32f));
                            EditorGUIUtility.labelWidth = 40f;
                            m_BroadcasterAnnotation = EditorGUILayout.TextField("", m_BroadcasterAnnotation, "textfielddropdowntext");
                            EditorGUIUtility.labelWidth = 0f;
                            int selectedIndex2 = -1;
                            EditorGUI.BeginChangeCheck();
                            selectedIndex2 = EditorGUILayout.Popup(selectedIndex2, _Schema ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                m_BroadcasterAnnotation = _Schema[selectedIndex2];
                            }
                        }

                        if (flag4)
                        {
                            GUILayout.Label("In", GUILayout.Width(15f));
                            _TaskAnnotation = (ActionMode)(object)EditorGUILayout.EnumPopup(_TaskAnnotation, GUILayout.Width(140f));
                        }

                        if (flag3 && _TaskAnnotation == ActionMode.LayersTaggedWith)
                        {
                            proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
                        }

                        if (m_GlobalAnnotation == ControllerAction.Copy)
                        {
                            m_ProcessAnnotation = (MoveMode)(object)EditorGUILayout.EnumPopup(m_ProcessAnnotation, GUILayout.Width(ControllerEditorUtility.PostQueue(m_ProcessAnnotation) + 28f));
                            if (m_ProcessAnnotation == MoveMode.LayersTaggedWith)
                            {
                                flag3 = true;
                                proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
                            }

                            GUILayout.Label("To", GUILayout.Width(20f));
                            producerAnnotation = (MoveDestination)(object)EditorGUILayout.EnumPopup(producerAnnotation, GUILayout.Width(ControllerEditorUtility.PostQueue(producerAnnotation) + 28f));
                            if (producerAnnotation == MoveDestination.Controller)
                            {
                                m_MethodAnnotation = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(m_MethodAnnotation, typeof(UnityEditor.Animations.AnimatorController), false);
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
                        if (m_GlobalAnnotation != ControllerAction.RemoveParameter)
                        {
                            if (m_GlobalAnnotation != ControllerAction.ReplaceParameter)
                            {
                                if (m_GlobalAnnotation != ControllerAction.Copy)
                                {
                                    GUILayout.FlexibleSpace();
                                }
                                else
                                {
                                    serviceAnnotation = EditorGUILayout.Toggle(new GUIContent("Add Required Parameters", "Add the parameters used by the Source to the destination Controller. Adds Suffix if Suffix isn't empty."), serviceAnnotation, GUILayout.Width(180f));
                                    GUILayout.FlexibleSpace();
                                    EditorGUIUtility.labelWidth = 50f;
                                    _StateAnnotation = EditorGUILayout.TextField(new GUIContent("Suffix:", "Add a Suffix to all the Parameters in the newly copied layers. Adds a Suffix to the added parameters if enabled."), _StateAnnotation);
                                    EditorGUIUtility.labelWidth = 0f;
                                }

                                goto IL_0d17;
                            }
                        }

                        structAnnotation = EditorGUILayout.Toggle(new GUIContent("Match Whole Word", "Apply to parameters that match exactly. Otherwise apply to parameters that contain it"), structAnnotation);
                        goto IL_0d17;
                        IL_0d17:
                            EditorGUI.BeginDisabledGroup((string.IsNullOrEmpty(_SchemaAnnotation) && flag) || (string.IsNullOrEmpty(m_BroadcasterAnnotation) && flag2) || (string.IsNullOrEmpty(proxyAnnotation) && flag3) || (m_GlobalAnnotation == ControllerAction.Copy && producerAnnotation == MoveDestination.Controller && !m_MethodAnnotation));
                        if (ControllerEditorUtility.CountQueue("Apply", "minibutton", GUILayout.Width(140f)))
                        {
                            LogoutVisitor();
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    finally
                    {
                        if (horizontalScope != null)
                        {
                            ((IDisposable)horizontalScope).Dispose();
                        }
                    }

                    EditorGUILayout.Space();
                    ControllerEditorUtility.MapQueue();
                    EditorGUILayout.Space();
                    CustomizeVisitor();
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }
            }
        }

        private void CustomizeVisitor()
        {
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                EditorGUI.BeginChangeCheck();
                _Client = ControllerEditorUtility.AddQueue(_Client, "Write Defaults", "toolbarbutton");
                if (EditorGUI.EndChangeCheck())
                {
                    WriteMapper();
                }

                EditorGUI.BeginChangeCheck();
                m_Config = ControllerEditorUtility.AddQueue(m_Config, "Explore Controller Sub-Assets", "toolbarbutton");
                if (EditorGUI.EndChangeCheck())
                {
                    FillMapper();
                }

                if (ControllerEditorUtility.RestartQueue(new GUIContent("Cleanup unused Sub-Assets", "Some Controllers have residue in their Sub-Assets that may be unused, may happen when using this tool. Use this button to clean it up."), "toolbarbutton") && (bool)LogoutMapper())
                {
                    SearchVisitor(LogoutMapper());
                    VerifyMapper();
                }
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            if (m_Config)
            {
                _ModelAnnotation = GUILayout.Toolbar(_ModelAnnotation, ControllerEditorUtility.DestroyError().m_FacadeProcessor, EditorStyles.toolbarButton);
                switch (_ModelAnnotation)
                {
                    case 2:
                        _Service.structVisitor.ForEach(RateVisitor);
                        break;
                    case 3:
                        _Service._SchemaVisitor.ForEach(RateVisitor);
                        break;
                    case 5:
                        _Service.stateVisitor.ForEach(RateVisitor);
                        break;
                    case 1:
                        _Service.m_ServiceVisitor.ForEach(RateVisitor);
                        break;
                    case 4:
                        _Service.broadcasterVisitor.ForEach(RateVisitor);
                        break;
                    case 0:
                        _Service._ProxyVisitor.ForEach(RateVisitor);
                        break;
                }
            }
            else
            {
                if (!_Client)
                {
                    return;
                }

                if (!LogoutMapper())
                {
                    VerifyMapper();
                }

                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope();
                    try
                    {
                        if (ControllerEditorUtility.DisableQueue("Set All On"))
                        {
                            ForgotMapper(istask: true);
                        }

                        EditorGUILayout.Space();
                        ReflectAnnotation("Write Defaults On");
                        IEnumerator<AnimatorState> enumerator = _Service.m_ServiceVisitor.Where(delegate (AnimatorState s)
                        {
                            if ((bool)s)
                            {
                                return s.writeDefaultValues;
                            }

                            return false;
                        }).GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                AnimatorState current = enumerator.Current;
                                GUILayout.HorizontalScope horizontalScope2 = new GUILayout.HorizontalScope();
                                try
                                {
                                    CreateAnnotation(current, GUILayout.ExpandWidth(expand: true));
                                    if (ControllerEditorUtility.DisableQueue(">", GUILayout.ExpandWidth(expand: false)))
                                    {
                                        StopMapper(current, connectionreguired: false);
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
                        }
                        finally
                        {
                            if (enumerator != null)
                            {
                                enumerator.Dispose();
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

                    LoginAnnotation();
                    verticalScope = new GUILayout.VerticalScope();
                    try
                    {
                        if (ControllerEditorUtility.DisableQueue("Set All Off"))
                        {
                            ForgotMapper(istask: false);
                        }

                        EditorGUILayout.Space();
                        ReflectAnnotation("Write Defaults Off");
                        IEnumerator<AnimatorState> enumerator = _Service.m_ServiceVisitor.Where(delegate (AnimatorState s)
                        {
                            if ((bool)s)
                            {
                                return !s.writeDefaultValues;
                            }

                            return false;
                        }).GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                AnimatorState current2 = enumerator.Current;
                                GUILayout.HorizontalScope horizontalScope2 = new GUILayout.HorizontalScope();
                                try
                                {
                                    if (ControllerEditorUtility.DisableQueue("<", GUILayout.ExpandWidth(expand: false)))
                                    {
                                        StopMapper(current2, connectionreguired: true);
                                    }

                                    CreateAnnotation(current2, GUILayout.ExpandWidth(expand: true));
                                }
                                finally
                                {
                                    if (horizontalScope2 != null)
                                    {
                                        ((IDisposable)horizontalScope2).Dispose();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (enumerator != null)
                            {
                                enumerator.Dispose();
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
            }
        }

        private static void RateVisitor(UnityEngine.Object task)
        {
            if (!task)
            {
                _ModelAnnotation = 0;
                EditorWindow.GetWindow<ControllerEditor>().Repaint();
                return;
            }

            bool flag = Selection.activeObject == task;
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                string text;
                if (!string.IsNullOrEmpty(task.name))
                {
                    text = task.name;
                }
                else
                {
                    text = task.GetType().Name;
                }

                string text2 = text;
                if (ControllerEditorUtility.CountQueue("- " + text2, flag ? ControllerEditorUtility.CalcError().m_MockProcessor : GUI.skin.label))
                {
                    Selection.activeObject = ((!flag) ? task : null);
                }

                GUILayout.FlexibleSpace();
                if (ControllerEditorUtility.RestartQueue(task.hideFlags.HasFlag(HideFlags.HideInHierarchy) ? ControllerEditorUtility.DestroyError().m_StatusProcessor : ControllerEditorUtility.DestroyError()._RefProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(14f), GUILayout.Height(18f)))
                {
                    Undo.RecordObject(task, "Toggle Sub-Asset Visibility");
                    task.hideFlags ^= HideFlags.HideInHierarchy;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(task), ImportAssetOptions.ForceUpdate);
                }

                if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor) && EditorUtility.DisplayDialog("Delete", "Delete " + task.name + "?\nUse cautiously! May result in unintended behavior!", "Ok", "Cancel"))
                {
                    Undo.RecordObject(task, "Remove SubAsset");
                    AssetDatabase.RemoveObjectFromAsset(task);
                    Undo.DestroyObjectImmediate(task);
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

        private static void DestroyVisitor()
        {
            if (!_Descriptor)
            {
                return;
            }

            int num = m_AlgoAnnotation.Count + (m_Template ? 1 : 0) + (m_Message ? 1 : 0) + (collection ? 1 : 0);
            {
                string token = $"State Count: {num}";
                DeleteAnnotation(GetVisitor, token, AddInitializer().showStateCount, iscont2: true, 3);
                DeleteAnnotation(CalcVisitor, "State Settings", AddInitializer().showStateSettings, iscont2: true, 4);
                if (SerializedPropertyGroup.InvokePage())
                {
                    DeleteAnnotation(RunVisitor, "VRC Parameter Drivers", AddInitializer().showVRCDrivers, iscont2: false, 5);
                    DeleteAnnotation(CloneVisitor, "VRC Tracking Control", AddInitializer().showVRCTracking, iscont2: false, 6);
                }
            }
        }

        private static void GetVisitor()
        {
            int num = m_AlgoAnnotation.Count + (m_Template ? 1 : 0) + (m_Message ? 1 : 0) + (collection ? 1 : 0);
            GUILayout.HorizontalScope horizontalScope;
            if (num > 0)
            {
                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    GUILayout.Space(5f);
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
                    {
                        _ = Selection.objects;
                        UnityEngine.Object[] array = Selection.objects.Except(RevertMapper().states.Select((ChildAnimatorState c) => c.state)).ToArray();
                        for (int num2 = array.Length - 1; num2 >= 0; num2--)
                        {
                            Type type = array[num2].GetType();
                            if (type == UnityAnimatorBridge.ConnectionTests.m_FilterTests || type == UnityAnimatorBridge.ConnectionTests._WorkerTests || type == UnityAnimatorBridge.ConnectionTests._SystemTests)
                            {
                                ArrayUtility.RemoveAt(ref array, num2);
                            }
                        }

                        Selection.objects = array;
                    }

                    GUILayout.Space(18f);
                    GUILayout.FlexibleSpace();
                    if (ControllerEditorUtility.DisableQueue("Out", GUILayout.Width(34f)))
                    {
                        GetAlgo();
                    }

                    GUILayout.Label("Selected " + num + " States", ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                    if (ControllerEditorUtility.DisableQueue("In", GUILayout.Width(34f)))
                    {
                        CalcAlgo();
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Space(42f);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                EditorGUILayout.Space();
            }
            else
            {
                horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Selected 0 States", ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                    GUILayout.FlexibleSpace();
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }

                EditorGUILayout.Space();
                GUILayout.Space(37f);
            }

            IEnumerator<UnityAnimatorBridge.TokenizerTests> enumerator = m_MapperAnnotation.Where((UnityAnimatorBridge.TokenizerTests n) => n.objectTests == UnityAnimatorBridge.TokenizerTests.NodeType.state).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UnityAnimatorBridge.TokenizerTests current = enumerator.Current;
                    horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
                    try
                    {
                        if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
                        {
                            UnityEngine.Object[] array2 = Selection.objects;
                            ArrayUtility.Remove(ref array2, current.comparatorTests);
                            Selection.objects = array2;
                        }

                        using (new EditorGUI.DisabledScope(!current.comparatorTests.motion))
                        {
                            if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_RegistryProcessor, ControllerEditorUtility.CalcError().configProcessor, GUILayout.Width(17f)))
                            {
                                EditorGUIUtility.PingObject(current.comparatorTests.motion);
                            }
                        }

                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledScope(current.comparatorTests.transitions.Length < 1))
                        {
                            if (ControllerEditorUtility.DisableQueue("Out", GUILayout.Width(34f)))
                            {
                                Selection.objects = Selection.objects.Concat(current.RegisterPolicy()).ToArray();
                            }
                        }

                        GUILayout.Label(RunAnnotation(current.comparatorTests.name, 18), ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                        using (new EditorGUI.DisabledScope(!current.ChangePolicy().Any()))
                        {
                            if (ControllerEditorUtility.DisableQueue("In", GUILayout.Width(34f)))
                            {
                                Selection.objects = Selection.objects.Concat(current.ChangePolicy()).ToArray();
                            }
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.Space(42f);
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
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            if (m_Template)
            {
                horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
                try
                {
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
                    {
                        UnityEngine.Object[] array3 = Selection.objects;
                        for (int num3 = 0; num3 < array3.Length; num3++)
                        {
                            if (array3[num3].GetType() == UnityAnimatorBridge.ConnectionTests._WorkerTests)
                            {
                                ArrayUtility.RemoveAt(ref array3, num3);
                                m_Template = false;
                                break;
                            }
                        }

                        Selection.objects = array3;
                    }

                    GUILayout.Space(21f);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(!RevertMapper() || RevertMapper().anyStateTransitions.Length < 1))
                    {
                        if (ControllerEditorUtility.DisableQueue("Out", GUILayout.Width(34f)))
                        {
                            Selection.objects = Selection.objects.Concat(ManageMapper().anyStateTransitions.Where(delegate (AnimatorStateTransition t)
                            {
                                _003C_003Ec__DisplayClass308_0 _003C_003Ec__DisplayClass308_ = new _003C_003Ec__DisplayClass308_0();
                                _003C_003Ec__DisplayClass308_.serverReg = t;
                                return RevertMapper().states.Any(_003C_003Ec__DisplayClass308_.InvokeServer);
                            })).ToArray();
                        }
                    }

                    GUILayout.Label("Any State", ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        ControllerEditorUtility.DisableQueue("In", GUILayout.Width(34f));
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Space(42f);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            }

            if (m_Message)
            {
                horizontalScope = new GUILayout.HorizontalScope("box");
                try
                {
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
                    {
                        UnityEngine.Object[] array4 = Selection.objects;
                        for (int num4 = 0; num4 < array4.Length; num4++)
                        {
                            if (array4[num4].GetType() == UnityAnimatorBridge.ConnectionTests._SystemTests)
                            {
                                ArrayUtility.RemoveAt(ref array4, num4);
                                m_Message = false;
                                break;
                            }
                        }

                        Selection.objects = array4;
                    }

                    GUILayout.Space(21f);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(!RevertMapper() || RevertMapper().entryTransitions.Length == 0))
                    {
                        if (ControllerEditorUtility.DisableQueue("Out", GUILayout.Width(34f)))
                        {
                            Selection.objects = Selection.objects.Concat(RevertMapper().entryTransitions).ToArray();
                        }
                    }

                    GUILayout.Label("Entry", ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        ControllerEditorUtility.DisableQueue("In", GUILayout.Width(34f));
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.Space(42f);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            }

            if (collection)
            {
                horizontalScope = new GUILayout.HorizontalScope("box");
                try
                {
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
                    {
                        UnityEngine.Object[] objects = Selection.objects;
                        for (int num5 = 0; num5 < objects.Length; num5++)
                        {
                            if (objects[num5].GetType() == UnityAnimatorBridge.ConnectionTests.m_FilterTests)
                            {
                                objects[num5] = null;
                                collection = false;
                                break;
                            }
                        }

                        Selection.objects = objects;
                    }

                    GUILayout.Space(21f);
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(disabled: true);
                    ControllerEditorUtility.DisableQueue("Out", GUILayout.Width(30f));
                    EditorGUI.EndDisabledGroup();
                    GUILayout.Label("Exit", ControllerEditorUtility.CalcError()._StructProcessor, GUILayout.Width(140f));
                    EditorGUI.BeginDisabledGroup(regAnnotation.GetRules());
                    if (ControllerEditorUtility.DisableQueue("In", GUILayout.Width(30f)))
                    {
                        Selection.objects = Selection.objects.Concat(regAnnotation).ToArray();
                    }

                    EditorGUI.EndDisabledGroup();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(42f);
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(num < 2);
            horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                if (ControllerEditorUtility.CountQueue("Align Vertical", "toolbarbutton"))
                {
                    CountAlgo();
                }

                if (ControllerEditorUtility.CountQueue("Align Horizontal", "toolbarbutton"))
                {
                    DisableAlgo();
                }
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(num < 1);
            horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                if (ControllerEditorUtility.CountQueue("Up 0.25", "toolbarbutton"))
                {
                    ChildAnimatorState[] states = RevertMapper().states;
                    for (int num6 = 0; num6 < states.Length; num6++)
                    {
                        if (m_AlgoAnnotation.Contains(states[num6].state))
                        {
                            states[num6].position += Vector3.down * 3f;
                        }
                    }

                    if (m_Message)
                    {
                        RevertMapper().entryPosition += Vector3.down * 3f;
                        InterruptAlgo(wantfirst: false);
                    }

                    if (m_Template)
                    {
                        RevertMapper().anyStatePosition += Vector3.down * 3f;
                    }

                    if (collection)
                    {
                        RevertMapper().exitPosition += Vector3.down * 3f;
                    }

                    RevertMapper().states = states;
                    EditorUtility.SetDirty(LogoutMapper());
                    if (m_Message || collection)
                    {
                        InterruptAlgo(wantfirst: false);
                    }
                }

                if (ControllerEditorUtility.CountQueue("Right 0.25", "toolbarbutton"))
                {
                    ChildAnimatorState[] states2 = RevertMapper().states;
                    for (int num7 = 0; num7 < states2.Length; num7++)
                    {
                        if (m_AlgoAnnotation.Contains(states2[num7].state))
                        {
                            states2[num7].position += Vector3.right * 3f;
                        }
                    }

                    if (m_Message)
                    {
                        RevertMapper().entryPosition += Vector3.right * 3f;
                    }

                    if (m_Template)
                    {
                        RevertMapper().anyStatePosition += Vector3.right * 3f;
                    }

                    if (collection)
                    {
                        RevertMapper().exitPosition += Vector3.right * 3f;
                    }

                    RevertMapper().states = states2;
                    EditorUtility.SetDirty(LogoutMapper());
                    if (m_Message || collection)
                    {
                        InterruptAlgo(wantfirst: false);
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

            EditorGUI.EndDisabledGroup();
        }

        private static void CalcVisitor()
        {
            if (m_AlgoAnnotation.Count < 1)
            {
                IncludeVisitor();
                return;
            }

            _Publisher.Update();
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                GUILayout.Label(ControllerEditorUtility.DestroyError()._DatabaseProcessor, GUILayout.Width(35f), GUILayout.Height(35f));
                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope();
                try
                {
                    MixedValueScope managerThread = new MixedValueScope(m_TokenizerAnnotation);
                    try
                    {
                        EditorGUI.BeginChangeCheck();
                        string serv = EditorGUILayout.DelayedTextField(string.Empty, m_TokenizerAnnotation.stringValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RestartAlgo(RevertMapper(), m_AlgoAnnotation, serv);
                        }
                    }
                    finally
                    {
                        if (managerThread != null)
                        {
                            ((IDisposable)managerThread).Dispose();
                        }
                    }

                    EditorGUIUtility.labelWidth = 35f;
                    EditorGUILayout.PropertyField(decoratorAnnotation);
                    EditorGUIUtility.labelWidth = 0f;
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
            EditorGUILayout.PropertyField(m_ComparatorAnnotation);
            EditorGUILayout.PropertyField(m_ExceptionAnnotation);
            PrepareMapper("Multiplier", null, objectAnnotation, m_ReponseAnnotation, _Proxy, res4stop: true);
            PrepareMapper("Motion Time", null, _UtilsAnnotation, _PoolAnnotation, _Proxy);
            PrepareMapper("Mirror", valAnnotation, _RepositoryAnnotation, m_ParameterAnnotation, broadcaster);
            PrepareMapper("Cycle Offset", m_ValueAnnotation, m_MappingAnnotation, _ComposerAnnotation, _Proxy);
            horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                EditorGUILayout.PropertyField(m_AuthenticationAnnotation, new GUIContent("Write Defaults"));
                EditorGUILayout.PropertyField(_MerchantAnnotation, new GUIContent("Foot IK"));
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            _Publisher.ApplyModifiedProperties();
        }

        private static void IncludeVisitor()
        {
            EditorGUI.showMixedValue = true;
            EditorGUI.BeginDisabledGroup(disabled: true);
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                GUILayout.Label(ControllerEditorUtility.DestroyError()._DatabaseProcessor, GUILayout.Height(35f), GUILayout.Width(35f));
                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope();
                try
                {
                    GUILayout.TextField("");
                    EditorGUIUtility.labelWidth = 35f;
                    EditorGUILayout.TextField("Tag", "");
                    EditorGUIUtility.labelWidth = 0f;
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
            EditorGUILayout.ObjectField("Motion", null, typeof(UnityEngine.Object), false);
            EditorGUILayout.IntField("Speed", 0);
            EditorGUI.indentLevel++;
            horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                EditorGUILayout.TextField("Multiplier", "");
                EditorGUI.indentLevel--;
                EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
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
                GUILayout.Label("Normalized Time");
                EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
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
                EditorGUILayout.Toggle("Mirror", false);
                EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
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
                EditorGUILayout.IntField("Cycle Offset", 0);
                EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
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
                EditorGUILayout.Toggle("Write Defaults", false);
                EditorGUILayout.Toggle("Foot IK", false);
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.showMixedValue = false;
        }

        private static void RunVisitor()
        {
            while (true)
            {
                EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count < 1);
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 4:
                            return;
                        case 3:
                            break;
                        case 1:
                            EditorGUI.EndDisabledGroup();
                            num = 0;
                            continue;
                        case 2:
                            _ErrorAnnotation.DoLayoutList();
                            num = 5;
                            continue;
                        case 5:
                            goto case 1;
                    }

                    break;
                }
            }
        }

        internal static void CloneVisitor()
        {
            if (m_ConnectionAnnotation)
            {
                rulesAnnotation.PrintInitializer();
                return;
            }

            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope("in bigtitle");
            try
            {
                EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count <= 0);
                if (ControllerEditorUtility.DisableQueue("Add Tracking to Selected States"))
                {
                    ConcatAnnotation();
                }

                EditorGUI.EndDisabledGroup();
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }
        }

        private static void LoginVisitor()
        {
            while (_Publisher != null)
            {
                int num = 17;
                while (true)
                {
                    switch (num)
                    {
                        case 15:
                            _UtilsAnnotation = _Publisher.FindProperty("m_TimeParameter");
                            num = 13;
                            continue;
                        case 1:
                            return;
                        case 10:
                            return;
                        case 13:
                            valAnnotation = _Publisher.FindProperty("m_Mirror");
                            num = 6;
                            continue;
                        case 14:
                            m_ReponseAnnotation = _Publisher.FindProperty("m_SpeedParameterActive");
                            num = 2;
                            continue;
                        case 9:
                            m_ParameterAnnotation = _Publisher.FindProperty("m_MirrorParameterActive");
                            num = 4;
                            continue;
                        case 18:
                            break;
                        case 4:
                            _ComposerAnnotation = _Publisher.FindProperty("m_CycleOffsetParameterActive");
                            num = 11;
                            continue;
                        case 8:
                            m_ExceptionAnnotation = _Publisher.FindProperty("m_Speed");
                            goto case 3;
                        case 5:
                            _MerchantAnnotation = _Publisher.FindProperty("m_IKOnFeet");
                            num = 0;
                            continue;
                        default:
                            m_AuthenticationAnnotation = _Publisher.FindProperty("m_WriteDefaultValues");
                            goto case 14;
                        case 16:
                            m_MappingAnnotation = _Publisher.FindProperty("m_CycleOffsetParameter");
                            num = 10;
                            continue;
                        case 12:
                            m_ComparatorAnnotation = _Publisher.FindProperty("m_Motion");
                            num = 8;
                            continue;
                        case 17:
                            m_TokenizerAnnotation = _Publisher.FindProperty("m_Name");
                            goto case 19;
                        case 6:
                        case 7:
                            m_ValueAnnotation = _Publisher.FindProperty("m_CycleOffset");
                            num = 5;
                            continue;
                        case 2:
                            _PoolAnnotation = _Publisher.FindProperty("m_TimeParameterActive");
                            num = 9;
                            continue;
                        case 11:
                            _RepositoryAnnotation = _Publisher.FindProperty("m_MirrorParameter");
                            num = 16;
                            continue;
                        case 19:
                            decoratorAnnotation = _Publisher.FindProperty("m_Tag");
                            num = 12;
                            continue;
                        case 3:
                            objectAnnotation = _Publisher.FindProperty("m_SpeedParameter");
                            num = 15;
                            continue;
                    }

                    break;
                }
            }
        }

        private void ReflectVisitor()
        {
            string token = default(string);
            while (parserAnnotation)
            {
                int num = 2;
                while (true)
                {
                    Func<bool> func;
                    switch (num)
                    {
                        default:
                            return;
                        case 0:
                            return;
                        case 11:
                            DeleteAnnotation(DeleteVisitor, token, AddInitializer().showTransitionsCount, iscont2: true, 0);
                            goto case 5;
                        case 9:
                            return;
                        case 8:
                            return;
                        case 3:
                            break;
                        case 6:
                            goto end_IL_0110;
                        case 1:
                        case 7:
                            DeleteAnnotation(NewVisitor, "Transition Conditions", AddInitializer().showTransitionConditions, iscont2: false, 2);
                            num = 8;
                            continue;
                        case 5:
                            DeleteAnnotation(CreateVisitor, "Transition Settings", AddInitializer().showTransitionSettings, iscont2: true, 1);
                            num = 7;
                            continue;
                        case 2:
                            func = _003C_003Ec.itemInitializer;
                            if (func != null)
                            {
                                goto IL_0255;
                            }

                            num = 4;
                            continue;
                        case 4:
                            func = (_003C_003Ec.itemInitializer = () => true);
                            goto IL_0255;
                        case 10:
                        {
                            token = $"Transition Count: {_DefinitionAnnotation.Count}";
                            num = 11;
                            continue;
                        }

                            IL_0255:
                                if (!func())
                                {
                                    return;
                                }

                            num = 10;
                            continue;
                    }

                    break;
                }

                continue;
                end_IL_0110:
                    break;
            }
        }

        private void DeleteVisitor()
        {
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                GUILayout.FlexibleSpace();
                if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(25f)))
                {
                    Selection.objects = Selection.objects.Except(propertyAnnotation).ToArray();
                }

                GUILayout.Label($"Editing {_DefinitionAnnotation.Count} Transitions");
                GUILayout.FlexibleSpace();
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!_Manager && !m_Parser);
            try
            {
                int num = Mathf.CeilToInt((float)_DefinitionAnnotation.Count / 3f);
                int num2 = 0;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                if (_Manager || m_Parser)
                {
                    IEnumerator<UnityAnimatorBridge.InstanceTests> enumerator = _DefinitionAnnotation.Where((UnityAnimatorBridge.InstanceTests et) => et.itemTests != null).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            UnityAnimatorBridge.InstanceTests current = enumerator.Current;
                            if (num2 == num)
                            {
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.BeginVertical();
                                num2 = 0;
                            }

                            horizontalScope = new GUILayout.HorizontalScope();
                            try
                            {
                                if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().importerProcessor, ControllerEditorUtility.CalcError().m_InstanceProcessor, GUILayout.Width(25f)))
                                {
                                    UnityEngine.Object[] array = Selection.objects;
                                    ArrayUtility.Remove(ref array, current.itemTests);
                                    Selection.objects = array;
                                }

                                bool flag = current.itemTests == _ProcessorAnnotation.itemTests;
                                if (ControllerEditorUtility.CountQueue(current.VisitSerializer(), flag ? ControllerEditorUtility.CalcError().m_MockProcessor : GUI.skin.label, GUILayout.MinWidth(1f)))
                                {
                                    if (!flag)
                                    {
                                        _ProcessorAnnotation = current;
                                        specification = true;
                                        MapVisitor();
                                        RunAlgo();
                                        CalculateAnnotation();
                                    }
                                    else
                                    {
                                        _ProcessorAnnotation = default(UnityAnimatorBridge.InstanceTests);
                                        ManageWrapper();
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

                            num2++;
                        }
                    }
                    finally
                    {
                        if (enumerator != null)
                        {
                            enumerator.Dispose();
                        }
                    }
                }
                else
                {
                    GUILayout.Label(string.Empty);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            finally
            {
                ((IDisposable)disabledScope /*cast due to .constrained prefix*/).Dispose();
            }
        }

        private void CreateVisitor()
        {
            EditorGUI.BeginDisabledGroup(!m_Parser);
            m_Proc.Update();
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                string text;
                if (!_ProcessorAnnotation.specificationTests)
                {
                    text = string.Empty;
                }
                else
                {
                    text = _ProcessorAnnotation.VisitSerializer() + "'s Settings";
                }

                GUILayout.Label(text, GUILayout.ExpandWidth(expand: true));
                if ((_TestsAnnotation.Count == 1 || (bool)_ProcessorAnnotation.specificationTests) && ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_BridgeProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                {
                    if (_ObserverAnnotation == null)
                    {
                        _ObserverAnnotation = new AnimatorStateTransition();
                    }

                    CustomizeAlgo(_ProcessorAnnotation.specificationTests ? _ProcessorAnnotation.specificationTests : _TestsAnnotation[0], _ObserverAnnotation);
                }

                using (new EditorGUI.DisabledScope(!_ObserverAnnotation))
                {
                    if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.DestroyError().m_StrategyProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        for (int i = 0; i < _TestsAnnotation.Count; i++)
                        {
                            Undo.RecordObject(_TestsAnnotation[i], "PasteSettings");
                            CustomizeAlgo(_ObserverAnnotation, _TestsAnnotation[i]);
                        }
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
                EditorGUILayout.PropertyField(_MockAnnotation);
                EditorGUI.BeginDisabledGroup(!_MockAnnotation.boolValue);
                EditorGUILayout.PropertyField(instanceAnnotation);
                EditorGUI.EndDisabledGroup();
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
                EditorGUILayout.PropertyField(_FieldAnnotation);
                EditorGUILayout.PropertyField(attributeAnnotation);
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            EditorGUILayout.PropertyField(m_ClientAnnotation);
            EditorGUILayout.PropertyField(configAnnotation);
            horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                EditorGUILayout.PropertyField(m_DescriptorAnnotation);
                EditorGUILayout.PropertyField(m_CollectionAnnotation);
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
                EditorGUILayout.PropertyField(_TemplateAnnotation);
                EditorGUILayout.PropertyField(_MessageAnnotation);
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            m_Proc.ApplyModifiedProperties();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
        }

        private void NewVisitor()
        {
            GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope(GUI.skin.box);
            try
            {
                if ((bool)AddInitializer().showMatchingOptions)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
                        try
                        {
                            AddInitializer().matchParameter.AddDefinition("Match Parameter", "Ignore Parameter", true, Color.green, Color.red);
                            AddInitializer().matchMode.AddDefinition("Match Mode", "Ignore Mode", true, Color.green, Color.red);
                            AddInitializer().matchValue.AddDefinition("Match Value", "Ignore Value", true, Color.green, Color.red);
                            if (changeCheckScope.changed)
                            {
                                UpdateVisitor();
                            }
                        }
                        finally
                        {
                            if (changeCheckScope != null)
                            {
                                ((IDisposable)changeCheckScope).Dispose();
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

                using (new EditorGUI.DisabledScope(_DefinitionAnnotation.Count == 0))
                {
                    object obj;
                    if (!SetInitializer())
                    {
                        obj = (specification ? invocationAnnotation : _RoleAnnotation);
                    }
                    else
                    {
                        obj = m_ParamAnnotation;
                    }

                    ReorderableList reorderableList = (ReorderableList)obj;
                    Event current = Event.current;
                    if (current.type == EventType.KeyDown)
                    {
                        bool flag = current.keyCode == KeyCode.DownArrow;
                        bool flag2 = !flag && current.keyCode == KeyCode.UpArrow;
                        if (flag || flag2)
                        {
                            Match match = Regex.Match(GUI.GetNameOfFocusedControl(), "Threshold(\\d+)");
                            if (match.Success)
                            {
                                int num = int.Parse(match.Groups[1].Value);
                                int num2 = (int)Mathf.Repeat(flag ? (num + 1) : (num - 1), reorderableList.count);
                                EditorGUI.FocusTextInControl($"Threshold{num2}");
                            }
                        }
                    }

                    reorderableList.DoLayoutList();
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

        private static void PushVisitor()
        {
            if (m_Proc == null)
            {
                return;
            }

            SerializedObject proc = m_Proc;
            _MockAnnotation = proc.FindProperty("m_HasExitTime");
            instanceAnnotation = proc.FindProperty("m_ExitTime");
            _FieldAnnotation = proc.FindProperty("m_HasFixedDuration");
            attributeAnnotation = proc.FindProperty("m_TransitionDuration");
            m_ClientAnnotation = proc.FindProperty("m_TransitionOffset");
            configAnnotation = proc.FindProperty("m_InterruptionSource");
            m_DescriptorAnnotation = proc.FindProperty("m_OrderedInterruption");
            _TemplateAnnotation = proc.FindProperty("m_CanTransitionToSelf");
            _MessageAnnotation = proc.FindProperty("m_Solo");
            m_CollectionAnnotation = proc.FindProperty("m_Mute");
        }

        [EditorInitializeAttribute(0)]
        private static void ViewVisitor()
        {
            List<Type> list = AppDomain.CurrentDomain.GetAssemblies().SelectMany((System.Reflection.Assembly a) => a.GetTypes()).Where(delegate (Type t)
            {
                while (t.IsSubclassOf(typeof(Component)))
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                return !t.IsGenericTypeDefinition;
                            case 2:
                                break;
                            case 3:
                                goto end_IL_0098;
                            default:
                                if (!t.IsAbstract)
                                {
                                    num = 4;
                                    continue;
                                }

                                goto end_IL_0098;
                            case 1:
                                goto default;
                        }

                        break;
                    }

                    continue;
                    end_IL_0098:
                        break;
                }

                return false;
            }).ToList();
            list.Add(typeof(GameObject));
            m_ManagerAnnotation = list.OrderBy((Type t) => t.Name).ToArray();
        }

        private static string[] CollectVisitor(Shader var1)
        {
            if (_SpecificationAnnotation.TryGetValue(var1, out var value))
            {
                return value;
            }

            GameObject gameObject = new GameObject();
            Material material = new Material(var1);
            try
            {
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                string[] array = (
                    from b in AnimationUtility.GetAnimatableBindings(gameObject, gameObject).Where((EditorCurveBinding b) =>
                    {
                        if (b.propertyName.StartsWith("material."))
                        {
                            return b.type.RemoveResolver<Renderer>();
                        }

                        return false;
                    })select b.propertyName into p
                        orderby p
                        select p).ToArray();
                _SpecificationAnnotation.Add(var1, array);
                return array;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        private static string[] ListVisitor(Type item)
        {
            _003C_003Ec__DisplayClass339_0 CS_0024_003C_003E8__locals8 = new _003C_003Ec__DisplayClass339_0();
            CS_0024_003C_003E8__locals8.m_PolicyReg = item;
            if (!(CS_0024_003C_003E8__locals8.m_PolicyReg == typeof(GameObject)))
            {
                if (CS_0024_003C_003E8__locals8.m_PolicyReg == typeof(Behaviour))
                {
                    return new string[1]{"m_Enabled"};
                }

                if (m_ItemAnnotation.TryGetValue(CS_0024_003C_003E8__locals8.m_PolicyReg, out var value))
                {
                    return value;
                }

                if (CS_0024_003C_003E8__locals8.m_PolicyReg.IsSubclassOf(typeof(Component)))
                {
                    InstantiateInitializer().ListRules(CS_0024_003C_003E8__locals8.m_PolicyReg);
                }

                string[] array = (
                    from b in AnimationUtility.GetAnimatableBindings(InstantiateInitializer(), InstantiateInitializer())
                    where b.type == CS_0024_003C_003E8__locals8.m_PolicyReg
                    select b.propertyName into s
                        orderby s
                        select s).ToArray();
                m_ItemAnnotation.Add(CS_0024_003C_003E8__locals8.m_PolicyReg, array);
                return array;
            }

            return new string[1]{"m_IsActive"};
        }

        private static bool VerifyVisitor(Type var1)
        {
            string[] value = default(string[]);
            while (true)
            {
                int num;
                if (!(var1 == typeof(GameObject)))
                {
                    if (!m_ItemAnnotation.TryGetValue(var1, out value))
                    {
                        break;
                    }

                    num = 4;
                }
                else
                {
                    num = 0;
                }

                switch (num)
                {
                    case 1:
                        continue;
                    case 2:
                    case 4:
                        return value.Length != 0;
                    case 3:
                    case 5:
                        break;
                    default:
                        return true;
                }

                break;
            }

            return true;
        }

        private static void FillVisitor(ReorderableList spec)
        {
            AnimatorCondition animatorCondition = default(AnimatorCondition);
            if (_TokenAnnotation.Count > 0)
            {
                animatorCondition = _TokenAnnotation.Last().m_ProcessVisitor;
            }
            else if (LogoutMapper() != null)
            {
                if (LogoutMapper().parameters.Length != 0)
                {
                    if (LogoutMapper().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Bool && LogoutMapper().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Trigger)
                    {
                        animatorCondition.mode = AnimatorConditionMode.Equals;
                    }
                    else
                    {
                        animatorCondition.mode = AnimatorConditionMode.If;
                    }

                    animatorCondition.parameter = LogoutMapper().parameters[0].name;
                    animatorCondition.threshold = 0f;
                }
                else
                {
                    animatorCondition.parameter = "New Parameter";
                }
            }

            if (SetInitializer())
            {
                _ProcessorAnnotation.itemTests.AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
            }
            else
            {
                for (int i = 0; i < propertyAnnotation.Count; i++)
                {
                    propertyAnnotation[i].AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
                }
            }

            _TokenAnnotation = AssetVisitor(propertyAnnotation);
            MapVisitor();
        }

        private static bool WriteVisitor(AnimatorCondition value, AnimatorCondition counter, bool isres, out bool[] reference2)
        {
            reference2 = new bool[3];
            if ((bool)LogoutMapper())
            {
                isres |= !AddInitializer().showMatchingOptions;
                reference2[0] = value.parameter == counter.parameter;
                if ((!isres && !AddInitializer().matchParameter) || reference2[0])
                {
                    int second;
                    UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(value.parameter, out second);
                    UnityEngine.AnimatorControllerParameter animatorControllerParameter2 = ResetAnnotation(counter.parameter, out second);
                    if (animatorControllerParameter == null && animatorControllerParameter2 == null)
                    {
                        return true;
                    }

                    if (animatorControllerParameter.type == animatorControllerParameter2.type)
                    {
                        UnityEngine.AnimatorControllerParameterType type = animatorControllerParameter.type;
                        if (type != UnityEngine.AnimatorControllerParameterType.Trigger)
                        {
                            reference2[1] = value.mode == counter.mode;
                            if ((isres || (type == UnityEngine.AnimatorControllerParameterType.Bool && (bool)AddInitializer().matchValue) || (type != UnityEngine.AnimatorControllerParameterType.Bool && (bool)AddInitializer().matchMode)) && !reference2[1])
                            {
                                return false;
                            }

                            if (type != UnityEngine.AnimatorControllerParameterType.Bool)
                            {
                                reference2[2] = value.threshold == counter.threshold;
                                if (isres || (bool)AddInitializer().matchValue)
                                {
                                    return reference2[2];
                                }

                                return true;
                            }

                            return true;
                        }

                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }

        private static bool ForgotVisitor(AnimatorCondition asset, AnimatorCondition token, bool forcetag)
        {
            bool[] reference;
            return WriteVisitor(asset, token, forcetag, out reference);
        }

        private static List<GlobalVisitor> StopVisitor()
        {
            if (!SetInitializer())
            {
                if (!specification)
                {
                    return m_CodeAnnotation;
                }

                return _TokenAnnotation;
            }

            return m_DicAnnotation;
        }

        private static List<GlobalVisitor> CheckVisitor(AnimatorTransitionBase ident, List<GlobalVisitor> reg)
        {
            int num = 0;
            while (num < ident.conditions.Length)
            {
                IEnumerator<GlobalVisitor> enumerator = reg.Where((GlobalVisitor sc) => !sc._TaskVisitor).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        GlobalVisitor current = enumerator.Current;
                        if (!WriteVisitor(current.m_ProcessVisitor, ident.conditions[num], isres: false, out var reference))
                        {
                            continue;
                        }

                        current.CollectInitializer(ident, num);
                        current.CheckInitializer(reference);
                        break;
                    }
                }
                finally
                {
                    if (enumerator != null)
                    {
                        enumerator.Dispose();
                    }
                }

                num++;
            }

            List<GlobalVisitor> list = new List<GlobalVisitor>();
            foreach (GlobalVisitor item in reg)
            {
                if (!item._TaskVisitor)
                {
                    list.Add(item);
                }

                item._TaskVisitor = false;
            }

            reg = reg.Except(list).ToList();
            return reg;
        }

        private static List<GlobalVisitor> PrepareVisitor(AnimatorTransitionBase config)
        {
            List<GlobalVisitor> list = new List<GlobalVisitor>();
            for (int i = 0; i < config.conditions.Length; i++)
            {
                list.Add(new GlobalVisitor(config, i));
            }

            return list;
        }

        private static List<GlobalVisitor> AssetVisitor(List<AnimatorTransitionBase> ident)
        {
            if (ident.Count == 0)
            {
                return new List<GlobalVisitor>();
            }

            List<GlobalVisitor> list = PrepareVisitor(ident[0]);
            for (int i = 1; i < ident.Count; i++)
            {
                if (list.Count == 0)
                {
                    return list;
                }

                list = CheckVisitor(ident[i], list);
            }

            return list;
        }

        private static void UpdateVisitor()
        {
            _TokenAnnotation = AssetVisitor(propertyAnnotation);
            MapVisitor();
        }

        private static void ChangeVisitor()
        {
            List<GlobalVisitor> list = new List<GlobalVisitor>();
            List<AnimatorTransitionBase> list2 = new List<AnimatorTransitionBase>();
            for (int i = 0; i < m_CodeAnnotation.Count; i++)
            {
                AnimatorTransitionBase item = m_CodeAnnotation[i]._ProducerVisitor[0].Item1;
                if (!list2.Contains(item))
                {
                    list2.Add(item);
                    for (int j = 0; j < item.conditions.Length; j++)
                    {
                        list.Add(new GlobalVisitor(item, j));
                    }
                }
            }

            m_CodeAnnotation = list;
        }

        private static void SortVisitor()
        {
            while (true)
            {
                int num;
                if (!SetInitializer())
                {
                    num = 0;
                    goto IL_001d;
                }

                List<AnimatorCondition> threadAnnotation = _ProcessorAnnotation.itemTests.conditions.ToList();
                goto IL_017d;
                IL_012c:
                    threadAnnotation = _TokenAnnotation.Select((GlobalVisitor sc) => sc.m_ProcessVisitor).ToList();
                goto IL_017d;
                IL_008d:
                    threadAnnotation = m_CodeAnnotation.Select((GlobalVisitor sc) => sc.m_ProcessVisitor).ToList();
                goto IL_017d;
                IL_017d:
                    m_ThreadAnnotation = threadAnnotation;
                num = 4;
                goto IL_001d;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                return;
                            case 2:
                                return;
                            case 1:
                                break;
                            case 3:
                                goto IL_008d;
                            case 5:
                                if (!specification)
                                {
                                    num = 3;
                                    continue;
                                }

                                goto IL_012c;
                            default:
                                goto case 5;
                        }

                        break;
                    }
            }
        }

        private static void RegisterVisitor()
        {
            if (SetInitializer())
            {
                foreach (AnimatorCondition item in m_ThreadAnnotation)
                {
                    _ProcessorAnnotation.itemTests.AddCondition(item.mode, item.threshold, item.parameter);
                    m_DicAnnotation.Add(new GlobalVisitor(_ProcessorAnnotation.itemTests, _ProcessorAnnotation.itemTests.conditions.Length - 1));
                }
            }
            else
            {
                foreach (UnityAnimatorBridge.InstanceTests item2 in _DefinitionAnnotation)
                {
                    using List<AnimatorCondition>.Enumerator enumerator = m_ThreadAnnotation.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        AnimatorCondition current3 = enumerator.Current;
                        item2.itemTests.AddCondition(current3.mode, current3.threshold, current3.parameter);
                        m_CodeAnnotation.Add(new GlobalVisitor(item2.itemTests, item2.itemTests.conditions.Length - 1));
                    }
                }
            }

            _TokenAnnotation = AssetVisitor(propertyAnnotation);
            ChangeVisitor();
            MapVisitor();
        }

        private void LogoutVisitor()
        {
            switch (m_GlobalAnnotation)
            {
                default:
                    break;
                case ControllerAction.TagCurrentLayerWith:
                    if (!ManageMapper().anyStateTransitions.Any(delegate (AnimatorStateTransition t)
                    {
                        if (!(t.name == proxyAnnotation))
                        {
                            return false;
                        }

                        return t.isExit;
                    }))
                    {
                        AnimatorStateTransition animatorStateTransition = ManageMapper().AddAnyStateTransition((AnimatorState)null);
                        animatorStateTransition.isExit = true;
                        animatorStateTransition.mute = true;
                        animatorStateTransition.name = proxyAnnotation;
                        FlushAnnotation();
                    }

                    break;
                case ControllerAction.ReplaceParameter:
                {
                    bool last2;
                    AnimatorStateMachine[] array4 = InterruptVisitor(out last2);
                    for (int num4 = 0; num4 < array4.Length; num4++)
                    {
                        SetAlgo(array4[num4], _SchemaAnnotation, m_BroadcasterAnnotation, structAnnotation, last2);
                    }

                    if (_TaskAnnotation != ActionMode.CurrentController)
                    {
                        break;
                    }

                    UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
                    for (int num5 = LogoutMapper().parameters.Length - 1; num5 >= 0; num5--)
                    {
                        if (UpdateMapper(LogoutMapper().parameters[num5].name))
                        {
                            parameters[num5].name = parameters[num5].name.Replace(_SchemaAnnotation, m_BroadcasterAnnotation);
                        }
                    }

                    LogoutMapper().parameters = parameters;
                    break;
                }

                case ControllerAction.RemoveTag:
                {
                    UnityEditor.Animations.AnimatorControllerLayer[] array2 = PatchVisitor((int)_TaskAnnotation);
                    int num = 0;
                    while (num < array2.Length)
                    {
                        UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer2 = array2[num];
                        IEnumerator<AnimatorStateTransition> enumerator = animatorControllerLayer2.stateMachine.anyStateTransitions.Where(delegate (AnimatorStateTransition t)
                        {
                            if (t.name == _SchemaAnnotation)
                            {
                                return t.isExit;
                            }

                            return false;
                        }).GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                AnimatorStateTransition current = enumerator.Current;
                                animatorControllerLayer2.stateMachine.RemoveAnyStateTransition(current);
                            }
                        }
                        finally
                        {
                            if (enumerator != null)
                            {
                                enumerator.Dispose();
                            }
                        }

                        num++;
                    }

                    FlushAnnotation();
                    break;
                }

                case ControllerAction.RemoveParameter:
                {
                    bool last;
                    AnimatorStateMachine[] array3 = InterruptVisitor(out last);
                    for (int num = 0; num < array3.Length; num++)
                    {
                        SetupAlgo(array3[num], _SchemaAnnotation, last);
                    }

                    if (_TaskAnnotation != ActionMode.CurrentController)
                    {
                        break;
                    }

                    for (int num2 = LogoutMapper().parameters.Length - 1; num2 >= 0; num2--)
                    {
                        if (AssetMapper(LogoutMapper().parameters[num2].name))
                        {
                            LogoutMapper().RemoveParameter(num2);
                        }
                    }

                    break;
                }

                case ControllerAction.RemoveLayersWithTag:
                {
                    for (int num3 = LogoutMapper().layers.Length - 1; num3 >= 0; num3--)
                    {
                        if (LogoutMapper().layers[num3].stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation))
                        {
                            LogoutMapper().RemoveLayer(num3);
                        }
                    }

                    break;
                }

                case ControllerAction.Copy:
                {
                    _003C_003Ec__DisplayClass370_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass370_0();
                    UnityEditor.Animations.AnimatorControllerLayer[] array = PatchVisitor((int)m_ProcessAnnotation);
                    CS_0024_003C_003E8__locals4.serializerReg = ((producerAnnotation == MoveDestination.Controller) ? m_MethodAnnotation : LogoutMapper());
                    for (int i = 0; i < array.Length; i++)
                    {
                        UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = ControllerEditorUtility.CopyLayer(array[i], CS_0024_003C_003E8__locals4.serializerReg);
                        if (animatorControllerLayer == null)
                        {
                            continue;
                        }

                        if (serviceAnnotation)
                        {
                            ManageVisitor(animatorControllerLayer).InvokeResolver(delegate (UnityEngine.AnimatorControllerParameter p)
                            {
                                _003C_003Ec__DisplayClass370_1 _003C_003Ec__DisplayClass370_ = new _003C_003Ec__DisplayClass370_1();
                                if (p != null)
                                {
                                    _003C_003Ec__DisplayClass370_._ResolverReg = ((!ControllerEditorUtility.m_AdapterProcessor.Contains(p.name)) ? (p.name + _StateAnnotation) : p.name);
                                    if (CS_0024_003C_003E8__locals4.serializerReg.parameters.All(_003C_003Ec__DisplayClass370_.VisitServer))
                                    {
                                        CS_0024_003C_003E8__locals4.serializerReg.AddParameter(new UnityEngine.AnimatorControllerParameter{name = _003C_003Ec__DisplayClass370_._ResolverReg, defaultBool = p.defaultBool, defaultFloat = p.defaultFloat, defaultInt = p.defaultInt, type = p.type});
                                    }
                                }
                            });
                        }

                        if (!string.IsNullOrEmpty(_StateAnnotation))
                        {
                            EnableAlgo(animatorControllerLayer, _StateAnnotation);
                        }
                    }

                    break;
                }
            }
        }

        private static UnityEditor.Animations.AnimatorControllerLayer[] PatchVisitor(int ID_ident)
        {
            UnityEditor.Animations.AnimatorControllerLayer[] result = null;
            switch (ID_ident)
            {
                case 1:
                    result = LogoutMapper().layers.Where((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine.anyStateTransitions.Any(delegate (AnimatorStateTransition t)
                    {
                        if (!(t.name == proxyAnnotation))
                        {
                            return false;
                        }

                        return t.isExit;
                    })).ToArray();
                    break;
                case 2:
                case 3:
                    result = new UnityEditor.Animations.AnimatorControllerLayer[1]{LogoutMapper().layers.First((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine == ManageMapper())};
                    break;
                case 0:
                    result = LogoutMapper().layers;
                    break;
            }

            return result;
        }

        private static AnimatorStateMachine[] InterruptVisitor(out bool last)
        {
            AnimatorStateMachine[] result = null;
            ActionMode taskAnnotation = _TaskAnnotation;
            switch (taskAnnotation)
            {
                default:
                    last = true;
                    break;
                case ActionMode.CurrentStatemachine:
                    last = false;
                    result = new AnimatorStateMachine[1]{RevertMapper()};
                    break;
                case ActionMode.CurrentController:
                    last = true;
                    result = LogoutMapper().layers.Select((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine).ToArray();
                    break;
                case ActionMode.CurrentLayer:
                    last = true;
                    result = new AnimatorStateMachine[1]{ManageMapper()};
                    break;
                case ActionMode.LayersTaggedWith:
                    last = true;
                    result = (
                        from l in LogoutMapper().layers
                        where l.stateMachine.anyStateTransitions.Any(delegate (AnimatorStateTransition t)
                        {
                            if (t.name == proxyAnnotation)
                            {
                                return t.isExit;
                            }

                            return false;
                        })select l.stateMachine).ToArray();
                    break;
            }

            return result;
        }

        private static List<UnityEngine.AnimatorControllerParameter> ManageVisitor(UnityEditor.Animations.AnimatorControllerLayer last)
        {
            return PrintVisitor(last.stateMachine);
        }

        private static List<UnityEngine.AnimatorControllerParameter> PrintVisitor(AnimatorStateMachine spec, bool extractpol = true)
        {
            _003C_003Ec__DisplayClass374_0 CS_0024_003C_003E8__locals14 = new _003C_003Ec__DisplayClass374_0();
            CS_0024_003C_003E8__locals14.predicateReg = new List<UnityEngine.AnimatorControllerParameter>();
            spec.AssetPredicate(delegate (AnimatorState s)
            {
                int second;
                if (s.cycleOffsetParameterActive)
                {
                    if (!string.IsNullOrEmpty(s.cycleOffsetParameter))
                    {
                        CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(s.cycleOffsetParameter, out second));
                    }
                }

                if (s.mirrorParameterActive && !string.IsNullOrEmpty(s.mirrorParameter))
                {
                    CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(s.mirrorParameter, out second));
                }

                if (s.speedParameterActive && !string.IsNullOrEmpty(s.speedParameter))
                {
                    CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(s.speedParameter, out second));
                }

                if (s.timeParameterActive && !string.IsNullOrEmpty(s.timeParameter))
                {
                    CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(s.timeParameter, out second));
                }

                s.motion.VisitMotions(delegate (UnityEditor.Animations.BlendTree tree)
                {
                    if (tree.blendType != BlendTreeType.Direct)
                    {
                        int second2;
                        if (!string.IsNullOrEmpty(tree.blendParameter))
                        {
                            CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(tree.blendParameter, out second2));
                        }

                        if (tree.blendType != BlendTreeType.Simple1D && !string.IsNullOrEmpty(tree.blendParameterY))
                        {
                            CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(tree.blendParameterY, out second2));
                        }
                    }
                }, null);
                s.transitions.InvokeResolver(delegate (AnimatorStateTransition t)
                {
                    t.conditions.InvokeResolver(delegate (AnimatorCondition c)
                    {
                        if (!string.IsNullOrEmpty(c.parameter))
                        {
                            CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(c.parameter, out var _));
                        }
                    });
                });
                if (SerializedPropertyGroup.InvokePage())
                {
                    s.behaviours.InvokeResolver(delegate (StateMachineBehaviour b)
                    {
                        while (b.GetType() == SerializedPropertyGroup.InsertPage())
                        {
                            int num = 0;
                            while (true)
                            {
                                List<SerializedPropertyGroup.BridgeProperty.AttrProperty> databaseProperty;
                                Action<SerializedPropertyGroup.BridgeProperty.AttrProperty> visitor;
                                switch (num)
                                {
                                    case 4:
                                        return;
                                    case 1:
                                        break;
                                    case 3:
                                        return;
                                    default:
                                        goto case 2;
                                    case 2:
                                        databaseProperty = new SerializedPropertyGroup.BridgeProperty(b).m_DatabaseProperty;
                                        visitor = delegate (SerializedPropertyGroup.BridgeProperty.AttrProperty p)
                                        {
                                            while (!string.IsNullOrEmpty(p.ResetPage()))
                                            {
                                                int num2 = 0;
                                                while (true)
                                                {
                                                    switch (num2)
                                                    {
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            return;
                                                        case 3:
                                                            return;
                                                        case 4:
                                                            goto IL_008b;
                                                        default:
                                                            goto IL_008b;
                                                    }

                                                    break;
                                                    IL_008b:
                                                        CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(p.ResetPage(), out var _));
                                                    num2 = 3;
                                                }
                                            }
                                        };
                                        goto IL_00f2;
                                }

                                break;
                                IL_00f2:
                                    databaseProperty.InvokeResolver(visitor);
                                num = 3;
                            }
                        }
                    });
                }
            });
            spec.anyStateTransitions.InvokeResolver(delegate (AnimatorStateTransition t)
            {
                t.conditions.InvokeResolver(delegate (AnimatorCondition c)
                {
                    while (!string.IsNullOrEmpty(c.parameter))
                    {
                        int num = 2;
                        while (true)
                        {
                            switch (num)
                            {
                                case 3:
                                    break;
                                default:
                                    goto IL_0058;
                                case 1:
                                    return;
                                case 4:
                                    return;
                                case 2:
                                    goto IL_0058;
                            }

                            break;
                            IL_0058:
                                CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(c.parameter, out var _));
                            num = 4;
                        }
                    }
                });
            });
            spec.entryTransitions.InvokeResolver(delegate (AnimatorTransition t)
            {
                t.conditions.InvokeResolver(delegate (AnimatorCondition c)
                {
                    while (true)
                    {
                        int num;
                        if (string.IsNullOrEmpty(c.parameter))
                        {
                            num = 1;
                            goto IL_001d;
                        }

                        goto IL_00ef;
                        IL_001d:
                            switch (num)
                            {
                                case 3:
                                    return;
                                case 1:
                                    return;
                                case 4:
                                    return;
                                case 2:
                                    break;
                                default:
                                    goto IL_00ca;
                                case 5:
                                    goto IL_00ef;
                            }

                        continue;
                        IL_00ef:
                            ;
                        goto IL_00ca;
                        IL_00ca:
                            CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(c.parameter, out var _));
                        num = 4;
                        goto IL_001d;
                    }
                });
            });
            if (SerializedPropertyGroup.InvokePage())
            {
                spec.behaviours.InvokeResolver(delegate (StateMachineBehaviour b)
                {
                    while (b.GetType() == SerializedPropertyGroup.InsertPage())
                    {
                        int num = 1;
                        while (true)
                        {
                            List<SerializedPropertyGroup.BridgeProperty.AttrProperty> databaseProperty;
                            Action<SerializedPropertyGroup.BridgeProperty.AttrProperty> visitor;
                            switch (num)
                            {
                                default:
                                    return;
                                case 2:
                                    break;
                                case 4:
                                    return;
                                case 1:
                                    goto case 3;
                                case 3:
                                    databaseProperty = new SerializedPropertyGroup.BridgeProperty(b).m_DatabaseProperty;
                                    visitor = delegate (SerializedPropertyGroup.BridgeProperty.AttrProperty p)
                                    {
                                        while (true)
                                        {
                                            int num2;
                                            if (string.IsNullOrEmpty(p.ResetPage()))
                                            {
                                                num2 = 3;
                                                goto IL_001d;
                                            }

                                            goto IL_00ed;
                                            IL_001d:
                                                switch (num2)
                                                {
                                                    case 2:
                                                        return;
                                                    case 4:
                                                        continue;
                                                    case 3:
                                                        return;
                                                    case 1:
                                                        return;
                                                    case 5:
                                                        goto IL_00ed;
                                                }

                                            goto IL_005e;
                                            IL_00ed:
                                                ;
                                            goto IL_005e;
                                            IL_005e:
                                                CS_0024_003C_003E8__locals14.predicateReg.Add(ResetAnnotation(p.ResetPage(), out var _));
                                            num2 = 1;
                                            goto IL_001d;
                                        }
                                    };
                                    goto IL_00f2;
                            }

                            break;
                            IL_00f2:
                                databaseProperty.InvokeResolver(visitor);
                            num = 0;
                        }
                    }
                });
            }

            if (extractpol)
            {
                spec.stateMachines.InvokeResolver(delegate (ChildAnimatorStateMachine c)
                {
                    CS_0024_003C_003E8__locals14.predicateReg.AddRange(PrintVisitor(c.stateMachine));
                });
            }

            return CS_0024_003C_003E8__locals14.predicateReg.Where((UnityEngine.AnimatorControllerParameter p) => p != null).Distinct().ToList();
        }

        private static void SearchVisitor(UnityEditor.Animations.AnimatorController init)
        {
            _003C_003Ec__DisplayClass375_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass375_0();
            CS_0024_003C_003E8__locals3.consumerReg = new HashSet<UnityEngine.Object>{init};
            init.layers.InvokeResolver(delegate (UnityEditor.Animations.AnimatorControllerLayer l)
            {
                CS_0024_003C_003E8__locals3.CustomizeServer(l.stateMachine);
            });
            UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(init));
            int num = 0;
            string text = "";
            for (int num2 = 0; num2 < array.Length; num2++)
            {
                if (!CS_0024_003C_003E8__locals3.consumerReg.Contains(array[num2]) && array[num2] != null)
                {
                    num++;
                    text = text + (string.IsNullOrEmpty(array[num2].name) ? array[num2].GetType().Name : array[num2].name) + "\n";
                    Undo.DestroyObjectImmediate(array[num2]);
                }
            }

            LogoutMapper().ForEachLayerMachine(delegate (AnimatorStateMachine l)
            {
                l.ForEachStateMachine(delegate (AnimatorStateMachine m)
                {
                    while (true)
                    {
                        m.AssetPredicate(delegate (AnimatorState s)
                        {
                            s.transitions = s.transitions.Where((AnimatorStateTransition t) => t).ToArray();
                            EditorUtility.SetDirty(s);
                        }, requiresc: false);
                        int num3 = 1;
                        while (true)
                        {
                            switch (num3)
                            {
                                case 3:
                                    return;
                                case 4:
                                    return;
                                case 6:
                                    EditorUtility.SetDirty(m);
                                    num3 = 3;
                                    continue;
                                case 2:
                                    break;
                                case 1:
                                    m.entryTransitions = m.entryTransitions.Where((AnimatorTransition t) => t).ToArray();
                                    num3 = 0;
                                    continue;
                                default:
                                    goto case 5;
                                case 5:
                                    m.anyStateTransitions = m.anyStateTransitions.Where((AnimatorStateTransition t) => t).ToArray();
                                    num3 = 6;
                                    continue;
                            }

                            break;
                        }
                    }
                });
            });
            CustomizeAnnotation($"Found and removed {num} unused Sub-Assets:\n{text}");
            AssetDatabase.SaveAssets();
        }

        [EditorInitializeAttribute(0)]
        private static void RevertVisitor()
        {
            List<UnityEditor.Animations.AnimatorController> list = new List<UnityEditor.Animations.AnimatorController>();
            List<string> list2 = new List<string>();
            list.Add(null);
            list2.Add("Blank Layer");
            IEnumerator<string> enumerator = AssetDatabase.FindAssets("t:AnimatorController").Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(enumerator.Current);
                    if (!animatorController)
                    {
                        continue;
                    }

                    string[] labels = AssetDatabase.GetLabels(animatorController);
                    for (int i = 0; i < labels.Length; i++)
                    {
                        Match match = Regex.Match(labels[i], "^Template:(.*)$");
                        if (match.Success)
                        {
                            string item = Regex.Replace(match.Groups[1].Value, "(?<!%)%(?!%)", " ").Replace("%%", "%");
#if !FYNNS_CONTROLLER_EDITOR_AVATARS
                            if (item.StartsWith("VRC/", StringComparison.Ordinal) || item.StartsWith("Gesture/", StringComparison.Ordinal)) continue;
#endif
                            list.Add(animatorController);
                            list2.Add(item);
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            requestVisitor = list.ToArray();
            m_PrinterVisitor = list2.ToArray();
        }

        private static void OrderAlgo(UnityEditor.Animations.AnimatorController init, UnityEditor.Animations.AnimatorController ord)
        {
            ParameterRenameWindow parameterRenameWindow = ParameterRenameWindow.ResolveTests(init, ord, !Event.current.control && !Event.current.shift);
            try
            {
                EditorWindow editorWindow = QueryAnnotation();
                Vector2 reg = parameterRenameWindow.FillTests();
                Vector2 center = editorWindow.position.center;
                Vector2 setup = new Vector2(center.x - reg.x / 2f, center.y - reg.y / 2f);
                parameterRenameWindow.LoginHelper(setup, reg);
            }
            catch
            {
                Vector2 reg2 = parameterRenameWindow.FillTests();
                Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
                Vector2 setup2 = new Vector2(vector.x - reg2.x / 2f, vector.y - reg2.y / 2f);
                parameterRenameWindow.LoginHelper(setup2, reg2);
            }
        }

        private static UnityEditor.Animations.AnimatorControllerLayer[] CompareAlgo(UnityEditor.Animations.AnimatorController spec, UnityEditor.Animations.AnimatorController ord, (UnityEngine.AnimatorControllerParameter, string)[] dir)
        {
            _003C_003Ec__DisplayClass378_0 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass378_0();
            CS_0024_003C_003E8__locals22.m_ProductReg = ord;
            CS_0024_003C_003E8__locals22._CandidateReg = new Dictionary<string, string>();
            ControllerEditorUtility.CopyLayers(spec, CS_0024_003C_003E8__locals22.m_ProductReg, out var dir2);
            IEnumerator<UnityEngine.AnimatorControllerParameter> enumerator = spec.parameters.Where((UnityEngine.AnimatorControllerParameter p) => ControllerEditorUtility.m_AdapterProcessor.Contains(p.name)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UnityEngine.AnimatorControllerParameter current = enumerator.Current;
                    CS_0024_003C_003E8__locals22.m_ProductReg.AddParameterIfMissing(current);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            int num = 0;
            UnityEditor.Animations.AnimatorControllerLayer[] array;
            for (; num < dir.Length; num++)
            {
                (UnityEngine.AnimatorControllerParameter, string) tuple = dir[num];
                UnityEngine.AnimatorControllerParameter item = tuple.Item1;
                string item2 = tuple.Item2;
                UnityEngine.AnimatorControllerParameter predicate = item.CloneParameter();
                predicate.name = item2;
                CS_0024_003C_003E8__locals22.m_ProductReg.AddParameterIfMissing(predicate);
                if (!(item.name == item2))
                {
                    CS_0024_003C_003E8__locals22._CandidateReg.Add(item.name, item2);
                    array = dir2;
                    for (int num2 = 0; num2 < array.Length; num2++)
                    {
                        SetAlgo(array[num2].stateMachine, item.name, item2, istask2: true);
                    }
                }
            }

            UnityEditor.Animations.AnimatorControllerLayer[] layers = CS_0024_003C_003E8__locals22.m_ProductReg.layers;
            array = layers;
            for (num = 0; num < array.Length; num++)
            {
                _003C_003Ec__DisplayClass378_1 CS_0024_003C_003E8__locals20 = new _003C_003Ec__DisplayClass378_1();
                CS_0024_003C_003E8__locals20._StubReg = array[num];
                if (!dir2.All((UnityEditor.Animations.AnimatorControllerLayer l2) => l2.stateMachine != CS_0024_003C_003E8__locals20._StubReg.stateMachine))
                {
                    CS_0024_003C_003E8__locals20._StubReg.name = CS_0024_003C_003E8__locals22.CloneServer(CS_0024_003C_003E8__locals20._StubReg.name);
                    CS_0024_003C_003E8__locals20._StubReg.stateMachine.name = CS_0024_003C_003E8__locals22.CloneServer(CS_0024_003C_003E8__locals20._StubReg.stateMachine.name);
                    CS_0024_003C_003E8__locals20._StubReg.stateMachine.AssetPredicate(delegate (AnimatorState s)
                    {
                        s.name = CS_0024_003C_003E8__locals22.CloneServer(s.name);
                    });
                }
            }

            CS_0024_003C_003E8__locals22.m_ProductReg.layers = layers;
            CS_0024_003C_003E8__locals22.systemReg = new HashSet<Motion>();
            CS_0024_003C_003E8__locals22.m_ExpressionReg = new Dictionary<Motion, Motion>();
            array = dir2;
            for (num = 0; num < array.Length; num++)
            {
                array[num].stateMachine.AssetPredicate(delegate (AnimatorState s)
                {
                    s.motion = CS_0024_003C_003E8__locals22.DeleteServer(s.motion);
                });
            }

            return dir2;
        }

        private static void SetAlgo(AnimatorStateMachine param, string map, string consumer, bool istask2, bool skipconnection3 = true)
        {
            _003C_003Ec__DisplayClass379_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass379_0();
            CS_0024_003C_003E8__locals15._ReaderReg = istask2;
            CS_0024_003C_003E8__locals15.m_BridgeReg = map;
            CS_0024_003C_003E8__locals15.m_StrategyReg = consumer;
            if (SerializedPropertyGroup.InvokePage())
            {
                CS_0024_003C_003E8__locals15.CollectServer(param.behaviours);
            }

            param.AssetPredicate(delegate (AnimatorState s)
            {
                if (s.cycleOffsetParameterActive)
                {
                    s.cycleOffsetParameter = CS_0024_003C_003E8__locals15.ViewServer(s.cycleOffsetParameter);
                }

                if (s.mirrorParameterActive)
                {
                    s.mirrorParameter = CS_0024_003C_003E8__locals15.ViewServer(s.mirrorParameter);
                }

                if (s.speedParameterActive)
                {
                    s.speedParameter = CS_0024_003C_003E8__locals15.ViewServer(s.speedParameter);
                }

                if (s.timeParameterActive)
                {
                    s.timeParameter = CS_0024_003C_003E8__locals15.ViewServer(s.timeParameter);
                }

                CS_0024_003C_003E8__locals15.ResolveServer(s.motion);
                for (int num = s.transitions.Length - 1; num >= 0; num--)
                {
                    AnimatorCondition[] conditions = s.transitions[num].conditions;
                    for (int num2 = s.transitions[num].conditions.Length - 1; num2 >= 0; num2--)
                    {
                        conditions[num2].parameter = CS_0024_003C_003E8__locals15.ViewServer(conditions[num2].parameter);
                    }

                    s.transitions[num].conditions = conditions;
                }

                EditorUtility.SetDirty(s);
                if (SerializedPropertyGroup.InvokePage())
                {
                    CS_0024_003C_003E8__locals15.CollectServer(s.behaviours);
                }
            }, requiresc: false);
            param.ForEachTransition(delegate (TransitionReference t)
            {
                int num = t.SetContext().Length - 1;
                while (num >= 0)
                {
                    AnimatorCondition[] array = t.SetContext();
                    array[num].parameter = CS_0024_003C_003E8__locals15.ViewServer(array[num].parameter);
                    t.PostContext(array);
                    num--;
                }

                EditorUtility.SetDirty((AnimatorTransitionBase)t);
            }, moveserv: false);
            if (!skipconnection3)
            {
                return;
            }

            IEnumerator<AnimatorStateMachine> enumerator = param.stateMachines.Select((ChildAnimatorStateMachine c) => c.stateMachine).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    SetAlgo(enumerator.Current, CS_0024_003C_003E8__locals15.m_BridgeReg, CS_0024_003C_003E8__locals15.m_StrategyReg, CS_0024_003C_003E8__locals15._ReaderReg);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
        }

        private static void PostAlgo(AnimatorStateMachine init, string token, string serv, bool isord2, bool islast3 = true)
        {
            _003C_003Ec__DisplayClass380_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass380_0();
            CS_0024_003C_003E8__locals5.customerReg = isord2;
            CS_0024_003C_003E8__locals5._DatabaseReg = token;
            CS_0024_003C_003E8__locals5.m_ExporterReg = serv;
            init.ForEachStateMachine(delegate (AnimatorStateMachine m)
            {
                CS_0024_003C_003E8__locals5.ForgotServer(m.behaviours);
                ChildAnimatorState[] states = m.states;
                foreach (ChildAnimatorState childAnimatorState in states)
                {
                    CS_0024_003C_003E8__locals5.ForgotServer(childAnimatorState.state.behaviours);
                }
            });
        }

        private static void SetupAlgo(AnimatorStateMachine res, string connection, bool insertrule = true)
        {
            _003C_003Ec__DisplayClass381_0 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass381_0();
            CS_0024_003C_003E8__locals22._IdentifierReg = connection;
            CS_0024_003C_003E8__locals22.m_DispatcherReg = res;
            CS_0024_003C_003E8__locals22._AttrReg = null;
            if (SerializedPropertyGroup.InvokePage())
            {
                CS_0024_003C_003E8__locals22._AttrReg = delegate (StateMachineBehaviour[] b)
                {
                    foreach (StateMachineBehaviour stateMachineBehaviour in b)
                    {
                        if (!(stateMachineBehaviour.GetType() != SerializedPropertyGroup.InsertPage()))
                        {
                            SerializedPropertyGroup.BridgeProperty bridgeProperty = new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour);
                            for (int num2 = bridgeProperty.m_DatabaseProperty.Count - 1; num2 >= 0; num2--)
                            {
                                if (CS_0024_003C_003E8__locals22.CheckServer(bridgeProperty.m_DatabaseProperty[num2].ResetPage()))
                                {
                                    bridgeProperty.InitPage(num2);
                                }
                            }

                            EditorUtility.SetDirty(stateMachineBehaviour);
                        }
                    }
                };
            }

            CS_0024_003C_003E8__locals22.m_DispatcherReg.AssetPredicate(delegate (AnimatorState s)
            {
                if (s.cycleOffsetParameterActive)
                {
                    if (CS_0024_003C_003E8__locals22.CheckServer(s.cycleOffsetParameter))
                    {
                        s.cycleOffsetParameterActive = false;
                    }
                }

                if (s.mirrorParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.mirrorParameter))
                {
                    s.mirrorParameterActive = false;
                }

                if (s.speedParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.speedParameter))
                {
                    s.speedParameterActive = false;
                }

                if (s.timeParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.timeParameter))
                {
                    s.timeParameterActive = false;
                }

                CS_0024_003C_003E8__locals22.PrepareServer(s.transitions, s.RemoveTransition);
                if (SerializedPropertyGroup.InvokePage())
                {
                    CS_0024_003C_003E8__locals22._AttrReg(s.behaviours);
                }
            });
            CS_0024_003C_003E8__locals22.PrepareServer(CS_0024_003C_003E8__locals22.m_DispatcherReg.entryTransitions, delegate (AnimatorTransition t)
            {
                CS_0024_003C_003E8__locals22.m_DispatcherReg.RemoveEntryTransition(t);
            });
            CS_0024_003C_003E8__locals22.PrepareServer(CS_0024_003C_003E8__locals22.m_DispatcherReg.anyStateTransitions, delegate (AnimatorStateTransition t)
            {
                CS_0024_003C_003E8__locals22.m_DispatcherReg.RemoveAnyStateTransition(t);
            });
            if (SerializedPropertyGroup.InvokePage())
            {
                CS_0024_003C_003E8__locals22._AttrReg(CS_0024_003C_003E8__locals22.m_DispatcherReg.behaviours);
            }

            if (insertrule)
            {
                ChildAnimatorStateMachine[] stateMachines = CS_0024_003C_003E8__locals22.m_DispatcherReg.stateMachines;
                foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
                {
                    SetupAlgo(childAnimatorStateMachine.stateMachine, CS_0024_003C_003E8__locals22._IdentifierReg);
                }
            }
        }

        private static void EnableAlgo(UnityEditor.Animations.AnimatorControllerLayer item, string attr)
        {
            _003C_003Ec__DisplayClass382_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass382_0();
            CS_0024_003C_003E8__locals9._RegistryReg = attr;
            item.stateMachine.AssetPredicate(delegate (AnimatorState s)
            {
                s.mirrorParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.mirrorParameter);
                s.cycleOffsetParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.cycleOffsetParameter);
                s.speedParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.speedParameter);
                s.timeParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.timeParameter);
                StateMachineBehaviour[] behaviours = s.behaviours;
                int num3 = 0;
                while (num3 < behaviours.Length)
                {
                    StateMachineBehaviour stateMachineBehaviour = behaviours[num3];
                    if (stateMachineBehaviour.GetType() == SerializedPropertyGroup.InsertPage())
                    {
                        foreach (SerializedPropertyGroup.BridgeProperty.AttrProperty item2 in new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour).m_DatabaseProperty)
                        {
                            item2.FlushPage(CS_0024_003C_003E8__locals9.RegisterServer(item2.ResetPage()));
                            EditorUtility.SetDirty(stateMachineBehaviour);
                        }
                    }

                    num3++;
                }

                for (int i = 0; i < s.transitions.Length; i++)
                {
                    AnimatorStateTransition animatorStateTransition2 = s.transitions[i];
                    AnimatorCondition[] conditions2 = animatorStateTransition2.conditions;
                    for (int j = 0; j < animatorStateTransition2.conditions.Length; j++)
                    {
                        conditions2[j].parameter = CS_0024_003C_003E8__locals9.RegisterServer(conditions2[j].parameter);
                    }

                    animatorStateTransition2.conditions = conditions2;
                    EditorUtility.SetDirty(animatorStateTransition2);
                }
            });
            AnimatorStateTransition[] anyStateTransitions = item.stateMachine.anyStateTransitions;
            foreach (AnimatorStateTransition animatorStateTransition in anyStateTransitions)
            {
                AnimatorCondition[] conditions = animatorStateTransition.conditions;
                for (int num2 = 0; num2 < animatorStateTransition.conditions.Length; num2++)
                {
                    conditions[num2].parameter = CS_0024_003C_003E8__locals9.RegisterServer(conditions[num2].parameter);
                }

                animatorStateTransition.conditions = conditions;
                animatorStateTransition.ManagePredicate();
            }

            CS_0024_003C_003E8__locals9.PatchServer(item.stateMachine);
        }

        internal static void PublishAlgo(UnityEditor.Animations.AnimatorController v, int sizecol, UnityEngine.AnimatorControllerParameterType consumer)
        {
            _003C_003Ec__DisplayClass383_0 CS_0024_003C_003E8__locals12 = new _003C_003Ec__DisplayClass383_0();
            UnityEngine.AnimatorControllerParameter[] parameters = v.parameters;
            CS_0024_003C_003E8__locals12.tagReg = parameters[sizecol];
            if (RateAnnotation(CS_0024_003C_003E8__locals12.tagReg.type == consumer, $"Parameter {CS_0024_003C_003E8__locals12.tagReg.name} is already of type {consumer}!"))
            {
                return;
            }

            if (CS_0024_003C_003E8__locals12.tagReg.type == UnityEngine.AnimatorControllerParameterType.Float)
            {
                _003C_003Ec__DisplayClass383_1 visitor = default(_003C_003Ec__DisplayClass383_1);
                visitor.m_RequestReg = false;
                visitor.m_PrinterReg = string.Empty;
                Queue<AnimatorStateMachine> queue = new Queue<AnimatorStateMachine>();
                UnityEditor.Animations.AnimatorControllerLayer[] layers = v.layers;
                _003C_003Ec__DisplayClass383_2 comp = default(_003C_003Ec__DisplayClass383_2);
                _003C_003Ec__DisplayClass383_3 second = default(_003C_003Ec__DisplayClass383_3);
                for (int i = 0; i < layers.Length; i++)
                {
                    comp._WriterReg = layers[i];
                    if ((bool)comp._WriterReg.stateMachine)
                    {
                        queue.Enqueue(comp._WriterReg.stateMachine);
                        while (!visitor.m_RequestReg)
                        {
                            if (queue.Count == 0)
                            {
                                break;
                            }

                            AnimatorStateMachine animatorStateMachine = queue.Dequeue();
                            IEnumerator<AnimatorStateMachine> enumerator = animatorStateMachine.stateMachines.Select((ChildAnimatorStateMachine cm) => cm.stateMachine).GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    AnimatorStateMachine current = enumerator.Current;
                                    queue.Enqueue(current);
                                }
                            }
                            finally
                            {
                                if (enumerator != null)
                                {
                                    enumerator.Dispose();
                                }
                            }

                            IEnumerator<AnimatorState> enumerator2 = animatorStateMachine.states.Select((ChildAnimatorState cs) => cs.state).GetEnumerator();
                            try
                            {
                                while (enumerator2.MoveNext())
                                {
                                    second._ParamsReg = enumerator2.Current;
                                    if (!(second._ParamsReg.motion is UnityEditor.Animations.BlendTree blendTree))
                                    {
                                        continue;
                                    }

                                    if (blendTree.blendType == BlendTreeType.Direct)
                                    {
                                        if (ChangeMapper(blendTree.children.Any((ChildMotion c) => c.directBlendParameter == CS_0024_003C_003E8__locals12.tagReg.name), ref visitor, ref comp, ref second))
                                        {
                                            break;
                                        }
                                    }
                                    else if (ChangeMapper(blendTree.blendParameter == CS_0024_003C_003E8__locals12.tagReg.name, ref visitor, ref comp, ref second) || ChangeMapper(blendTree.blendType != BlendTreeType.Simple1D && blendTree.blendParameterY == CS_0024_003C_003E8__locals12.tagReg.name, ref visitor, ref comp, ref second))
                                    {
                                        break;
                                    }
                                }
                            }
                            finally
                            {
                                if (enumerator2 != null)
                                {
                                    enumerator2.Dispose();
                                }
                            }
                        }
                    }

                    if (visitor.m_RequestReg)
                    {
                        break;
                    }
                }

                if (visitor.m_RequestReg && !string.IsNullOrEmpty(visitor.m_PrinterReg) && !EditorUtility.DisplayDialog("WARNING!", "WARNING! This float is used in a blendtree! Converting " + CS_0024_003C_003E8__locals12.tagReg.name + " will make blendtrees using it stop working.\n[" + visitor.m_PrinterReg + "]", "Continue", "Cancel"))
                {
                    return;
                }
            }

            if ((bool)AddInitializer().warnParameterConversion)
            {
                switch (EditorUtility.DisplayDialogComplex("WARNING!", "WARNING: MOST CONVERSIONS ARE LOSSY!\nConverted animator parameters may not handle all existing condition cases the same way as their original types! Use at your own risk! Undo is possible.", "Continue", "Always Continue", "Cancel"))
                {
                    case 2:
                        return;
                    case 1:
                        AddInitializer().warnParameterConversion.ExcludeDefinition(excludeparam: false);
                        break;
                }
            }

            Undo.RegisterCompleteObjectUndo(v, "Convert Parameter");
            try
            {
                int num = v.layers.Length;
                float num2 = 1f / (float)num;
                AssetDatabase.StartAssetEditing();
                for (int num3 = 0; num3 < num; num3++)
                {
                    UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = v.layers[num3];
                    EditorUtility.DisplayProgressBar("Converting Parameter", $"{animatorControllerLayer.name} ({num3 + 1}/{num})", (float)(num3 + 1) * num2);
                    PopAlgo(v, animatorControllerLayer.stateMachine, CS_0024_003C_003E8__locals12.tagReg.name, CS_0024_003C_003E8__locals12.tagReg.type, consumer);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            UnityEngine.AnimatorControllerParameterType type = CS_0024_003C_003E8__locals12.tagReg.type;
            parameters[sizecol].type = consumer;
            v.parameters = parameters;
            CustomizeAnnotation($"Finished converting {CS_0024_003C_003E8__locals12.tagReg.name} from {type} to {consumer}!");
        }

        internal static void PopAlgo(UnityEditor.Animations.AnimatorController config, AnimatorStateMachine ivk, string dir, UnityEngine.AnimatorControllerParameterType last2, UnityEngine.AnimatorControllerParameterType value3)
        {
            _003C_003Ec__DisplayClass384_0 asset = default(_003C_003Ec__DisplayClass384_0);
            asset.listenerReg = dir;
            asset.m_GetterReg = last2;
            asset.m_InterceptorReg = value3;
            if (asset.m_GetterReg == asset.m_InterceptorReg)
            {
                return;
            }

            Undo.RecordObject(ivk, "Convert Parameter");
            ChildAnimatorState[] states = ivk.states;
            int num = 0;
            AnimatorTransitionBase[] transitions;
            while (num < states.Length)
            {
                ChildAnimatorState childAnimatorState = states[num];
                Undo.RecordObject(childAnimatorState.state, "Convert Parameter");
                transitions = childAnimatorState.state.transitions;
                SortMapper(transitions, out var counter, out var helper, ref asset);
                using (List<AnimatorTransitionBase>.Enumerator enumerator = counter.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorTransitionBase current = enumerator.Current;
                        childAnimatorState.state.AddTransition((AnimatorStateTransition)current);
                        AssetDatabase.AddObjectToAsset(current, config);
                        current.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                using (List<AnimatorTransitionBase>.Enumerator enumerator = helper.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorTransitionBase current2 = enumerator.Current;
                        childAnimatorState.state.RemoveTransition((AnimatorStateTransition)current2);
                    }
                }

                num++;
            }

            transitions = ivk.entryTransitions;
            SortMapper(transitions, out var counter2, out var helper2, ref asset);
            foreach (AnimatorTransitionBase item in counter2)
            {
                if (!item.destinationState)
                {
                    if ((bool)item.destinationStateMachine)
                    {
                        AnimatorTransition cust = ivk.AddEntryTransition(item.destinationStateMachine);
                        TestAlgo(item, cust);
                    }
                }
                else
                {
                    AnimatorTransition cust2 = ivk.AddEntryTransition(item.destinationState);
                    TestAlgo(item, cust2);
                }
            }

            using (List<AnimatorTransitionBase>.Enumerator enumerator = helper2.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    AnimatorTransitionBase current4 = enumerator.Current;
                    ivk.RemoveEntryTransition((AnimatorTransition)current4);
                }
            }

            transitions = ivk.anyStateTransitions;
            SortMapper(transitions, out var counter3, out var helper3, ref asset);
            foreach (AnimatorTransitionBase item2 in counter3)
            {
                if ((bool)item2.destinationState)
                {
                    AnimatorStateTransition cust3 = ivk.AddAnyStateTransition(item2.destinationState);
                    TestAlgo(item2, cust3);
                }
                else if ((bool)item2.destinationStateMachine)
                {
                    AnimatorStateTransition cust4 = ivk.AddAnyStateTransition(item2.destinationStateMachine);
                    TestAlgo(item2, cust4);
                }
            }

            using (List<AnimatorTransitionBase>.Enumerator enumerator = helper3.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    AnimatorTransitionBase current6 = enumerator.Current;
                    ivk.RemoveAnyStateTransition((AnimatorStateTransition)current6);
                }
            }

            ChildAnimatorStateMachine[] stateMachines = ivk.stateMachines;
            num = 0;
            while (num < stateMachines.Length)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[num];
                AnimatorTransition[] stateMachineTransitions = ivk.GetStateMachineTransitions(childAnimatorStateMachine.stateMachine);
                transitions = stateMachineTransitions;
                SortMapper(transitions, out var counter4, out var helper4, ref asset);
                ivk.SetStateMachineTransitions(childAnimatorStateMachine.stateMachine, stateMachineTransitions.Union(counter4).Except(helper4).Cast<AnimatorTransition>().ToArray());
                using (List<AnimatorTransitionBase>.Enumerator enumerator = counter4.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorTransitionBase current7 = enumerator.Current;
                        AssetDatabase.AddObjectToAsset(current7, config);
                        current7.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                using (List<AnimatorTransitionBase>.Enumerator enumerator = helper4.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AssetDatabase.RemoveObjectFromAsset(enumerator.Current);
                    }
                }

                num++;
            }

            stateMachines = ivk.stateMachines;
            foreach (ChildAnimatorStateMachine childAnimatorStateMachine2 in stateMachines)
            {
                PopAlgo(config, childAnimatorStateMachine2.stateMachine, asset.listenerReg, asset.m_GetterReg, asset.m_InterceptorReg);
            }
        }

        internal static void ComputeAlgo(AnimatorTransitionBase[] v, string vis, UnityEngine.AnimatorControllerParameterType serv, UnityEngine.AnimatorControllerParameterType task2, out List<AnimatorTransitionBase> init3, out List<AnimatorTransitionBase> def4)
        {
            _003C_003Ec__DisplayClass385_0 CS_0024_003C_003E8__locals60 = new _003C_003Ec__DisplayClass385_0();
            CS_0024_003C_003E8__locals60._CreatorReg = vis;
            init3 = (def4 = null);
            if (serv == task2)
            {
                return;
            }

            init3 = new List<AnimatorTransitionBase>();
            def4 = new List<AnimatorTransitionBase>();
            for (int i = 0; i < v.Length; i++)
            {
                AnimatorTransitionBase animatorTransitionBase = v[i];
                if (def4.Contains(animatorTransitionBase))
                {
                    continue;
                }

                Undo.RecordObject(animatorTransitionBase, "Convert Parameter");
                AnimatorCondition[] array = animatorTransitionBase.conditions;
                _003C_003Ec__DisplayClass385_1 CS_0024_003C_003E8__locals61 = new _003C_003Ec__DisplayClass385_1();
                CS_0024_003C_003E8__locals61.infoReg = array.Length - 1;
                while (CS_0024_003C_003E8__locals61.infoReg >= 0)
                {
                    if (!(array[CS_0024_003C_003E8__locals61.infoReg].parameter != CS_0024_003C_003E8__locals60._CreatorReg))
                    {
                        bool flag = serv == UnityEngine.AnimatorControllerParameterType.Bool;
                        bool flag2 = (flag ? (array[CS_0024_003C_003E8__locals61.infoReg].mode == AnimatorConditionMode.If) : (array[CS_0024_003C_003E8__locals61.infoReg].threshold > 0.008f));
                        switch (task2)
                        {
                            case UnityEngine.AnimatorControllerParameterType.Bool:
                                switch (serv)
                                {
                                    case UnityEngine.AnimatorControllerParameterType.Float:
                                        if (!(array[CS_0024_003C_003E8__locals61.infoReg].threshold >= 0f))
                                        {
                                            array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.IfNot;
                                        }
                                        else if (!(array[CS_0024_003C_003E8__locals61.infoReg].threshold > 1f))
                                        {
                                            array[CS_0024_003C_003E8__locals61.infoReg].mode = ((array[CS_0024_003C_003E8__locals61.infoReg].mode == AnimatorConditionMode.Greater) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
                                        }
                                        else
                                        {
                                            array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.If;
                                        }

                                        break;
                                    case UnityEngine.AnimatorControllerParameterType.Int:
                                        switch (array[CS_0024_003C_003E8__locals61.infoReg].mode)
                                        {
                                            default:
                                                if (!(array[CS_0024_003C_003E8__locals61.infoReg].threshold < 0f))
                                                {
                                                    if (array[CS_0024_003C_003E8__locals61.infoReg].threshold <= 1f)
                                                    {
                                                        array[CS_0024_003C_003E8__locals61.infoReg].mode = ((array[CS_0024_003C_003E8__locals61.infoReg].mode == AnimatorConditionMode.Greater) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
                                                    }
                                                    else
                                                    {
                                                        array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.If;
                                                    }
                                                }
                                                else
                                                {
                                                    array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.IfNot;
                                                }

                                                break;
                                            case AnimatorConditionMode.NotEqual:
                                                array[CS_0024_003C_003E8__locals61.infoReg].mode = ((array[CS_0024_003C_003E8__locals61.infoReg].threshold != 1f) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
                                                break;
                                            case AnimatorConditionMode.Equals:
                                                array[CS_0024_003C_003E8__locals61.infoReg].mode = ((array[CS_0024_003C_003E8__locals61.infoReg].threshold >= 1f) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
                                                break;
                                        }

                                        break;
                                }

                                break;
                            case UnityEngine.AnimatorControllerParameterType.Float:
                                if (serv == UnityEngine.AnimatorControllerParameterType.Int)
                                {
                                    if (array[CS_0024_003C_003E8__locals61.infoReg].mode == AnimatorConditionMode.Equals)
                                    {
                                        array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.Greater;
                                        array[CS_0024_003C_003E8__locals61.infoReg].threshold -= 0.008f;
                                        AnimatorCondition item = array[CS_0024_003C_003E8__locals61.infoReg];
                                        item.mode = AnimatorConditionMode.Less;
                                        item.threshold += 0.016f;
                                        ArrayUtility.Insert(ref array, CS_0024_003C_003E8__locals61.infoReg, item);
                                    }
                                    else
                                    {
                                        if (array[CS_0024_003C_003E8__locals61.infoReg].mode != AnimatorConditionMode.NotEqual)
                                        {
                                            break;
                                        }

                                        array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.Less;
                                        array[CS_0024_003C_003E8__locals61.infoReg].threshold -= 0.008f;
                                        AnimatorTransitionBase animatorTransitionBase3;
                                        if (!(animatorTransitionBase is AnimatorStateTransition spec))
                                        {
                                            if (animatorTransitionBase is AnimatorTransition spec2)
                                            {
                                                AnimatorTransition animatorTransition = new AnimatorTransition();
                                                TestAlgo(spec2, animatorTransition);
                                                animatorTransitionBase3 = animatorTransition;
                                            }
                                            else
                                            {
                                                AnimatorTransitionBase animatorTransitionBase4 = (AnimatorTransitionBase)Activator.CreateInstance(animatorTransitionBase.GetType());
                                                TestAlgo(animatorTransitionBase, animatorTransitionBase4);
                                                animatorTransitionBase3 = animatorTransitionBase4;
                                            }
                                        }
                                        else
                                        {
                                            AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
                                            TestAlgo(spec, animatorStateTransition);
                                            animatorTransitionBase3 = animatorStateTransition;
                                        }

                                        Undo.RegisterCreatedObjectUndo(animatorTransitionBase3, "Convert Parameter");
                                        AnimatorCondition[] conditions = animatorTransitionBase3.conditions;
                                        conditions[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.Greater;
                                        conditions[CS_0024_003C_003E8__locals61.infoReg].threshold += 0.008f;
                                        animatorTransitionBase3.conditions = conditions;
                                        init3.Add(animatorTransitionBase3);
                                    }
                                }
                                else if (flag)
                                {
                                    array[CS_0024_003C_003E8__locals61.infoReg].threshold = (flag2 ? 0.992f : 0.008f);
                                    array[CS_0024_003C_003E8__locals61.infoReg].mode = (flag2 ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less);
                                }

                                break;
                            case UnityEngine.AnimatorControllerParameterType.Int:
                                if (serv == UnityEngine.AnimatorControllerParameterType.Float)
                                {
                                    float num = 0.9f;
                                    float num2 = 0.032f;
                                    AnimatorConditionMode mode = array[CS_0024_003C_003E8__locals61.infoReg].mode;
                                    bool flag3 = false;
                                    for (int num3 = CS_0024_003C_003E8__locals61.infoReg; num3 >= 0; num3--)
                                    {
                                        if (!(array[num3].parameter != CS_0024_003C_003E8__locals60._CreatorReg) && mode != array[num3].mode)
                                        {
                                            bool flag4 = mode == AnimatorConditionMode.Less && array[num3].threshold >= array[CS_0024_003C_003E8__locals61.infoReg].threshold - num;
                                            bool flag5 = mode == AnimatorConditionMode.Greater && array[num3].threshold <= array[CS_0024_003C_003E8__locals61.infoReg].threshold + num;
                                            if (flag4 || flag5)
                                            {
                                                int num4 = (flag4 ? Mathf.FloorToInt(array[CS_0024_003C_003E8__locals61.infoReg].threshold) : Mathf.FloorToInt(array[num3].threshold));
                                                array[num3].threshold = num4;
                                                array[num3].mode = AnimatorConditionMode.Equals;
                                                ArrayUtility.RemoveAt(ref array, CS_0024_003C_003E8__locals61.infoReg);
                                                flag3 = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (flag3)
                                    {
                                        break;
                                    }

                                    for (int j = i + 1; j < v.Length; j++)
                                    {
                                        _003C_003Ec__DisplayClass385_2 CS_0024_003C_003E8__locals62 = new _003C_003Ec__DisplayClass385_2();
                                        AnimatorTransitionBase animatorTransitionBase2 = v[j];
                                        CS_0024_003C_003E8__locals62.advisorReg = animatorTransitionBase2.conditions.FindResolver((AnimatorCondition c) => c.parameter == CS_0024_003C_003E8__locals60._CreatorReg);
                                        if (CS_0024_003C_003E8__locals62.advisorReg == -1)
                                        {
                                            continue;
                                        }

                                        AnimatorCondition animatorCondition = animatorTransitionBase2.conditions[CS_0024_003C_003E8__locals62.advisorReg];
                                        if (animatorCondition.mode == mode)
                                        {
                                            continue;
                                        }

                                        bool num5 = mode == AnimatorConditionMode.Less && animatorCondition.threshold <= array[CS_0024_003C_003E8__locals61.infoReg].threshold + num2;
                                        bool flag6 = mode == AnimatorConditionMode.Greater && animatorCondition.threshold >= array[CS_0024_003C_003E8__locals61.infoReg].threshold - num2;
                                        if (num5 || flag6)
                                        {
                                            IEnumerable<AnimatorCondition> key = array.Where((AnimatorCondition _, int index) => index != CS_0024_003C_003E8__locals61.infoReg);
                                            IEnumerable<AnimatorCondition> b = animatorTransitionBase2.conditions.Where((AnimatorCondition _, int index) => index != CS_0024_003C_003E8__locals62.advisorReg);
                                            if (ControllerEditorUtility.ConditionsEqual(key, b))
                                            {
                                                array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.NotEqual;
                                                array[CS_0024_003C_003E8__locals61.infoReg].threshold = Mathf.RoundToInt((array[CS_0024_003C_003E8__locals61.infoReg].threshold + animatorCondition.threshold) / 2f);
                                                def4.Add(animatorTransitionBase2);
                                                flag3 = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!flag3)
                                    {
                                        array[CS_0024_003C_003E8__locals61.infoReg].threshold = Mathf.RoundToInt(array[CS_0024_003C_003E8__locals61.infoReg].threshold);
                                    }
                                }
                                else if (flag)
                                {
                                    array[CS_0024_003C_003E8__locals61.infoReg].threshold = (flag2 ? 1 : 0);
                                    array[CS_0024_003C_003E8__locals61.infoReg].mode = AnimatorConditionMode.Equals;
                                }

                                break;
                        }
                    }

                    CS_0024_003C_003E8__locals61.infoReg--;
                }

                animatorTransitionBase.conditions = array;
            }

            if (init3.Any())
            {
                List<AnimatorTransitionBase> init4 = init3;
                List<AnimatorTransitionBase> def5 = def4;
                int num6 = 0;
                do
                {
                    ComputeAlgo(init4.ToArray(), CS_0024_003C_003E8__locals60._CreatorReg, serv, task2, out init4, out def5);
                    init3.AddRange(init4);
                    def4.AddRange(def5);
                    num6++;
                }
                while (init4.Any() && num6 <= 30);
            }
        }

        private static void MoveAlgo()
        {
            m_ClassAnnotation = !m_ClassAnnotation;
            if (m_ClassAnnotation)
            {
                serverAnnotation.Clear();
                containerAnnotation = false;
                baseAnnotation = false;
            }
            else
            {
                ConcatAlgo();
            }
        }

        private static void ConcatAlgo()
        {
            List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
            if (m_AlgoAnnotation.Count <= 0)
            {
                if (m_WrapperAnnotation.Length == 0 && !collection)
                {
                    return;
                }
            }

            AnimatorStateMachine[] wrapperAnnotation;
            foreach (AnimatorState item in _VisitorAnnotation)
            {
                using (List<AnimatorState>.Enumerator enumerator2 = m_AlgoAnnotation.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        AnimatorState current2 = enumerator2.Current;
                        list.Add(item.AddTransition(current2));
                    }
                }

                wrapperAnnotation = m_WrapperAnnotation;
                foreach (AnimatorStateMachine destinationStateMachine in wrapperAnnotation)
                {
                    list.Add(item.AddTransition(destinationStateMachine));
                }

                if (collection)
                {
                    list.Add(item.AddExitTransition());
                }
            }

            if (m_Template)
            {
                using (List<AnimatorState>.Enumerator enumerator = m_AlgoAnnotation.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorState current3 = enumerator.Current;
                        list.Add(ManageMapper().AddAnyStateTransition(current3));
                    }
                }

                wrapperAnnotation = m_WrapperAnnotation;
                foreach (AnimatorStateMachine destinationStateMachine2 in wrapperAnnotation)
                {
                    list.Add(ManageMapper().AddAnyStateTransition(destinationStateMachine2));
                }
            }

            if (m_Message)
            {
                using (List<AnimatorState>.Enumerator enumerator = m_AlgoAnnotation.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorState current4 = enumerator.Current;
                        list.Add(RevertMapper().AddEntryTransition(current4));
                    }
                }

                wrapperAnnotation = m_WrapperAnnotation;
                int i = 0;
                for (; i < wrapperAnnotation.Length; i++)
                {
                    AnimatorStateMachine destinationStateMachine3 = wrapperAnnotation[i];
                    list.Add(RevertMapper().AddEntryTransition(destinationStateMachine3));
                }
            }

            wrapperAnnotation = _AnnotationAnnotation;
            foreach (AnimatorStateMachine sourceStateMachine in wrapperAnnotation)
            {
                using (List<AnimatorState>.Enumerator enumerator = m_AlgoAnnotation.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AnimatorState current5 = enumerator.Current;
                        list.Add(RevertMapper().AddStateMachineTransition(sourceStateMachine, current5));
                    }
                }

                AnimatorStateMachine[] wrapperAnnotation2 = m_WrapperAnnotation;
                foreach (AnimatorStateMachine destinationStateMachine4 in wrapperAnnotation2)
                {
                    list.Add(RevertMapper().AddStateMachineTransition(sourceStateMachine, destinationStateMachine4));
                }

                if (collection)
                {
                    list.Add(RevertMapper().AddStateMachineExitTransition(sourceStateMachine));
                }
            }

            list.ForEach(delegate (AnimatorTransitionBase t)
            {
                RateAlgo(AddInitializer().defaultTransition, t);
            });
            UnityEngine.Object[] objects = list.ToArray();
            Selection.objects = objects;
            if (m_Template || m_Message)
            {
                InterruptAlgo(wantfirst: false);
            }
        }

        private static void CallAlgo()
        {
            _003C_003Ec__DisplayClass389_0 CS_0024_003C_003E8__locals27 = new _003C_003Ec__DisplayClass389_0();
            CS_0024_003C_003E8__locals27._PrototypeReg = typeof(AnimatorStateMachine).GetMethod("MoveState", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo method = typeof(AnimatorStateMachine).GetMethod("MoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
            IEnumerable<ChildAnimatorState> first = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state));
            IEnumerable<ChildAnimatorStateMachine> first2 = RevertMapper().stateMachines.Where((ChildAnimatorStateMachine c) => m_WrapperAnnotation.Contains(c.stateMachine));
            CS_0024_003C_003E8__locals27.m_CallbackReg = new Dictionary<UnityEngine.Object, Vector3>();
            CS_0024_003C_003E8__locals27.indexerReg = Vector3.zero;
            CS_0024_003C_003E8__locals27.m_IssuerReg = 0;
            first.InvokeResolver(delegate (ChildAnimatorState c)
            {
                CS_0024_003C_003E8__locals27.m_CallbackReg.Add(c.state, c.position);
                CS_0024_003C_003E8__locals27.indexerReg += c.position;
                CS_0024_003C_003E8__locals27.m_IssuerReg++;
            });
            first2.InvokeResolver(delegate (ChildAnimatorStateMachine c)
            {
                CS_0024_003C_003E8__locals27.m_CallbackReg.Add(c.stateMachine, c.position);
                CS_0024_003C_003E8__locals27.indexerReg += c.position;
                CS_0024_003C_003E8__locals27.m_IssuerReg++;
            });
            CS_0024_003C_003E8__locals27.indexerReg /= (float)CS_0024_003C_003E8__locals27.m_IssuerReg;
            CS_0024_003C_003E8__locals27.m_RuleReg = RevertMapper().AddStateMachine("New StateMachine", CS_0024_003C_003E8__locals27.indexerReg);
            m_AlgoAnnotation.ForEach(delegate (AnimatorState s)
            {
                CS_0024_003C_003E8__locals27._PrototypeReg.Invoke(RevertMapper(), new object[2]{s, CS_0024_003C_003E8__locals27.m_RuleReg});
            });
            AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
            foreach (AnimatorStateMachine animatorStateMachine in wrapperAnnotation)
            {
                AnimatorTransition[] stateMachineTransitions = RevertMapper().GetStateMachineTransitions(animatorStateMachine);
                RevertMapper().SetStateMachineTransitions(animatorStateMachine, null);
                method.Invoke(RevertMapper(), new object[2]{animatorStateMachine, CS_0024_003C_003E8__locals27.m_RuleReg});
                AnimatorTransition[] array = RevertMapper().GetStateMachineTransitions(CS_0024_003C_003E8__locals27.m_RuleReg);
                ArrayUtility.AddRange(ref array, stateMachineTransitions);
                RevertMapper().SetStateMachineTransitions(CS_0024_003C_003E8__locals27.m_RuleReg, array);
            }

            ChildAnimatorState[] states = CS_0024_003C_003E8__locals27.m_RuleReg.states;
            for (int num2 = 0; num2 < states.Length; num2++)
            {
                if (CS_0024_003C_003E8__locals27.m_CallbackReg.ContainsKey(states[num2].state))
                {
                    states[num2].position = CS_0024_003C_003E8__locals27.m_CallbackReg[states[num2].state];
                }
            }

            CS_0024_003C_003E8__locals27.m_RuleReg.states = states;
            ChildAnimatorStateMachine[] stateMachines = CS_0024_003C_003E8__locals27.m_RuleReg.stateMachines;
            for (int num3 = 0; num3 < stateMachines.Length; num3++)
            {
                if (CS_0024_003C_003E8__locals27.m_CallbackReg.ContainsKey(stateMachines[num3].stateMachine))
                {
                    stateMachines[num3].position = CS_0024_003C_003E8__locals27.m_CallbackReg[stateMachines[num3].stateMachine];
                }
            }

            CS_0024_003C_003E8__locals27.m_RuleReg.stateMachines = stateMachines;
        }

        private static void CancelAlgo()
        {
            MethodInfo method = typeof(AnimatorStateMachine).GetMethod("MoveState", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo method2 = typeof(AnimatorStateMachine).GetMethod("MoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<UnityEngine.Object, Vector3> dictionary = new Dictionary<UnityEngine.Object, Vector3>();
            for (int i = 0; i < m_WrapperAnnotation.Length; i++)
            {
                _003C_003Ec__DisplayClass390_0 CS_0024_003C_003E8__locals12 = new _003C_003Ec__DisplayClass390_0();
                CS_0024_003C_003E8__locals12._SingletonReg = m_WrapperAnnotation[i];
                if (!(RevertMapper() == CS_0024_003C_003E8__locals12._SingletonReg))
                {
                    Vector3 vector = RevertMapper().stateMachines.First((ChildAnimatorStateMachine c) => c.stateMachine == CS_0024_003C_003E8__locals12._SingletonReg).position;
                    Vector3 seed = CS_0024_003C_003E8__locals12._SingletonReg.states.Aggregate(Vector3.zero, (Vector3 current, ChildAnimatorState child) => current + child.position);
                    seed = CS_0024_003C_003E8__locals12._SingletonReg.stateMachines.Aggregate(seed, (Vector3 current, ChildAnimatorStateMachine child) => current + child.position);
                    seed /= (float)(CS_0024_003C_003E8__locals12._SingletonReg.stateMachines.Length + CS_0024_003C_003E8__locals12._SingletonReg.states.Length);
                    ChildAnimatorState[] states = CS_0024_003C_003E8__locals12._SingletonReg.states;
                    for (int num = 0; num < states.Length; num++)
                    {
                        ChildAnimatorState childAnimatorState = states[num];
                        dictionary.Add(childAnimatorState.state, childAnimatorState.position - seed + vector);
                        method.Invoke(CS_0024_003C_003E8__locals12._SingletonReg, new object[2]{childAnimatorState.state, RevertMapper()});
                    }

                    ChildAnimatorStateMachine[] stateMachines = CS_0024_003C_003E8__locals12._SingletonReg.stateMachines;
                    for (int num = 0; num < stateMachines.Length; num++)
                    {
                        ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[num];
                        dictionary.Add(childAnimatorStateMachine.stateMachine, childAnimatorStateMachine.position - seed + vector);
                        method2.Invoke(CS_0024_003C_003E8__locals12._SingletonReg, new object[2]{childAnimatorStateMachine.stateMachine, RevertMapper()});
                    }

                    RevertMapper().RemoveStateMachine(CS_0024_003C_003E8__locals12._SingletonReg);
                }
            }

            ChildAnimatorState[] states2 = RevertMapper().states;
            for (int num2 = 0; num2 < states2.Length; num2++)
            {
                if (dictionary.TryGetValue(states2[num2].state, out var value))
                {
                    states2[num2].position = value;
                }
            }

            RevertMapper().states = states2;
            ChildAnimatorStateMachine[] stateMachines2 = RevertMapper().stateMachines;
            for (int num3 = 0; num3 < stateMachines2.Length; num3++)
            {
                if (dictionary.TryGetValue(stateMachines2[num3].stateMachine, out var value2))
                {
                    stateMachines2[num3].position = value2;
                }
            }

            RevertMapper().stateMachines = stateMachines2;
        }

        private static void CountAlgo()
        {
            ChildAnimatorState[] source = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state)).ToArray();
            float x = source.Max((ChildAnimatorState c) => c.position.x);
            ChildAnimatorState[] states = RevertMapper().states;
            for (int num = 0; num < states.Length; num++)
            {
                if (source.Contains(states[num]))
                {
                    states[num].position = new Vector3(x, states[num].position.y);
                }
            }

            if (m_Message)
            {
                RevertMapper().entryPosition = new Vector3(x, RevertMapper().entryPosition.y);
            }

            if (m_Template)
            {
                RevertMapper().anyStatePosition = new Vector3(x, RevertMapper().anyStatePosition.y);
            }

            if (collection)
            {
                RevertMapper().exitPosition = new Vector3(x, RevertMapper().exitPosition.y);
            }

            Undo.RecordObject(RevertMapper(), "Align Horizontal");
            RevertMapper().states = states;
            EditorUtility.SetDirty(LogoutMapper());
            if (m_Message || collection)
            {
                InterruptAlgo(wantfirst: false);
            }
        }

        private static void DisableAlgo()
        {
            ChildAnimatorState[] source = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state)).ToArray();
            float y = source.Min((ChildAnimatorState c) => c.position.y);
            ChildAnimatorState[] states = RevertMapper().states;
            for (int num = 0; num < states.Length; num++)
            {
                if (source.Contains(states[num]))
                {
                    states[num].position = new Vector3(states[num].position.x, y);
                }
            }

            if (m_Message)
            {
                RevertMapper().entryPosition = new Vector3(RevertMapper().entryPosition.x, y);
            }

            if (m_Template)
            {
                RevertMapper().anyStatePosition = new Vector3(RevertMapper().anyStatePosition.x, y);
            }

            if (collection)
            {
                RevertMapper().exitPosition = new Vector3(RevertMapper().exitPosition.x, y);
            }

            Undo.RecordObject(RevertMapper(), "Align Vertical");
            RevertMapper().states = states;
            EditorUtility.SetDirty(LogoutMapper());
            if (m_Message || collection)
            {
                InterruptAlgo(wantfirst: false);
            }
        }

        private static void InsertAlgo()
        {
            while (true)
            {
                int num;
                if (m_AlgoAnnotation.Count == 0)
                {
                    num = 0;
                    goto IL_001d;
                }

                goto IL_0133;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                                break;
                            case 4:
                                return;
                            case 3:
                                return;
                            default:
                                goto case 2;
                            case 2:
                                Selection.objects = Selection.objects.Concat(RevertMapper().states.Select((ChildAnimatorState c) => c.state)).ToArray();
                                num = 4;
                                continue;
                            case 5:
                                goto IL_0133;
                        }

                        break;
                    }

                continue;
                IL_0133:
                    Selection.objects = Selection.objects.Where((UnityEngine.Object o) => !(o is AnimatorState)).ToArray();
                num = 3;
                goto IL_001d;
            }
        }

        private static void RestartAlgo(AnimatorStateMachine param, IEnumerable<AnimatorState> vis, string serv)
        {
            IEnumerator<AnimatorState> enumerator = vis.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AnimatorState current = enumerator.Current;
                    if (current.name != serv)
                    {
                        Undo.RecordObject(current, "Rename States");
                        current.name = param.MakeUniqueStateName(serv);
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
        }

        private static void QueryAlgo(AnimatorState i)
        {
            AddAlgo(AddInitializer().defaultState, i);
        }

        private static void AddAlgo(AnimatorState last, AnimatorState reg)
        {
            string text = reg.name;
            Motion motion = reg.motion;
            EditorUtility.CopySerialized(last, reg);
            reg.name = text;
            reg.motion = motion;
            EditorUtility.SetDirty(reg);
        }

        private static string[] InvokeAlgo(AnimatorState setup)
        {
            if (setup.tag.FlushResolver())
            {
                return Array.Empty<string>();
            }

            return setup.tag.Split(new char[1]{','});
        }

        private static bool VisitAlgo(AnimatorState asset)
        {
            return InvokeAlgo(asset).Any((string t) => m_SerializerAnnotation.Contains(t));
        }

        private static void ReadAlgo()
        {
            while (true)
            {
                UnityEngine.Object obj = Selection.objects.FirstOrDefault(delegate (UnityEngine.Object o)
                {
                    if (!(o is AnimatorState))
                    {
                        return o is AnimatorStateMachine;
                    }

                    return true;
                });
                int num = 1;
                while (true)
                {
                    object obj2;
                    switch (num)
                    {
                        case 6:
                            return;
                        case 4:
                        {
                            AnimatorState obj4 = obj as AnimatorState;
                            if ((object)obj4 == null)
                            {
                                num = 5;
                                continue;
                            }

                            obj2 = obj4.behaviours;
                            goto IL_0113;
                        }

                        case 3:
                            return;
                        case 2:
                            break;
                        case 1:
                            goto case 4;
                        case 5:
                            obj2 = null;
                            goto IL_0113;
                        default:
                        {
                            AnimatorStateMachine obj3 = obj as AnimatorStateMachine;
                            if ((object)obj3 == null)
                            {
                                num = 7;
                                continue;
                            }

                            obj2 = obj3.behaviours;
                            goto IL_016f;
                        }

                        case 7:
                        {
                            obj2 = null;
                            goto IL_016f;
                        }

                            IL_0113:
                                if (obj2 == null)
                                {
                                    num = 0;
                                    continue;
                                }

                            goto IL_016f;
                            IL_016f:
                                @struct = (StateMachineBehaviour[])obj2;
                            num = 3;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void SelectAlgo()
        {
            AnimatorState[] array = m_AlgoAnnotation.ToArray();
            AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
            UnityEngine.Object[] objectsToUndo = array;
            Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
            objectsToUndo = wrapperAnnotation;
            Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
            StateMachineBehaviour[] array2 = @struct;
            foreach (StateMachineBehaviour stateMachineBehaviour in array2)
            {
                Type type = stateMachineBehaviour.GetType();
                AnimatorState[] array3 = array;
                foreach (AnimatorState animatorState in array3)
                {
                    EditorUtility.CopySerialized(stateMachineBehaviour, animatorState.AddStateMachineBehaviour(type));
                }

                AnimatorStateMachine[] array4 = wrapperAnnotation;
                foreach (AnimatorStateMachine animatorStateMachine in array4)
                {
                    EditorUtility.CopySerialized(stateMachineBehaviour, animatorStateMachine.AddStateMachineBehaviour(type));
                }
            }
        }

        private static bool RemoveAlgo()
        {
            if (@struct == null)
            {
                return false;
            }

            return @struct.Length != 0;
        }

        private static void InstantiateAlgo()
        {
            AnimatorState[] array = m_AlgoAnnotation.ToArray();
            AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
            UnityEngine.Object[] objectsToUndo = array;
            Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
            objectsToUndo = wrapperAnnotation;
            Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
            AnimatorState[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                array2[i].behaviours = Array.Empty<StateMachineBehaviour>();
            }

            AnimatorStateMachine[] array3 = wrapperAnnotation;
            for (int i = 0; i < array3.Length; i++)
            {
                array3[i].behaviours = Array.Empty<StateMachineBehaviour>();
            }
        }

        private static List<AnimatorStateTransition> AwakeAlgo(List<AnimatorStateTransition> value = null)
        {
            _003C_003Ec__DisplayClass410_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass410_0();
            CS_0024_003C_003E8__locals4._FactoryReg = value;
            if (CS_0024_003C_003E8__locals4._FactoryReg == null)
            {
                CS_0024_003C_003E8__locals4._FactoryReg = _TestsAnnotation;
            }

            return ManageMapper().anyStateTransitions.Where(delegate (AnimatorStateTransition t)
            {
                _003C_003Ec__DisplayClass410_1 _003C_003Ec__DisplayClass410_ = new _003C_003Ec__DisplayClass410_1();
                _003C_003Ec__DisplayClass410_._AccountReg = t;
                return CS_0024_003C_003E8__locals4._FactoryReg.Contains(_003C_003Ec__DisplayClass410_._AccountReg) && (RevertMapper().states.Any(_003C_003Ec__DisplayClass410_.SetupThread) || RevertMapper().stateMachines.Any(_003C_003Ec__DisplayClass410_.EnableThread));
            }).ToList();
        }

        private static List<AnimatorTransitionBase> ResetAlgo(List<AnimatorTransitionBase> info = null)
        {
            if (info == null)
            {
                info = propertyAnnotation;
            }

            return info.Where((AnimatorTransitionBase t) => RevertMapper().entryTransitions.Contains(t)).ToList();
        }

        private static (AnimatorState, List<AnimatorStateTransition>)[] FlushAlgo(List<AnimatorStateTransition> var1 = null)
        {
            _003C_003Ec__DisplayClass412_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass412_0();
            CS_0024_003C_003E8__locals9.refReg = var1;
            if (CS_0024_003C_003E8__locals9.refReg == null)
            {
                CS_0024_003C_003E8__locals9.refReg = _TestsAnnotation;
            }

            List<AnimatorState> list = new List<AnimatorState>();
            for (int i = 0; i < RevertMapper().states.Length; i++)
            {
                if (RevertMapper().states[i].state.transitions.Any((AnimatorStateTransition t) => CS_0024_003C_003E8__locals9.refReg.Contains(t)))
                {
                    list.Add(RevertMapper().states[i].state);
                }
            }

            (AnimatorState, List<AnimatorStateTransition>)[] array = new (AnimatorState, List<AnimatorStateTransition>)[list.Count];
            for (int num = 0; num < list.Count; num++)
            {
                List<AnimatorStateTransition> list2 = new List<AnimatorStateTransition>();
                for (int num2 = 0; num2 < CS_0024_003C_003E8__locals9.refReg.Count; num2++)
                {
                    if (list[num].transitions.Contains(CS_0024_003C_003E8__locals9.refReg[num2]))
                    {
                        list2.Add(CS_0024_003C_003E8__locals9.refReg[num2]);
                    }
                }

                CS_0024_003C_003E8__locals9.refReg = CS_0024_003C_003E8__locals9.refReg.Except(list2).ToList();
                array[num] = (list[num], list2);
            }

            return array;
        }

        private static void ConnectAlgo()
        {
            _003C_003Ec__DisplayClass413_0 CS_0024_003C_003E8__locals24 = new _003C_003Ec__DisplayClass413_0();
            CS_0024_003C_003E8__locals24.tokenReg = new List<AnimatorTransitionBase>();
            (AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(_TestsAnnotation);
            CS_0024_003C_003E8__locals24.codeReg = AwakeAlgo();
            CS_0024_003C_003E8__locals24.m_DicReg = ResetAlgo();
            (AnimatorState, List<List<AnimatorStateTransition>>)[] array2 = new (AnimatorState, List<List<AnimatorStateTransition>>)[array.Length];
            List<List<AnimatorStateTransition>> list = new List<List<AnimatorStateTransition>>();
            List<List<AnimatorTransitionBase>> list2 = new List<List<AnimatorTransitionBase>>();
            for (int i = 0; i < array2.Length; i++)
            {
                _003C_003Ec__DisplayClass413_4 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass413_4();
                AnimatorState item = array[i].Item1;
                CS_0024_003C_003E8__locals22.objectReg = array[i].Item2;
                List<List<AnimatorStateTransition>> list3 = new List<List<AnimatorStateTransition>>();
                while (CS_0024_003C_003E8__locals22.objectReg.Count > 0)
                {
                    new List<AnimatorStateTransition>();
                    List<AnimatorStateTransition> list4 = CS_0024_003C_003E8__locals22.objectReg.Where((AnimatorStateTransition t) => t.destinationState == CS_0024_003C_003E8__locals22.objectReg[0].destinationState).ToList();
                    CS_0024_003C_003E8__locals22.objectReg = CS_0024_003C_003E8__locals22.objectReg.Except(list4).ToList();
                    list3.Add(list4);
                }

                array2[i] = (item, list3);
            }

            while (CS_0024_003C_003E8__locals24.codeReg.Count > 0)
            {
                new List<AnimatorStateTransition>();
                List<AnimatorStateTransition> list5 = CS_0024_003C_003E8__locals24.codeReg.Where((AnimatorStateTransition t) => t.destinationState == CS_0024_003C_003E8__locals24.codeReg[0].destinationState).ToList();
                CS_0024_003C_003E8__locals24.codeReg = CS_0024_003C_003E8__locals24.codeReg.Except(list5).ToList();
                list.Add(list5);
            }

            while (CS_0024_003C_003E8__locals24.m_DicReg.Count > 0)
            {
                new List<AnimatorTransitionBase>();
                List<AnimatorTransitionBase> list6 = CS_0024_003C_003E8__locals24.m_DicReg.Where((AnimatorTransitionBase t) => t.destinationState == CS_0024_003C_003E8__locals24.m_DicReg[0].destinationState).ToList();
                CS_0024_003C_003E8__locals24.m_DicReg = CS_0024_003C_003E8__locals24.m_DicReg.Except(list6).ToList();
                list2.Add(list6);
            }

            for (int num = 0; num < array2.Length; num++)
            {
                for (int num2 = 0; num2 < array2[num].Item2.Count; num2++)
                {
                    CS_0024_003C_003E8__locals24.PopThread(array2[num].Item2[num2], array2[num].Item1);
                }
            }

            for (int num3 = 0; num3 < list.Count; num3++)
            {
                CS_0024_003C_003E8__locals24.MoveThread(list[num3]);
            }

            for (int num4 = 0; num4 < list2.Count; num4++)
            {
                CS_0024_003C_003E8__locals24.ComputeThread(list2[num4]);
            }

            Selection.objects = Selection.objects.Concat(CS_0024_003C_003E8__locals24.tokenReg).ToArray();
            EditorUtility.SetDirty(LogoutMapper());
        }

        private static AnimatorStateTransition CalculateAlgo<T>(T value)
            where T : AnimatorTransitionBase
        {
            AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
            EditorUtility.CopySerialized(value, animatorStateTransition);
            Undo.RegisterCreatedObjectUndo(animatorStateTransition, "DuplicatedTransition");
            AssetDatabase.AddObjectToAsset(animatorStateTransition, AssetDatabase.GetAssetPath(value));
            animatorStateTransition.hideFlags = value.hideFlags;
            return animatorStateTransition;
        }

        private static void TestAlgo<T>(T spec, T cust)
            where T : AnimatorTransitionBase
        {
            if (spec.GetType() == cust.GetType())
            {
                EditorUtility.CopySerialized(spec, cust);
                return;
            }

            cust.isExit = spec.isExit;
            cust.mute = spec.mute;
            cust.name = spec.name;
            cust.solo = spec.solo;
            cust.destinationState = spec.destinationState;
            cust.destinationStateMachine = spec.destinationStateMachine;
            cust.conditions = spec.conditions;
        }

        private static AnimatorStateTransition MapAlgo<T>(T i)
            where T : AnimatorTransitionBase
        {
            AnimatorStateTransition animatorStateTransition = ManageMapper().AddAnyStateTransition(i.destinationState);
            EditorUtility.CopySerialized(i, animatorStateTransition);
            animatorStateTransition.hideFlags = i.hideFlags;
            return animatorStateTransition;
        }

        private static AnimatorTransitionBase ValidateAlgo(AnimatorTransitionBase reference)
        {
            AnimatorTransitionBase animatorTransitionBase = RevertMapper().AddEntryTransition(reference.destinationState);
            EditorUtility.CopySerialized(reference, animatorTransitionBase);
            return animatorTransitionBase;
        }

        private static void CustomizeAlgo(AnimatorStateTransition reference, AnimatorStateTransition col)
        {
            AnimatorCondition[] conditions = col.conditions;
            AnimatorStateMachine destinationStateMachine = col.destinationStateMachine;
            AnimatorState destinationState = col.destinationState;
            bool isExit = col.isExit;
            string text = col.name;
            EditorUtility.CopySerialized(reference, col);
            col.conditions = conditions;
            col.destinationStateMachine = destinationStateMachine;
            col.destinationState = destinationState;
            col.isExit = isExit;
            col.name = text;
            EditorUtility.SetDirty(col);
        }

        private static void RateAlgo(AnimatorTransitionBase key, AnimatorTransitionBase map)
        {
            AnimatorStateTransition animatorStateTransition = key as AnimatorStateTransition;
            if ((object)animatorStateTransition != null && map is AnimatorStateTransition col)
            {
                CustomizeAlgo(animatorStateTransition, col);
            }
        }

        private static void DestroyAlgo()
        {
            if (_TestsAnnotation.Count == 0)
            {
                _003C_003Ec__DisplayClass420_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass420_0();
                CS_0024_003C_003E8__locals4.valReg = new List<AnimatorStateTransition>();
                RevertMapper().AssetPredicate(delegate (AnimatorState s)
                {
                    CS_0024_003C_003E8__locals4.valReg.AddRange(s.transitions);
                }, requiresc: false);
                CS_0024_003C_003E8__locals4.valReg.AddRange(ManageMapper().anyStateTransitions.Where(delegate (AnimatorStateTransition t)
                {
                    _003C_003Ec__DisplayClass420_1 _003C_003Ec__DisplayClass420_ = new _003C_003Ec__DisplayClass420_1();
                    _003C_003Ec__DisplayClass420_._ValueReg = t;
                    return RevertMapper().states.Any(_003C_003Ec__DisplayClass420_.QueryThread);
                }));
                Selection.objects = Selection.objects.Concat(CS_0024_003C_003E8__locals4.valReg).ToArray();
            }
            else
            {
                Selection.objects = Selection.objects.Where((UnityEngine.Object o) => !(o is AnimatorStateTransition)).ToArray();
            }
        }

        private static void GetAlgo()
        {
            List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
            foreach (AnimatorState item in m_AlgoAnnotation)
            {
                list.AddRange(item.transitions);
            }

            if (m_Message)
            {
                list.AddRange(RevertMapper().entryTransitions);
            }

            if (m_Template)
            {
                list.AddRange(RevertMapper().anyStateTransitions);
            }

            Selection.objects = Selection.objects.Concat(list).ToArray();
        }

        private static void CalcAlgo()
        {
            List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
            ChildAnimatorState[] states = RevertMapper().states;
            foreach (ChildAnimatorState childAnimatorState in states)
            {
                list.AddRange(childAnimatorState.state.transitions.Where(delegate (AnimatorStateTransition t)
                {
                    if (!m_AlgoAnnotation.Contains(t.destinationState))
                    {
                        if (collection)
                        {
                            return t.isExit;
                        }

                        return false;
                    }

                    return true;
                }));
            }

            Selection.objects = Selection.objects.Concat(list).ToArray();
        }

        private static void IncludeAlgo()
        {
            List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
            foreach (AnimatorState item in m_AlgoAnnotation)
            {
                AnimatorStateTransition[] transitions = item.transitions;
                foreach (AnimatorStateTransition animatorStateTransition in transitions)
                {
                    if (m_AlgoAnnotation.Contains(animatorStateTransition.destinationState) || (animatorStateTransition.destinationState == null && collection))
                    {
                        list.Add(animatorStateTransition);
                    }
                }
            }

            if (m_Template)
            {
                list.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => m_AlgoAnnotation.Contains(t.destinationState)));
            }

            if (m_Message)
            {
                list.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => m_AlgoAnnotation.Contains(t.destinationState)));
            }

            Selection.objects = Selection.objects.Concat(list).ToArray();
        }

        private static void RunAlgo()
        {
            SerializedObject proc;
            if (!(_ProcessorAnnotation.itemTests != null))
            {
                if (_TestsAnnotation.Count <= 0)
                {
                    proc = task;
                }
                else
                {
                    UnityEngine.Object[] objs = _TestsAnnotation.ToArray();
                    proc = new SerializedObject(objs);
                }
            }
            else
            {
                proc = new SerializedObject(_ProcessorAnnotation.itemTests);
            }

            m_Proc = proc;
        }

        private static void CloneAlgo()
        {
            _FieldVisitor = !_FieldVisitor;
        }

        private static void LoginAlgo()
        {
            while (true)
            {
                containerAnnotation = !containerAnnotation;
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            return;
                        case 6:
                            return;
                        case 1:
                            break;
                        case 3:
                            m_ClassAnnotation = (baseAnnotation = false);
                            num = 6;
                            continue;
                        case 4:
                            ReflectAlgo();
                            num = 5;
                            continue;
                        case 5:
                            return;
                        default:
                            if (!containerAnnotation)
                            {
                                num = 4;
                                continue;
                            }

                            goto case 3;
                    }

                    break;
                }
            }
        }

        private static void ReflectAlgo()
        {
            var destinations = m_AlgoAnnotation.Where(state => state).Distinct().ToArray();
            if (destinations.Length == 0 && !collection) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Redirect Transitions");
            foreach (var transition in serverAnnotation)
            {
                ControllerTransitionRedirect.Apply(transition.itemTests, transition.m_SchemaTests,
                    transition.broadcasterTests, transition.globalTests, destinations, collection);
            }
            Undo.CollapseUndoOperations(group);
            PatchAlgo();
        }

        private static void DeleteAlgo()
        {
            if (_DefinitionAnnotation.Count > 0)
            {
                List<UnityEngine.Object> list = new List<UnityEngine.Object>();
                foreach (UnityAnimatorBridge.InstanceTests item in _DefinitionAnnotation)
                {
                    if (item.globalTests != UnityAnimatorBridge.TokenizerTests.NodeType.state)
                    {
                        continue;
                    }

                    AnimatorStateTransition specificationTests = item.specificationTests;
                    if (!specificationTests.destinationState)
                    {
                        return;
                    }

                    AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
                    EditorUtility.CopySerialized(specificationTests, animatorStateTransition);
                    animatorStateTransition.destinationState = item.m_SchemaTests;
                    if ((bool)AddInitializer().autoReverseModes)
                    {
                        AnimatorCondition[] conditions = animatorStateTransition.conditions;
                        for (int i = 0; i < conditions.Length; i++)
                        {
                            conditions[i] = ListAlgo(conditions[i], clientVisitor);
                        }

                        animatorStateTransition.conditions = conditions;
                    }

                    Undo.RegisterCreatedObjectUndo(animatorStateTransition, "ReversedTransitions");
                    Undo.RecordObject(specificationTests.destinationState, "ReversedTransitions");
                    AssetDatabase.AddObjectToAsset(animatorStateTransition, LogoutMapper());
                    animatorStateTransition.hideFlags = specificationTests.hideFlags;
                    specificationTests.destinationState.AddTransition(animatorStateTransition);
                    list.Add(animatorStateTransition);
                    if (_AttributeVisitor)
                    {
                        Undo.RecordObject(item.m_SchemaTests, "ReversedTransitions");
                        item.m_SchemaTests.RemoveTransition(item.specificationTests);
                        PatchAlgo();
                    }
                }

                if (list.Count > 0)
                {
                    Selection.objects = list.ToArray();
                }
            }

            EditorUtility.SetDirty(LogoutMapper());
        }

        private static void CreateAlgo()
        {
            baseAnnotation = !baseAnnotation;
            if (baseAnnotation)
            {
                m_ClassAnnotation = (containerAnnotation = false);
            }
            else
            {
                NewAlgo();
            }
        }

        private static void NewAlgo()
        {
            List<UnityAnimatorBridge.InstanceTests> list = serverAnnotation;
            if (list != null)
            {
                list.InvokeResolver(delegate (UnityAnimatorBridge.InstanceTests t)
                {
                    _003C_003Ec__DisplayClass430_0 _003C_003Ec__DisplayClass430_ = new _003C_003Ec__DisplayClass430_0();
                    _003C_003Ec__DisplayClass430_.m_MerchantReg = t;
                    m_AlgoAnnotation.InvokeResolver(_003C_003Ec__DisplayClass430_.AddThread);
                    if (m_Template && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
                    {
                        TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddAnyStateTransition((AnimatorState)null));
                    }

                    if (m_Message && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
                    {
                        TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddEntryTransition((AnimatorState)null));
                    }

                    if (_AttributeVisitor)
                    {
                        _003C_003Ec__DisplayClass430_.m_MerchantReg.InitSerializer();
                    }
                });
            }

            if (m_Template || _AttributeVisitor)
            {
                InterruptAlgo(wantfirst: false);
            }
        }

        private static void PushAlgo()
        {
            while (true)
            {
                _003C_003Ec__DisplayClass431_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass431_0();
                while (true)
                {
                    IL_00cc:
                        CS_0024_003C_003E8__locals4.authenticationReg = new HashSet<UnityEngine.Object>();
                    int num = 4;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 0:
                                return;
                            case 1:
                                return;
                            case 6:
                                break;
                            case 3:
                                Selection.objects = CS_0024_003C_003E8__locals4.authenticationReg.ToArray();
                                num = 1;
                                continue;
                            case 4:
                                _TestsAnnotation.InvokeResolver((AnimatorStateTransition sel) =>
                                {
                                    _003C_003Ec__DisplayClass431_1 _003C_003Ec__DisplayClass431_ = new _003C_003Ec__DisplayClass431_1();
                                    _003C_003Ec__DisplayClass431_.m_PoolReg = CS_0024_003C_003E8__locals4;
                                    _003C_003Ec__DisplayClass431_._ReponseReg = sel;
                                    if (_003C_003Ec__DisplayClass431_._ReponseReg.destinationState != null || _003C_003Ec__DisplayClass431_._ReponseReg.destinationStateMachine != null)
                                    {
                                        if (!ManageMapper().anyStateTransitions.Any(_003C_003Ec__DisplayClass431_.FindThread))
                                        {
                                            CS_0024_003C_003E8__locals4.authenticationReg.Add(MapAlgo(_003C_003Ec__DisplayClass431_._ReponseReg));
                                            for (int i = 0; i < RevertMapper().states.Length; i++)
                                            {
                                                if (RevertMapper().states[i].state.transitions.Any(_003C_003Ec__DisplayClass431_.InitThread))
                                                {
                                                    Undo.RecordObject(RevertMapper().states[i].state, "Make AnyTransition");
                                                    RevertMapper().states[i].state.RemoveTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            RevertMapper().states.InvokeResolver(_003C_003Ec__DisplayClass431_.ExcludeThread);
                                            _ = ManageMapper().anyStateTransitions;
                                            Undo.RecordObject(ManageMapper(), "Remove AnyTransition");
                                            ManageMapper().RemoveAnyStateTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
                                        }
                                    }
                                });
                                num = 2;
                                continue;
                            case 5:
                                goto IL_00cc;
                            case 2:
                                goto case 3;
                        }

                        break;
                    }

                    break;
                }
            }
        }

        private static void ViewAlgo()
        {
            _003C_003Ec__DisplayClass432_0 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_0();
            _003C_003Ec__DisplayClass432_._ComposerReg = new List<AnimatorTransitionBase>();
            if (!SetInitializer())
            {
                (AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(_TestsAnnotation);
                List<AnimatorStateTransition> list = AwakeAlgo(_TestsAnnotation);
                List<AnimatorTransitionBase> list2 = ResetAlgo(propertyAnnotation);
                if (_DefinitionAnnotation.Count > 0)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        AnimatorState item = array[i].Item1;
                        List<AnimatorStateTransition> item2 = array[i].Item2;
                        new List<AnimatorStateTransition>();
                        for (int j = 0; j < item2.Count; j++)
                        {
                            if (_TestsAnnotation.Contains(item2[j]))
                            {
                                _003C_003Ec__DisplayClass432_.StartThread(item2[j], item);
                            }
                        }
                    }

                    for (int k = 0; k < list.Count; k++)
                    {
                        _003C_003Ec__DisplayClass432_.DefineThread(list[k]);
                    }

                    for (int l = 0; l < list2.Count; l++)
                    {
                        _003C_003Ec__DisplayClass432_.VisitThread(list2[l]);
                    }
                }
            }
            else
            {
                int num = 0;
                if (!RevertMapper().entryTransitions.Contains(_ProcessorAnnotation.itemTests))
                {
                    if (RevertMapper().anyStateTransitions.Contains(_ProcessorAnnotation.itemTests))
                    {
                        num = 2;
                    }
                }
                else
                {
                    num = 1;
                }

                AnimatorState animatorState = null;
                switch (num)
                {
                    case 1:
                        _003C_003Ec__DisplayClass432_.VisitThread(_ProcessorAnnotation.itemTests);
                        break;
                    case 0:
                        animatorState = RevertMapper().states.First((ChildAnimatorState c) => c.state.transitions.Contains(_ProcessorAnnotation.itemTests)).state;
                        _003C_003Ec__DisplayClass432_.StartThread((AnimatorStateTransition)_ProcessorAnnotation.itemTests, animatorState);
                        break;
                    case 2:
                        _003C_003Ec__DisplayClass432_.DefineThread((AnimatorStateTransition)_ProcessorAnnotation.itemTests);
                        break;
                }
            }

            Selection.objects = Selection.objects.Concat(_003C_003Ec__DisplayClass432_._ComposerReg).ToArray();
            EditorUtility.SetDirty(LogoutMapper());
        }

        private static AnimatorConditionMode CollectAlgo(AnimatorConditionMode reference)
        {
            return reference switch
            {
                AnimatorConditionMode.Equals => AnimatorConditionMode.NotEqual,
                AnimatorConditionMode.If => AnimatorConditionMode.IfNot,
                AnimatorConditionMode.IfNot => AnimatorConditionMode.If,
                AnimatorConditionMode.NotEqual => AnimatorConditionMode.Equals,
                AnimatorConditionMode.Greater => AnimatorConditionMode.Less,
                _ => AnimatorConditionMode.Greater,
            };
        }

        private static AnimatorCondition ResolveAlgo(AnimatorCondition reference)
        {
            return ListAlgo(reference, (bool)AddInitializer().reverseModifiesValues ^ Event.current.control);
        }

        private static AnimatorCondition ListAlgo(AnimatorCondition config, bool excludeord)
        {
            AnimatorCondition result = config;
            result.mode = CollectAlgo(config.mode);
            if (excludeord)
            {
                bool flag = config.mode == AnimatorConditionMode.Greater;
                bool flag2 = config.mode == AnimatorConditionMode.Less;
                if (flag || flag2)
                {
                    int second;
                    UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(config.parameter, out second);
                    if (animatorControllerParameter != null)
                    {
                        if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
                        {
                            if (flag)
                            {
                                result.threshold += 0.008f;
                            }
                            else
                            {
                                result.threshold -= 0.008f;
                            }
                        }
                        else if (!flag)
                        {
                            result.threshold -= 1f;
                        }
                        else
                        {
                            result.threshold += 1f;
                        }
                    }
                }
            }

            return result;
        }

        [SpecialName]
        private static EditorWindow FindInitializer()
        {
            if (_IteratorAnnotation == null)
            {
                UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(mapperVisitor);
                if (array.Length == 0)
                {
                    return null;
                }

                _IteratorAnnotation = array[0] as EditorWindow;
            }

            return _IteratorAnnotation;
        }

        [SpecialName]
        private static object InitInitializer()
        {
            if (FindInitializer() == null)
            {
                return null;
            }

            return m_ProcessorVisitor.GetValue(FindInitializer());
        }

        [SpecialName]
        private static AnimationClip DefineInitializer()
        {
            if (InitInitializer() != null)
            {
                return (AnimationClip)_ObserverVisitor.GetValue(InitInitializer());
            }

            return null;
        }

        [SpecialName]
        private static void StartInitializer(AnimationClip init)
        {
            while (true)
            {
                int num;
                if (InitInitializer() == null)
                {
                    num = 0;
                }
                else
                {
                    _ObserverVisitor.SetValue(InitInitializer(), init);
                    num = 3;
                }

                switch (num)
                {
                    case 1:
                        continue;
                    case 4:
                        return;
                    case 2:
                        return;
                    case 3:
                        return;
                }

                return;
            }
        }

        [SpecialName]
        private static GameObject SelectInitializer()
        {
            return VerifyAlgo();
        }

        private static GameObject VerifyAlgo(bool checklast = true)
        {
            while (!(_AlgoVisitor != null))
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                        case 6:
                            return (GameObject)serverVisitor.GetValue(InitInitializer());
                        case 2:
                        case 5:
                            return null;
                        case 4:
                            break;
                        case 3:
                            if ((bool)CalculateInitializer())
                            {
                                goto case 2;
                            }

                            num = 1;
                            continue;
                        default:
                            goto end_IL_00c0;
                    }

                    break;
                }

                continue;
                end_IL_00c0:
                    break;
            }

            return _AlgoVisitor;
        }

        [SpecialName]
        private static GameObject InstantiateInitializer()
        {
            while (m_PublisherAnnotation == null)
            {
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            m_ConfigurationAnnotation = InstantiateInitializer().AddComponent<Animator>();
                            num = 5;
                            continue;
                        case 4:
                        case 5:
                            break;
                        case 2:
                            goto end_IL_001d;
                        case 1:
                            goto case 3;
                        case 3:
                            AwakeInitializer(new GameObject("OverrideGameObject")
                            {hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)});
                            num = 0;
                            continue;
                    }

                    goto end_IL_00a8;
                    continue;
                    end_IL_001d:
                        break;
                }

                continue;
                end_IL_00a8:
                    break;
            }

            return m_PublisherAnnotation;
        }

        [SpecialName]
        private static void AwakeInitializer(GameObject ident)
        {
            m_PublisherAnnotation = ident;
        }

        [SpecialName]
        private static Animator FlushInitializer()
        {
            if (m_ConfigurationAnnotation == null)
            {
                m_ConfigurationAnnotation = InstantiateInitializer().GetComponent<Animator>();
            }

            return m_ConfigurationAnnotation;
        }

        [SpecialName]
        private static UnityEditor.Animations.AnimatorController CalculateInitializer()
        {
            return annotationVisitor;
        }

        [SpecialName]
        private static void TestInitializer(UnityEditor.Animations.AnimatorController first)
        {
            if (first == annotationVisitor)
            {
                return;
            }

            if (!(first != null))
            {
                if (InstantiateInitializer() != null)
                {
                    UnityEngine.Object.DestroyImmediate(InstantiateInitializer());
                }
            }
            else
            {
                wrapperVisitor = true;
                FlushInitializer().runtimeAnimatorController = first;
                mapperVisitor.DisableList("EditGameObjectInternal").Invoke(Resources.FindObjectsOfTypeAll(mapperVisitor)[0], new object[2]{InstantiateInitializer(), null});
                wrapperVisitor = false;
            }

            annotationVisitor = first;
        }

        internal static void FillAlgo()
        {
            mapperVisitor = ControllerEditorUtility.FillRules("UnityEditor.AnimationWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _InitializerVisitor = ControllerEditorUtility.FillRules("UnityEditorInternal.AnimationWindowState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            definitionVisitor = ControllerEditorUtility.FillRules("UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _RegVisitor = ControllerEditorUtility.FillRules("UnityEditor.AnimEditor, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _PropertyVisitor = ControllerEditorUtility.FillRules("UnityEditorInternal.AnimationWindowSelectionItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _TestsVisitor = ControllerEditorUtility.FillRules("UnityEditorInternal.AnimationWindowControl, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            m_SerializerVisitor = ControllerEditorUtility.FillRules("UnityEditorInternal.AnimationWindowHierarchyNode, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("binding", BindingFlags.Instance | BindingFlags.Public);
            m_ProcessorVisitor = mapperVisitor.QueryList("state");
            _ObserverVisitor = _InitializerVisitor.QueryList("activeAnimationClip");
            serverVisitor = _InitializerVisitor.QueryList("activeRootGameObject");
            m_ThreadVisitor = _InitializerVisitor.QueryList("activeGameObject");
            m_PolicyVisitor = _InitializerVisitor.QueryList("activeScriptableObject");
            m_ItemVisitor = _RegVisitor.DisableList("PlayControlsOnGUI");
        }

        internal static void WriteAlgo()
        {
            while (true)
            {
                int num;
                if (Event.current.type == EventType.Layout)
                {
                    num = 2;
                }
                else
                {
                    EditorPatchRegistry.CloneReg("AnimationWindowFieldsPatch");
                    num = 0;
                }

                switch (num)
                {
                    default:
                        return;
                    case 3:
                        break;
                    case 1:
                        return;
                    case 4:
                        return;
                    case 2:
                        return;
                }
            }
        }

        internal static void ForgotAlgo()
        {
            while (true)
            {
                EditorPatchRegistry.IncludeReg("AnimationWindowFieldsPatch", mapperVisitor, "OnGUI", EditorPatchRegistry.CreateReg(WriteAlgo), _RegVisitor, "TabSelectionOnGUI", EditorPatchRegistry.PrepareReg(PlayControlsOnGUIPrefix));
                while (true)
                {
                    IL_0139:
                        EditorPatchRegistry.MapReg(mapperVisitor, "ShouldUpdateGameObjectSelection", EditorPatchRegistry.RevertReg<bool, bool>(ShouldUpdateGameObjectSelectionPrefix));
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 8:
                                return;
                            case 5:
                                EditorPatchRegistry.MapReg(_TestsVisitor, "get_canPreview", EditorPatchRegistry.PrepareReg(InterruptIfOverridingControllerPrefix));
                                num = 2;
                                continue;
                            case 11:
                                EditorPatchRegistry.MapReg(definitionVisitor, "DoNodeGUI", EditorPatchRegistry.PushReg<Rect, TreeViewItem>(AnimationWindowHierarchyNodeGUIPrefix), EditorPatchRegistry.NewReg<Rect>(AnimationWindowHierarchyNodeGUIPost));
                                num = 7;
                                continue;
                            case 3:
                                return;
                            case 7:
                                EditorPatchRegistry.MapReg(definitionVisitor, "GenerateMenu", null, EditorPatchRegistry.CountTests<GenericMenu, List<object>>(AnimationWindowHierarchyGUIGenerateMenuPost));
                                num = 10;
                                continue;
                            case 13:
                                break;
                            case 12:
                                goto IL_0139;
                            default:
                                EditorPatchRegistry.MapReg(mapperVisitor, "ShouldUpdateSelection", EditorPatchRegistry.PrepareReg(ShouldUpdateAnimationSelectionPrefix));
                                num = 14;
                                continue;
                            case 14:
                                goto case 9;
                            case 9:
                                EditorPatchRegistry.MapReg(_TestsVisitor, "get_canPlay", EditorPatchRegistry.PrepareReg(InterruptIfOverridingControllerPrefix));
                                num = 5;
                                continue;
                            case 2:
                                EditorPatchRegistry.MapReg(_TestsVisitor, "get_canRecord", EditorPatchRegistry.PrepareReg(InterruptIfOverridingControllerPrefix));
                                num = 1;
                                continue;
                            case 1:
                                EditorPatchRegistry.MapReg(_PropertyVisitor, "GetEditorCurveValueType", EditorPatchRegistry.CreateReg(GetEditorCurveValueTypePrefix), EditorPatchRegistry.CreateReg(GetEditorCurveValueTypePost));
                                num = 4;
                                continue;
                            case 4:
                                EditorPatchRegistry.MapReg(_PropertyVisitor, "get_rootGameObject", EditorPatchRegistry.RevertReg<GameObject, bool>(AnimationWindowSelectionItemGetRootGameObjectPrefix));
                                num = 6;
                                continue;
                            case 6:
                                EditorPatchRegistry.TestReg("UnityEditorInternal.AddCurvesPopupHierarchyDataSource, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "FetchData", EditorPatchRegistry.CreateReg(CurvesPopupFetchDataPrefix), EditorPatchRegistry.CreateReg(CurvesPopupFetchDataPost));
                                num = 11;
                                continue;
                            case 10:
                                EditorPatchRegistry.MapReg(definitionVisitor, "DoAddCurveButton", null, EditorPatchRegistry.NewReg<Rect>(AnimationWindowDoAddCurveButtonPost));
                                num = 3;
                                continue;
                        }

                        break;
                    }

                    break;
                }
            }
        }

        private static bool PlayControlsOnGUIPrefix()
        {
            if ((bool)AddInitializer().aw_active)
            {
                if ((bool)AddInitializer().aw_enableOverride)
                {
                    GuiEnabledScope annotationPolicy = new GuiEnabledScope(iskey: false);
                    try
                    {
                        if (CalculateInitializer() == null)
                        {
                            TestInitializer(null);
                            GUILayout.Label("[Override Controller]", ControllerEditorUtility.CalcError().annotationObserver, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            if (ControllerEditorUtility.DefineQueue(lastRect))
                            {
                                ControllerEditorUtility.ConcatList(CalculateInitializer(), typeof(UnityEditor.Animations.AnimatorController), null, null, loaddef3: true, null, null, delegate (UnityEngine.Object o)
                                {
                                    TestInitializer((UnityEditor.Animations.AnimatorController)o);
                                });
                            }

                            ControllerEditorUtility.AwakeRules(lastRect, delegate (IEnumerable<UnityEditor.Animations.AnimatorController> o)
                            {
                                UnityEditor.Animations.AnimatorController[] source = (o as UnityEditor.Animations.AnimatorController[]) ?? o.ToArray();
                                if (source.Any())
                                {
                                    TestInitializer(source.First());
                                }
                            });
                        }
                        else
                        {
                            TestInitializer((UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(CalculateInitializer(), typeof(UnityEditor.Animations.AnimatorController), true));
                            if ((bool)CalculateInitializer())
                            {
                                ControllerEditorUtility.TestQueue("Controller", iscfg: true, 80f + (float)CalculateInitializer().name.Length * 6.5f, 20f, isivk3: false);
                            }
                        }

                        if (!(_AlgoVisitor == null))
                        {
                            _AlgoVisitor = (GameObject)EditorGUILayout.ObjectField(_AlgoVisitor, typeof(GameObject), true);
                            if ((bool)_AlgoVisitor)
                            {
                                ControllerEditorUtility.TestQueue("Root", iscfg: true, 50f + (float)_AlgoVisitor.name.Length * 6.5f, 20f, isivk3: false);
                            }
                        }
                        else
                        {
                            _AlgoVisitor = null;
                            GUILayout.Label("[Override Root]", ControllerEditorUtility.CalcError().annotationObserver, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
                            Rect lastRect2 = GUILayoutUtility.GetLastRect();
                            if (ControllerEditorUtility.DefineQueue(lastRect2))
                            {
                                ControllerEditorUtility.ConcatList(_AlgoVisitor, typeof(GameObject), null, null, loaddef3: true, null, null, delegate (UnityEngine.Object o)
                                {
                                    _AlgoVisitor = (GameObject)o;
                                });
                            }

                            ControllerEditorUtility.AwakeRules(lastRect2, delegate (IEnumerable<GameObject> o)
                            {
                                GameObject[] source = (o as GameObject[]) ?? o.ToArray();
                                if (source.Any())
                                {
                                    _AlgoVisitor = source.First();
                                }
                            });
                        }
                    }
                    finally
                    {
                        if (annotationPolicy != null)
                        {
                            ((IDisposable)annotationPolicy).Dispose();
                        }
                    }

                    return true;
                }
            }

            return true;
        }

        private static void AnimationWindowHierarchyGUIGenerateMenuPost(ref GenericMenu __result, List<object> interactedNodes)
        {
            m_SingletonVisitor = __result;
            pageVisitor = true;
            resolverVisitor = interactedNodes;
        }

        private static void AnimationWindowHierarchyNodeGUIPrefix(Rect rect, TreeViewItem node)
        {
            _003C_003Ec__DisplayClass481_0 CS_0024_003C_003E8__locals12 = new _003C_003Ec__DisplayClass481_0();
            if (!AddInitializer().aw_active || !AddInitializer().aw_enablePropertyEditing || node == null)
            {
                return;
            }

            CS_0024_003C_003E8__locals12._AttributeReg = (EditorCurveBinding? )m_SerializerVisitor.GetValue(node);
            if (!CS_0024_003C_003E8__locals12._AttributeReg.HasValue)
            {
                return;
            }

            CS_0024_003C_003E8__locals12.fieldReg = SelectInitializer();
            if (CS_0024_003C_003E8__locals12.fieldReg == null)
            {
                return;
            }

            ControllerEditorUtility.InstantiateRules(rect, (GameObject o) =>
            {
                AnimationClip setup = DefineInitializer();
                if (!(CS_0024_003C_003E8__locals12.fieldReg == null))
                {
                    if (!o.transform.IsChildOf(CS_0024_003C_003E8__locals12.fieldReg.transform) && o != CS_0024_003C_003E8__locals12.fieldReg)
                    {
                        FindVisitor(o.name + " is not a child of " + CS_0024_003C_003E8__locals12.fieldReg.name, CustomLogType.Warning);
                    }
                    else
                    {
                        string text = AnimationUtility.CalculateTransformPath(o.transform, CS_0024_003C_003E8__locals12.fieldReg.transform);
                        if (!(text == CS_0024_003C_003E8__locals12._AttributeReg.Value.path))
                        {
                            EditorCurveBinding value = CS_0024_003C_003E8__locals12._AttributeReg.Value;
                            value.path = text;
                            CheckAlgo(setup, CS_0024_003C_003E8__locals12._AttributeReg.Value, value);
                        }
                    }
                }
            });
        }

        private static void AnimationWindowHierarchyNodeGUIPost(Rect rect)
        {
            if (!AddInitializer().aw_active)
            {
                return;
            }

            if (!AddInitializer().aw_enablePropertyEditing || !pageVisitor)
            {
                return;
            }

            pageVisitor = false;
            rect.y += 60f;
            rect.x += Event.current.mousePosition.x;
            EditorCurveBinding[] array;
            try
            {
                _003C_003Ec__DisplayClass482_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass482_0();
                CS_0024_003C_003E8__locals3.clientReg = DefineInitializer();
                array = (
                    from n in resolverVisitor
                    where ((TreeViewItem)n).children.CalcRules()select (EditorCurveBinding)m_SerializerVisitor.GetValue(n)).Where(delegate (EditorCurveBinding b)
                {
                    if (b.isPPtrCurve)
                    {
                        if (!AnimationUtility.GetObjectReferenceCurve(CS_0024_003C_003E8__locals3.clientReg, b).GetRules())
                        {
                            return true;
                        }
                    }

                    return !AnimationUtility.GetEditorCurve(CS_0024_003C_003E8__locals3.clientReg, b).keys.GetRules();
                }).ToArray();
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                return;
            }

            if (array.Length == 0)
            {
                return;
            }

            StopAlgo(m_SingletonVisitor, rect, array);
            m_SingletonVisitor.ShowAsContext();
        }

        private static void StopAlgo(GenericMenu last, Rect token, EditorCurveBinding[] temp)
        {
            _003C_003Ec__DisplayClass483_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass483_0();
            CS_0024_003C_003E8__locals15.descriptorReg = temp;
            CS_0024_003C_003E8__locals15._TemplateReg = token;
            CS_0024_003C_003E8__locals15.configReg = CS_0024_003C_003E8__locals15.descriptorReg[0];
            bool num = CS_0024_003C_003E8__locals15.descriptorReg.Any((EditorCurveBinding b) => b.type != CS_0024_003C_003E8__locals15.configReg.type);
            last.AddSeparator("");
            if (num)
            {
                last.AddDisabledItem("Set Property Name".CreateResolver());
            }
            else
            {
                last.AddItem(new GUIContent("Set Property Name"), on: false, () =>
                {
                    string[] array = ListVisitor(CS_0024_003C_003E8__locals15.configReg.type);
                    if (SelectInitializer() != null)
                    {
                        if (CS_0024_003C_003E8__locals15.configReg.type.RemoveResolver<Renderer>())
                        {
                            Transform transform = SelectInitializer().transform.Find(CS_0024_003C_003E8__locals15.configReg.path);
                            if (transform != null)
                            {
                                IEnumerable<string> second = transform.GetComponents<Renderer>().SelectMany(_003C_003Ec.watcherInitializer.CancelObserver).Select(_003C_003Ec.watcherInitializer.CountObserver).SelectMany(CollectVisitor).Distinct();
                                array = array.Concat(second).ToArray();
                            }
                        }
                    }

                    SearchPopup<string> statusServer = new SearchPopup<string>("Property Name", array, _003C_003Ec.watcherInitializer.DisableObserver, delegate (int i, string s)
                    {
                        IEnumerator<EditorCurveBinding> enumerator = CS_0024_003C_003E8__locals15.descriptorReg.Distinct().GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                EditorCurveBinding current = enumerator.Current;
                                CheckAlgo(DefineInitializer(), current, new EditorCurveBinding{type = current.type, path = current.path, propertyName = s});
                            }
                        }
                        finally
                        {
                            if (enumerator != null)
                            {
                                enumerator.Dispose();
                            }
                        }
                    });
                    statusServer.GetConnection(_003C_003Ec.watcherInitializer.InsertObserver);
                    statusServer.RunConnection(CS_0024_003C_003E8__locals15._TemplateReg);
                });
            }

            last.AddItem("Set Type".CreateResolver(), on: false, () =>
            {
                GameObject gameObject = VerifyAlgo(checklast: false);
                IEnumerable<Type> enumerable = null;
                if (gameObject != null)
                {
                    GameObject gameObject2 = (string.IsNullOrEmpty(CS_0024_003C_003E8__locals15.configReg.path) ? gameObject : gameObject.transform.Find(CS_0024_003C_003E8__locals15.configReg.path)?.gameObject);
                    if (gameObject2 != null)
                    {
                        enumerable = new Type[1]{typeof(GameObject)}.Concat(gameObject2.GetComponents<Component>().Select(_003C_003Ec.watcherInitializer.RestartObserver)).Where(_003C_003Ec.watcherInitializer.QueryObserver).Distinct();
                    }
                }

                if (enumerable == null)
                {
                    enumerable = m_ManagerAnnotation.Where(VerifyVisitor);
                }

                SearchPopup<Type> statusServer = new SearchPopup<Type>("Type", enumerable, _003C_003Ec.watcherInitializer.AddObserver, (int i, Type t) =>
                {
                    string[] array = ListVisitor(t);
                    if (array.Length == 0)
                    {
                        FindVisitor(t.Name + " has no animatable properties.");
                        return;
                    }

                    IEnumerator<EditorCurveBinding> enumerator = CS_0024_003C_003E8__locals15.descriptorReg.Distinct().GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            _003C_003Ec__DisplayClass483_1 _003C_003Ec__DisplayClass483_ = new _003C_003Ec__DisplayClass483_1();
                            _003C_003Ec__DisplayClass483_.parserReg = enumerator.Current;
                            CheckAlgo(DefineInitializer(), _003C_003Ec__DisplayClass483_.parserReg, new EditorCurveBinding{type = t, path = _003C_003Ec__DisplayClass483_.parserReg.path, propertyName = (array.FirstOrDefault(_003C_003Ec__DisplayClass483_.MapThread) ?? array[0])});
                        }
                    }
                    finally
                    {
                        if (enumerator != null)
                        {
                            enumerator.Dispose();
                        }
                    }
                });
                statusServer.IncludeConnection(_003C_003Ec.watcherInitializer.InvokeObserver);
                statusServer.GetConnection(_003C_003Ec.watcherInitializer.FindObserver);
                statusServer.RunConnection(new Rect(CS_0024_003C_003E8__locals15._TemplateReg));
            });
        }

        private static void AnimationWindowDoAddCurveButtonPost(Rect rect)
        {
            _003C_003Ec__DisplayClass485_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass485_0();
            if (!AddInitializer().aw_active)
            {
                return;
            }

            if (!AddInitializer().aw_enableGameObjectDND)
            {
                return;
            }

            CS_0024_003C_003E8__locals15.m_ItemReg = SelectInitializer();
            CS_0024_003C_003E8__locals15.m_ManagerReg = DefineInitializer();
            if (CS_0024_003C_003E8__locals15.m_ItemReg == null || CS_0024_003C_003E8__locals15.m_ManagerReg == null)
            {
                return;
            }

            CS_0024_003C_003E8__locals15._SpecificationReg = CS_0024_003C_003E8__locals15.m_ItemReg.transform;
            float num = (rect.width - 230f) / 2f;
            Rect spec = new Rect(rect.xMin + num, rect.yMin + 10f, rect.width - num * 2f, rect.height - 20f);
            ControllerEditorUtility.StartQueue(spec);
            rect = new Rect(0f, spec.y + spec.height + 4f, rect.width, EditorGUIUtility.singleLineHeight * 2f);
            Rect rect2 = new Rect(rect);
            rect2.height = 14f;
            Rect rect3 = rect2;
            GuiEnabledScope annotationPolicy = new GuiEnabledScope(CS_0024_003C_003E8__locals15.m_ItemReg == null);
            try
            {
                GUI.Label(rect3, "[Drag & Drop GameObjects]", ControllerEditorUtility.CalcError()._VisitorObserver);
                ControllerEditorUtility.AwakeRules(rect, (IEnumerable<GameObject> enu) =>
                {
                    EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(CS_0024_003C_003E8__locals15.m_ManagerReg);
                    Undo.RecordObject(CS_0024_003C_003E8__locals15.m_ManagerReg, "[CE] Add Curve");
                    IEnumerator<GameObject> enumerator = enu.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            GameObject current = enumerator.Current;
                            if (!FindVisitor(current.name + " is not a child of " + CS_0024_003C_003E8__locals15.m_ItemReg.name, CustomLogType.Warning, !current.transform.IsChildOf(CS_0024_003C_003E8__locals15._SpecificationReg) && current != CS_0024_003C_003E8__locals15.m_ItemReg))
                            {
                                string path = AnimationUtility.CalculateTransformPath(current.transform, CS_0024_003C_003E8__locals15.m_ItemReg.transform);
                                EditorCurveBinding tag = new EditorCurveBinding{type = typeof(GameObject), path = path, propertyName = "m_IsActive"};
                                EditorCurveBinding editorCurveBinding = tag;
                                if (!FindVisitor("Matching binding already exists!", CustomLogType.Warning, curveBindings.InitPredicate(editorCurveBinding, out tag)))
                                {
                                    int num2 = (current.activeSelf ? 1 : 0);
                                    AnimationUtility.SetEditorCurve(DefineInitializer(), editorCurveBinding, new AnimationCurve(new Keyframe(0f, num2), new Keyframe(1f / CS_0024_003C_003E8__locals15.m_ManagerReg.frameRate, num2)));
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator != null)
                        {
                            enumerator.Dispose();
                        }
                    }

                    EditorUtility.SetDirty(CS_0024_003C_003E8__locals15.m_ManagerReg);
                });
            }
            finally
            {
                if (annotationPolicy != null)
                {
                    ((IDisposable)annotationPolicy).Dispose();
                }
            }
        }

        private static void CheckAlgo(AnimationClip setup, EditorCurveBinding pol, EditorCurveBinding util)
        {
            _003C_003Ec__DisplayClass486_0 CS_0024_003C_003E8__locals10 = new _003C_003Ec__DisplayClass486_0();
            CS_0024_003C_003E8__locals10.m_MethodReg = util;
            if (pol == CS_0024_003C_003E8__locals10.m_MethodReg)
            {
                return;
            }

            Undo.RecordObject(setup, "[CE] Modify Binding");
            if (pol.isPPtrCurve)
            {
                EditorCurveBinding editorCurveBinding = AnimationUtility.GetObjectReferenceCurveBindings(setup).FirstOrDefault(delegate (EditorCurveBinding b)
                {
                    if (b.path == CS_0024_003C_003E8__locals10.m_MethodReg.path)
                    {
                        if (b.propertyName == CS_0024_003C_003E8__locals10.m_MethodReg.propertyName)
                        {
                            return b.type == CS_0024_003C_003E8__locals10.m_MethodReg.type;
                        }
                    }

                    return false;
                });
                ObjectReferenceKeyframe[] array = ((editorCurveBinding != default(EditorCurveBinding)) ? AnimationUtility.GetObjectReferenceCurve(setup, editorCurveBinding) : null);
                FindVisitor("Caution! Merging with pre-existing property.", CustomLogType.Warning, array != null && (bool)AddInitializer().aw_warnPropertyMerge);
                ObjectReferenceKeyframe[] array2 = AnimationUtility.GetObjectReferenceCurve(setup, pol);
                AnimationUtility.SetObjectReferenceCurve(setup, pol, null);
                if (array != null)
                {
                    array2 = (
                        from f in array2.Concat(array)group f by f.time into g
                            select g.First()).ToArray();
                }

                AnimationUtility.SetObjectReferenceCurve(setup, CS_0024_003C_003E8__locals10.m_MethodReg, array2);
                return;
            }

            EditorCurveBinding editorCurveBinding2 = AnimationUtility.GetCurveBindings(setup).FirstOrDefault(delegate (EditorCurveBinding b)
            {
                while (b.path == CS_0024_003C_003E8__locals10.m_MethodReg.path)
                {
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                                break;
                            case 2:
                            case 3:
                                goto end_IL_003f;
                            case 5:
                                if (b.propertyName == CS_0024_003C_003E8__locals10.m_MethodReg.propertyName)
                                {
                                    num = 4;
                                    continue;
                                }

                                goto end_IL_003f;
                            default:
                                goto case 5;
                            case 4:
                                return b.type == CS_0024_003C_003E8__locals10.m_MethodReg.type;
                        }

                        break;
                    }

                    continue;
                    end_IL_003f:
                        break;
                }

                return false;
            });
            AnimationCurve animationCurve = ((editorCurveBinding2 != default(EditorCurveBinding)) ? AnimationUtility.GetEditorCurve(setup, editorCurveBinding2) : null);
            FindVisitor("Caution! Merging with pre-existing property.", CustomLogType.Warning, animationCurve != null && (bool)AddInitializer().aw_warnPropertyMerge);
            AnimationCurve animationCurve2 = AnimationUtility.GetEditorCurve(setup, pol);
            AnimationUtility.SetEditorCurve(setup, pol, null);
            if (animationCurve != null)
            {
                animationCurve2 = new AnimationCurve((
                    from k in animationCurve2.keys.Concat(animationCurve.keys)group k by k.time into g
                        select g.First()).ToArray());
            }

            AnimationUtility.SetEditorCurve(setup, CS_0024_003C_003E8__locals10.m_MethodReg, animationCurve2);
        }

        private static bool InterruptIfOverridingControllerPrefix()
        {
            return CalculateInitializer() == null;
        }

        private static bool ShouldUpdateAnimationSelectionPrefix()
        {
            return !CalculateInitializer();
        }

        private static void GetEditorCurveValueTypePost()
        {
            m_VisitorVisitor = false;
        }

        private static void CurvesPopupFetchDataPost()
        {
            m_VisitorVisitor = false;
        }

        private static void GetEditorCurveValueTypePrefix()
        {
            m_VisitorVisitor = _AlgoVisitor;
        }

        private static void CurvesPopupFetchDataPrefix()
        {
            m_VisitorVisitor = _AlgoVisitor;
        }

        private static bool ShouldUpdateGameObjectSelectionPrefix(ref bool __result)
        {
            if (wrapperVisitor)
            {
                __result = true;
                return false;
            }

            return !CalculateInitializer();
        }

        private static bool AnimationWindowSelectionItemGetRootGameObjectPrefix(ref GameObject __result)
        {
            while (true)
            {
                if (m_VisitorVisitor)
                {
                    int num = 4;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                break;
                            case 1:
                            case 3:
                                return false;
                            case 4:
                                __result = _AlgoVisitor;
                                num = 3;
                                continue;
                            case 5:
                                goto end_IL_001d;
                            default:
                                goto IL_00e8;
                        }

                        goto end_IL_00bb;
                        continue;
                        end_IL_001d:
                            break;
                    }

                    continue;
                }

                goto IL_00e8;
                IL_00e8:
                    ;
                break;
                continue;
                end_IL_00bb:
                    break;
            }

            return true;
        }

        private static void PrepareAlgo()
        {
            while (true)
            {
                rulesVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            return;
                        case 2:
                            break;
                        case 1:
                            goto default;
                        default:
                            m_QueueVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Edge, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 5;
                            continue;
                        case 4:
                        case 5:
                            errorVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 3;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void AssetAlgo()
        {
            Type type = default(Type);
            while (true)
            {
                EditorPatchRegistry.MapReg(_StatusVisitor, "get_selectedEdgeColor", EditorPatchRegistry.RevertReg<Color, bool>(SelectedTransitionColorPrefix));
                int num = 6;
                while (true)
                {
                    switch (num)
                    {
                        case 11:
                            EditorPatchRegistry.ValidateReg(m_AdapterVisitor, typeof(bool), "AddNode", null, EditorPatchRegistry.NewReg<Node>(AddNodePost));
                            num = 1;
                            continue;
                        case 5:
                            return;
                        case 1:
                            return;
                        case 2:
                            EditorPatchRegistry.MapReg(rulesVisitor, "get_gridMinorColor", EditorPatchRegistry.RevertReg<Color, bool>(GraphGUIMinorGridColorPrefix));
                            num = 9;
                            continue;
                        case 3:
                            EditorPatchRegistry.CalcReg(m_QueueVisitor.GetConstructor(new Type[2]{type, type}), null, EditorPatchRegistry.NewReg<Edge>(EdgeConstructorPost));
                            num = 4;
                            continue;
                        case 9:
                            EditorPatchRegistry.MapReg(_StatusVisitor, "DrawArrows", EditorPatchRegistry.AssetReg<Vector3[]>(DrawArrowsPrefix));
                            num = 11;
                            continue;
                        case 7:
                            break;
                        case 6:
                            EditorPatchRegistry.MapReg(_StatusVisitor, "get_defaultTransitionColor", EditorPatchRegistry.RevertReg<Color, bool>(EntryTransitionColorPrefix));
                            num = 10;
                            continue;
                        case 10:
                            EditorPatchRegistry.MapReg(_StatusVisitor, "get_selectorTransitionColor", EditorPatchRegistry.RevertReg<Color, bool>(BaseTransitionColorPrefix));
                            num = 8;
                            continue;
                        case 8:
                            type = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Slot, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            goto case 3;
                        default:
                            EditorPatchRegistry.MapReg(rulesVisitor, "get_gridMajorColor", EditorPatchRegistry.RevertReg<Color, bool>(GraphGUIMajorGridColorPrefix));
                            num = 2;
                            continue;
                    }

                    break;
                }
            }
        }

        private static bool SelectedTransitionColorPrefix(ref Color __result)
        {
            while (true)
            {
                if ((bool)AddInitializer().cosmeticTransitionsActive)
                {
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            case 5:
                                break;
                            case 3:
                                goto end_IL_001d;
                            default:
                                return false;
                            case 2:
                                __result = AddInitializer().selectedTransitionColor.WriteDefinition();
                                num = 0;
                                continue;
                            case 4:
                                goto IL_00f1;
                        }

                        goto end_IL_0081;
                        continue;
                        end_IL_001d:
                            break;
                    }

                    continue;
                }

                goto IL_00f1;
                IL_00f1:
                    ;
                break;
                continue;
                end_IL_0081:
                    break;
            }

            return true;
        }

        private static bool EntryTransitionColorPrefix(ref Color __result)
        {
            __result = AddInitializer().entryTransitionColor.WriteDefinition();
            return !AddInitializer().cosmeticTransitionsActive;
        }

        private static bool BaseTransitionColorPrefix(ref Color __result)
        {
            __result = AddInitializer().baseTransitionColor.WriteDefinition();
            return !AddInitializer().cosmeticTransitionsActive;
        }

        private static void EdgeConstructorPost(Edge __instance)
        {
            while ((bool)AddInitializer().cosmeticTransitionsActive)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 4:
                            break;
                        case 1:
                            return;
                        case 2:
                            return;
                        default:
                            goto IL_0097;
                        case 3:
                            goto IL_0097;
                    }

                    break;
                    IL_0097:
                        __instance.color = AddInitializer().normalTransitionColor.WriteDefinition();
                    num = 1;
                }
            }
        }

        private static void AddNodePost(Node node)
        {
        }

        private static void DrawArrowsPrefix(ref Vector3[] edgePoints)
        {
            if (!m_HelperVisitor)
            {
                if (!m_RecordVisitor)
                {
                    return;
                }
            }

            Vector3 vector = edgePoints[0];
            Vector3 vector2 = edgePoints[1];
            Vector3 vector3 = vector2 - vector;
            if (!(vector3.magnitude <= 1f))
            {
                Vector2 a = UpdateAlgo(vector, vector2);
                float magnitude = vector3.magnitude;
                float num = Vector2.Distance(a, vector2);
                float num2 = AddInitializer().arrowLerpRatio.ResetDefinition();
                if (m_HelperVisitor && (m_ContextVisitor.Contains(vector2) || selectedArrowEdges.Contains((vector, vector2))))
                {
                    _ExpressionVisitor = true;
                    float num3 = Mathf.Repeat((float)EditorApplication.timeSinceStartup / 2f, 1f);
                    num2 = ((num3 > 1f) ? Mathf.Lerp(1f, -1f, num3 - 1f) : Mathf.Lerp(-1f, 1f, num3));
                }

                edgePoints[1] = vector2 + vector3 * num2 - vector3 * (num / magnitude) * 2f * num2;
            }
        }

        public static Vector2 UpdateAlgo(Vector2 asset, Vector2 map)
        {
            Vector2 vector = new Vector2(asset.x, asset.y);
            Vector2 vector2 = new Vector2(map.x, map.y);
            Vector2 to = vector2 - vector;
            float num = Vector2.SignedAngle(Vector3.up, to);
            Vector2 state;
            Vector2 attr;
            if (num <= -78.69f || !(num < 78.69f))
            {
                float num2 = num;
                if (num2 > 78.69f && !(num2 >= 101.31f))
                {
                    state = vector2 + new Vector2(100f, 20f);
                    attr = vector2 + new Vector2(100f, -20f);
                }
                else
                {
                    float num3 = num;
                    if (!(num3 <= 101.31f) || num3 < -101.31f)
                    {
                        state = vector2 + new Vector2(100f, 20f);
                        attr = vector2 + new Vector2(-100f, 20f);
                    }
                    else
                    {
                        float num4 = num;
                        if (!(num4 < -78.69f) || num4 <= -101.31f)
                        {
                            goto IL_0158;
                        }

                        state = vector2 + new Vector2(-100f, -20f);
                        attr = vector2 + new Vector2(-100f, 20f);
                    }
                }

                goto IL_046f;
            }

            goto IL_0158;
            IL_0158:
                state = vector2 + new Vector2(-100f, -20f);
            attr = vector2 + new Vector2(100f, -20f);
            goto IL_046f;
            IL_046f:
                ChangeAlgo(vector, vector2, state, attr, out var ident);
            return ident;
        }

        public static bool ChangeAlgo(Vector2 reference, Vector2 visitor, Vector2 state, Vector2 attr2, out Vector2 ident3)
        {
            ident3 = Vector2.zero;
            Vector2 vector = visitor - reference;
            Vector2 normalized = vector.normalized;
            Vector2 normalized2 = (attr2 - state).normalized;
            if (!(Mathf.Abs(Vector2.Dot(normalized, Vector2.Perpendicular(normalized2))) < Mathf.Epsilon))
            {
                float num = Vector2.Dot(state - reference, Vector2.Perpendicular(normalized2)) / Vector2.Dot(normalized, Vector2.Perpendicular(normalized2));
                ident3 = reference + num * normalized;
                return true;
            }

            return false;
        }

        private static bool GraphGUIMajorGridColorPrefix(ref Color __result)
        {
            while (true)
            {
                int num;
                if (!AddInitializer().cosmeticGraphActive)
                {
                    num = 0;
                    goto IL_001d;
                }

                goto IL_00b5;
                IL_001d:
                    switch (num)
                    {
                        case 1:
                            break;
                        case 2:
                            return false;
                        case 4:
                            return true;
                        default:
                            goto case 4;
                        case 3:
                            goto IL_00b5;
                    }

                continue;
                IL_00b5:
                    __result = (ControllerEditorSettingsWindow.PushTests() ? AddInitializer().gridMajorDarkColor : AddInitializer().gridMajorLightColor);
                num = 2;
                goto IL_001d;
            }
        }

        private static bool GraphGUIMinorGridColorPrefix(ref Color __result)
        {
            while (true)
            {
                int num;
                if (!AddInitializer().cosmeticGraphActive)
                {
                    num = 0;
                    goto IL_001d;
                }

                goto IL_00c5;
                IL_001d:
                    switch (num)
                    {
                        case 4:
                            return false;
                        case 1:
                            break;
                        case 3:
                            return true;
                        default:
                            goto case 3;
                        case 2:
                            goto IL_00c5;
                    }

                continue;
                IL_00c5:
                    __result = (ControllerEditorSettingsWindow.PushTests() ? AddInitializer().gridMinorDarkColor : AddInitializer().gridMinorLightColor);
                num = 4;
                goto IL_001d;
            }
        }

        private static void SortAlgo()
        {
            try
            {
                if (GUI.skin == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            if (m_SetterVisitor == null)
            {
                m_SetterVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("graphBackground", BindingFlags.Public | BindingFlags.Static);
            }

            bool flag = !AddInitializer().cosmeticGraphActive;
            GUIStyle gUIStyle = new GUIStyle();
            Texture2D background;
            if (!flag && (bool)AddInitializer().graphBackgroundIsTexture)
            {
                background = AddInitializer().graphBackgroundTexture.UpdateDefinition<Texture2D>();
            }
            else
            {
                if (connectionVisitor == null)
                {
                    connectionVisitor = new Texture2D(1, 1);
                }

                connectionVisitor.SetPixel(0, 0, (flag || !AddInitializer().cosmeticGraphActive) ? ((Color)AddInitializer().gridBackgroundColor.callbackAlgo) : ((Color)AddInitializer().gridBackgroundColor));
                connectionVisitor.Apply();
                background = connectionVisitor;
            }

            gUIStyle.normal.background = background;
            m_SetterVisitor.SetValue(null, gUIStyle);
        }

        private static void RegisterAlgo()
        {
            while (true)
            {
                _ConsumerVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                int num = 10;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            m_InterpreterVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationBlendTree.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 0;
                            continue;
                        case 7:
                            m_ProductVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 2;
                            continue;
                        default:
                            _WatcherVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 5;
                            continue;
                        case 12:
                            return;
                        case 11:
                            break;
                        case 10:
                            m_AdapterVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 3;
                            continue;
                        case 1:
                        case 5:
                            m_CandidateVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 7;
                            continue;
                        case 2:
                            workerVisitor = _ConsumerVisitor.DisableList("RebuildGraph");
                            goto case 6;
                        case 6:
                            m_PrototypeVisitor = _ConsumerVisitor.DisableList("AddBreadCrumb");
                            goto case 4;
                        case 4:
                        case 9:
                            _AdapterAnnotation = _ConsumerVisitor.DisableList("get_activeGraphGUI");
                            goto case 8;
                        case 8:
                            interpreterAnnotation = m_ProductVisitor.InsertList("GetEdgePoints", new Type[1]{typeof(Edge)});
                            return;
                    }

                    break;
                }
            }
        }

        private static void LogoutAlgo()
        {
            while (true)
            {
                EditorPatchRegistry.MapReg(m_CandidateVisitor, "SetStateMachines", EditorPatchRegistry.ViewReg<AnimatorStateMachine, AnimatorStateMachine, AnimatorStateMachine>(SetStateMachinesPost));
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 0:
                            return;
                        case 1:
                            EditorPatchRegistry.MapReg(_WatcherVisitor, "HandleObjectDragging", null, null, EditorPatchRegistry.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(HandleObjectDraggingTranspiler));
                            num = 5;
                            continue;
                        case 4:
                            break;
                        case 3:
                            goto case 6;
                        case 6:
                            EditorPatchRegistry.MapReg(_ConsumerVisitor, "DoGraphBottomBar", null, new Action<Rect>(GraphGUIBottomBarPost).Method);
                            num = 2;
                            continue;
                        case 2:
                            EditorPatchRegistry.MapReg(_WatcherVisitor, "OnGraphGUI", EditorPatchRegistry.PrepareReg(OnGraphGUIPrefix), EditorPatchRegistry.CreateReg(OnGraphGUIPost));
                            num = 1;
                            continue;
                        case 5:
                        case 7:
                            EditorPatchRegistry.MapReg(_WatcherVisitor, "HandleContextMenu", EditorPatchRegistry.QueryTests<object, bool, object>(GraphGUIContextMenuPrefix), EditorPatchRegistry.NewReg<bool>(GraphGUIContextMenuPost));
                            num = 0;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void SetStateMachinesPost(AnimatorStateMachine stateMachine, AnimatorStateMachine parent, AnimatorStateMachine root)
        {
            OrderInitializer(stateMachine);
            PrintMapper(root);
            InstantiateAnnotation();
            SortAlgo();
        }

        private static bool OnGraphGUIPrefix()
        {
            filterVisitor = true;
            if (poolVisitor)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            poolVisitor = false;
            m_ContextVisitor.Clear();
            m_MapperAnnotation = UnityAnimatorBridge.RefTests.CalcPolicy()?.ToList() ?? _Process;
            bool flag = AddInitializer().animateInboundEdges;
            bool flag2 = AddInitializer().animateOutboundEdges;
            bool flag3 = flag && flag2;
            m_RecordVisitor = AddInitializer().arrowLerpRatio.ResetDefinition() != 0f;
            m_HelperVisitor = (flag || flag2) && !m_MapperAnnotation.CalcRules();
            if (m_HelperVisitor)
            {
                try
                {
                    foreach (UnityAnimatorBridge.TokenizerTests item in m_MapperAnnotation)
                    {
                        IEnumerator<Vector3> enumerator2 = (flag3 ? item.CheckPolicy().Concat(item.AssetPolicy()) : (flag ? item.CheckPolicy() : item.AssetPolicy())).Select((UnityAnimatorBridge.BaseTests e) => InitAnnotation(e)[1]).GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                Vector3 current2 = enumerator2.Current;
                                m_ContextVisitor.Add(current2);
                            }
                        }
                        finally
                        {
                            if (enumerator2 != null)
                            {
                                enumerator2.Dispose();
                            }
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    Selection.objects = null;
                    UnityEngine.Debug.LogException(exception);
                }
            }

            RefreshSelectedArrowAnimations();
            m_HelperVisitor |= selectedArrowEdges.Count > 0;
            return true;
        }

        private static void OnGraphGUIPost()
        {
            filterVisitor = false;
        }

        private static void GraphGUIContextMenuPrefix(object __instance, out bool __state, object ___m_EdgeGUI)
        {
            __state = true;
            Event current = Event.current;
            if (HandleGraphShortcuts(__instance)) { __state = false; return; }
            switch (current.type)
            {
                case EventType.MouseDown:
                    if ((current.control ^ (bool)AddInitializer().switchDoubleClick) && current.clickCount == 2)
                    {
                        Vector2 vector = current.mousePosition + new Vector2(-100f, -20f);
                        AnimatorState animatorState = RevertMapper().AddState(AddInitializer().defaultState.name, vector.PostPredicate(10));
                        QueryAlgo(animatorState);
                        animatorState.motion = AddInitializer().defaultState.motion;
                        current.Use();
                    }

                    break;
                case EventType.ContextClick:
                    if ((_Manager || m_Parser) && _ParserVisitor.Invoke(___m_EdgeGUI, null) != null)
                    {
                        __state = false;
                        _AttributeVisitor = _FieldVisitor ^ current.shift;
                        clientVisitor = (bool)AddInitializer().reverseModifiesValues ^ current.control;
                        GenericMenu genericMenu = new GenericMenu();
                        genericMenu.AddItem(new GUIContent("Reverse Transitions"), on: false, DeleteAlgo);
                        genericMenu.AddItem(new GUIContent("Redirect Transitions"), containerAnnotation, LoginAlgo);
                        genericMenu.AddItem(new GUIContent("Replicate Transitions"), baseAnnotation, CreateAlgo);
                        genericMenu.AddItem(new GUIContent("From\\To Any Transition"), on: false, PushAlgo);
                        genericMenu.AddSeparator(string.Empty);
                        genericMenu.AddItem(new GUIContent("(Replacing)"), _AttributeVisitor, CloneAlgo);
                        genericMenu.ShowAsContext();
                        current.Use();
                    }

                    break;
                case EventType.KeyDown:
                    switch (current.keyCode)
                    {
                        case KeyCode.Escape:
                            baseAnnotation = (containerAnnotation = (m_ClassAnnotation = false));
                            QueryAnnotation()?.Repaint();
                            break;
                        case KeyCode.A:
                            if (current.control)
                            {
                                if (!current.shift)
                                {
                                    InsertAlgo();
                                }
                                else
                                {
                                    DestroyAlgo();
                                }

                                current.Use();
                            }

                            break;
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            if (!baseAnnotation)
                            {
                                if (!containerAnnotation)
                                {
                                    if (m_ClassAnnotation)
                                    {
                                        MoveAlgo();
                                    }
                                }
                                else
                                {
                                    LoginAlgo();
                                }
                            }
                            else
                            {
                                CreateAlgo();
                            }

                            QueryAnnotation()?.Repaint();
                            break;
                    }

                    break;
            }
        }

        private static void GraphGUIContextMenuPost(bool __state)
        {
            if (!__state)
            {
            }
            else if (Event.current.type == EventType.ContextClick)
            {
                m_SingletonVisitor.AddSeparator(string.Empty);
                m_SingletonVisitor.AddItem(new GUIContent(((_TestsAnnotation.Count > 0) ? "Deselect" : "Select") + " All Transitions"), on: false, DestroyAlgo);
                m_SingletonVisitor.AddItem(new GUIContent(((m_AlgoAnnotation.Count > 0) ? "Deselect" : "Select") + " All States"), on: false, InsertAlgo);
                m_SingletonVisitor.ShowAsContext();
            }
        }

        private static void GraphGUIBottomBarPost(Rect nameRect)
        {
            if (!baseAnnotation)
            {
                if (!containerAnnotation && !m_ClassAnnotation)
                {
                    goto IL_031c;
                }
            }

            Rect screenRect = new Rect(nameRect);
            screenRect.y -= screenRect.height + 5f;
            GUILayout.BeginArea(screenRect);
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
            try
            {
                object text;
                if (!baseAnnotation)
                {
                    text = (containerAnnotation ? "Redirecting Transitions" : "Making Transitions");
                }
                else
                {
                    text = "Replicating Transitions";
                }

                GUILayout.Label((string)text, ControllerEditorUtility.CalcError()._StructProcessor);
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            GUILayout.EndArea();
            goto IL_031c;
            IL_031c:
                if ((bool)LogoutMapper())
                {
                    if (!AddInitializer().hasPingedController)
                    {
                        GUI.Label(nameRect, "Click to highlight Controller", ControllerEditorUtility.CalcError().annotationObserver);
                    }

                    if (ControllerEditorUtility.DefineQueue(nameRect))
                    {
                        EditorGUIUtility.PingObject(LogoutMapper());
                        AddInitializer().hasPingedController.ExcludeDefinition(excludeparam: true);
                    }
                }

            if (_InfoVisitor)
            {
                _InfoVisitor = false;
                EditorWindow obj = QueryAnnotation();
                _ConsumerVisitor.GetMethod("FrameAutofit").Invoke(obj, Array.Empty<object>());
            }

            if (_SystemVisitor)
            {
                _SystemVisitor = false;
                PatchAlgo();
            }
        }

        private static void PatchAlgo()
        {
            InterruptAlgo(wantfirst: false);
        }

        private static void InterruptAlgo(bool wantfirst)
        {
            EditorWindow editorWindow = QueryAnnotation();
            if (editorWindow != null)
            {
                workerVisitor.Invoke(editorWindow, new object[1]{wantfirst});
            }
        }

        [SpecialName]
        private static string ValidateInitializer()
        {
            return AddInitializer().categoryBaseName;
        }

        [SpecialName]
        private static bool RateInitializer()
        {
            return writerVisitor == LayerViewViewType.DefaultView;
        }

        [SpecialName]
        private static bool GetInitializer()
        {
            return writerVisitor == LayerViewViewType.CategoryByName;
        }

        [SpecialName]
        private static float CloneInitializer()
        {
            return AddInitializer().layerCompactView ? 20 : 40;
        }

        [SpecialName]
        private static string PushInitializer()
        {
            return AddInitializer().categoryDelimiter;
        }

        private static void ManageAlgo()
        {
            while (true)
            {
                _ReaderVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.LayerControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            registryVisitor = _ReaderVisitor.DisableList("KeyboardHandling");
                            num = 9;
                            continue;
                        case 4:
                            return;
                        case 3:
                            break;
                        case 2:
                            _BridgeVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.LayerSettingsWindow, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                            num = 7;
                            continue;
                        case 7:
                            m_StrategyVisitor = _ReaderVisitor.RestartList("m_LayerScroll");
                            num = 5;
                            continue;
                        case 5:
                            _CustomerVisitor = _ReaderVisitor.DisableList("OnRemoveLayer");
                            num = 8;
                            continue;
                        default:
                            _ExporterVisitor = _BridgeVisitor.DisableList("ShowAtPosition");
                            num = 12;
                            continue;
                        case 8:
                            m_DatabaseVisitor = _ReaderVisitor.DisableList("RenameEnd");
                            num = 10;
                            continue;
                        case 10:
                            m_IdentifierVisitor = _ReaderVisitor.RestartList("m_LayerList");
                            num = 6;
                            continue;
                        case 6:
                            m_AttrVisitor = _ReaderVisitor.RestartList("m_Host");
                            num = 1;
                            continue;
                        case 11:
                        case 12:
                            m_DispatcherVisitor = _ConsumerVisitor.RestartList("m_AnimatorController");
                            num = 4;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void PrintAlgo()
        {
            EditorPatchRegistry.MapReg(_ReaderVisitor, "OnToolbarGUI", EditorPatchRegistry.CreateReg(OnToolbarGUIPrefix), EditorPatchRegistry.CreateReg(OnToolbarGUIPost));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "OnGUI", EditorPatchRegistry.CheckReg<Rect, bool>(OnLayerViewGUIPrefix), EditorPatchRegistry.CreateReg(OnLayerViewGUIPost));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "Init", null, EditorPatchRegistry.PushReg<object, ReorderableList>(LayerViewInitPost));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "OnDrawLayer", EditorPatchRegistry.SearchReg<Rect, int, bool>(OnDrawLayerPrefix));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "OnRemoveLayer", null, EditorPatchRegistry.CreateReg(DisableMapper));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "RenameEnd", EditorPatchRegistry.OrderTests<string>(LayerViewRenameEndPrefix), EditorPatchRegistry.NewReg<string>(LayerViewRenameEndPost));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "OnSelectLayer", EditorPatchRegistry.OrderTests<int>(OnSelectLayerPrefix), EditorPatchRegistry.NewReg<int>(OnSelectLayerPost));
            EditorPatchRegistry.MapReg(_ReaderVisitor, "ResetUI", null, null, EditorPatchRegistry.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(LayerResetTranspiler));
            EditorPatchRegistry.MapReg(_ConsumerVisitor, "AddNewLayer", EditorPatchRegistry.CheckReg<object, bool>(AddNewLayerPrefix));
            EditorPatchRegistry.MapReg(typeof(ReorderableList), "DoLayoutList", EditorPatchRegistry.PrepareReg(RevertAlgo));
        }

        private static void OnToolbarGUIPrefix()
        {
            string text = writerVisitor.ToString().NewResolver();
            bool flag = AddInitializer().displayCategoryView;
            bool flag2 = AddInitializer().displayLayerCompactView;
            Rect var = GUILayoutUtility.GetLastRect().ForgotResolver(-10f).InterruptResolver(flag ? (text.PushResolver() + 24f) : 0f).ManageResolver(20f);
            Rect rect = var.ViewResolver().InterruptResolver(flag2 ? 20 : 0);
            Rect item = rect.ViewResolver().InterruptResolver(20f);
            if (flag && GUI.Button(var, text.CreateResolver(), EditorStyles.toolbarDropDown))
            {
                SearchAlgo();
            }

            if (flag2)
            {
                GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, !AddInitializer().layerCompactView, Color.gray);
                try
                {
                    if (ControllerEditorUtility.QueryQueue(rect, new GUIContent(ControllerEditorUtility.DestroyError().m_SingletonProcessor)
                    {tooltip = "Compact View"}, ControllerEditorUtility.CalcError().configProcessor))
                    {
                        AddInitializer().layerCompactView.InsertDefinition();
                        DisableMapper();
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

            if (!flag)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(writerVisitor == LayerViewViewType.DefaultView))
            {
                GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, !AddInitializer().sortCategoryViewLayers, Color.gray);
                try
                {
                    if (ControllerEditorUtility.QueryQueue(item, ControllerEditorUtility.DestroyError()._MappingProcessor, ControllerEditorUtility.CalcError().configProcessor))
                    {
                        AddInitializer().sortCategoryViewLayers.InsertDefinition();
                        DisableMapper();
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
        }

        private static void OnToolbarGUIPost()
        {
            Rect rect = GUILayoutUtility.GetRect(90f, 18f);
            rect.x -= 1f;
            rect.y += 1f;
            if (rect.Contains(Event.current.mousePosition))
            {
                if (_TagVisitor)
                {
                    _TagVisitor = false;
                    RevertVisitor();
                }
            }
            else
            {
                _TagVisitor = true;
            }

            int num = EditorGUI.Popup(rect, 0, new[] { "Templates" }.Concat(m_PrinterVisitor).ToArray()) - 1;
            switch (num)
            {
                case 0:
                    LogoutMapper().CreateLayer("New Layer", AddInitializer().defaultLayerWeight, AddInitializer().defaultLayerMask.UpdateDefinition<AvatarMask>());
                    break;
                default:
                    OrderAlgo(requestVisitor[num], LogoutMapper());
                    break;
                case -1:
                    break;
            }
        }

        private static void SearchAlgo()
        {
            GenericMenu genericMenu = new GenericMenu();
            IEnumerator enumerator = Enum.GetValues(typeof(LayerViewViewType)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    _003C_003Ec__DisplayClass581_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass581_0();
                    CS_0024_003C_003E8__locals3.schemaReg = (LayerViewViewType)enumerator.Current;
                    genericMenu.AddItem(CS_0024_003C_003E8__locals3.schemaReg.ToString().NewResolver().DeleteResolver(tokenneeded: true), on: false, delegate
                    {
                        writerVisitor = CS_0024_003C_003E8__locals3.schemaReg;
                        DisableMapper();
                    });
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                disposable?.Dispose();
            }

            genericMenu.ShowAsContext();
        }

        private static bool OnLayerViewGUIPrefix(Rect rect)
        {
            LayerViewViewType layerViewViewType = writerVisitor;
            if (layerViewViewType == LayerViewViewType.DefaultView || (uint)(layerViewViewType - 1) > 1u)
            {
                eventVisitor = false;
                return true;
            }

            RestartMapper();
            registryVisitor.Invoke(ReadAnnotation(), null);
            importerVisitor = GUILayout.BeginScrollView(importerVisitor);
            m_GetterVisitor.DoLayoutList();
            EditorGUILayout.HelpBox("Category view may have missing interactions or unhandled issues.", MessageType.None);
            GUILayout.EndScrollView();
            eventVisitor = true;
            return true;
        }

        private static bool RevertAlgo()
        {
            return !eventVisitor;
        }

        private static void OnLayerViewGUIPost()
        {
            eventVisitor = false;
        }

        private static void LayerViewInitPost(object __instance, ReorderableList ___m_LayerList)
        {
            m_StubVisitor = __instance;
            m_InterceptorVisitor = ___m_LayerList;
            PublishAnnotation();
        }

        private static bool OnDrawLayerPrefix(ref Rect rect, ref int index)
        {
            if (m_InterceptorVisitor == null)
            {
                return true;
            }

            UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = m_InterceptorVisitor.list[index] as UnityEditor.Animations.AnimatorControllerLayer;
            OrderMapper(rect, index);
            if ((bool)AddInitializer().displayLayerIndex && RateInitializer())
            {
                Rect rect2 = rect.ForgotResolver(-20f).InterruptResolver(22f);
                if (!AddInitializer().layerCompactView)
                {
                    rect2 = rect2.StopResolver(10f).UpdateResolver(-10f);
                }
                else
                {
                    EditorGUI.DrawRect(rect2 = rect2.InterruptResolver(18f), new Color(0.2f, 0.2f, 0.2f));
                }

                GUI.Label(rect2, index.ToString(), ControllerEditorUtility.CalcError()._VisitorObserver);
            }

            if (animatorControllerLayer.stateMachine == null)
            {
                float x = 20f + animatorControllerLayer.name.PushResolver();
                GUI.Label(new Rect(x, rect.yMin + 6f, 20f, 14f), new GUIContent(ControllerEditorUtility.DestroyError().issuerProcessor)
                {tooltip = "Statemachine is Null! This layer will cause issues!"});
            }

            if ((bool)AddInitializer().layerCompactView)
            {
                if (indexerVisitor.ReflectSerializer() && indexerVisitor.CreateSerializer() == index && !indexerVisitor.ViewSerializer())
                {
                    if (rect.width >= 0f && !(rect.height < 0f))
                    {
                        rect.x -= 2f;
                        indexerVisitor.IncludeSerializer(rect);
                    }

                    if (!indexerVisitor.OnGUI())
                    {
                        m_DatabaseVisitor.Invoke(ReadAnnotation(), null);
                    }
                }
                else
                {
                    float num = ((index == 0) ? 1f : (AddAnnotation() ? InvokeAnnotation().GetLayerWeight(index) : animatorControllerLayer.defaultWeight));
                    bool flag = num < 1f;
                    bool flag2 = num == 0f;
                    float num2 = (flag ? Mathf.Lerp(0.5f, 1f, num) : 1f);
                    GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, flag, new Color(num2, num2, num2));
                    try
                    {
                        GUI.Label(rect, animatorControllerLayer.name, EditorStyles.label);
                    }
                    finally
                    {
                        if (templateThread != null)
                        {
                            ((IDisposable)templateThread).Dispose();
                        }
                    }

                    Rect res = rect;
                    GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
                    {margin = new RectOffset(), padding = new RectOffset(), alignment = TextAnchor.MiddleCenter};
                    Rect rect3 = res.PatchResolver(16f, isserv: true);
                    if (ControllerEditorUtility.QueryQueue(rect3.StopResolver(2f), ControllerEditorUtility.DestroyError()._ParamProcessor, ControllerEditorUtility.CalcError().configProcessor))
                    {
                        _ExporterVisitor.Invoke(null, new object[4]{rect3, animatorControllerLayer, index, LogoutMapper()});
                    }

                    if (animatorControllerLayer.syncedLayerIndex >= 0)
                    {
                        bool syncedLayerAffectsTiming = animatorControllerLayer.syncedLayerAffectsTiming;
                        GUI.Label(text: syncedLayerAffectsTiming ? "S+T" : "S", position: (!syncedLayerAffectsTiming) ? res.PatchResolver(14f, isserv: true) : res.PatchResolver(28f, isserv: true), style: style);
                    }

                    if (animatorControllerLayer.iKPass)
                    {
                        GUI.Label(res.PatchResolver(14f, isserv: true), "IK", style);
                    }

                    if (animatorControllerLayer.blendingMode == UnityEditor.Animations.AnimatorLayerBlendingMode.Additive)
                    {
                        GUI.Label(res.PatchResolver(14f, isserv: true), "A", style);
                    }

                    if (!animatorControllerLayer.avatarMask.ConcatRules())
                    {
                        GUI.Label(res.PatchResolver(14f, isserv: true), "M", style);
                    }

                    if (flag && !flag2)
                    {
                        GUI.Label(res.PatchResolver(32f, isserv: true, 2f, isvisitor3: true).StopResolver(-1f), num.ToString("F2"), ControllerEditorUtility.CalcError().algoObserver);
                    }

                    if (ControllerEditorUtility.VerifyQueue())
                    {
                        CompareMapper();
                    }
                }

                return false;
            }

            return true;
        }

        private static void LayerViewRenameEndPrefix(out string __state)
        {
            if (indexerVisitor.RateSerializer() == null)
            {
                if (indexerVisitor.ConnectSerializer() == null)
                {
                    __state = "";
                    return;
                }
            }

            string text = indexerVisitor.WriteSerializer();
            string text2 = indexerVisitor.ListSerializer();
            __state = ((text2 != text) ? text2 : "");
        }

        private static void LayerViewRenameEndPost(string __state)
        {
            _003C_003Ec__DisplayClass589_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass589_0();
            CS_0024_003C_003E8__locals4.m_BroadcasterReg = __state;
            if (RateInitializer())
            {
                return;
            }

            if (!CS_0024_003C_003E8__locals4.m_BroadcasterReg.ResetResolver() && GetInitializer())
            {
                DisableMapper();
                ImporterMapper importerMapper = listenerVisitor;
                listenerVisitor = m_ParamsVisitor.ConnectTests(CS_0024_003C_003E8__locals4.m_BroadcasterReg);
                if (importerMapper != listenerVisitor)
                {
                    AddMapper();
                    int num = listenerVisitor.listenerMapper.FindIndex((EventMapper l) => l._InfoMapper.name == CS_0024_003C_003E8__locals4.m_BroadcasterReg);
                    if (num >= 0)
                    {
                        m_GetterVisitor.index = num;
                    }
                }
            }

            m_GetterVisitor.GrabKeyboardFocus();
        }

        private static void OrderMapper(Rect first, int cfgY)
        {
            _003C_003Ec__DisplayClass590_0 CS_0024_003C_003E8__locals32 = new _003C_003Ec__DisplayClass590_0();
            CS_0024_003C_003E8__locals32.m_ProxyReg = Event.current;
            if (!m_InterceptorVisitor.HasKeyboardControl())
            {
                ReorderableList getterVisitor = m_GetterVisitor;
                if (getterVisitor == null || !getterVisitor.HasKeyboardControl())
                {
                    goto IL_088d;
                }
            }

            switch (CS_0024_003C_003E8__locals32.m_ProxyReg.type)
            {
                case EventType.ExecuteCommand:
                    m_ConfigVisitor = m_InterceptorVisitor.index;
                    m_MessageVisitor = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(m_AttrVisitor.GetValue(ReadAnnotation()));
                    if (CS_0024_003C_003E8__locals32.m_ProxyReg.commandName == "Copy")
                    {
                        CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
                        PostMapper();
                    }
                    else if (!(CS_0024_003C_003E8__locals32.m_ProxyReg.commandName == "Paste"))
                    {
                        if (CS_0024_003C_003E8__locals32.m_ProxyReg.commandName == "Duplicate")
                        {
                            CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
                            SetMapper();
                        }
                    }
                    else
                    {
                        CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
                        SetupMapper();
                    }

                    break;
                case EventType.KeyDown:
                    if (CS_0024_003C_003E8__locals32.m_ProxyReg.keyCode != KeyCode.F2)
                    {
                        char character = CS_0024_003C_003E8__locals32.m_ProxyReg.character;
                        if (character == '\0')
                        {
                            break;
                        }

                        bool flag = writerVisitor == LayerViewViewType.DefaultView;
                        ReorderableList reorderableList = (flag ? m_InterceptorVisitor : m_GetterVisitor);
                        int count = reorderableList.count;
                        int num = -1;
                        for (int i = 0; i < count; i++)
                        {
                            int num2 = (int)Mathf.Repeat(i + reorderableList.index + 1, count);
                            if (char.ToLower(DefineMapper(num2).name[0]) == character)
                            {
                                num = num2;
                                break;
                            }
                        }

                        if (num >= 0)
                        {
                            reorderableList.index = num;
                            float y = (float)num * CloneInitializer();
                            m_StrategyVisitor.SetValue(ReadAnnotation(), new Vector2(0f, y));
                            if (!flag)
                            {
                                importerVisitor = new Vector2(0f, y);
                            }

                            CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
                        }
                    }
                    else
                    {
                        CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
                        int index = m_InterceptorVisitor.index;
                        string text = ((UnityEditor.Animations.AnimatorControllerLayer)m_InterceptorVisitor.list[index]).name;
                        indexerVisitor.TestSerializer(text, index, 0.1f);
                    }

                    break;
            }

            goto IL_088d;
            IL_088d:
                if (CS_0024_003C_003E8__locals32.m_ProxyReg.type != EventType.MouseUp || CS_0024_003C_003E8__locals32.m_ProxyReg.button != 1 || !first.Contains(CS_0024_003C_003E8__locals32.m_ProxyReg.mousePosition))
                {
                    return;
                }

            m_ConfigVisitor = cfgY;
            m_MessageVisitor = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(m_AttrVisitor.GetValue(ReadAnnotation()));
            GenericMenu genericMenu = new GenericMenu();
            var motionController = m_MessageVisitor; int motionLayer = m_ConfigVisitor;
            genericMenu.AddItem(new GUIContent("View Animations"), false, () => ShowLayerAnimations(motionController, motionLayer));
            genericMenu.AddItem(new GUIContent("Delete"), on: false, CompareMapper);
            genericMenu.AddItem(new GUIContent("Duplicate"), on: false, SetMapper);
            genericMenu.AddItem(new GUIContent("Copy"), on: false, PostMapper);
            genericMenu.AddItem(new GUIContent("Paste"), on: false, (collectionVisitor != null) ? new GenericMenu.MenuFunction(SetupMapper) : null);
            genericMenu.AddSeparator("");
            CS_0024_003C_003E8__locals32.structReg = VisitMapper();
            string[] source = ReadMapper(CS_0024_003C_003E8__locals32.structReg).ToArray();
            if (creatorVisitor == null)
            {
                InsertMapper();
            }

            string[] array = creatorVisitor;
            for (int j = 0; j < array.Length; j++)
            {
                _003C_003Ec__DisplayClass590_1 CS_0024_003C_003E8__locals30 = new _003C_003Ec__DisplayClass590_1();
                CS_0024_003C_003E8__locals30.m_TaskReg = CS_0024_003C_003E8__locals32;
                CS_0024_003C_003E8__locals30.m_StateReg = array[j];
                CS_0024_003C_003E8__locals30._GlobalReg = source.Contains(CS_0024_003C_003E8__locals30.m_StateReg);
                genericMenu.AddItem(("Category Tag/" + CS_0024_003C_003E8__locals30.m_StateReg).DeleteResolver(tokenneeded: true), CS_0024_003C_003E8__locals30._GlobalReg, () =>
                {
                    string text2 = "_category:" + CS_0024_003C_003E8__locals30.m_StateReg;
                    if (CS_0024_003C_003E8__locals30._GlobalReg)
                    {
                        CS_0024_003C_003E8__locals30.m_TaskReg.structReg.ReadPredicate(text2);
                    }
                    else
                    {
                        CS_0024_003C_003E8__locals30.m_TaskReg.structReg.StartPredicate(text2);
                    }

                    EditorUtility.SetDirty(LogoutMapper());
                    DisableMapper();
                });
            }

            genericMenu.AddItem("Category Tag/[New Category]".DeleteResolver(tokenneeded: true), on: false, () =>
            {
                QuickInputWindow quickInputWindow = QuickInputWindow.CreateHelper("New Category", ControllerEditorUtility.CheckRules<QuickInputWindow.FieldType>(QuickInputWindow.FieldType.String), ControllerEditorUtility.CheckRules<GUIContent>("Category Name".DeleteResolver(tokenneeded: true)), (object[] results) =>
                {
                    CS_0024_003C_003E8__locals32.structReg.StartPredicate("_category:" + (string)results[0]);
                    DisableMapper();
                }, _003C_003Ec.watcherInitializer.ReadObserver);
                quickInputWindow.NewHelper(0, ControllerEditorUtility.ChangeRules("New Category", creatorVisitor));
                quickInputWindow.CollectHelper(CS_0024_003C_003E8__locals32.m_ProxyReg.mousePosition);
            });
            genericMenu.AddSeparator("");
            if (m_ConfigVisitor == 0)
            {
                genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Masks"), on: false, MoveMapper);
                genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/From Animator"), on: false, ConcatMapper);
                genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/Generic"), on: false, ComputeMapper);
            }

            genericMenu.AddItem(new GUIContent("Build Mask/From Animator"), on: false, PublishMapper);
            genericMenu.AddItem(new GUIContent("Build Mask/Generic"), on: false, EnableMapper);
            genericMenu.ShowAsContext();
            m_ContainerVisitor = Event.current.mousePosition;
            CS_0024_003C_003E8__locals32.m_ProxyReg.Use();
        }

        private static void OnSelectLayerPrefix(out int __state)
        {
            __state = SetupInitializer();
        }

        private static void OnSelectLayerPost(int __state)
        {
            while (__state != SetupInitializer())
            {
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            return;
                        case 4:
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
                        _InfoVisitor = AddInitializer().autoFrameLayer;
                    num = 4;
                }
            }
        }

        private static bool AddNewLayerPrefix(object __instance)
        {
            UnityEditor.Animations.AnimatorController animatorController = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(__instance);
            string text = "New Layer";
            if (writerVisitor == LayerViewViewType.CategoryByName && listenerVisitor != m_ParamsVisitor && listenerVisitor.CustomizeTests() != ValidateInitializer())
            {
                text = listenerVisitor.CustomizeTests() + PushInitializer() + text;
            }

            text = animatorController.MakeUniqueLayerName(text);
            UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = animatorController.CreateLayer(text, AddInitializer().defaultLayerWeight, AddInitializer().defaultLayerMask.UpdateDefinition<AvatarMask>());
            _ConsumerVisitor.QueryList("selectedLayerIndex").SetValue(__instance, animatorController.layers.Length - 1);
            object value = _ConsumerVisitor.RestartList("m_LayerEditor").GetValue(__instance);
            m_StrategyVisitor.SetValue(value, new Vector2(0f, animatorController.layers.Length * 40));
            animatorControllerLayer.stateMachine.entryPosition = AddInitializer().defaultEntryPosition;
            animatorControllerLayer.stateMachine.anyStatePosition = AddInitializer().defaultAnyPosition;
            animatorControllerLayer.stateMachine.exitPosition = AddInitializer().defaultExitPosition;
            if (writerVisitor == LayerViewViewType.CategoryByTag && listenerVisitor != m_ParamsVisitor && listenerVisitor.CustomizeTests() != ValidateInitializer())
            {
                animatorControllerLayer.StartPredicate("_category:" + listenerVisitor.CustomizeTests());
            }

            DisableMapper();
            return false;
        }

        private static IEnumerable<CodeInstruction> LayerResetTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            for (int num = list.Count - 1; num > 1; num--)
            {
                if (list[num].operand != null && list[num].opcode.Name == "stfld" && list[num].operand.ToString() == "UnityEngine.Vector2 m_LayerScroll")
                {
                    list.RemoveRange(num - 2, 3);
                    break;
                }
            }

            return list.AsEnumerable();
        }

        private static void CompareMapper()
        {
            _CustomerVisitor.Invoke(ReadAnnotation(), new object[1]{m_ConfigVisitor});
        }

        private static void SetMapper()
        {
            UnityEditor.Animations.AnimatorControllerLayer item = ControllerEditorUtility.CopyLayer(m_MessageVisitor.layers[m_ConfigVisitor], m_MessageVisitor);
            if (m_ConfigVisitor < m_MessageVisitor.layers.Length - 2)
            {
                UnityEditor.Animations.AnimatorControllerLayer[] array = m_MessageVisitor.layers;
                ArrayUtility.Insert(ref array, m_ConfigVisitor + 1, item);
                ArrayUtility.RemoveAt(ref array, array.Length - 1);
                m_MessageVisitor.layers = array;
            }
        }

        private static void PostMapper()
        {
            collectionVisitor = m_MessageVisitor.layers[m_ConfigVisitor];
        }

        private static void SetupMapper()
        {
            if (collectionVisitor == null)
            {
                return;
            }

            UnityEditor.Animations.AnimatorControllerLayer item = ControllerEditorUtility.CopyLayer(collectionVisitor, m_MessageVisitor);
            if (m_ConfigVisitor < m_MessageVisitor.layers.Length - 2)
            {
                UnityEditor.Animations.AnimatorControllerLayer[] array = m_MessageVisitor.layers;
                ArrayUtility.Insert(ref array, m_ConfigVisitor + 1, item);
                ArrayUtility.RemoveAt(ref array, array.Length - 1);
                m_MessageVisitor.layers = array;
            }
        }

        private static void EnableMapper()
        {
            CancelMapper();
        }

        private static void PublishMapper()
        {
            while (true)
            {
                int num;
                if (!ControllerEditorSettingsWindow._CallbackMapper)
                {
                    num = 2;
                    goto IL_001d;
                }

                goto IL_015f;
                IL_001d:
                    switch (num)
                    {
                        case 3:
                            break;
                        case 2:
                        case 7:
                            goto IL_00b7;
                        case 1:
                            goto IL_00d3;
                        case 4:
                            return;
                        case 6:
                            return;
                        case 5:
                            return;
                        default:
                            goto IL_015f;
                    }

                continue;
                IL_015f:
                    ;
                goto IL_00d3;
                IL_00d3:
                    if (!ControllerEditorSettingsWindow.m_AdvisorMapper)
                    {
                        goto IL_00b7;
                    }

                CancelMapper(ControllerEditorSettingsWindow.m_AdvisorMapper.transform);
                num = 4;
                goto IL_001d;
                IL_00b7:
                    ValidateAnnotation(PopMapper);
                num = 5;
                goto IL_001d;
            }
        }

        private static void PopMapper(object[] ident)
        {
            while (true)
            {
                int num;
                if (!RateAnnotation(ident[0] == null, "No Animator was given!"))
                {
                    ControllerEditorSettingsWindow.m_AdvisorMapper = (Animator)ident[0];
                    num = 1;
                }
                else
                {
                    num = 5;
                }

                while (true)
                {
                    switch (num)
                    {
                        case 4:
                            return;
                        case 3:
                            return;
                        case 1:
                            goto IL_0094;
                        default:
                            CancelMapper(ControllerEditorSettingsWindow.m_AdvisorMapper.transform);
                            return;
                        case 6:
                            break;
                        case 5:
                            return;
                    }

                    break;
                    IL_0094:
                        ControllerEditorSettingsWindow._CallbackMapper = (bool)ident[2];
                    num = 0;
                }
            }
        }

        private static void ComputeMapper()
        {
            CountMapper(null);
        }

        private static void MoveMapper()
        {
            string text = string.Concat(AddInitializer().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
            ControllerEditorUtility.RemoveList(text);
            AvatarMask avatarMask = ControllerEditorUtility.AddRules(m_MessageVisitor);
            if ((bool)avatarMask)
            {
                AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + m_MessageVisitor.name + ".mask"));
                UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
                layers[0].avatarMask = avatarMask;
                m_MessageVisitor.layers = layers;
                EditorUtility.SetDirty(m_MessageVisitor);
            }
        }

        private static void ConcatMapper()
        {
            if (ControllerEditorSettingsWindow._CallbackMapper)
            {
                if ((bool)ControllerEditorSettingsWindow.m_AdvisorMapper)
                {
                    CountMapper(ControllerEditorSettingsWindow.m_AdvisorMapper.transform);
                    return;
                }
            }

            ValidateAnnotation(CallMapper);
        }

        private static void CallMapper(object[] v)
        {
            while (!RateAnnotation(v[0] == null, "No Animator was given!"))
            {
                ControllerEditorSettingsWindow.m_AdvisorMapper = (Animator)v[0];
                int num = 5;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            CountMapper(ControllerEditorSettingsWindow.m_AdvisorMapper.transform);
                            num = 6;
                            continue;
                        case 5:
                            ControllerEditorSettingsWindow._CallbackMapper = (bool)v[2];
                            num = 2;
                            continue;
                        case 1:
                            return;
                        case 6:
                            return;
                        case 4:
                            break;
                        case 3:
                            goto end_IL_00fc;
                    }

                    break;
                }

                continue;
                end_IL_00fc:
                    break;
            }
        }

        private static void CancelMapper(Transform setup = null)
        {
            UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = m_MessageVisitor.layers[m_ConfigVisitor];
            AvatarMask avatarMask = ControllerEditorUtility.FindRules(animatorControllerLayer, setup);
            string text = string.Concat(AddInitializer().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
            ControllerEditorUtility.RemoveList(text);
            AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + animatorControllerLayer.name + ".mask"));
            UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
            layers[m_ConfigVisitor].avatarMask = avatarMask;
            m_MessageVisitor.layers = layers;
        }

        private static void CountMapper(Transform task)
        {
            string text = string.Concat(AddInitializer().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
            ControllerEditorUtility.RemoveList(text);
            AvatarMask avatarMask = ControllerEditorUtility.InvokeRules(m_MessageVisitor, task);
            AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + m_MessageVisitor.name + ".mask"));
            UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
            layers[0].avatarMask = avatarMask;
            m_MessageVisitor.layers = layers;
            EditorUtility.SetDirty(m_MessageVisitor);
        }

        private static void DisableMapper()
        {
            InsertMapper();
            if (writerVisitor == LayerViewViewType.DefaultView)
            {
                return;
            }

            try
            {
                if (LogoutMapper() == null)
                {
                    return;
                }

                UnityEditor.Animations.AnimatorControllerLayer[] layers = LogoutMapper().layers;
                string text = listenerVisitor?.CustomizeTests();
                m_ParamsVisitor = new ImporterMapper("Root", "Root");
                LayerViewViewType layerViewViewType = writerVisitor;
                if (layerViewViewType != LayerViewViewType.CategoryByName)
                {
                    if (layerViewViewType == LayerViewViewType.CategoryByTag)
                    {
                        for (int i = 0; i < layers.Length; i++)
                        {
                            UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = layers[i];
                            string[] array = ReadMapper(animatorControllerLayer).ToArray();
                            if (array.Any())
                            {
                                string[] array2 = array;
                                foreach (string text2 in array2)
                                {
                                    m_ParamsVisitor.ResetTests(text2 + PushInitializer() + "DUMMY", animatorControllerLayer, i);
                                }
                            }
                            else
                            {
                                m_ParamsVisitor.ResetTests(ValidateInitializer(), animatorControllerLayer, i);
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < layers.Length; k++)
                    {
                        UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer2 = layers[k];
                        m_ParamsVisitor.ResetTests(animatorControllerLayer2.name, animatorControllerLayer2, k);
                    }
                }

                Stack<ImporterMapper> stack = new Stack<ImporterMapper>();
                stack.Push(m_ParamsVisitor);
                while (stack.Count > 0)
                {
                    ImporterMapper importerMapper = stack.Pop();
                    using (List<ImporterMapper>.Enumerator enumerator = importerMapper.paramsMapper.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ImporterMapper current = enumerator.Current;
                            stack.Push(current);
                        }
                    }

                    importerMapper.paramsMapper.Sort((ImporterMapper c1, ImporterMapper c2) => string.Compare(c1.requestMapper, c2.requestMapper, StringComparison.Ordinal));
                    if ((bool)AddInitializer().sortCategoryViewLayers)
                    {
                        importerMapper.listenerMapper.Sort((EventMapper l1, EventMapper l2) => string.Compare(l1._InfoMapper.name, l2._InfoMapper.name, StringComparison.Ordinal));
                    }

                    ImporterMapper getterMapper = importerMapper.m_GetterMapper;
                    if (getterMapper != null)
                    {
                        importerMapper.paramsMapper.Remove(getterMapper);
                        importerMapper.paramsMapper.Add(getterMapper);
                    }
                }

                listenerVisitor = ((!text.ResetResolver()) ? (m_ParamsVisitor.CalculateTests(text) ?? m_ParamsVisitor) : m_ParamsVisitor);
                AddMapper();
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }

            m_GetterVisitor.GrabKeyboardFocus();
        }

        private static void InsertMapper()
        {
            UnityEditor.Animations.AnimatorControllerLayer[] layers = LogoutMapper().layers;
            HashSet<string> hashSet = new HashSet<string>();
            IEnumerator<string> enumerator = layers.SelectMany(ReadMapper).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    hashSet.Add(current);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            creatorVisitor = hashSet.OrderBy((string n) => n).ToArray();
        }

        private static void RestartMapper()
        {
            _003C_003Ec__DisplayClass611_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass611_0();
            CS_0024_003C_003E8__locals3._ProducerReg = true;
            m_ParamsVisitor?.MapTests(listenerVisitor.CustomizeTests(), delegate (ImporterMapper c)
            {
                ImporterMapper[] array = c.paramsMapper.Where(_003C_003Ec.watcherInitializer.AwakeObserver).ToArray();
                if (array.Length < 2)
                {
                    return;
                }

                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                try
                {
                    Rect def = EditorGUILayout.GetControlRect(false, 24f, GUIStyle.none, GUILayout.ExpandWidth(expand: true));
                    if (!CS_0024_003C_003E8__locals3._ProducerReg)
                    {
                        def.y -= 4f;
                    }

                    CS_0024_003C_003E8__locals3._ProducerReg = false;
                    float visitor = def.width / (float)array.Length;
                    string[] array2 = QueryMapper(listenerVisitor.CustomizeTests());
                    ImporterMapper[] array3 = array;
                    int num = 0;
                    while (num < array3.Length)
                    {
                        ImporterMapper importerMapper = array3[num];
                        string[] array4 = QueryMapper(importerMapper.CustomizeTests());
                        bool flag = array2.Length >= array4.Length;
                        if (flag)
                        {
                            int num2 = Mathf.Min(array2.Length, array4.Length);
                            for (int i = 0; i < num2; i++)
                            {
                                if (array2[i] != array4[i])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }

                        Rect var = def.SortResolver(visitor, isfield: true);
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, flag, ControllerEditorUtility.configurationProperty, Color.gray);
                        try
                        {
                            if (ControllerEditorUtility.CancelQueue(var, importerMapper.requestMapper, EditorStyles.toolbarButton))
                            {
                                if (listenerVisitor != importerMapper)
                                {
                                    listenerVisitor = importerMapper;
                                }
                                else
                                {
                                    int num3 = importerMapper.CustomizeTests().LastIndexOf(PushInitializer(), StringComparison.Ordinal);
                                    listenerVisitor = ((num3 > 0) ? (m_ParamsVisitor.CalculateTests(importerMapper.CustomizeTests().Substring(0, num3)) ?? m_ParamsVisitor) : m_ParamsVisitor);
                                }

                                AddMapper();
                                m_GetterVisitor.index = 0;
                                EnableInitializer(StartMapper(isreference: true));
                            }
                        }
                        finally
                        {
                            if (templateThread != null)
                            {
                                ((IDisposable)templateThread).Dispose();
                            }
                        }

                        num++;
                    }
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            });
        }

        private static string[] QueryMapper(string task)
        {
            return task.Split(ControllerEditorUtility.CheckRules<string>(PushInitializer()), StringSplitOptions.None);
        }

        private static void AddMapper()
        {
            _003C_003Ec__DisplayClass616_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass616_0();
            CS_0024_003C_003E8__locals9.m_IteratorReg = ReadAnnotation();
            if (CS_0024_003C_003E8__locals9.m_IteratorReg == null)
            {
                return;
            }

            CS_0024_003C_003E8__locals9._PublisherReg = ((m_GetterVisitor == null || !m_GetterVisitor.CallPredicate()) ? "" : ((EventMapper)m_GetterVisitor.list[m_GetterVisitor.index])._InfoMapper.name);
            _FacadeVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.ElementCallbackDelegate>("OnDrawLayer");
            _AdvisorVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnSelectLayer");
            _CallbackVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnMouseUpLayer");
            m_GetterVisitor = new ReorderableList(listenerVisitor.listenerMapper, typeof(EventMapper), draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
            {drawElementBackgroundCallback = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.ElementCallbackDelegate>("OnDrawLayerBackground"), drawElementCallback = InvokeMapper, onSelectCallback = FindMapper, onMouseUpCallback = ExcludeMapper, showDefaultBackground = false, headerHeight = 0f, footerHeight = 0f, elementHeight = CloneInitializer()};
            if (!CS_0024_003C_003E8__locals9._PublisherReg.ResetResolver())
            {
                int num = listenerVisitor.listenerMapper.FindIndex((EventMapper l) => l._InfoMapper.name == CS_0024_003C_003E8__locals9._PublisherReg);
                if (num >= 0)
                {
                    m_GetterVisitor.index = num;
                }
            }
        }

        private static void InvokeMapper(Rect last, int mean_cont, bool createrule, bool t2reguired)
        {
            if (!mean_cont.CustomizeResolver(listenerVisitor.listenerMapper))
            {
                return;
            }

            int facadeMapper = listenerVisitor.listenerMapper[mean_cont]._FacadeMapper;
            if (facadeMapper.CustomizeResolver(m_InterceptorVisitor.list))
            {
                _FacadeVisitor(last, facadeMapper, t2reguired, createrule);
            }
        }

        private static void FindMapper(ReorderableList res)
        {
            InitMapper(_AdvisorVisitor);
        }

        private static void ExcludeMapper(ReorderableList i)
        {
            InitMapper(_CallbackVisitor);
        }

        private static void InitMapper(ReorderableList.SelectCallbackDelegate instance)
        {
            int index = StartMapper(isreference: true);
            m_InterceptorVisitor.index = index;
            instance(m_InterceptorVisitor);
        }

        private static UnityEditor.Animations.AnimatorControllerLayer VisitMapper()
        {
            return DefineMapper(StartMapper());
        }

        private static UnityEditor.Animations.AnimatorControllerLayer DefineMapper(int sizelast)
        {
            if (RateInitializer())
            {
                return (UnityEditor.Animations.AnimatorControllerLayer)m_InterceptorVisitor.list[sizelast];
            }

            return listenerVisitor.listenerMapper[sizelast]._InfoMapper;
        }

        private static int StartMapper(bool isreference = false)
        {
            if (writerVisitor == LayerViewViewType.DefaultView)
            {
                return m_InterceptorVisitor.index;
            }

            if (!isreference)
            {
                return m_GetterVisitor.index;
            }

            if (!m_GetterVisitor.index.CustomizeResolver(m_GetterVisitor.list))
            {
                return -1;
            }

            return listenerVisitor.listenerMapper[m_GetterVisitor.index]._FacadeMapper;
        }

        [IteratorStateMachine(typeof(_003CGetLayerCategories_003Ed__624))]
        private static IEnumerable<string> ReadMapper(UnityEditor.Animations.AnimatorControllerLayer var1)
        {
            //yield-return decompiler failed: Missing enumeratorCtor.Body
            return new _003CGetLayerCategories_003Ed__624(-2)
            {_003C_003E3__l = var1};
        }

        [EditorInitializeAttribute(1)]
        private static void SelectMapper()
        {
            if (m_InterceptorVisitor == null)
            {
                object obj = ReadAnnotation();
                if (obj != null)
                {
                    m_InterceptorVisitor = (ReorderableList)m_IdentifierVisitor.GetValue(obj);
                }
            }
        }

        private static void RemoveMapper()
        {
            EditorPatchRegistry.MapReg(typeof(Unsupported), "PasteToStateMachineFromPasteboard", EditorPatchRegistry.InsertTests<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPrefix), EditorPatchRegistry.PushReg<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPost));
        }

        private static void PasteToStateMachineFromPasteboardPrefix(AnimatorStateMachine sm, out ChildAnimatorState[] __state)
        {
            __state = sm.states;
        }

        private static void PasteToStateMachineFromPasteboardPost(AnimatorStateMachine sm, ChildAnimatorState[] __state)
        {
            _003C_003Ec__DisplayClass630_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass630_0();
            CS_0024_003C_003E8__locals2.configurationReg = __state.Select((ChildAnimatorState s) => s.state);
            AnimatorState[] array = (
                from cs in sm.states.Except(__state)select cs.state).ToArray();
            HashSet<string> hashSet = new HashSet<string>(
                from cs in sm.states
                where !CS_0024_003C_003E8__locals2.configurationReg.Contains(cs.state)select ControllerEditorUtility.RegisterRules(cs.state.name));
            AnimatorState[] array2 = array;
            foreach (AnimatorState obj in array2)
            {
                string item = (obj.name = ControllerEditorUtility.ChangeRules(ControllerEditorUtility.RegisterRules(obj.name), hashSet));
                hashSet.Add(item);
            }

            UnityEngine.Object[] objects = array;
            Selection.objects = objects;
        }

        private static void InstantiateMapper()
        {
            m_RuleVisitor = ControllerEditorUtility.FillRules("UnityEditor.GenericMenu+MenuItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetConstructor(new Type[4]{typeof(GUIContent), typeof(bool), typeof(bool), typeof(GenericMenu.MenuFunction)});
            _ManagerVisitor = typeof(EditorGUI).InsertList("AdvancedPopup", new Type[3]{typeof(Rect), typeof(int), typeof(string[])});
            m_SpecificationVisitor = typeof(GUIUtility).DisableList("GetBuiltinSkin");
            contextAnnotation = _ConsumerVisitor.RestartList("m_LayerEditor");
            recordAnnotation = _ConsumerVisitor.RestartList("m_PreviewAnimator");
            helperAnnotation = _ConsumerVisitor.QueryList("liveLink");
            m_ConsumerAnnotation = _ReaderVisitor.QueryList("selectedLayerIndex");
            indexerVisitor = new AnimationWindowBridge(() => _ReaderVisitor.GetMethod("get_renameOverlay").Invoke(ReadAnnotation(), null));
            _IssuerVisitor = new AnimationWindowBridge();
            _IssuerVisitor._RulesProperty = delegate (bool accepted)
            {
                while (accepted)
                {
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 4:
                                break;
                            case 2:
                                return;
                            case 1:
                                goto IL_008d;
                            case 3:
                                goto IL_008d;
                        }

                        break;
                        IL_008d:
                            RestartAlgo(RevertMapper(), m_AlgoAnnotation, _IssuerVisitor.ListSerializer());
                        num = 0;
                    }
                }
            };
        }

        private static void AwakeMapper()
        {
            EditorPatchRegistry.MapReg(typeof(GenericMenu), "ShowAsContext", EditorPatchRegistry.NewReg<object>(ShowAsContextPrefix), EditorPatchRegistry.NewReg<object>(ShowAsContextPost));
        }

        private static void ShowAsContextPost(object __instance)
        {
            m_SingletonVisitor = (GenericMenu)__instance;
        }

        private static void ResetMapper()
        {
            _FactoryVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationStateMachine.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _AccountVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _RefVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _StatusVisitor = ControllerEditorUtility.FillRules("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            roleVisitor = UnityAnimatorBridge.ConnectionTests._SystemTests.ChangeRecord().DisableList("MakeTransitionCallback");
            paramVisitor = UnityAnimatorBridge.ConnectionTests._WorkerTests.ChangeRecord().DisableList("MakeTransitionCallback");
            modelVisitor = UnityAnimatorBridge.ConnectionTests._CandidateTests.ChangeRecord().DisableList("MakeTransitionCallback");
            tokenizerVisitor = UnityAnimatorBridge.ConnectionTests.m_ExpressionTests.ChangeRecord().DisableList("MakeTransitionCallback");
            _ParserVisitor = _StatusVisitor.DisableList("FindClosestEdge");
            m_DecoratorVisitor = _FactoryVisitor.DisableList("GenericMenuForStateMachineNode");
            tokenVisitor = UnityAnimatorBridge.ConnectionTests._CandidateTests.ChangeRecord().RestartList("state");
            codeVisitor = _RefVisitor.RestartList("motion");
            m_DicVisitor = _RefVisitor.RestartList("children");
            _InvocationVisitor = _RefVisitor.QueryList("parent");
        }

        private static void FlushMapper()
        {
            while (true)
            {
                EditorPatchRegistry.MapReg(m_CandidateVisitor, "CreateNodeFromState", null, EditorPatchRegistry.NewReg<ChildAnimatorState>(CreateNodeFromStatePost));
                int num = 16;
                while (true)
                {
                    switch (num)
                    {
                        case 5:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests._SystemTests, "NodeUI", EditorPatchRegistry.NewReg<Node>(EntryStateNodeUIPrefix), EditorPatchRegistry.CreateReg(EntryStateNodeUIPost));
                            num = 14;
                            continue;
                        case 12:
                            EditorPatchRegistry.MapReg(_StatusVisitor, "EndSlotDragging", EditorPatchRegistry.CreateReg(EndSlotDraggingPrefix), EditorPatchRegistry.CreateReg(EndSlotDraggingPost));
                            num = 4;
                            continue;
                        case 8:
                            EditorPatchRegistry.MapReg(m_InterpreterVisitor, "HandleNodeInput", EditorPatchRegistry.OrderTests<bool>(TreeNodeInputPrefix), EditorPatchRegistry.PushReg<object, bool>(TreeNodeInputPost));
                            num = 18;
                            continue;
                        case 3:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests._WorkerTests, "Connect", null, EditorPatchRegistry.PushReg<object, object>(AnyStateNodeConnectPost));
                            num = 9;
                            continue;
                        case 10:
                            return;
                        case 14:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests.m_FilterTests, "NodeUI", EditorPatchRegistry.NewReg<Node>(ExitNodeUIPrefix), EditorPatchRegistry.CreateReg(ExitNodeUIPost));
                            num = 0;
                            continue;
                        case 18:
                            EditorPatchRegistry.MapReg(m_InterpreterVisitor, "NodeGUI", EditorPatchRegistry.NewReg<object>(TreeNodeGUIPrefix), EditorPatchRegistry.NewReg<Node>(TreeNodeGUIPost));
                            num = 11;
                            continue;
                        default:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests._WorkerTests, "NodeUI", EditorPatchRegistry.NewReg<Node>(AnyStateNodeUIPrefix), EditorPatchRegistry.CreateReg(AnyStateNodeUIPost));
                            num = 12;
                            continue;
                        case 7:
                            EditorPatchRegistry.MapReg(_WatcherVisitor, "AddStateEmptyCallback", EditorPatchRegistry.StopReg<object, object, bool>(AddEmptyStatePrefix));
                            num = 15;
                            continue;
                        case 17:
                            break;
                        case 16:
                            EditorPatchRegistry.MapReg(m_CandidateVisitor, "CreateNodeFromStateMachine", null, EditorPatchRegistry.PushReg<object, ChildAnimatorStateMachine>(CreateNodeFromStateMachinePost));
                            num = 13;
                            continue;
                        case 13:
                            EditorPatchRegistry.MapReg(m_CandidateVisitor, "CreateNodes", null, EditorPatchRegistry.NewReg<object>(CreateNodesPost));
                            num = 7;
                            continue;
                        case 6:
                        case 9:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests._CandidateTests, "Connect", null, EditorPatchRegistry.PushReg<Node, Node>(StateNodeConnectPost));
                            goto case 8;
                        case 4:
                            EditorPatchRegistry.MapReg(_StatusVisitor, "EndDragging", null, EditorPatchRegistry.CreateReg(EndDraggingPost));
                            num = 3;
                            continue;
                        case 15:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests._CandidateTests, "NodeUI", EditorPatchRegistry.NewReg<Node>(StateNodeUIPrefix), EditorPatchRegistry.CreateReg(StateNodeUIPost));
                            num = 2;
                            continue;
                        case 2:
                            EditorPatchRegistry.MapReg(UnityAnimatorBridge.ConnectionTests.m_ExpressionTests, "NodeUI", EditorPatchRegistry.NewReg<Node>(MachineNodeUIPrefix), EditorPatchRegistry.CreateReg(MachineNodeUIPost));
                            num = 5;
                            continue;
                        case 1:
                        case 11:
                            EditorPatchRegistry.MapReg(_WatcherVisitor, "NodeGUI", EditorPatchRegistry.InsertTests<Node, AnimatorState>(GraphGUINodeGUIPrefix), EditorPatchRegistry.NewReg<AnimatorState>(GraphGUINodeGUIPost));
                            num = 10;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void CreateNodeFromStatePost(ChildAnimatorState state)
        {
            AnimatorState state2 = state.state;
            UnityAnimatorBridge.TokenizerTests tokenizerTests = UnityAnimatorBridge.RevertThread(state2);
            if ((bool)AddInitializer().cosmeticNodesActive)
            {
                tokenizerTests.FillPolicy().color = (Styles.Color)AddInitializer().normalStateNodeColor.ResetDefinition();
            }

            string[] array = InvokeAlgo(state2);
            foreach (string text in array)
            {
                if (m_PolicyAnnotation.ContainsKey(text))
                {
                    tokenizerTests.FillPolicy().style = text;
                }
            }
        }

        private static void CreateNodeFromStateMachinePost(object __instance, ChildAnimatorStateMachine subStateMachine)
        {
            while ((bool)AddInitializer().cosmeticNodesActive)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 2:
                        case 3:
                            UnityAnimatorBridge.OrderPolicy(subStateMachine.stateMachine).FillPolicy().color = (Styles.Color)AddInitializer().machineStateNodeColor.ResetDefinition();
                            num = 5;
                            continue;
                        case 5:
                            return;
                        case 4:
                            break;
                        case 0:
                            return;
                        case 1:
                            goto end_IL_00a4;
                    }

                    break;
                }

                continue;
                end_IL_00a4:
                    break;
            }
        }

        private static void CreateNodesPost(object __instance)
        {
            if (!AddInitializer().cosmeticNodesActive)
            {
                return;
            }

            UnityAnimatorBridge.RefTests.FlushPolicy().InterruptPolicy((Styles.Color)AddInitializer().entryStateNodeColor.ResetDefinition());
            UnityAnimatorBridge.RefTests.CalculatePolicy().InterruptPolicy((Styles.Color)AddInitializer().exitStateNodeColor.ResetDefinition());
            UnityAnimatorBridge.RefTests.MapPolicy().InterruptPolicy((Styles.Color)AddInitializer().anyStateNodeColor.ResetDefinition());
            if ((bool)ManageMapper().defaultState)
            {
                UnityAnimatorBridge.RevertThread(ManageMapper().defaultState).FillPolicy().color = (Styles.Color)AddInitializer().defaultStateNodeColor.ResetDefinition();
            }
        }

        private static bool AddEmptyStatePrefix(object __instance, object data)
        {
            while (true)
            {
                AnimatorState animatorState = RevertMapper().AddState(AddInitializer().defaultState.name, ((Vector2)data).PostPredicate(10));
                QueryAlgo(animatorState);
                animatorState.motion = AddInitializer().defaultState.motion;
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            break;
                        case 2:
                        case 4:
                            return false;
                        case 3:
                            goto IL_00e2;
                        default:
                            goto IL_00e2;
                    }

                    break;
                    IL_00e2:
                        InterruptAlgo(wantfirst: false);
                    num = 2;
                }
            }
        }

        private static IEnumerable<CodeInstruction> HandleObjectDraggingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            MethodInfo[] source = (
                from m in AccessTools.GetDeclaredMethods(typeof(AnimatorStateMachine))
                where m.Name == "AddState"
                select m).ToArray();
            for (int num = 0; num < list.Count; num++)
            {
                CodeInstruction codeInstruction = list[num];
                if (codeInstruction.opcode.ToString() == "callvirt" && !codeInstruction.operand.ToString().StartsWith("Void") && source.Any(codeInstruction.Calls))
                {
                    num++;
                    CodeInstruction codeInstruction2 = list[num];
                    if (codeInstruction2.IsStloc())
                    {
                        object operand = codeInstruction2.operand;
                        CodeInstruction[] array = new CodeInstruction[2]{new CodeInstruction(OpCodes.Ldloc, operand), new CodeInstruction(OpCodes.Call, m_ComparatorVisitor)};
                        list.InsertRange(num + 1, array);
                        num += 2;
                    }
                }
            }

            return list;
        }

        private static void ConnectMapper(Node config, int contend)
        {
            _InstanceVisitor = false;
            bool flag = contend <= 1;
            Event current = Event.current;
            bool flag2 = current.control && current.shift;
            switch (current.type)
            {
                case EventType.ContextClick:
                    if (contend == 3)
                    {
                        current.Use();
                    }

                    _InstanceVisitor = true;
                    break;
                case EventType.MouseDown:
                    if (m_ObjectVisitor)
                    {
                        m_ObjectVisitor = false;
                        if (m_AuthenticationVisitor == null)
                        {
                            m_AuthenticationVisitor = new AnimatorState{name = "Amogus"};
                        }

                        if (merchantVisitor == null)
                        {
                            merchantVisitor = new AnimatorStateTransition{destinationState = m_AuthenticationVisitor, exitTime = 69f, duration = 420f, offset = 80085f};
                        }

                        object obj = ExcludeAnnotation();
                        m_CandidateVisitor.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m2) =>
                        {
                            if (!(m2.Name == "CreateEdges"))
                            {
                                return false;
                            }

                            return m2.GetParameters().Length == 3;
                        }).Invoke(StartAnnotation(), new object[3]{exceptionVisitor, config, Activator.CreateInstance(UnityAnimatorBridge.ConnectionTests.readerTests.ChangeRecord(), merchantVisitor, null, null, null, null)});
                        _StatusVisitor.DisableList("EndDragging").Invoke(obj, null);
                        current.Use();
                    }
                    else if (!flag2 || current.clickCount != 2)
                    {
                        if (contend != 3 && !flag2 && current.clickCount == 2 && (!flag || _ValVisitor || (current.control ^ (bool)AddInitializer().switchDoubleClick)))
                        {
                            (contend switch
                            {
                                4 => paramVisitor,
                                2 => roleVisitor,
                                1 => tokenizerVisitor,
                                _ => modelVisitor,
                            }).Invoke(config, null);
                            _MockVisitor = contend;
                            classVisitor = config;
                            m_DescriptorVisitor = true;
                            m_UtilsVisitor = true;
                            current.Use();
                        }
                        else if (!m_UtilsVisitor || current.clickCount != 1)
                        {
                            if (_ValVisitor && current.clickCount == 1)
                            {
                                _ValVisitor = false;
                            }
                        }
                        else
                        {
                            m_UtilsVisitor = false;
                            _ValVisitor = true;
                        }
                    }
                    else if (!m_ObjectVisitor)
                    {
                        exceptionVisitor = config;
                        m_ObjectVisitor = true;
                        _StatusVisitor.DisableList("BeginSlotDragging").Invoke(ExcludeAnnotation(), new object[3]{config.inputSlots.First(), true, true});
                        current.Use();
                    }

                    break;
            }
        }

        private static void CalculateMapper(int next_task)
        {
            if (!_InstanceVisitor)
            {
                return;
            }

            _InstanceVisitor = false;
            bool flag = next_task == 3;
            if (flag)
            {
                m_SingletonVisitor = new GenericMenu();
            }

            IList list = m_SingletonVisitor.CountList();
            if (next_task == 0)
            {
                AnimatorState animatorState = Selection.activeObject as AnimatorState;
                if (animatorState && ControllerEditorExtraSettings.Current.motionSearch)
                    m_SingletonVisitor.AddItem(new GUIContent("Search Motion…"), false, () => SearchStateMotion(animatorState, new Rect(Event.current?.mousePosition ?? Vector2.zero, Vector2.zero)));
                if ((bool)animatorState && animatorState.motion is UnityEditor.Animations.BlendTree)
                {
                    templateVisitor = animatorState;
                    object value = m_RuleVisitor.Invoke(new object[4]{new GUIContent("Edit BlendTree"), false, false, new GenericMenu.MenuFunction(TestMapper)});
                    list.Insert(3, value);
                }
            }

            bool flag2 = false;
            if ((baseAnnotation && !flag) || containerAnnotation)
            {
                m_SingletonVisitor.AddSeparator(string.Empty);
                if (containerAnnotation)
                {
                    m_SingletonVisitor.AddItem(new GUIContent("Redirect Transitions"), on: true, LoginAlgo);
                }

                if (baseAnnotation && !flag)
                {
                    m_SingletonVisitor.AddItem(new GUIContent("Replicate Transitions"), on: true, CreateAlgo);
                }

                flag2 = true;
            }

            if (!flag || m_ClassAnnotation)
            {
                object value2 = m_RuleVisitor.Invoke(new object[4]{new GUIContent("Make Multiple Transitions"), false, m_ClassAnnotation, new GenericMenu.MenuFunction(MoveAlgo)});
                list.Insert((!flag) ? 1 : 0, value2);
                flag2 = true;
            }

            if (next_task <= 1)
            {
                m_SingletonVisitor.AddItem(new GUIContent("Pack into StateMachine"), on: false, CallAlgo);
                flag2 = true;
            }

            if (flag2)
            {
                m_SingletonVisitor.AddSeparator(string.Empty);
            }

            if (next_task != 1)
            {
                m_SingletonVisitor.AddItem(new GUIContent("Select Shared Transitions"), on: false, IncludeAlgo);
                if (!flag)
                {
                    m_SingletonVisitor.AddItem(new GUIContent("Select Out Transitions"), on: false, GetAlgo);
                }

                if (next_task != 2 && next_task != 4)
                {
                    m_SingletonVisitor.AddItem(new GUIContent("Select In Transitions"), on: false, CalcAlgo);
                }
            }

            if (next_task <= 1)
            {
                if (next_task == 1)
                {
                    m_SingletonVisitor.AddItem(new GUIContent("Unpack StateMachine"), on: false, CancelAlgo);
                }

                m_SingletonVisitor.AddSeparator(string.Empty);
                m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Copy"), on: false, ReadAlgo);
                m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Paste"), on: false, RemoveAlgo() ? new GenericMenu.MenuFunction(SelectAlgo) : null);
                m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Remove"), on: false, InstantiateAlgo);
                foreach (string item in _PageAnnotation)
                {
                    _003C_003Ec__DisplayClass668_0 CS_0024_003C_003E8__locals6 = new _003C_003Ec__DisplayClass668_0();
                    CS_0024_003C_003E8__locals6.m_ProcReg = "ce_" + item;
                    CS_0024_003C_003E8__locals6.m_WrapperTests = m_AlgoAnnotation.All((AnimatorState state) => state.tag == CS_0024_003C_003E8__locals6.m_ProcReg);
                    m_SingletonVisitor.AddItem(new GUIContent("Styles/" + item), CS_0024_003C_003E8__locals6.m_WrapperTests, () =>
                    {
                        UnityEngine.Object[] objectsToUndo = m_AlgoAnnotation.ToArray();
                        Undo.RecordObjects(objectsToUndo, "Set State Style");
                        using (List<AnimatorState>.Enumerator enumerator2 = m_AlgoAnnotation.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                AnimatorState current2 = enumerator2.Current;
                                current2.tag = (CS_0024_003C_003E8__locals6.m_WrapperTests ? "" : CS_0024_003C_003E8__locals6.m_ProcReg);
                                current2.ManagePredicate();
                            }
                        }

                        _SystemVisitor = true;
                    });
                }
            }

            m_SingletonVisitor.ShowAsContext();
        }

        private static void TreeNodeGUIPrefix(object n)
        {
        }

        private static void TreeNodeGUIPost(Node n)
        {
            _003C_003Ec__DisplayClass670_0 CS_0024_003C_003E8__locals6 = new _003C_003Ec__DisplayClass670_0();
            CS_0024_003C_003E8__locals6.m_AnnotationTests = n;
            string last = $"TreeNode{CS_0024_003C_003E8__locals6.m_AnnotationTests.GetInstanceID()}";
            if (Event.current.type == EventType.Repaint)
            {
                ControllerEditorUtility.SearchRules(last, CS_0024_003C_003E8__locals6.m_AnnotationTests.position);
            }

            Rect value = ControllerEditorUtility.OrderQueue(last, Rect.zero);
            float x = (value.y = 0f);
            value.x = x;
            DrawTreeMotionExtras(n, value);
            var detailTree = codeVisitor.GetValue(n) as UnityEditor.Animations.BlendTree;
            if (detailTree)
            {
                ControllerMotionDisplay.DrawTreeDetails(detailTree, value);
                ControllerBlendTreeToggles.HandleDrop(value, detailTree, LogoutMapper(), ControllerEditorSettingsWindow.m_AdvisorMapper ? ControllerEditorSettingsWindow.m_AdvisorMapper.transform : null);
            }
            ControllerEditorUtility.AwakeRules(value, (IEnumerable<Motion> motions) =>
            {
                _003C_003Ec__DisplayClass670_1 _003C_003Ec__DisplayClass670_ = new _003C_003Ec__DisplayClass670_1();
                object value2 = codeVisitor.GetValue(CS_0024_003C_003E8__locals6.m_AnnotationTests);
                _003C_003Ec__DisplayClass670_.visitorTests = value2 as UnityEditor.Animations.BlendTree;
                if ((object)_003C_003Ec__DisplayClass670_.visitorTests == null)
                {
                    object value3 = _InvocationVisitor.GetValue(CS_0024_003C_003E8__locals6.m_AnnotationTests);
                    if (value3 != null)
                    {
                        IList list = (IList)m_DicVisitor.GetValue(value3);
                        int num2 = -1;
                        for (int i = 0; i < list.Count; i++)
                        {
                            object obj = list[i];
                            if (CS_0024_003C_003E8__locals6.m_AnnotationTests == obj)
                            {
                                num2 = i;
                                break;
                            }
                        }

                        UnityEditor.Animations.BlendTree obj2 = (UnityEditor.Animations.BlendTree)codeVisitor.GetValue(value3);
                        Undo.RecordObject(obj2, "DragNDrop Motion");
                        ChildMotion[] children = obj2.children;
                        children[num2].motion = motions.First();
                        obj2.children = children;
                    }
                }
                else
                {
                    Undo.RecordObject(_003C_003Ec__DisplayClass670_.visitorTests, "DragNDrop Motions");
                    _003C_003Ec__DisplayClass670_.visitorTests.children = _003C_003Ec__DisplayClass670_.visitorTests.children.Concat(motions.Select(_003C_003Ec__DisplayClass670_.CollectThread)).ToArray();
                }
            });
        }

        private static void TreeNodeInputPrefix(out bool __state)
        {
            __state = default;
            while (true)
            {
                Event current = Event.current;
                int num = 3;
                while (true)
                {
                    bool num2;
                    switch (num)
                    {
                        default:
                            return;
                        case 1:
                            return;
                        case 4:
                            break;
                        case 3:
                            goto case 2;
                        case 2:
                            num2 = current.type == EventType.MouseDown && current.button == 1;
                            goto IL_00db;
                    }

                    break;
                    IL_00db:
                        __state = num2;
                    num = 0;
                }
            }
        }

        private static void TreeNodeInputPost(object node, bool __state)
        {
            _003C_003Ec__DisplayClass672_0 CS_0024_003C_003E8__locals7 = new _003C_003Ec__DisplayClass672_0();
            if (!__state)
            {
                return;
            }

            CS_0024_003C_003E8__locals7.algoTests = codeVisitor.GetValue(node) as UnityEditor.Animations.BlendTree;
            AddTreeMotionSearch(m_SingletonVisitor, node);
            if ((bool)CS_0024_003C_003E8__locals7.algoTests)
            {
                m_SingletonVisitor.AddItem("Add Root Tree".CreateResolver(), false, () => ControllerBlendTreeTools.WrapContents(CS_0024_003C_003E8__locals7.algoTests));
                var parentNode = _InvocationVisitor.GetValue(node);
                var parentTree = parentNode == null ? null : codeVisitor.GetValue(parentNode) as UnityEditor.Animations.BlendTree;
                ControllerEditorInspection.AddBlendTreeMenu(m_SingletonVisitor, CS_0024_003C_003E8__locals7.algoTests, LogoutMapper(), parentTree);
                m_SingletonVisitor.ShowAsContext();
            }

            if (!CS_0024_003C_003E8__locals7.algoTests) m_SingletonVisitor.ShowAsContext();
            _InstanceVisitor = false;
        }

        private static void StateNodeUIPrefix(Node __instance)
        {
            ConnectMapper(__instance, 0);
        }

        private static void StateNodeUIPost()
        {
            CalculateMapper(0);
        }

        private static void MachineNodeUIPrefix(Node __instance)
        {
            ConnectMapper(__instance, 1);
        }

        private static void MachineNodeUIPost()
        {
            CalculateMapper(1);
        }

        private static void EntryStateNodeUIPrefix(Node __instance)
        {
            ConnectMapper(__instance, 2);
        }

        private static void EntryStateNodeUIPost()
        {
            CalculateMapper(2);
        }

        private static void ExitNodeUIPrefix(Node __instance)
        {
            ConnectMapper(__instance, 3);
        }

        private static void ExitNodeUIPost()
        {
            CalculateMapper(3);
        }

        private static void AnyStateNodeUIPrefix(Node __instance)
        {
            ConnectMapper(__instance, 4);
        }

        private static void AnyStateNodeUIPost()
        {
            CalculateMapper(4);
        }

        private static void TestMapper()
        {
            m_PrototypeVisitor.Invoke(QueryAnnotation(), new object[2]{templateVisitor, true});
        }

        private static void EndSlotDraggingPrefix()
        {
            valueVisitor = true;
        }

        private static void EndSlotDraggingPost()
        {
            if (!m_DescriptorVisitor)
            {
                return;
            }

            (_MockVisitor switch
            {
                2 => roleVisitor,
                1 => tokenizerVisitor,
                4 => paramVisitor,
                _ => modelVisitor,
            }).Invoke(classVisitor, null);
            m_DescriptorVisitor = true;
            m_UtilsVisitor = true;
        }

        private static void EndDraggingPost()
        {
            m_DescriptorVisitor = false;
            if (!valueVisitor)
            {
                _ValVisitor = (m_UtilsVisitor = false);
            }

            valueVisitor = false;
        }

        private static void GraphGUINodeGUIPrefix(Node n, out AnimatorState __state)
        {
            _003C_003Ec__DisplayClass690_0 CS_0024_003C_003E8__locals6 = new _003C_003Ec__DisplayClass690_0();
            if (!(n.GetType() != UnityAnimatorBridge.ConnectionTests._CandidateTests))
            {
                __state = (AnimatorState)tokenVisitor.GetValue(n);
            }
            else
            {
                __state = null;
            }

            if (__state == null)
            {
                return;
            }

            _ReponseVisitor = n.position.size;
            if (VisitAlgo(__state))
            {
                return;
            }

            CS_0024_003C_003E8__locals6.m_MapperTests = __state;
            ControllerEditorSettings.StateCosmeticOptions stateCosmeticOptions = AddInitializer().PostDefinition();
            Rect connection = new Rect(1f, 1f, 11f, 11f);
            if (stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.coordinates))
            {
                float x = n.position.x;
                float y = n.position.y;
                Rect source = new Rect(1f, _ReponseVisitor.y - 7f, RegisterMapper(x), 7f);
                Rect rect = new Rect(source);
                rect.x = source.width;
                rect.width = RegisterMapper(y);
                Rect rect2 = rect;
                EditorGUI.BeginChangeCheck();
                GUIStyle style = new GUIStyle(ControllerEditorUtility.CalcError().annotationObserver)
                {fontSize = 7};
                x = EditorGUI.DelayedFloatField(source, x, style);
                y = EditorGUI.DelayedFloatField(rect2, y, style);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(RevertMapper(), "Set ChildState Position");
                    ChildAnimatorState[] states = RevertMapper().states;
                    if (states.ExcludeResolver((ChildAnimatorState childAnimatorState) => childAnimatorState.state == CS_0024_003C_003E8__locals6.m_MapperTests, out var c))
                    {
                        states[c].position = new Vector3(x, y);
                        RevertMapper().states = states;
                    }
                }
            }

            if (stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.indicators))
            {
                bool flag = CS_0024_003C_003E8__locals6.m_MapperTests.behaviours.Length != 0;
                bool writeDefaultValues = CS_0024_003C_003E8__locals6.m_MapperTests.writeDefaultValues;
                Rect res = new Rect(0f, 0f, _ReponseVisitor.x, _ReponseVisitor.y);
                if (stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.inactiveIndicators) || writeDefaultValues)
                {
                    GUI.Label(res.PatchResolver(20f, isserv: true).ManageResolver(14f), ControllerEditorUtility.CalcError()._ClientProcessor, writeDefaultValues ? ControllerEditorUtility.CalcError()._ClassProcessor : ControllerEditorUtility.CalcError()._VisitorObserver);
                }

                if (stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.inactiveIndicators) || flag)
                {
                    GUI.Label(res.PatchResolver(10f, isserv: true).ManageResolver(14f), ControllerEditorUtility.CalcError().m_AttributeProcessor, flag ? ControllerEditorUtility.CalcError()._ClassProcessor : ControllerEditorUtility.CalcError()._VisitorObserver);
                }
            }

            if (!stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.quickNewClip))
            {
                return;
            }

            GUI.Label(connection, "+", GUI.skin.label);
            if (!new EditorEvent(Event.current).ExcludeHelper().InvokeHelper().QueryHelper(connection))
            {
                return;
            }

            string text = $"{AddInitializer().lastAnimationPath}/{AddInitializer().lastAnimationName}.anim";
            bool flag2 = AssetDatabase.IsValidFolder(AddInitializer().lastAnimationPath);
            string defaultName = Path.GetFileNameWithoutExtension(flag2 ? AssetDatabase.GenerateUniqueAssetPath(text) : text) + ".anim";
            string text2 = EditorUtility.SaveFilePanel("New Animation Path", flag2 ? ((string)AddInitializer().lastAnimationPath) : "Assets", defaultName, "anim");
            if (string.IsNullOrWhiteSpace(text2))
            {
                return;
            }

            string projectRelativePath = FileUtil.GetProjectRelativePath(text2);
            if (!projectRelativePath.StartsWith("Assets"))
            {
                RateAnnotation(isi: true, "Asset Path must be a folder within Assets!");
                return;
            }

            ControllerEditorSettings.WorkerAlgo workerAlgo = new ControllerEditorSettings.WorkerAlgo();
            try
            {
                AddInitializer().lastAnimationPath.ResolveDefinition(Path.GetDirectoryName(projectRelativePath).Replace('\\', '/'));
                AddInitializer().lastAnimationName.ResolveDefinition(Path.GetFileNameWithoutExtension(projectRelativePath));
            }
            finally
            {
                if (workerAlgo != null)
                {
                    ((IDisposable)workerAlgo).Dispose();
                }
            }

            AnimationClip animationClip = new AnimationClip();
            AssetDatabase.CreateAsset(animationClip, projectRelativePath);
            CS_0024_003C_003E8__locals6.m_MapperTests.motion = animationClip;
            EditorUtility.SetDirty(CS_0024_003C_003E8__locals6.m_MapperTests);
        }

        private static void GraphGUINodeGUIPost(AnimatorState __state)
        {
            if (__state == null)
            {
                return;
            }

            bool flag = VisitAlgo(__state);
            if (!flag)
            {
                MapMapper(__state);
            }

            GUIStyle value;
            GUIStyle gUIStyle = (m_PolicyAnnotation.TryGetValue(__state.tag, out value) ? value : predicateAnnotation);
            if (gUIStyle == null)
            {
                return;
            }

            float num = ((gUIStyle.alignment < TextAnchor.MiddleLeft) ? 5f : (gUIStyle.fixedHeight / 2f - 15f));
            int num2 = ((gUIStyle.fontSize == 0) ? 15 : (gUIStyle.fontSize + 2));
            Event current = Event.current;
            EventType type = current.type;
            if (m_AlgoAnnotation.Contains(__state))
            {
                if (type == EventType.KeyDown)
                {
                    switch (current.keyCode)
                    {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            if (_IssuerVisitor.ReflectSerializer())
                            {
                                _IssuerVisitor.VerifySerializer(_IssuerVisitor.ListSerializer().Replace("\\n", "\n"));
                            }

                            _IssuerVisitor.MapSerializer(isconfig: true);
                            break;
                        case KeyCode.F2:
                            _IssuerVisitor.CalculateSerializer(flag ? new Rect(0f, 0f, gUIStyle.fixedWidth, gUIStyle.fixedHeight) : new Rect(gUIStyle.fixedWidth * 0.15f, num, gUIStyle.fixedWidth * 0.7f, num2), __state.name, 0, 0f);
                            break;
                        case KeyCode.Escape:
                            _IssuerVisitor.MapSerializer(isconfig: false);
                            break;
                    }
                }

                if (_IssuerVisitor.ReflectSerializer())
                {
                    string key = _IssuerVisitor.ListSerializer();
                    _IssuerVisitor.OnGUI(GUI.skin.textArea);
                    if (__state == m_AlgoAnnotation[0])
                    {
                        _IssuerVisitor.ValidateSerializer();
                    }
                    else
                    {
                        _IssuerVisitor.VerifySerializer(key);
                    }
                }
            }

            if (flag)
            {
                return;
            }

            ControllerEditorSettings.StateCosmeticOptions stateCosmeticOptions = AddInitializer().PostDefinition();
            bool flag2 = stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.motionName);
            bool flag3 = stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.motionIcon);
            if (flag2 || flag3)
            {
                float bottom = _ReponseVisitor.y - (stateCosmeticOptions.HasFlag(ControllerEditorSettings.StateCosmeticOptions.coordinates) ? 9f : 3f);
                Rect rect = new Rect(6f, num + num2, Mathf.Max(0f, _ReponseVisitor.x - 12f), Mathf.Min(16f, bottom - num - num2));
                if (rect.height < 10f || rect.width < 20f) return;
                GUIContent content = new GUIContent(flag2 ? (__state.motion ? "(" + ControllerMotionDisplay.Summary(__state.motion) + ")" : "(None)") : "");
                if (ControllerMotionDisplay.MixedWriteDefaults(LogoutMapper())) content.text = "WD! " + content.text;
                content.tooltip = content.text + (ControllerMotionDisplay.MixedWriteDefaults(LogoutMapper()) ? "\nThis controller mixes Write Defaults on and off." : "");
                var motionStyle = new GUIStyle(ControllerEditorUtility.CalcError()._ClassProcessor)
                {
                    alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, wordWrap = false,
                    fontSize = 9, fixedWidth = 0, fixedHeight = 0, padding = new RectOffset(2, 2, 0, 0)
                };
                Rect textRect = rect;
                if (__state.motion && flag3)
                {
                    var icon = __state.motion is AnimationClip animationClip ?
                        (animationClip.isLooping ? ControllerEditorUtility.DestroyError().readerProcessor.image : ControllerEditorUtility.DestroyError().m_RegistryProcessor.image) :
                        ControllerEditorUtility.DestroyError().m_PrinterProcessor.image;
                    if (icon) GUI.DrawTexture(new Rect(rect.x, rect.y + (rect.height - 12f) / 2f, 12f, 12f), icon, ScaleMode.ScaleToFit);
                    textRect.xMin += 14f;
                }
                Color previousMotionColour = GUI.contentColor;
                GUI.contentColor = ControllerMotionDisplay.NameColour(__state.motion ? __state.motion.name : "", previousMotionColour);
                try
                {
                    if (ControllerEditorExtraSettings.Current.motionSearch)
                    {
                        if (GUI.Button(textRect, content, motionStyle)) SearchStateMotion(__state, rect);
                    }
                    else GUI.Label(textRect, content, motionStyle);
                }
                finally { GUI.contentColor = previousMotionColour; }
            }
        }

        private static void MapMapper(AnimatorState config)
        {
            Event current = Event.current;
            EventType type = current.type;
            List<GameObject> list = DragAndDrop.objectReferences.Where(delegate (UnityEngine.Object o)
            {
                GameObject gameObject = o as GameObject;
                return gameObject?.scene.isLoaded ?? false;
            }).Cast<GameObject>().ToList();
            bool flag = list.Any();
            if (type == EventType.DragUpdated && flag)
            {
                poolVisitor = true;
            }

            if (type != EventType.DragPerform)
            {
                return;
            }

            Motion motion = DragAndDrop.objectReferences.FirstOrDefault((UnityEngine.Object o) => o is Motion) as Motion;
            if (!((bool)motion || flag))
            {
                return;
            }

            if ((bool)motion)
            {
                if (m_AlgoAnnotation.Count > 1 && m_AlgoAnnotation.Contains(config))
                {
                    UnityEngine.Object[] objectsToUndo = m_AlgoAnnotation.ToArray();
                    Undo.RecordObjects(objectsToUndo, "Drag & Drop to States");
                    foreach (AnimatorState item in m_AlgoAnnotation)
                    {
                        item.motion = motion;
                        item.PrintPredicate();
                    }
                }
                else
                {
                    Undo.RecordObject(config, "Drag & Drop to State");
                    config.motion = motion;
                    config.PrintPredicate();
                }
            }
            else if (flag)
            {
                _003C_003Ec__DisplayClass692_0 CS_0024_003C_003E8__locals7 = new _003C_003Ec__DisplayClass692_0();
                parameterVisitor = config;
                CS_0024_003C_003E8__locals7._InitializerTests = list[0].transform.root;
                CS_0024_003C_003E8__locals7._InitializerTests = CS_0024_003C_003E8__locals7._InitializerTests.GetComponentInChildren<Animator>()?.transform ?? CS_0024_003C_003E8__locals7._InitializerTests;
                List<GameObject> template = list.Where(delegate (GameObject o)
                {
                    if (o == CS_0024_003C_003E8__locals7._InitializerTests)
                    {
                        return true;
                    }

                    return o.transform.IsChildOf(CS_0024_003C_003E8__locals7._InitializerTests);
                }).ToList();
                QuickToggleWindow quickToggleWindow = QuickToggleWindow.AssetTests(m_AlgoAnnotation.Contains(parameterVisitor) ? m_AlgoAnnotation : new List<AnimatorState>{parameterVisitor}, CS_0024_003C_003E8__locals7._InitializerTests, template);
                Vector2 mousePosition = current.mousePosition;
                quickToggleWindow.SortTests(GUIUtility.GUIToScreenPoint(mousePosition));
            }

            EditorUtility.SetDirty(config);
            current.Use();
        }

        private static void StateNodeConnectPost(Node __instance, Node toNode)
        {
            _003C_003Ec__DisplayClass693_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass693_0();
            CS_0024_003C_003E8__locals5.definitionTests = (AnimatorState)tokenVisitor.GetValue(__instance);
            if (UnityAnimatorBridge.ConnectionTests.m_ExpressionTests != toNode.GetType())
            {
                AnimatorStateTransition animatorStateTransition = CS_0024_003C_003E8__locals5.definitionTests.transitions.Last();
                CustomizeAlgo(AddInitializer().defaultTransition, animatorStateTransition);
                animatorStateTransition.canTransitionToSelf = true;
                return;
            }

            m_DecoratorVisitor.Invoke(null, new object[3]{toNode, true, (GenericMenu.MenuFunction2)delegate (object data)
            {
                AnimatorStateTransition animatorStateTransition2 = null;
                if (!(data is AnimatorState destinationState))
                {
                    if (data is AnimatorStateMachine animatorStateMachine)
                    {
                        animatorStateTransition2 = ((!animatorStateMachine.states.GetRules()) ? CS_0024_003C_003E8__locals5.definitionTests.AddTransition(animatorStateMachine) : CS_0024_003C_003E8__locals5.definitionTests.transitions.Last());
                    }
                }
                else
                {
                    animatorStateTransition2 = CS_0024_003C_003E8__locals5.definitionTests.AddTransition(destinationState);
                }

                if (animatorStateTransition2 != null)
                {
                    CustomizeAlgo(AddInitializer().defaultTransition, animatorStateTransition2);
                    animatorStateTransition2.canTransitionToSelf = true;
                }
            }});
        }

        private static void AnyStateNodeConnectPost(object __instance, object toNode)
        {
            if (UnityAnimatorBridge.ConnectionTests.m_ExpressionTests != toNode.GetType())
            {
                CustomizeAlgo(AddInitializer().defaultTransition, ManageMapper().anyStateTransitions.Last());
                return;
            }

            m_DecoratorVisitor.Invoke(null, new object[3]{toNode, true, (GenericMenu.MenuFunction2)delegate (object data)
            {
                AnimatorStateTransition col = null;
                if (!(data is AnimatorState destinationState))
                {
                    if (data is AnimatorStateMachine animatorStateMachine)
                    {
                        col = ((!animatorStateMachine.states.GetRules()) ? ManageMapper().AddAnyStateTransition(animatorStateMachine) : ManageMapper().anyStateTransitions.Last());
                    }
                }
                else
                {
                    col = ManageMapper().AddAnyStateTransition(destinationState);
                }

                CustomizeAlgo(AddInitializer().defaultTransition, col);
            }});
        }

        internal static void ValidateMapper()
        {
            mappingVisitor = Type.GetType("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            _RepositoryVisitor = mappingVisitor.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void CustomizeMapper()
        {
            while (true)
            {
                EditorPatchRegistry.MapReg(mappingVisitor, "AddParameterMenu", null, EditorPatchRegistry.NewReg<object>(AddNewParameterPost));
                int num = 4;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 7:
                            EditorPatchRegistry.TestReg("UnityEditor.Graphs.ParameterControllerView+IntElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, EditorPatchRegistry.NewReg<Rect>(IntElementGUIPost));
                            num = 2;
                            continue;
                        case 0:
                            return;
                        case 3:
                            return;
                        case 2:
                            EditorPatchRegistry.TestReg("UnityEditor.Graphs.ParameterControllerView+FloatElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, EditorPatchRegistry.NewReg<Rect>(FloatElementGUIPost));
                            num = 1;
                            continue;
                        case 5:
                            break;
                        case 4:
                            EditorPatchRegistry.MapReg(mappingVisitor, "OnDrawParameter", typeof(ControllerEditor).GetMethod(nameof(DrawParameterPrefix), BindingFlags.Static | BindingFlags.NonPublic), EditorPatchRegistry.NewReg<int>(DrawParameterPost));
                            num = 7;
                            continue;
                        case 1:
                        case 6:
                            EditorPatchRegistry.TestReg("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "RenameEnd", EditorPatchRegistry.CreateReg(ParameterRenameEndPrefix), EditorPatchRegistry.CreateReg(ParameterRenameEndPost));
                            num = 3;
                            continue;
                    }

                    break;
                }
            }
        }

        private static void DrawParameterPrefix(object __instance, ref Rect rect, int index, out int __state)
        {
            __state = -1;
            var controller = LogoutMapper();
            var parameter = ControllerParameterTools.ParameterAt(__instance, index);
            int parameterIndex = controller && parameter != null ? Array.FindIndex(controller.parameters, item => item.name == parameter.name) : -1;
            if (parameterIndex >= 0)
            {
                float inset = ControllerParameterTools.DrawBadge(rect, controller, parameter, () => ShowParameterUsages(controller, parameter.name));
                rect.x += inset; rect.width -= inset;
            }
            Event current = Event.current;
            if (current.type == EventType.MouseUp && current.button == 1 && rect.Contains(current.mousePosition))
            {
                __state = parameterIndex;
            }
        }

        private static void DrawParameterPost(int __state)
        {
            _003C_003Ec__DisplayClass702_0 CS_0024_003C_003E8__locals19 = new _003C_003Ec__DisplayClass702_0();
            CS_0024_003C_003E8__locals19.m_TestsTests = __state;
            if (CS_0024_003C_003E8__locals19.m_TestsTests < 0)
            {
                return;
            }

            CS_0024_003C_003E8__locals19.m_RegTests = LogoutMapper().parameters[CS_0024_003C_003E8__locals19.m_TestsTests];
            if (CS_0024_003C_003E8__locals19.m_RegTests.type == UnityEngine.AnimatorControllerParameterType.Float)
            {
                _003C_003Ec__DisplayClass702_1 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass702_1();
                CS_0024_003C_003E8__locals15.processorTests = CS_0024_003C_003E8__locals19;
                CS_0024_003C_003E8__locals15.m_PropertyTests = DefineInitializer();
                bool flag = FindInitializer() != null && CS_0024_003C_003E8__locals15.m_PropertyTests != null;
                m_SingletonVisitor.AddItem(new GUIContent("Animate"), on: false, (!flag) ? null : ((GenericMenu.MenuFunction)(() =>
                {
                    EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Animator), CS_0024_003C_003E8__locals15.processorTests.m_RegTests.name);
                    if (RateAnnotation(AnimationUtility.GetEditorCurve(CS_0024_003C_003E8__locals15.m_PropertyTests, binding) != null, "Property \"" + CS_0024_003C_003E8__locals15.processorTests.m_RegTests.name + "\" already exists in the active animation clip \"" + CS_0024_003C_003E8__locals15.m_PropertyTests.name + "\"."))
                    {
                    }
                    else
                    {
                        AnimationUtility.SetEditorCurve(CS_0024_003C_003E8__locals15.m_PropertyTests, binding, new AnimationCurve());
                        FindInitializer().Repaint();
                    }
                })));
            }

            m_SingletonVisitor.AddSeparator(string.Empty);
            m_SingletonVisitor.AddItem(new GUIContent("Find usages"), false, () => ShowParameterUsages(LogoutMapper(), TestAnnotation(CS_0024_003C_003E8__locals19.m_TestsTests)));
            ControllerParameterTools.AddMenu(m_SingletonVisitor, LogoutMapper(), CS_0024_003C_003E8__locals19.m_RegTests);
            m_SingletonVisitor.AddItem(new GUIContent("Convert/Bool"), on: false, delegate
            {
                PublishAlgo(LogoutMapper(), CS_0024_003C_003E8__locals19.m_TestsTests, UnityEngine.AnimatorControllerParameterType.Bool);
            });
            m_SingletonVisitor.AddItem(new GUIContent("Convert/Int"), on: false, delegate
            {
                PublishAlgo(LogoutMapper(), CS_0024_003C_003E8__locals19.m_TestsTests, UnityEngine.AnimatorControllerParameterType.Int);
            });
            m_SingletonVisitor.AddItem(new GUIContent("Convert/Float"), on: false, delegate
            {
                PublishAlgo(LogoutMapper(), CS_0024_003C_003E8__locals19.m_TestsTests, UnityEngine.AnimatorControllerParameterType.Float);
            });
            m_SingletonVisitor.AddItem(new GUIContent("Convert/Trigger"), on: false, delegate
            {
                PublishAlgo(LogoutMapper(), CS_0024_003C_003E8__locals19.m_TestsTests, UnityEngine.AnimatorControllerParameterType.Trigger);
            });
            m_SingletonVisitor.ShowAsContext();
        }

        private static void AddNewParameterPost(object __instance)
        {
            buildingParameterMenu = false;
            ControllerParameterTools.ClearCache();
            _RepositoryVisitor.SetValue(__instance, new Vector2(0f, 2.1474836E+09f));
        }

        private static void ParameterRenameEndPrefix()
        {
            m_ComposerVisitor = LogoutMapper().parameters;
        }

        private static void ParameterRenameEndPost()
        {
            var controller = LogoutMapper();
            if (!controller || m_ComposerVisitor == null) return;
            var parameters = controller.parameters;
            for (int index = 0; index < Math.Min(parameters.Length, m_ComposerVisitor.Length); index++)
            {
                string before = m_ComposerVisitor[index].name, after = parameters[index].name;
                if (before == after) continue;
                ControllerParameterNotes.Rename(controller, before, after);
                if (SerializedPropertyGroup.InvokePage())
                    foreach (var layer in controller.layers) PostAlgo(layer.stateMachine, before, after, isord2: true);
                CustomizeAnnotation("Renamed " + before + " to " + after + ".");
            }
            ControllerParameterTools.ClearCache();
        }

        private static void IntElementGUIPost(Rect rect)
        {
            CalcAnnotation(rect, AddInitializer().capitalParameterIndicator ? "I" : "i");
        }

        private static void FloatElementGUIPost(Rect rect)
        {
            CalcAnnotation(rect, AddInitializer().capitalParameterIndicator ? "F" : "f");
        }

        static ControllerEditor()
        {
            _Process = new List<UnityAnimatorBridge.TokenizerTests>();
            _Producer = new ConcurrentBag<UnityAnimatorBridge.BaseTests>();
            _Iterator = new List<AnimatorTransitionBase>();
            _VisitorAnnotation = new List<AnimatorState>();
            m_AlgoAnnotation = new List<AnimatorState>();
            _TestsAnnotation = new List<AnimatorStateTransition>();
            propertyAnnotation = new List<AnimatorTransitionBase>();
            serverAnnotation = new List<UnityAnimatorBridge.InstanceTests>();
            m_ThreadAnnotation = new List<AnimatorCondition>();
            m_PolicyAnnotation = new Dictionary<string, GUIStyle>();
            m_SerializerAnnotation = new HashSet<string>();
            _PageAnnotation = new List<string>();
            _QueueAnnotation = new List<AlgoAlgo>();
            setterAnnotation = new List<SerializedPropertyGroup.BridgeProperty>();
            m_BridgeAnnotation = "";
            strategyAnnotation = "";
            m_CustomerAnnotation = "";
            infoAnnotation = new Type[2]{typeof(ControllerEditor), typeof(ControllerEditorSettingsWindow)};
            factoryAnnotation = new AnimBool();
            _AccountAnnotation = new AnimBool();
            m_RefAnnotation = "3.3.2";
            statusAnnotation = new (string, string)[1]{("Templates", "https://notes.sleightly.dev/templates/")};
            _TokenAnnotation = new List<GlobalVisitor>();
            m_CodeAnnotation = new List<GlobalVisitor>();
            m_DicAnnotation = new List<GlobalVisitor>();
            _ModelAnnotation = -1;
            m_ItemAnnotation = new Dictionary<Type, string[]>();
            _SpecificationAnnotation = new Dictionary<Shader, string[]>();
            structAnnotation = true;
            serviceAnnotation = true;
            m_GlobalAnnotation = ControllerAction.Copy;
            _TaskAnnotation = ActionMode.CurrentController;
            m_ProcessAnnotation = MoveMode.CurrentLayer;
            m_ContextVisitor = new HashSet<Vector3>();
            _TagVisitor = true;
            writerVisitor = LayerViewViewType.DefaultView;
            m_ComparatorVisitor = EditorPatchRegistry.NewReg<AnimatorState>(QueryAlgo);
        }

        [CompilerGenerated]
        internal static void VerifyMapper()
        {
            while (true)
            {
                _Service = new ControllerAssetInventory(LogoutMapper());
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            _ModelAnnotation = -1;
                            num = 2;
                            continue;
                        case 2:
                            m_Config = false;
                            num = 3;
                            continue;
                        case 3:
                            _Client = false;
                            return;
                        case 1:
                            break;
                    }

                    break;
                }
            }
        }

        [CompilerGenerated]
        internal static void FillMapper()
        {
            bool config = m_Config;
            VerifyMapper();
            m_Config = config;
        }

        [CompilerGenerated]
        internal static void WriteMapper()
        {
            bool client = _Client;
            VerifyMapper();
            _Client = client;
        }

        [CompilerGenerated]
        internal static void ForgotMapper(bool istask)
        {
            int num2 = default(int);
            UnityEngine.Object[] objs = default(UnityEngine.Object[]);
            while (true)
            {
                _003C_003Ec__DisplayClass289_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass289_0();
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            break;
                        case 2:
                            CS_0024_003C_003E8__locals4.observerReg = istask;
                            num = 4;
                            continue;
                        case 6:
                            return;
                        case 4:
                            goto case 5;
                        case 5:
                        {
                            AnimatorState[] array = _Service.m_ServiceVisitor.Where((AnimatorState s) =>
                            {
                                if (s.name.IndexOf("(wd on)", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    return false;
                                }

                                return s.name.IndexOf("(wd off)", StringComparison.OrdinalIgnoreCase) < 0;
                            }).ToArray();
                            num2 = array.Count((AnimatorState s) => s.writeDefaultValues != CS_0024_003C_003E8__locals4.observerReg);
                            objs = array;
                            num = 1;
                            continue;
                        }

                        default:
                        {
                            SerializedObject serializedObject = new SerializedObject(objs);
                            serializedObject.FindProperty("m_WriteDefaultValues").boolValue = CS_0024_003C_003E8__locals4.observerReg;
                            serializedObject.ApplyModifiedProperties();
                            num = 7;
                            continue;
                        }

                        case 7:
                            CustomizeAnnotation($"Set {num2} States' Write Defaults to {CS_0024_003C_003E8__locals4.observerReg}.");
                            num = 6;
                            continue;
                    }

                    break;
                }
            }
        }

        [CompilerGenerated]
        internal static void StopMapper(AnimatorState reference, bool connectionreguired)
        {
            SerializedObject serializedObject = new SerializedObject(reference);
            serializedObject.FindProperty("m_WriteDefaultValues").boolValue = connectionreguired;
            serializedObject.ApplyModifiedProperties();
        }

        [CompilerGenerated]
        internal static bool CheckMapper(SerializedProperty task)
        {
            if (task.boolValue)
            {
                return true;
            }

            return task.hasMultipleDifferentValues;
        }

        [CompilerGenerated]
        internal static void PrepareMapper(string ident, SerializedProperty selection, SerializedProperty field, SerializedProperty def2, string[] var13, bool res4stop = false)
        {
            if (_Schema == null)
            {
                ConnectAnnotation();
                if (_Schema == null)
                {
                    return;
                }
            }

            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            try
            {
                if (res4stop)
                {
                    EditorGUI.indentLevel++;
                }

                bool flag = CheckMapper(def2);
                MixedValueScope managerThread;
                if (res4stop || flag)
                {
                    using (new EditorGUI.DisabledScope(!flag))
                    {
                        _ = field.stringValue;
                        object[] parameters = new object[3]{new GUIContent(ident), field.stringValue, var13};
                        try
                        {
                            managerThread = new MixedValueScope(field);
                            try
                            {
                                EditorGUI.BeginChangeCheck();
                                string stringValue = (string)ControllerEditorUtility.CalcError().LoginError().Invoke(null, parameters);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    field.stringValue = stringValue;
                                }
                            }
                            finally
                            {
                                if (managerThread != null)
                                {
                                    ((IDisposable)managerThread).Dispose();
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else if (selection != null)
                {
                    EditorGUILayout.PropertyField(selection);
                }
                else
                {
                    EditorGUILayout.LabelField(ident);
                }

                if (res4stop)
                {
                    EditorGUI.indentLevel--;
                }

                managerThread = new MixedValueScope(def2);
                try
                {
                    EditorGUI.BeginChangeCheck();
                    bool boolValue = EditorGUILayout.ToggleLeft("Parameter", def2.boolValue, GUILayout.MaxWidth(100f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        def2.boolValue = boolValue;
                    }
                }
                finally
                {
                    if (managerThread != null)
                    {
                        ((IDisposable)managerThread).Dispose();
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

        [CompilerGenerated]
        internal static bool AssetMapper(string task)
        {
            if (structAnnotation)
            {
                if (task == _SchemaAnnotation)
                {
                    return true;
                }
            }

            if (!structAnnotation)
            {
                return task.Contains(_SchemaAnnotation);
            }

            return false;
        }

        [CompilerGenerated]
        internal static bool UpdateMapper(string task)
        {
            if (structAnnotation)
            {
                if (task == _SchemaAnnotation)
                {
                    return true;
                }
            }

            if (!structAnnotation)
            {
                return task.Contains(_SchemaAnnotation);
            }

            return false;
        }

        [CompilerGenerated]
        internal static bool ChangeMapper(bool vclose, ref _003C_003Ec__DisplayClass383_1 visitor, ref _003C_003Ec__DisplayClass383_2 comp, ref _003C_003Ec__DisplayClass383_3 second2)
        {
            while (true)
            {
                int num;
                if (!vclose)
                {
                    num = 4;
                    goto IL_001d;
                }

                goto IL_00db;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 5:
                                break;
                            default:
                                return vclose;
                            case 6:
                                goto IL_009a;
                            case 2:
                                goto IL_00db;
                            case 1:
                                visitor.m_PrinterReg = comp._WriterReg.name + " -> " + second2._ParamsReg.name;
                                num = 3;
                                continue;
                        }

                        break;
                    }

                continue;
                IL_00db:
                    ;
                goto IL_009a;
                IL_009a:
                    visitor.m_RequestReg = true;
                num = 1;
                goto IL_001d;
            }
        }

        [CompilerGenerated]
        internal static void SortMapper(AnimatorTransitionBase[] key, out List<AnimatorTransitionBase> counter, out List<AnimatorTransitionBase> helper, ref _003C_003Ec__DisplayClass384_0 asset2)
        {
            ComputeAlgo(key, asset2.listenerReg, asset2.m_GetterReg, asset2.m_InterceptorReg, out counter, out helper);
        }

        [CompilerGenerated]
        internal static float RegisterMapper(float def)
        {
            return (float)def.ToString().Length * 4f + 4f;
        }
    }
}
