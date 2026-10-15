#if VRC_SDK_VRCSDK3 && !UDON
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace FynnsTools.VRCSDKPlus
{
    internal static class ParameterImportUtility
    {
        internal static int Import(VRCExpressionParameters target, IEnumerable<ImportableParameter> candidates)
        {
            if (!target || candidates == null || !AssetDatabase.IsOpenForEdit(target)) return 0;
            var entries = new List<VRCExpressionParameters.Parameter>(target.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>());
            var names = new HashSet<string>(entries.Where(p => p != null).Select(p => p.name), StringComparer.Ordinal);
            int count = 0;
            foreach (var candidate in candidates.Where(p => p.selected))
            {
                if (string.IsNullOrWhiteSpace(candidate.name) || !names.Add(candidate.name)) continue;
                entries.Add(new VRCExpressionParameters.Parameter { name = candidate.name, valueType = candidate.valueType,
                    defaultValue = candidate.defaultValue, saved = candidate.saved, networkSynced = candidate.networkSynced });
                count++;
            }
            if (count == 0) return 0;
            Undo.RecordObject(target, "Import Parameters");
            target.parameters = entries.ToArray(); target.isEmpty = false;
            EditorUtility.SetDirty(target);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            return count;
        }
        internal static List<ImportableParameter> FromAnimator(AnimatorController controller, HashSet<string> existingNames)
        {
            var result = new List<ImportableParameter>();
            if (!controller) return result;
            var seen = new HashSet<string>(existingNames, System.StringComparer.Ordinal);
            foreach (var parameter in controller.parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.name) || seen.Contains(parameter.name)) continue;
                VRCExpressionParameters.ValueType type;
                float value;
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        type = VRCExpressionParameters.ValueType.Float; value = parameter.defaultFloat; break;
                    case AnimatorControllerParameterType.Int:
                        type = VRCExpressionParameters.ValueType.Int; value = parameter.defaultInt; break;
                    case AnimatorControllerParameterType.Bool:
                        type = VRCExpressionParameters.ValueType.Bool; value = parameter.defaultBool ? 1 : 0; break;
                    default: continue; 
                }
                seen.Add(parameter.name);
                result.Add(new ImportableParameter { name = parameter.name, valueType = type, defaultValue = value, saved = true, networkSynced = true, selected = true });
            }
            return result;
        }
    }
}
#endif
