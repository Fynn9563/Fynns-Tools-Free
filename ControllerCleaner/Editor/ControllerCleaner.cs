using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace FynnsTools.Controllers
{
    public class ControllerCleaner : EditorWindow
    {
        private static AnimatorController scanOneController;
        private static readonly List<ScanResult> results = new List<ScanResult>();
        private static Vector2 scroll;

        [MenuItem("Tools/Fynn's Tools/Controller Cleaner", false, 11)]
        private static void ShowWindow()
        {
            var window = GetWindow<ControllerCleaner>(false, "Controller Cleaner", true);
            window.titleContent.image = EditorGUIUtility.IconContent("Grid.PaintTool").image;
            window.minSize = new Vector2(440, 280);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(Styles.window))
            {
                EditorGUILayout.LabelField("Controller Cleaner", Styles.heading);
                EditorGUILayout.LabelField("Find unused items inside Animator Controllers.", EditorStyles.miniLabel);
                EditorGUILayout.Space(10);
                using (new EditorGUILayout.VerticalScope(Styles.card))
                {
                    EditorGUILayout.LabelField("Controller", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        scanOneController = (AnimatorController)EditorGUILayout.ObjectField(scanOneController, typeof(AnimatorController), false);
                        using (new EditorGUI.DisabledScope(!scanOneController))
                            if (GUILayout.Button("Scan", GUILayout.Width(70), GUILayout.Height(22))) ScanOne();
                    }
                    EditorGUILayout.Space(4);
                    if (GUILayout.Button("Scan All Controllers", GUILayout.Height(24))) ScanAll();
                }
                EditorGUILayout.Space(10);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
                    GUILayout.Label(results.Count + " controllers", EditorStyles.miniLabel);
                }
                scroll = EditorGUILayout.BeginScrollView(scroll);
                if (results.Count == 0)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Choose a controller or scan the project to begin.", Styles.empty);
                    GUILayout.FlexibleSpace();
                }
                foreach (var r in results) r.Display();
                EditorGUILayout.EndScrollView();
                if (results.Count > 0)
                {
                    EditorGUILayout.Space(6);
                    if (GUILayout.Button("Clean All", GUILayout.Height(26)))
                    {
                        foreach (var r in results) r.CleanUpController();
                        GreenLog("Finished Cleanup!");
                    }
                }
                EditorGUILayout.Space(6);
                Credit();
            }
        }

        private void ScanOne()
        {
            results.Remove(results.FirstOrDefault(r => r.controller == scanOneController));
            results.Insert(0, new ScanResult(scanOneController));
        }
        private void ScanAll()
        {
            results.Clear();
            var allControllers = AssetDatabase.FindAssets($"t:{nameof(AnimatorController)}").Select(g => AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(g)));
            foreach (var c in allControllers)
                results.Add(new ScanResult(c));
        }

        private static void DrawSeperator(int thickness = 2, int padding = 10, int maxWidth = 0)
        {
            var r = maxWidth > 0 ?
                EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding), GUILayout.MaxWidth(maxWidth)) :
                EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));

            r.height = thickness;
            r.y += padding / 2f;
            r.x -= 2;
            r.width += 6;
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out Color lineColor);
            EditorGUI.DrawRect(r, lineColor);
        }

        private static void GreenLog(string msg) 
            => Debug.Log($"<color=green>[ControllerCleaner]</color> {msg}");
        private static void Credit()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Created by Fynn9563", EditorStyles.miniLabel))
                    Application.OpenURL("https://github.com/Fynn9563");
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            }
        }


        private class ScanResult
        {
            public AnimatorController controller;
            private ConcurrentBag<Object> usedObjects;
            private Object[] obsoleteObjects;
            private Object[] allObjects;
            private string failMessage;
            private bool finished;
            private bool finalizing;
            private bool completed;
            private bool failed;
            private bool cancelled;
            private bool isClean => obsoleteObjects.Length == 0;

            private string scanTimeString => $"({stopwatch.Elapsed.TotalMilliseconds / 1000f:0.0})";
            private readonly Stopwatch stopwatch = new Stopwatch();
            private CancellationTokenSource tokenSource;

            private static readonly Type[] scanTypes =
            {
                typeof(AnimatorState),
                typeof(AnimatorStateTransition),
                typeof(AnimatorStateMachine),
                typeof(AnimatorTransition),
                typeof(AnimatorTransitionBase),
                typeof(StateMachineBehaviour),
                typeof(BlendTree),
            };

            public ScanResult(AnimatorController target) => ScanController(target);
            public void Display()
            {
                GUILayoutOption[] buttonOptions = new[] { GUILayout.Width(58), GUILayout.Height(22) };

                if (finalizing && !failed && !cancelled && !completed) FinalizeScan();

                using (new GUILayout.HorizontalScope(Styles.row))
                {
                    string name = controller ? controller.name : "[Missing]";
                    if (GUILayout.Button(new GUIContent(name, name), Styles.controllerName, GUILayout.MinWidth(70)))
                        if (controller) EditorGUIUtility.PingObject(controller);
                    GUIContent state;
                    if (failed) state = new GUIContent(" Failed", Styles.fail.image, failMessage);
                    else if (cancelled) state = new GUIContent(" Cancelled", Styles.cancel.image);
                    else if (!completed) state = new GUIContent(" Scanning…");
                    else if (isClean) state = new GUIContent(" Clean", Styles.clean.image);
                    else state = new GUIContent(" " + obsoleteObjects.Length + " unused", Styles.warn.image);
                    GUILayout.Label(state, Styles.status, GUILayout.Width(95));

                    using (new EditorGUI.DisabledScope(isClean || !completed || failed || cancelled))
                        if (GUILayout.Button("Clean", buttonOptions))
                            CleanUpController();

                    if (finished)
                    {
                        using (new EditorGUI.DisabledScope(!controller))
                            if (GUILayout.Button("Scan", buttonOptions))
                                ScanController(controller);
                    }
                    else if (GUILayout.Button("Cancel", buttonOptions))
                        CancelScan();
                }
            }

            #region Cleanup
            public void CleanUpController()
            {
                if (!completed || failed || cancelled || obsoleteObjects.Length <= 0) return;
                StringBuilder removedAssets = new StringBuilder();

                foreach (var o in obsoleteObjects)
                {
                    if (!o) continue;
                    removedAssets.AppendLine($"{o.name} ({o.GetType().Name})");
                    AssetDatabase.RemoveObjectFromAsset(o);
                    DestroyImmediate(o);
                }

                GreenLog($"Removed {obsoleteObjects.Length} unused Sub-Assets from {controller.name}!\n\n{removedAssets}");
                RemoveMissingTransitions();
                ScanController(controller);
            }

            private void RemoveMissingTransitions(AnimatorStateMachine m)
            {
                m.entryTransitions = KeepValid(m.entryTransitions);
                m.anyStateTransitions = KeepValid(m.anyStateTransitions);
                foreach (var cs in m.states)
                {
                    cs.state.transitions = KeepValid(cs.state.transitions);
                    EditorUtility.SetDirty(cs.state);
                }
                foreach (var cssm in m.stateMachines)
                {
                    m.SetStateMachineTransitions(cssm.stateMachine, KeepValid(m.GetStateMachineTransitions(cssm.stateMachine)));
                    RemoveMissingTransitions(cssm.stateMachine);
                }
                EditorUtility.SetDirty(m);
            }

            private void RemoveMissingTransitions()
            {
                foreach (var l in controller.layers)
                    RemoveMissingTransitions(l.stateMachine);
            }
            #endregion

            #region Scanning
            private void CancelScan() => tokenSource?.Cancel();

            private void ScanController(AnimatorController target)
            {
                controller = target;
                allObjects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller))
                    .Where(o => o && scanTypes.Contains(o.GetType())).ToArray();
                AsyncScanControllerStart();
            }

            private async void AsyncScanControllerStart()
            {
                stopwatch.Restart();
                tokenSource = new CancellationTokenSource();
                usedObjects = new ConcurrentBag<Object>();
                obsoleteObjects = Array.Empty<Object>();
                finished = finalizing = failed = completed = cancelled = false;
                await AsyncScanController();
                stopwatch.Stop();
                finalizing = true;
            }
            private async Task AsyncScanController()
            {
                Task[] tasks = new Task[controller.layers.Length];
                try
                {
                    for (int i = 0; i < tasks.Length; i++)
                        tasks[i] = AsyncScanMachine(controller.layers[i].stateMachine, true, i);
                }
                catch (OperationCanceledException)
                {
                    cancelled = finished = true;
                    stopwatch.Stop();
                }
                catch (Exception e)
                {
                    failed = finished = true;
                    failMessage = e.ToString();
                    stopwatch.Stop();
                }
                await Task.WhenAll(tasks);
            }
            private async Task AsyncScanMachine(AnimatorStateMachine machine, bool isRootMachine, int layerIndex)
            {
                try
                {
                    tokenSource.Token.ThrowIfCancellationRequested();

                    usedObjects.Add(machine);
                    AddBehaviours(machine.behaviours);
                    bool isSyncedLayer = controller.layers[layerIndex].syncedLayerIndex >= 0;
                    foreach (var cs in machine.states)
                    {
                        var s = cs.state;
                        usedObjects.Add(s);
                        AddTree(s.motion as BlendTree);
                        if (isSyncedLayer)
                            AddTree(controller.GetStateEffectiveMotion(s, layerIndex) as BlendTree);
                        AddTransitions(s.transitions);
                        AddBehaviours(s.behaviours);
                    }

                    AddTransitions(machine.entryTransitions);
                    if (isRootMachine) AddTransitions(machine.anyStateTransitions);

                    Task[] tasks = new Task[machine.stateMachines.Length];
                    for (int i = 0; i < tasks.Length; i++)
                        tasks[i] = AsyncScanMachine(machine.stateMachines[i].stateMachine, false, layerIndex);
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    cancelled = finished = true;
                    stopwatch.Stop();
                }
                catch (Exception e)
                {
                    failed = finished = true;
                    failMessage = e.ToString();
                    stopwatch.Stop();
                }
            }
            private void FinalizeScan()
            {
                if (failed || cancelled) return;
                stopwatch.Start();

                foreach (var l in controller.layers) 
                    FinalScanMachine(l.stateMachine);

                if (failed || cancelled)
                {
                    stopwatch.Stop();
                    return;
                }
                obsoleteObjects = allObjects.Except(usedObjects).ToArray();
                stopwatch.Stop();
                finished = completed = true;
            }
            private void FinalScanMachine(AnimatorStateMachine machine)
            {
                if (!machine) return;
                try
                {
                    foreach (var cssm in machine.stateMachines)
                    {
                        if (!cssm.stateMachine) continue;
                        AddTransitions(machine.GetStateMachineTransitions(cssm.stateMachine));
                        FinalScanMachine(cssm.stateMachine);
                    }
                }
                catch (Exception e)
                {
                    failed = finished = true;
                    failMessage = e.ToString();
                    stopwatch.Stop();
                }
            }
            #endregion

            #region Helpers
            private void AddTransitions(IEnumerable<AnimatorTransitionBase> transitions)
            {
                foreach (var t in transitions)
                {
                    if (!t) continue;
                    if (t.destinationState || t.destinationStateMachine || t.isExit)
                        usedObjects.Add(t);
                }
            }

            private void AddBehaviours(IEnumerable<StateMachineBehaviour> behaviours)
            {
                foreach (var b in behaviours)
                    usedObjects.Add(b);
            }
            private void AddTree(BlendTree t)
            {
                if (!t) return;
                usedObjects.Add(t);
                foreach (var cm in t.children)
                    AddTree(cm.motion as BlendTree);
            }

            private static T[] KeepValid<T>(T[] array) where T : Object
                => array.Where(o => o).ToArray();
            #endregion
        }

        private static class Styles
        {
            public static readonly GUIContent warn = EditorGUIUtility.IconContent("console.warnicon");
            public static readonly GUIContent cancel = new GUIContent(EditorGUIUtility.IconContent("console.warnicon.inactive.sml")) {tooltip = "Skipped"};
            public static readonly GUIContent clean = new GUIContent(EditorGUIUtility.IconContent("TestPassed")) { tooltip = "Clean!" };
            public static readonly GUIContent fail = new GUIContent(EditorGUIUtility.IconContent("console.erroricon.sml")) {tooltip = "Failed"};

            public static readonly GUIStyle fadedLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            public static readonly GUIStyle toolbarButton = GUI.skin.GetStyle("toolbarbutton");
            public static readonly GUIStyle window = new GUIStyle { padding = new RectOffset(12, 12, 12, 8) };
            public static readonly GUIStyle heading = new GUIStyle(EditorStyles.boldLabel) { fontSize = 17, fixedHeight = 24 };
            public static readonly GUIStyle card = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(8, 8, 8, 8) };
            public static readonly GUIStyle row = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 5, 5), margin = new RectOffset(0, 0, 2, 2) };
            public static readonly GUIStyle controllerName = new GUIStyle(EditorStyles.label) { clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft, stretchWidth = true };
            public static readonly GUIStyle status = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            public static readonly GUIStyle empty = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true };
        }
    }
}
