using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	internal static class ClientSimRuntimeCaches
	{
		private const string LogPrefix = "[ClientSim Storage Cleaner] ";
		private const BindingFlags StaticFields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		private static readonly string[] TextureFields =
		{
			"BlackTexture", "GrayTexture", "LinearGrayTexture", "NormalTexture", "RedTexture", "WhiteTexture"
		};

		// Reset after scene teardown and before ClientSim startup.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void BeforePlay()
		{
			if (!ClientSimStorageCleanerSettings.ResetRuntimeCaches) return;
			if (!EditorSettings.enterPlayModeOptionsEnabled ||
				(EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) == 0) return;
			Reset();
		}

		public static void Reset()
		{
			int playerLists = 0;
			int textures = 0;
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					Type networking = assembly.GetType("VRC.SDK3.ClientSim.ClientSimNetworkingUtilities", false);
					if (networking != null)
					{
						FieldInfo list = networking.GetField("_playerObjectList", StaticFields);
						if (list == null || list.IsInitOnly || !list.FieldType.IsArray ||
							list.FieldType.GetElementType().FullName != "VRC.SDK3.Components.VRCPlayerObject")
						{
							Debug.LogWarning(LogPrefix + "ClientSim player cache layout has changed; cache reset was skipped. Check SDK compatibility.");
						}
						else
						{
							list.SetValue(null, null);
							playerLists++;
						}
					}

					Type holder = assembly.GetType("VRC.Udon.Security.Texture2DDefaultTextureHolder", false);
					if (holder == null) continue;
					foreach (string name in TextureFields)
					{
						FieldInfo field = holder.GetField(name, StaticFields);
						if (field == null || field.IsInitOnly || field.FieldType != typeof(Texture2D))
						{
							Debug.LogWarning(LogPrefix + "Udon default texture cache layout has changed at " + name + "; reset was skipped for this field.");
							continue;
						}
						var texture = (Texture2D)field.GetValue(null);
						// Destroyed Unity objects still have non-null CLR references.
						if (!ReferenceEquals(texture, null) && texture == null)
						{
							field.SetValue(null, null);
							textures++;
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogWarning(LogPrefix + "Could not reset runtime caches in " + assembly.GetName().Name + ": " + exception);
				}
			}
			if (playerLists > 0 || textures > 0)
				Debug.Log(LogPrefix + "Reset " + playerLists + " ClientSim player-object cache(s) and " + textures + " destroyed Udon texture reference(s). Saved player data was kept.");
		}
	}
}
