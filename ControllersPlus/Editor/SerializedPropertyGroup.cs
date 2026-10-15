using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class SerializedPropertyGroup
    {
        internal class BridgeProperty
        {
            internal class AttrProperty
            {
                internal enum ChangeType
                {
                    Set,
                    Add,
                    Random
                }

                internal BridgeProperty _DispatcherProperty;
                internal SerializedProperty m_RegistryProperty;
                private bool _TagProperty;
                [SpecialName]
                internal bool RemovePage()
                {
                    return _TagProperty;
                }

                [SpecialName]
                internal void InstantiatePage(bool validatekey)
                {
                    if (_TagProperty)
                    {
                        if (!validatekey)
                        {
                            _DispatcherProperty.DefinePage();
                        }
                    }

                    _TagProperty = validatekey;
                }

                [SpecialName]
                internal string ResetPage()
                {
                    return m_RegistryProperty.FindPropertyRelative("name").stringValue;
                }

                [SpecialName]
                internal void FlushPage(string def)
                {
                    while (true)
                    {
                        m_RegistryProperty.FindPropertyRelative("name").stringValue = def;
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
                                    return;
                                case 0:
                                    return;
                                case 6:
                                    _DispatcherProperty.DefinePage();
                                    num = 5;
                                    continue;
                                case 3:
                                    if (RemovePage())
                                    {
                                        num = 0;
                                        continue;
                                    }

                                    goto case 2;
                                case 4:
                                    break;
                                case 2:
                                    goto case 6;
                            }

                            break;
                        }
                    }
                }

                [SpecialName]
                internal string CalculatePage()
                {
                    try
                    {
                        return m_RegistryProperty.FindPropertyRelative("source").stringValue;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                [SpecialName]
                internal void TestPage(string spec)
                {
                    try
                    {
                        m_RegistryProperty.FindPropertyRelative("source").stringValue = spec;
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
                                    return;
                                case 2:
                                    break;
                                case 1:
                                    break;
                            }

                            if (RemovePage())
                            {
                                num = 0;
                                continue;
                            }

                            _DispatcherProperty.DefinePage();
                            num = 3;
                        }
                    }
                    catch
                    {
                    }
                }

                [SpecialName]
                internal float ValidatePage()
                {
                    return m_RegistryProperty.FindPropertyRelative("value").floatValue;
                }

                [SpecialName]
                internal void CustomizePage(float info)
                {
                    m_RegistryProperty.FindPropertyRelative("value").floatValue = info;
                    if (!RemovePage())
                    {
                        _DispatcherProperty.DefinePage();
                    }
                }

                [SpecialName]
                internal float DestroyPage()
                {
                    return m_RegistryProperty.FindPropertyRelative("chance").floatValue;
                }

                [SpecialName]
                internal void GetPage(float def)
                {
                    while (true)
                    {
                        m_RegistryProperty.FindPropertyRelative("chance").floatValue = def;
                        int num = 2;
                        while (true)
                        {
                            switch (num)
                            {
                                case 5:
                                    return;
                                case 2:
                                    if (!RemovePage())
                                    {
                                        num = 0;
                                        continue;
                                    }

                                    return;
                                case 4:
                                    return;
                                case 1:
                                    _DispatcherProperty.DefinePage();
                                    num = 4;
                                    continue;
                                case 3:
                                    break;
                                default:
                                    goto case 1;
                            }

                            break;
                        }
                    }
                }

                [SpecialName]
                internal float IncludePage()
                {
                    return m_RegistryProperty.FindPropertyRelative("valueMin").floatValue;
                }

                [SpecialName]
                internal void RunPage(float setup)
                {
                    while (true)
                    {
                        m_RegistryProperty.FindPropertyRelative("valueMin").floatValue = setup;
                        int num = 3;
                        while (true)
                        {
                            switch (num)
                            {
                                default:
                                    return;
                                case 0:
                                    return;
                                case 2:
                                    return;
                                case 3:
                                    if (RemovePage())
                                    {
                                        return;
                                    }

                                    num = 5;
                                    continue;
                                case 1:
                                    _DispatcherProperty.DefinePage();
                                    num = 2;
                                    continue;
                                case 4:
                                    break;
                                case 5:
                                    goto case 1;
                            }

                            break;
                        }
                    }
                }

                [SpecialName]
                internal float LoginPage()
                {
                    return m_RegistryProperty.FindPropertyRelative("valueMax").floatValue;
                }

                [SpecialName]
                internal void ReflectPage(float setup)
                {
                    m_RegistryProperty.FindPropertyRelative("valueMax").floatValue = setup;
                    if (!RemovePage())
                    {
                        _DispatcherProperty.DefinePage();
                    }
                }

                [SpecialName]
                internal ChangeType CreatePage()
                {
                    return (ChangeType)m_RegistryProperty.FindPropertyRelative("type").enumValueIndex;
                }

                [SpecialName]
                internal void NewPage(ChangeType instance)
                {
                    m_RegistryProperty.FindPropertyRelative("type").enumValueIndex = (int)instance;
                    _DispatcherProperty.m_CustomerProperty.ApplyModifiedProperties();
                }

                internal AttrProperty(BridgeProperty param, SerializedProperty b)
                {
                    _DispatcherProperty = param;
                    m_RegistryProperty = b;
                }
            }

            internal StateMachineBehaviour _StrategyProperty;
            internal SerializedObject m_CustomerProperty;
            internal List<AttrProperty> m_DatabaseProperty = new List<AttrProperty>();
            internal SerializedProperty m_ExporterProperty;
            private readonly SerializedProperty _IdentifierProperty;
            [SpecialName]
            internal bool StartPage()
            {
                return _IdentifierProperty.boolValue;
            }

            [SpecialName]
            internal void ReadPage(bool instanceinstall)
            {
                _IdentifierProperty.boolValue = instanceinstall;
                DefinePage();
            }

            internal BridgeProperty(StateMachineBehaviour value)
            {
                int num = 6;
                int num2 = default(int);
                while (true)
                {
                    switch (num)
                    {
                        case 7:
                            num2++;
                            num = 3;
                            break;
                        case 2:
                            m_DatabaseProperty.Add(new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(num2)));
                            num = 7;
                            break;
                        case 5:
                            return;
                        case 4:
                            m_CustomerProperty = new SerializedObject(value);
                            num = 8;
                            break;
                        case 9:
                            num2 = 0;
                            num = 0;
                            break;
                        default:
                            if (num2 >= m_ExporterProperty.arraySize)
                            {
                                num = 5;
                                break;
                            }

                            goto case 2;
                        case 6:
                            _StrategyProperty = value;
                            num = 4;
                            break;
                        case 8:
                            m_ExporterProperty = m_CustomerProperty.FindProperty("parameters");
                            num = 1;
                            break;
                        case 1:
                            _IdentifierProperty = m_CustomerProperty.FindProperty("localOnly");
                            num = 9;
                            break;
                    }
                }
            }

            internal AttrProperty ExcludePage(int instance_Z)
            {
                return new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(instance_Z));
            }

            internal bool InitPage(int init)
            {
                m_DatabaseProperty.RemoveAt(init);
                m_ExporterProperty.DeleteArrayElementAtIndex(init);
                m_CustomerProperty.ApplyModifiedProperties();
                if (m_ExporterProperty.arraySize == 0)
                {
                    return true;
                }

                return false;
            }

            internal AttrProperty VisitPage()
            {
                while (true)
                {
                    m_ExporterProperty.InsertArrayElementAtIndex(m_ExporterProperty.arraySize);
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return ExcludePage(m_ExporterProperty.arraySize - 1);
                            case 4:
                                break;
                            case 3:
                                m_DatabaseProperty.Add(new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(m_ExporterProperty.arraySize - 1)));
                                num = 2;
                                continue;
                            case 1:
                                m_CustomerProperty.ApplyModifiedProperties();
                                num = 5;
                                continue;
                            case 2:
                                goto case 1;
                        }

                        break;
                    }
                }
            }

            internal void DefinePage()
            {
                m_CustomerProperty.ApplyModifiedProperties();
            }
        }

        private static Dictionary<string, Type> m_FilterProperty;
        private static bool stubProperty;
        private static bool readerProperty;
        [SpecialName]
        internal static Type InsertPage()
        {
            return CancelPage("VRCAvatarParameterDriver");
        }

        [SpecialName]
        internal static Type QueryPage()
        {
            return CancelPage("VRCAnimatorTrackingControl");
        }

        [SpecialName]
        internal static bool InvokePage()
        {
            if (!readerProperty)
            {
                CancelPage("VRCAvatarDescriptor");
            }

            return stubProperty;
        }

        internal static Type CancelPage(string spec)
        {
            readerProperty = true;
            if (m_FilterProperty == null)
            {
                string text = "VRCAvatarDescriptor";
                m_FilterProperty = new Dictionary<string, Type>();
                Type type = ControllerEditorUtility.ForgotRules(text);
                if (type != null)
                {
                    stubProperty = true;
                    m_FilterProperty.Add(text, type);
                }
            }

            if (stubProperty)
            {
                if (m_FilterProperty.TryGetValue(spec, out var value))
                {
                    return value;
                }

                Type type2 = ControllerEditorUtility.ForgotRules(spec);
                m_FilterProperty.Add(spec, type2);
                return type2;
            }

            return null;
        }
    }
}
