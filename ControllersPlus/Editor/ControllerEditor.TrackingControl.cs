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
        internal struct TrackingControlEditor
        {
            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass14_0
            {
                public int recordAlgo;
            }

            [CompilerGenerated]
            internal sealed class _003C_003Ec__DisplayClass15_0
            {
                public List<SerializedProperty> helperAlgo;
            }

            private readonly SerializedObject _PropertyAlgo;
            private SerializedProperty m_ProcessorAlgo;
            private SerializedProperty m_ObserverAlgo;
            private SerializedProperty _ServerAlgo;
            private SerializedProperty threadAlgo;
            private SerializedProperty _PolicyAlgo;
            private SerializedProperty serializerAlgo;
            private SerializedProperty m_PageAlgo;
            private SerializedProperty m_ResolverAlgo;
            private SerializedProperty m_PredicateAlgo;
            private SerializedProperty _RulesAlgo;
            private readonly List<SerializedProperty> queueAlgo;
            private readonly List<GUIContent> _ErrorAlgo;
            internal TrackingControlEditor(StateMachineBehaviour[] init) : this()
            {
                int num = 5;
                while (true)
                {
                    switch (num)
                    {
                        case 9:
                            queueAlgo = new List<SerializedProperty>{m_ProcessorAlgo, m_ObserverAlgo, _ServerAlgo, threadAlgo, _PolicyAlgo, serializerAlgo, m_PageAlgo, m_ResolverAlgo, m_PredicateAlgo, _RulesAlgo};
                            num = 3;
                            break;
                        case 11:
                            return;
                        case 5:
                            _PropertyAlgo = new SerializedObject(init);
                            m_ProcessorAlgo = _PropertyAlgo.FindProperty("trackingHead");
                            num = 4;
                            break;
                        case 4:
                            m_ObserverAlgo = _PropertyAlgo.FindProperty("trackingLeftHand");
                            num = 1;
                            break;
                        case 1:
                            _ServerAlgo = _PropertyAlgo.FindProperty("trackingRightHand");
                            num = 2;
                            break;
                        case 2:
                            threadAlgo = _PropertyAlgo.FindProperty("trackingHip");
                            num = 0;
                            break;
                        default:
                            _PolicyAlgo = _PropertyAlgo.FindProperty("trackingLeftFoot");
                            num = 6;
                            break;
                        case 6:
                            serializerAlgo = _PropertyAlgo.FindProperty("trackingRightFoot");
                            num = 7;
                            break;
                        case 7:
                            m_PageAlgo = _PropertyAlgo.FindProperty("trackingLeftFingers");
                            m_ResolverAlgo = _PropertyAlgo.FindProperty("trackingRightFingers");
                            num = 10;
                            break;
                        case 10:
                            m_PredicateAlgo = _PropertyAlgo.FindProperty("trackingEyes");
                            goto case 8;
                        case 8:
                            _RulesAlgo = _PropertyAlgo.FindProperty("trackingMouth");
                            goto case 9;
                        case 3:
                            _ErrorAlgo = new List<GUIContent>(queueAlgo.Select((SerializedProperty t) => new GUIContent(t.displayName.Replace("Tracking ", string.Empty), t.tooltip)));
                            return;
                    }
                }
            }

            private void ManageInitializer(int length_first)
            {
                while (true)
                {
                    _003C_003Ec__DisplayClass14_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass14_0();
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                queueAlgo.ForEach(delegate (SerializedProperty p)
                                {
                                    p.enumValueIndex = CS_0024_003C_003E8__locals2.recordAlgo;
                                });
                                num = 2;
                                continue;
                            case 2:
                                return;
                            case 1:
                                CS_0024_003C_003E8__locals2.recordAlgo = length_first;
                                num = 0;
                                continue;
                            case 4:
                                break;
                            case 3:
                                goto case 1;
                        }

                        break;
                    }
                }
            }

            internal void PrintInitializer()
            {
                EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count < 1);
                _PropertyAlgo.Update();
                GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope("helpbox");
                try
                {
                    Color[] colors = new Color[4]{new Color(0.7f, 0.7f, 0.7f), Color.green, Color.yellow, Color.cyan};
                    GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        GUILayout.Label("Tracking Control");
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.BG, Color.red);
                        try
                        {
                            if (ControllerEditorUtility.RestartQueue(ControllerEditorUtility.CalcError().fieldProcessor, ControllerEditorUtility.CalcError().configProcessor, GUILayout.Width(25f), GUILayout.Height(20f)))
                            {
                                m_AlgoAnnotation.InvokeResolver(delegate (AnimatorState s)
                                {
                                    s.RemoveBehavioursOfType(SerializedPropertyGroup.QueryPage(), isstate: true);
                                });
                                m_ConnectionAnnotation = false;
                                return;
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

                    horizontalScope = new GUILayout.HorizontalScope();
                    try
                    {
                        _003C_003Ec__DisplayClass15_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass15_0();
                        CS_0024_003C_003E8__locals2.helperAlgo = queueAlgo;
                        int num = (queueAlgo.All(delegate (SerializedProperty p)
                        {
                            if (!p.hasMultipleDifferentValues)
                            {
                                return p.enumValueIndex == CS_0024_003C_003E8__locals2.helperAlgo[0].enumValueIndex;
                            }

                            return false;
                        }) ? queueAlgo[0].enumValueIndex : 3);
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, num, colors);
                        try
                        {
                            GUILayout.HorizontalScope horizontalScope2 = new GUILayout.HorizontalScope();
                            try
                            {
                                if (ControllerEditorUtility.CountQueue("All", GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
                                {
                                    int length_first = ((Event.current.button != 0) ? ((num != 2) ? 2 : 0) : ((num != 1) ? 1 : 0));
                                    ManageInitializer(length_first);
                                }

                                GUILayout.FlexibleSpace();
                                EditorGUI.showMixedValue = num == 3;
                                EditorGUI.BeginChangeCheck();
                                num = EditorGUILayout.Popup(num, queueAlgo[0].enumDisplayNames, GUILayout.Width(260f));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    ManageInitializer(num);
                                }

                                EditorGUI.showMixedValue = false;
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

                    ControllerEditorUtility.MapQueue();
                    int num2 = 0;
                    while (num2 < queueAlgo.Count)
                    {
                        SerializedProperty serializedProperty = queueAlgo[num2];
                        int visZ = (serializedProperty.hasMultipleDifferentValues ? 3 : serializedProperty.enumValueIndex);
                        GuiColorScope templateThread = new GuiColorScope(GuiColorScope.ColoringType.FG, visZ, colors);
                        try
                        {
                            horizontalScope = new GUILayout.HorizontalScope();
                            try
                            {
                                if (ControllerEditorUtility.RestartQueue(_ErrorAlgo[num2], GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
                                {
                                    bool flag = Event.current.button == 0;
                                    serializedProperty.enumValueIndex = (flag ? ((serializedProperty.enumValueIndex != 1) ? 1 : 0) : ((serializedProperty.enumValueIndex != 2) ? 2 : 0));
                                }

                                GUILayout.FlexibleSpace();
                                EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, GUILayout.Width(260f));
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
                            if (templateThread != null)
                            {
                                ((IDisposable)templateThread).Dispose();
                            }
                        }

                        num2++;
                    }
                }
                finally
                {
                    if (verticalScope != null)
                    {
                        ((IDisposable)verticalScope).Dispose();
                    }
                }

                _PropertyAlgo.ApplyModifiedProperties();
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
