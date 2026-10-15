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
        internal static class MotionAssetOperations
        {
            [MenuItem("CONTEXT/AnimatorState/Motion/Embed", true)]
            private static bool QueryReg(MenuCommand item)
            {
                return InvokeReg(item.context as AnimatorState);
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Embed")]
            private static void AddReg(MenuCommand res)
            {
                FindReg(res.context as AnimatorState);
            }

            private static bool InvokeReg(AnimatorState instance)
            {
                if (instance != null)
                {
                    if (instance.motion != null)
                    {
                        return !ResetReg(instance.motion);
                    }
                }

                return false;
            }

            private static void FindReg(AnimatorState setup)
            {
                if (InvokeReg(setup))
                    ControllerMotionAssets.Embed(setup);
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Extract", true)]
            private static bool ExcludeReg(MenuCommand config)
            {
                return InitReg(config.context as AnimatorState);
            }

            private static bool InitReg(AnimatorState i)
            {
                while (i != null)
                {
                    int num = 0;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                if (!(i.motion != null))
                                {
                                    num = 2;
                                    continue;
                                }

                                goto case 5;
                            case 2:
                            case 3:
                                break;
                            case 1:
                                goto end_IL_001d;
                            case 5:
                                return ResetReg(i.motion);
                            default:
                                goto case 4;
                        }

                        goto end_IL_00a4;
                        continue;
                        end_IL_001d:
                            break;
                    }

                    continue;
                    end_IL_00a4:
                        break;
                }

                return false;
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Extract")]
            private static void VisitReg(MenuCommand first)
            {
                DefineReg(first.context as AnimatorState);
            }

            private static void DefineReg(AnimatorState asset)
            {
                if (InitReg(asset))
                    ControllerMotionAssets.Extract(asset);
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Rename", true)]
            private static bool ReadReg(MenuCommand ident)
            {
                while (true)
                {
                    AnimatorState animatorState = ident.context as AnimatorState;
                    int num = 3;
                    while (true)
                    {
                        switch (num)
                        {
                            case 4:
                                break;
                            case 1:
                            case 2:
                                return false;
                            default:
                                if (!(animatorState != null))
                                {
                                    goto IL_00d1;
                                }

                                return animatorState.motion != null;
                            case 3:
                                goto default;
                        }

                        break;
                        IL_00d1:
                            num = 2;
                    }
                }
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Rename")]
            private static void SelectReg(MenuCommand res)
            {
                AnimatorState animatorState = res.context as AnimatorState;
                if (!(animatorState == null) && !(animatorState.motion == null))
                {
                    RemoveReg(animatorState.motion);
                }
            }

            private static void RemoveReg(Motion setup)
            {
                if (setup == null)
                {
                    return;
                }

                MotionRenameWindow window = EditorWindow.GetWindow<MotionRenameWindow>(utility: true, "Motion Rename");
                window._ManagerMapper.Add(setup);
                if (window._ManagerMapper.Count != 1)
                {
                    return;
                }

                window._ItemMapper = setup.name;
                EditorWindow[] array = Resources.FindObjectsOfTypeAll<EditorWindow>();
                int num = 0;
                EditorWindow editorWindow;
                while (true)
                {
                    if (num >= array.Length)
                    {
                        return;
                    }

                    editorWindow = array[num];
                    if (!(editorWindow != null) || !(editorWindow.GetType().Name == "InspectorWindow"))
                    {
                        num++;
                        continue;
                    }

                    break;
                }

                Vector2 position = editorWindow.position.position + new Vector2(0f, 50f);
                Vector2 size = (window.maxSize = (window.minSize = new Vector2(300f, 50f)));
                window.position = new Rect(position, size);
            }

            internal static void InstantiateReg()
            {
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            internal static string AwakeReg(string init)
            {
                string text = Regex.Escape(new string (Path.GetInvalidFileNameChars()));
                if (!string.IsNullOrEmpty(init))
                {
                    return Regex.Replace(init, "[" + text + "]", "-");
                }

                return "Unnamed";
            }

            internal static bool ResetReg(UnityEngine.Object spec)
            {
                string assetPath = AssetDatabase.GetAssetPath(spec);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return false;
                }

                if (AssetDatabase.LoadAllAssetsAtPath(assetPath).Length > 1)
                {
                    return AssetDatabase.LoadMainAssetAtPath(assetPath) != spec;
                }

                return false;
            }

            internal static string FlushReg(string info, string cont)
            {
                cont = AwakeReg(cont);
                string directoryName = Path.GetDirectoryName(info);
                string extension = Path.GetExtension(info);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                    AssetDatabase.ImportAsset(directoryName);
                }

                return Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(directoryName + "/" + cont + extension));
            }
        }
    }
}
