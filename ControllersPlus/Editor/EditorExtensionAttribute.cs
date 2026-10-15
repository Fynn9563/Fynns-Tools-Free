using System;

namespace FynnsTools.ControllerEditor
{
    internal abstract class EditorExtensionAttribute : Attribute
    {
        internal object[] m_IndexerServer;
        internal int _IssuerServer;
    }
}
