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
        internal static class EditorPatchRegistry
        {
            internal struct ProcessAlgo
            {
                internal readonly MethodInfo m_ProducerAlgo;
                internal readonly MethodInfo iteratorAlgo;
                internal readonly MethodInfo m_PublisherAlgo;
                internal readonly MethodInfo configurationAlgo;
                internal readonly MethodInfo procAlgo;
                internal readonly MethodInfo wrapperMapper;
                internal ProcessAlgo(MethodInfo asset, MethodInfo selection, MethodInfo field, MethodInfo ord2 = null, MethodInfo first3 = null, MethodInfo key4 = null) : this()
                {
                    m_ProducerAlgo = asset;
                    iteratorAlgo = selection;
                    m_PublisherAlgo = field;
                    configurationAlgo = ord2;
                    procAlgo = first3;
                    wrapperMapper = key4;
                }
            }

            internal delegate void AnnotationMapper<T>(ref T arg);
            internal delegate void VisitorMapper<T, TT>(ref T arg1, ref TT arg2);
            internal delegate void AlgoMapper<T, TT, T3>(ref T arg1, ref TT arg2, ref T3 arg3);
            internal delegate void MapperMapper<T, TT, T3, G>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);
            internal delegate void InitializerMapper<T, TT, T3, G, GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);
            internal delegate void DefinitionMapper<T, TT, T3, G, GG, A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);
            internal delegate AA RegMapper<T, TT, T3, G, GG, A, out AA>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);
            internal delegate A TestsMapper<T, TT, T3, G, GG, out A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);
            internal delegate GG PropertyMapper<T, TT, T3, G, out GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);
            internal delegate G ProcessorMapper<T, TT, T3, out G>(ref T arg1, ref TT arg2, ref T3 arg3);
            internal delegate T3 ObserverMapper<T, TT, out T3>(ref T arg1, ref TT arg2);
            internal delegate TT ServerMapper<T, out TT>(ref T arg);
            internal delegate void ThreadMapper<T>(out T arg);
            internal delegate void PolicyMapper<T, TT>(out T arg1, out TT arg2);
            internal delegate void SerializerMapper<T, TT, T3>(out T arg1, out TT arg2, out T3 arg3);
            internal delegate void PageMapper<T, TT, T3, G>(out T arg1, out TT arg2, out T3 arg3, out G arg4);
            internal delegate void ResolverMapper<T, TT, T3, G, GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);
            internal delegate void PredicateMapper<T, TT, T3, G, GG, A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);
            internal delegate AA RulesMapper<T, TT, T3, G, GG, A, out AA>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);
            internal delegate A QueueMapper<T, TT, T3, G, GG, out A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);
            internal delegate GG ErrorMapper<T, TT, T3, G, out GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4);
            internal delegate G SetterMapper<T, TT, T3, out G>(out T arg1, out TT arg2, out T3 arg3);
            internal delegate T3 ConnectionMapper<T, TT, out T3>(out T arg1, out TT arg2);
            internal delegate TT ContextMapper<T, out TT>(out T arg);
            internal delegate void RecordMapper<T, TT, T3, G>(T arg1, TT arg2, ref T3 arg3, ref G arg4);
            internal delegate void HelperMapper<T, in TT>(ref T arg1, TT arg2);
            internal delegate void ConsumerMapper<in T, TT>(T arg1, ref TT arg2);
            internal delegate void AdapterMapper<in T, TT>(T arg1, out TT arg2);
            internal delegate void InterpreterMapper<in T, in TT, T3>(T arg1, TT arg2, out T3 arg3);
            internal delegate void WatcherMapper<in T, TT, in T3>(T arg1, out TT arg2, T3 arg3);
            [Serializable]
            [CompilerGenerated]
            internal sealed class _003C_003Ec
            {
                [StructLayout(LayoutKind.Auto)]
                internal struct FilterMapper : IAsyncStateMachine
                {
                    public int _StubMapper;
                    public AsyncTaskMethodBuilder _ReaderMapper;
                    private TaskAwaiter _BridgeMapper;
                    private void MoveNext()
                    {
                        int num = _StubMapper;
                        try
                        {
                            TaskAwaiter awaiter;
                            if (num != 0)
                            {
                                awaiter = Task.Delay(4000).GetAwaiter();
                                if (!awaiter.IsCompleted)
                                {
                                    _StubMapper = 0;
                                    _BridgeMapper = awaiter;
                                    _ReaderMapper.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                                    return;
                                }
                            }
                            else
                            {
                                awaiter = _BridgeMapper;
                                _BridgeMapper = default(TaskAwaiter);
                                _StubMapper = -1;
                            }

                            awaiter.GetResult();
                            ControllerEditorUtility.CountRules(candidateMapper.VisitTests);
                        }
                        catch (System.Exception exception)
                        {
                            _StubMapper = -2;
                            _ReaderMapper.SetException(exception);
                            return;
                        }

                        _StubMapper = -2;
                        _ReaderMapper.SetResult();
                    }

                    void IAsyncStateMachine.MoveNext()
                    {
                        this.MoveNext();
                    }

                    [DebuggerHidden]
                    private void SetStateMachine(IAsyncStateMachine first)
                    {
                        _ReaderMapper.SetStateMachine(first);
                    }

                    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine first)
                    {
                        this.SetStateMachine(first);
                    }
                }

                public static readonly _003C_003Ec candidateMapper = new _003C_003Ec();
                public static Func<ParameterInfo, Type> m_ProductMapper;
                public static Func<ParameterInfo, Type> m_ExpressionMapper;
                public static Action systemMapper;
                public static Func<Task> m_WorkerMapper;
                private static _003C_003Ec ChangeProduct;
                internal void VisitTests()
                {
                    try
                    {
                        ReflectReg();
                    }
                    catch (System.Exception exception)
                    {
                        UnityEngine.Debug.LogException(exception);
                    }
                    finally
                    {
                        _StructAlgo = false;
                    }
                }
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass15_0
            {
                public string _StrategyMapper;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass16_0
            {
                public string _CustomerMapper;
                public Type _DatabaseMapper;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass22_0
            {
                public string _RegistryMapper;
                public string tagMapper;
            }

            internal static Dictionary<string, Harmony> methodAlgo;
            private static Dictionary<string, ProcessAlgo> m_SchemaAlgo = new Dictionary<string, ProcessAlgo>();
            internal static bool m_BroadcasterAlgo;
            internal static bool _ProxyAlgo;
            internal static bool _StructAlgo;
            internal static bool _ServiceAlgo;
            internal static bool _StateAlgo;
            internal static string _GlobalAlgo;
            internal static readonly (Action, bool)[] _TaskAlgo = new (Action, bool)[1]{(RevertWrapper, false)};
            [SpecialName]
            internal static Harmony AddTests()
            {
                return DeleteReg("com.fynn.tools.controllereditor.tool");
            }

            [EditorInitializeAttribute(0)]
            internal static void ConnectReg()
            {
                for (int i = 0; i < _TaskAlgo.Length; i++)
                {
                    (Action, bool) tuple = _TaskAlgo[i];
                    var(item, _) = tuple;
                    if (!tuple.Item2)
                    {
                        _TaskAlgo[i] = (item, true);
                        _TaskAlgo[i].Item1();
                    }
                }
            }

            [EditorShutdownAttribute(0)]
            internal static void CalculateReg()
            {
                if (methodAlgo != null)
                {
                    foreach (KeyValuePair<string, Harmony> item2 in methodAlgo)
                    {
                        item2.Value.UnpatchAll(item2.Key);
                    }

                    methodAlgo.Clear();
                }

                for (int i = 0; i < _TaskAlgo.Length; i++)
                {
                    (Action, bool) tuple = _TaskAlgo[i];
                    var(item, _) = tuple;
                    if (tuple.Item2)
                    {
                        _TaskAlgo[i] = (item, false);
                    }
                }
            }

            internal static void TestReg(string value, string token, MethodInfo comp = null, MethodInfo instance2 = null, MethodInfo res3 = null, string init4 = "")
            {
                Type type = ControllerEditorUtility.ForgotRules(value);
                if (!(type == null))
                {
                    MapReg(type, token, comp, instance2, res3);
                }
                else
                {
                    FindVisitor("Couldn't find patch target type:\n" + value, CustomLogType.Error);
                }
            }

            internal static void MapReg(Type init, string selection, MethodInfo util = null, MethodInfo second2 = null, MethodInfo token3 = null, string task4 = "")
            {
                while (true)
                {
                    _003C_003Ec__DisplayClass15_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass15_0();
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                return;
                            case 4:
                                return;
                            case 1:
                                break;
                            case 5:
                                RateReg(AccessTools.GetDeclaredMethods(init).First((MethodInfo m) => m.Name == CS_0024_003C_003E8__locals2._StrategyMapper), util, second2, token3);
                                num = 4;
                                continue;
                            default:
                                CS_0024_003C_003E8__locals2._StrategyMapper = selection;
                                num = 3;
                                continue;
                            case 3:
                                goto case 5;
                        }

                        break;
                    }
                }
            }

            internal static void ValidateReg(Type first, Type caller, string pool, MethodInfo col2 = null, MethodInfo task3 = null, MethodInfo t4 = null, string def5 = "")
            {
                while (true)
                {
                    _003C_003Ec__DisplayClass16_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass16_0();
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 2:
                                CS_0024_003C_003E8__locals4._CustomerMapper = pool;
                                CS_0024_003C_003E8__locals4._DatabaseMapper = caller;
                                num = 1;
                                continue;
                            case 3:
                                break;
                            case 1:
                                RateReg(AccessTools.GetDeclaredMethods(first).First(delegate (MethodInfo m)
                                {
                                    if (!(m.Name == CS_0024_003C_003E8__locals4._CustomerMapper))
                                    {
                                        return false;
                                    }

                                    return m.GetParameters().Any((ParameterInfo p) => p.ParameterType == CS_0024_003C_003E8__locals4._DatabaseMapper);
                                }), col2, task3, t4);
                                num = 0;
                                continue;
                            case 0:
                                return;
                        }

                        break;
                    }
                }
            }

            internal static void RateReg(MethodInfo i, MethodInfo col = null, MethodInfo comp = null, MethodInfo ident2 = null, string second3 = "")
            {
                try
                {
                    object obj;
                    if (!(col != null))
                    {
                        obj = null;
                    }
                    else
                    {
                        obj = new HarmonyMethod(col);
                    }

                    HarmonyMethod prefix = (HarmonyMethod)obj;
                    HarmonyMethod postfix = ((comp != null) ? new HarmonyMethod(comp) : null);
                    HarmonyMethod transpiler = ((ident2 != null) ? new HarmonyMethod(ident2) : null);
                    DeleteReg(second3).Patch(i, prefix, postfix, transpiler);
                }
                catch (System.Exception ex)
                {
                    _ProxyAlgo = true;
                    _GlobalAlgo = _GlobalAlgo + ex.Message + "\n";
                }
            }

            internal static void CalcReg(ConstructorInfo key, MethodInfo col = null, MethodInfo state = null, MethodInfo counter2 = null, string pol3 = "")
            {
                try
                {
                    object obj;
                    if (!(col != null))
                    {
                        obj = null;
                    }
                    else
                    {
                        obj = new HarmonyMethod(col);
                    }

                    HarmonyMethod prefix = (HarmonyMethod)obj;
                    HarmonyMethod postfix = ((state != null) ? new HarmonyMethod(state) : null);
                    HarmonyMethod transpiler = ((counter2 != null) ? new HarmonyMethod(counter2) : null);
                    DeleteReg(pol3).Patch(key, prefix, postfix, transpiler);
                }
                catch (System.Exception ex)
                {
                    _ProxyAlgo = true;
                    _GlobalAlgo = _GlobalAlgo + ex.Message + "\n";
                }
            }

            internal static void IncludeReg(string key, Type ivk, string tag, MethodInfo visitor2, Type key3, string token4, MethodInfo reference5 = null, MethodInfo last6 = null, MethodInfo x7 = null)
            {
                _003C_003Ec__DisplayClass22_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass22_0();
                CS_0024_003C_003E8__locals4._RegistryMapper = tag;
                CS_0024_003C_003E8__locals4.tagMapper = token4;
                MethodInfo cust = AccessTools.GetDeclaredMethods(ivk).First((MethodInfo m) => m.Name == CS_0024_003C_003E8__locals4._RegistryMapper);
                MethodInfo cfg = AccessTools.GetDeclaredMethods(key3).First((MethodInfo m) => m.Name == CS_0024_003C_003E8__locals4.tagMapper);
                RunReg(key, cust, visitor2, cfg, reference5, last6, x7);
            }

            internal static void RunReg(string reference, MethodInfo cust, MethodInfo c, MethodInfo cfg2, MethodInfo res3 = null, MethodInfo ivk4 = null, MethodInfo ident5 = null)
            {
                ProcessAlgo value = new ProcessAlgo(cust, c, cfg2, res3, ivk4, ident5);
                m_SchemaAlgo[reference] = value;
                try
                {
                    AddTests().Patch(cust, null, new HarmonyMethod(c));
                }
                catch (System.Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }

            internal static void CloneReg(string config)
            {
                while (true)
                {
                    ProcessAlgo processAlgo = m_SchemaAlgo[config];
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 5:
                                return;
                            case 1:
                                RateReg(processAlgo.m_PublisherAlgo, processAlgo.configurationAlgo, processAlgo.procAlgo, processAlgo.wrapperMapper);
                                num = 0;
                                continue;
                            case 2:
                                AddTests().Unpatch(processAlgo.m_ProducerAlgo, processAlgo.iteratorAlgo);
                                num = 1;
                                continue;
                            case 4:
                                break;
                            case 3:
                                goto case 2;
                        }

                        break;
                    }
                }
            }

            internal static void LoginReg()
            {
                if (!_ProxyAlgo)
                {
                    return;
                }

                if (_ServiceAlgo)
                {
                    return;
                }

                GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box);
                try
                {
                    if (!_StateAlgo)
                    {
                        if (!_StructAlgo)
                        {
                            _StructAlgo = true;
                            Task.Run(() =>
                            {
                                _003C_003Ec.FilterMapper stateMachine = default(_003C_003Ec.FilterMapper);
                                while (true)
                                {
                                    stateMachine._ReaderMapper = AsyncTaskMethodBuilder.Create();
                                    int num = 1;
                                    while (true)
                                    {
                                        switch (num)
                                        {
                                            case 2:
                                                break;
                                            case 4:
                                                stateMachine._ReaderMapper.Start(ref stateMachine);
                                                num = 5;
                                                continue;
                                            case 1:
                                                stateMachine._StubMapper = -1;
                                                num = 0;
                                                continue;
                                            case 3:
                                            case 5:
                                                return stateMachine._ReaderMapper.Task;
                                            default:
                                                goto case 4;
                                        }

                                        break;
                                    }
                                }
                            });
                        }
                    }

                    GUILayout.Label(new GUIContent(ControllerEditorUtility.DestroyError().m_CreatorProcessor)
                    {tooltip = "This may happen if there were special characters in the project's path.\n\nSimple error log:\n" + _GlobalAlgo}, ControllerEditorUtility.CalcError().broadcasterProcessor, GUILayout.Width(18f));
                    GUILayout.Label("Patching not fully successful. Some functions/patches may be missing.", GUILayout.ExpandWidth(expand: false));
                    if (_StructAlgo)
                    {
                        GUILayout.Label("Retrying...", GUILayout.ExpandWidth(expand: false));
                    }

                    GUILayout.FlexibleSpace();
                    if (_StateAlgo)
                    {
                        if (ControllerEditorUtility.CountQueue("Hide", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
                        {
                            _ServiceAlgo = true;
                        }

                        if (ControllerEditorUtility.CountQueue("Retry", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
                        {
                            ReflectReg();
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

            private static void ReflectReg()
            {
                while (true)
                {
                    _StateAlgo = true;
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                _ProxyAlgo = false;
                                num = 0;
                                continue;
                            case 3:
                                CalculateReg();
                                num = 2;
                                continue;
                            case 4:
                                break;
                            case 1:
                                return;
                            default:
                                m_BroadcasterAlgo = false;
                                RevertWrapper();
                                num = 1;
                                continue;
                        }

                        break;
                    }
                }
            }

            private static Harmony DeleteReg(string v)
            {
                if (string.IsNullOrWhiteSpace(v))
                {
                    return AddTests();
                }

                if (methodAlgo == null)
                {
                    methodAlgo = new Dictionary<string, Harmony>();
                }

                if (!methodAlgo.TryGetValue(v, out var value))
                {
                    value = new Harmony(v);
                    methodAlgo.Add(v, value);
                }

                return value;
            }

            internal static MethodInfo CreateReg(Action spec)
            {
                return spec.Method;
            }

            internal static MethodInfo NewReg<T>(Action<T> instance)
            {
                return instance.Method;
            }

            internal static MethodInfo PushReg<T, TT>(Action<T, TT> var1)
            {
                return var1.Method;
            }

            internal static MethodInfo ViewReg<T, TT, T3>(Action<T, TT, T3> res)
            {
                return res.Method;
            }

            internal static MethodInfo StopReg<T, TT, T3>(Func<T, TT, T3> spec)
            {
                return spec.Method;
            }

            internal static MethodInfo CheckReg<T, TT>(Func<T, TT> res)
            {
                return res.Method;
            }

            internal static MethodInfo PrepareReg<T>(Func<T> task)
            {
                return task.Method;
            }

            internal static MethodInfo AssetReg<T>(AnnotationMapper<T> var1)
            {
                return var1.Method;
            }

            internal static MethodInfo SearchReg<T, TT, T3>(ObserverMapper<T, TT, T3> instance)
            {
                return instance.Method;
            }

            internal static MethodInfo RevertReg<T, TT>(ServerMapper<T, TT> key)
            {
                return key.Method;
            }

            internal static MethodInfo OrderTests<T>(ThreadMapper<T> task)
            {
                return task.Method;
            }

            internal static MethodInfo CountTests<T, TT>(HelperMapper<T, TT> item)
            {
                return item.Method;
            }

            internal static MethodInfo InsertTests<T, TT>(AdapterMapper<T, TT> asset)
            {
                return asset.Method;
            }

            internal static MethodInfo RestartTests<T, TT, T3>(InterpreterMapper<T, TT, T3> def)
            {
                return def.Method;
            }

            internal static MethodInfo QueryTests<T, TT, T3>(WatcherMapper<T, TT, T3> info)
            {
                return info.Method;
            }
        }
    }
}
