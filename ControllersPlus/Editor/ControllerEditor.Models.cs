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
        internal class GlobalVisitor
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass10_0
            {
                public float _WrapperAlgo;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass8_0
            {
                public string _AnnotationAlgo;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass9_0
            {
                public AnimatorConditionMode _VisitorAlgo;
            }

            internal bool _TaskVisitor;
            internal AnimatorCondition m_ProcessVisitor;
            internal readonly List<(AnimatorTransitionBase, int)> _ProducerVisitor;
            internal readonly bool[] iteratorVisitor = new bool[3];
            internal GlobalVisitor(AnimatorTransitionBase asset, int ivk_low)
            {
                _TaskVisitor = false;
                m_ProcessVisitor = asset.conditions[ivk_low];
                _ProducerVisitor = new List<(AnimatorTransitionBase, int)>{(asset, ivk_low)};
            }

            internal void CollectInitializer(AnimatorTransitionBase res, int colhigh)
            {
                while (true)
                {
                    _TaskVisitor = true;
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
                                goto IL_009e;
                            default:
                                goto IL_009e;
                        }

                        break;
                        IL_009e:
                            _ProducerVisitor.Add((res, colhigh));
                        num = 4;
                    }
                }
            }

            internal void ResolveInitializer(AnimatorCondition item)
            {
                UnityEngine.Object[] objectsToUndo = _ProducerVisitor.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
                Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
                foreach (var item4 in _ProducerVisitor)
                {
                    AnimatorTransitionBase item2 = item4.Item1;
                    int item3 = item4.Item2;
                    AnimatorCondition[] conditions = item2.conditions;
                    conditions[item3] = item;
                    item2.conditions = conditions;
                }
            }

            private void ListInitializer(Func<AnimatorCondition, AnimatorCondition> init)
            {
                UnityEngine.Object[] objectsToUndo = _ProducerVisitor.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
                Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
                using List<(AnimatorTransitionBase, int)>.Enumerator enumerator = _ProducerVisitor.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    (AnimatorTransitionBase, int) current = enumerator.Current;
                    AnimatorTransitionBase item = current.Item1;
                    int item2 = current.Item2;
                    AnimatorCondition[] conditions = item.conditions;
                    conditions[item2] = init(conditions[item2]);
                    item.conditions = conditions;
                }
            }

            internal void VerifyInitializer(string value)
            {
                _003C_003Ec__DisplayClass8_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass8_0();
                CS_0024_003C_003E8__locals3._AnnotationAlgo = value;
                m_ProcessVisitor = new AnimatorCondition{parameter = CS_0024_003C_003E8__locals3._AnnotationAlgo, mode = m_ProcessVisitor.mode, threshold = m_ProcessVisitor.threshold};
                ListInitializer(delegate (AnimatorCondition c)
                {
                    AnimatorCondition result = default(AnimatorCondition);
                    result.parameter = CS_0024_003C_003E8__locals3._AnnotationAlgo;
                    result.mode = c.mode;
                    result.threshold = c.threshold;
                    return result;
                });
                iteratorVisitor[0] = false;
            }

            internal void FillInitializer(AnimatorConditionMode value)
            {
                _003C_003Ec__DisplayClass9_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass9_0();
                CS_0024_003C_003E8__locals3._VisitorAlgo = value;
                m_ProcessVisitor = new AnimatorCondition{parameter = m_ProcessVisitor.parameter, mode = CS_0024_003C_003E8__locals3._VisitorAlgo, threshold = m_ProcessVisitor.threshold};
                ListInitializer(delegate (AnimatorCondition c)
                {
                    while (true)
                    {
                        AnimatorCondition result = default(AnimatorCondition);
                        int num = 0;
                        while (true)
                        {
                            switch (num)
                            {
                                case 5:
                                    result.threshold = c.threshold;
                                    num = 2;
                                    continue;
                                case 6:
                                    result.parameter = c.parameter;
                                    num = 4;
                                    continue;
                                case 4:
                                    result.mode = CS_0024_003C_003E8__locals3._VisitorAlgo;
                                    num = 5;
                                    continue;
                                case 2:
                                case 3:
                                    return result;
                                case 1:
                                    break;
                                default:
                                    goto case 6;
                            }

                            break;
                        }
                    }
                });
                iteratorVisitor[1] = false;
            }

            internal void WriteInitializer(float ident)
            {
                _003C_003Ec__DisplayClass10_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass10_0();
                CS_0024_003C_003E8__locals3._WrapperAlgo = ident;
                m_ProcessVisitor = new AnimatorCondition{parameter = m_ProcessVisitor.parameter, mode = m_ProcessVisitor.mode, threshold = CS_0024_003C_003E8__locals3._WrapperAlgo};
                ListInitializer(delegate (AnimatorCondition c)
                {
                    while (true)
                    {
                        AnimatorCondition result = default(AnimatorCondition);
                        int num = 0;
                        while (true)
                        {
                            switch (num)
                            {
                                default:
                                    result.parameter = c.parameter;
                                    num = 3;
                                    continue;
                                case 1:
                                    break;
                                case 3:
                                    result.mode = c.mode;
                                    result.threshold = CS_0024_003C_003E8__locals3._WrapperAlgo;
                                    num = 2;
                                    continue;
                                case 2:
                                    return result;
                            }

                            break;
                        }
                    }
                });
                iteratorVisitor[2] = false;
            }

            internal void ForgotInitializer()
            {
                while (true)
                {
                    m_ProcessVisitor = ResolveAlgo(m_ProcessVisitor);
                    int num = 2;
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                                return;
                            default:
                                goto IL_005a;
                            case 4:
                                return;
                            case 3:
                                break;
                            case 2:
                                goto IL_005a;
                        }

                        break;
                        IL_005a:
                            ListInitializer(ResolveAlgo);
                        num = 4;
                    }
                }
            }

            internal void StopInitializer()
            {
                using List<(AnimatorTransitionBase, int)>.Enumerator enumerator = _ProducerVisitor.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Item1.RemoveCondition(m_ProcessVisitor);
                }
            }

            internal void CheckInitializer(bool[] item)
            {
                int i = 0;
                for (; i < 3; i++)
                {
                    iteratorVisitor[i] |= !item[i];
                }
            }
        }

        internal class AlgoAlgo
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass6_0
            {
                public SerializedPropertyGroup.BridgeProperty _RegAlgo;
            }

            internal bool _MapperAlgo;
            internal SerializedPropertyGroup.BridgeProperty.AttrProperty m_InitializerAlgo;
            internal List<(SerializedPropertyGroup.BridgeProperty, int)> definitionAlgo;
            internal AlgoAlgo(SerializedPropertyGroup.BridgeProperty asset, int cust)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            m_InitializerAlgo = asset.m_DatabaseProperty[cust];
                            num = 0;
                            break;
                        case 1:
                            return;
                        default:
                            definitionAlgo = new List<(SerializedPropertyGroup.BridgeProperty, int)>{(asset, cust)};
                            num = 1;
                            break;
                        case 3:
                            _MapperAlgo = false;
                            num = 2;
                            break;
                    }
                }
            }

            internal void RegisterInitializer(SerializedPropertyGroup.BridgeProperty config, int size_connection)
            {
                _MapperAlgo = true;
                definitionAlgo.Add((config, size_connection));
            }

            internal void LogoutInitializer(SerializedPropertyGroup.BridgeProperty.AttrProperty ident)
            {
                int i = 0;
                for (; i < definitionAlgo.Count; i++)
                {
                    SerializedPropertyGroup.BridgeProperty item = definitionAlgo[i].Item1;
                    RestartAnnotation(ident, item.m_DatabaseProperty[definitionAlgo[i].Item2]);
                    EditorUtility.SetDirty(item._StrategyProperty);
                }
            }

            internal void PatchInitializer()
            {
                using List<(SerializedPropertyGroup.BridgeProperty, int)>.Enumerator enumerator = definitionAlgo.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _003C_003Ec__DisplayClass6_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass6_0();
                    (SerializedPropertyGroup.BridgeProperty, int) current = enumerator.Current;
                    CS_0024_003C_003E8__locals4._RegAlgo = current.Item1;
                    int item = current.Item2;
                    bool num = CS_0024_003C_003E8__locals4._RegAlgo.InitPage(item);
                    EditorUtility.SetDirty(CS_0024_003C_003E8__locals4._RegAlgo._StrategyProperty);
                    if (num)
                    {
                        m_AlgoAnnotation.InvokeResolver(delegate (AnimatorState s)
                        {
                            s.RemoveBehaviour(s.FindBehaviourIndex(CS_0024_003C_003E8__locals4._RegAlgo._StrategyProperty), verifytemp: true);
                        });
                    }
                }
            }
        }

        internal enum VRCFullOptions
        {
            Set,
            Add,
            Random
        }

        internal enum VRCHalfOptions
        {
            Set = 0,
            Random = 2
        }

        internal sealed class PoolAlgo
        {
            private readonly ProcessStartInfo _ParameterAlgo;
            private readonly Action<string> _RepositoryAlgo;
            private readonly Action m_MappingAlgo;
            private readonly bool baseAlgo;
            internal PoolAlgo(string var1, Action<string> col, bool isfield = false, bool containscol2 = false, Action token3 = null)
            {
                _ParameterAlgo = new ProcessStartInfo(isfield ? "cmd.exe" : "powershell.exe")
                {CreateNoWindow = true, UseShellExecute = false, RedirectStandardInput = false, RedirectStandardOutput = true, Arguments = "/c " + var1};
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                _ParameterAlgo.WorkingDirectory = folderPath;
                if (!isfield)
                {
                    string text = Path.Combine(folderPath, "WindowsPowerShell", "v1.0");
                    if (Directory.Exists(text))
                    {
                        _ParameterAlgo.WorkingDirectory = text;
                    }
                }

                _RepositoryAlgo = col;
                m_MappingAlgo = token3;
                baseAlgo = containscol2;
            }
        }

        [DefaultMember("Item")]
        internal readonly struct FieldAlgo
        {
            private readonly string _AttributeAlgo;
            private readonly Dictionary<string, DescriptorAlgo> _ClientAlgo;
            internal readonly bool _ConfigAlgo;
            internal FieldAlgo(string i) : this()
            {
                int num = 14;
                string value = default(string);
                string value2 = default(string);
                int count = default(int);
                int num2 = default(int);
                MatchCollection matchCollection = default(MatchCollection);
                Match match = default(Match);
                while (true)
                {
                    switch (num)
                    {
                        case 6:
                            _ClientAlgo[value] = new DescriptorAlgo(value2);
                            num = 18;
                            break;
                        case 7:
                            if (string.IsNullOrWhiteSpace(value2))
                            {
                                num = 9;
                                break;
                            }

                            goto IL_01a7;
                        case 2:
                            _ClientAlgo = new Dictionary<string, DescriptorAlgo>();
                            num = 13;
                            break;
                        case 11:
                            if (count == 0)
                            {
                                _ConfigAlgo = true;
                                num = 15;
                                break;
                            }

                            goto case 5;
                        case 5:
                            _ConfigAlgo = false;
                            num = 2;
                            break;
                        case 12:
                            return;
                        case 13:
                            num2 = 0;
                            num = 0;
                            break;
                        case 14:
                            _AttributeAlgo = i;
                            num = 1;
                            break;
                        case 15:
                            _ClientAlgo = null;
                            num = 8;
                            break;
                        case 16:
                        case 18:
                            num2++;
                            goto default;
                        case 8:
                            return;
                        case 10:
                            count = matchCollection.Count;
                            num = 11;
                            break;
                        case 9:
                            value2 = match.Groups[3].Value;
                            goto IL_01a7;
                        default:
                            if (num2 >= count)
                            {
                                num = 12;
                                break;
                            }

                            goto case 17;
                        case 4:
                            value = match.Groups[1].Value;
                            value2 = match.Groups[2].Value;
                            num = 7;
                            break;
                        case 17:
                            match = matchCollection[num2];
                            num = 4;
                            break;
                        case 1:
                        {
                            matchCollection = Regex.Matches(i, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
                            num = 10;
                            break;
                        }

                            IL_01a7:
                                if (string.IsNullOrEmpty(value))
                                {
                                    num = 16;
                                    break;
                                }

                            goto case 6;
                    }
                }
            }

            public override string ToString()
            {
                return _AttributeAlgo;
            }
        }

        internal readonly struct DescriptorAlgo
        {
            internal readonly string templateAlgo;
            internal readonly string m_MessageAlgo;
            internal readonly bool m_CollectionAlgo;
            internal readonly float _ParserAlgo;
            internal readonly bool m_ManagerAlgo;
            internal DescriptorAlgo(string last) : this()
            {
                while (true)
                {
                    templateAlgo = last;
                    int num = 9;
                    while (true)
                    {
                        switch (num)
                        {
                            case 12:
                                break;
                            case 5:
                                float.TryParse(m_MessageAlgo, out _ParserAlgo);
                                num = 3;
                                continue;
                            case 3:
                                return;
                            default:
                                m_MessageAlgo = last;
                                num = 7;
                                continue;
                            case 9:
                                m_ManagerAlgo = true;
                                num = 6;
                                continue;
                            case 6:
                                if (last.Length <= 1)
                                {
                                    m_MessageAlgo = last;
                                    num = 11;
                                    continue;
                                }

                                goto case 1;
                            case 1:
                                if (!last.StartsWith("\""))
                                {
                                    num = 2;
                                    continue;
                                }

                                goto case 10;
                            case 10:
                                if (!last.EndsWith("\""))
                                {
                                    goto default;
                                }

                                goto case 8;
                            case 8:
                                m_MessageAlgo = ((last.Length == 2) ? string.Empty : last.Substring(1, last.Length - 2));
                                num = 4;
                                continue;
                            case 4:
                            case 7:
                            case 11:
                                m_CollectionAlgo = m_MessageAlgo == "true";
                                num = 5;
                                continue;
                        }

                        break;
                    }
                }
            }

            public override string ToString()
            {
                return m_MessageAlgo;
            }

            public static implicit operator string (DescriptorAlgo var1)
            {
                return var1.m_MessageAlgo;
            }

            public static implicit operator bool (DescriptorAlgo key)
            {
                return key.m_CollectionAlgo;
            }

            public static implicit operator float (DescriptorAlgo key)
            {
                return key._ParserAlgo;
            }
        }

        internal enum CustomLogType
        {
            Regular,
            Warning,
            Error
        }

        internal enum ControllerAction
        {
            ReplaceParameter,
            RemoveParameter,
            Copy,
            TagCurrentLayerWith,
            RemoveLayersWithTag,
            RemoveTag
        }

        internal enum ActionMode
        {
            CurrentController,
            LayersTaggedWith,
            CurrentLayer,
            CurrentStatemachine
        }

        internal enum MoveMode
        {
            CurrentController,
            LayersTaggedWith,
            CurrentLayer
        }

        internal enum MoveDestination
        {
            Controller,
            CurrentController
        }

        internal enum FloatModes
        {
            Greater = 3,
            Less
        }

        internal enum IntModes
        {
            Greater = 3,
            Less = 4,
            Equals = 6,
            NotEqual = 7
        }

        internal enum BoolModes
        {
            True = 1,
            False
        }

        internal enum LayerViewViewType
        {
            DefaultView,
            CategoryByName,
            CategoryByTag
        }

        internal class ImporterMapper
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass10_0
            {
                public EventMapper interceptorMapper;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass12_0
            {
                public string m_CreatorMapper;
            }

            internal readonly string requestMapper;
            internal readonly int _PrinterMapper;
            internal readonly string m_WriterMapper;
            internal readonly List<ImporterMapper> paramsMapper = new List<ImporterMapper>();
            internal readonly List<EventMapper> listenerMapper = new List<EventMapper>();
            internal ImporterMapper m_GetterMapper;
            [SpecialName]
            internal string CustomizeTests()
            {
                return ValidateTests(m_WriterMapper);
            }

            internal ImporterMapper(string v, string result, int dirPtr = 0)
            {
                requestMapper = v;
                _PrinterMapper = dirPtr;
                m_WriterMapper = result;
            }

            internal ImporterMapper ResetTests(string key, UnityEditor.Animations.AnimatorControllerLayer selection, int positionc)
            {
                EventMapper res = new EventMapper(selection, positionc);
                FlushTests(res);
                string[] array = QueryMapper(key);
                string text = array[0];
                string text2 = string.Join(PushInitializer(), array, 1, array.Length - 1);
                ImporterMapper importerMapper = CalculateTests(text);
                if (importerMapper == null && !text2.FlushResolver())
                {
                    importerMapper = new ImporterMapper(text, m_WriterMapper + PushInitializer() + text, _PrinterMapper + 1);
                    paramsMapper.Add(importerMapper);
                }

                if (text2.ResetResolver())
                {
                    importerMapper?.FlushTests(res);
                    if (importerMapper != TestTests())
                    {
                        TestTests().FlushTests(res);
                    }

                    return importerMapper;
                }

                return importerMapper.ResetTests(text2, selection, positionc);
            }

            internal void FlushTests(EventMapper res)
            {
                _003C_003Ec__DisplayClass10_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass10_0();
                CS_0024_003C_003E8__locals3.interceptorMapper = res;
                if (listenerMapper.All((EventMapper l) => l._FacadeMapper != CS_0024_003C_003E8__locals3.interceptorMapper._FacadeMapper))
                {
                    listenerMapper.Add(CS_0024_003C_003E8__locals3.interceptorMapper);
                }
            }

            internal ImporterMapper ConnectTests(string info)
            {
                string[] array = QueryMapper(info);
                ImporterMapper importerMapper = this;
                string[] array2 = array;
                foreach (string def in array2)
                {
                    ImporterMapper importerMapper2 = importerMapper.CalculateTests(def);
                    if (importerMapper2 == null)
                    {
                        break;
                    }

                    importerMapper = importerMapper2;
                }

                return importerMapper;
            }

            internal ImporterMapper CalculateTests(string def)
            {
                _003C_003Ec__DisplayClass12_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass12_0();
                string[] array = QueryMapper(def);
                CS_0024_003C_003E8__locals2.m_CreatorMapper = array[0];
                string text = ((array.Length > 1) ? string.Join(PushInitializer(), array, 1, array.Length - 1) : "");
                ImporterMapper importerMapper = paramsMapper.FirstOrDefault((ImporterMapper c) => c.requestMapper == CS_0024_003C_003E8__locals2.m_CreatorMapper);
                if (importerMapper != null)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        return importerMapper.CalculateTests(text);
                    }

                    return importerMapper;
                }

                return null;
            }

            internal ImporterMapper TestTests()
            {
                if (m_GetterMapper != null)
                {
                    return m_GetterMapper;
                }

                m_GetterMapper = CalculateTests(ValidateInitializer());
                if (m_GetterMapper == null)
                {
                    paramsMapper.Add(m_GetterMapper = new ImporterMapper(ValidateInitializer(), m_WriterMapper + PushInitializer() + ValidateInitializer(), _PrinterMapper + 1));
                }

                return m_GetterMapper;
            }

            internal void MapTests(string param, Action<ImporterMapper> selection, bool isres = true)
            {
                if (isres)
                {
                    selection(this);
                }

                if (param.ResetResolver())
                {
                    return;
                }

                string[] array = param.Split(new char[1]{'/'});
                ImporterMapper importerMapper = this;
                string[] array2 = array;
                foreach (string def in array2)
                {
                    importerMapper = importerMapper.CalculateTests(def);
                    if (importerMapper == null)
                    {
                        break;
                    }

                    selection(importerMapper);
                }
            }

            private static string ValidateTests(string item)
            {
                return Regex.Replace(item, "^Root" + Regex.Escape(PushInitializer()) + "?", "");
            }
        }

        internal struct EventMapper
        {
            internal readonly UnityEditor.Animations.AnimatorControllerLayer _InfoMapper;
            internal readonly int _FacadeMapper;
            internal EventMapper(UnityEditor.Animations.AnimatorControllerLayer value, int next_cfg) : this()
            {
                _InfoMapper = value;
                _FacadeMapper = next_cfg;
            }

            public static implicit operator UnityEditor.Animations.AnimatorControllerLayer(EventMapper asset)
            {
                return asset._InfoMapper;
            }
        }
    }
}
