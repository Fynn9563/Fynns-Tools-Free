using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal class AnimationWindowBridge
    {
        private static bool _VisitorProperty;
        private static Type m_AlgoProperty;
        private static MethodInfo m_MapperProperty;
        private static MethodInfo initializerProperty;
        private static MethodInfo definitionProperty;
        private static MethodInfo m_RegProperty;
        private static MethodInfo _TestsProperty;
        private static MethodInfo _PropertyProperty;
        private static FieldInfo _ProcessorProperty;
        private static FieldInfo _ObserverProperty;
        private static FieldInfo _ServerProperty;
        private static FieldInfo _ThreadProperty;
        private static FieldInfo _PolicyProperty;
        private static FieldInfo m_SerializerProperty;
        private object pageProperty;
        private bool m_ResolverProperty;
        private readonly Func<object> m_PredicateProperty;
        internal Action<bool> _RulesProperty;
        private static void FlushSerializer()
        {
            if (!_VisitorProperty)
            {
                _VisitorProperty = true;
                try
                {
                    m_AlgoProperty = ControllerEditorUtility.FillRules("UnityEditor.RenameOverlay, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                return;
                            case 14:
                                return;
                            default:
                                m_RegProperty = m_AlgoProperty.GetMethod("OnGUI", new Type[1]{typeof(GUIStyle)});
                                num = 3;
                                break;
                            case 13:
                                _PropertyProperty = m_AlgoProperty.GetMethod("Clear");
                                num = 8;
                                break;
                            case 11:
                                definitionProperty = m_AlgoProperty.GetMethod("IsRenaming");
                                num = 0;
                                break;
                            case 7:
                                _ThreadProperty = m_AlgoProperty.RestartList("m_Name");
                                num = 4;
                                break;
                            case 8:
                                _ProcessorProperty = m_AlgoProperty.RestartList("m_EditFieldRect");
                                num = 12;
                                break;
                            case 1:
                                goto case 10;
                            case 10:
                                m_MapperProperty = m_AlgoProperty.GetMethod("BeginRename");
                                num = 9;
                                break;
                            case 9:
                                initializerProperty = m_AlgoProperty.GetMethod("EndRename");
                                num = 11;
                                break;
                            case 4:
                                _PolicyProperty = m_AlgoProperty.RestartList("m_UserData");
                                num = 5;
                                break;
                            case 5:
                                m_SerializerProperty = m_AlgoProperty.RestartList("m_IsWaitingForDelay");
                                num = 14;
                                break;
                            case 3:
                                _TestsProperty = m_AlgoProperty.GetMethod("OnEvent");
                                num = 13;
                                break;
                            case 12:
                                _ObserverProperty = m_AlgoProperty.RestartList("m_UserAcceptedRename");
                                num = 6;
                                break;
                            case 6:
                                _ServerProperty = m_AlgoProperty.RestartList("m_OriginalName");
                                num = 7;
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("Rename Overlay Wrapper has failed to initialize!");
                    throw;
                }
            }
        }

        internal AnimationWindowBridge()
        {
            int num = 1;
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 0:
                        return;
                    case 1:
                        FlushSerializer();
                        num = 2;
                        break;
                    case 2:
                        DestroySerializer(Activator.CreateInstance(m_AlgoProperty));
                        num = 0;
                        break;
                }
            }
        }

        internal AnimationWindowBridge(object value)
        {
            int num = 1;
            while (true)
            {
                switch (num)
                {
                    case 3:
                        return;
                    default:
                        DestroySerializer(value);
                        num = 3;
                        continue;
                    case 2:
                        break;
                    case 1:
                        break;
                }

                FlushSerializer();
                num = 4;
            }
        }

        internal AnimationWindowBridge(Func<object> i)
        {
            m_PredicateProperty = i;
        }

        internal object ConnectSerializer()
        {
            FlushSerializer();
            pageProperty = m_PredicateProperty?.Invoke();
            m_ResolverProperty = true;
            return pageProperty;
        }

        [SpecialName]
        internal object RateSerializer()
        {
            if (pageProperty == null)
            {
                if (!m_ResolverProperty)
                {
                    FlushSerializer();
                    ConnectSerializer();
                    return pageProperty;
                }
            }

            return pageProperty;
        }

        [SpecialName]
        internal void DestroySerializer(object reference)
        {
            pageProperty = reference;
        }

        [SpecialName]
        internal void IncludeSerializer(Rect def)
        {
            _ProcessorProperty.SetValue(RateSerializer(), def);
        }

        [SpecialName]
        internal bool ReflectSerializer()
        {
            object obj = RateSerializer();
            return (bool)definitionProperty.Invoke(obj, null);
        }

        [SpecialName]
        internal int CreateSerializer()
        {
            return (int)_PolicyProperty.GetValue(RateSerializer());
        }

        [SpecialName]
        internal bool ViewSerializer()
        {
            return (bool)m_SerializerProperty.GetValue(RateSerializer());
        }

        [SpecialName]
        internal string ListSerializer()
        {
            return (string)_ThreadProperty.GetValue(RateSerializer());
        }

        [SpecialName]
        internal void VerifySerializer(string key)
        {
            _ThreadProperty.SetValue(RateSerializer(), key);
        }

        [SpecialName]
        internal string WriteSerializer()
        {
            return (string)_ServerProperty.GetValue(RateSerializer());
        }

        internal bool CalculateSerializer(Rect first, string ivk, int minhelper, float map2)
        {
            bool result = TestSerializer(ivk, minhelper, map2);
            object obj = RateSerializer();
            _ProcessorProperty.SetValue(obj, first);
            return result;
        }

        internal bool TestSerializer(string task, int indexvisitor, float helper)
        {
            object obj = RateSerializer();
            return (bool)m_MapperProperty.Invoke(obj, new object[3]{task, indexvisitor, helper});
        }

        internal void MapSerializer(bool isconfig, bool iscfg = true)
        {
            if (!ReflectSerializer())
            {
                return;
            }

            object obj = RateSerializer();
            initializerProperty.Invoke(obj, new object[1]{isconfig});
            _RulesProperty?.Invoke(isconfig);
            if (iscfg)
            {
                CustomizeSerializer();
            }
        }

        internal bool OnGUI(GUIStyle textFieldStyle = null)
        {
            object obj = RateSerializer();
            return (bool)m_RegProperty.Invoke(obj, new object[1]{textFieldStyle});
        }

        internal bool ValidateSerializer()
        {
            object obj = RateSerializer();
            return (bool)_TestsProperty.Invoke(obj, null);
        }

        internal void CustomizeSerializer()
        {
            object obj = RateSerializer();
            _PropertyProperty.Invoke(obj, null);
        }
    }
}
