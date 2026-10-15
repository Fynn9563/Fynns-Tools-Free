using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal abstract class CustomUtilityWindow<T> : EditorWindow where T : CustomUtilityWindow<T>
    {
        private static readonly PropertyInfo m_StatusPolicy = typeof(EditorWindow).GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
        private T tokenPolicy;
        private bool _CodePolicy;
        internal bool m_DicPolicy = true;
        internal string invocationPolicy;
        private Vector2 m_RolePolicy;
        internal new abstract string title { get; }

        internal static T CloneHelper(bool compareparam = true, string vis = "")
        {
            ReflectHelper();
            T val = ScriptableObject.CreateInstance<T>();
            val.titleContent.text = val.title;
            val._CodePolicy = compareparam;
            val.tokenPolicy = val;
            val.invocationPolicy = vis;
            return val;
        }

        private void OnGUI()
        {
            ScrollViewScope specificationThread = default(ScrollViewScope);
            EditorGUI.DisabledScope disabledScope = default(EditorGUI.DisabledScope);
            while (true)
            {
                int num;
                if (!(tokenPolicy == null))
                {
                    num = 6;
                    goto IL_001d;
                }

                goto IL_0420;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 7:
                                break;
                            case 9:
                                goto IL_007e;
                            default:
                                specificationThread = new ScrollViewScope(ref m_RolePolicy);
                                num = 2;
                                continue;
                            case 3:
                                try
                                {
                                    if (!ControllerEditorUtility.DisableQueue("Confirm"))
                                    {
                                        return;
                                    }

                                    int num4 = 1;
                                    while (true)
                                    {
                                        switch (num4)
                                        {
                                            case 3:
                                                return;
                                            case 2:
                                                return;
                                            case 1:
                                                break;
                                        }

                                        DeleteHelper();
                                        num4 = 3;
                                    }
                                }
                                finally
                                {
                                    ((IDisposable)disabledScope /*cast due to .constrained prefix*/).Dispose();
                                }

                            case 4:
                                return;
                            case 5:
                                return;
                            case 10:
                                if (!_CodePolicy)
                                {
                                    return;
                                }

                                num = 1;
                                continue;
                            case 2:
                                try
                                {
                                    int num2;
                                    if (!string.IsNullOrEmpty(invocationPolicy))
                                    {
                                        num2 = 2;
                                        goto IL_0242;
                                    }

                                    goto IL_0260;
                                    IL_0260:
                                        OnCustomGUI();
                                    num2 = 3;
                                    goto IL_0242;
                                    IL_0242:
                                        while (true)
                                        {
                                            switch (num2)
                                            {
                                                case 4:
                                                    break;
                                                default:
                                                    goto IL_0270;
                                                case 3:
                                                    goto end_IL_0242;
                                                case 1:
                                                    goto end_IL_0242;
                                                case 2:
                                                    goto IL_0270;
                                            }

                                            goto IL_0260;
                                            IL_0270:
                                                EditorGUILayout.HelpBox(invocationPolicy, MessageType.Info);
                                            num2 = 4;
                                            continue;
                                            end_IL_0242:
                                                break;
                                        }
                                }
                                finally
                                {
                                    int num3;
                                    if (specificationThread == null)
                                    {
                                        num3 = 2;
                                        goto IL_033b;
                                    }

                                    goto IL_03cb;
                                    IL_03a9:
                                        ((IDisposable)specificationThread).Dispose();
                                    num3 = 1;
                                    goto IL_033b;
                                    IL_033b:
                                        switch (num3)
                                        {
                                            case 3:
                                                break;
                                            case 2:
                                                break;
                                            case 1:
                                                break;
                                            case 4:
                                                goto IL_03a9;
                                            default:
                                                goto IL_03cb;
                                        }

                                    goto end_IL_0326;
                                    IL_03cb:
                                        ;
                                    goto IL_03a9;
                                    end_IL_0326:
                                        ;
                                }

                                goto case 10;
                            case 1:
                                disabledScope = new EditorGUI.DisabledScope(!m_DicPolicy);
                                num = 3;
                                continue;
                            case 8:
                                goto IL_0420;
                        }

                        break;
                    }

                continue;
                IL_0420:
                    ;
                goto IL_007e;
                IL_007e:
                    Close();
                num = 5;
                goto IL_001d;
            }
        }

        internal void LoginHelper(Vector2 setup, Vector2 reg)
        {
            ShowUtility();
            base.position = new Rect(new Vector2(setup.x, setup.y), reg);
        }

        internal static void ReflectHelper()
        {
            T[] array = Resources.FindObjectsOfTypeAll<T>();
            int num = 0;
            while (num < array.Length)
            {
                T val = array[num];
                try
                {
                    val.Close();
                }
                catch
                {
                    UnityEngine.Object.DestroyImmediate(val);
                }

                num++;
            }
        }

        internal void DeleteHelper()
        {
            OnCustomConfirm();
            Close();
        }

        internal abstract void OnCustomGUI();
        internal abstract void OnCustomConfirm();
    }
}
