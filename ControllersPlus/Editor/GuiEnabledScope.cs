using System;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class GuiEnabledScope : IDisposable
    {
        private readonly bool previous;
        public GuiEnabledScope(bool iskey)
        {
            previous = GUI.enabled;
            GUI.enabled = previous && !iskey;
        }

        public void Dispose() => GUI.enabled = previous;
    }
}
