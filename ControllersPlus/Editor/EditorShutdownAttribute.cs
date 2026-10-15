using System;

namespace FynnsTools.ControllerEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class EditorShutdownAttribute : EditorExtensionAttribute
    {
        public EditorShutdownAttribute(int no__task = 0)
        {
            int num = 1;
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 1:
                        _IssuerServer = no__task;
                        num = 0;
                        break;
                    case 0:
                        return;
                }
            }
        }

        public EditorShutdownAttribute(object[] info, int next_col)
        {
            int num = 2;
            while (true)
            {
                switch (num)
                {
                    default:
                        _IssuerServer = next_col;
                        num = 1;
                        break;
                    case 2:
                        m_IndexerServer = info;
                        num = 0;
                        break;
                    case 1:
                        return;
                }
            }
        }
    }
}
