using System;
using UnityEngine;

namespace FynnsTools.MultiUploader.Editor
{
    public static class QuestAudioCleanup
    {
        static bool IsSpatialAudio(Type type)
        {
            for (; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
                if (type.Name == "VRCSpatialAudioSource" || type.Name == "VRC_SpatialAudioSource") return true;
            return false;
        }

        public static void Remove(GameObject buildCopy)
        {
            foreach (var component in buildCopy.GetComponentsInChildren<Component>(true))
                if (component != null && IsSpatialAudio(component.GetType()))
                    UnityEngine.Object.DestroyImmediate(component);
            foreach (var audio in buildCopy.GetComponentsInChildren<AudioSource>(true))
                UnityEngine.Object.DestroyImmediate(audio);
        }
    }
}
