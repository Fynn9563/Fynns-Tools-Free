using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static partial class ControllerEditorUtility
    {
        private static void FinalizeLayerCopy(ref _003C_003Ec__DisplayClass128_0 context)
        {
            var copies = context.iteratorObserver.Values.Distinct().Where(copy => copy).ToArray();
            foreach (var copy in copies)
            {
                using (var serialized = new SerializedObject(copy))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference || !property.objectReferenceValue)
                            continue;
                        if (context.iteratorObserver.TryGetValue(property.objectReferenceValue, out var replacement))
                            property.objectReferenceValue = replacement;
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUtility.SetDirty(copy);
            }

            if (context.configurationObserver)
                foreach (var copy in copies)
                    Undo.RegisterCreatedObjectUndo(copy, "Copy Layer");
        }
    }
}
