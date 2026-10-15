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
        internal class ControllerEditorSettings
        {
            internal enum StateCosmeticOptions
            {
                none = 0,
                motionName = 1,
                motionIcon = 2,
                coordinates = 4,
                indicators = 8,
                inactiveIndicators = 16,
                quickNewClip = 32,
                all = -1
            }

            internal class WorkerAlgo : IDisposable
            {
                private readonly Action _FilterAlgo;
                private readonly bool stubAlgo;
                private readonly EditorGUI.ChangeCheckScope m_ReaderAlgo;
                public WorkerAlgo(Action ident = null)
                {
                    _FilterAlgo = ident;
                    stubAlgo = ComputeDefinition();
                    MoveDefinition(isitem: true);
                    m_ReaderAlgo = new EditorGUI.ChangeCheckScope();
                }

                public void Dispose()
                {
                    while (true)
                    {
                        bool changed = m_ReaderAlgo.changed;
                        m_ReaderAlgo.Dispose();
                        int num;
                        if (!changed)
                        {
                            num = 0;
                            goto IL_001d;
                        }

                        goto IL_0126;
                        IL_001d:
                            while (true)
                            {
                                switch (num)
                                {
                                    case 7:
                                        return;
                                    default:
                                        MoveDefinition(stubAlgo);
                                        num = 7;
                                        continue;
                                    case 2:
                                    case 3:
                                        SetupDefinition();
                                        num = 5;
                                        continue;
                                    case 1:
                                        break;
                                    case 4:
                                        goto IL_00e5;
                                    case 8:
                                        goto IL_0126;
                                }

                                break;
                            }

                        continue;
                        IL_0126:
                            ;
                        goto IL_00e5;
                        IL_00e5:
                            Action filterAlgo = _FilterAlgo;
                        if (filterAlgo == null)
                        {
                            num = 2;
                        }
                        else
                        {
                            filterAlgo();
                            num = 3;
                        }

                        goto IL_001d;
                    }
                }

                public static implicit operator bool (WorkerAlgo task)
                {
                    return task.m_ReaderAlgo.changed;
                }
            }

            [Serializable]
            internal class BoolSetting : SettingValue
            {
                [SerializeField]
                private bool _value;
                internal readonly Action m_DatabaseAlgo;
                private static BoolSetting RunState;
                [SpecialName]
                internal bool FindDefinition()
                {
                    return _value;
                }

                [SpecialName]
                internal void ExcludeDefinition(bool excludeparam)
                {
                    if (_value == excludeparam)
                    {
                        return;
                    }

                    _value = excludeparam;
                    m_DatabaseAlgo?.Invoke();
                    SetupDefinition();
                }

                internal BoolSetting(bool appendlast, Action ord = null)
                {
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                                return;
                            default:
                                _value = appendlast;
                                num = 1;
                                break;
                            case 3:
                                callbackAlgo = appendlast;
                                num = 0;
                                break;
                            case 1:
                                m_DatabaseAlgo = ord;
                                num = 2;
                                break;
                        }
                    }
                }

                internal void InsertDefinition()
                {
                    ExcludeDefinition(!_value);
                }

                internal void QueryDefinition(GUIContent setup, GUIStyle visitor = null, params GUILayoutOption[] options)
                {
                    if (visitor == null)
                    {
                        visitor = EditorStyles.toggle;
                    }

                    ExcludeDefinition(EditorGUILayout.Toggle(setup, FindDefinition(), visitor, options));
                }

                internal void AddDefinition(string last, string vis = null, bool isthird = false, Color? vis2 = null, Color? x3 = null, params GUILayoutOption[] options)
                {
                    InvokeDefinition(string.IsNullOrEmpty(last) ? GUIContent.none : new GUIContent(last), string.IsNullOrEmpty(vis) ? GUIContent.none : new GUIContent(vis), isthird, vis2, x3, options);
                }

                internal void InvokeDefinition(GUIContent init, GUIContent cont = null, bool striptemplate = false, Color? counter2 = null, Color? map3 = null, params GUILayoutOption[] options)
                {
                    counter2 = counter2 ?? GUI.backgroundColor;
                    map3 = map3 ?? GUI.backgroundColor;
                    Color backgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = (FindDefinition() ? counter2.Value : map3.Value);
                    ExcludeDefinition(GUILayout.Toggle(FindDefinition(), (FindDefinition() || cont == null) ? init : cont, striptemplate ? EditorStyles.toolbarButton : GUI.skin.button, options));
                    GUI.backgroundColor = backgroundColor;
                }

                public static implicit operator bool (BoolSetting key)
                {
                    return key._value;
                }

                internal override void Reset()
                {
                    ExcludeDefinition((bool)callbackAlgo);
                }
            }

            [Serializable]
            internal class FloatSetting : SettingValue
            {
                [SerializeField]
                private float _value;
                internal readonly Action identifierAlgo;
                private static FloatSetting ConnectState;
                [SpecialName]
                internal float ResetDefinition()
                {
                    return _value;
                }

                [SpecialName]
                internal void FlushDefinition(float reference)
                {
                    while (_value != reference)
                    {
                        int num = 2;
                        while (true)
                        {
                            switch (num)
                            {
                                default:
                                    return;
                                case 6:
                                {
                                    Action action = identifierAlgo;
                                    if (action == null)
                                    {
                                        num = 5;
                                        continue;
                                    }

                                    action();
                                    num = 7;
                                    continue;
                                }

                                case 4:
                                    return;
                                case 3:
                                    break;
                                case 1:
                                    _value = reference;
                                    num = 6;
                                    continue;
                                case 5:
                                case 7:
                                    SetupDefinition();
                                    num = 0;
                                    continue;
                                case 2:
                                    goto case 1;
                            }

                            break;
                        }
                    }
                }

                internal FloatSetting(float ident, Action result = null)
                {
                    callbackAlgo = ident;
                    _value = ident;
                    identifierAlgo = result;
                }

                internal void RemoveDefinition(GUIContent last, float cfg, float temp, bool compareconfig2 = true, params GUILayoutOption[] options)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        FlushDefinition(EditorGUILayout.Slider(last, ResetDefinition(), cfg, temp, options));
                        while (true)
                        {
                            IL_014e:
                                int num;
                            if (!compareconfig2)
                            {
                                num = 3;
                                goto IL_005b;
                            }

                            goto IL_0170;
                            IL_005b:
                                while (true)
                                {
                                    switch (num)
                                    {
                                        case 1:
                                            break;
                                        case 2:
                                            Reset();
                                            num = 5;
                                            continue;
                                        case 5:
                                            return;
                                        case 3:
                                            return;
                                        case 6:
                                            return;
                                        case 4:
                                            goto IL_014e;
                                        default:
                                            goto IL_0170;
                                    }

                                    break;
                                }

                            goto IL_0081;
                            IL_0170:
                                ;
                            goto IL_0081;
                            IL_0081:
                                if (ControllerEditorUtility.CallQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor))
                                {
                                    num = 2;
                                    goto IL_005b;
                                }

                            break;
                        }
                    }
                    finally
                    {
                        if (horizontalScope != null)
                        {
                            int num2 = 3;
                            while (true)
                            {
                                switch (num2)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        goto IL_0214;
                                    case 2:
                                        break;
                                    case 3:
                                        goto IL_0214;
                                }

                                break;
                                IL_0214:
                                    ((IDisposable)horizontalScope).Dispose();
                                num2 = 2;
                            }
                        }
                    }
                }

                internal override void Reset()
                {
                    FlushDefinition((float)callbackAlgo);
                }

                public static implicit operator int (FloatSetting key)
                {
                    return (int)key._value;
                }

                public static implicit operator float (FloatSetting item)
                {
                    return item._value;
                }
            }

            [Serializable]
            internal class EnumSetting : FloatSetting
            {
                private static EnumSetting ChangeState;
                [SerializeField]
                internal int CalcDefinition
                {
                    get
                    {
                        return (int)ResetDefinition();
                    }

                    set
                    {
                        FlushDefinition(value);
                    }
                }

                internal EnumSetting(int previous_task, Action vis = null) : base(previous_task, vis)
                {
                }

                internal T CalculateDefinition<T>()
                    where T : Enum
                {
                    return (T)(object)CalcDefinition;
                }

                internal void ValidateDefinition<T>(GUIContent info, bool ismap = false, GUIStyle tag = null, params GUILayoutOption[] options)
                    where T : Enum
                {
                    if (tag == null)
                    {
                        tag = EditorStyles.popup;
                    }

                    CalcDefinition = (ismap ? ((int)(object)EditorGUILayout.EnumFlagsField(info, (T)(object)CalcDefinition, tag, options)) : ((int)(object)EditorGUILayout.EnumPopup(info, (T)(object)CalcDefinition, tag, options)));
                }

                internal void CustomizeDefinition<T>(string spec, bool hasreg = false, GUIStyle template = null, params GUILayoutOption[] options)
                    where T : Enum
                {
                    ValidateDefinition<T>(new GUIContent(spec), hasreg, template, options);
                }

                internal static EnumSetting RateDefinition<T>(T ident, Action result = null)
                    where T : Enum
                {
                    return new EnumSetting((int)(object)ident, result);
                }

                public static implicit operator int (EnumSetting param)
                {
                    return param.CalcDefinition;
                }

                public static implicit operator float (EnumSetting var1)
                {
                    return var1.CalcDefinition;
                }
            }

            [Serializable]
            internal class Vector2Setting : SettingValue
            {
                [SerializeField]
                private float _valueX;
                [SerializeField]
                private float _valueY;
                [SerializeField]
                private float _valueZ;
                internal Action _RegistryAlgo;
                internal bool _TagAlgo;
                internal Vector3 importerAlgo;
                private static Vector2Setting GetState;
                [SpecialName]
                internal Vector3 DeleteDefinition()
                {
                    while (true)
                    {
                        IL_00d1:
                            int num;
                        if (_TagAlgo)
                        {
                            num = 0;
                            goto IL_001d;
                        }

                        goto IL_0106;
                        IL_001d:
                            while (true)
                            {
                                switch (num)
                                {
                                    case 3:
                                        break;
                                    default:
                                        return importerAlgo;
                                    case 5:
                                        importerAlgo = new Vector3(_valueX, _valueY, _valueZ);
                                        num = 2;
                                        continue;
                                    case 1:
                                        goto IL_00d1;
                                    case 6:
                                        goto IL_0106;
                                }

                                break;
                            }

                        goto IL_0070;
                        IL_0106:
                            ;
                        goto IL_0070;
                        IL_0070:
                            _TagAlgo = true;
                        num = 5;
                        goto IL_001d;
                    }
                }

                [SpecialName]
                internal void CreateDefinition(Vector3 value)
                {
                    if (importerAlgo != value)
                    {
                        importerAlgo = value;
                        _valueX = value.x;
                        _valueY = value.y;
                        _valueZ = value.z;
                        _RegistryAlgo?.Invoke();
                        SetupDefinition();
                    }
                }

                internal void IncludeDefinition(Vector3 config, Action ord)
                {
                    callbackAlgo = config;
                    _RegistryAlgo = ord;
                    _valueX = config.x;
                    _valueY = config.y;
                    _valueZ = config.z;
                }

                internal Vector2Setting(Vector3 value, Action selection = null)
                {
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
                                return;
                            case 2:
                                break;
                            case 3:
                                break;
                        }

                        IncludeDefinition(value, selection);
                        num = 1;
                    }
                }

                internal Vector2Setting(float init, float token, float pool, Action second2 = null)
                {
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 1:
                                return;
                        }

                        IncludeDefinition(new Vector3(init, token, pool), second2);
                        num = 1;
                    }
                }

                internal Vector2Setting(float asset, float ivk, Action tag = null)
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                return;
                            case 1:
                                IncludeDefinition(new Vector3(asset, ivk), tag);
                                num = 0;
                                break;
                            case 0:
                                return;
                        }
                    }
                }

                internal void RunDefinition(GUIContent init, params GUILayoutOption[] options)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        GUILayout.Label(init, GUILayout.MaxWidth(117f));
                        CreateDefinition(EditorGUILayout.Vector2Field(GUIContent.none, DeleteDefinition(), options));
                        if (GUILayout.Button(ControllerEditorUtility.DestroyError()._CallbackProcessor, ControllerEditorUtility.CalcError().m_ServiceProcessor, GUILayout.Width(18f), GUILayout.Height(18f)))
                        {
                            Reset();
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

                internal void CloneDefinition(string key, params GUILayoutOption[] options)
                {
                    RunDefinition(new GUIContent(key), options);
                }

                internal override void Reset()
                {
                    CreateDefinition((Vector3)callbackAlgo);
                }

                public static implicit operator Vector3(Vector2Setting v)
                {
                    return v.DeleteDefinition();
                }

                public static implicit operator Vector2(Vector2Setting asset)
                {
                    return asset.DeleteDefinition();
                }
            }

            [Serializable]
            internal class StringSetting : SettingValue
            {
                [SerializeField]
                private string _value;
                internal readonly Action printerAlgo;
                private static StringSetting StopState;
                [SpecialName]
                internal string CollectDefinition()
                {
                    return _value;
                }

                [SpecialName]
                internal void ResolveDefinition(string value)
                {
                    while (_value != value)
                    {
                        int num = 6;
                        while (true)
                        {
                            switch (num)
                            {
                                default:
                                    SetupDefinition();
                                    num = 2;
                                    continue;
                                case 1:
                                    return;
                                case 5:
                                {
                                    Action action = printerAlgo;
                                    if (action == null)
                                    {
                                        num = 0;
                                        continue;
                                    }

                                    action();
                                    num = 3;
                                    continue;
                                }

                                case 2:
                                    return;
                                case 7:
                                    break;
                                case 4:
                                    _value = value;
                                    num = 5;
                                    continue;
                                case 6:
                                    goto case 4;
                            }

                            break;
                        }
                    }
                }

                internal StringSetting(string last = "", Action pred = null)
                {
                    callbackAlgo = last;
                    _value = last;
                    printerAlgo = pred;
                }

                internal void ViewDefinition(GUIContent def, bool setcust = true, bool deletehelper = true, GUIStyle counter2 = null, params GUILayoutOption[] options)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        if (counter2 == null)
                        {
                            counter2 = EditorStyles.textField;
                        }

                        ResolveDefinition(deletehelper ? EditorGUILayout.DelayedTextField(def, CollectDefinition(), counter2, options) : EditorGUILayout.TextField(def, CollectDefinition(), counter2, options));
                        if (setcust && ControllerEditorUtility.CallQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor))
                        {
                            Reset();
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

                internal override void Reset()
                {
                    ResolveDefinition((string)callbackAlgo);
                }

                public override string ToString()
                {
                    return CollectDefinition();
                }

                public static implicit operator string (StringSetting res)
                {
                    return res._value;
                }
            }

            [Serializable]
            internal class ColorSetting : SettingValue
            {
                internal readonly Action m_ParamsAlgo;
                [SerializeField]
                private float r;
                [SerializeField]
                private float g;
                [SerializeField]
                private float b;
                [SerializeField]
                private float a;
                private static ColorSetting RateState;
                [SpecialName]
                internal Color WriteDefinition()
                {
                    return new Color(r, g, b, a);
                }

                [SpecialName]
                internal void ForgotDefinition(Color value)
                {
                    r = value.r;
                    g = value.g;
                    b = value.b;
                    a = value.a;
                    m_ParamsAlgo?.Invoke();
                    SetupDefinition();
                }

                internal ColorSetting(float item, float second, float state, float visitor2 = 1f, Action res3 = null)
                {
                    Color color = new Color(item, second, state, visitor2);
                    callbackAlgo = color;
                    r = item;
                    g = second;
                    b = state;
                    a = visitor2;
                    m_ParamsAlgo = res3;
                }

                internal ColorSetting(Color info, Action cont = null)
                {
                    callbackAlgo = info;
                    r = info.r;
                    g = info.g;
                    b = info.b;
                    a = info.a;
                    m_ParamsAlgo = cont;
                }

                internal void VerifyDefinition(string setup, bool isvis = true, params GUILayoutOption[] options)
                {
                    FillDefinition(new GUIContent(setup), isvis, options);
                }

                internal void FillDefinition(GUIContent task, bool outputtoken = true, params GUILayoutOption[] options)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        ForgotDefinition(EditorGUILayout.ColorField(task, WriteDefinition(), options));
                        if (outputtoken && ControllerEditorUtility.CallQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor))
                        {
                            Reset();
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

                internal override void Reset()
                {
                    ForgotDefinition((Color)callbackAlgo);
                }

                public static implicit operator Color(ColorSetting spec)
                {
                    return spec.WriteDefinition();
                }
            }

            [Serializable]
            internal class ObjectSetting : SettingValue
            {
                internal readonly Action _GetterAlgo;
                private readonly Type interceptorAlgo;
                [SerializeField]
                internal string guid;
                [SerializeField]
                internal long localID;
                private string _CreatorAlgo;
                private long _EventAlgo;
                private bool m_InfoAlgo;
                private UnityEngine.Object m_FacadeAlgo;
                internal static ObjectSetting DefineState;
                [SpecialName]
                internal UnityEngine.Object ChangeDefinition()
                {
                    if (!m_InfoAlgo)
                    {
                        m_InfoAlgo = true;
                        m_FacadeAlgo = AssetDefinition<UnityEngine.Object>(guid, localID);
                    }

                    return m_FacadeAlgo;
                }

                [SpecialName]
                internal void SortDefinition(UnityEngine.Object reference)
                {
                    if (m_FacadeAlgo != reference)
                    {
                        m_FacadeAlgo = reference;
                        if (!(reference == null))
                        {
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out guid, out localID);
                        }
                        else
                        {
                            guid = string.Empty;
                            localID = 0L;
                        }

                        _GetterAlgo?.Invoke();
                        SetupDefinition();
                    }
                }

                internal ObjectSetting(Type asset, string ivk = "", long column_comp = 0L, Action pol2 = null)
                {
                    interceptorAlgo = asset;
                    _CreatorAlgo = ivk;
                    _EventAlgo = column_comp;
                    guid = ivk;
                    localID = column_comp;
                    _GetterAlgo = pol2;
                }

                internal void CheckDefinition(string item, bool istoken = true, params GUILayoutOption[] options)
                {
                    PrepareDefinition(new GUIContent(item), istoken, options);
                }

                internal void PrepareDefinition(GUIContent def, bool isb = true, params GUILayoutOption[] options)
                {
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        SortDefinition(EditorGUILayout.ObjectField(def, ChangeDefinition(), interceptorAlgo, allowSceneObjects: false, options));
                        if (isb && ControllerEditorUtility.CallQueue(ControllerEditorUtility.DestroyError()._CallbackProcessor))
                        {
                            Reset();
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

                private static T AssetDefinition<T>(string key, long indexOf_pol)
                    where T : UnityEngine.Object
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        return null;
                    }

                    if (indexOf_pol != 0L)
                    {
                        UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(key));
                        foreach (UnityEngine.Object obj in array)
                        {
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long localId);
                            if (localId == indexOf_pol)
                            {
                                return (T)obj;
                            }
                        }

                        return null;
                    }

                    return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(key));
                }

                internal T UpdateDefinition<T>()
                    where T : UnityEngine.Object
                {
                    return (T)ChangeDefinition();
                }

                internal override void Reset()
                {
                    SortDefinition(AssetDefinition<UnityEngine.Object>(_CreatorAlgo, _EventAlgo));
                }

                public static implicit operator bool (ObjectSetting last)
                {
                    return last.ChangeDefinition();
                }
            }

            internal abstract class SettingValue
            {
                internal object callbackAlgo;
                internal abstract void Reset();
            }

            [AttributeUsage(AttributeTargets.Field)]
            internal class IndexerAlgo : Attribute
            {
            }

            [SerializeField]
            internal BoolSetting editingTransitions = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting editingStates = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting editingController = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting matchParameter = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting matchMode = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting matchValue = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting showTransitionSettings = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting showTransitionConditions = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting showMatchingOptions = new BoolSetting(appendlast: false, UpdateVisitor);
            [SerializeField]
            internal BoolSetting showTransitionsCount = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting showStateSettings = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting showStateCount = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting showVRCDrivers = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting showVRCTracking = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting useLegacyDropdown = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting switchDoubleClick = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting autoReverseModes = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting reverseModifiesValues = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting animateInboundEdges = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting animateOutboundEdges = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting autoFrameLayer = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting displayLayerIndex = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting displayParameterType = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting capitalParameterIndicator = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting aw_active = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting aw_autoSwitchClip = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting aw_enablePropertyEditing = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting aw_enableGameObjectDND = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting aw_enableOverride = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting aw_warnPropertyMerge = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting graphBackgroundIsTexture = new BoolSetting(appendlast: false, SortAlgo);
            [SerializeField]
            internal BoolSetting cosmeticGraphActive = new BoolSetting(appendlast: false, SortAlgo);
            [SerializeField]
            internal BoolSetting cosmeticNodesActive = new BoolSetting(appendlast: false, PatchAlgo);
            [SerializeField]
            internal BoolSetting cosmeticTransitionsActive = new BoolSetting(appendlast: false, PatchAlgo);
            [SerializeField]
            internal BoolSetting hasPingedController = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting requiresStateRename = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting advancedQuickToggle = new BoolSetting(appendlast: false);
            [SerializeField]
            internal BoolSetting mergeQuickToggle = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting warnParameterConversion = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting displayCategoryView = new BoolSetting(appendlast: true, delegate
            {
                AddInitializer().sortCategoryViewLayers.ExcludeDefinition(excludeparam: false);
                writerVisitor = LayerViewViewType.DefaultView;
            });
            [SerializeField]
            internal BoolSetting sortCategoryViewLayers = new BoolSetting(appendlast: true);
            [SerializeField]
            internal BoolSetting displayLayerCompactView = new BoolSetting(appendlast: true, delegate
            {
                AddInitializer().layerCompactView.ExcludeDefinition(excludeparam: false);
            });
            [SerializeField]
            internal BoolSetting layerCompactView = new BoolSetting(appendlast: false, PublishAnnotation);
            [SerializeField]
            internal FloatSetting anyStateNodeColor = new FloatSetting(2f, PatchAlgo);
            [SerializeField]
            internal FloatSetting entryStateNodeColor = new FloatSetting(3f, PatchAlgo);
            [SerializeField]
            internal FloatSetting exitStateNodeColor = new FloatSetting(6f, PatchAlgo);
            [SerializeField]
            internal FloatSetting machineStateNodeColor = new FloatSetting(0f, PatchAlgo);
            [SerializeField]
            internal FloatSetting normalStateNodeColor = new FloatSetting(0f, PatchAlgo);
            [SerializeField]
            internal FloatSetting defaultStateNodeColor = new FloatSetting(5f, PatchAlgo);
            [SerializeField]
            internal FloatSetting defaultLayerWeight = new FloatSetting(1f);
            [SerializeField]
            internal FloatSetting arrowLerpRatio = new FloatSetting(-0.5f);
            [SerializeField]
            internal Vector2Setting defaultEntryPosition = new Vector2Setting(50f, 120f);
            [SerializeField]
            internal Vector2Setting defaultExitPosition = new Vector2Setting(800f, 120f);
            [SerializeField]
            internal Vector2Setting defaultAnyPosition = new Vector2Setting(50f, 20f);
            [SerializeField]
            internal ColorSetting normalTransitionColor = new ColorSetting(1f, 1f, 1f);
            [SerializeField]
            internal ColorSetting entryTransitionColor = new ColorSetting(0.6f, 0.4f, 0f);
            [SerializeField]
            internal ColorSetting selectedTransitionColor = new ColorSetting(0.42f, 0.7f, 1f);
            [SerializeField]
            internal ColorSetting baseTransitionColor = new ColorSetting(0.5f, 0.5f, 0.5f);
            [SerializeField]
            internal ColorSetting gridBackgroundColor = new ColorSetting(0.1647f, 0.1647f, 0.16f, 1f, SortAlgo);
            [SerializeField]
            internal ColorSetting gridMinorLightColor = new ColorSetting(0f, 0f, 0f, 0.1f);
            [SerializeField]
            internal ColorSetting gridMajorLightColor = new ColorSetting(0f, 0f, 0f, 0.15f);
            [SerializeField]
            internal ColorSetting gridMinorDarkColor = new ColorSetting(0f, 0f, 0f, 0.18f);
            [SerializeField]
            internal ColorSetting gridMajorDarkColor = new ColorSetting(0f, 0f, 0f, 0.28f);
            [SerializeField]
            internal ColorSetting parameterLabelColor = new ColorSetting(0.7f, 0.7f, 0.7f);
            [SerializeField]
            internal ObjectSetting defaultLayerMask = new ObjectSetting(typeof(AvatarMask), "", 0L);
            [SerializeField]
            internal ObjectSetting graphBackgroundTexture = new ObjectSetting(typeof(Texture2D), "", 0L, SortAlgo);
            [SerializeField]
            internal StringSetting saveFolder = new StringSetting("Assets/Fynn's Tools/Generated/Controller Editor");
            [SerializeField]
            internal StringSetting lastAnimationPath = new StringSetting("Assets");
            [SerializeField]
            internal StringSetting lastAnimationName = new StringSetting("New Animation Clip");
            [SerializeField]
            internal StringSetting categoryBaseName = new StringSetting("Base");
            [SerializeField]
            internal StringSetting categoryDelimiter = new StringSetting("/");
            [SerializeField]
            internal EnumSetting parameterLabelFontStyle = EnumSetting.RateDefinition(FontStyle.Normal, SetDefinition);
            [SerializeField]
            internal EnumSetting stateCosmetics = EnumSetting.RateDefinition(StateCosmeticOptions.all);
            [IndexerAlgo]
            internal AnimatorState defaultState;
            [IndexerAlgo]
            internal AnimatorStateTransition defaultTransition;
            [NonSerialized]
            internal static GUIStyle m_AdapterAlgo;
            private static bool _InterpreterAlgo;
            private static bool watcherAlgo;
            private static bool _CandidateAlgo;
            private static FieldInfo[] _ProductAlgo;
            private static ControllerEditorSettings m_ExpressionAlgo;
            internal static Action _SystemAlgo;
            private static ControllerEditorSettings RegisterState;
            internal static void SetDefinition()
            {
                m_AdapterAlgo = new GUIStyle(ControllerEditorUtility.CalcError().algoObserver)
                {fontStyle = AddInitializer().parameterLabelFontStyle.CalculateDefinition<FontStyle>()};
            }

            internal StateCosmeticOptions PostDefinition()
            {
                return stateCosmetics.CalculateDefinition<StateCosmeticOptions>();
            }

            [SpecialName]
            internal static bool ComputeDefinition()
            {
                return _CandidateAlgo;
            }

            [SpecialName]
            internal static void MoveDefinition(bool isitem)
            {
                bool candidateAlgo = _CandidateAlgo;
                _CandidateAlgo = isitem;
                if (candidateAlgo)
                {
                    if (!_CandidateAlgo && watcherAlgo)
                    {
                        SetupDefinition();
                    }
                }
            }

            [SpecialName]
            internal static ControllerEditorSettings CallDefinition()
            {
                while (m_ExpressionAlgo == null)
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                EnableDefinition();
                                num = 0;
                                continue;
                            case 2:
                                goto end_IL_001d;
                            case 1:
                                goto case 4;
                        }

                        goto end_IL_0084;
                        continue;
                        end_IL_001d:
                            break;
                    }

                    continue;
                    end_IL_0084:
                        break;
                }

                return m_ExpressionAlgo;
            }

            private ControllerEditorSettings()
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
                            _ProductAlgo = (
                                from m in typeof(ControllerEditorSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                where m.IsDefined(typeof(IndexerAlgo), inherit: false)select m).ToArray();
                            num = 0;
                            break;
                    }
                }
            }

            internal static void SetupDefinition()
            {
                watcherAlgo = false;
                if (ComputeDefinition())
                {
                    watcherAlgo = true;
                }
                else
                {
                    if (_InterpreterAlgo)
                    {
                        return;
                    }

                    StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(CallDefinition()) + "]\u200b\u200b\u200b");
                    FieldInfo[] productAlgo = _ProductAlgo;
                    int num = 0;
                    while (num < productAlgo.Length)
                    {
                        FieldInfo fieldInfo = productAlgo[num];
                        try
                        {
                            string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(CallDefinition()));
                            stringBuilder.Append(fieldInfo.Name + "[" + text + "]\u200b\u200b\u200b");
                        }
                        catch (System.Exception message)
                        {
                            UnityEngine.Debug.LogError(message);
                        }

                        num++;
                    }

                    string value = stringBuilder.ToString();
                    EditorPrefs.SetString("FynnsTools.ControllerEditor.Settings", value);
                }
            }

            private static void EnableDefinition()
            {
                string text = string.Empty;
                if (EditorPrefs.HasKey("FynnsTools.ControllerEditor.Settings"))
                {
                    text = EditorPrefs.GetString("FynnsTools.ControllerEditor.Settings", string.Empty);
                }

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(text))
                {
                    MatchCollection matchCollection = Regex.Matches(text, "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B");
                    for (int i = 0; i < matchCollection.Count; i++)
                    {
                        Match match = matchCollection[i];
                        dictionary.Add(match.Groups[1].Value, match.Groups[2].Value);
                    }
                }

                if (dictionary.TryGetValue("MAIN", out var value))
                {
                    m_ExpressionAlgo = JsonUtility.FromJson<ControllerEditorSettings>(value);
                }

                if (m_ExpressionAlgo == null)
                {
                    m_ExpressionAlgo = new ControllerEditorSettings();
                }

                FieldInfo[] productAlgo = _ProductAlgo;
                foreach (FieldInfo fieldInfo in productAlgo)
                {
                    object obj = fieldInfo.GetValue(m_ExpressionAlgo) ?? Activator.CreateInstance(fieldInfo.FieldType);
                    if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
                    {
                        EditorJsonUtility.FromJsonOverwrite(value2, obj);
                    }

                    fieldInfo.SetValue(m_ExpressionAlgo, obj);
                    if (fieldInfo.GetValue(m_ExpressionAlgo) == null)
                    {
                        fieldInfo.SetValue(m_ExpressionAlgo, Activator.CreateInstance(fieldInfo.FieldType));
                    }
                }
            }
        }
    }
}
