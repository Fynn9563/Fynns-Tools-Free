using UnityEditor;

namespace FynnsTools.MultiUploader.Editor
{
    public static class PlatformReadiness
    {
        public static BuildTarget CompiledTarget
        {
            get
            {
#if UNITY_ANDROID
                return BuildTarget.Android;
#elif UNITY_STANDALONE_WIN
                return BuildTarget.StandaloneWindows64;
#else
                return BuildTarget.NoTarget;
#endif
            }
        }

        public static bool IsReady(BuildTarget target) => EditorUserBuildSettings.activeBuildTarget == target
            && EditorUserBuildSettings.selectedBuildTargetGroup == BuildPipeline.GetBuildTargetGroup(target)
            && (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.Android ? CompiledTarget == target : true);

        public static string Describe() => "active=" + EditorUserBuildSettings.activeBuildTarget
            + ", selected group=" + EditorUserBuildSettings.selectedBuildTargetGroup + ", compiled=" + CompiledTarget;
    }
}
