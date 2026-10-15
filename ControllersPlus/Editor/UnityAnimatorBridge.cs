using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class UnityAnimatorBridge
    {
        internal static class ConnectionTests
        {
            public static readonly ReflectedType contextTests;
            public static readonly ReflectedType recordTests;
            public static readonly ReflectedType helperTests;
            public static readonly ReflectedType consumerTests;
            public static readonly ReflectedType _AdapterTests;
            public static readonly ReflectedType interpreterTests;
            public static readonly ReflectedType _WatcherTests;
            public static readonly ReflectedType _CandidateTests;
            public static readonly ReflectedType m_ProductTests;
            public static readonly ReflectedType m_ExpressionTests;
            public static readonly ReflectedType _SystemTests;
            public static readonly ReflectedType _WorkerTests;
            public static readonly ReflectedType m_FilterTests;
            public static readonly ReflectedType m_StubTests;
            public static readonly ReflectedType readerTests;
            static ConnectionTests()
            {
                while (true)
                {
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            case 3:
                                break;
                            case 15:
                                m_ProductTests = new ReflectedType("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 18;
                                continue;
                            case 5:
                                interpreterTests = new ReflectedType("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 4;
                                continue;
                            case 13:
                                _CandidateTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.StateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 15;
                                continue;
                            case 9:
                                _AdapterTests = new ReflectedType("UnityEditor.Graphs.AnimatorControllerTool+BreadCrumbElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 5;
                                continue;
                            case 4:
                                _WatcherTests = new ReflectedType("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 13;
                                continue;
                            case 1:
                                return;
                            case 18:
                                m_ExpressionTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.StateMachineNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 6;
                                continue;
                            case 2:
                                contextTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 16;
                                continue;
                            case 16:
                                recordTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 0;
                                continue;
                            default:
                                helperTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 10;
                                continue;
                            case 8:
                            case 12:
                                m_FilterTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.ExitNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 14;
                                continue;
                            case 10:
                                consumerTests = new ReflectedType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 9;
                                continue;
                            case 6:
                                _SystemTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.EntryNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                goto case 7;
                            case 7:
                                _WorkerTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.AnyStateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 12;
                                continue;
                            case 11:
                            case 14:
                                m_StubTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.EdgeInfo, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 17;
                                continue;
                            case 17:
                                readerTests = new ReflectedType("UnityEditor.Graphs.AnimationStateMachine.TransitionEditionContext, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                                num = 1;
                                continue;
                        }

                        break;
                    }
                }
            }
        }

        internal static class BridgeTests
        {
            public static readonly ReflectedMember<FieldInfo> _StrategyTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "tool");
            public static readonly ReflectedMember<FieldInfo> customerTests;
            public static readonly ReflectedMember<FieldInfo> m_DatabaseTests;
            public static readonly ReflectedMember<FieldInfo> m_ExporterTests;
            public static readonly ReflectedMember<FieldInfo> m_IdentifierTests;
            public static readonly ReflectedMember<FieldInfo> attrTests;
            public static readonly ReflectedMember<FieldInfo> m_DispatcherTests;
            public static readonly ReflectedMember<FieldInfo> _RegistryTests;
            public static readonly ReflectedMember<FieldInfo> m_TagTests;
            public static readonly ReflectedMember<FieldInfo> _ImporterTests;
            public static readonly ReflectedMember<FieldInfo> m_RequestTests;
            public static readonly ReflectedMember<FieldInfo> printerTests;
            public static readonly ReflectedMember<FieldInfo> m_WriterTests;
            public static readonly ReflectedMember<FieldInfo> _ParamsTests;
            public static readonly ReflectedMember<FieldInfo> m_ListenerTests;
            public static readonly ReflectedMember<FieldInfo> getterTests;
            public static readonly ReflectedMember<FieldInfo> m_InterceptorTests;
            public static readonly ReflectedMember<FieldInfo> _CreatorTests;
            public static readonly ReflectedMember<FieldInfo> eventTests;
            public static readonly ReflectedMember<FieldInfo> m_InfoTests;
            public static readonly ReflectedMember<FieldInfo> m_FacadeTests;
            public static readonly ReflectedMember<FieldInfo> advisorTests;
            public static readonly ReflectedMember<FieldInfo> callbackTests;
            public static readonly ReflectedMember<MethodInfo> m_IndexerTests;
            public static readonly ReflectedMember<MethodInfo> _IssuerTests;
            public static readonly ReflectedMember<MethodInfo> _PrototypeTests;
            public static readonly ReflectedMember<MethodInfo> m_RuleTests;
            public static readonly ReflectedMember<PropertyInfo> _SingletonTests;
            public static readonly ReflectedMember<PropertyInfo> factoryTests;
            public static readonly ReflectedMember<PropertyInfo> m_AccountTests;
            static BridgeTests()
            {
                customerTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "stateMachineGraph");
                m_DatabaseTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "blendTreeGraph");
                m_ExporterTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "stateMachineGraphGUI");
                m_IdentifierTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "blendTreeGraphGUI");
                attrTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "m_BreadCrumbs");
                m_DispatcherTests = new ReflectedMember<FieldInfo>(ConnectionTests.consumerTests, "graphDirtyCallback");
                _RegistryTests = new ReflectedMember<FieldInfo>(ConnectionTests._AdapterTests, "m_Target");
                m_TagTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "m_ActiveStateMachine");
                _ImporterTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "rootStateMachine");
                m_RequestTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "parentStateMachine");
                printerTests = new ReflectedMember<FieldInfo>(ConnectionTests._WatcherTests, "selection");
                m_WriterTests = new ReflectedMember<FieldInfo>(ConnectionTests.interpreterTests, "nodes");
                _ParamsTests = new ReflectedMember<FieldInfo>(ConnectionTests.interpreterTests, "edges");
                m_ListenerTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "m_EntryNode");
                getterTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "m_ExitNode");
                m_InterceptorTests = new ReflectedMember<FieldInfo>(ConnectionTests.contextTests, "m_AnyStateNode");
                _CreatorTests = new ReflectedMember<FieldInfo>(ConnectionTests._CandidateTests, "state");
                eventTests = new ReflectedMember<FieldInfo>(ConnectionTests._SystemTests, "m_StateMachine");
                m_InfoTests = new ReflectedMember<FieldInfo>(ConnectionTests.m_ExpressionTests, "stateMachine");
                m_FacadeTests = new ReflectedMember<FieldInfo>(ConnectionTests._CandidateTests, "m_StateMachineProxyLookup");
                advisorTests = new ReflectedMember<FieldInfo>(ConnectionTests._CandidateTests, "m_StateMachineLookup");
                callbackTests = new ReflectedMember<FieldInfo>(ConnectionTests.m_StubTests, "transitions");
                m_IndexerTests = new ReflectedMember<MethodInfo>(ConnectionTests.contextTests, "GetEdgeInfo");
                _IssuerTests = new ReflectedMember<MethodInfo>(ConnectionTests.contextTests, "FindNode", typeof(AnimatorState));
                _PrototypeTests = new ReflectedMember<MethodInfo>(ConnectionTests.contextTests, "FindNode", typeof(AnimatorStateMachine));
                m_RuleTests = new ReflectedMember<MethodInfo>(ConnectionTests.m_StubTests, "HasTransition", typeof(AnimatorTransitionBase));
                _SingletonTests = new ReflectedMember<PropertyInfo>(ConnectionTests.consumerTests, "animatorController");
                factoryTests = new ReflectedMember<PropertyInfo>(ConnectionTests.recordTests, "edgeGUI");
                m_AccountTests = new ReflectedMember<PropertyInfo>(ConnectionTests.m_StubTests, "hasMultipleTransitions");
            }
        }

        internal static class RefTests
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass39_0
            {
                public HashSet<int> roleTests;
                public ConcurrentBag<BaseTests> _ParamTests;
                public BaseTests[] _ModelTests;
            }

            private static EditorWindow _StatusTests;
            [SpecialName]
            public static EditorWindow PopPolicy()
            {
                if (_StatusTests != null)
                {
                    return _StatusTests;
                }

                return _StatusTests = (EditorWindow)BridgeTests._StrategyTests.PrepareRecord().GetValue(null);
            }

            [SpecialName]
            public static AnimatorController ConcatPolicy()
            {
                if (!(PopPolicy() == null))
                {
                    return (AnimatorController)BridgeTests._SingletonTests.PrepareRecord().GetValue(PopPolicy());
                }

                return null;
            }

            [SpecialName]
            public static Graph CancelPolicy()
            {
                if (PopPolicy() == null)
                {
                    return null;
                }

                return (Graph)BridgeTests.customerTests.PrepareRecord().GetValue(PopPolicy());
            }

            [SpecialName]
            public static Graph DisablePolicy()
            {
                if (!(PopPolicy() == null))
                {
                    return (Graph)BridgeTests.m_DatabaseTests.PrepareRecord().GetValue(PopPolicy());
                }

                return null;
            }

            [SpecialName]
            public static GraphGUI RestartPolicy()
            {
                if (PopPolicy() == null)
                {
                    return null;
                }

                return (GraphGUI)BridgeTests.m_ExporterTests.PrepareRecord().GetValue(PopPolicy());
            }

            [SpecialName]
            public static GraphGUI AddPolicy()
            {
                if (PopPolicy() == null)
                {
                    return null;
                }

                return (GraphGUI)BridgeTests.m_IdentifierTests.PrepareRecord().GetValue(PopPolicy());
            }

            [SpecialName]
            public static Graph FindPolicy()
            {
                if (!(PopPolicy() == null))
                {
                    if (CreatePolicy())
                    {
                        return DisablePolicy();
                    }

                    return CancelPolicy();
                }

                return null;
            }

            [SpecialName]
            public static GraphGUI InitPolicy()
            {
                if (!(PopPolicy() == null))
                {
                    if (CreatePolicy())
                    {
                        return AddPolicy();
                    }

                    return RestartPolicy();
                }

                return null;
            }

            [SpecialName]
            public static IEdgeGUI DefinePolicy()
            {
                if (RestartPolicy() == null)
                {
                    return null;
                }

                return (IEdgeGUI)BridgeTests.factoryTests.PrepareRecord().GetValue(RestartPolicy());
            }

            [SpecialName]
            public static AnimatorStateMachine ReadPolicy()
            {
                if (CancelPolicy() == null)
                {
                    return null;
                }

                return (AnimatorStateMachine)BridgeTests.m_TagTests.PrepareRecord().GetValue(CancelPolicy());
            }

            [SpecialName]
            public static AnimatorStateMachine RemovePolicy()
            {
                if (CancelPolicy() == null)
                {
                    return null;
                }

                return (AnimatorStateMachine)BridgeTests._ImporterTests.PrepareRecord().GetValue(CancelPolicy());
            }

            [SpecialName]
            public static TokenizerTests FlushPolicy()
            {
                if (CancelPolicy() == null)
                {
                    return null;
                }

                return new TokenizerTests((Node)BridgeTests.m_ListenerTests.PrepareRecord().GetValue(CancelPolicy()));
            }

            [SpecialName]
            public static TokenizerTests CalculatePolicy()
            {
                if (CancelPolicy() == null)
                {
                    return null;
                }

                return new TokenizerTests((Node)BridgeTests.getterTests.PrepareRecord().GetValue(CancelPolicy()));
            }

            [SpecialName]
            public static TokenizerTests MapPolicy()
            {
                if (CancelPolicy() == null)
                {
                    return null;
                }

                return new TokenizerTests((Node)BridgeTests.m_InterceptorTests.PrepareRecord().GetValue(CancelPolicy()));
            }

            [SpecialName]
            public static IEnumerable<BaseTests> DestroyPolicy()
            {
                if (!(FindPolicy() == null))
                {
                    return
                        from Edge e in (IList)BridgeTests._ParamsTests.PrepareRecord().GetValue(FindPolicy())select new BaseTests(e);
                }

                return null;
            }

            [SpecialName]
            public static IEnumerable<TokenizerTests> CalcPolicy()
            {
                if (!(InitPolicy() == null))
                {
                    return
                        from Node n in (IList)BridgeTests.printerTests.PrepareRecord().GetValue(InitPolicy())select new TokenizerTests(n);
                }

                return null;
            }

            [SpecialName]
            public static ConcurrentBag<BaseTests> RunPolicy()
            {
                _003C_003Ec__DisplayClass39_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass39_0();
                if (!(CancelPolicy() == null) && DefinePolicy() != null)
                {
                    CS_0024_003C_003E8__locals9._ModelTests = DestroyPolicy().ToArray();
                    CS_0024_003C_003E8__locals9.roleTests = new HashSet<int>();
                    foreach (int item in DefinePolicy().edgeSelection)
                    {
                        CS_0024_003C_003E8__locals9.roleTests.Add(item);
                    }

                    CS_0024_003C_003E8__locals9._ParamTests = new ConcurrentBag<BaseTests>();
                    Parallel.For(0, CS_0024_003C_003E8__locals9._ModelTests.Length, delegate (int i)
                    {
                        while (CS_0024_003C_003E8__locals9.roleTests.Contains(i))
                        {
                            int num = 2;
                            while (true)
                            {
                                switch (num)
                                {
                                    default:
                                        return;
                                    case 1:
                                        return;
                                    case 3:
                                        break;
                                    case 4:
                                        goto IL_0066;
                                    case 2:
                                        goto IL_0066;
                                }

                                break;
                                IL_0066:
                                    CS_0024_003C_003E8__locals9._ParamTests.Add(CS_0024_003C_003E8__locals9._ModelTests[i]);
                                num = 0;
                            }
                        }
                    });
                    return CS_0024_003C_003E8__locals9._ParamTests;
                }

                return null;
            }

            [SpecialName]
            public static bool CreatePolicy()
            {
                IList list = SetupPolicy();
                if (list.Count == 0)
                {
                    return false;
                }

                return !(BridgeTests._RegistryTests.PrepareRecord().GetValue(list[list.Count - 1]) is AnimatorStateMachine);
            }

            public static IList SetupPolicy()
            {
                return (IList)BridgeTests.attrTests.PrepareRecord().GetValue(PopPolicy());
            }
        }

        internal class TokenizerTests
        {
            internal enum NodeType
            {
                unknown,
                state,
                machine,
                tree,
                entry,
                exit,
                any
            }

            public bool _DecoratorTests;
            public readonly AnimatorState comparatorTests;
            public readonly AnimatorStateMachine m_ExceptionTests;
            public readonly NodeType objectTests;
            private Node valueTests;
            [SpecialName]
            public Node FillPolicy()
            {
                if (!_DecoratorTests)
                {
                    if (!(valueTests != null))
                    {
                        _DecoratorTests = true;
                        switch (objectTests)
                        {
                            case NodeType.any:
                                valueTests = RefTests.MapPolicy();
                                break;
                            case NodeType.exit:
                                valueTests = RefTests.CalculatePolicy();
                                break;
                            case NodeType.machine:
                                valueTests = OrderPolicy(m_ExceptionTests).FillPolicy();
                                break;
                            case NodeType.entry:
                                valueTests = RefTests.FlushPolicy();
                                break;
                            case NodeType.state:
                                valueTests = RevertThread(comparatorTests).FillPolicy();
                                break;
                        }

                        return valueTests;
                    }
                }

                return valueTests;
            }

            [SpecialName]
            public IEnumerable<BaseTests> CheckPolicy()
            {
                return FillPolicy().inputEdges.Select((Edge e) => new BaseTests(e));
            }

            [SpecialName]
            public IEnumerable<BaseTests> AssetPolicy()
            {
                return FillPolicy().outputEdges.Select((Edge e) => new BaseTests(e));
            }

            [SpecialName]
            public IEnumerable<AnimatorTransitionBase> ChangePolicy()
            {
                return CheckPolicy().SelectMany((BaseTests e) =>
                    from t in e.FindSerializer()select t.itemTests);
            }

            [SpecialName]
            public IEnumerable<AnimatorTransitionBase> RegisterPolicy()
            {
                return AssetPolicy().SelectMany((BaseTests e) =>
                    from t in e.FindSerializer()select t.itemTests);
            }

            [SpecialName]
            public void InterruptPolicy(Styles.Color ident)
            {
                FillPolicy().color = ident;
            }

            public TokenizerTests()
            {
            }

            public TokenizerTests(Node init)
            {
                _DecoratorTests = true;
                valueTests = init;
                if (!(init == null))
                {
                    Type type = init.GetType();
                    if (type == ConnectionTests._CandidateTests.ChangeRecord())
                    {
                        objectTests = NodeType.state;
                        comparatorTests = (AnimatorState)BridgeTests._CreatorTests.PrepareRecord().GetValue(init);
                    }
                    else if (type == ConnectionTests.m_ExpressionTests.ChangeRecord())
                    {
                        objectTests = NodeType.machine;
                        m_ExceptionTests = (AnimatorStateMachine)BridgeTests.m_InfoTests.PrepareRecord().GetValue(init);
                    }
                    else if (type == ConnectionTests._SystemTests.ChangeRecord())
                    {
                        objectTests = NodeType.entry;
                    }
                    else if (type == ConnectionTests.m_FilterTests.ChangeRecord())
                    {
                        objectTests = NodeType.exit;
                    }
                    else if (type == ConnectionTests._WorkerTests.ChangeRecord())
                    {
                        objectTests = NodeType.any;
                    }
                    else if (type == ConnectionTests.m_ProductTests.ChangeRecord())
                    {
                        objectTests = NodeType.tree;
                    }
                }
            }

            public TokenizerTests(AnimatorState task)
            {
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            comparatorTests = task;
                            num = 0;
                            break;
                        case 1:
                            return;
                        default:
                            objectTests = NodeType.state;
                            num = 1;
                            break;
                    }
                }
            }

            public TokenizerTests(AnimatorStateMachine item)
            {
                m_ExceptionTests = item;
                objectTests = NodeType.machine;
            }

            public static implicit operator Node(TokenizerTests reference)
            {
                return reference.FillPolicy();
            }
        }

        internal class BaseTests
        {
            public readonly Edge m_ContainerTests;
            private bool m_ClassTests;
            private object _MockTests;
            [SpecialName]
            private object AddSerializer()
            {
                while (true)
                {
                    int num;
                    if (m_ClassTests)
                    {
                        num = 0;
                        goto IL_001d;
                    }

                    goto IL_00e3;
                    IL_001d:
                        while (true)
                        {
                            switch (num)
                            {
                                case 2:
                                    return _MockTests;
                                case 4:
                                    return _MockTests;
                                case 3:
                                    _MockTests = BridgeTests.m_IndexerTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1]{m_ContainerTests});
                                    num = 2;
                                    continue;
                                case 1:
                                    break;
                                case 5:
                                    goto IL_00e3;
                                default:
                                    goto case 4;
                            }

                            break;
                        }

                    continue;
                    IL_00e3:
                        m_ClassTests = true;
                    num = 3;
                    goto IL_001d;
                }
            }

            [SpecialName]
            public IEnumerable<InstanceTests> FindSerializer()
            {
                return
                    from object setup in (IList)BridgeTests.callbackTests.PrepareRecord().GetValue(AddSerializer())select new InstanceTests(setup, this);
            }

            public BaseTests(Edge value)
            {
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            return;
                    }

                    m_ContainerTests = value;
                    num = 1;
                }
            }

            public static implicit operator Edge(BaseTests task)
            {
                return task.m_ContainerTests;
            }
        }

        internal readonly struct InstanceTests
        {
            private static readonly ReflectedMember<FieldInfo> m_FieldTests;
            private static readonly ReflectedMember<FieldInfo> attributeTests;
            private static readonly ReflectedMember<FieldInfo> m_ClientTests;
            private static readonly ReflectedMember<FieldInfo> m_ConfigTests;
            private static readonly ReflectedMember<FieldInfo> descriptorTests;
            private static readonly ReflectedMember<FieldInfo> _TemplateTests;
            private static readonly ReflectedMember<PropertyInfo> m_MessageTests;
            private static readonly ReflectedMember<PropertyInfo> m_CollectionTests;
            public readonly object parserTests;
            public readonly BaseTests _ManagerTests;
            public readonly AnimatorTransitionBase itemTests;
            public readonly AnimatorStateTransition specificationTests;
            public readonly AnimatorTransition m_MethodTests;
            public readonly AnimatorState m_SchemaTests;
            public readonly AnimatorStateMachine broadcasterTests;
            public readonly AnimatorStateMachine proxyTests;
            public readonly AnimatorState _StructTests;
            public readonly AnimatorStateMachine _ServiceTests;
            public readonly TokenizerTests.NodeType stateTests;
            public readonly TokenizerTests.NodeType globalTests;
            public readonly bool taskTests;
            public readonly bool m_ProcessTests;
            public readonly bool _ProducerTests;
            public readonly bool m_IteratorTests;
            [SpecialName]
            public string VisitSerializer()
            {
                return (string)m_FieldTests.PrepareRecord().GetValue(parserTests);
            }

            public InstanceTests(object config, BaseTests second) : this()
            {
                int num = 9;
                bool flag = default(bool);
                while (true)
                {
                    switch (num)
                    {
                        case 7:
                            proxyTests = (AnimatorStateMachine)descriptorTests.PrepareRecord().GetValue(config);
                            m_SchemaTests = (AnimatorState)m_ClientTests.PrepareRecord().GetValue(config);
                            num = 0;
                            break;
                        case 13:
                            _ManagerTests = second;
                            num = 4;
                            break;
                        case 6:
                            m_ProcessTests = (bool)m_MessageTests.PrepareRecord().GetValue(config);
                            itemTests = (AnimatorTransitionBase)_TemplateTests.PrepareRecord().GetValue(config);
                            goto case 11;
                        case 9:
                            parserTests = config;
                            num = 13;
                            break;
                        case 11:
                            flag = itemTests != null;
                            goto case 12;
                        case 4:
                            taskTests = (bool)m_MessageTests.PrepareRecord().GetValue(config);
                            num = 6;
                            break;
                        case 5:
                            return;
                        case 14:
                            m_IteratorTests = stateTests == TokenizerTests.NodeType.exit;
                            num = 1;
                            break;
                        case 12:
                            _StructTests = (flag ? itemTests.destinationState : null);
                            num = 10;
                            break;
                        case 10:
                            _ServiceTests = (flag ? itemTests.destinationStateMachine : null);
                            num = 7;
                            break;
                        default:
                            broadcasterTests = (AnimatorStateMachine)((taskTests || m_ProcessTests) ? proxyTests : m_ConfigTests.PrepareRecord().GetValue(config));
                            num = 15;
                            break;
                        case 15:
                            globalTests = ((m_SchemaTests != null) ? TokenizerTests.NodeType.state : ((!(broadcasterTests == null)) ? (taskTests ? TokenizerTests.NodeType.any : ((m_ProcessTests || proxyTests != null) ? TokenizerTests.NodeType.entry : TokenizerTests.NodeType.machine)) : TokenizerTests.NodeType.unknown));
                            num = 2;
                            break;
                        case 2:
                            stateTests = ((_StructTests != null) ? TokenizerTests.NodeType.state : ((_ServiceTests != null) ? TokenizerTests.NodeType.machine : TokenizerTests.NodeType.exit));
                            num = 3;
                            break;
                        case 3:
                            _ProducerTests = globalTests == TokenizerTests.NodeType.entry && !m_ProcessTests;
                            num = 14;
                            break;
                        case 1:
                            specificationTests = ((taskTests || globalTests == TokenizerTests.NodeType.state) ? ((AnimatorStateTransition)itemTests) : null);
                            num = 8;
                            break;
                        case 8:
                            m_MethodTests = ((_ProducerTests || globalTests == TokenizerTests.NodeType.machine) ? ((AnimatorTransition)itemTests) : null);
                            return;
                    }
                }
            }

            public void InitSerializer()
            {
                TokenizerTests.NodeType nodeType = globalTests;
                switch (nodeType)
                {
                    case TokenizerTests.NodeType.state:
                        m_SchemaTests.RemoveTransition(specificationTests);
                        break;
                    case TokenizerTests.NodeType.machine:
                        proxyTests.RemoveStateMachineTransition(broadcasterTests, m_MethodTests);
                        break;
                    case TokenizerTests.NodeType.entry:
                        if (!m_ProcessTests)
                        {
                            broadcasterTests.RemoveEntryTransition(m_MethodTests);
                        }

                        break;
                    case TokenizerTests.NodeType.any:
                        broadcasterTests.RemoveAnyStateTransition(specificationTests);
                        break;
                    case TokenizerTests.NodeType.tree:
                    case TokenizerTests.NodeType.exit:
                        break;
                }
            }

            static InstanceTests()
            {
                while (true)
                {
                    int num = 4;
                    while (true)
                    {
                        switch (num)
                        {
                            case 9:
                                m_MessageTests = new ReflectedMember<PropertyInfo>(ConnectionTests.readerTests, "isAnyStateTransition");
                                num = 6;
                                continue;
                            case 5:
                                break;
                            case 11:
                                descriptorTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "ownerStateMachine");
                                num = 10;
                                continue;
                            case 8:
                                m_ClientTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "sourceState");
                                num = 0;
                                continue;
                            case 2:
                                return;
                            case 4:
                                m_FieldTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "m_DisplayName");
                                num = 3;
                                continue;
                            case 3:
                                goto case 1;
                            case 1:
                                attributeTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "m_FullName");
                                num = 8;
                                continue;
                            case 10:
                                _TemplateTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "transition");
                                num = 9;
                                continue;
                            default:
                                m_ConfigTests = new ReflectedMember<FieldInfo>(ConnectionTests.readerTests, "sourceStateMachine");
                                num = 11;
                                continue;
                            case 6:
                            case 7:
                                m_CollectionTests = new ReflectedMember<PropertyInfo>(ConnectionTests.readerTests, "isDefaultTransition");
                                num = 2;
                                continue;
                        }

                        break;
                    }
                }
            }
        }

        public static TokenizerTests RevertThread(AnimatorState i)
        {
            return new TokenizerTests((Node)BridgeTests._IssuerTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1]{i}));
        }

        public static TokenizerTests OrderPolicy(AnimatorStateMachine last)
        {
            return new TokenizerTests((Node)BridgeTests._PrototypeTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1]{last}));
        }
    }
}
