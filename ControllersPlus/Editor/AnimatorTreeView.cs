using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal class AnimatorTreeView
    {
        [Serializable]
        [CompilerGenerated]
        internal sealed class _003C_003Ec
        {
            public static readonly _003C_003Ec m_WatcherProperty = new _003C_003Ec();
            public static Func<EditorCurveBinding, string> candidateProperty;
            public static Func<string, string> productProperty;
            internal static _003C_003Ec PushStruct;
            internal string PublishPage(EditorCurveBinding b)
            {
                return b.propertyName;
            }

            internal string PopPage(string s)
            {
                return s;
            }
        }

        [CompilerGenerated]
        internal sealed class _003C_003Ec__DisplayClass27_0
        {
            public AnimatorTreeView expressionProperty;
            public string m_SystemProperty;
            internal void ComputePage()
            {
                if (expressionProperty.RevertSerializer() == -1)
                {
                    expressionProperty.m_HelperProperty = new string[1]{"m_IsActive"};
                }
                else
                {
                    expressionProperty.m_HelperProperty = (
                        from b in AnimationUtility.GetAnimatableBindings(expressionProperty.ManageSerializer(), expressionProperty.ManageSerializer())
                        where b.type == expressionProperty._RecordProperty
                        select b).Select(_003C_003Ec.m_WatcherProperty.PublishPage).OrderBy(_003C_003Ec.m_WatcherProperty.PopPage).ToArray();
                }

                if (expressionProperty._ConsumerProperty >= expressionProperty.m_HelperProperty.Length)
                {
                    expressionProperty._ConsumerProperty = Mathf.Max(0, expressionProperty.m_HelperProperty.Length - 1);
                }
            }

            internal bool ConcatPage()
            {
                int num = expressionProperty.m_HelperProperty.FindResolver((string s) => s == m_SystemProperty);
                if (num < 0)
                {
                    return false;
                }

                expressionProperty._ConsumerProperty = num;
                return true;
            }
        }

        private GameObject errorProperty;
        public Component[] m_SetterProperty;
        public int _ConnectionProperty = -1;
        public UnityEngine.Object contextProperty;
        public Type _RecordProperty;
        public string[] m_HelperProperty;
        public int _ConsumerProperty;
        public float m_AdapterProperty = 1f;
        internal static readonly Type[] _InterpreterProperty = new Type[3]{typeof(GameObject), typeof(Behaviour), typeof(Renderer)};
        [SpecialName]
        public bool RegisterSerializer()
        {
            if ((bool)errorProperty)
            {
                if (_ConnectionProperty < m_SetterProperty.Length)
                {
                    return _ConsumerProperty < m_HelperProperty.Length;
                }
            }

            return false;
        }

        [SpecialName]
        public bool PatchSerializer()
        {
            return m_AdapterProperty > 0f;
        }

        [SpecialName]
        internal GameObject ManageSerializer()
        {
            return errorProperty;
        }

        [SpecialName]
        internal void PrintSerializer(GameObject def)
        {
            while (true)
            {
                IL_0072:
                    int num;
                if (!(errorProperty != def))
                {
                    num = 3;
                    goto IL_001d;
                }

                goto IL_00f0;
                IL_001d:
                    while (true)
                    {
                        switch (num)
                        {
                            default:
                                PrepareSerializer();
                                num = 5;
                                continue;
                            case 6:
                                break;
                            case 4:
                                goto IL_0072;
                            case 2:
                                return;
                            case 3:
                                return;
                            case 5:
                                return;
                            case 1:
                                goto IL_00f0;
                        }

                        break;
                    }

                goto IL_0050;
                IL_00f0:
                    ;
                goto IL_0050;
                IL_0050:
                    errorProperty = def;
                num = 0;
                goto IL_001d;
            }
        }

        [SpecialName]
        public int RevertSerializer()
        {
            return _ConnectionProperty;
        }

        [SpecialName]
        public void OrderPage(int param)
        {
            if (_ConnectionProperty != param)
            {
                _ConnectionProperty = param;
                UpdateSerializer();
                ChangeSerializer();
            }
        }

        [SpecialName]
        public string SetPage()
        {
            if (m_HelperProperty.Any())
            {
                if (_ConsumerProperty < m_HelperProperty.Length)
                {
                    return m_HelperProperty[_ConsumerProperty];
                }
            }

            return string.Empty;
        }

        public AnimatorTreeView()
        {
            int num = 1;
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 2:
                        m_SetterProperty = Array.Empty<Component>();
                        num = 0;
                        break;
                    case 1:
                        m_HelperProperty = Array.Empty<string>();
                        num = 2;
                        break;
                    case 0:
                        return;
                }
            }
        }

        public AnimatorTreeView(GameObject def)
        {
            int num = 0;
            while (true)
            {
                switch (num)
                {
                    default:
                        PrintSerializer(def);
                        num = 1;
                        break;
                    case 4:
                        ChangeSerializer();
                        num = 3;
                        break;
                    case 3:
                        return;
                    case 2:
                        AssetSerializer();
                        num = 4;
                        break;
                    case 1:
                        m_AdapterProperty = (def.activeSelf ? 1 : 0);
                        num = 2;
                        break;
                }
            }
        }

        public void StopSerializer(bool hasparam)
        {
            do
            {
                int param = RevertSerializer() + 1;
                OrderPage(param);
            }
            while (hasparam && !SetupPage());
        }

        public void CheckSerializer(bool isreference)
        {
            do
            {
                int param = RevertSerializer() - 1;
                OrderPage(param);
            }
            while (isreference && !SetupPage());
        }

        private void PrepareSerializer()
        {
            AssetSerializer();
            if (!errorProperty)
            {
                ChangeSerializer();
                return;
            }

            for (int i = 0; i < m_SetterProperty.Length; i++)
            {
                if (m_SetterProperty[i].GetType() == _RecordProperty)
                {
                    OrderPage(i);
                    ChangeSerializer();
                    return;
                }
            }

            _ConnectionProperty = -1;
            ChangeSerializer();
        }

        private void AssetSerializer()
        {
            m_SetterProperty = (ManageSerializer() ? ManageSerializer().GetComponents<Component>() : Array.Empty<Component>());
        }

        private void UpdateSerializer()
        {
            if (m_SetterProperty != null)
            {
                if (_ConnectionProperty < m_SetterProperty.Length)
                {
                    goto IL_010e;
                }
            }

            AssetSerializer();
            goto IL_010e;
            IL_010e:
                if (_ConnectionProperty < m_SetterProperty.Length)
                {
                    if (_ConnectionProperty < -1)
                    {
                        _ConnectionProperty = m_SetterProperty.Length - 1;
                    }
                }
                else
                {
                    _ConnectionProperty = -1;
                }
        }

        private void ChangeSerializer()
        {
            _003C_003Ec__DisplayClass27_0 _003C_003Ec__DisplayClass27_ = new _003C_003Ec__DisplayClass27_0();
            _003C_003Ec__DisplayClass27_.expressionProperty = this;
            contextProperty = ((!ManageSerializer()) ? null : ((RevertSerializer() == -1) ? ((UnityEngine.Object)ManageSerializer()) : ((UnityEngine.Object)m_SetterProperty[RevertSerializer()])));
            Type recordProperty = _RecordProperty;
            _RecordProperty = (contextProperty ? contextProperty.GetType() : null);
            if (!contextProperty || recordProperty == _RecordProperty)
            {
                return;
            }

            if (m_HelperProperty != null && _ConsumerProperty < m_HelperProperty.Length && RevertSerializer() != -1)
            {
                _003C_003Ec__DisplayClass27_.m_SystemProperty = m_HelperProperty[_ConsumerProperty];
                _003C_003Ec__DisplayClass27_.ComputePage();
                if (m_HelperProperty.Length != 0 && !_003C_003Ec__DisplayClass27_.ConcatPage())
                {
                    _003C_003Ec__DisplayClass27_.m_SystemProperty = ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_IsActive") ? "m_Enabled" : ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_Enabled") ? "m_IsActive" : string.Empty));
                    if (!_003C_003Ec__DisplayClass27_.ConcatPage())
                    {
                        _ConsumerProperty = 0;
                    }
                }
            }
            else
            {
                _003C_003Ec__DisplayClass27_.ComputePage();
            }
        }

        [SpecialName]
        private bool SetupPage()
        {
            return _InterpreterProperty.Any((Type i) => _RecordProperty.InstantiateResolver(i));
        }
    }
}
