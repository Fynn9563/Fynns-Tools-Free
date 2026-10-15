using System;
using System.Runtime.CompilerServices;

namespace FynnsTools.ControllerEditor
{
    internal class ReflectedType
    {
        public readonly string resolverPolicy;
        public readonly bool predicatePolicy;
        public bool m_RulesPolicy;
        private Type queuePolicy;
        [SpecialName]
        public Type ChangeRecord()
        {
            while (!m_RulesPolicy)
            {
                int num = 1;
                while (true)
                {
                    switch (num)
                    {
                        case 2:
                            break;
                        default:
                            goto end_IL_0067;
                        case 5:
                            m_RulesPolicy = true;
                            num = 3;
                            continue;
                        case 3:
                            queuePolicy = (predicatePolicy ? ControllerEditorUtility.WriteRules(resolverPolicy) : ControllerEditorUtility.FillRules(resolverPolicy));
                            num = 0;
                            continue;
                        case 1:
                            goto case 5;
                    }

                    break;
                }

                continue;
                end_IL_0067:
                    break;
            }

            return queuePolicy;
        }

        public static implicit operator Type(ReflectedType task)
        {
            return task.ChangeRecord();
        }

        public ReflectedType(string spec, bool issecond = false)
        {
            int num = 2;
            while (true)
            {
                switch (num)
                {
                    case 2:
                        resolverPolicy = spec;
                        num = 0;
                        break;
                    case 1:
                        return;
                    default:
                        predicatePolicy = issecond;
                        num = 1;
                        break;
                }
            }
        }

        public ReflectedType(Type value)
        {
            int num = 0;
            while (true)
            {
                switch (num)
                {
                    case 2:
                        return;
                    default:
                        queuePolicy = value;
                        num = 3;
                        break;
                    case 3:
                        resolverPolicy = value.FullName;
                        num = 1;
                        break;
                    case 1:
                        m_RulesPolicy = true;
                        num = 2;
                        break;
                }
            }
        }
    }
}
