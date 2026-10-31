using System;
using UnityEngine;
using nadena.dev.ndmf;

namespace FynnsTools.MultiUploader
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Fynn's Tools/MultiUploader/Quest conversion settings")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge", "Fynn.QuestBridge", "QuestBridgeSettings")]
    public sealed class MultiUploaderSettings : MonoBehaviour, INDMFEditorOnly
    {
        [Tooltip("Only applies during Android avatar builds. PC assets are never edited.")]
        public bool convertOnAndroid = true;
        [Range(256, 2048)] public int maxTextureSize = 1024;
        public bool bakeStaticPoiyomiDecals = true;
        public bool removeQuestAudio = true;
        public bool approximateTransparencyAsOpaque;
        [Tooltip("Require a manual mobile material for transparent, cutout, alternate-UV or unknown shaders.")]
        public bool blockUnsafeConversions = true;
        public MaterialOverride[] overrides = Array.Empty<MaterialOverride>();
        [Tooltip("Keep these objects and their children out of Quest material conversion.")]
        public GameObject[] ignoredObjects = Array.Empty<GameObject>();
    }

    [Serializable]
    public sealed class MaterialOverride
    {
        public Material pcMaterial;
        [Tooltip("Use VRChat/Mobile/Toon Standard. This material will also be copied.")]
        public Material questMaterial;
    }
}
