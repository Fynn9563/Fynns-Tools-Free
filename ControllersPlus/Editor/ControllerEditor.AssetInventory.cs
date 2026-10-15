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
        internal struct ControllerAssetInventory
        {
            internal readonly List<UnityEditor.Animations.BlendTree> _SchemaVisitor;
            internal readonly List<StateMachineBehaviour> broadcasterVisitor;
            internal readonly List<AnimatorStateMachine> _ProxyVisitor;
            internal readonly List<AnimatorTransitionBase> structVisitor;
            internal readonly List<AnimatorState> m_ServiceVisitor;
            internal readonly List<UnityEngine.Object> stateVisitor;
            internal ControllerAssetInventory(UnityEditor.Animations.AnimatorController instance) : this()
            {
                _SchemaVisitor = new List<UnityEditor.Animations.BlendTree>();
                broadcasterVisitor = new List<StateMachineBehaviour>();
                _ProxyVisitor = new List<AnimatorStateMachine>();
                structVisitor = new List<AnimatorTransitionBase>();
                m_ServiceVisitor = new List<AnimatorState>();
                stateVisitor = new List<UnityEngine.Object>();
                if (!instance)
                {
                    return;
                }

                UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(instance));
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] is AnimatorTransitionBase item)
                    {
                        structVisitor.Add(item);
                    }
                    else if (array[i] is AnimatorState item2)
                    {
                        m_ServiceVisitor.Add(item2);
                    }
                    else if (array[i] is UnityEditor.Animations.BlendTree item3)
                    {
                        _SchemaVisitor.Add(item3);
                    }
                    else if (array[i] is StateMachineBehaviour item4)
                    {
                        broadcasterVisitor.Add(item4);
                    }
                    else if (array[i] is AnimatorStateMachine item5)
                    {
                        _ProxyVisitor.Add(item5);
                    }
                    else if (array[i] != instance && !(array[i] is ControllerParameterNotes))
                    {
                        stateVisitor.Add(array[i]);
                    }
                }
            }
        }
    }
}
