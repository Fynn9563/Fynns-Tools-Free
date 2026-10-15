using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal class InlineReorderableList<T>
    {
        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass12_0
        {
            public InlineReorderableList<T> _003C_003E4__this;
            public string title;
            public string tooltip;
        }

        internal readonly IList m_PolicyTests;
        private readonly ReorderableList m_SerializerTests;
        private object _PageTests;
        private readonly ReorderableList.ElementCallbackDelegate resolverTests;
        private readonly Action _PredicateTests;
        internal Action m_RulesTests;
        internal bool m_QueueTests;
        internal bool m_ErrorTests;
        [SpecialName]
        internal int InterruptThread()
        {
            return m_SerializerTests.index = RegisterThread(m_SerializerTests.index);
        }

        internal InlineReorderableList(Action last, IList caller, Action<ReorderableList> proc, ReorderableList.ElementCallbackDelegate attr2, ReorderableList.ElementHeightCallbackDelegate init3 = null)
        {
            m_QueueTests = true;
            _PredicateTests = last;
            m_PolicyTests = caller;
            resolverTests = attr2;
            m_SerializerTests = new ReorderableList(caller, typeof(T), draggable: true, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
            {headerHeight = 1f, footerHeight = 0f, drawElementCallback = ChangeThread, onAddCallback = proc.Invoke};
            if (init3 != null)
            {
                m_SerializerTests.elementHeightCallback = init3;
            }
        }

        internal InlineReorderableList(string info, string pol, IList pool, Action<ReorderableList> item2, ReorderableList.ElementCallbackDelegate ivk3, ReorderableList.ElementHeightCallbackDelegate token4 = null) : this((Action)null, pool, item2, ivk3, token4)
        {
            _003C_003Ec__DisplayClass12_0 CS_0024_003C_003E8__locals7 = new _003C_003Ec__DisplayClass12_0();
            CS_0024_003C_003E8__locals7.title = info;
            CS_0024_003C_003E8__locals7.tooltip = pol;
            CS_0024_003C_003E8__locals7._003C_003E4__this = this;
            _PredicateTests = delegate
            {
                CS_0024_003C_003E8__locals7._003C_003E4__this.LogoutThread(CS_0024_003C_003E8__locals7.title, CS_0024_003C_003E8__locals7.tooltip);
                CS_0024_003C_003E8__locals7._003C_003E4__this.PatchThread();
            };
        }

        private void ChangeThread(Rect param, int pol_low, bool allowcomp, bool skipfirst2)
        {
            if (m_PolicyTests.Count == 0)
            {
            }
            else if (pol_low >= 0 && pol_low < m_PolicyTests.Count)
            {
                if (GUI.Button(new Rect(param.x + param.width - 28f, param.y + param.height / 2f - 8f, 32f, 18f), ControllerEditorUtility.DestroyError().m_DecoratorProcessor, ControllerEditorUtility.CalcError()._TemplateProcessor))
                {
                    m_PolicyTests.RemoveAt(pol_low);
                    return;
                }

                Rect rect = new Rect(param);
                rect.width = param.width - 29f;
                Rect rect2 = rect;
                resolverTests(rect2, pol_low, allowcomp, skipfirst2);
            }
        }

        internal void SortThread()
        {
            bool flag = m_PolicyTests.Count == 0;
            if (m_RulesTests != null)
            {
                object obj = (flag ? null : m_PolicyTests[InterruptThread()]);
                if (obj != _PageTests)
                {
                    _PageTests = obj;
                    m_RulesTests();
                }
            }

            if (_PredicateTests != null)
            {
                EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope("RL Header");
                try
                {
                    _PredicateTests();
                }
                finally
                {
                    if (horizontalScope != null)
                    {
                        ((IDisposable)horizontalScope).Dispose();
                    }
                }
            }

            if (m_QueueTests)
            {
                if (!flag || m_ErrorTests)
                {
                    m_SerializerTests.DoLayoutList();
                }
            }
        }

        internal int RegisterThread(int var1_X)
        {
            return Mathf.Clamp(var1_X, 0, m_PolicyTests.Count - 1);
        }

        internal void LogoutThread(string def, string vis = null)
        {
            while (true)
            {
                GUILayout.Label(def, EditorStyles.boldLabel);
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            if (!string.IsNullOrEmpty(vis))
                            {
                                num = 4;
                                continue;
                            }

                            return;
                        case 1:
                            break;
                        case 2:
                            return;
                        case 5:
                            return;
                        case 3:
                            GUILayout.Label(new GUIContent(ControllerEditorUtility.DestroyError()._AccountProcessor.CompareHelper(), vis), GUILayout.Width(14f), GUILayout.Height(18f));
                            num = 2;
                            continue;
                        case 4:
                            goto case 3;
                    }

                    break;
                }
            }
        }

        internal void PatchThread(bool rejectv = true, bool writeattr = true)
        {
            EditorGUI.DisabledScope disabledScope = default(EditorGUI.DisabledScope);
            while (true)
            {
                IL_01d5:
                    int num;
                if (writeattr)
                {
                    num = 4;
                    goto IL_001d;
                }

                goto IL_024c;
                IL_004b:
                    disabledScope = new EditorGUI.DisabledScope(!m_QueueTests);
                num = 3;
                goto IL_001d;
                IL_024c:
                    if (rejectv)
                    {
                        num = 1;
                        goto IL_001d;
                    }

                goto IL_004b;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 2:
                            case 7:
                                break;
                            case 3:
                                try
                                {
                                    int num2;
                                    if (!ControllerEditorUtility.RestartQueue(EditorGUIUtility.IconContent("d_ol_plus"), GUI.skin.label, GUILayout.Width(18f)))
                                    {
                                        num2 = 0;
                                        goto IL_00d8;
                                    }

                                    goto IL_0177;
                                    IL_0118:
                                        m_SerializerTests.onAddCallback(m_SerializerTests);
                                    num2 = 3;
                                    goto IL_00d8;
                                    IL_00d8:
                                        switch (num2)
                                        {
                                            default:
                                                return;
                                            case 3:
                                                return;
                                            case 4:
                                                break;
                                            case 0:
                                                return;
                                            case 1:
                                                return;
                                            case 2:
                                                goto IL_0177;
                                        }

                                    goto IL_0118;
                                    IL_0177:
                                        ;
                                    goto IL_0118;
                                }
                                finally
                                {
                                    ((IDisposable)disabledScope /*cast due to .constrained prefix*/).Dispose();
                                }

                            case 5:
                                goto IL_01d5;
                            case 8:
                                return;
                            case 6:
                                goto IL_024c;
                            case 4:
                                GUILayout.FlexibleSpace();
                                num = 6;
                                continue;
                            default:
                                m_QueueTests = ControllerEditorUtility.ExcludeQueue(m_QueueTests, m_QueueTests ? ControllerEditorUtility.DestroyError()._RefProcessor : ControllerEditorUtility.DestroyError().m_StatusProcessor, EditorStyles.label, GUILayout.Width(18f), GUILayout.Height(18f));
                                num = 7;
                                continue;
                        }

                        break;
                    }

                goto IL_004b;
            }
        }
    }
}
