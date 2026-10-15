using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class EditorResourceCache
    {
        private GUIContent _SetterPolicy;
        private Texture2D _ConnectionPolicy;
        private readonly string contextPolicy;
        private readonly string _RecordPolicy;
        private bool _HelperPolicy;
        [SpecialName]
        private GUIContent SearchRecord()
        {
            while (true)
            {
                int num;
                if (!_HelperPolicy)
                {
                    num = 2;
                    goto IL_001d;
                }

                goto IL_012f;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            case 3:
                                break;
                            case 5:
                                goto IL_0077;
                            case 1:
                            case 2:
                            case 6:
                                goto end_IL_0043;
                            default:
                                RegisterRecord();
                                num = 1;
                                continue;
                            case 4:
                                goto IL_012f;
                        }

                        break;
                    }

                continue;
                IL_012f:
                    ;
                goto IL_0077;
                IL_0077:
                    if (_SetterPolicy.image == null)
                    {
                        num = 0;
                        goto IL_001d;
                    }

                break;
                continue;
                end_IL_0043:
                    break;
            }

            return _SetterPolicy;
        }

        [SpecialName]
        private void RevertRecord(GUIContent item)
        {
            _SetterPolicy = item;
        }

        [SpecialName]
        internal Texture2D CompareHelper()
        {
            while (_HelperPolicy)
            {
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            break;
                        case 4:
                            if (!(_ConnectionPolicy == null))
                            {
                                num = 3;
                                continue;
                            }

                            RegisterRecord();
                            num = 0;
                            continue;
                        default:
                            goto end_IL_003f;
                        case 1:
                            goto case 4;
                    }

                    break;
                }

                continue;
                end_IL_003f:
                    break;
            }

            return _ConnectionPolicy;
        }

        [SpecialName]
        internal void SetHelper(Texture2D setup)
        {
            _ConnectionPolicy = setup;
            _HelperPolicy = _ConnectionPolicy != null;
            if (_HelperPolicy)
            {
                PrintRecord(setup.EncodeToPNG(), contextPolicy);
            }

            LogoutRecord();
        }

        public EditorResourceCache(string i, string reg = "")
        {
            int num = 0;
            while (true)
            {
                switch (num)
                {
                    default:
                        contextPolicy = i;
                        num = 3;
                        break;
                    case 3:
                        _RecordPolicy = reg;
                        num = 1;
                        break;
                    case 4:
                        LogoutRecord();
                        num = 2;
                        break;
                    case 2:
                        return;
                    case 1:
                        RegisterRecord();
                        num = 4;
                        break;
                }
            }
        }

        private void RegisterRecord()
        {
            SetHelper(ManageRecord(contextPolicy));
        }

        private void LogoutRecord()
        {
            RevertRecord(new GUIContent(CompareHelper(), _RecordPolicy));
        }

        private static byte[] PatchRecord(int[] key)
        {
            byte[] array = new byte[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                array[i] = (byte)key[i];
            }

            return array;
        }

        private static int[] InterruptRecord(byte[] value)
        {
            int num = value.Length;
            int[] array = new int[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = value[i];
            }

            return array;
        }

        internal static Texture2D ManageRecord(string task)
        {
            int[] intArray = SessionState.GetIntArray(task, null);
            if (intArray != null)
            {
                try
                {
                    byte[] data = PatchRecord(intArray);
                    Texture2D texture2D = new Texture2D(0, 0);
                    texture2D.LoadImage(data);
                    texture2D.Apply();
                    return texture2D;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    SessionState.EraseIntArray(task);
                }
            }

            return null;
        }

        internal static void PrintRecord(byte[] v, string b)
        {
            while (true)
            {
                int[] value = InterruptRecord(v);
                int num = 2;
                while (true)
                {
                    switch (num)
                    {
                        default:
                            return;
                        case 3:
                            break;
                        case 1:
                            return;
                        case 4:
                            goto IL_0062;
                        case 2:
                            goto IL_0062;
                    }

                    break;
                    IL_0062:
                        SessionState.SetIntArray(b, value);
                    num = 0;
                }
            }
        }

        public static implicit operator GUIContent(EditorResourceCache param)
        {
            return param.SearchRecord();
        }
    }
}
