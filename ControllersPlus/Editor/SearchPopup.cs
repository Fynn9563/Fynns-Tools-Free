using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal class SearchPopup<T> : PopupWindowContent
    {
        internal class UtilsServer
        {
            internal readonly int m_ValServer;
            internal readonly T _ValueServer;
            internal object[] m_MerchantServer;
            internal bool authenticationServer = true;
            [SpecialName]
            internal object CloneConnection()
            {
                return m_MerchantServer[0];
            }

            internal UtilsServer(T spec, int cust_size)
            {
                _ValueServer = spec;
                m_ValServer = cust_size;
            }
        }

        private readonly string _TokenServer;
        private string m_CodeServer;
        internal UtilsServer[] m_DicServer;
        private readonly Action<UtilsServer> invocationServer;
        private readonly Action<int, T> roleServer;
        private Func<T, string, bool> _ParamServer;
        private bool _ModelServer;
        private float m_TokenizerServer;
        private bool _DecoratorServer = true;
        private Vector2 m_ComparatorServer;
        private readonly Rect[] _ExceptionServer;
        internal readonly GUIStyle objectServer = new GUIStyle{hover = {background = ControllerEditorUtility.ReflectList(new Color(0.302f, 0.302f, 0.302f))}, active = {background = ControllerEditorUtility.ReflectList(new Color(0.1725f, 0.3647f, 0.5294f))}};
        public SearchPopup(string param, IEnumerable<T> attr, Action<UtilsServer> third, Action<int, T> reference2)
        {
            _TokenServer = param;
            roleServer = reference2;
            invocationServer = third;
            m_DicServer = attr.Select((T item, int i) => new UtilsServer(item, i)).ToArray();
            _ExceptionServer = new Rect[m_DicServer.Length];
        }

        public void GetConnection(Func<T, string, bool> key)
        {
            _ModelServer = true;
            _ParamServer = key;
        }

        public void IncludeConnection(Func<T, object[]> ident)
        {
            UtilsServer[] dicServer = m_DicServer;
            foreach (UtilsServer utilsServer in dicServer)
            {
                utilsServer.m_MerchantServer = ident(utilsServer._ValueServer);
            }
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.AreaScope areaScope = new GUILayout.AreaScope(rect);
            try
            {
                Event current = Event.current;
                ScrollViewScope specificationThread = new ScrollViewScope(ref m_ComparatorServer);
                try
                {
                    if (!string.IsNullOrEmpty(_TokenServer))
                    {
                        GUILayout.Label(_TokenServer, ControllerEditorUtility.CalcError()._StructProcessor);
                        ControllerEditorUtility.MapQueue();
                    }

                    if (_ModelServer)
                    {
                        EditorGUI.BeginChangeCheck();
                        if (_DecoratorServer)
                        {
                            GUI.SetNextControlName(_TokenServer + "SearchBar");
                        }

                        m_CodeServer = EditorGUILayout.TextField(m_CodeServer, GUI.skin.GetStyle("SearchTextField"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            UtilsServer[] dicServer = m_DicServer;
                            foreach (UtilsServer utilsServer in dicServer)
                            {
                                utilsServer.authenticationServer = _ParamServer(utilsServer._ValueServer, m_CodeServer);
                            }
                        }
                    }

                    EventType type = current.type;
                    for (int j = 0; j < m_DicServer.Length; j++)
                    {
                        UtilsServer utilsServer2 = m_DicServer[j];
                        if (!utilsServer2.authenticationServer)
                        {
                            continue;
                        }

                        if (!_DecoratorServer && GUI.Button(_ExceptionServer[j], string.Empty, objectServer))
                        {
                            roleServer(utilsServer2.m_ValServer, utilsServer2._ValueServer);
                            base.editorWindow.Close();
                        }

                        GUILayout.VerticalScope verticalScope = new GUILayout.VerticalScope();
                        try
                        {
                            invocationServer(utilsServer2);
                        }
                        finally
                        {
                            if (verticalScope != null)
                            {
                                ((IDisposable)verticalScope).Dispose();
                            }
                        }

                        if (type == EventType.Repaint)
                        {
                            _ExceptionServer[j] = GUILayoutUtility.GetLastRect();
                            if (_DecoratorServer && !(_ExceptionServer[j].width <= m_TokenizerServer))
                            {
                                m_TokenizerServer = _ExceptionServer[j].width;
                            }
                        }
                    }

                    if (type == EventType.Repaint && _DecoratorServer)
                    {
                        _DecoratorServer = false;
                        GUI.FocusControl(_TokenServer + "SearchBar");
                    }
                }
                finally
                {
                    if (specificationThread != null)
                    {
                        ((IDisposable)specificationThread).Dispose();
                    }
                }

                if (rect.Contains(current.mousePosition))
                {
                    base.editorWindow.Repaint();
                }
            }
            finally
            {
                if (areaScope != null)
                {
                    ((IDisposable)areaScope).Dispose();
                }
            }
        }

        public override Vector2 GetWindowSize()
        {
            while (true)
            {
                Vector2 windowSize = base.GetWindowSize();
                int num = 4;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            windowSize.x = m_TokenizerServer + 21f;
                            num = 0;
                            continue;
                        case 4:
                            if (!_DecoratorServer)
                            {
                                num = 3;
                                continue;
                            }

                            goto default;
                        default:
                            return windowSize;
                        case 5:
                            break;
                        case 3:
                            goto case 2;
                    }

                    break;
                }
            }
        }

        public void RunConnection(Rect item)
        {
            PopupWindow.Show(item, this);
        }
    }
}
