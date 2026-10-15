using System;

namespace FynnsTools.ControllerEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class EditorInitializeAttribute : EditorExtensionAttribute
    {
        public EditorInitializeAttribute(int key_Ptr = 0)
        {
            _IssuerServer = key_Ptr;
        }

        public EditorInitializeAttribute(object[] setup, int flags_token)
        {
            m_IndexerServer = setup;
            _IssuerServer = flags_token;
        }
    }
}
