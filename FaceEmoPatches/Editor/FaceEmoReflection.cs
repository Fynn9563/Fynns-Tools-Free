#if VRC_SDK_VRCSDK3
using System;
using System.Reflection;

namespace FynnsTools.FaceEmoPatches
{
	// Every patch finds the FaceEmo members it rewrites by name at runtime instead of
	// referencing FaceEmo's assemblies. FaceEmo keeps the interesting types internal or
	// sealed, so a reference would not help anyway, and going through names means this
	// tool still compiles and installs in a project that has no FaceEmo at all.
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
