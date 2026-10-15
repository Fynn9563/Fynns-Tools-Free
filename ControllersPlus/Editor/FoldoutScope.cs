using System;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal class FoldoutScope : IDisposable
    {
        private readonly bool _ConfigurationThread;
        private readonly IndentedLayoutScope m_ProcThread;
        public FoldoutScope(bool vopen) : this(ref vopen, iscfg: false, null)
        {
        }

        public FoldoutScope(ref bool def) : this(ref def, iscfg: false, null)
        {
        }

        public FoldoutScope(ref bool value, string col, GUIStyle c = null) : this(ref value, iscfg: true, new GUIContent(col), c)
        {
        }

        public FoldoutScope(ref bool info, GUIContent connection, GUIStyle filter = null) : this(ref info, iscfg: true, connection, filter)
        {
        }

        public FoldoutScope(ref bool spec, bool iscfg, GUIContent proc, GUIStyle info2 = null)
        {
            int num = 4;
            while (true)
            {
                switch (num)
                {
                    case 4:
                        if (!iscfg)
                        {
                            num = 2;
                            continue;
                        }

                        goto case 3;
                    case 3:
                        if (info2 != null)
                        {
                            num = 7;
                            continue;
                        }

                        goto case 6;
                    case 5:
                    case 7:
                        spec = EditorGUILayout.Foldout(spec, proc, info2);
                        num = 0;
                        continue;
                    case 6:
                        info2 = EditorStyles.foldout;
                        num = 5;
                        continue;
                    case 1:
                        m_ProcThread = new IndentedLayoutScope();
                        num = 8;
                        continue;
                    case 8:
                        return;
                }

                bool num2 = spec;
                bool flag = num2;
                _ConfigurationThread = num2;
                if (flag)
                {
                    num = 1;
                    continue;
                }

                return;
            }
        }

        public void Dispose()
        {
            while (true)
            {
                int num;
                if (!_ConfigurationThread)
                {
                    num = 3;
                    goto IL_001d;
                }

                goto IL_00d8;
                IL_001d:
                    switch (num)
                    {
                        case 3:
                            return;
                        case 4:
                            break;
                        case 5:
                            return;
                        case 2:
                            goto IL_0092;
                        case 1:
                            return;
                        default:
                            goto IL_00d8;
                    }

                continue;
                IL_00d8:
                    ;
                goto IL_0092;
                IL_0092:
                    m_ProcThread.Dispose();
                num = 1;
                goto IL_001d;
            }
        }

        public static implicit operator bool (FoldoutScope last)
        {
            return last._ConfigurationThread;
        }
    }
}
