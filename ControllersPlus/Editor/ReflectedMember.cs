using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FynnsTools.ControllerEditor
{
    internal class ReflectedMember<T>
        where T : MemberInfo
    {
        public readonly string m_RegPolicy;
        public readonly ReflectedType _TestsPolicy;
        public readonly BindingFlags propertyPolicy;
        public readonly Type[] processorPolicy;
        private readonly bool observerPolicy;
        public bool _ServerPolicy;
        private T[] _ThreadPolicy;
        private bool m_PolicyPolicy;
        private T m_SerializerPolicy;
        [SpecialName]
        public Type WriteRecord()
        {
            return processorPolicy[0];
        }

        [SpecialName]
        public T[] StopRecord()
        {
            if (!_ServerPolicy)
            {
                _ServerPolicy = true;
                Type typeFromHandle = typeof(T);
                MemberTypes type = ((typeFromHandle == typeof(FieldInfo)) ? MemberTypes.Field : ((typeFromHandle == typeof(PropertyInfo)) ? MemberTypes.Property : MemberTypes.Method));
                _ThreadPolicy = (T[])_TestsPolicy.ChangeRecord().GetMember(m_RegPolicy, type, propertyPolicy);
            }

            return _ThreadPolicy;
        }

        [SpecialName]
        public T PrepareRecord()
        {
            if (m_PolicyPolicy)
            {
                return m_SerializerPolicy;
            }

            m_PolicyPolicy = true;
            if (StopRecord().Length == 0)
            {
                return null;
            }

            if (StopRecord().Length == 1 || WriteRecord() == null)
            {
                return m_SerializerPolicy = StopRecord()[0];
            }

            IEnumerator<MethodInfo> enumerator = StopRecord().Cast<MethodInfo>().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    MethodInfo current = enumerator.Current;
                    ParameterInfo[] parameters = current.GetParameters();
                    if ((!observerPolicy && parameters.Any((ParameterInfo asset) => asset.ParameterType == WriteRecord())) || (observerPolicy && parameters.Select((ParameterInfo p) => p.ParameterType).SequenceEqual(processorPolicy)))
                    {
                        m_SerializerPolicy = (T)(MemberInfo)current;
                        break;
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

            return m_SerializerPolicy;
        }

        public static implicit operator T(ReflectedMember<T> first)
        {
            return first.PrepareRecord();
        }

        public ReflectedMember(ReflectedType res, string cust, Type state = null, BindingFlags t2 = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static) : this(res, cust, new Type[1]{state}, t2, iskey3: false)
        {
        }

        public ReflectedMember(Type reference, string b, Type field = null, BindingFlags token2 = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static) : this(new ReflectedType(reference), b, new Type[1]{field}, token2, iskey3: false)
        {
        }

        public ReflectedMember(ReflectedType config, string token, Type[] state, BindingFlags cont2) : this(config, token, state, cont2, iskey3: true)
        {
        }

        public ReflectedMember(Type v, string vis, Type[] dic, BindingFlags item2) : this(new ReflectedType(v), vis, dic, item2, iskey3: true)
        {
        }

        public ReflectedMember(ReflectedType setup, string reg, Type[] rule, BindingFlags spec2, bool iskey3)
        {
            int num = 4;
            while (true)
            {
                switch (num)
                {
                    case 2:
                        processorPolicy = rule;
                        num = 0;
                        break;
                    case 3:
                        return;
                    case 5:
                        propertyPolicy = spec2;
                        num = 2;
                        break;
                    default:
                        observerPolicy = iskey3;
                        num = 3;
                        break;
                    case 4:
                        m_RegPolicy = reg;
                        num = 1;
                        break;
                    case 1:
                        _TestsPolicy = setup;
                        num = 5;
                        break;
                }
            }
        }
    }
}
