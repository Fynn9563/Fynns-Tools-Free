using System;
using UnityEditor;

namespace FynnsTools.ControllerEditor
{
    internal sealed class MixedValueScope : IDisposable
    {
        private readonly bool previous;
        public MixedValueScope(bool mixed)
        {
            previous = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed;
        }

        public MixedValueScope(SerializedProperty property) : this(property != null && property.hasMultipleDifferentValues)
        {
        }

        public void Dispose() => EditorGUI.showMixedValue = previous;
    }
}
