#if VRC_SDK_VRCSDK3
using System;
using System.Reflection;

namespace FynnsTools.FaceEmoPatches
{
	// Resolve internal FaceEmo members without an assembly dependency.
	internal static class FaceEmoReflection
	{
		private const string AssemblyPrefix = "jp.suzuryg.face-emo";

		public static bool IsInstalled
		{
			get
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetName().Name.StartsWith(AssemblyPrefix, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}

				return false;
			}
		}
	}
}
#endif
