using System;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class GuiColorScope : IDisposable
    {
        internal enum ColoringType
        {
            BG = 1,
            FG = 2,
            General = 4,
            All = 7
        }

        private readonly Color[] _MessageThread = new Color[3];
        private readonly ColoringType _CollectionThread;
        private bool m_ParserThread;
        private void CollectRecord()
        {
            m_ParserThread = true;
            _MessageThread[0] = GUI.backgroundColor;
            _MessageThread[1] = GUI.contentColor;
            _MessageThread[2] = GUI.color;
        }

        private void ResolveRecord(Color config)
        {
            CollectRecord();
            if (_CollectionThread.HasFlag(ColoringType.BG))
            {
                GUI.backgroundColor = config;
            }

            if (_CollectionThread.HasFlag(ColoringType.FG))
            {
                GUI.contentColor = config;
            }

            if (_CollectionThread.HasFlag(ColoringType.General))
            {
                GUI.color = config;
            }
        }

        internal GuiColorScope(ColoringType v, Color connection)
        {
            int num = 2;
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 0:
                        return;
                    case 2:
                        _CollectionThread = v;
                        num = 1;
                        break;
                    case 1:
                        ResolveRecord(connection);
                        num = 0;
                        break;
                }
            }
        }

        internal GuiColorScope(ColoringType setup, bool wantconnection, Color proc)
        {
            _CollectionThread = setup;
            if (wantconnection)
            {
                ResolveRecord(proc);
            }
        }

        internal GuiColorScope(ColoringType def, bool skipcont, Color proc, Color config2)
        {
            int num = 2;
            while (true)
            {
                switch (num)
                {
                    default:
                        return;
                    case 2:
                        _CollectionThread = def;
                        num = 1;
                        break;
                    case 0:
                        return;
                    case 1:
                        ResolveRecord(skipcont ? proc : config2);
                        num = 0;
                        break;
                }
            }
        }

        internal GuiColorScope(ColoringType res, int visZ, params Color[] colors)
        {
            _CollectionThread = res;
            if (visZ >= 0)
            {
                CollectRecord();
                ResolveRecord(colors[visZ]);
            }
        }

        public void Dispose()
        {
            if (!m_ParserThread)
            {
                return;
            }

            if (_CollectionThread.HasFlag(ColoringType.BG))
            {
                GUI.backgroundColor = _MessageThread[0];
            }

            if (_CollectionThread.HasFlag(ColoringType.FG))
            {
                GUI.contentColor = _MessageThread[1];
            }

            if (_CollectionThread.HasFlag(ColoringType.General))
            {
                GUI.color = _MessageThread[2];
            }
        }
    }
}
