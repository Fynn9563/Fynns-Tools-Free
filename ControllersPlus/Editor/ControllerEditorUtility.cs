using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;

namespace FynnsTools.ControllerEditor
{
    internal static partial class ControllerEditorUtility
    {
        internal sealed class WatcherProcessor
        {
            internal readonly GUIContent candidateProcessor = new GUIContent(EditorGUIUtility.IconContent("Prefab Icon"))
            {tooltip = "Simple Mode"};
            internal readonly GUIContent productProcessor = new GUIContent(EditorGUIUtility.IconContent("GameObject Icon"))
            {tooltip = "Advanced Mode"};
            internal readonly GUIContent m_ExpressionProcessor = new GUIContent(EditorGUIUtility.IconContent("NetworkStartPosition Icon"))
            {tooltip = "Default Merge Clip"};
            internal readonly GUIContent systemProcessor = new GUIContent(EditorGUIUtility.IconContent("CompositeCollider2D Icon"))
            {tooltip = "Default Replace Clip"};
            internal readonly GUIContent workerProcessor = EditorGUIUtility.IconContent("curvekeyframesemiselectedoverlay");
            internal readonly GUIContent _FilterProcessor = EditorGUIUtility.IconContent("curvekeyframeweighted");
            internal readonly GUIContent stubProcessor = new GUIContent(EditorGUIUtility.IconContent("EditCollider"))
            {tooltip = "Linear"};
            internal readonly GUIContent readerProcessor = new GUIContent(EditorGUIUtility.IconContent("preAudioLoopOff"))
            {tooltip = "Looping Clip"};
            internal readonly GUIContent m_BridgeProcessor = new GUIContent(EditorGUIUtility.IconContent("SaveActive"))
            {tooltip = "Copy"};
            internal readonly GUIContent m_StrategyProcessor = new GUIContent(EditorGUIUtility.IconContent("Clipboard"))
            {tooltip = "Paste"};
            internal readonly GUIContent m_CustomerProcessor = new GUIContent(EditorGUIUtility.IconContent("Refresh"))
            {tooltip = "Restore Defaults"};
            internal readonly GUIContent _DatabaseProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorState Icon"))
            {tooltip = "Animator States"};
            internal readonly GUIContent m_ExporterProcessor = new GUIContent(EditorGUIUtility.IconContent("UnityEditor.VersionControl"))
            {tooltip = "Switch"};
            internal readonly GUIContent _IdentifierProcessor = new GUIContent(EditorGUIUtility.IconContent("BlendTree Icon"))
            {tooltip = "Separate"};
            internal readonly GUIContent _AttrProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateTransition Icon"))
            {tooltip = "Merge"};
            internal readonly GUIContent dispatcherProcessor = new GUIContent(EditorGUIUtility.IconContent("Animation.Record"))
            {tooltip = "Shared?"};
            internal readonly GUIContent m_RegistryProcessor = new GUIContent(EditorGUIUtility.IconContent("Animation.Play"))
            {tooltip = "Animation Clip"};
            internal readonly GUIContent _TagProcessor = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
            {tooltip = "Remove Condition"};
            internal readonly GUIContent importerProcessor = new GUIContent(EditorGUIUtility.IconContent("winbtn_win_close"))
            {tooltip = "Deselect"};
            internal readonly GUIContent _RequestProcessor = new GUIContent(EditorGUIUtility.IconContent("_Popup"))
            {tooltip = "Settings"};
            internal readonly GUIContent m_PrinterProcessor = new GUIContent(EditorGUIUtility.IconContent("BlendTree Icon"))
            {tooltip = "BlendTrees"};
            internal readonly GUIContent m_WriterProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateMachine Icon"))
            {tooltip = "StateMachines"};
            internal readonly GUIContent m_ParamsProcessor = new GUIContent(EditorGUIUtility.IconContent("dll Script Icon"))
            {tooltip = "StateMachine Behaviors"};
            internal readonly GUIContent _ListenerProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateTransition Icon"))
            {tooltip = "Transitions"};
            internal readonly Texture _GetterProcessor = EditorGUIUtility.IconContent("BodySilhouette").image;
            internal readonly Texture m_InterceptorProcessor = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            internal readonly GUIContent m_CreatorProcessor = new GUIContent(EditorGUIUtility.IconContent("CollabError"))
            {tooltip = "Invalid Pattern"};
            internal readonly GUIContent _EventProcessor = EditorGUIUtility.IconContent("FolderOpened Icon");
            internal readonly GUIContent infoProcessor = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
            {tooltip = "Remove element from list"};
            internal readonly GUIContent[] m_FacadeProcessor;
            internal readonly EditorResourceCache m_AdvisorProcessor = NewList("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");
            internal readonly EditorResourceCache _CallbackProcessor = NewList("Refresh", "ds-icon-refresh", "Reset");
            internal readonly EditorResourceCache indexerProcessor = NewList("console.infoicon.sml", "ds-icon-announcement");
            internal readonly EditorResourceCache issuerProcessor = NewList("console.warnicon.sml", "ds-icon-warning");
            internal readonly EditorResourceCache prototypeProcessor = NewList("console.warnicon.inactive.sml", "ds-icon-note");
            internal readonly EditorResourceCache ruleProcessor = NewList("console.erroricon.sml", "ds-icon-error");
            internal readonly EditorResourceCache m_SingletonProcessor = NewList("VerticalLayoutGroup Icon", "ds-icon-hamMenu");
            internal readonly EditorResourceCache _FactoryProcessor = NewList("Lightmapping", "ds-icon-light", "Ping");
            internal readonly EditorResourceCache _AccountProcessor = NewList("_Help", "ds-icon-help");
            internal readonly EditorResourceCache _RefProcessor = NewList("scenevis_visible_hover", "ds-icon-visible", "Visible");
            internal readonly EditorResourceCache m_StatusProcessor = NewList("scenevis_hidden_hover", "ds-icon-hidden", "Hidden");
            internal readonly EditorResourceCache m_TokenProcessor = NewList("Mirror", "ds-icon-mirror", "Mirror");
            internal readonly GUIContent m_CodeProcessor = PushQueue("TestPassed", "Up to Date!");
            internal readonly GUIContent _DicProcessor = PushQueue("UnityEditor.InspectorWindow");
            internal readonly GUIContent invocationProcessor = PushQueue("FolderOpened Icon", "Select a folder");
            internal readonly GUIContent roleProcessor = PushQueue("editicon.sml");
            internal readonly GUIContent _ParamProcessor = PushQueue("settings");
            internal readonly GUIContent m_ModelProcessor = PushQueue("Selectable Icon");
            internal readonly GUIContent m_TokenizerProcessor = PushQueue("eyeDropper.Large");
            internal readonly GUIContent m_DecoratorProcessor = PushQueue("Toolbar Minus", "Remove selection from list");
            internal readonly GUIContent m_ComparatorProcessor = PushQueue("CollabCreate Icon");
            internal readonly GUIContent exceptionProcessor = PushQueue("d_CreateAddNew@2x");
            internal readonly GUIContent _ObjectProcessor = PushQueue("IN LockButton");
            internal readonly GUIContent _UtilsProcessor = PushQueue("IN LockButton on");
            internal readonly GUIContent m_ValProcessor = PushQueue("d_scenepicking_pickable_hover@2x");
            internal readonly GUIContent _ValueProcessor = PushQueue("d_scenepicking_notpickable@2x");
            internal readonly GUIContent m_MerchantProcessor = PushQueue("d_CustomTool@2x");
            internal readonly GUIContent authenticationProcessor = PushQueue("winbtn_win_close");
            internal readonly GUIContent _ReponseProcessor = PushQueue("Search Icon");
            internal readonly GUIContent _PoolProcessor = PushQueue("BlendTree Icon");
            internal readonly GUIContent parameterProcessor = PushQueue("UnityEditor.FindDependencies");
            internal readonly GUIContent m_ComposerProcessor = PushQueue("UnityEditor.SceneHierarchyWindow");
            internal readonly GUIContent repositoryProcessor = PushQueue("Collab.FolderAdded");
            internal readonly GUIContent _MappingProcessor = PushQueue("AlphabeticalSorting", "Sort");
            internal WatcherProcessor()
            {
                m_FacadeProcessor = new GUIContent[6]{m_WriterProcessor, _DatabaseProcessor, _ListenerProcessor, m_PrinterProcessor, m_ParamsProcessor, m_RegistryProcessor};
            }
        }

        internal class BaseProcessor
        {
            internal MethodInfo m_ContainerProcessor;
            internal readonly GUIStyle _ClassProcessor = new GUIStyle(EditorStyles.miniLabel)
            {alignment = TextAnchor.MiddleCenter};
            internal readonly GUIStyle m_MockProcessor = new GUIStyle(GUI.skin.label)
            {normal = {textColor = Color.cyan}, hover = {textColor = Color.cyan}};
            internal readonly GUIStyle m_InstanceProcessor = new GUIStyle{margin = new RectOffset(4, 4, 4, 4), alignment = TextAnchor.MiddleCenter};
            internal readonly GUIContent fieldProcessor = new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Trash"))
            {tooltip = "Remove"};
            internal readonly GUIContent m_AttributeProcessor = new GUIContent("B", "Behaviours");
            internal readonly GUIContent _ClientProcessor = new GUIContent("WD", "Write Defaults");
            internal readonly GUIStyle configProcessor = new GUIStyle{padding = new RectOffset(2, 2, 2, 2), margin = new RectOffset()};
            internal readonly GUIStyle descriptorProcessor = new GUIStyle(GUI.skin.GetStyle("DropDownButton"))
            {alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(2.5f, 0f), fixedHeight = 0f};
            internal readonly GUIStyle _TemplateProcessor = GUI.skin.GetStyle("RL FooterButton");
            internal readonly GUIStyle messageProcessor;
            internal readonly GUIStyle m_CollectionProcessor;
            internal static readonly Color m_ParserProcessor = new Color(0.357f, 0.357f, 0.357f);
            internal readonly GUILayoutOption[] managerProcessor = new GUILayoutOption[2]{GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight)};
            internal readonly GUIStyle itemProcessor = new GUIStyle(GUI.skin.label)
            {fontStyle = FontStyle.Bold, fontSize = 18, alignment = TextAnchor.MiddleLeft};
            internal readonly GUIStyle _SpecificationProcessor = new GUIStyle(GUI.skin.label)
            {fontStyle = FontStyle.Bold, fontSize = 14, alignment = TextAnchor.MiddleLeft};
            internal readonly GUIStyle methodProcessor = new GUIStyle(GUI.skin.label)
            {fontStyle = FontStyle.Bold, fontSize = 12, alignment = TextAnchor.MiddleLeft};
            internal readonly GUIStyle schemaProcessor = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleCenter, richText = true, fontStyle = FontStyle.Bold, fontSize = 18};
            internal readonly GUIStyle broadcasterProcessor = new GUIStyle(GUI.skin.label)
            {padding = new RectOffset(1, 1, 1, 1), fixedWidth = 18f, fixedHeight = 18f};
            internal readonly GUIStyle _ProxyProcessor = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleCenter, richText = true};
            internal readonly GUIStyle _StructProcessor = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = true};
            internal readonly GUIStyle m_ServiceProcessor = new GUIStyle(GUI.skin.label)
            {padding = new RectOffset(), margin = new RectOffset(1, 1, 1, 1)};
            internal readonly GUIStyle m_StateProcessor = new GUIStyle(GUI.skin.button)
            {padding = new RectOffset(0, 0, 2, 2)};
            internal readonly GUIStyle m_GlobalProcessor = new GUIStyle(GUI.skin.label)
            {richText = true, wordWrap = true};
            internal readonly GUIStyle m_TaskProcessor = new GUIStyle(GUI.skin.label)
            {fontStyle = FontStyle.Bold, richText = true, wordWrap = true};
            internal readonly GUIStyle _ProcessProcessor = new GUIStyle(GUI.skin.button)
            {fontSize = 14, fixedHeight = 30f};
            internal readonly GUIStyle m_ProducerProcessor = new GUIStyle(GUI.skin.button)
            {fontSize = 18, fixedHeight = 40f, fontStyle = FontStyle.Bold};
            internal readonly GUIStyle _IteratorProcessor = new GUIStyle(GUI.skin.label)
            {name = "Toggle"};
            internal readonly GUIStyle _PublisherProcessor = new GUIStyle(GUI.skin.label)
            {richText = true};
            internal readonly GUIStyle m_ConfigurationProcessor = new GUIStyle(GUI.skin.textArea)
            {richText = true};
            internal readonly GUIStyle procProcessor = "AssetLabel";
            internal readonly GUIStyle _WrapperObserver = "in bigtitle";
            internal readonly GUIStyle annotationObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Italic, richText = true, fontSize = 11, normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)}};
            internal readonly GUIStyle _VisitorObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic, richText = true, fontSize = 11, normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)}};
            internal readonly GUIStyle algoObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Italic, richText = true, fontSize = 11, normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)}};
            internal readonly GUIStyle _MapperObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Italic, richText = true, fontSize = 11, normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)}, contentOffset = new Vector2(-3f, 1.5f)};
            internal readonly GUIStyle m_InitializerObserver = new GUIStyle(EditorStyles.miniLabel)
            {alignment = TextAnchor.MiddleLeft, richText = true, contentOffset = new Vector2(-3f, 3f)};
            internal readonly GUIStyle m_DefinitionObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Italic, richText = true, fontSize = 11, normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)}, name = "Toggle", hover = {textColor = new Color(0.3f, 0.7f, 1f)}};
            internal readonly GUIStyle m_RegObserver = new GUIStyle(GUI.skin.label)
            {alignment = TextAnchor.MiddleLeft, richText = true, name = "Toggle", hover = {textColor = m_AlgoProcessor}};
            [SpecialName]
            internal MethodInfo LoginError()
            {
                return m_ContainerProcessor ?? (m_ContainerProcessor = typeof(EditorGUILayout).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).First((MethodInfo m) =>
                {
                    if (m.Name == "TextFieldDropDown")
                    {
                        return m.GetParameters().Length == 3;
                    }

                    return false;
                }));
            }

            internal BaseProcessor()
            {
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            return;
                        default:
                            messageProcessor = new GUIStyle(algoObserver)
                            {contentOffset = new Vector2(-2.5f, 0f), normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : PushList(91f))}};
                            num = 2;
                            break;
                        case 2:
                            m_CollectionProcessor = new GUIStyle(messageProcessor)
                            {alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(2.5f, 0f), normal = {textColor = (EditorGUIUtility.isProSkin ? Color.gray : PushList(91f))}};
                            num = 1;
                            break;
                    }
                }
            }
        }

        internal enum EventCommands
        {
            Copy,
            Cut,
            Paste,
            Duplicate,
            Delete,
            SoftDelete,
            SelectAll,
            Find,
            FrameSelected,
            FrameSelectedWithLock,
            FocusProjectWindow
        }

        internal enum PathOption
        {
            Normal,
            ForceFolder,
            ForceFile
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass120_0
        {
            public string _TaskObserver;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass121_0
        {
            public bool m_ProcessObserver;
            public string m_ProducerObserver;
        }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        internal struct _003C_003Ec__DisplayClass128_0
        {
            public Dictionary<UnityEngine.Object, UnityEngine.Object> iteratorObserver;
            public UnityEditor.Animations.AnimatorController m_PublisherObserver;
            public bool configurationObserver;
            public Dictionary<UnityEngine.Object, UnityEngine.Object> _ProcObserver;
            public UnityEditor.Animations.AnimatorControllerLayer m_WrapperServer;
            public UnityEditor.Animations.AnimatorControllerLayer annotationServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass142_0
        {
            public StateMachineBehaviour visitorServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass143_0
        {
            public Type algoServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass155_0
        {
            public Action<Motion> _TestsServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass156_0
        {
            public Action<UnityEditor.Animations.BlendTree> m_PropertyServer;
            public Action<AnimationClip> processorServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass196_0
        {
            public AvatarMask m_PredicateServer;
            public Transform m_RulesServer;
            public HashSet<string> m_QueueServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass200_0
        {
            public HashSet<string> setterServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass205_0<T>
            where T : UnityEngine.Object
        {
            public Func<T, bool> acceptConditions;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass206_0<T>
            where T : UnityEngine.Object
        {
            public Func<T, bool> acceptConditions;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass237_0
        {
            public string contextServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass243_0
        {
            public HashSet<string> m_ConsumerServer;
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass410_0
        {
            public string _CustomerServer;
        }

        internal static readonly GUIContent m_CollectionProperty = new GUIContent();
        internal static string itemProperty;
        internal static readonly Dictionary<Type, string> m_SpecificationProperty;
        private static readonly Queue<Action> _MethodProperty;
        private static readonly Queue<Action> _SchemaProperty;
        internal static readonly string m_BroadcasterProperty;
        internal static readonly bool m_ProxyProperty;
        private static Dictionary<string, object> serviceProperty;
        private static readonly Stack<(Rect, MouseCursor)> stateProperty;
        private static bool globalProperty;
        private static readonly int iteratorProperty;
        internal static Color publisherProperty;
        internal static Color configurationProperty;
        internal static Color _ProcProperty;
        internal static Color _WrapperProcessor;
        internal static Color m_AnnotationProcessor;
        internal static Color visitorProcessor;
        internal static Color m_AlgoProcessor;
        internal static Color m_MapperProcessor;
        internal static WatcherProcessor _InitializerProcessor;
        internal static BaseProcessor _DefinitionProcessor;
        private static readonly int _PropertyProcessor;
        private static MethodInfo processorProcessor;
        private static Type observerProcessor;
        private static bool m_ServerProcessor;
        private static FieldInfo queueProcessor;
        private static bool errorProcessor;
        private static Texture2D connectionProcessor;
        internal static readonly string[] m_AdapterProcessor;
        internal static readonly string[] interpreterProcessor;
        internal static void InvokeResolver<T>(this IEnumerable<T> first, Action<T> visitor)
        {
            IEnumerator<T> enumerator = first.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    visitor(current);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
        }

        internal static int FindResolver<T>(this IEnumerable<T> var1, Func<T, bool> cont)
        {
            int num = -1;
            IEnumerator<T> enumerator = var1.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    num = checked(num + 1);
                    if (enumerator.Current != null && cont(enumerator.Current))
                    {
                        return num;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            return -1;
        }

        internal static bool ExcludeResolver<T>(this IEnumerable<T> param, Func<T, bool> caller, out int c)
        {
            c = param.FindResolver(caller);
            return c != -1;
        }

        internal static bool RemoveResolver<T>(this Type config)
        {
            if (!config.IsSubclassOf(typeof(T)))
            {
                return config == typeof(T);
            }

            return true;
        }

        internal static bool InstantiateResolver(this Type reference, Type attr)
        {
            if (!reference.IsSubclassOf(attr))
            {
                return reference == attr;
            }

            return true;
        }

        internal static bool ResetResolver(this string param)
        {
            return string.IsNullOrEmpty(param);
        }

        internal static bool FlushResolver(this string item)
        {
            return string.IsNullOrWhiteSpace(item);
        }

        internal static bool CustomizeResolver(this int end_param, IList pred)
        {
            if (end_param < 0)
            {
                return false;
            }

            return end_param < pred.Count;
        }

        internal static bool GetResolver(this int offsetident, int countermin, int start_control)
        {
            if (offsetident < countermin)
            {
                return true;
            }

            return offsetident > start_control;
        }

        internal static int CalcResolver(this float spec, int counter_end)
        {
            return Mathf.RoundToInt(spec).IncludeResolver(counter_end);
        }

        internal static int IncludeResolver(this int minfirst, int ivkhigh)
        {
            return Mathf.RoundToInt((float)minfirst / (float)ivkhigh) * ivkhigh;
        }

        internal static string RunResolver(this string spec, Color reg)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(reg) + ">" + spec + "</color>";
        }

        internal static string CloneResolver(this string task, LogType caller)
        {
            switch (caller)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return task.RunResolver(_ProcProperty);
                case LogType.Warning:
                    return task.RunResolver(_WrapperProcessor);
                default:
                    return task.RunResolver(configurationProperty);
            }
        }

        internal static void LoginResolver(this string param, LogType selection = LogType.Log)
        {
            switch (selection)
            {
                default:
                    goto case LogType.Log;
                case LogType.Log:
                    UnityEngine.Debug.Log(param);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(param);
                    break;
                case LogType.Exception:
                    UnityEngine.Debug.LogException(new Exception(param));
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(param);
                    break;
                case LogType.Assert:
                    break;
            }
        }

        internal static void ReflectResolver(this string item, LogType connection = LogType.Log)
        {
            item.CloneResolver(connection).LoginResolver(connection);
        }

        internal static GUIContent DeleteResolver(this string param, bool tokenneeded)
        {
            return param.CreateResolver("", tokenneeded);
        }

        internal static GUIContent CreateResolver(this string var1, string cfg = "", bool wanthelper = false)
        {
            while (true)
            {
                m_CollectionProperty.text = var1;
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            m_CollectionProperty.tooltip = cfg;
                            num = 6;
                            continue;
                        case 4:
                            return m_CollectionProperty;
                        case 3:
                        case 5:
                            return new GUIContent(m_CollectionProperty);
                        case 6:
                            if (wanthelper)
                            {
                                num = 5;
                                continue;
                            }

                            goto case 4;
                        case 1:
                            break;
                        default:
                            goto case 2;
                    }

                    break;
                }
            }
        }

        internal static string NewResolver(this string key)
        {
            if (key.ResetResolver())
            {
                return key;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToUpper(key[0]));
            for (int i = 1; i < key.Length; i++)
            {
                if (char.IsUpper(key[i]) && !char.IsUpper(key[i - 1]))
                {
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append(key[i]);
            }

            return stringBuilder.ToString();
        }

        internal static float PushResolver(this string param, GUIStyle visitor = null)
        {
            while (visitor == null)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                        case 2:
                            break;
                        case 4:
                            goto end_IL_001d;
                        default:
                            visitor = GUI.skin.label;
                            num = 1;
                            continue;
                        case 3:
                            goto default;
                    }

                    goto end_IL_0055;
                    continue;
                    end_IL_001d:
                        break;
                }

                continue;
                end_IL_0055:
                    break;
            }

            return visitor.CalcSize(param.CreateResolver()).x;
        }

        internal static Rect ViewResolver(this Rect var1)
        {
            Rect result = new Rect(var1);
            result.x = var1.x + var1.width;
            return result;
        }

        public static Rect ForgotResolver(this Rect info, float pol)
        {
            info.x += pol;
            return info;
        }

        public static Rect StopResolver(this Rect init, float pred)
        {
            init.y += pred;
            return init;
        }

        public static Rect UpdateResolver(this Rect last, float b)
        {
            last.height += b;
            return last;
        }

        public static Rect SortResolver(this ref Rect def, float visitor, bool isfield = false, float vis2 = -1f, bool iscont3 = false, bool isattr4 = true)
        {
            Rect result = def;
            result.width = (isfield ? visitor : (visitor * def.width / 100f));
            result.height = def.height;
            float num = ((vis2 == -1f) ? 0f : (iscont3 ? vis2 : (vis2 * def.width / 100f)));
            result.x = def.x + num;
            result.y = def.y;
            if (isattr4)
            {
                def.x = result.x + result.width;
                def.width -= result.width;
            }

            return result;
        }

        public static Rect PatchResolver(this ref Rect res, float map, bool isserv = false, float v2 = -1f, bool isvisitor3 = false, bool istoken4 = true)
        {
            Rect result = res;
            result.width = (isserv ? map : (map * res.width / 100f));
            float num = ((v2 == -1f) ? 0f : (isvisitor3 ? v2 : (v2 * res.width / 100f)));
            result.x = res.x + res.width - result.width - num;
            if (istoken4)
            {
                res.width -= result.width + num;
            }

            return result;
        }

        public static Rect InterruptResolver(this Rect asset, float connection)
        {
            asset.width = connection;
            return asset;
        }

        public static Rect ManageResolver(this Rect param, float result)
        {
            param.height = result;
            return param;
        }

        internal static Vector2 PostPredicate(this Vector2 task, int b_Z)
        {
            return new Vector2(task.x.CalcResolver(b_Z), task.y.CalcResolver(b_Z));
        }

        internal static bool CallPredicate(this ReorderableList key)
        {
            return key.index.CustomizeResolver(key.list);
        }

        internal static T CountPredicate<T>(this T ident, GUIContent map, bool haveutil = true, params GUILayoutOption[] options)
            where T : UnityEngine.Object
        {
            return (T)EditorGUILayout.ObjectField(map, ident, typeof(T), haveutil, options);
        }

        internal static AnimationCurve InvokePredicate(AnimationUtility.TangentMode setup = AnimationUtility.TangentMode.Free, params (float, float)[] timeValuePair)
        {
            AnimationCurve animationCurve = new AnimationCurve();
            for (int i = 0; i < timeValuePair.Length; i++)
            {
                var(time, value) = timeValuePair[i];
                animationCurve.AddKey(time, value);
                if (i > 0)
                {
                    AnimationUtility.SetKeyRightTangentMode(animationCurve, i - 1, setup);
                }

                AnimationUtility.SetKeyLeftTangentMode(animationCurve, i, setup);
            }

            return animationCurve;
        }

        internal static float FindPredicate(this AnimationClip v)
        {
            while (true)
            {
                int num;
                if (AnimationUtility.GetCurveBindings(v).Any())
                {
                    num = 0;
                    goto IL_001d;
                }

                goto IL_016b;
                IL_0150:
                    if (v.frameRate <= 0f)
                    {
                        break;
                    }

                num = 3;
                goto IL_001d;
                IL_016b:
                    ;
                goto IL_00ec;
                IL_00ec:
                    if (AnimationUtility.GetObjectReferenceCurveBindings(v).Any())
                    {
                        num = 4;
                        goto IL_001d;
                    }

                goto IL_0150;
                IL_001d:
                    switch (num)
                    {
                        default:
                            return v.length;
                        case 1:
                            break;
                        case 7:
                            goto IL_00ec;
                        case 3:
                            return 1f / v.frameRate;
                        case 2:
                            goto IL_0150;
                        case 6:
                            goto IL_016b;
                    }
            }

            return 1f / 60f;
        }

        internal static bool InitPredicate(this IEnumerable<EditorCurveBinding> i, EditorCurveBinding attr, out EditorCurveBinding tag)
        {
            IEnumerator<EditorCurveBinding> enumerator = i.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    EditorCurveBinding current = enumerator.Current;
                    if (current.propertyName == attr.propertyName && current.type == attr.type && current.path == attr.path)
                    {
                        tag = current;
                        return true;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            tag = default(EditorCurveBinding);
            return false;
        }

        internal static UnityEditor.Animations.AnimatorControllerLayer CreateLayer(this UnityEditor.Animations.AnimatorController task, string selection, float field, AvatarMask asset2 = null)
        {
            UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = new UnityEditor.Animations.AnimatorControllerLayer{name = selection, defaultWeight = field, avatarMask = asset2, stateMachine = new AnimatorStateMachine{name = selection, hideFlags = HideFlags.HideInHierarchy}};
            AssetDatabase.AddObjectToAsset(animatorControllerLayer.stateMachine, task);
            task.AddLayer(animatorControllerLayer);
            return animatorControllerLayer;
        }

        private static bool DefinePredicate(AnimatorStateTransition item)
        {
            while (item.isExit)
            {
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 5:
                            return item.destinationStateMachine == null;
                        case 1:
                        case 4:
                        case 6:
                            break;
                        case 3:
                            goto end_IL_001d;
                        case 2:
                            if (item.mute)
                            {
                                num = 0;
                                continue;
                            }

                            break;
                        default:
                            if (!(item.destinationState == null))
                            {
                                num = 4;
                                continue;
                            }

                            goto case 5;
                    }

                    goto end_IL_00ea;
                    continue;
                    end_IL_001d:
                        break;
                }

                continue;
                end_IL_00ea:
                    break;
            }

            return false;
        }

        internal static void StartPredicate(this UnityEditor.Animations.AnimatorControllerLayer info, string caller)
        {
            while (true)
            {
                int num;
                if (info.SelectPredicate(caller))
                {
                    num = 3;
                    goto IL_001d;
                }

                goto IL_0053;
                IL_001d:
                    switch (num)
                    {
                        default:
                            return;
                        case 4:
                            break;
                        case 2:
                            goto IL_0053;
                        case 1:
                            return;
                        case 0:
                            return;
                        case 3:
                            return;
                    }

                continue;
                IL_0053:
                    AnimatorStateTransition animatorStateTransition = info.stateMachine.AddAnyStateTransition((AnimatorState)null);
                animatorStateTransition.isExit = true;
                animatorStateTransition.mute = true;
                animatorStateTransition.name = caller;
                num = 1;
                goto IL_001d;
            }
        }

        internal static void ReadPredicate(this UnityEditor.Animations.AnimatorControllerLayer i, string visitor)
        {
            AnimatorStateTransition transition = default(AnimatorStateTransition);
            while (true)
            {
                _003C_003Ec__DisplayClass120_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass120_0();
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 4:
                            return;
                        case 2:
                            i.stateMachine.RemoveAnyStateTransition(transition);
                            return;
                        case 5:
                            return;
                        case 1:
                            break;
                        case 6:
                            transition = i.stateMachine.anyStateTransitions.First(delegate (AnimatorStateTransition tr)
                            {
                                if (!DefinePredicate(tr))
                                {
                                    return false;
                                }

                                return tr.name == CS_0024_003C_003E8__locals2._TaskObserver;
                            });
                            num = 2;
                            continue;
                        case 3:
                            CS_0024_003C_003E8__locals2._TaskObserver = visitor;
                            num = 6;
                            continue;
                        default:
                            goto case 3;
                    }

                    break;
                }
            }
        }

        internal static bool SelectPredicate(this UnityEditor.Animations.AnimatorControllerLayer def, string result, bool iscontrol = true)
        {
            while (true)
            {
                _003C_003Ec__DisplayClass121_0 CS_0024_003C_003E8__locals6 = new _003C_003Ec__DisplayClass121_0();
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            goto IL_0033;
                        default:
                            CS_0024_003C_003E8__locals6.m_ProducerObserver = result;
                            return def.stateMachine.anyStateTransitions.Any(delegate (AnimatorStateTransition t)
                            {
                                if (DefinePredicate(t))
                                {
                                    if (!CS_0024_003C_003E8__locals6.m_ProcessObserver || !(t.name == CS_0024_003C_003E8__locals6.m_ProducerObserver))
                                    {
                                        if (CS_0024_003C_003E8__locals6.m_ProcessObserver)
                                        {
                                            return false;
                                        }

                                        return t.name.Contains(CS_0024_003C_003E8__locals6.m_ProducerObserver);
                                    }

                                    return true;
                                }

                                return false;
                            });
                        case 2:
                            break;
                    }

                    break;
                    IL_0033:
                        CS_0024_003C_003E8__locals6.m_ProcessObserver = iscontrol;
                    num = 0;
                }
            }
        }

        internal static IEnumerable<string> RemovePredicate(this UnityEditor.Animations.AnimatorControllerLayer config)
        {
            return
                from t in config.stateMachine.anyStateTransitions.Where(DefinePredicate)select t.name;
        }

        internal static IEnumerable<string> InstantiatePredicate(this UnityEditor.Animations.AnimatorControllerLayer value)
        {
            return
                from s in value.RemovePredicate()
                where s[0] == '_'
                select s;
        }

        internal static void CopyLayers(UnityEditor.Animations.AnimatorController info, UnityEditor.Animations.AnimatorController cust, out UnityEditor.Animations.AnimatorControllerLayer[] dir)
        {
            try
            {
                UnityEditor.Animations.AnimatorControllerLayer[] layers = info.layers;
                int num = layers.Length;
                dir = new UnityEditor.Animations.AnimatorControllerLayer[num];
                int i = 0;
                for (; i < num; i++)
                {
                    EditorUtility.DisplayProgressBar("Copying Layers", $"Copying layer {i + 1} of {num}", (float)i / (float)num);
                    dir[i] = CopyLayer(layers[i], cust);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        internal static UnityEditor.Animations.AnimatorControllerLayer CopyLayer(UnityEditor.Animations.AnimatorControllerLayer value, UnityEditor.Animations.AnimatorController ivk, int row_role = -1, bool iscfg2 = false)
        {
            _003C_003Ec__DisplayClass128_0 connection = default(_003C_003Ec__DisplayClass128_0);
            connection.m_PublisherObserver = ivk;
            connection.configurationObserver = iscfg2;
            connection.m_WrapperServer = value;
            connection.annotationServer = new UnityEditor.Animations.AnimatorControllerLayer{name = connection.m_PublisherObserver.MakeUniqueLayerName(connection.m_WrapperServer.name)};
            CopyLayerSettings(connection.m_WrapperServer, connection.annotationServer);
            connection.iteratorObserver = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            connection._ProcObserver = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            connection.annotationServer.stateMachine = ConnectError(connection.m_WrapperServer.stateMachine, ref connection);
            CalculateError(connection.annotationServer.stateMachine, ref connection);
            TestError(connection.annotationServer.stateMachine, ref connection);
            if (row_role >= 0 && row_role <= connection.m_PublisherObserver.layers.Length - 1)
            {
                UnityEditor.Animations.AnimatorControllerLayer[] array = connection.m_PublisherObserver.layers;
                ArrayUtility.Insert(ref array, row_role, connection.annotationServer);
                connection.m_PublisherObserver.layers = array;
            }
            else
            {
                connection.m_PublisherObserver.AddLayer(connection.annotationServer);
            }

            FinalizeLayerCopy(ref connection);
            return connection.annotationServer;
        }

        internal static void CopyLayerSettings(UnityEditor.Animations.AnimatorControllerLayer key, UnityEditor.Animations.AnimatorControllerLayer ord)
        {
            UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(key.stateMachine));
            float defaultWeight = key.defaultWeight;
            if ((bool)animatorController && animatorController.layers.Length != 0 && animatorController.layers[0].stateMachine == key.stateMachine)
            {
                defaultWeight = 1f;
            }

            ord.defaultWeight = defaultWeight;
            ord.avatarMask = key.avatarMask;
            ord.blendingMode = key.blendingMode;
            ord.iKPass = key.iKPass;
            ord.syncedLayerAffectsTiming = key.syncedLayerAffectsTiming;
            ord.syncedLayerIndex = key.syncedLayerIndex;
        }

        internal static UnityEngine.AnimatorControllerParameter AddParameterIfMissing(this UnityEditor.Animations.AnimatorController asset, UnityEngine.AnimatorControllerParameter map)
        {
            bool dir;
            return asset.EnsureParameter(map, out dir);
        }

        internal static UnityEngine.AnimatorControllerParameter EnsureParameter(this UnityEditor.Animations.AnimatorController var1, UnityEngine.AnimatorControllerParameter cust, out bool dir)
        {
            float attr = 0f;
            UnityEngine.AnimatorControllerParameterType type = cust.type;
            switch (type)
            {
                case UnityEngine.AnimatorControllerParameterType.Bool:
                    attr = (cust.defaultBool ? 1 : 0);
                    break;
                case UnityEngine.AnimatorControllerParameterType.Float:
                    attr = cust.defaultFloat;
                    break;
                case UnityEngine.AnimatorControllerParameterType.Int:
                    attr = cust.defaultInt;
                    break;
            }

            return var1.EnsureParameter(cust.name, cust.type, attr, out dir);
        }

        internal static UnityEngine.AnimatorControllerParameter CloneParameter(this UnityEngine.AnimatorControllerParameter instance)
        {
            return new UnityEngine.AnimatorControllerParameter{name = instance.name, type = instance.type, defaultBool = instance.defaultBool, defaultInt = instance.defaultInt, defaultFloat = instance.defaultFloat};
        }

        internal static UnityEngine.AnimatorControllerParameter EnsureParameter(this UnityEditor.Animations.AnimatorController task, string pol, UnityEngine.AnimatorControllerParameterType proc, float attr2, out bool pred3)
        {
            UnityEngine.AnimatorControllerParameter[] parameters = task.parameters;
            int num = 0;
            UnityEngine.AnimatorControllerParameter animatorControllerParameter2;
            while (true)
            {
                if (num >= parameters.Length)
                {
                    pred3 = true;
                    UnityEngine.AnimatorControllerParameter animatorControllerParameter = new UnityEngine.AnimatorControllerParameter{name = pol, type = proc, defaultBool = (attr2 != 0f), defaultInt = (int)attr2, defaultFloat = attr2};
                    task.AddParameter(animatorControllerParameter);
                    return animatorControllerParameter;
                }

                animatorControllerParameter2 = parameters[num];
                if (!(animatorControllerParameter2.name != pol))
                {
                    if (animatorControllerParameter2.type != proc)
                    {
                        $"Type mismatch! Parameter {pol} already exists in {task.name} but with type {animatorControllerParameter2.type} rather than {proc}".ReflectResolver(LogType.Warning);
                    }

                    break;
                }

                num++;
            }

            pred3 = false;
            return animatorControllerParameter2;
        }

        internal static bool ClonePredicate(this AnimatorTransitionBase i)
        {
            while (!i.isExit)
            {
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 4:
                            break;
                        default:
                            goto end_IL_006c;
                        case 1:
                        case 2:
                            return false;
                        case 3:
                            if (!i.destinationState)
                            {
                                num = 5;
                                continue;
                            }

                            goto case 1;
                        case 5:
                            return !i.destinationStateMachine;
                    }

                    break;
                }

                continue;
                end_IL_006c:
                    break;
            }

            return true;
        }

        internal static int FindBehaviourIndex(this AnimatorState def, StateMachineBehaviour reg)
        {
            _003C_003Ec__DisplayClass142_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass142_0();
            CS_0024_003C_003E8__locals2.visitorServer = reg;
            return def.behaviours.FindResolver((StateMachineBehaviour b) => b == CS_0024_003C_003E8__locals2.visitorServer);
        }

        internal static void RemoveBehavioursOfType(this AnimatorState key, Type counter, bool isstate = false)
        {
            _003C_003Ec__DisplayClass143_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass143_0();
            CS_0024_003C_003E8__locals2.algoServer = counter;
            if (key.behaviours.ExcludeResolver((StateMachineBehaviour b) => b.GetType() == CS_0024_003C_003E8__locals2.algoServer, out var c))
            {
                key.RemoveBehaviour(c, isstate);
            }
        }

        internal static void RemoveBehaviour(this AnimatorState spec, int visitor_Ptr, bool verifytemp = false)
        {
            if (visitor_Ptr < 0)
            {
                return;
            }

            if (verifytemp)
            {
                Undo.RecordObject(spec, "Delete Behaviour");
            }

            StateMachineBehaviour stateMachineBehaviour = spec.behaviours[visitor_Ptr];
            StateMachineBehaviour[] array = spec.behaviours;
            ArrayUtility.RemoveAt(ref array, visitor_Ptr);
            spec.behaviours = array;
            string assetPath = AssetDatabase.GetAssetPath(stateMachineBehaviour);
            if (!(AssetDatabase.LoadMainAssetAtPath(assetPath) == stateMachineBehaviour))
            {
                if (!verifytemp)
                {
                    UnityEngine.Object.DestroyImmediate(stateMachineBehaviour);
                }
                else
                {
                    Undo.DestroyObjectImmediate(stateMachineBehaviour);
                }
            }
            else
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            EditorUtility.SetDirty(spec);
        }

        internal static bool ConditionsEqual(IEnumerable<AnimatorCondition> key, IEnumerable<AnimatorCondition> b)
        {
            Dictionary<AnimatorCondition, int> dictionary = new Dictionary<AnimatorCondition, int>();
            IEnumerator<AnimatorCondition> enumerator = key.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AnimatorCondition current = enumerator.Current;
                    if (!dictionary.ContainsKey(current))
                    {
                        dictionary.Add(current, 1);
                    }
                    else
                    {
                        dictionary[current]++;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            enumerator = b.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AnimatorCondition current2 = enumerator.Current;
                    if (dictionary.ContainsKey(current2))
                    {
                        dictionary[current2]--;
                        continue;
                    }

                    return false;
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }

            return dictionary.Values.All((int c) => c == 0);
        }

        internal static void ForEachLayerMachine(this UnityEditor.Animations.AnimatorController i, Action<AnimatorStateMachine> pred)
        {
            UnityEditor.Animations.AnimatorControllerLayer[] layers = i.layers;
            int j = 0;
            for (; j < layers.Length; j++)
            {
                UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = layers[j];
                pred(animatorControllerLayer.stateMachine);
            }
        }

        internal static void ForEachStateMachine(this AnimatorStateMachine init, Action<AnimatorStateMachine> col, bool containsthird = true)
        {
            col(init);
            if (containsthird)
            {
                ChildAnimatorStateMachine[] stateMachines = init.stateMachines;
                foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
                {
                    childAnimatorStateMachine.stateMachine.ForEachStateMachine(col);
                }
            }
        }

        internal static void ForEachTransition(this AnimatorStateMachine asset, Action<TransitionReference> attr, bool moveserv = true)
        {
            if (!asset)
            {
                return;
            }

            AnimatorTransition[] entryTransitions = asset.entryTransitions;
            foreach (AnimatorTransition init in entryTransitions)
            {
                attr(new TransitionReference(init, TransitionReference.TransitionSourceType.EntryTransition, asset));
            }

            AnimatorStateTransition[] anyStateTransitions = asset.anyStateTransitions;
            foreach (AnimatorStateTransition init2 in anyStateTransitions)
            {
                attr(new TransitionReference(init2, TransitionReference.TransitionSourceType.AnyTransition, asset));
            }

            ChildAnimatorState[] states = asset.states;
            for (int i = 0; i < states.Length; i++)
            {
                ChildAnimatorState childAnimatorState = states[i];
                anyStateTransitions = childAnimatorState.state.transitions;
                foreach (AnimatorStateTransition param in anyStateTransitions)
                {
                    attr(new TransitionReference(param, TransitionReference.TransitionSourceType.StateTransition, childAnimatorState.state));
                }
            }

            ChildAnimatorStateMachine[] stateMachines = asset.stateMachines;
            for (int i = 0; i < stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
                entryTransitions = asset.GetStateMachineTransitions(childAnimatorStateMachine.stateMachine);
                foreach (AnimatorTransition item in entryTransitions)
                {
                    attr(new TransitionReference(item, TransitionReference.TransitionSourceType.MachineTransition, childAnimatorStateMachine.stateMachine, asset));
                }
            }

            if (!moveserv)
            {
                return;
            }

            stateMachines = asset.stateMachines;
            for (int i = 0; i < stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine2 = stateMachines[i];
                if (childAnimatorStateMachine2.stateMachine != asset)
                {
                    childAnimatorStateMachine2.stateMachine.ForEachTransition(attr);
                }
            }
        }

        internal static void ForEachMotion(this Motion config, Action<Motion> ivk)
        {
            _003C_003Ec__DisplayClass155_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass155_0();
            CS_0024_003C_003E8__locals3._TestsServer = ivk;
            config.AnyMotion(delegate (UnityEditor.Animations.BlendTree t)
            {
                CS_0024_003C_003E8__locals3._TestsServer(t);
                return false;
            }, delegate (AnimationClip c)
            {
                CS_0024_003C_003E8__locals3._TestsServer(c);
                return false;
            });
        }

        internal static void VisitMotions(this Motion var1, Action<UnityEditor.Animations.BlendTree> vis, Action<AnimationClip> consumer)
        {
            _003C_003Ec__DisplayClass156_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass156_0();
            CS_0024_003C_003E8__locals4.m_PropertyServer = vis;
            CS_0024_003C_003E8__locals4.processorServer = consumer;
            var1.AnyMotion(delegate (UnityEditor.Animations.BlendTree t)
            {
                CS_0024_003C_003E8__locals4.m_PropertyServer(t);
                return false;
            }, delegate (AnimationClip c)
            {
                CS_0024_003C_003E8__locals4.processorServer(c);
                return false;
            });
        }

        internal static bool AnyMotion(this Motion i, Func<UnityEditor.Animations.BlendTree, bool> result, Func<AnimationClip, bool> pool)
        {
            if (i == null)
            {
                return false;
            }

            if (!(i is UnityEditor.Animations.BlendTree blendTree))
            {
                if (pool != null && i is AnimationClip arg && pool(arg))
                {
                    return true;
                }
            }
            else
            {
                if (result != null && result(blendTree))
                {
                    return true;
                }

                ChildMotion[] children = blendTree.children;
                foreach (ChildMotion childMotion in children)
                {
                    if (childMotion.motion.AnyMotion(result, pool))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static void AssetPredicate(this AnimatorStateMachine param, Action<AnimatorState> second, bool requiresc = true)
        {
            ChildAnimatorState[] states = param.states;
            foreach (ChildAnimatorState childAnimatorState in states)
            {
                second(childAnimatorState.state);
            }

            if (!requiresc)
            {
                return;
            }

            ChildAnimatorStateMachine[] stateMachines = param.stateMachines;
            for (int i = 0; i < stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
                if (childAnimatorStateMachine.stateMachine != param)
                {
                    childAnimatorStateMachine.stateMachine.AssetPredicate(second);
                }
            }
        }

        internal static bool PatchPredicate(UnityEngine.Object param, out UnityEngine.Object result)
        {
            result = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(param));
            return param != result;
        }

        internal static void InterruptPredicate(UnityEngine.Object asset, UnityEngine.Object counter, bool isutil = true, bool removeinstance2 = false)
        {
            AssetDatabase.AddObjectToAsset(asset, counter);
            if (isutil)
            {
                asset.hideFlags |= HideFlags.HideInHierarchy;
                asset.hideFlags |= HideFlags.HideInInspector;
            }

            if (removeinstance2)
            {
                asset.hideFlags |= HideFlags.NotEditable;
            }

            EditorUtility.SetDirty(counter);
        }

        internal static void ManagePredicate(this UnityEngine.Object first)
        {
            EditorUtility.SetDirty(first);
        }

        internal static void PrintPredicate(this UnityEngine.Object last)
        {
            try
            {
                if (!MapError(last))
                {
                    EditorUtility.SetDirty(last);
                }

                if (!(last is GameObject gameObject))
                {
                    if (last is Component component)
                    {
                        EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                    }
                }
                else
                {
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
            catch
            {
            }
        }

        internal static T CompareRules<T>(T task)
            where T : UnityEngine.Object
        {
            if (!task)
            {
                return null;
            }

            Type type = task.GetType();
            UnityEngine.Object obj = ((!type.IsSubclassOf(typeof(ScriptableObject)) && !(type == typeof(ScriptableObject))) ? ((UnityEngine.Object)Activator.CreateInstance(type)) : ScriptableObject.CreateInstance(type));
            EditorUtility.CopySerialized(task, obj);
            return (T)obj;
        }

        internal static T SetRules<T>(T reference, string vis, out bool proc, bool isinfo2 = true)
            where T : UnityEngine.Object
        {
            proc = PatchPredicate(reference, out var _);
            T val = CompareRules(reference);
            if (!proc || isinfo2)
            {
                AssetDatabase.CreateAsset(val, vis);
            }

            return val;
        }

        internal static string EnableRules(string asset, string pol)
        {
            GUILayout.HorizontalScope horizontalScope = new GUILayout.HorizontalScope();
            string result;
            try
            {
                EditorGUILayout.PrefixLabel(pol);
                EditorGUILayout.SelectableLabel(asset, EditorStyles.objectField, GUILayout.Height(16f), GUILayout.ExpandWidth(expand: true));
                if (!CallQueue(DestroyError().invocationProcessor))
                {
                    goto IL_0300;
                }

                string text = asset;
                if (!text.StartsWith("Assets"))
                {
                    text = "Assets";
                }
                else
                {
                    while (!AssetDatabase.IsValidFolder(text))
                    {
                        text = Path.GetDirectoryName(text);
                    }
                }

                string text2 = EditorUtility.OpenFolderPanel(pol, text, string.Empty);
                if (string.IsNullOrEmpty(text2))
                {
                    result = asset;
                }
                else
                {
                    string projectRelativePath = FileUtil.GetProjectRelativePath(text2);
                    if (projectRelativePath.StartsWith("Assets"))
                    {
                        asset = projectRelativePath;
                        goto IL_0300;
                    }

                    "New Path must be a folder within Assets!".ReflectResolver(LogType.Warning);
                    result = asset;
                }

                goto end_IL_000c;
                IL_0300:
                    using (new EditorGUI.DisabledScope(!AssetDatabase.IsValidFolder(asset)))
                    {
                        if (CallQueue(DestroyError()._FactoryProcessor))
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset));
                        }
                    }

                return asset;
                end_IL_000c:
                    ;
            }
            finally
            {
                if (horizontalScope != null)
                {
                    ((IDisposable)horizontalScope).Dispose();
                }
            }

            return result;
        }

        internal static bool ConcatRules(this UnityEngine.Object key)
        {
            return key == null;
        }

        internal static void CountRules(Action spec)
        {
            while (true)
            {
                bool num = _MethodProperty.Count == 0;
                _MethodProperty.Enqueue(spec);
                int num2;
                if (!num)
                {
                    num2 = 5;
                    goto IL_001d;
                }

                goto IL_0151;
                IL_001d:
                    switch (num2)
                    {
                        case 3:
                            return;
                        case 5:
                            return;
                        case 2:
                            return;
                        case 1:
                            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
                            return;
                        case 6:
                            continue;
                        case 4:
                            goto IL_0151;
                    }

                goto IL_0065;
                IL_0151:
                    ;
                goto IL_0065;
                IL_0065:
                    EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
                num2 = 1;
                goto IL_001d;
            }
        }

        internal static void DisableRules(Action config, bool isb = true)
        {
            bool num = _SchemaProperty.Count == 0;
            _SchemaProperty.Enqueue(config);
            if (num)
            {
                EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
                EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
            }

            if (isb)
            {
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private static void InsertRules()
        {
            while (_MethodProperty.Count != 0)
            {
                Action action = _MethodProperty.Dequeue();
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
        }

        private static void RestartRules(int end_init, Rect connection)
        {
            while (_SchemaProperty.Count != 0)
            {
                Action action = _SchemaProperty.Dequeue();
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }

            EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
        }

        internal static AvatarMask AddRules(UnityEditor.Animations.AnimatorController setup)
        {
            bool flag = setup.layers[0].avatarMask != null;
            if (flag && EditorUtility.DisplayDialog("Existing Mask!", "The Base Layer already uses a mask! Continue Anyway?", "Continue", "Cancel"))
            {
                flag = false;
            }

            if (!flag)
            {
                AvatarMask avatarMask = RemoveRules();
                avatarMask.ExcludeRules();
                for (int i = 1; i < setup.layers.Length; i++)
                {
                    UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = setup.layers[i];
                    if ((bool)animatorControllerLayer.avatarMask)
                    {
                        avatarMask.DefineRules(animatorControllerLayer.avatarMask);
                    }
                }

                avatarMask.InitRules();
                return avatarMask;
            }

            return null;
        }

        internal static AvatarMask InvokeRules(UnityEditor.Animations.AnimatorController asset, Transform cust)
        {
            AvatarMask avatarMask = RemoveRules();
            avatarMask.ExcludeRules();
            UnityEditor.Animations.AnimatorControllerLayer[] layers = asset.layers;
            foreach (UnityEditor.Animations.AnimatorControllerLayer var in layers)
            {
                avatarMask.DefineRules(FindRules(var, cust));
            }

            avatarMask.InitRules();
            return avatarMask;
        }

        internal static AvatarMask FindRules(UnityEditor.Animations.AnimatorControllerLayer var1, Transform reg)
        {
            _003C_003Ec__DisplayClass196_0 CS_0024_003C_003E8__locals16 = new _003C_003Ec__DisplayClass196_0();
            CS_0024_003C_003E8__locals16.m_RulesServer = reg;
            CS_0024_003C_003E8__locals16.m_PredicateServer = new AvatarMask();
            Transform transform = CS_0024_003C_003E8__locals16.m_RulesServer;
            if (!CS_0024_003C_003E8__locals16.m_RulesServer)
            {
                transform = new GameObject("Dummy").transform;
            }

            CS_0024_003C_003E8__locals16.m_PredicateServer.AddTransformPath(transform, recursive: false);
            if (!CS_0024_003C_003E8__locals16.m_RulesServer)
            {
                UnityEngine.Object.DestroyImmediate(transform.gameObject);
            }

            for (int i = 0; i < 13; i++)
            {
                CS_0024_003C_003E8__locals16.m_PredicateServer.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: false);
            }

            CS_0024_003C_003E8__locals16.m_QueueServer = new HashSet<string>();
            var1.stateMachine.AssetPredicate(delegate (AnimatorState s)
            {
                s.motion.VisitMotions(null, delegate (AnimationClip c)
                {
                    IEnumerator<EditorCurveBinding> enumerator = AnimationUtility.GetCurveBindings(c).Concat(AnimationUtility.GetObjectReferenceCurveBindings(c)).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            EditorCurveBinding current = enumerator.Current;
                            if (current.path == string.Empty)
                            {
                                if (current.type == typeof(Animator) && SelectRules(current.propertyName, out var visitor))
                                {
                                    CS_0024_003C_003E8__locals16.m_PredicateServer.SetHumanoidBodyPartActive(visitor, value: true);
                                }
                            }
                            else
                            {
                                MessageServer messageServer = null;
                                Transform transform2;
                                if ((bool)CS_0024_003C_003E8__locals16.m_RulesServer)
                                {
                                    transform2 = CS_0024_003C_003E8__locals16.m_RulesServer.Find(current.path);
                                }
                                else
                                {
                                    messageServer = new MessageServer(current.path);
                                    transform2 = messageServer.m_CollectionServer.Last().transform;
                                }

                                if ((bool)transform2 && !CS_0024_003C_003E8__locals16.m_QueueServer.Contains(current.path))
                                {
                                    CS_0024_003C_003E8__locals16.m_PredicateServer.AddTransformPath(transform2, recursive: false);
                                    CS_0024_003C_003E8__locals16.m_QueueServer.Add(current.path);
                                }

                                messageServer?.EnableContext();
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator != null)
                        {
                            enumerator.Dispose();
                        }
                    }
                });
            });
            CS_0024_003C_003E8__locals16.m_PredicateServer.InitRules();
            return CS_0024_003C_003E8__locals16.m_PredicateServer;
        }

        internal static void ExcludeRules(this AvatarMask spec, bool compareselection = false)
        {
            if (!compareselection)
            {
                if (spec.transformCount != 0)
                {
                    return;
                }
            }

            GameObject gameObject = new GameObject();
            spec.AddTransformPath(gameObject.transform);
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        internal static void InitRules(this AvatarMask config, bool compareb = false)
        {
            config.ExcludeRules();
            if (compareb || config.transformCount <= 1)
            {
                GameObject gameObject = new GameObject();
                GameObject gameObject2 = new GameObject("Dummy Transform");
                gameObject2.transform.parent = gameObject.transform;
                config.AddTransformPath(gameObject2.transform);
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(gameObject2);
            }
        }

        internal static void VisitRules(this AvatarMask res, string connection)
        {
            while (true)
            {
                MessageServer messageServer = new MessageServer(connection);
                int num = 0;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            return;
                        case 5:
                            return;
                        case 3:
                            messageServer.EnableContext();
                            num = 2;
                            continue;
                        case 1:
                            break;
                        case 4:
                            res.AddTransformPath(messageServer.m_CollectionServer.Last().transform);
                            num = 3;
                            continue;
                        default:
                            goto case 4;
                    }

                    break;
                }
            }
        }

        internal static void DefineRules(this AvatarMask v, AvatarMask token)
        {
            _003C_003Ec__DisplayClass200_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass200_0();
            int i = 0;
            for (; i < 13; i++)
            {
                if (token.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i))
                {
                    v.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: true);
                }
            }

            List<(string, bool)> list = token.StartRules();
            if (list.Count <= 0)
            {
                return;
            }

            CS_0024_003C_003E8__locals3.setterServer = v.ReadRules();
            IEnumerator<(string, bool)> enumerator = list.Where(delegate ((string, bool) s)
            {
                if (!CS_0024_003C_003E8__locals3.setterServer.Contains(s.Item1))
                {
                    return s.Item2;
                }

                return false;
            }).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string item = enumerator.Current.Item1;
                    v.VisitRules(item);
                    CS_0024_003C_003E8__locals3.setterServer.Add(item);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
        }

        internal static List<(string, bool)> StartRules(this AvatarMask def)
        {
            List<(string, bool)> list = new List<(string, bool)>();
            int transformCount = def.transformCount;
            for (int i = 0; i < transformCount; i++)
            {
                list.Add((def.GetTransformPath(i), def.GetTransformActive(i)));
            }

            return list;
        }

        internal static HashSet<string> ReadRules(this AvatarMask task)
        {
            HashSet<string> hashSet = new HashSet<string>();
            int transformCount = task.transformCount;
            for (int i = 0; i < transformCount; i++)
            {
                hashSet.Add(task.GetTransformPath(i));
            }

            return hashSet;
        }

        internal static bool SelectRules(string setup, out AvatarMaskBodyPart visitor)
        {
            string[] source = new string[3]{"Arm", "Forearm", "Shoulder"};
            string[] source2 = new string[3]{"Leg", "Foot", "Toes"};
            string[] source3 = new string[4]{"Neck", "Head", "Eye", "Jaw"};
            string[] source4 = new string[2]{"Chest", "Spine"};
            bool flag = setup.Contains("Left");
            if (!setup.Contains("Hand"))
            {
                if (!setup.Contains("Root"))
                {
                    if (source4.Any(setup.Contains))
                    {
                        visitor = AvatarMaskBodyPart.Body;
                        return true;
                    }

                    if (source.Any(setup.Contains))
                    {
                        visitor = (flag ? AvatarMaskBodyPart.LeftArm : AvatarMaskBodyPart.RightArm);
                        return true;
                    }

                    if (!source2.Any(setup.Contains))
                    {
                        if (source3.Any(setup.Contains))
                        {
                            visitor = AvatarMaskBodyPart.Head;
                            return true;
                        }

                        visitor = AvatarMaskBodyPart.LastBodyPart;
                        return false;
                    }

                    visitor = (flag ? AvatarMaskBodyPart.LeftLeg : AvatarMaskBodyPart.RightLeg);
                    return true;
                }

                visitor = AvatarMaskBodyPart.Root;
                return true;
            }

            visitor = (flag ? AvatarMaskBodyPart.LeftFingers : AvatarMaskBodyPart.RightFingers);
            return true;
        }

        internal static AvatarMask RemoveRules()
        {
            AvatarMask avatarMask = new AvatarMask();
            int i = 0;
            for (; i < 13; i++)
            {
                avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: false);
            }

            return avatarMask;
        }

        internal static void InstantiateRules<T>(Rect ident, Action<T> ivk, Func<T, bool> control = null, Action ident2 = null)
            where T : UnityEngine.Object
        {
            _003C_003Ec__DisplayClass205_0<T> CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass205_0<T>();
            CS_0024_003C_003E8__locals3.acceptConditions = control;
            Event current = Event.current;
            if ((current.type != EventType.DragPerform && current.type != EventType.DragUpdated) || !ident.Contains(current.mousePosition))
            {
                return;
            }

            T val = (typeof(T).IsSubclassOf(typeof(Component)) ? DragAndDrop.objectReferences.Select(delegate (UnityEngine.Object o)
            {
                GameObject obj = o as GameObject;
                return ((object)obj == null) ? null : obj.GetComponent<T>();
            }).FirstOrDefault(delegate (T c)
            {
                if (c != null)
                {
                    return CS_0024_003C_003E8__locals3.acceptConditions?.Invoke(c) ?? true;
                }

                return false;
            }) : DragAndDrop.objectReferences.OfType<T>().FirstOrDefault((T el) => CS_0024_003C_003E8__locals3.acceptConditions?.Invoke(el) ?? true));
            bool flag = val != null;
            if (flag)
            {
                ident2?.Invoke();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            if (current.type == EventType.DragPerform && flag)
            {
                DragAndDrop.AcceptDrag();
                ivk(val);
            }

            current.Use();
        }

        internal static void AwakeRules<T>(Rect value, Action<IEnumerable<T>> ord, Func<T, bool> filter = null, Action asset2 = null)
            where T : UnityEngine.Object
        {
            _003C_003Ec__DisplayClass206_0<T> CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass206_0<T>();
            CS_0024_003C_003E8__locals3.acceptConditions = filter;
            Event current = Event.current;
            if (current.type != EventType.DragPerform)
            {
                if (current.type != EventType.DragUpdated)
                {
                    return;
                }
            }

            if (!value.Contains(current.mousePosition))
            {
                return;
            }

            T[] array = (typeof(T).IsSubclassOf(typeof(Component)) ? DragAndDrop.objectReferences.Select(delegate (UnityEngine.Object o)
            {
                GameObject obj = o as GameObject;
                return ((object)obj == null) ? null : obj.GetComponent<T>();
            }).Where(delegate (T c)
            {
                if (c != null)
                {
                    return CS_0024_003C_003E8__locals3.acceptConditions?.Invoke(c) ?? true;
                }

                return false;
            }).ToArray() : DragAndDrop.objectReferences.OfType<T>().Where(delegate (T o)
            {
                if (o != null)
                {
                    return CS_0024_003C_003E8__locals3.acceptConditions?.Invoke(o) ?? true;
                }

                return false;
            }).ToArray());
            bool flag = array.Length != 0;
            if (flag)
            {
                asset2?.Invoke();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            if (current.type == EventType.DragPerform && flag)
            {
                DragAndDrop.AcceptDrag();
                ord(array);
            }

            current.Use();
        }

        internal static bool GetRules<T>(this T[] key, bool requiresord = false)
        {
            if (key != null)
            {
                if (key.Length != 0)
                {
                    if (requiresord)
                    {
                        return key.All((T e) => e == null);
                    }

                    return false;
                }
            }

            return true;
        }

        internal static bool CalcRules(this IList last, bool dosecond = false)
        {
            if (last != null)
            {
                if (last.Count != 0)
                {
                    if (!dosecond)
                    {
                        return false;
                    }

                    return last.Cast<object>().All((object e) => e == null);
                }
            }

            return true;
        }

        internal static IEnumerable<T> CloneRules<T>(this IEnumerable<T> info)
        {
            return info.Where((T x) => x != null);
        }

        internal static Component ListRules(this GameObject var1, Type counter)
        {
            Component component = var1.GetComponent(counter);
            if (component == null)
            {
                component = var1.AddComponent(counter);
            }

            return component;
        }

        internal static Type FillRules(string config)
        {
            Type type = Type.GetType(config);
            if (type == null)
            {
                throw new Exception("Type \"" + config + "\" not found.");
            }

            return type;
        }

        internal static Type WriteRules(string param)
        {
            Type type = ForgotRules(param);
            if (type == null)
            {
                throw new Exception("Type \"" + param + "\" not found.");
            }

            return type;
        }

        internal static Type ForgotRules(string task)
        {
            _003C_003Ec__DisplayClass237_0 CS_0024_003C_003E8__locals4 = new _003C_003Ec__DisplayClass237_0();
            CS_0024_003C_003E8__locals4.contextServer = task;
            Type type = Type.GetType(CS_0024_003C_003E8__locals4.contextServer);
            if (type != null)
            {
                return type;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int num = 0;
            while (true)
            {
                if (num < assemblies.Length)
                {
                    Type[] types = assemblies[num].GetTypes();
                    type = types.FirstOrDefault((Type t) => t.FullName == CS_0024_003C_003E8__locals4.contextServer);
                    if (type != null)
                    {
                        return type;
                    }

                    type = types.FirstOrDefault((Type t) => t.Name == CS_0024_003C_003E8__locals4.contextServer);
                    if (type != null)
                    {
                        break;
                    }

                    num++;
                    continue;
                }

                return null;
            }

            return type;
        }

        internal static T[] CheckRules<T>(params T[] arg)
        {
            return arg;
        }

        internal static string ChangeRules(string setup, IEnumerable<string> ord)
        {
            _003C_003Ec__DisplayClass243_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass243_0();
            CS_0024_003C_003E8__locals2.m_ConsumerServer = (ord as HashSet<string>) ?? new HashSet<string>(ord);
            return SortRules(setup, (string s) => !CS_0024_003C_003E8__locals2.m_ConsumerServer.Contains(s));
        }

        internal static string SortRules(string instance, Func<string, bool> cfg)
        {
            if (cfg(instance))
            {
                return instance;
            }

            if (!LogoutRules(instance, out var i))
            {
                i = 1;
            }

            string arg;
            for (arg = RegisterRules(instance); !cfg($"{arg} {i}"); i++)
            {
            }

            return $"{arg} {i}";
        }

        internal static string RegisterRules(string ident)
        {
            return Regex.Replace(ident, "(?=.*) \\d+$", string.Empty);
        }

        internal static bool LogoutRules(string info, out int b)
        {
            b = 0;
            if (!string.IsNullOrWhiteSpace(info))
            {
                Match match = Regex.Match(info, "(?=.*)(\\d+)$");
                if (match.Success)
                {
                    b = int.Parse(match.Groups[1].Value);
                    return true;
                }

                return false;
            }

            return false;
        }

        internal static void SearchRules(string task, object cfg)
        {
            while (serviceProperty == null)
            {
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            serviceProperty = new Dictionary<string, object>();
                            num = 3;
                            continue;
                        case 5:
                            return;
                        case 2:
                            break;
                        case 3:
                        case 4:
                            goto end_IL_00a2;
                        case 1:
                            goto default;
                    }

                    break;
                }

                continue;
                end_IL_00a2:
                    break;
            }

            serviceProperty[task] = cfg;
        }

        internal static T OrderQueue<T>(string last, T attr)
        {
            if (serviceProperty == null)
            {
                return attr;
            }

            object value;
            return (T)(serviceProperty.TryGetValue(last, out value) ? value : ((object)attr));
        }

        internal static void CompareQueue(string v, object vis, EventType dic)
        {
            SetQueue(v, vis, dic, Event.current.type);
        }

        internal static void SetQueue(string init, object attr, EventType proc, EventType var12)
        {
            while (true)
            {
                int num;
                if (var12 == proc)
                {
                    SearchRules(init, attr);
                    num = 2;
                }
                else
                {
                    num = 3;
                }

                switch (num)
                {
                    default:
                        return;
                    case 0:
                        return;
                    case 1:
                        return;
                    case 2:
                        return;
                    case 4:
                        break;
                    case 3:
                        return;
                }
            }
        }

        internal static float PostQueue(Enum key, GUIStyle second = null)
        {
            return SetupQueue(key.ToString(), second);
        }

        internal static float SetupQueue(string res, GUIStyle cust = null)
        {
            if (cust == null)
            {
                cust = GUI.skin.label;
            }

            return cust.CalcSize(res.CreateResolver()).x;
        }

        internal static bool CallQueue(GUIContent setup, float cust = -1f, float consumer = -1f)
        {
            if (cust == -1f)
            {
                cust = EditorGUIUtility.singleLineHeight;
            }

            if (consumer == -1f)
            {
                consumer = EditorGUIUtility.singleLineHeight;
            }

            bool result = GUILayout.Button(setup, CalcError().broadcasterProcessor, GUILayout.Width(cust), GUILayout.Height(consumer));
            StartQueue();
            return result;
        }

        internal static bool CancelQueue(Rect var1, string counter, GUIStyle control = null)
        {
            return QueryQueue(var1, new GUIContent(counter), control);
        }

        internal static bool CountQueue(string info, GUIStyle result = null, params GUILayoutOption[] options)
        {
            return RestartQueue(new GUIContent(info), result, options);
        }

        internal static bool DisableQueue(string res, params GUILayoutOption[] options)
        {
            return RestartQueue(new GUIContent(res), null, options);
        }

        internal static bool RestartQueue(GUIContent item, GUIStyle selection = null, params GUILayoutOption[] options)
        {
            return ExcludeQueue(isfirst: false, item, selection, options);
        }

        internal static bool QueryQueue(Rect item, GUIContent second, GUIStyle helper = null)
        {
            if (helper == null)
            {
                helper = GUI.skin.button;
            }

            bool result = GUI.Button(item, second, helper);
            StartQueue(item);
            return result;
        }

        internal static bool AddQueue(bool extractparam, string vis, GUIStyle control = null, params GUILayoutOption[] options)
        {
            return ExcludeQueue(extractparam, new GUIContent(vis), control, options);
        }

        internal static bool InvokeQueue(bool isident, string map, params GUILayoutOption[] options)
        {
            return ExcludeQueue(isident, new GUIContent(map), null, options);
        }

        internal static bool FindQueue(bool isident, GUIContent reg, params GUILayoutOption[] options)
        {
            return ExcludeQueue(isident, reg, null, options);
        }

        internal static bool ExcludeQueue(bool isfirst, GUIContent visitor, GUIStyle consumer = null, params GUILayoutOption[] options)
        {
            while (true)
            {
                int num;
                if (consumer != null)
                {
                    num = 1;
                    goto IL_001d;
                }

                goto IL_0105;
                IL_001d:
                    switch (num)
                    {
                        case 1:
                        case 4:
                        case 5:
                        {
                            bool result = GUILayout.Toggle(isfirst, visitor, consumer, options);
                            StartQueue();
                            return result;
                        }

                        case 3:
                            break;
                        case 2:
                            continue;
                        default:
                            goto IL_0105;
                    }

                goto IL_009c;
                IL_0105:
                    ;
                goto IL_009c;
                IL_009c:
                    consumer = GUI.skin.button;
                num = 5;
                goto IL_001d;
            }
        }

        internal static bool DefineQueue(Rect v = default(Rect))
        {
            if (v == default(Rect))
            {
                v = GUILayoutUtility.GetLastRect();
            }

            StartQueue(v);
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0)
            {
                return v.Contains(current.mousePosition);
            }

            return false;
        }

        internal static void StartQueue(Rect spec = default(Rect), bool containsvisitor = false)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (spec == default(Rect))
                {
                    spec = GUILayoutUtility.GetLastRect();
                }

                AwakeQueue(spec, MouseCursor.Link, containsvisitor);
            }
        }

        internal static void AwakeQueue(Rect ident, MouseCursor pred, bool hascontrol = false)
        {
            if (!GUI.enabled)
            {
                if (!hascontrol)
                {
                    return;
                }
            }

            if (!globalProperty)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUIUtility.AddCursorRect(ident, pred);
                }

                return;
            }

            if (m_ProxyProperty)
            {
                ident.y += 46f;
            }

            stateProperty.Push((GUIUtility.GUIToScreenRect(ident), pred));
        }

        internal static void ResetQueue(Rect config, Action<float> col)
        {
            Event current = Event.current;
            if (current.type == EventType.ScrollWheel)
            {
                if (config == default(Rect))
                {
                    config = GUILayoutUtility.GetLastRect();
                }

                if (config.Contains(current.mousePosition))
                {
                    col(current.delta.y);
                }
            }
        }

        internal static void FlushQueue(Rect res, string cfg, float dic = 0f, float pol2 = 0f, bool stripresult3 = true, GUIStyle cust4 = null)
        {
            ConnectQueue(res, cfg, overridethird: true, dic, pol2, stripresult3, cust4);
        }

        internal static void ConnectQueue(Rect key, string caller, bool overridethird = true, float second2 = 0f, float pred3 = 0f, bool isparam4 = true, GUIStyle pol5 = null)
        {
            if (!overridethird)
            {
            }
            else if (key.width > second2 + pred3)
            {
                if (!isparam4)
                {
                    key.x -= pred3 + 2.5f;
                }
                else
                {
                    key.x += pred3 + 2.5f;
                }

                GUI.Label(key, caller, pol5 ?? (isparam4 ? CalcError().annotationObserver : CalcError().algoObserver));
            }
        }

        internal static void TestQueue(string res, bool iscfg = true, float tag = 0f, float def2 = 0f, bool isivk3 = true)
        {
            ConnectQueue(GUILayoutUtility.GetLastRect(), res, iscfg, tag, def2, isivk3);
        }

        internal static void MapQueue(int idx_var1 = 2, int contHigh = 10, int state_start = 0)
        {
            Rect controlRect;
            if (state_start <= 0)
            {
                controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(idx_var1 + contHigh));
            }
            else
            {
                controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(idx_var1 + contHigh), GUILayout.MaxWidth(state_start));
            }

            Rect rect = controlRect;
            rect.height = idx_var1;
            rect.y += (float)contHigh / 2f;
            rect.x -= 2f;
            rect.width += 6f;
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out var color);
            EditorGUI.DrawRect(rect, color);
        }

        internal static GUIContent PushQueue(string config, string selection = null)
        {
            return new GUIContent(EditorGUIUtility.IconContent(config))
            {tooltip = selection};
        }

        [SpecialName]
        internal static WatcherProcessor DestroyError()
        {
            return _InitializerProcessor ?? (_InitializerProcessor = new WatcherProcessor());
        }

        [SpecialName]
        internal static BaseProcessor CalcError()
        {
            return _DefinitionProcessor ?? (_DefinitionProcessor = new BaseProcessor());
        }

        internal static bool ViewQueue(EventCommands setup, string result = "", bool consumerinstall = true)
        {
            if (!string.IsNullOrEmpty(result))
            {
                if (GUI.GetNameOfFocusedControl() != result)
                {
                    return false;
                }
            }

            Event current = Event.current;
            if (current.type != EventType.ExecuteCommand && current.type != EventType.ValidateCommand)
            {
                return false;
            }

            bool num = setup.ToString() == current.commandName;
            if (num && consumerinstall)
            {
                current.Use();
            }

            return num;
        }

        internal static bool VerifyQueue(string last = "", bool havesecond = true)
        {
            if (ViewQueue(EventCommands.SoftDelete, last, havesecond))
            {
                return true;
            }

            return ViewQueue(EventCommands.Delete, last, havesecond);
        }

        internal static void ConcatList(UnityEngine.Object info, Type pol, UnityEngine.Object role = null, SerializedProperty pol2 = null, bool loaddef3 = true, List<int> task4 = null, Action<UnityEngine.Object> result5 = null, Action<UnityEngine.Object> config6 = null, bool rejectresult7 = true)
        {
            if (observerProcessor == null)
            {
                observerProcessor = Type.GetType("UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            }

            if (processorProcessor == null)
            {
                Type[] second = new Type[4]{typeof(bool), typeof(List<int>), typeof(Action<UnityEngine.Object>), typeof(Action<UnityEngine.Object>)};
                Type[] types = new Type[3]{typeof(UnityEngine.Object), typeof(Type), typeof(UnityEngine.Object)}.Concat(second).Concat(new Type[1]{typeof(bool)}).ToArray();
                processorProcessor = observerProcessor.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);
                m_ServerProcessor = processorProcessor != null;
                if (!m_ServerProcessor)
                {
                    Type[] types2 = new Type[3]{typeof(UnityEngine.Object), typeof(Type), typeof(SerializedProperty)}.Concat(second).ToArray();
                    processorProcessor = observerProcessor.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types2, null);
                }
            }

            EditorWindow window = EditorWindow.GetWindow(observerProcessor);
            object[] second2 = new object[4]{loaddef3, task4, result5, config6};
            second2 = (m_ServerProcessor ? new object[3]{info, pol, role}.Concat(second2).Concat(new object[1]{rejectresult7}).ToArray() : new object[3]{info, pol, pol2}.Concat(second2).ToArray());
            processorProcessor.Invoke(window, second2);
        }

        internal static IList CountList(this GenericMenu init)
        {
            if (!errorProcessor)
            {
                errorProcessor = true;
                queueProcessor = typeof(GenericMenu).RestartList("menuItems");
                if (queueProcessor == null)
                {
                    queueProcessor = typeof(GenericMenu).RestartList("m_MenuItems");
                }
            }

            if (queueProcessor == null)
            {
                return null;
            }

            return (IList)queueProcessor.GetValue(init);
        }

        internal static MethodInfo DisableList(this Type i, string counter)
        {
            return i.GetMethod(counter, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        internal static MethodInfo InsertList(this Type init, string result, Type[] template)
        {
            return init.GetMethod(result, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, template, null);
        }

        internal static FieldInfo RestartList(this Type i, string ord)
        {
            return i.GetField(ord, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        internal static PropertyInfo QueryList(this Type i, string visitor)
        {
            return i.GetProperty(visitor, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        internal static void RemoveList(string first)
        {
            while (true)
            {
                int num;
                if (Directory.Exists(first))
                {
                    num = 2;
                    goto IL_001d;
                }

                goto IL_00ea;
                IL_001d:
                    switch (num)
                    {
                        default:
                            return;
                        case 2:
                            return;
                        case 0:
                            return;
                        case 1:
                            return;
                        case 3:
                            break;
                        case 4:
                            goto IL_0086;
                        case 5:
                            goto IL_00ea;
                    }

                continue;
                IL_00ea:
                    ;
                goto IL_0086;
                IL_0086:
                    Directory.CreateDirectory(first);
                num = 1;
                goto IL_001d;
            }
        }

        internal static string InstantiateList(string var1, bool overridecust = false, PathOption filter = PathOption.Normal)
        {
            bool flag = filter == PathOption.ForceFolder;
            bool flag2 = filter == PathOption.ForceFile;
            var1 = (flag2 ? CalculateList(var1) : (flag ? ConnectList(var1) : FlushList(var1)));
            if (flag || (!flag2 && string.IsNullOrEmpty(Path.GetExtension(var1))))
            {
                if (!Directory.Exists(var1))
                {
                    Directory.CreateDirectory(var1);
                    AssetDatabase.ImportAsset(var1);
                }
                else if (overridecust)
                {
                    var1 = AssetDatabase.GenerateUniqueAssetPath(var1);
                    Directory.CreateDirectory(var1);
                    AssetDatabase.ImportAsset(var1);
                }
            }
            else
            {
                string text = Path.GetDirectoryName(var1);
                string fileName = Path.GetFileName(var1);
                if (!string.IsNullOrEmpty(text))
                {
                    if (!text.StartsWith(Application.dataPath) && !text.StartsWith("Assets"))
                    {
                        text = "Assets/" + text;
                    }
                }
                else
                {
                    text = "Assets";
                }

                if (text != "Assets" && !Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    AssetDatabase.ImportAsset(text);
                }

                var1 = text + "/" + fileName;
                if (overridecust)
                {
                    var1 = AssetDatabase.GenerateUniqueAssetPath(var1);
                }
            }

            return var1;
        }

        internal static string AwakeList(string i, string reg, bool writestate = false)
        {
            if (string.IsNullOrEmpty(reg))
            {
                return InstantiateList(ConnectList(i), writestate, PathOption.ForceFolder);
            }

            if (string.IsNullOrEmpty(i))
            {
                return InstantiateList(CalculateList(reg), writestate, PathOption.ForceFile);
            }

            return InstantiateList(ConnectList(i) + "/" + CalculateList(reg), writestate);
        }

        internal static string FlushList(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                UnityEngine.Debug.LogWarning("Legalizing empty path! Returned path as 'EmptyPath'");
                return "EmptyPath";
            }

            string extension = Path.GetExtension(param);
            if (!string.IsNullOrEmpty(extension))
            {
                string directoryName = Path.GetDirectoryName(param);
                string text = CalculateList(Path.GetFileNameWithoutExtension(param));
                if (string.IsNullOrEmpty(directoryName))
                {
                    return text + extension;
                }

                directoryName = ConnectList(directoryName);
                return directoryName + "/" + text + extension;
            }

            return ConnectList(param);
        }

        internal static string ConnectList(string init)
        {
            _003C_003Ec__DisplayClass410_0 CS_0024_003C_003E8__locals2 = new _003C_003Ec__DisplayClass410_0();
            CS_0024_003C_003E8__locals2._CustomerServer = Regex.Escape(new string (Path.GetInvalidPathChars()));
            init = init.Replace('\\', '/');
            if (init.IndexOf('/') > 0)
            {
                init = string.Join("/", from s in init.Split(new char[1]{'/'})select Regex.Replace(s, "[" + CS_0024_003C_003E8__locals2._CustomerServer + "]", "-"));
            }

            return init;
        }

        internal static string CalculateList(string setup)
        {
            while (true)
            {
                string text = Regex.Escape(new string (Path.GetInvalidFileNameChars()));
                int num = 3;
                while (true)
                {
                    switch (num)
                    {
                        case 1:
                            return Regex.Replace(setup, "[" + text + "]", "-");
                        case 4:
                            break;
                        default:
                            if (!string.IsNullOrEmpty(setup))
                            {
                                goto IL_00ae;
                            }

                            goto case 2;
                        case 3:
                            goto default;
                        case 2:
                            return "Unnamed";
                    }

                    break;
                    IL_00ae:
                        num = 1;
                }
            }
        }

        private static Texture2D CloneList(Texture2D ident, float reg = 0.2f, int position_dic = 1)
        {
            if (ident == null)
            {
                throw new ArgumentNullException("texture");
            }

            ReadableTextureScope consumerPolicy = new ReadableTextureScope(ident);
            try
            {
                Texture2D interpreterPolicy = consumerPolicy.m_InterpreterPolicy;
                int width = interpreterPolicy.width;
                int height = interpreterPolicy.height;
                int num = width;
                int num2 = 0;
                int num3 = height;
                int num4 = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (interpreterPolicy.GetPixel(j, i).a >= reg)
                        {
                            if (j < num)
                            {
                                num = j;
                            }

                            if (j > num2)
                            {
                                num2 = j;
                            }

                            if (i < num3)
                            {
                                num3 = i;
                            }

                            if (i > num4)
                            {
                                num4 = i;
                            }
                        }
                    }
                }

                int num5 = num2 - num + 1;
                int num6 = num4 - num3 + 1;
                int num7 = num5 + position_dic * 2;
                int num8 = num6 + position_dic * 2;
                if (num5 < 1 || num6 < 1)
                {
                    UnityEngine.Debug.LogError("Trimmed texture has zero size.");
                    return null;
                }

                Color[] pixels = interpreterPolicy.GetPixels(num, num3, num5, num6);
                Texture2D texture2D = new Texture2D(num7, num8);
                for (int k = 0; k < num8; k++)
                {
                    for (int l = 0; l < num7; l++)
                    {
                        if (l < position_dic || l >= position_dic + num5 || k < position_dic || k >= position_dic + num6)
                        {
                            texture2D.SetPixel(l, k, Color.clear);
                        }
                    }
                }

                texture2D.SetPixels(position_dic, position_dic, num5, num6, pixels);
                texture2D.Apply();
                return texture2D;
            }
            finally
            {
                if (consumerPolicy != null)
                {
                    ((IDisposable)consumerPolicy).Dispose();
                }
            }
        }

        internal static Texture2D LoginList(Color res)
        {
            if (connectionProcessor == null)
            {
                connectionProcessor = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
                {filterMode = FilterMode.Point, anisoLevel = 0};
            }

            connectionProcessor.SetPixel(0, 0, res);
            connectionProcessor.Apply();
            return connectionProcessor;
        }

        internal static Texture2D ReflectList(Color instance)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
            texture2D.filterMode = FilterMode.Point;
            texture2D.anisoLevel = 0;
            texture2D.SetPixel(0, 0, instance);
            texture2D.Apply();
            return texture2D;
        }

        internal static EditorResourceCache NewList(string value, string vis, string template = "")
        {
            EditorResourceCache errorPolicy = new EditorResourceCache(vis, template);
            if (errorPolicy.CompareHelper() == null)
            {
                GUIContent gUIContent = EditorGUIUtility.IconContent(value);
                if (gUIContent != null && gUIContent.image != null)
                {
                    errorPolicy.SetHelper(CloneList(gUIContent.image as Texture2D));
                }
            }

            return errorPolicy;
        }

        internal static Color PushList(float i)
        {
            while (i > 1f)
            {
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        case 3:
                            goto end_IL_001d;
                        case 1:
                            i /= 255f;
                            num = 0;
                            continue;
                        case 2:
                            goto case 1;
                    }

                    goto end_IL_0068;
                    continue;
                    end_IL_001d:
                        break;
                }

                continue;
                end_IL_0068:
                    break;
            }

            return new Color(i, i, i, 1f);
        }

        static ControllerEditorUtility()
        {
            itemProperty = "Assets";
            m_SpecificationProperty = new Dictionary<Type, string>{{typeof(UnityEditor.Animations.AnimatorController), "controller"}, {typeof(AnimationClip), "anim"}, {typeof(UnityEditor.Animations.BlendTree), "blendTree"}, {typeof(Shader), "shader"}, {typeof(Material), "mat"}, {typeof(GameObject), "prefab"}};
            _MethodProperty = new Queue<Action>();
            _SchemaProperty = new Queue<Action>();
            m_BroadcasterProperty = Application.unityVersion;
            m_ProxyProperty = m_BroadcasterProperty.Contains("2022");
            stateProperty = new Stack<(Rect, MouseCursor)>();
            iteratorProperty = "RadiusHandleHash".GetHashCode();
            publisherProperty = new Color(0.5f, 0.8f, 1f);
            configurationProperty = new Color(0.56f, 0.94f, 0.47f);
            _ProcProperty = new Color(1f, 0.25f, 0.25f);
            _WrapperProcessor = new Color(0.99f, 0.95f, 0f);
            m_AnnotationProcessor = new Color(0.7f, 0.3f, 1f);
            visitorProcessor = new Color(1f, 0.65f, 0f);
            m_AlgoProcessor = new Color(1f, 0.5f, 0.7f);
            m_MapperProcessor = new Color(0.3f, 0.7f, 1f);
            _PropertyProcessor = Shader.PropertyToID("_Color");
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            m_AdapterProcessor = new string[28]{"IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY", "VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList", "AvatarVersion", "ScaleModified", "ScaleFactor", "ScaleFactorInverse", "EyeHeightAsMeters", "EyeHeightAsPercent"};
#else
            m_AdapterProcessor = Array.Empty<string>();
#endif
            interpreterProcessor = new string[23]{"Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL", "FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR", "FingerMiddleR", "FingerRingR", "FingerLittleR"};
        }

        [CompilerGenerated]
        internal static T InstantiateError<T>(T def, ref _003C_003Ec__DisplayClass128_0 cfg)
            where T : UnityEngine.Object
        {
            if (!def)
            {
                return null;
            }

            if (!cfg.iteratorObserver.TryGetValue(def, out var value))
            {
                T val = CompareRules(def);
                AssetDatabase.AddObjectToAsset(val, cfg.m_PublisherObserver);
                val.hideFlags = def.hideFlags;
                cfg.iteratorObserver.Add(def, val);
                cfg._ProcObserver.Add(val, def);
                return val;
            }

            return (T)value;
        }

        [CompilerGenerated]
        internal static T AwakeError<T>(T setup, ref _003C_003Ec__DisplayClass128_0 counter)
            where T : Motion
        {
            if (!setup)
                return setup;
            string motionPath = AssetDatabase.GetAssetPath(setup);
            string controllerPath = AssetDatabase.GetAssetPath(counter.m_WrapperServer.stateMachine);
            if (!string.IsNullOrEmpty(motionPath) && (motionPath != controllerPath || AssetDatabase.LoadMainAssetAtPath(motionPath) == setup))
                return setup;
            if (counter.iteratorObserver.TryGetValue(setup, out var existing))
                return existing as T;
            T copy = InstantiateError(setup, ref counter);
            if (copy is UnityEditor.Animations.BlendTree tree)
            {
                var children = tree.children;
                for (int index = 0; index < children.Length; index++)
                    children[index].motion = AwakeError(children[index].motion, ref counter);
                tree.children = children;
            }

            EditorUtility.SetDirty(copy);
            return copy;
        }

        [CompilerGenerated]
        internal static T[] ResetError<T>(T[] value, ref _003C_003Ec__DisplayClass128_0 reg)
            where T : UnityEngine.Object
        {
            T[] array = new T[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                array[i] = InstantiateError(value[i], ref reg);
            }

            return array;
        }

        [CompilerGenerated]
        internal static T[] FlushError<T>(T[] v, ref _003C_003Ec__DisplayClass128_0 cont)
            where T : AnimatorTransitionBase
        {
            T[] array = ResetError(v, ref cont);
            T[] array2 = array;
            foreach (T val in array2)
            {
                val.destinationState = InstantiateError(val.destinationState, ref cont);
                val.destinationStateMachine = InstantiateError(val.destinationStateMachine, ref cont);
            }

            return array;
        }

        [CompilerGenerated]
        internal static AnimatorStateMachine ConnectError(AnimatorStateMachine key, ref _003C_003Ec__DisplayClass128_0 connection)
        {
            AnimatorStateMachine animatorStateMachine = InstantiateError(key, ref connection);
            if (!animatorStateMachine)
            {
                return null;
            }

            if (animatorStateMachine.name == connection.m_WrapperServer.name)
            {
                animatorStateMachine.name = connection.annotationServer.name;
            }

            animatorStateMachine.behaviours = ResetError(key.behaviours, ref connection);
            ChildAnimatorStateMachine[] stateMachines = key.stateMachines;
            for (int i = 0; i < stateMachines.Length; i++)
            {
                stateMachines[i].stateMachine = ConnectError(key.stateMachines[i].stateMachine, ref connection);
            }

            animatorStateMachine.stateMachines = stateMachines;
            return animatorStateMachine;
        }

        [CompilerGenerated]
        internal static void CalculateError(AnimatorStateMachine ident, ref _003C_003Ec__DisplayClass128_0 token)
        {
            AnimatorStateMachine animatorStateMachine = (AnimatorStateMachine)token._ProcObserver[ident];
            ChildAnimatorState[] states = animatorStateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState animatorState = InstantiateError(animatorStateMachine.states[i].state, ref token);
                if ((bool)animatorState)
                {
                    states[i].state = animatorState;
                    animatorState.behaviours = ResetError(animatorState.behaviours, ref token);
                    if (animatorState.motion && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(animatorState.motion)) != animatorState.motion)
                    {
                        animatorState.motion = AwakeError(animatorState.motion, ref token);
                    }
                }
            }

            ident.states = states;
            ChildAnimatorStateMachine[] stateMachines = ident.stateMachines;
            foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
            {
                CalculateError(childAnimatorStateMachine.stateMachine, ref token);
            }
        }

        [CompilerGenerated]
        internal static void TestError(AnimatorStateMachine item, ref _003C_003Ec__DisplayClass128_0 reg)
        {
            using (var serializedMachine = new UnityEditor.SerializedObject(item))
            {
                serializedMachine.FindProperty("m_StateMachineTransitions").ClearArray();
                serializedMachine.ApplyModifiedPropertiesWithoutUndo();
            }

            item.entryTransitions = FlushError(((AnimatorStateMachine)reg._ProcObserver[item]).entryTransitions, ref reg);
            item.anyStateTransitions = FlushError(((AnimatorStateMachine)reg._ProcObserver[item]).anyStateTransitions, ref reg);
            ChildAnimatorState[] states = item.states;
            for (int i = 0; i < states.Length; i++)
            {
                ChildAnimatorState childAnimatorState = states[i];
                childAnimatorState.state.transitions = FlushError(((AnimatorState)reg._ProcObserver[childAnimatorState.state]).transitions, ref reg);
            }

            ChildAnimatorStateMachine[] stateMachines = item.stateMachines;
            for (int i = 0; i < stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
                var originalMachine = (AnimatorStateMachine)reg._ProcObserver[item];
                var originalChild = (AnimatorStateMachine)reg._ProcObserver[childAnimatorStateMachine.stateMachine];
                item.SetStateMachineTransitions(childAnimatorStateMachine.stateMachine, FlushError(originalMachine.GetStateMachineTransitions(originalChild), ref reg));
            }

            stateMachines = item.stateMachines;
            foreach (ChildAnimatorStateMachine childAnimatorStateMachine2 in stateMachines)
            {
                TestError(childAnimatorStateMachine2.stateMachine, ref reg);
            }

            item.defaultState = InstantiateError(((AnimatorStateMachine)reg._ProcObserver[item]).defaultState, ref reg);
        }

        [CompilerGenerated]
        internal static bool MapError(UnityEngine.Object v)
        {
            while (true)
            {
                if (PrefabUtility.GetPrefabAssetType(v) != PrefabAssetType.NotAPrefab)
                {
                    int num = 1;
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                PrefabUtility.RecordPrefabInstancePropertyModifications(v);
                                num = 4;
                                continue;
                            case 2:
                                break;
                            case 3:
                                goto end_IL_0083;
                            case 4:
                                return true;
                            case 5:
                                goto IL_00c6;
                        }

                        break;
                    }

                    continue;
                }

                goto IL_00c6;
                IL_00c6:
                    ;
                break;
                continue;
                end_IL_0083:
                    break;
            }

            return false;
        }
    }
}
