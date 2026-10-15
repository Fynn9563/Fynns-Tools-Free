using System.Runtime.CompilerServices;
using UnityEditor.Animations;

namespace FynnsTools.ControllerEditor
{
    internal readonly struct TransitionReference
    {
        internal enum TransitionSourceType
        {
            StateTransition,
            MachineTransition,
            EntryTransition,
            AnyTransition
        }

        internal readonly AnimatorTransitionBase fieldServer;
        internal readonly AnimatorStateTransition m_AttributeServer;
        internal readonly TransitionSourceType m_ClientServer;
        internal readonly AnimatorState m_ConfigServer;
        internal readonly AnimatorStateMachine m_DescriptorServer;
        internal readonly AnimatorStateMachine _TemplateServer;
        [SpecialName]
        internal AnimatorCondition[] SetContext()
        {
            return fieldServer.conditions;
        }

        [SpecialName]
        internal void PostContext(AnimatorCondition[] item)
        {
            fieldServer.conditions = item;
        }

        internal TransitionReference(AnimatorTransitionBase param, TransitionSourceType col, AnimatorState util) : this()
        {
            fieldServer = param;
            m_AttributeServer = param as AnimatorStateTransition;
            m_ClientServer = col;
            m_ConfigServer = util;
            m_DescriptorServer = null;
            _TemplateServer = null;
        }

        internal TransitionReference(AnimatorTransitionBase init, TransitionSourceType cfg, AnimatorStateMachine third) : this()
        {
            fieldServer = init;
            m_AttributeServer = init as AnimatorStateTransition;
            m_ClientServer = cfg;
            m_ConfigServer = null;
            m_DescriptorServer = third;
            _TemplateServer = null;
        }

        internal TransitionReference(AnimatorTransitionBase item, TransitionSourceType pol, AnimatorStateMachine dir, AnimatorStateMachine visitor2) : this()
        {
            fieldServer = item;
            m_AttributeServer = item as AnimatorStateTransition;
            m_ClientServer = pol;
            m_ConfigServer = null;
            m_DescriptorServer = dir;
            _TemplateServer = visitor2;
        }

        public static implicit operator AnimatorStateTransition(TransitionReference def)
        {
            return def.m_AttributeServer;
        }

        public static implicit operator AnimatorTransition(TransitionReference init)
        {
            return (AnimatorTransition)init.fieldServer;
        }

        public static implicit operator AnimatorTransitionBase(TransitionReference v)
        {
            return v.fieldServer;
        }
    }
}
