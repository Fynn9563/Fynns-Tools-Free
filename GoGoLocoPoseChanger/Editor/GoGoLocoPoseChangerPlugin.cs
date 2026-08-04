#if NDMF && VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using nadena.dev.ndmf.localization;
using nadena.dev.ndmf.vrchat;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.GoGoLocoPoseChanger
{
	// State class to persist enabled flag across build phases
	internal class GoGoLocoPoseChangerState
	{
		public bool Enabled;
		public bool Debug;
	}

	public class GoGoLocoPoseChangerPlugin : Plugin<GoGoLocoPoseChangerPlugin>
	{
		private enum ReplacementType
		{
			BlendTree,
			StateMachine
		}

		private static readonly Localizer Localizer = new Localizer("en", () => new List<LocalizationAsset>());

		private BuildPhase _phase;
		private bool _debug;

		public override string DisplayName => "GoGoLoco Pose Changer";
		public override string QualifiedName => "com.fynnstools.gogolocoposechanger";
		public static Version Version => Version.Parse("1.0.0");

		protected override void Configure()
		{
			_phase = BuildPhase.Resolving;
			InPhase(_phase).Run($"{DisplayName} {_phase}", (BuildContext ctx) =>
			{
				try
				{
					GoGoLocoPoseChangerComponent componentInChildren = ctx.AvatarRootObject.GetComponentInChildren<GoGoLocoPoseChangerComponent>();
					if (componentInChildren != null)
					{
						// Check for GoGoLoco in Assets or Packages
						bool gogoLocoFound = AssetDatabase.IsValidFolder("Assets/GoGo") ||
							AssetDatabase.IsValidFolder("Packages/gogoloco");

						if (!gogoLocoFound)
						{
							Debug.LogWarning("<color=#5ef02f>[GoGoLoco Pose Changer]</color> GoGoLoco was not found. Settings were not applied.");
						}
						else if (new object[] { componentInChildren.avatarThumbnail, componentInChildren.stand, componentInChildren.crouch, componentInChildren.prone, componentInChildren.fall, componentInChildren.afk, componentInChildren.afkInit, componentInChildren.afkLoop, componentInChildren.afkStop }.All(p => p == null))
						{
							Debug.LogWarning("<color=#5ef02f>[GoGoLoco Pose Changer]</color> No animations configured. Settings were not applied.");
						}
						else if (componentInChildren.isExtended && (componentInChildren.afkInit == null || componentInChildren.afkLoop == null || componentInChildren.afkStop == null))
						{
							Debug.LogWarning("<color=#5ef02f>[GoGoLoco Pose Changer]</color> Extended AFK mode enabled but required motions are missing. Settings were not applied.");
						}
						else
						{
							ctx.GetState<GoGoLocoPoseChangerState>().Enabled = true;
						}
					}
					if (ctx.GetState<GoGoLocoPoseChangerState>().Enabled)
					{
						ctx.GetState<GoGoLocoPoseChangerState>().Debug = componentInChildren.debug;
						ctx.GetState<GoGoLocoPoseChangerComponent>().avatarThumbnail = componentInChildren.avatarThumbnail;
						ctx.GetState<GoGoLocoPoseChangerComponent>().stand = componentInChildren.stand;
						ctx.GetState<GoGoLocoPoseChangerComponent>().crouch = componentInChildren.crouch;
						ctx.GetState<GoGoLocoPoseChangerComponent>().prone = componentInChildren.prone;
						ctx.GetState<GoGoLocoPoseChangerComponent>().fall = componentInChildren.fall;
						ctx.GetState<GoGoLocoPoseChangerComponent>().afk = componentInChildren.afk;
						ctx.GetState<GoGoLocoPoseChangerComponent>().afkInit = componentInChildren.afkInit;
						ctx.GetState<GoGoLocoPoseChangerComponent>().afkLoop = componentInChildren.afkLoop;
						ctx.GetState<GoGoLocoPoseChangerComponent>().afkStop = componentInChildren.afkStop;
						ctx.GetState<GoGoLocoPoseChangerComponent>().isExtended = componentInChildren.isExtended;
					}
					GoGoLocoPoseChangerComponent[] componentsInChildren = ctx.AvatarRootObject.GetComponentsInChildren<GoGoLocoPoseChangerComponent>();
					foreach (GoGoLocoPoseChangerComponent obj in componentsInChildren)
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
				}
				catch (Exception e)
				{
					HandleException(e);
				}
			});

			_phase = BuildPhase.Optimizing;
			InPhase(_phase).AfterPlugin("nadena.dev.modular-avatar").AfterPlugin("com.anatawa12.avatar-optimizer").AfterPlugin("com.vrcfury.vrcfury")
				.Run($"{DisplayName} {_phase}", (BuildContext ctx) =>
				{
					try
					{
						var pluginState = ctx.GetState<GoGoLocoPoseChangerState>();
						if (pluginState.Enabled)
						{
							_debug = pluginState.Debug;
							GoGoLocoPoseChangerComponent state = ctx.GetState<GoGoLocoPoseChangerComponent>();
							Dictionary<string, Motion> replacements = new Dictionary<string, Motion>
							{
								{ "go_VR_Stand_Idle", state.stand },
								{ "go_VR_Stand_Idle_Mirror", state.stand },
								{ "go_VR_Crouch_Idle", state.crouch },
								{ "go_VR_Crouch_Idle_Mirror", state.crouch },
								{ "go_VR_Prone_Idle", state.prone },
								{ "go_VR_Prone_Idle_Mirror", state.prone },
								{ "go_Desktop_Stand_Idle", state.stand },
								{ "go_Desktop_Stand_Idle_Mirror", state.stand },
								{ "go_Desktop_Crouch_Idle", state.crouch },
								{ "go_Desktop_Crouch_Idle_Mirror", state.crouch },
								{ "go_Desktop_Prone_Idle", state.prone },
								{ "go_Desktop_Prone_Idle_Mirror", state.prone }
							};
							Dictionary<string, Motion> replacements2 = state.isExtended ? new Dictionary<string, Motion>
							{
								{ "Avatar 3D Thumbnail", state.avatarThumbnail },
								{ "Standing Afk Init", state.afkInit },
								{ "Crouching Afk Init", state.afkInit },
								{ "Proning Afk Init", state.afkInit },
								{ "AFK Standing Loop", state.afkLoop },
								{ "AFK Crouching Loop", state.afkLoop },
								{ "AFK Proning Loop", state.afkLoop },
								{ "AFK Standing Stop", state.afkStop },
								{ "AFK Crouching Stop", state.afkStop },
								{ "AFK Proning Stop", state.afkStop },
								{ "Manual Init", state.afkInit },
								{ "AFK Manual", state.afkLoop },
								{ "AFK Manual Out", state.afkStop },
								{ "Fall", state.fall }
							} : new Dictionary<string, Motion>
							{
								{ "Avatar 3D Thumbnail", state.avatarThumbnail },
								{ "Standing Afk Init", state.afk },
								{ "Crouching Afk Init", state.afk },
								{ "Proning Afk Init", state.afk },
								{ "AFK Manual", state.afk },
								{ "Fall", state.fall }
							};
							var avatarDescriptor = VRChatContextExtensions.VRChatAvatarDescriptor(ctx);
							var baseLayer = avatarDescriptor?.baseAnimationLayers[0].animatorController as AnimatorController;
							var actionLayer = avatarDescriptor?.baseAnimationLayers[3].animatorController as AnimatorController;

							ReplaceAnimations(baseLayer, replacements, ReplacementType.BlendTree, state.isExtended);
							ReplaceAnimations(baseLayer, replacements2, ReplacementType.StateMachine, state.isExtended);
							ReplaceAnimations(actionLayer, replacements2, ReplacementType.StateMachine, state.isExtended);
						}
					}
					catch (Exception e)
					{
						HandleException(e);
					}
				});
		}

		private void HandleException(Exception e)
		{
			if (!EditorUtility.DisplayDialog(DisplayName, $"[{DisplayName}] An error occurred.\nWhen reporting issues, click 'Abort' and include the NDMF console log.\n\n{e.GetBaseException()}", "Ignore and Continue", "Abort"))
			{
				throw new Exception($"[{DisplayName}] An error occurred. Please copy this log when reporting issues:\n{e.GetBaseException()}\n\n[NDMF]");
			}
		}

		private void ReplaceAnimations(AnimatorController animatorController, Dictionary<string, Motion> replacements, ReplacementType type, bool isExtended)
		{
			if (animatorController == null) return;
			AnimatorControllerLayer[] layers = animatorController.layers;
			foreach (AnimatorControllerLayer animatorControllerLayer in layers)
			{
				TraverseStateMachine(animatorControllerLayer.stateMachine, replacements, type, isExtended);
			}
		}

		private void TraverseStateMachine(AnimatorStateMachine stateMachine, Dictionary<string, Motion> replacements, ReplacementType type, bool isExtended)
		{
			ChildAnimatorState[] states = stateMachine.states;
			foreach (ChildAnimatorState childAnimatorState in states)
			{
				AnimatorState state = childAnimatorState.state;
				switch (type)
				{
					case ReplacementType.BlendTree:
						if (state.motion is BlendTree blendTree)
						{
							TraverseBlendTree(stateMachine, blendTree, replacements);
						}
						break;
					case ReplacementType.StateMachine:
						if (!replacements.TryGetValue(state.name, out var value))
						{
							break;
						}
						if (value == null)
						{
							return;
						}
						state.motion = value;
						AnimatorStateTransition[] transitions = state.transitions;
						if (isExtended)
						{
							if (Regex.IsMatch(state.name, ".* Afk Init"))
							{
								transitions[0].exitTime = 1f;
							}
							else if (state.name == "Manual Init")
							{
								transitions[0].exitTime = 1f;
							}
							else if (state.name == "AFK Manual")
							{
								transitions[0].mute = false;
								transitions[1].mute = true;
							}
							else if (state.name == "AFK Manual Out")
							{
								transitions[0].exitTime = 1f;
							}
						}
						else if (Regex.IsMatch(state.name, ".* Afk Init"))
						{
							transitions[0].mute = true;
							transitions[1].mute = false;
						}
						else if (state.name == "AFK Manual")
						{
							stateMachine.defaultState = state;
						}
						if (_debug)
						{
							Debug.Log($"<color=#5ef02f>[GoGoLoco Pose Changer]</color> Replaced motion in state \"{state.name}\" within state machine \"{stateMachine.name}\" with \"{value.name}\".");
						}
						break;
				}
			}
			ChildAnimatorStateMachine[] stateMachines = stateMachine.stateMachines;
			foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
			{
				TraverseStateMachine(childAnimatorStateMachine.stateMachine, replacements, type, isExtended);
			}
		}

		private void TraverseBlendTree(AnimatorStateMachine stateMachine, BlendTree blendTree, Dictionary<string, Motion> replacements)
		{
			// Check for exact match or suffix match (for VRCFury/NDMF copied assets with "Copied from X/" prefix)
			Motion value = null;
			string matchedKey = null;
			if (replacements.TryGetValue(blendTree.name, out value))
			{
				matchedKey = blendTree.name;
			}
			else
			{
				foreach (var kvp in replacements)
				{
					if (blendTree.name.EndsWith("/" + kvp.Key) || blendTree.name.EndsWith(kvp.Key))
					{
						value = kvp.Value;
						matchedKey = kvp.Key;
						break;
					}
				}
			}
			if (matchedKey != null)
			{
				if (value == null)
				{
					return;
				}
				ChildMotion[] children = blendTree.children;
				if (children.Length != 0)
				{
					children[0].motion = value;
					blendTree.children = children;
					if (_debug)
					{
						Debug.Log($"<color=#5ef02f>[GoGoLoco Pose Changer]</color> Replaced motion in blend tree \"{blendTree.name}\" within state machine \"{stateMachine.name}\" with \"{value.name}\".");
					}
				}
			}
			ChildMotion[] children2 = blendTree.children;
			foreach (ChildMotion childMotion in children2)
			{
				if (childMotion.motion is BlendTree blendTree2)
				{
					TraverseBlendTree(stateMachine, blendTree2, replacements);
				}
			}
		}
	}
}
#endif
