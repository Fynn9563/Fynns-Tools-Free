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
        [Serializable]
        [CompilerGenerated]
        internal sealed class _003C_003Ec
        {
            public static readonly _003C_003Ec watcherInitializer = new _003C_003Ec();
            public static Func<bool> candidateInitializer;
            public static Func<UnityAnimatorBridge.TokenizerTests, bool> productInitializer;
            public static Func<UnityAnimatorBridge.TokenizerTests, bool> _ExpressionInitializer;
            public static Func<UnityAnimatorBridge.TokenizerTests, bool> systemInitializer;
            public static Func<UnityAnimatorBridge.InstanceTests, AnimatorTransitionBase> workerInitializer;
            public static Func<AnimatorTransitionBase, bool> m_FilterInitializer;
            public static Func<AnimatorTransitionBase, bool> _StubInitializer;
            public static Func<bool> readerInitializer;
            public static Func<bool> bridgeInitializer;
            public static Func<bool> m_StrategyInitializer;
            public static Func<GUIStyle, GUIStyle> _CustomerInitializer;
            public static Func<GUIStyle, GUIStyle> databaseInitializer;
            public static Func<GUIStyle, GUIStyle> m_ExporterInitializer;
            public static Func<GUIStyle, GUIStyle> identifierInitializer;
            public static Func<GUIStyle, GUIStyle> _AttrInitializer;
            public static Func<GUIStyle, GUIStyle> dispatcherInitializer;
            public static Func<GUIStyle, GUIStyle> _RegistryInitializer;
            public static Func<GUIStyle, GUIStyle> tagInitializer;
            public static GenericMenu.MenuFunction _ImporterInitializer;
            public static GenericMenu.MenuFunction _RequestInitializer;
            public static GenericMenu.MenuFunction printerInitializer;
            public static GenericMenu.MenuFunction m_WriterInitializer;
            public static Func<AlgoAlgo, bool> m_ParamsInitializer;
            public static Func<StateMachineBehaviour, bool> _ListenerInitializer;
            public static Func<AnimatorStateTransition, bool> m_GetterInitializer;
            public static Func<AnimatorStateTransition, string> _InterceptorInitializer;
            public static Func<object[], bool[]> creatorInitializer;
            public static Func<Animator, bool> eventInitializer;
            public static Func<Type, IEnumerable<MethodInfo>> infoInitializer;
            public static Func<System.Reflection.Assembly, IEnumerable<MethodInfo>> _FacadeInitializer;
            public static Func<(MethodInfo, EditorExtensionAttribute, bool), int> m_AdvisorInitializer;
            public static Action m_CallbackInitializer;
            public static Action _IndexerInitializer;
            public static Func<bool> m_IssuerInitializer;
            public static Action<System.Exception> m_PrototypeInitializer;
            public static Action m_RuleInitializer;
            public static Action<FieldAlgo> singletonInitializer;
            public static Action<System.Exception> _FactoryInitializer;
            public static Func<PoolAlgo, bool> _AccountInitializer;
            public static Func<string, bool> m_RefInitializer;
            public static Func<(bool, string), bool> statusInitializer;
            public static Func<(bool, string), string> _TokenInitializer;
            public static Func<(bool, string), bool> m_CodeInitializer;
            public static Func<(bool, string), string> _DicInitializer;
            public static Func<bool> m_InvocationInitializer;
            public static Func<bool> roleInitializer;
            public static Action<FieldAlgo> _ParamInitializer;
            public static Action<System.Exception> _ModelInitializer;
            public static Action m_TokenizerInitializer;
            public static Action _DecoratorInitializer;
            public static Action<FieldAlgo> m_ComparatorInitializer;
            public static Action<System.Exception> m_ExceptionInitializer;
            public static Action objectInitializer;
            public static GenericMenu.MenuFunction _UtilsInitializer;
            public static GenericMenu.MenuFunction m_ValInitializer;
            public static GenericMenu.MenuFunction m_ValueInitializer;
            public static GenericMenu.MenuFunction m_MerchantInitializer;
            public static GenericMenu.MenuFunction _AuthenticationInitializer;
            public static GenericMenu.MenuFunction reponseInitializer;
            public static GenericMenu.MenuFunction poolInitializer;
            public static GenericMenu.MenuFunction _ParameterInitializer;
            public static GenericMenu.MenuFunction _ComposerInitializer;
            public static Action repositoryInitializer;
            public static Action<System.Exception> mappingInitializer;
            public static Action m_BaseInitializer;
            public static Func<Task> _ContainerInitializer;
            public static Func<bool> classInitializer;
            public static Func<bool> m_MockInitializer;
            public static Func<bool> m_InstanceInitializer;
            public static Func<bool> _FieldInitializer;
            public static Func<bool> attributeInitializer;
            public static Func<bool> _ClientInitializer;
            public static Func<AnimatorState, bool> m_ConfigInitializer;
            public static Func<AnimatorState, bool> m_DescriptorInitializer;
            public static Func<AnimatorState, bool> m_TemplateInitializer;
            public static Func<bool> messageInitializer;
            public static Func<ChildAnimatorState, AnimatorState> collectionInitializer;
            public static Func<UnityAnimatorBridge.TokenizerTests, bool> _ParserInitializer;
            public static Func<AnimatorStateTransition, bool> _ManagerInitializer;
            public static Func<bool> itemInitializer;
            public static Func<UnityAnimatorBridge.InstanceTests, bool> specificationInitializer;
            public static Func<System.Reflection.Assembly, IEnumerable<Type>> _MethodInitializer;
            public static Func<Type, bool> _SchemaInitializer;
            public static Func<Type, string> broadcasterInitializer;
            public static Func<EditorCurveBinding, bool> proxyInitializer;
            public static Func<EditorCurveBinding, string> structInitializer;
            public static Func<string, string> _ServiceInitializer;
            public static Func<EditorCurveBinding, string> m_StateInitializer;
            public static Func<string, string> globalInitializer;
            public static Func<GlobalVisitor, bool> _TaskInitializer;
            public static Func<GlobalVisitor, AnimatorCondition> m_ProcessInitializer;
            public static Func<GlobalVisitor, AnimatorCondition> producerInitializer;
            public static Func<AnimatorStateTransition, bool> m_IteratorInitializer;
            public static Func<AnimatorStateTransition, bool> publisherInitializer;
            public static Func<AnimatorStateTransition, bool> _ConfigurationInitializer;
            public static Func<AnimatorStateTransition, bool> procInitializer;
            public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> wrapperDefinition;
            public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> _AnnotationDefinition;
            public static Func<AnimatorStateTransition, bool> _VisitorDefinition;
            public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> m_AlgoDefinition;
            public static Func<UnityEditor.Animations.AnimatorControllerLayer, AnimatorStateMachine> m_MapperDefinition;
            public static Func<UnityEditor.Animations.AnimatorControllerLayer, AnimatorStateMachine> initializerDefinition;
            public static Func<UnityEngine.AnimatorControllerParameter, bool> _DefinitionDefinition;
            public static Func<AnimatorStateTransition, bool> m_RegDefinition;
            public static Action<AnimatorState> _TestsDefinition;
            public static Func<AnimatorTransition, bool> propertyDefinition;
            public static Func<AnimatorStateTransition, bool> _ProcessorDefinition;
            public static Action<AnimatorStateMachine> m_ObserverDefinition;
            public static Action<AnimatorStateMachine> _ServerDefinition;
            public static Func<UnityEngine.AnimatorControllerParameter, bool> _ThreadDefinition;
            public static Func<ChildMotion, Motion> m_PolicyDefinition;
            public static Func<ChildAnimatorStateMachine, AnimatorStateMachine> serializerDefinition;
            public static Func<ChildAnimatorStateMachine, AnimatorStateMachine> pageDefinition;
            public static Func<ChildAnimatorState, AnimatorState> resolverDefinition;
            public static Action<AnimatorTransitionBase> _PredicateDefinition;
            public static Func<ChildAnimatorState, bool> m_RulesDefinition;
            public static Func<ChildAnimatorStateMachine, bool> queueDefinition;
            public static Func<Vector3, ChildAnimatorState, Vector3> errorDefinition;
            public static Func<Vector3, ChildAnimatorStateMachine, Vector3> setterDefinition;
            public static Func<ChildAnimatorState, bool> _ConnectionDefinition;
            public static Func<ChildAnimatorState, float> contextDefinition;
            public static Func<ChildAnimatorState, bool> recordDefinition;
            public static Func<ChildAnimatorState, float> m_HelperDefinition;
            public static Func<ChildAnimatorState, AnimatorState> m_ConsumerDefinition;
            public static Func<UnityEngine.Object, bool> _AdapterDefinition;
            public static Func<string, bool> m_InterpreterDefinition;
            public static Func<string, bool> m_WatcherDefinition;
            public static Func<string, bool> m_CandidateDefinition;
            public static Func<UnityEngine.Object, bool> productDefinition;
            public static Func<AnimatorTransitionBase, bool> expressionDefinition;
            public static Func<AnimatorStateTransition, bool> systemDefinition;
            public static Func<UnityEngine.Object, bool> m_WorkerDefinition;
            public static Func<AnimatorStateTransition, bool> _FilterDefinition;
            public static Func<AnimatorStateTransition, bool> stubDefinition;
            public static Func<AnimatorTransition, bool> _ReaderDefinition;
            public static Action<UnityAnimatorBridge.InstanceTests> _BridgeDefinition;
            public static Func<ChildAnimatorState, bool> m_StrategyDefinition;
            public static Action<UnityEngine.Object> m_CustomerDefinition;
            public static Action<IEnumerable<UnityEditor.Animations.AnimatorController>> databaseDefinition;
            public static Action<UnityEngine.Object> _ExporterDefinition;
            public static Action<IEnumerable<GameObject>> identifierDefinition;
            public static Func<object, bool> attrDefinition;
            public static Func<object, EditorCurveBinding> dispatcherDefinition;
            public static Func<Renderer, IEnumerable<Material>> m_RegistryDefinition;
            public static Func<Material, Shader> m_TagDefinition;
            public static Action<SearchPopup<string>.UtilsServer> importerDefinition;
            public static Func<string, string, bool> _RequestDefinition;
            public static Func<Component, Type> m_PrinterDefinition;
            public static Func<Type, bool> _WriterDefinition;
            public static Action<SearchPopup<Type>.UtilsServer> paramsDefinition;
            public static Func<Type, object[]> m_ListenerDefinition;
            public static Func<Type, string, bool> m_GetterDefinition;
            public static Func<ObjectReferenceKeyframe, float> interceptorDefinition;
            public static Func<IGrouping<float, ObjectReferenceKeyframe>, ObjectReferenceKeyframe> m_CreatorDefinition;
            public static Func<Keyframe, float> eventDefinition;
            public static Func<IGrouping<float, Keyframe>, Keyframe> m_InfoDefinition;
            public static Func<UnityAnimatorBridge.BaseTests, Vector3> _FacadeDefinition;
            public static Func<object[], bool[]> _AdvisorDefinition;
            public static Comparison<ImporterMapper> callbackDefinition;
            public static Comparison<EventMapper> m_IndexerDefinition;
            public static Func<string, string> m_IssuerDefinition;
            public static Func<ImporterMapper, bool> m_PrototypeDefinition;
            public static Func<ChildAnimatorState, AnimatorState> _RuleDefinition;
            public static Func<ChildAnimatorState, AnimatorState> m_SingletonDefinition;
            public static Func<ChildAnimatorState, string> _FactoryDefinition;
            public static Func<object> accountDefinition;
            public static Action<bool> refDefinition;
            public static Func<MethodInfo, bool> statusDefinition;
            public static Func<MethodInfo, bool> tokenDefinition;
            public static Func<UnityEngine.Object, bool> codeDefinition;
            public static Func<UnityEngine.Object, bool> m_DicDefinition;
            public static GenericMenu.MenuFunction2 invocationDefinition;
            internal static _003C_003Ec StopInfo;
            internal Motion ResolveProcessor(ChildMotion c)
            {
                return c.motion;
            }

            internal IEnumerable<Material> CancelObserver(Renderer c)
            {
                return c.sharedMaterials;
            }

            internal Shader CountObserver(Material m)
            {
                return m.shader;
            }

            internal void DisableObserver(SearchPopup<string>.UtilsServer i)
            {
                GUILayout.Label(i._ValueServer, EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                ControllerEditorUtility.StartQueue();
            }

            internal bool InsertObserver(string p, string s)
            {
                return p.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1;
            }

            internal Type RestartObserver(Component c)
            {
                return c.GetType();
            }

            internal bool QueryObserver(Type t)
            {
                return VerifyVisitor(t);
            }

            internal void AddObserver(SearchPopup<Type>.UtilsServer i)
            {
                GUILayout.Label((GUIContent)i.CloneConnection(), EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                ControllerEditorUtility.StartQueue();
            }

            internal object[] InvokeObserver(Type type)
            {
                return new object[1]{new GUIContent(image: EditorGUIUtility.ObjectContent(null, type).image ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image, text: type.Name, tooltip: type.AssemblyQualifiedName)};
            }

            internal bool FindObserver(Type t, string s)
            {
                return t.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1;
            }

            internal bool[] ReadObserver(object[] values)
            {
                return ControllerEditorUtility.CheckRules<bool>(((string)values[0]).FlushResolver());
            }

            internal bool AwakeObserver(ImporterMapper c2)
            {
                return c2.listenerMapper.Count > 0;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass121_0
        {
            public string _DecoratorDefinition;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass285_0
        {
            public bool _IteratorDefinition;
            public string publisherDefinition;
            public AnimatorCondition m_ConfigurationDefinition;
            public bool m_ProcDefinition;
            internal void CompareServer(Rect targetRect)
            {
                Event current = Event.current;
                if (_IteratorDefinition && current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
                {
                    GUI.FocusControl(string.Empty);
                }

                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(publisherDefinition);
                m_ConfigurationDefinition.parameter = EditorGUI.DelayedTextField(_IteratorDefinition ? targetRect : Rect.zero, m_ConfigurationDefinition.parameter);
                m_ProcDefinition = EditorGUI.EndChangeCheck();
                if (current.type == EventType.MouseUp && current.button == 1 && targetRect.Contains(current.mousePosition))
                {
                    GUI.FocusControl(publisherDefinition);
                    current.Use();
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass285_1
        {
            public List<AnimatorTransitionBase> mapperReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass285_2
        {
            public string initializerReg;
            public List<AnimatorTransitionBase> m_DefinitionReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass289_0
        {
            public bool observerReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass308_0
        {
            public AnimatorStateTransition serverReg;
            internal bool InvokeServer(ChildAnimatorState c)
            {
                return c.state == serverReg.destinationState;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass339_0
        {
            public Type m_PolicyReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass370_0
        {
            public UnityEditor.Animations.AnimatorController serializerReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass370_1
        {
            public string _ResolverReg;
            internal bool VisitServer(UnityEngine.AnimatorControllerParameter p2)
            {
                return p2.name != _ResolverReg;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass374_0
        {
            public List<UnityEngine.AnimatorControllerParameter> predicateReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass375_0
        {
            public HashSet<UnityEngine.Object> consumerReg;
            internal void ValidateServer(UnityEngine.Object o)
            {
                consumerReg.Add(o);
            }

            internal void CustomizeServer(AnimatorStateMachine m)
            {
                _003C_003Ec__DisplayClass375_1 _003C_003Ec__DisplayClass375_ = new _003C_003Ec__DisplayClass375_1();
                _003C_003Ec__DisplayClass375_.watcherReg = this;
                _003C_003Ec__DisplayClass375_.interpreterReg = m;
                ValidateServer(_003C_003Ec__DisplayClass375_.interpreterReg);
                _003C_003Ec__DisplayClass375_.interpreterReg.states.InvokeResolver(delegate (ChildAnimatorState s)
                {
                    while (true)
                    {
                        ValidateServer(s.state);
                        int num = 1;
                        while (true)
                        {
                            switch (num)
                            {
                                case 5:
                                    ((IEnumerable<StateMachineBehaviour>)s.state.behaviours).InvokeResolver((Action<StateMachineBehaviour>)delegate (StateMachineBehaviour o)
                                    {
                                        consumerReg.Add(o);
                                    });
                                    num = 0;
                                    continue;
                                default:
                                    s.state.motion.ForEachMotion((Action<Motion>)delegate (Motion o)
                                    {
                                        consumerReg.Add(o);
                                    });
                                    num = 4;
                                    continue;
                                case 4:
                                    return;
                                case 2:
                                    break;
                                case 6:
                                    return;
                                case 1:
                                    goto case 3;
                                case 3:
                                    s.state.transitions.InvokeResolver(delegate (AnimatorStateTransition t)
                                    {
                                        if ((bool)t)
                                        {
                                            if ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit)
                                            {
                                                ValidateServer(t);
                                            }
                                        }
                                    });
                                    num = 5;
                                    continue;
                            }

                            break;
                        }
                    }
                });
                ((IEnumerable<StateMachineBehaviour>)_003C_003Ec__DisplayClass375_.interpreterReg.behaviours).InvokeResolver((Action<StateMachineBehaviour>)delegate (StateMachineBehaviour o)
                {
                    consumerReg.Add(o);
                });
                _003C_003Ec__DisplayClass375_.interpreterReg.entryTransitions.InvokeResolver(delegate (AnimatorTransition t)
                {
                    if ((bool)t)
                    {
                        if ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit)
                        {
                            ValidateServer(t);
                        }
                    }
                });
                _003C_003Ec__DisplayClass375_.interpreterReg.anyStateTransitions.InvokeResolver(delegate (AnimatorStateTransition t)
                {
                    if ((bool)t)
                    {
                        if ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit)
                        {
                            ValidateServer(t);
                        }
                    }
                });
                _003C_003Ec__DisplayClass375_.interpreterReg.stateMachines.InvokeResolver(_003C_003Ec__DisplayClass375_.RunServer);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass375_1
        {
            public AnimatorStateMachine interpreterReg;
            public _003C_003Ec__DisplayClass375_0 watcherReg;
            internal void RunServer(ChildAnimatorStateMachine c)
            {
                interpreterReg.GetStateMachineTransitions(c.stateMachine).InvokeResolver(watcherReg.ValidateServer);
                watcherReg.CustomizeServer(c.stateMachine);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass378_0
        {
            public Dictionary<string, string> _CandidateReg;
            public UnityEditor.Animations.AnimatorController m_ProductReg;
            public Dictionary<Motion, Motion> m_ExpressionReg;
            public HashSet<Motion> systemReg;
            internal string CloneServer(string name)
            {
                MatchCollection matchCollection = Regex.Matches(name, "%(.+?)%");
                if (matchCollection.Count > 0)
                {
                    for (int num = matchCollection.Count - 1; num >= 0; num--)
                    {
                        Match match = matchCollection[num];
                        string text = match.Groups[1].Value;
                        if (_CandidateReg.TryGetValue(text, out var value))
                        {
                            text = value;
                        }

                        name = name.Remove(match.Index, match.Length).Insert(match.Index, text);
                    }
                }

                return name;
            }

            internal T ReflectServer<T>(T m)
                where T : Motion
            {
                object obj;
                if (!(m is AnimationClip))
                {
                    obj = ".asset";
                }
                else
                {
                    obj = ".anim";
                }

                string text = (string)obj;
                string text2 = $"{AddInitializer().saveFolder}/Animation Clips/{m_ProductReg.name}";
                ControllerEditorUtility.InstantiateList(text2, overridecust: false, ControllerEditorUtility.PathOption.ForceFolder);
                string text3 = ControllerEditorUtility.AwakeList(text2, m.name + text, writestate: true);
                bool proc;
                T val = ControllerEditorUtility.SetRules(m, text3, out proc, isinfo2: false);
                if (proc)
                {
                    ControllerEditorUtility.InterruptPredicate(val, m_ProductReg);
                }
                else
                {
                    AssetDatabase.ImportAsset(text3);
                }

                m_ExpressionReg.Add(m, val);
                return val;
            }

            internal Motion DeleteServer(Motion m)
            {
                if (!m)
                {
                    return null;
                }

                if (!m_ExpressionReg.TryGetValue(m, out var value))
                {
                    if (!systemReg.Add(m))
                    {
                        return m;
                    }

                    if (!(m is UnityEditor.Animations.BlendTree blendTree))
                    {
                        AnimationClip animationClip = (AnimationClip)m;
                        AnimationClip animationClip2 = animationClip;
                        bool flag = false;
                        if (animationClip.name.StartsWith("T_"))
                        {
                            animationClip2 = ReflectServer(animationClip);
                            flag = true;
                        }

                        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animationClip);
                        for (int i = 0; i < curveBindings.Length; i++)
                        {
                            EditorCurveBinding binding = curveBindings[i];
                            if (!(binding.type != typeof(Animator)) && !(binding.path != string.Empty) && _CandidateReg.ContainsKey(binding.propertyName))
                            {
                                if (!flag)
                                {
                                    animationClip2 = ReflectServer(animationClip);
                                    flag = true;
                                }

                                AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(animationClip2, binding);
                                AnimationUtility.SetEditorCurve(animationClip2, binding, null);
                                binding.propertyName = _CandidateReg[binding.propertyName];
                                AnimationUtility.SetEditorCurve(animationClip2, binding, editorCurve);
                                EditorUtility.SetDirty(animationClip2);
                            }
                        }

                        return animationClip2;
                    }

                    UnityEditor.Animations.BlendTree blendTree2 = blendTree;
                    bool flag2 = false;
                    if (blendTree.name.StartsWith("T_"))
                    {
                        flag2 = true;
                        blendTree2 = ReflectServer(blendTree);
                    }

                    ChildMotion[] children = blendTree.children;
                    for (int j = 0; j < children.Length; j++)
                    {
                        Motion motion = children[j].motion;
                        Motion motion2 = DeleteServer(motion);
                        if (!flag2 && motion != motion2)
                        {
                            flag2 = true;
                            blendTree2 = ReflectServer(blendTree);
                        }

                        children[j].motion = motion2;
                    }

                    blendTree2.children = children;
                    EditorUtility.SetDirty(blendTree2);
                    return blendTree2;
                }

                return value;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass378_1
        {
            public UnityEditor.Animations.AnimatorControllerLayer _StubReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass379_0
        {
            public bool _ReaderReg;
            public string m_BridgeReg;
            public string m_StrategyReg;
            internal bool PushServer(string s)
            {
                if (_ReaderReg)
                {
                    if (s == m_BridgeReg)
                    {
                        return true;
                    }
                }

                if (_ReaderReg)
                {
                    return false;
                }

                return s.Contains(m_BridgeReg);
            }

            internal string ViewServer(string s)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (PushServer(s))
                    {
                        return s.Replace(m_BridgeReg, m_StrategyReg);
                    }
                }

                return s;
            }

            internal void CollectServer(StateMachineBehaviour[] behaviours)
            {
                int i = 0;
                for (; i < behaviours.Length; i++)
                {
                    StateMachineBehaviour stateMachineBehaviour = behaviours[i];
                    if (!(stateMachineBehaviour.GetType() == SerializedPropertyGroup.InsertPage()))
                    {
                        continue;
                    }

                    SerializedPropertyGroup.BridgeProperty bridgeProperty = new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour);
                    for (int num = bridgeProperty.m_DatabaseProperty.Count - 1; num >= 0; num--)
                    {
                        if (PushServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()))
                        {
                            bridgeProperty.m_DatabaseProperty[num].FlushPage(ViewServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()));
                        }

                        if (PushServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()))
                        {
                            bridgeProperty.m_DatabaseProperty[num].TestPage(ViewServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()));
                        }
                    }

                    EditorUtility.SetDirty(stateMachineBehaviour);
                }
            }

            internal void ResolveServer(Motion motion)
            {
                UnityEditor.Animations.BlendTree blendTree = motion as UnityEditor.Animations.BlendTree;
                if ((object)blendTree == null)
                {
                    return;
                }

                blendTree.blendParameter = ViewServer(blendTree.blendParameter);
                blendTree.blendParameterY = ViewServer(blendTree.blendParameterY);
                EditorUtility.SetDirty(blendTree);
                IEnumerator<Motion> enumerator = blendTree.children.Select(_003C_003Ec.watcherInitializer.ResolveProcessor).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        Motion current = enumerator.Current;
                        ResolveServer(current);
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
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass380_0
        {
            public bool customerReg;
            public string _DatabaseReg;
            public string m_ExporterReg;
            internal bool FillServer(string s)
            {
                if (customerReg)
                {
                    if (s == _DatabaseReg)
                    {
                        return true;
                    }
                }

                if (!customerReg)
                {
                    return s.Contains(_DatabaseReg);
                }

                return false;
            }

            internal string WriteServer(string s)
            {
                while (!string.IsNullOrEmpty(s))
                {
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return s.Replace(_DatabaseReg, m_ExporterReg);
                            case 3:
                                break;
                            case 1:
                                if (FillServer(s))
                                {
                                    num = 0;
                                    continue;
                                }

                                goto end_IL_006e;
                            case 2:
                                goto case 1;
                        }

                        break;
                    }

                    continue;
                    end_IL_006e:
                        break;
                }

                return s;
            }

            internal void ForgotServer(StateMachineBehaviour[] behaviours)
            {
                if (behaviours == null)
                {
                    return;
                }

                foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
                {
                    if (!(stateMachineBehaviour.GetType() == SerializedPropertyGroup.InsertPage()))
                    {
                        continue;
                    }

                    SerializedPropertyGroup.BridgeProperty bridgeProperty = new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour);
                    for (int num = bridgeProperty.m_DatabaseProperty.Count - 1; num >= 0; num--)
                    {
                        if (FillServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()))
                        {
                            bridgeProperty.m_DatabaseProperty[num].FlushPage(WriteServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()));
                        }

                        if (FillServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()))
                        {
                            bridgeProperty.m_DatabaseProperty[num].TestPage(WriteServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()));
                        }
                    }

                    EditorUtility.SetDirty(stateMachineBehaviour);
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass381_0
        {
            public string _IdentifierReg;
            public Action<StateMachineBehaviour[]> _AttrReg;
            public AnimatorStateMachine m_DispatcherReg;
            internal bool CheckServer(string s)
            {
                if (structAnnotation)
                {
                    if (s == _IdentifierReg)
                    {
                        return true;
                    }
                }

                if (!structAnnotation)
                {
                    return s.Contains(_IdentifierReg);
                }

                return false;
            }

            internal void PrepareServer<T>(T[] transitions, Action<T> removeTransitionAction)
                where T : AnimatorTransitionBase
            {
                if (transitions == null)
                {
                    return;
                }

                for (int num = transitions.Length - 1; num >= 0; num--)
                {
                    T val = transitions[num];
                    for (int num2 = val.conditions.Length - 1; num2 >= 0; num2--)
                    {
                        AnimatorCondition condition = val.conditions[num2];
                        if (CheckServer(condition.parameter))
                        {
                            val.RemoveCondition(condition);
                            if (val.conditions.Length == 0)
                            {
                                removeTransitionAction(val);
                            }
                        }
                    }
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass382_0
        {
            public string _RegistryReg;
            internal string RegisterServer(string s)
            {
                while (!string.IsNullOrEmpty(s))
                {
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            case 5:
                                if (ControllerEditorUtility.m_AdapterProcessor.Contains(s))
                                {
                                    num = 0;
                                    continue;
                                }

                                goto case 1;
                            case 3:
                                break;
                            case 1:
                                return s + _RegistryReg;
                            default:
                                goto end_IL_0079;
                            case 2:
                                goto case 5;
                        }

                        break;
                    }

                    continue;
                    end_IL_0079:
                        break;
                }

                return s;
            }

            internal void PatchServer(AnimatorStateMachine m)
            {
                m.name += _RegistryReg;
                StateMachineBehaviour[] behaviours = m.behaviours;
                int i = 0;
                for (; i < behaviours.Length; i++)
                {
                    StateMachineBehaviour stateMachineBehaviour = behaviours[i];
                    if (!(stateMachineBehaviour.GetType() == SerializedPropertyGroup.InsertPage()))
                    {
                        continue;
                    }

                    foreach (SerializedPropertyGroup.BridgeProperty.AttrProperty item in new SerializedPropertyGroup.BridgeProperty(stateMachineBehaviour).m_DatabaseProperty)
                    {
                        item.FlushPage(RegisterServer(item.ResetPage()));
                    }

                    EditorUtility.SetDirty(stateMachineBehaviour);
                }

                for (int j = 0; j < m.entryTransitions.Length; j++)
                {
                    AnimatorTransition animatorTransition = m.entryTransitions[j];
                    AnimatorCondition[] conditions = animatorTransition.conditions;
                    for (int k = 0; k < animatorTransition.conditions.Length; k++)
                    {
                        conditions[k].parameter = RegisterServer(conditions[k].parameter);
                    }

                    animatorTransition.conditions = conditions;
                    EditorUtility.SetDirty(animatorTransition);
                }

                ChildAnimatorStateMachine[] stateMachines = m.stateMachines;
                for (i = 0; i < stateMachines.Length; i++)
                {
                    ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
                    if (childAnimatorStateMachine.stateMachine != m)
                    {
                        PatchServer(childAnimatorStateMachine.stateMachine);
                    }
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass383_0
        {
            public UnityEngine.AnimatorControllerParameter tagReg;
        }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        internal struct _003C_003Ec__DisplayClass383_1
        {
            public bool m_RequestReg;
            public string m_PrinterReg;
        }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        internal struct _003C_003Ec__DisplayClass383_2
        {
            public UnityEditor.Animations.AnimatorControllerLayer _WriterReg;
        }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        internal struct _003C_003Ec__DisplayClass383_3
        {
            public AnimatorState _ParamsReg;
        }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        internal struct _003C_003Ec__DisplayClass384_0
        {
            public string listenerReg;
            public UnityEngine.AnimatorControllerParameterType m_GetterReg;
            public UnityEngine.AnimatorControllerParameterType m_InterceptorReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass385_0
        {
            public string _CreatorReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass385_1
        {
            public int infoReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass385_2
        {
            public int advisorReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass389_0
        {
            public Dictionary<UnityEngine.Object, Vector3> m_CallbackReg;
            public Vector3 indexerReg;
            public int m_IssuerReg;
            public MethodInfo _PrototypeReg;
            public AnimatorStateMachine m_RuleReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass390_0
        {
            public AnimatorStateMachine _SingletonReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass410_0
        {
            public List<AnimatorStateTransition> _FactoryReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass410_1
        {
            public AnimatorStateTransition _AccountReg;
            internal bool SetupThread(ChildAnimatorState c)
            {
                return _AccountReg.destinationState == c.state;
            }

            internal bool EnableThread(ChildAnimatorStateMachine c)
            {
                return _AccountReg.destinationStateMachine == c.stateMachine;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass412_0
        {
            public List<AnimatorStateTransition> refReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass413_0
        {
            public List<AnimatorTransitionBase> tokenReg;
            public List<AnimatorStateTransition> codeReg;
            public List<AnimatorTransitionBase> m_DicReg;
            internal void PopThread(List<AnimatorStateTransition> t, AnimatorState s)
            {
                _003C_003Ec__DisplayClass413_1 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_1();
                if (t.Count != 1)
                {
                    _003C_003Ec__DisplayClass413_._ParamReg = CalculateAlgo(t[0]);
                    s.RemoveTransition(t[0]);
                    for (int i = 1; i < t.Count; i++)
                    {
                        t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.CancelThread);
                        s.RemoveTransition(t[i]);
                    }

                    s.AddTransition(_003C_003Ec__DisplayClass413_._ParamReg);
                    tokenReg.Add(_003C_003Ec__DisplayClass413_._ParamReg);
                }
                else
                {
                    tokenReg.Add(t[0]);
                }
            }

            internal void ComputeThread(List<AnimatorTransitionBase> t)
            {
                _003C_003Ec__DisplayClass413_2 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_2{_TokenizerReg = RevertMapper().AddEntryTransition(t[0].destinationState)};
                EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._TokenizerReg);
                RevertMapper().RemoveEntryTransition((AnimatorTransition)t[0]);
                for (int i = 1; i < t.Count; i++)
                {
                    t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.CountThread);
                    RevertMapper().RemoveEntryTransition((AnimatorTransition)t[i]);
                }

                tokenReg.Add(_003C_003Ec__DisplayClass413_._TokenizerReg);
            }

            internal void MoveThread(List<AnimatorStateTransition> t)
            {
                _003C_003Ec__DisplayClass413_3 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_3();
                _003C_003Ec__DisplayClass413_._ComparatorReg = RevertMapper().AddAnyStateTransition(t[0].destinationState);
                EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._ComparatorReg);
                RevertMapper().RemoveAnyStateTransition(t[0]);
                for (int i = 1; i < t.Count; i++)
                {
                    t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.DisableThread);
                    RevertMapper().RemoveAnyStateTransition(t[i]);
                }

                tokenReg.Add(_003C_003Ec__DisplayClass413_._ComparatorReg);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass413_1
        {
            public AnimatorStateTransition _ParamReg;
            internal void CancelThread(AnimatorCondition c)
            {
                _ParamReg.AddCondition(c.mode, c.threshold, c.parameter);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass413_2
        {
            public AnimatorTransitionBase _TokenizerReg;
            internal void CountThread(AnimatorCondition c)
            {
                _TokenizerReg.AddCondition(c.mode, c.threshold, c.parameter);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass413_3
        {
            public AnimatorStateTransition _ComparatorReg;
            internal void DisableThread(AnimatorCondition c)
            {
                _ComparatorReg.AddCondition(c.mode, c.threshold, c.parameter);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass413_4
        {
            public List<AnimatorStateTransition> objectReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass420_0
        {
            public List<AnimatorStateTransition> valReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass420_1
        {
            public AnimatorStateTransition _ValueReg;
            internal bool QueryThread(ChildAnimatorState s)
            {
                return s.state == _ValueReg.destinationState;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass430_0
        {
            public UnityAnimatorBridge.InstanceTests m_MerchantReg;
            internal void AddThread(AnimatorState s)
            {
                TestAlgo(m_MerchantReg.itemTests, s.AddTransition((AnimatorState)null));
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass431_0
        {
            public HashSet<UnityEngine.Object> authenticationReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass431_1
        {
            public AnimatorStateTransition _ReponseReg;
            public _003C_003Ec__DisplayClass431_0 m_PoolReg;
            internal bool FindThread(AnimatorStateTransition t)
            {
                return t == _ReponseReg;
            }

            internal void ExcludeThread(ChildAnimatorState c)
            {
                if (!(c.state != _ReponseReg.destinationState))
                {
                    if (!(c.state == _ReponseReg.destinationState) || !_ReponseReg.canTransitionToSelf)
                    {
                        return;
                    }
                }

                AnimatorStateTransition animatorStateTransition = CalculateAlgo(_ReponseReg);
                m_PoolReg.authenticationReg.Add(animatorStateTransition);
                c.state.AddTransition(animatorStateTransition);
            }

            internal bool InitThread(AnimatorStateTransition t)
            {
                return t == _ReponseReg;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass432_0
        {
            public List<AnimatorTransitionBase> _ComposerReg;
            internal void VisitThread(AnimatorTransitionBase t)
            {
                _003C_003Ec__DisplayClass432_1 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_1();
                _003C_003Ec__DisplayClass432_.mappingReg = this;
                _003C_003Ec__DisplayClass432_.m_RepositoryReg = t;
                if (_003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.Length != 0)
                {
                    _003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.ReadThread);
                    RevertMapper().RemoveEntryTransition((AnimatorTransition)_003C_003Ec__DisplayClass432_.m_RepositoryReg);
                }
            }

            internal void DefineThread(AnimatorStateTransition t)
            {
                _003C_003Ec__DisplayClass432_2 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_2();
                _003C_003Ec__DisplayClass432_.containerReg = this;
                _003C_003Ec__DisplayClass432_.baseReg = t;
                if (_003C_003Ec__DisplayClass432_.baseReg.conditions.Length != 0)
                {
                    _003C_003Ec__DisplayClass432_.baseReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.SelectThread);
                    RevertMapper().RemoveAnyStateTransition(_003C_003Ec__DisplayClass432_.baseReg);
                }
            }

            internal void StartThread(AnimatorStateTransition t, AnimatorState s)
            {
                while (true)
                {
                    _003C_003Ec__DisplayClass432_3 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_3();
                    int num = 7;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                return;
                            case 1:
                                _003C_003Ec__DisplayClass432_.mockReg.RemoveTransition(_003C_003Ec__DisplayClass432_.m_ClassReg);
                                return;
                            case 5:
                                if (_003C_003Ec__DisplayClass432_.m_ClassReg.conditions.Length == 0)
                                {
                                    return;
                                }

                                num = 6;
                                continue;
                            default:
                                _003C_003Ec__DisplayClass432_.m_ClassReg = t;
                                num = 4;
                                continue;
                            case 8:
                                break;
                            case 7:
                                _003C_003Ec__DisplayClass432_.instanceReg = this;
                                num = 0;
                                continue;
                            case 4:
                                _003C_003Ec__DisplayClass432_.mockReg = s;
                                num = 5;
                                continue;
                            case 3:
                                _003C_003Ec__DisplayClass432_.m_ClassReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.RemoveThread);
                                num = 1;
                                continue;
                            case 6:
                                goto case 3;
                        }

                        break;
                    }
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass432_1
        {
            public AnimatorTransitionBase m_RepositoryReg;
            public _003C_003Ec__DisplayClass432_0 mappingReg;
            internal void ReadThread(AnimatorCondition c)
            {
                while (true)
                {
                    AnimatorTransitionBase animatorTransitionBase = RevertMapper().AddEntryTransition(m_RepositoryReg.destinationState);
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                return;
                            case 1:
                                break;
                            case 5:
                                animatorTransitionBase.conditions = new AnimatorCondition[1]{c};
                                num = 3;
                                continue;
                            case 3:
                            case 6:
                                mappingReg._ComposerReg.Add(animatorTransitionBase);
                                num = 4;
                                continue;
                            default:
                                EditorUtility.CopySerializedManagedFieldsOnly(m_RepositoryReg, animatorTransitionBase);
                                num = 2;
                                continue;
                            case 2:
                                goto case 5;
                        }

                        break;
                    }
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass432_2
        {
            public AnimatorStateTransition baseReg;
            public _003C_003Ec__DisplayClass432_0 containerReg;
            internal void SelectThread(AnimatorCondition c)
            {
                while (true)
                {
                    AnimatorTransitionBase animatorTransitionBase = RevertMapper().AddAnyStateTransition(baseReg.destinationState);
                    int num = 4;
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                            case 6:
                                animatorTransitionBase.conditions = new AnimatorCondition[1]{c};
                                goto IL_00fd;
                            case 5:
                                break;
                            default:
                                EditorUtility.CopySerialized(baseReg, animatorTransitionBase);
                                goto case 1;
                            case 3:
                                return;
                            case 2:
                                goto IL_00fd;
                            case 4:
                                goto default;
                        }

                        break;
                        IL_00fd:
                            containerReg._ComposerReg.Add(animatorTransitionBase);
                        num = 3;
                    }
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass432_3
        {
            public AnimatorStateTransition m_ClassReg;
            public AnimatorState mockReg;
            public _003C_003Ec__DisplayClass432_0 instanceReg;
            internal void RemoveThread(AnimatorCondition c)
            {
                AnimatorStateTransition animatorStateTransition = CalculateAlgo(m_ClassReg);
                animatorStateTransition.conditions = new AnimatorCondition[1]{c};
                mockReg.AddTransition(animatorStateTransition);
                instanceReg._ComposerReg.Add(animatorStateTransition);
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass481_0
        {
            public GameObject fieldReg;
            public EditorCurveBinding? _AttributeReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass482_0
        {
            public AnimationClip clientReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass483_0
        {
            public EditorCurveBinding configReg;
            public EditorCurveBinding[] descriptorReg;
            public Rect _TemplateReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass483_1
        {
            public EditorCurveBinding parserReg;
            internal bool MapThread(string p)
            {
                return p == parserReg.propertyName;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass485_0
        {
            public AnimationClip m_ManagerReg;
            public GameObject m_ItemReg;
            public Transform _SpecificationReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass486_0
        {
            public EditorCurveBinding m_MethodReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass581_0
        {
            public LayerViewViewType schemaReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass589_0
        {
            public string m_BroadcasterReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass590_0
        {
            public Event m_ProxyReg;
            public UnityEditor.Animations.AnimatorControllerLayer structReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass590_1
        {
            public string m_StateReg;
            public bool _GlobalReg;
            public _003C_003Ec__DisplayClass590_0 m_TaskReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass59_0
        {
            public AnimatorTransitionBase processReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass611_0
        {
            public bool _ProducerReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass616_0
        {
            public object m_IteratorReg;
            public string _PublisherReg;
            internal T ReflectThread<T>(string methodName)
                where T : Delegate
            {
                return (T)Delegate.CreateDelegate(typeof(T), m_IteratorReg, _ReaderVisitor.DisableList(methodName));
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass630_0
        {
            public IEnumerable<AnimatorState> configurationReg;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass668_0
        {
            public string m_ProcReg;
            public bool m_WrapperTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass670_0
        {
            public Node m_AnnotationTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass670_1
        {
            public UnityEditor.Animations.BlendTree visitorTests;
            internal ChildMotion CollectThread(Motion m)
            {
                ChildMotion result = default(ChildMotion);
                result.motion = m;
                result.timeScale = 1f;
                result.threshold = 0f;
                result.directBlendParameter = visitorTests.blendParameter;
                return result;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass672_0
        {
            public UnityEditor.Animations.BlendTree algoTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass690_0
        {
            public AnimatorState m_MapperTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass692_0
        {
            public Transform _InitializerTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass693_0
        {
            public AnimatorState definitionTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass702_0
        {
            public UnityEngine.AnimatorControllerParameter m_RegTests;
            public int m_TestsTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass702_1
        {
            public AnimationClip m_PropertyTests;
            public _003C_003Ec__DisplayClass702_0 processorTests;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass73_0
        {
            public GUIStyle[] m_ObserverTests;
            public Texture2D _ServerTests;
            internal void AssetThread(string name, Func<GUIStyle, GUIStyle> func, bool isCosmeticOnlyStyle = false, bool hiddenFromList = false)
            {
                int i = 0;
                for (; i < 4; i++)
                {
                    bool validatesecond = i > 1;
                    bool isrule = i % 2 == 1;
                    GUIStyle arg = m_ObserverTests[i];
                    GUIStyle gUIStyle = func(arg);
                    EnableAnnotation(name, gUIStyle, isrule, validatesecond);
                    if (i == 0)
                    {
                        m_PolicyAnnotation.Add(name, gUIStyle);
                    }

                    if (isCosmeticOnlyStyle)
                    {
                        m_SerializerAnnotation.Add(name);
                    }
                }

                if (!hiddenFromList)
                {
                    _PageAnnotation.Add(name.Substring(3));
                }
            }
        }

        [CompilerGenerated]
        internal sealed class _003CGetLayerCategories_003Ed__624 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;
            private string _003C_003E2__current;
            private int _003C_003El__initialThreadId;
            private UnityEditor.Animations.AnimatorControllerLayer l;
            public UnityEditor.Animations.AnimatorControllerLayer _003C_003E3__l;
            private IEnumerator<string> _003C_003E7__wrap1;
            string IEnumerator<string>.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            [DebuggerHidden]
            public _003CGetLayerCategories_003Ed__624(int _003C_003E1__state)
            {
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 1:
                            _003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
                            num = 0;
                            break;
                        case 2:
                            this._003C_003E1__state = _003C_003E1__state;
                            num = 1;
                            break;
                        case 0:
                            return;
                    }
                }
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = _003C_003E1__state;
                if (num == -3 || num == 1)
                {
                    try
                    {
                    }
                    finally
                    {
                        _003C_003Em__Finally1();
                    }
                }
            }

            private bool MoveNext()
            {
                bool result;
                try
                {
                    int num = _003C_003E1__state;
                    if (num == 0)
                    {
                        _003C_003E1__state = -1;
                        IEnumerable<string> enumerable = l.InstantiatePredicate();
                        _003C_003E7__wrap1 = enumerable.GetEnumerator();
                        _003C_003E1__state = -3;
                        goto IL_01a6;
                    }

                    if (num == 1)
                    {
                        _003C_003E1__state = -3;
                        goto IL_01a6;
                    }

                    result = false;
                    goto end_IL_0000;
                    IL_01a6:
                        while (true)
                        {
                            if (!_003C_003E7__wrap1.MoveNext())
                            {
                                _003C_003Em__Finally1();
                                _003C_003E7__wrap1 = null;
                                result = false;
                                break;
                            }

                            Match match = Regex.Match(_003C_003E7__wrap1.Current, "^_category:(.+)$");
                            if (!match.Success)
                            {
                                continue;
                            }

                            string value = match.Groups[1].Value;
                            _003C_003E2__current = value;
                            _003C_003E1__state = 1;
                            result = true;
                            break;
                        }

                    end_IL_0000:
                        ;
                }
                catch
                {
                    //try-fault
                    ((IDisposable)this).Dispose();
                    throw;
                }

                return result;
            }

            bool IEnumerator.MoveNext()
            {
                return this.MoveNext();
            }

            private void _003C_003Em__Finally1()
            {
                while (true)
                {
                    _003C_003E1__state = -1;
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                return;
                            case 1:
                                if (_003C_003E7__wrap1 == null)
                                {
                                    goto IL_006d;
                                }

                                goto case 6;
                            case 3:
                                return;
                            default:
                                _003C_003E7__wrap1.Dispose();
                                return;
                            case 5:
                                return;
                            case 2:
                                break;
                            case 6:
                                goto default;
                        }

                        break;
                        IL_006d:
                            num = 5;
                    }
                }
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                _003CGetLayerCategories_003Ed__624 _003CGetLayerCategories_003Ed__;
                if (_003C_003E1__state == -2)
                {
                    if (_003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
                    {
                        _003C_003E1__state = 0;
                        _003CGetLayerCategories_003Ed__ = this;
                        goto IL_00c8;
                    }
                }

                _003CGetLayerCategories_003Ed__ = new _003CGetLayerCategories_003Ed__624(0);
                goto IL_00c8;
                IL_00c8:
                    _003CGetLayerCategories_003Ed__.l = _003C_003E3__l;
                return _003CGetLayerCategories_003Ed__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<string>)this).GetEnumerator();
            }
        }
    }
}
