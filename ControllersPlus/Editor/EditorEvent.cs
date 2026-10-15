using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal struct EditorEvent
    {
        internal readonly Event m_RegistryPolicy;
        internal bool m_TagPolicy;
        internal EditorEvent(Event init) : this()
        {
            m_RegistryPolicy = init;
            m_TagPolicy = true;
        }

        public static implicit operator EditorEvent(Event item)
        {
            return new EditorEvent(item);
        }

        public static implicit operator Event(EditorEvent info)
        {
            return info.m_RegistryPolicy;
        }

        public static implicit operator bool (EditorEvent i)
        {
            return i.m_TagPolicy;
        }
    }
}
