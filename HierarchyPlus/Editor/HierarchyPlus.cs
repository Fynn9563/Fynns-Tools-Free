using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using System.IO;
using System.Linq;
using static HierarchyPlus.SavedSettings;
using static HierarchyPlus.StylesContainer;

namespace HierarchyPlus
{
    public class HierarchyPlus
    {
        #region Constants
        private const string PRODUCT_NAME = "HierarchyPlus";
        private const string PACKAGE_ICON_FOLDER_PATH = "CustomIcons";
        private const string MISSING_SCRIPT_ICON_NAME = "Missing";
        private const string DEFAULT_ICON_NAME = "Default";
        private static readonly int DRAG_TOGGLE_HOT_CONTROL_ID = "HierarchyPlusDragToggleId".GetHashCode();
        #endregion
        
        #region Variables
        private static readonly Dictionary<Type, GUIContent> iconCache = new Dictionary<Type, GUIContent>();
        private static readonly Dictionary<string, GUIContent> customIconCache = new Dictionary<string, GUIContent>();
        private static readonly HashSet<string> customIconAssetGuids = new HashSet<string>();
        private static readonly Dictionary<string, GUIContent> layerIconCache = new Dictionary<string, GUIContent>();
        private static readonly Dictionary<string, Texture2D> predefinedLayerIcons = new Dictionary<string, Texture2D>();
        private static bool layerIconsLoaded = false;
        
        // Custom component icon registration system (from BluHierarchy)
        private static readonly Dictionary<System.Type, Texture2D> customComponentIcons = new Dictionary<System.Type, Texture2D>();
        private static readonly Dictionary<Color, Texture2D> colorTextureCache = new Dictionary<Color, Texture2D>();
        private static readonly Texture2D[] defaultTextures = new Texture2D[3];
        private static readonly HashSet<Object> dragToggledObjects = new HashSet<Object>();
        private static bool dragToggleNewState;
        
        // Enhanced drag-toggle state management (from BluHierarchy)
        private static bool isDragToggling = false;
        private static bool targetActiveState = false;
        private static bool suppressNextMouseUp = false;
        
        private static float nextRepaintTime = 0f;

        private static GUIContent gameObjectContent;
        private static GUIContent missingScriptContent;
        private static GUIContent defaultContent;
        private static string iconFolderPath;
        private static Vector2 scroll;

        private static bool ranOnceThisFrame;
        private static int lastMaxIconCount;
        private static int maxIconCount;
        
        private static bool hasInitializedOnLoad = false;
        private static double lastRefreshTime = 0;
        private static double lastIconCheckTime = 0;
        
        internal static System.Action RefreshIconsDelayed;

        private static bool
	        colorsFoldout = true,
	        mainColorsFoldout,
	        miscColorsFolddout,
	        iconsFoldout = true,
	        labelsFoldout,
	        layerLabelFoldout,
	        tagLabelFoldout,
	        coloredItemsFoldout,
	        hiddenIconsFoldout,
	        rowShadingFolout;
        #endregion
        
        #region Window

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			scroll = EditorGUILayout.BeginScrollView(scroll);
			
			using (new GUILayout.HorizontalScope())
			{
				settings.enabled.DrawField("HierarchyPlus Enabled");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Refresh Icons", "Use this to update the icons in the hierarchy window."), GUI.skin.button, GUILayout.ExpandWidth(false))) 
					InitializeAll();
				MakeRectLinkCursor();
			}
			
			if (settings.enabled)
			{
				EditorGUILayout.LabelField("Show in Play Mode", EditorStyles.boldLabel);
				settings.renderInPlayMode.DrawField("Icons");
				settings.renderColorsInPlayMode.DrawField("Colors and Lines");
				settings.renderLabelsInPlayMode.DrawField("Labels");
				settings.renderCollectionsInPlayMode.DrawField("Collection Badges");
			}

			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				colorsFoldout = DrawFoldoutTitle("Colors", colorsFoldout, settings.colorsEnabled);

				if (colorsFoldout)
				{
					using (new EditorGUI.DisabledScope(!settings.GetColorsEnabled()))
					{
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							if (Foldout("Main", ref mainColorsFoldout))
							{
								using (new IndentScope())
								{
									DrawColorSetting("Active Icon Tint", settings.iconTintColor);
									DrawColorSetting("Inactive Icon Tint", settings.iconFadedTintColor);
									DrawColorSetting("Guide Lines", settings.guideLinesColor, settings.guideLinesEnabled);
									DrawColorSetting("Icon Background", settings.iconBackgroundColor, settings.iconBackgroundColorEnabled);
									using (new GUILayout.HorizontalScope())
									{
										var toggle = settings.iconBackgroundOverlapOnly;
										GUIContent toggleTooltip = toggle ? new GUIContent(string.Empty, "Enabled") : new GUIContent(string.Empty, "Disabled");
										toggle.DrawToggle(toggleTooltip, null, EditorStyles.radioButton, pastelGreenColor, Color.grey, GUILayout.Width(18), GUILayout.Height(18));
										var r = GUILayoutUtility.GetLastRect();
										EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
										GUILayout.Label("Icon Background On Overlap Only", EditorStyles.label);
									}
								}
							}


						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							Foldout("Row Coloring", ref rowShadingFolout);
							if (rowShadingFolout)
							{
								using (new IndentScope())
								{
									DrawColorSetting("Odd Color", settings.rowOddColor, settings.rowColoringOddEnabled);
									DrawColorSetting("Even Color", settings.rowEvenColor, settings.rowColoringEvenEnabled);
								}
							}
						}

						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							Foldout("Misc", ref miscColorsFolddout);
							if (miscColorsFolddout)
								using (new IndentScope())
								{
									DrawColorSetting("Misc 1", settings.colorOne, settings.colorOneEnabled);
									DrawColorSetting("Misc 2", settings.colorTwo, settings.colorTwoEnabled);
									DrawColorSetting("Misc 3", settings.colorThree, settings.colorThreeEnabled);
								}
						}

					}

				}
			}

			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				iconsFoldout = DrawFoldoutTitle("Components", iconsFoldout, settings.iconsEnabled);

				if (iconsFoldout)
				{
					using (new EditorGUI.DisabledScope(!settings.GetIconsEnabled()))
					using (new GUILayout.VerticalScope())
					{
						EditorGUIUtility.labelWidth = 200;
						settings.enableContextClick.DrawField("Enable Context Click");
						settings.enableDragToggle.DrawField("Enable Drag-Toggle");
						using (new EditorGUI.DisabledScope(!settings.enableDragToggle))
							settings.requireAltForDragToggle.DrawField("Require Alt for Drag-Toggle");
						settings.showGameObjectIcon.DrawField("Show GameObject Icon");
						settings.showLayerIcon.DrawField("Show Layer Icon");
						settings.showTransformIcon.DrawField("Show Transform Icon");
						settings.showNonBehaviourIcons.DrawField("Show Non-Toggleable Icons");
						settings.alwaysShowIcons.DrawField("Always Render Icons");
						settings.linkCursorOnHover.DrawField("Link Cursor On Hover");
						settings.guiXOffset.value = EditorGUILayout.FloatField("Icons X Offset", settings.guiXOffset.value);
						using (new GUILayout.VerticalScope())
						{
							Foldout(new GUIContent("Hidden Types", "Hover over an icon to see its type name.\nWrite the type name here to hide the icon from the hierarchy view."), ref hiddenIconsFoldout);
							if (hiddenIconsFoldout)
							{
								using (new IndentScope())
								{
									bool changed = false;
									for (int i = 0; i < settings.hiddenIconTypes.Length; i++)
									{
										using (new EditorGUILayout.HorizontalScope())
										{
											EditorGUI.BeginChangeCheck();
											settings.hiddenIconTypes[i].DrawField(GUIContent.none);
											if (EditorGUI.EndChangeCheck())
												changed = true;
											
											if (GUILayout.Button("X", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
											{
												var arr = settings.hiddenIconTypes;
												ArrayUtility.RemoveAt(ref arr, i);
												settings.hiddenIconTypes = arr;
												changed = true;
												i--;
											}

											MakeRectLinkCursor();
										}
									}

									if (GUILayout.Button("+", EditorStyles.toolbarButton))
									{
										var arr = settings.hiddenIconTypes;
										ArrayUtility.Add(ref arr, new SavedString(""));
										settings.hiddenIconTypes = arr;
										changed = true;
									}
									
									if (changed)
										settings.OnHiddenTypesChanged();

									MakeRectLinkCursor();
								}
							}
						}

						EditorGUIUtility.labelWidth = 0;
					}
				}
			}

			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				labelsFoldout = DrawFoldoutTitle("Labels", labelsFoldout, settings.labelsEnabled);

				if (labelsFoldout)
				{
					using (new EditorGUI.DisabledScope(!settings.GetLabelsEnabled()))
					using (new GUILayout.VerticalScope())
					{
						settings.enableLabelContextClick.DrawField("Enable Context Click");
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							layerLabelFoldout = Foldout("Layer Label", ref layerLabelFoldout);
							if (layerLabelFoldout)
							{
								using (new IndentScope())
								{
									settings.layerLabelEnabled.DrawField("Show Layer Label");
									settings.displayDefaultLayerLabel.DrawField("Show Default Label");
									settings.displayLayerIndex.DrawField("Show Layer Index");
								}
							}
						}
						
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							tagLabelFoldout = Foldout("Tag Label", ref tagLabelFoldout);
							if (tagLabelFoldout)
							{
								using (new IndentScope())
								{
									settings.tagLabelEnabled.DrawField("Show Tag Label");
									settings.displayUntaggedLabel.DrawField("Show Untagged Label");
									settings.tagLabelWidth.value = EditorGUILayout.FloatField("Tag Label Width", settings.tagLabelWidth.value);
								}
							}
						}
					}
				}
			}
			
			EditorGUILayout.EndScrollView();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				w_Credit();
			}

			if (EditorGUI.EndChangeCheck())
				EditorApplication.RepaintHierarchyWindow();
		}
    
		private static void w_Credit()
        {
	        using (new ColoredScope(ColoredScope.ColoringType.BG, Color.clear))
	        {
		        if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), Styles.faintLinkLabel))
			        Application.OpenURL("https://github.com/Fynn9563");
		        w_UnderlineLastRectOnHover();
	        }
        }
        internal static void w_UnderlineLastRectOnHover(Color? color = null)
        {
	        if (color == null) color = new Color(0.3f, 0.7f, 1);
	        if (Event.current.type == EventType.Repaint)
	        {
		        Rect rect = GUILayoutUtility.GetLastRect();
		        Vector2 mp = Event.current.mousePosition;
		        if (rect.Contains(mp)) EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color.Value);
		        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
	        }

        }
        #endregion

        #region Hierarchy
        private static ColoredScope colorScope;
        private static ColoredScope colorScope2;
        private static ColoredScope colorScope3;

        private static void OnHierarchyItemGUI(int id, Rect rect)
        {
	        if (!ranOnceThisFrame)
	        {
		        ranOnceThisFrame = true;
		        lastMaxIconCount = maxIconCount;
		        maxIconCount = 0;
	        }

	        DisposeOfColorScopes();
	        bool isPlaying = EditorApplication.isPlaying;
	        bool colorsEnabled = settings.GetColorsEnabled() && (!isPlaying || settings.renderColorsInPlayMode);
	        bool willDrawColors = colorsEnabled && (settings.guideLinesEnabled || settings.GetRowColoringEnabled());
	        bool willDrawIcons = settings.GetIconsEnabled() && (!isPlaying || settings.renderInPlayMode);
	        bool willDrawLabels = settings.GetLabelsEnabled() && (!isPlaying || settings.renderLabelsInPlayMode);
	        bool willDrawCollections = !CollectionsHierarchy.DrawingVirtualObjectRow && settings.showCollectionsInNormalMode && settings.GetIconsEnabled() && (!isPlaying || settings.renderCollectionsInPlayMode);
	        if (colorsEnabled)
	        {
		        colorScope = new ColoredScope(ColoredScope.ColoringType.General, settings.colorOneEnabled, settings.colorOne);
		        colorScope2 = new ColoredScope(ColoredScope.ColoringType.FG, settings.colorTwoEnabled, settings.colorTwo);
		        colorScope3 = new ColoredScope(ColoredScope.ColoringType.BG, settings.colorThreeEnabled, settings.colorThree);
	        }

	        if (!willDrawColors && !willDrawIcons && !willDrawLabels && !willDrawCollections) return;

	        Object obj = EditorUtility.InstanceIDToObject(id);
	        if (!(obj is GameObject go)) return;


	        if (willDrawColors)
	        {
		        var t = go.transform;
		        var p = t.parent;
		        bool hasParent = p;
		        bool isLastChild = hasParent && t.GetSiblingIndex() == p.childCount - 1;
		        bool hasChildren = t.childCount > 0;

		        List<bool> middleLines = new List<bool>();
		        int depth = 0;

		        if (settings.guideLinesEnabled)
		        {
			        while (p)
			        {
				        middleLines.Insert(0, t.GetSiblingIndex() != p.childCount - 1);
				        depth++;
				        t = p;
				        p = p.parent;
			        }
		        }
		        else
			        while (p)
			        {
				        depth++;
				        t = p;
				        p = p.parent;
			        }

		        int marginWidth = hasChildren ? 14 : 2;
		        int lineWidth = 14 * depth + 34;
		        Rect lineRect = new Rect(rect.x - lineWidth, rect.y, lineWidth - marginWidth, rect.height);

		        if (settings.GetRowColoringEnabled() && Event.current.type == EventType.Repaint)
		        {
			        Rect backgroundRect = new Rect(lineRect);
			        backgroundRect.width += rect.width + marginWidth + 12;
			        backgroundRect.x += 5;



			        if (backgroundRect.y % 32 > 15)
			        {
				        if (settings.rowColoringOddEnabled)
					        EditorGUI.DrawRect(backgroundRect, settings.rowOddColor);
			        }
			        else if (settings.rowColoringEvenEnabled)
				        EditorGUI.DrawRect(backgroundRect, settings.rowEvenColor);
		        }

		        if (hasParent && settings.guideLinesEnabled && !CollectionsHierarchy.DrawingVirtualObjectRow)
		        {
			        int extraWidth = hasChildren ? 0 : 12;

			        void Line(Vector3 start, Vector3 end) => Handles.DrawAAPolyLine(1, 2, start, end);

			        Handles.color = settings.guideLinesColor;

			        GUI.BeginClip(lineRect);
			        float basef = lineWidth - marginWidth;
			        Vector3 startingPoint = new Vector3(basef, rect.height / 2);
			        Vector3 middlePoint = new Vector3(basef - extraWidth - 8, rect.height / 2);
			        Line(startingPoint, middlePoint);

			        if (isLastChild)
			        {
				        Vector3 connectionPoint = new Vector3(basef - extraWidth - 8, 0);
				        Line(middlePoint, connectionPoint);
			        }

			        for (int i = 0; i < middleLines.Count; i++)
			        {
				        if (!middleLines[i]) continue;
				        float x = lineRect.x + 14 * i;
				        Vector3 topConnection = new Vector3(x, 0);
				        Vector3 bottomConnection = new Vector3(x, rect.height);
				        Line(topConnection, bottomConnection);
			        }

			        GUI.EndClip();
		        }
	        }

	        float nameAdjust = GUI.skin.label.CalcSize(new GUIContent(go.name)).x + 18;
	        Rect baseRect = new Rect(rect) {width = rect.width - 32 + settings.guiXOffset - nameAdjust};
	        baseRect.x += nameAdjust;

	        if (willDrawIcons || willDrawCollections)
	        {
		        Rect availableIconArea = new Rect(baseRect);
		        int currentIconCount = 0;
		        Rect iconRect = availableIconArea;
		        iconRect.x = availableIconArea.xMax - 18;
		        iconRect.width = 18;

		        bool CanDrawIcon(out bool drawBackground, out float alpha)
		        {
			        currentIconCount++;
			        bool dotsOnly = availableIconArea.width < 36;
			        bool overlapping = availableIconArea.width < 18;
			        bool drawIcon = settings.alwaysShowIcons || (!dotsOnly && !overlapping);
			        drawBackground = drawIcon && colorsEnabled && settings.iconBackgroundColorEnabled && (overlapping || !settings.iconBackgroundOverlapOnly);
			        
			        GUIStyle labelStyle = EditorStyles.label;
			        float nameWidth = labelStyle.CalcSize(new GUIContent(go.name)).x;
			        float minX = rect.x + nameWidth + 8f; 
			        float fadeThreshold = minX + 16f; 
			        
			        alpha = 1f;
			        if (iconRect.x < minX)
			        {
				        // Hide icons before they overlap the object name.
				        return false;
			        }
			        else if (iconRect.x < fadeThreshold)
			        {
				        alpha = Mathf.InverseLerp(minX, fadeThreshold, iconRect.x);
			        }

			        if (!drawIcon && dotsOnly)
			        {
				        var prevColor = GUI.color;
				        GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, alpha);
				        GUI.Label(iconRect, "...", EditorStyles.centeredGreyMiniLabel);
				        GUI.color = prevColor;
				        availableIconArea.width -= 18;
				        return false;
			        }

			        availableIconArea.width -= 18;
			        return drawIcon;
		        }

		        
		        if (willDrawIcons)
		        {
		        
		        if (settings.showGameObjectIcon && CanDrawIcon(out bool withBg, out float goAlpha))
		        {
			        if (Mathf.RoundToInt((float)settings.gameObjectToggleStyle.value) == 1)
			        {
				        iconRect = DrawTickBoxToggle(iconRect, go, withBg, goAlpha);
			        }
			        else
			        {
				        iconRect = DrawIconToggle(iconRect, go, withBg, goAlpha);
			        }
		        }

		        int missingScriptCount = ComponentIconHandler.GetMissingScriptCount(go);
		        if (missingScriptCount > 0 && CanDrawIcon(out bool withBgMissing, out float missingAlpha))
		        {
			        var warningIcon = ComponentIconHandler.GetMissingScriptWarningIcon(missingScriptCount);
			        iconRect = DrawMissingScriptIcon(iconRect, warningIcon, withBgMissing, missingAlpha);
		        }

		        bool isFirstComponent = true;
		        foreach (var c in go.GetComponents<Component>())
		        {
			        if (c != null)
			        {
				        // Check for hidden components (from BluHierarchy)
				        if (!settings.showHiddenComponents && (c.hideFlags & HideFlags.HideInInspector) != 0)
					        continue;
					        
				        if (isFirstComponent)
				        {
					        isFirstComponent = false;
					        if (!settings.showTransformIcon)
						        continue;
				        }
				        else
				        {
					        if (!settings.showNonBehaviourIcons)
						        if (!(c is Behaviour))
							        continue;
				        }

				        if (!isFirstComponent && settings.IsTypeHidden(c.GetType().Name)) continue;
			        }

			        if (CanDrawIcon(out bool withBg2, out float compAlpha))
			        {
				        Rect nextRect = iconRect;
				        iconRect = DrawIconToggle(iconRect, c, withBg2, compAlpha);
				        Event e = Event.current;
				        if (settings.enableContextClick && e.type == EventType.MouseDown && e.button == 1 && nextRect.Contains(e.mousePosition))
				        {
					        var method = typeof(EditorUtility).GetMethod("DisplayObjectContextMenu", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] {typeof(Rect), typeof(Object[]), typeof(int)}, null);
					        method?.Invoke(null, new object[] {nextRect, new Object[] {c == null ? c as MonoBehaviour : c}, 0});
					        e.Use();
				        }
			        }
		        }
		        } 

		        if (currentIconCount > maxIconCount) maxIconCount = currentIconCount;
		        
		        if (willDrawCollections && Collections.collections != null && Collections.collections.Count > 0)
		        {
			        var collectionsForGo = Collections.GetCollectionsForObjectStatic(go);
			        if (collectionsForGo != null && collectionsForGo.Count > 0 && CanDrawIcon(out bool withBgColl, out float collAlpha))
			        {
				        Collections.DrawCollectionIcon(iconRect, collectionsForGo, withBgColl, collAlpha);
			        }
		        }
	        }

	        if (willDrawLabels || (willDrawIcons && settings.showLayerIcon))
	        {
		        GUIStyle als = "assetlabel";
		        Rect availableLabelsArea = baseRect;
		        availableLabelsArea.width -= 18 * (lastMaxIconCount + 1);

		        string layerName = LayerMask.LayerToName(go.layer);
		        bool showLayerText = willDrawLabels && settings.layerLabelEnabled && (settings.displayDefaultLayerLabel || go.layer != 0);
		        GUIContent layerIcon = null;
		        if (willDrawIcons && settings.showLayerIcon && go.layer != 0)
		        {
			        if (predefinedLayerIcons.TryGetValue(layerName, out var predefinedIcon)) layerIcon = new GUIContent(predefinedIcon, $"Layer: {layerName}");
			        else customIconCache.TryGetValue("L_" + layerName, out layerIcon);
		        }
		        if (showLayerText || layerIcon?.image != null)
		        {
			        string label = settings.displayLayerIndex ? $"{go.layer}: {layerName}" : layerName;
			        float iconWidth = layerIcon?.image != null ? 18 : 0;
			        float textWidth = showLayerText ? Mathf.Ceil(als.CalcSize(new GUIContent(label)).x) : 0;
			        Rect layerRect = UseRectEnd(ref availableLabelsArea, iconWidth + textWidth);
			        float alpha = Mathf.InverseLerp(baseRect.x + 8, baseRect.x + 24, layerRect.x);
			        if (alpha > 0)
			        {
				        if (iconWidth > 0) DrawLayerIcon(new Rect(layerRect.x, layerRect.y, iconWidth, layerRect.height), layerIcon, go,
				            colorsEnabled && settings.iconBackgroundColorEnabled && !settings.iconBackgroundOverlapOnly, alpha);
				        if (showLayerText)
				        {
				            var textRect = layerRect; textRect.xMin += iconWidth;
				            using (new ColoredScope(ColoredScope.ColoringType.BG, new Color(0, 0, 0, 0.4f * alpha)))
				            using (new ColoredScope(ColoredScope.ColoringType.FG, new Color(0.7f, 0.7f, 0.7f, alpha)))
				                GUI.Label(textRect, label, als);
				        }

				        if (alpha > 0.5f && settings.enableLabelContextClick && RightClicked(layerRect))
				        {
					        var layerNames = Enumerable.Range(0, 31).Select(LayerMask.LayerToName).ToArray();

					        GenericMenu layerMenu = new GenericMenu();
					        for (int i = 0; i < layerNames.Length; i++)
					        {
						        var n = layerNames[i];
						        if (!string.IsNullOrEmpty(n))
						        {
							        int index = i;
							        layerMenu.AddItem(new GUIContent($"{i}: {n}"), go.layer == index, () =>
							        {
								        Undo.RecordObject(go, "Change Layer");
								        go.layer = index;
								        EditorUtility.SetDirty(go);
							        });
						        }
					        }

					        layerMenu.ShowAsContext();
				        }
			        }
		        }
		        
		        if (willDrawLabels && settings.tagLabelEnabled && (settings.displayUntaggedLabel || !go.CompareTag("Untagged")))
		        {
			        UseRectEnd(ref availableLabelsArea, 18);
			        float maxWidth = availableLabelsArea.width;
			        float labelWidth = settings.tagLabelWidth;
			        Rect tagRect = UseRectEnd(ref availableLabelsArea, labelWidth);
			        tagRect.width = Mathf.Clamp(tagRect.width, 0, Mathf.Max(0, maxWidth));
			        if (tagRect.width > 10)
			        {
				        tagRect.x += labelWidth - tagRect.width;
				        string tagName = go.tag;
				        using (new ColoredScope(ColoredScope.ColoringType.BG, new Color(0, 0, 0, 0.4f)))
				        using (new ColoredScope(ColoredScope.ColoringType.FG, new Color(0.7f, 0.7f, 0.7f)))
					        GUI.Label(tagRect, tagName, "assetlabel");

				        if (settings.enableLabelContextClick && RightClicked(tagRect))
				        {
					        var tagNames = UnityEditorInternal.InternalEditorUtility.tags;

					        GenericMenu tagMenu = new GenericMenu();
					        foreach (var n in tagNames)
					        {
						        if (string.IsNullOrEmpty(n)) continue;
						        tagMenu.AddItem(new GUIContent(n), go.CompareTag(n), () =>
						        {
							        Undo.RecordObject(go, "Change Tag");
							        go.tag = n;
							        EditorUtility.SetDirty(go);
						        });
					        }

					        tagMenu.ShowAsContext();
				        }
			        }
		        }
	        }
        }

        private static void DisposeOfColorScopes()
        {
	        colorScope3?.Dispose();
	        colorScope2?.Dispose();
	        colorScope?.Dispose();
        }
        #endregion

        #region Drawing Helpers

        internal static Rect UseRectEnd(ref Rect rect, float width)
        {
	        var returnRect = rect;
	        rect.width -= width;
	        returnRect.x = rect.xMax;
	        returnRect.width = width;
	        return returnRect;
        }
        
        internal static void MakeRectLinkCursor(Rect rect = default)
        {
	        if (Event.current.type == EventType.Repaint)
	        {
		        if (rect == default) rect = GUILayoutUtility.GetLastRect();
		        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
	        }
        }

        private static bool Foldout(GUIContent label, ref bool b)
        {
	        return b = EditorGUILayout.Foldout(b, label, true);
        }

        private static bool Foldout(string label, ref bool b) => Foldout(new GUIContent(label), ref b);

        private static bool DrawFoldoutTitle(string label, bool foldout, SavedBool enabled)
        {
	        using (new GUILayout.HorizontalScope())
	        {
		        var r = EditorGUILayout.GetControlRect(false, 24, Styles.bigTitle, GUILayout.ExpandWidth(true));
		        GUI.Label(r, label, EditorStyles.whiteLargeLabel);

		        if (enabled != null)
		        {
			        enabled.DrawToggle("Enabled", "Disabled", null, pastelGreenColor, pastelRedColor, GUILayout.ExpandWidth(false));
			        MakeRectLinkCursor();
		        }

		        if (LeftClicked(r)) foldout = !foldout;
		        MakeRectLinkCursor(r);
	        }

	        return foldout;
        }
        private static void DrawColorSetting(string label, SavedColor color, SavedBool toggle = null)
        {
	        using (new GUILayout.HorizontalScope())
	        {
		        if (toggle != null)
		        {
			        GUIContent toggleTooltip = toggle ? new GUIContent("","Enabled") : new GUIContent("","Disabled");
			        toggle.DrawToggle(toggleTooltip, null, EditorStyles.radioButton, pastelGreenColor, Color.grey, GUILayout.Width(18), GUILayout.Height(18));
			        var r = GUILayoutUtility.GetLastRect();
			        EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
		        } else using (new EditorGUI.DisabledScope(true))
			        GUILayout.Toggle(true, " ", EditorStyles.radioButton, GUILayout.Width(18), GUILayout.Height(18));
		        
		        EditorGUILayout.PrefixLabel(label);
		        color.DrawField(GUIContent.none);
	        }
        }

        private static Rect DrawIconToggle(Rect rect, GameObject go, bool withBackground, float alpha = 1f)
        {
	        var e = Event.current;
	        bool isActive = go.activeSelf;
	        
	        // Enhanced drag-to-toggle system (from BluHierarchy)
	        if (settings.enableDragToggle)
	        {
		        if (e.type == EventType.MouseDown && (!settings.requireAltForDragToggle || e.alt) && e.button == 0 && rect.Contains(e.mousePosition))
		        {
			        Undo.RecordObject(go, "[H+] Toggle GameObject (Drag Start)");
			        bool newState = !isActive;
			        go.SetActive(newState);
			        EditorUtility.SetDirty(go);
			        
			        isDragToggling = true;
			        targetActiveState = newState;
			        suppressNextMouseUp = true;
			        dragToggledObjects.Add(go);
			        
			        e.Use();
			        RepaintHierarchyWindow();
		        }
		        else if (isDragToggling && (e.type == EventType.MouseDrag || e.type == EventType.MouseMove))
		        {
			        if (rect.Contains(e.mousePosition) && isActive != targetActiveState && !dragToggledObjects.Contains(go))
			        {
				        Undo.RecordObject(go, "[H+] Toggle GameObject (Drag)");
				        go.SetActive(targetActiveState);
				        EditorUtility.SetDirty(go);
				        dragToggledObjects.Add(go);
				        e.Use();
				        RepaintHierarchyWindow();
			        }
		        }
		        else if (e.type == EventType.MouseUp && e.button == 0)
		        {
			        if (suppressNextMouseUp)
			        {
				        e.Use();
			        }
			        isDragToggling = false;
			        suppressNextMouseUp = false;
			        dragToggledObjects.Clear();
		        }
	        }
	        
	        bool leftClicked = LeftClicked(rect);
	        if (leftClicked && !isDragToggling)
	        {
		        Undo.RecordObject(go, "[H+] Toggle GameObject");
		        go.SetActive(!isActive);
		        EditorUtility.SetDirty(go);
		        RepaintHierarchyWindow();
	        }

	        return DrawIcon(rect, go, !isActive, withBackground, alpha);
        }

        private static Rect DrawTickBoxToggle(Rect rect, GameObject go, bool withBackground, float alpha = 1f)
        {
	        var e = Event.current;
	        bool isActive = go.activeSelf;
	        
	        if (settings.enableDragToggle)
	        {
		        if (e.type == EventType.MouseDown && (!settings.requireAltForDragToggle || e.alt) && e.button == 0 && rect.Contains(e.mousePosition))
		        {
			        Undo.RecordObject(go, "[H+] Toggle GameObject (Drag Start)");
			        bool newState = !isActive;
			        go.SetActive(newState);
			        EditorUtility.SetDirty(go);
			        
			        isDragToggling = true;
			        targetActiveState = newState;
			        suppressNextMouseUp = true;
			        dragToggledObjects.Add(go);
			        
			        e.Use();
			        RepaintHierarchyWindow();
		        }
		        else if (isDragToggling && (e.type == EventType.MouseDrag || e.type == EventType.MouseMove))
		        {
			        if (rect.Contains(e.mousePosition) && isActive != targetActiveState && !dragToggledObjects.Contains(go))
			        {
				        Undo.RecordObject(go, "[H+] Toggle GameObject (Drag)");
				        go.SetActive(targetActiveState);
				        EditorUtility.SetDirty(go);
				        dragToggledObjects.Add(go);
				        e.Use();
				        RepaintHierarchyWindow();
			        }
		        }
		        else if (e.type == EventType.MouseUp && e.button == 0)
		        {
			        if (suppressNextMouseUp)
			        {
				        e.Use();
			        }
			        isDragToggling = false;
			        suppressNextMouseUp = false;
			        dragToggledObjects.Clear();
		        }
	        }
	        
	        if (withBackground) 
	        {
		        var bgColor = settings.iconBackgroundColor.color;
		        bgColor.a *= alpha;
		        EditorGUI.DrawRect(rect, bgColor);
	        }
	        
	        EditorGUI.BeginDisabledGroup(isDragToggling);
	        bool uiState = EditorGUI.Toggle(rect, isActive);
	        EditorGUI.EndDisabledGroup();
	        
	        if ((!settings.enableDragToggle || !isDragToggling) && uiState != isActive)
	        {
		        Undo.RecordObject(go, "[H+] Toggle GameObject");
		        go.SetActive(uiState);
		        EditorUtility.SetDirty(go);
		        RepaintHierarchyWindow();
	        }
	        
	        rect.x -= 18;
	        return rect;
        }

        private static Rect DrawIconToggle(Rect rect, Component c, bool withBackgroun, float alpha = 1f)
        {
	        if (!IsComponentToggleable(c)) return DrawComponentIcon(GetIcon(c), rect, c, withBackgroun, alpha);
	        
	        var e = Event.current;
	        bool isEnabled = IsComponentEnabled(c);
	        
	        // Enhanced drag-to-toggle system for components (from BluHierarchy)
	        if (settings.enableDragToggle)
	        {
		        if (e.type == EventType.MouseDown && (!settings.requireAltForDragToggle || e.alt) && e.button == 0 && rect.Contains(e.mousePosition))
		        {
			        Undo.RecordObject(c, "[H+] Toggle Component (Drag Start)");
			        bool newState = !isEnabled;
			        SetComponentEnabled(c, newState);
			        EditorUtility.SetDirty(c);
			        
			        isDragToggling = true;
			        targetActiveState = newState;
			        suppressNextMouseUp = true;
			        dragToggledObjects.Add(c);
			        
			        e.Use();
			        RepaintHierarchyWindow();
		        }
		        else if (isDragToggling && (e.type == EventType.MouseDrag || e.type == EventType.MouseMove))
		        {
                if (rect.Contains(e.mousePosition) && isEnabled != targetActiveState && !dragToggledObjects.Contains(c))
			        {
				        Undo.RecordObject(c, "[H+] Toggle Component (Drag)");
				        SetComponentEnabled(c, targetActiveState);
				        EditorUtility.SetDirty(c);
				        dragToggledObjects.Add(c);
				        e.Use();
				        RepaintHierarchyWindow();
			        }
		        }
		        else if (e.type == EventType.MouseUp && e.button == 0)
		        {
			        if (suppressNextMouseUp)
			        {
				        e.Use();
			        }
			        isDragToggling = false;
			        suppressNextMouseUp = false;
			        dragToggledObjects.Clear();
		        }
	        }
	        
	        bool leftClicked = LeftClicked(rect);
	        if (leftClicked && !isDragToggling)
	        {
		        Undo.RecordObject(c, "[H+] Toggle Component");
		        SetComponentEnabled(c, !isEnabled);
		        EditorUtility.SetDirty(c);
		        RepaintHierarchyWindow();
	        }

	        return DrawComponentIcon(GetIcon(c), rect, c, withBackgroun, alpha);
        }
        private static Rect DrawIcon(Rect rect, Component c, bool faded, bool withBackground) => DrawIcon(GetIcon(c), rect, faded, withBackground);
        
        private static Rect DrawLayerIcon(Rect rect, GUIContent layerIcon, GameObject go, bool withBackground, float alpha = 1f)
        {
	        bool faded = !go.activeInHierarchy;
	        var iconColor = faded ? settings.iconFadedTintColor.color : settings.iconTintColor.color;
	        iconColor.a *= alpha; 
	        
	        using (new ColoredScope(ColoredScope.ColoringType.All, iconColor))
	        {
		        if (withBackground) 
		        {
			        var bgColor = settings.iconBackgroundColor.color;
			        bgColor.a *= alpha;
			        EditorGUI.DrawRect(rect, bgColor);
		        }
		        GUI.Label(rect, layerIcon);
		        if (settings.linkCursorOnHover)
			        MakeRectLinkCursor(rect);
	        }
	        rect.x -= 18;
	        return rect;
        }

        private static Rect DrawMissingScriptIcon(Rect rect, GUIContent warningIcon, bool withBackground, float alpha = 1f)
        {
	        // Keep missing-script warnings visible.
	        var iconColor = Color.red; 
	        iconColor.a *= alpha; 
	        
	        using (new ColoredScope(ColoredScope.ColoringType.All, iconColor))
	        {
		        if (withBackground) 
		        {
			        var bgColor = settings.iconBackgroundColor.color;
			        bgColor.a *= alpha;
			        EditorGUI.DrawRect(rect, bgColor);
		        }
		        GUI.Label(rect, warningIcon);
		        if (settings.linkCursorOnHover)
			        MakeRectLinkCursor(rect);
	        }
	        rect.x -= 18;
	        return rect;
        }

        private static Rect DrawIcon(Rect rect, GameObject go, bool faded, bool withBackground)
        {
	        return DrawIcon(GetGameObjectIcon(go), rect, faded, withBackground);
        }
        
        private static Rect DrawIcon(Rect rect, GameObject go, bool faded, bool withBackground, float alpha = 1f)
        {
	        return DrawIcon(GetGameObjectIcon(go), rect, faded, withBackground, alpha);
        }

        private static GUIContent GetGameObjectIcon(GameObject go)
        {
            if (Mathf.RoundToInt(settings.gameObjectToggleStyle.value) == 2)
            {
                var icon = EditorGUIUtility.GetIconForObject(go);
                if (icon != null) return new GUIContent(icon, gameObjectContent?.tooltip);
            }
            return gameObjectContent;
        }
        
        private static Rect DrawIcon(GUIContent content, Rect rect, bool faded, bool withBackground)
        {
	        if (content?.image == null) return rect;
	        
	        using (new ColoredScope(ColoredScope.ColoringType.All, faded, settings.iconFadedTintColor, settings.iconTintColor))
	        {
		        if (withBackground) EditorGUI.DrawRect(rect, settings.iconBackgroundColor);
		        GUI.Label(rect, content);
		        if (settings.linkCursorOnHover)
			        MakeRectLinkCursor(rect);
	        }
            rect.x -= 18;
            return rect;
        }
        
        private static Rect DrawIcon(GUIContent content, Rect rect, bool faded, bool withBackground, float alpha = 1f)
        {
	        if (content?.image == null) return rect;
	        
	        var iconColor = faded ? settings.iconFadedTintColor.color : settings.iconTintColor.color;
	        iconColor.a *= alpha; 
	        
	        using (new ColoredScope(ColoredScope.ColoringType.All, iconColor))
	        {
		        if (withBackground) 
		        {
			        var bgColor = settings.iconBackgroundColor.color;
			        bgColor.a *= alpha;
			        EditorGUI.DrawRect(rect, bgColor);
		        }
		        GUI.Label(rect, content);
		        if (settings.linkCursorOnHover)
			        MakeRectLinkCursor(rect);
	        }
            rect.x -= 18;
            return rect;
        }
        
        private static Rect DrawComponentIcon(GUIContent content, Rect rect, Component component, bool withBackground, float alpha = 1f)
        {
	        if (content?.image == null) return rect;
	        
	        bool isDisabled = !IsComponentActiveAndEnabled(component);
	        bool isHovered = rect.Contains(Event.current.mousePosition);
	        bool isMissing = component == null || content == missingScriptContent;
	        
	        Color iconTint = settings.iconTintColor;
	        Color bgColor = settings.iconBackgroundColor;
	        
	        if (isMissing)
	        {
		        iconTint = new Color(1f, 0.4f, 0.4f, 0.9f);
		        bgColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
	        }
	        else if (isDisabled)
	        {
		        iconTint = new Color(1f, 1f, 1f, 0.5f);
		        bgColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.5f);
	        }
	        else if (isHovered && IsComponentToggleable(component))
	        {
		        iconTint = new Color(iconTint.r * 1.1f, iconTint.g * 1.1f, iconTint.b * 1.1f, iconTint.a);
		        bgColor = new Color(bgColor.r * 1.2f, bgColor.g * 1.2f, bgColor.b * 1.2f, bgColor.a);
	        }
	        
	        iconTint.a *= alpha;
	        bgColor.a *= alpha;
	        
	        using (new ColoredScope(ColoredScope.ColoringType.All, iconTint))
	        {
		        if (withBackground) 
		        {
			        EditorGUI.DrawRect(rect, bgColor);
			        
			        if (isMissing)
			        {
				        var borderColor = new Color(1f, 0.2f, 0.2f, 0.6f * alpha);
				        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), borderColor); 
				        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), borderColor); 
				        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), borderColor); 
				        EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), borderColor); 
			        }
		        }
		        
		        var imageRect = new Rect(rect.center.x - 8, rect.center.y - 8, 16, 16);
		        GUI.DrawTexture(imageRect, content.image, ScaleMode.ScaleToFit, true);
		        GUI.Label(rect, new GUIContent(string.Empty, content.tooltip), GUIStyle.none);
		        
		        if (isHovered && alpha > 0.5f) 
		        {
			        if (IsComponentToggleable(component))
			        {
				        if (settings.linkCursorOnHover)
					        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
				        
				        if (settings.showTooltips && Event.current.type == EventType.Repaint)
				        {
					        string stateText = isDisabled ? "Click to Enable" : "Click to Disable";
					        var hoverStyle = new GUIStyle(EditorStyles.miniLabel);
					        hoverStyle.normal.textColor = Color.white;
					        hoverStyle.alignment = TextAnchor.MiddleCenter;
					        hoverStyle.fontSize = 8;
					        
					        var textSize = hoverStyle.CalcSize(new GUIContent(stateText));
					        var tooltipRect = new Rect(rect.x - textSize.x/2 + rect.width/2, rect.y - 15, textSize.x + 4, 12);
					        
					        if (tooltipRect.y > 0) 
					        {
						        EditorGUI.DrawRect(tooltipRect, new Color(0.1f, 0.1f, 0.1f, 0.9f));
						        GUI.Label(tooltipRect, stateText, hoverStyle);
					        }
				        }
			        }
			        else if (settings.linkCursorOnHover)
			        {
				        MakeRectLinkCursor(rect);
			        }
		        }
	        }
	        rect.x -= 18;
	        return rect;
        }
        
        private static bool IsComponentActiveAndEnabled(Component component)
        {
	        return ComponentIconHandler.IsComponentActiveAndEnabled(component);
        }
        
        private static Rect DrawVisualSeparator(Rect rect, float alpha = 1f)
        {
	        float separatorWidth = 1f;
	        float separatorHeight = rect.height;
	        
	        float spacing = 4f;
	        var separatorRect = new Rect(rect.x - separatorWidth - spacing, rect.y, separatorWidth, separatorHeight);
	        
	        var separatorColor = new Color(0.5f, 0.5f, 0.5f, 0.6f * alpha);
	        EditorGUI.DrawRect(separatorRect, separatorColor);
	        
	        rect.x -= separatorWidth + spacing * 2;
	        return rect;
        }

        private static GUIContent GetIcon(Component c)
        {
            var icon = ComponentIconHandler.GetComponentIcon(c, customIconCache, missingScriptContent, defaultContent, defaultTextures, GetCustomIcon, settings.showTooltips);
            
            if (!settings.showTooltips && icon != null)
            {
                icon = new GUIContent(icon.image, null);
            }
            
            if (c != null && icon != missingScriptContent && icon != defaultContent)
            {
                Type type = c.GetType();
                if (!iconCache.ContainsKey(type))
                {
                    iconCache.Add(type, icon);
                }
            }
            
            return icon;
        }
        
        internal static Texture2D GenerateColorTexture(float r, float g, float b, float a = 1)
        {
	        if (r > 1) r /= 255;
	        if (g > 1) g /= 255;
	        if (b > 1) b /= 255;
	        if (a > 1) a /= 255;

	        return GenerateColorTexture(new Color(r, g, b, a));
        }

        internal static Texture2D GenerateColorTexture(Color color)
        {
	        if (!colorTextureCache.TryGetValue(color, out var texture))
	        {
		        texture = new Texture2D(1, 1);
		        texture.anisoLevel = 0;
		        texture.filterMode = FilterMode.Point;
		        texture.SetPixel(0, 0, color);
		        texture.Apply();
		        colorTextureCache[color] = texture;
		        
		        // Bound the texture cache.
		        if (colorTextureCache.Count > 100)
		        {
			        var firstKey = colorTextureCache.Keys.First();
			        if (colorTextureCache[firstKey] != null)
				        Object.DestroyImmediate(colorTextureCache[firstKey]);
			        colorTextureCache.Remove(firstKey);
		        }
	        }
	        return texture;
        }
        
        [InitializeOnLoadMethod]
        private static void CleanupTextures()
        {
	        AssemblyReloadEvents.beforeAssemblyReload += () =>
	        {
		        foreach (var texture in colorTextureCache.Values)
		        {
			        if (texture != null)
				        Object.DestroyImmediate(texture);
		        }
		        colorTextureCache.Clear();
	        };
        }
        #endregion
        
        #region Functional Helpers
        private static bool IsComponentToggleable(Component c) => ComponentIconHandler.IsComponentToggleable(c);

        private static bool IsComponentEnabled(Component c)
        {
	        return ComponentIconHandler.IsComponentActiveAndEnabled(c);
        }

        private static void SetComponentEnabled(Component c, bool enabled)
        {
	        ComponentIconHandler.SetComponentEnabled(c, enabled);
        }

        private static int GetDepth(GameObject go) => GetDepth(go.transform);
        private static int GetDepth(Transform t)
        {
	        int depth = 0;
	        while (t.parent != null)
	        {
		        depth++;
		        t = t.parent;
	        }

	        return depth;
        }
        private static bool LeftClicked(Rect rect)
        {
            var e = Event.current;
            bool clicked = e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition);
            if (clicked) e.Use();
            return clicked;
        }
        private static bool RightClicked(Rect rect)
        {
            var e = Event.current;
            bool clicked = e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition);
            if (clicked) e.Use();
            return clicked;
        }

        private static bool MouseDraggedOver(Rect rect, Object o)
        {
	        var e = Event.current;
	        return GUIUtility.hotControl == DRAG_TOGGLE_HOT_CONTROL_ID && e.type != EventType.Layout && rect.Contains(e.mousePosition) && !dragToggledObjects.Contains(o);
        }

        private static void StartDragToggling(bool toggleToState)
        {
	        dragToggledObjects.Clear();
	        dragToggleNewState = toggleToState;
	        if (settings.enableDragToggle) 
		        GUIUtility.hotControl = DRAG_TOGGLE_HOT_CONTROL_ID;
        }
        
        private static void RepaintHierarchyWindow()
        {
	        // Performance optimization: Throttle repaints (from BluHierarchy)
	        if (Time.realtimeSinceStartup < nextRepaintTime) return;
	        
	        nextRepaintTime = Time.realtimeSinceStartup + 0.1f; 
	        EditorApplication.RepaintHierarchyWindow();
        }
        #endregion
        
        #region Layer Icon Loading
        
        // Adapted from BluHierarchy.
        private static void LoadPredefinedLayerIcons()
        {
	        if (EditorApplication.isPlaying) return;
	        
	        if (layerIconsLoaded) return;
	        
	        try
	        {
		        TryLoadLayerIcon("TransparentFX", "L_TransparentFX");
		        TryLoadLayerIcon("Ignore Raycast", "L_IgnoreRaycast");
		        TryLoadLayerIcon("reserved3", "L_Reserved");
		        TryLoadLayerIcon("Item", "L_Item");
		        TryLoadLayerIcon("Water", "L_Water");
		        TryLoadLayerIcon("UI", "L_UI");
		        TryLoadLayerIcon("reserved6", "L_Reserved");
		        TryLoadLayerIcon("reserved7", "L_Reserved");
		        TryLoadLayerIcon("Interactive", "L_Interactive");
		        TryLoadLayerIcon("Player", "L_Player");
		        TryLoadLayerIcon("PlayerLocal", "L_PlayerLocal");
		        TryLoadLayerIcon("Environment", "L_Environment");
		        TryLoadLayerIcon("UiMenu", "L_UiMenu");
		        TryLoadLayerIcon("Pickup", "L_Pickup");
		        TryLoadLayerIcon("PickupNoEnvironment", "L_Pickup");
		        TryLoadLayerIcon("StereoLeft", "L_Stereo");
		        TryLoadLayerIcon("StereoRight", "L_Stereo");
		        TryLoadLayerIcon("Walkthrough", "L_Walkthrough");
		        TryLoadLayerIcon("MirrorReflection", "L_MirrorReflection");
		        TryLoadLayerIcon("InternalUI", "L_InternalUI");
		        TryLoadLayerIcon("HardwareObjects", "L_HardwareObjects");
		        TryLoadLayerIcon("reserved2", "L_Reserved");
		        TryLoadLayerIcon("reserved4", "L_Reserved");
		        
		        layerIconsLoaded = true;
	        }
	        catch (System.Exception e)
	        {
		        Debug.LogWarning($"[HierarchyPlus] Error loading predefined layer icons: {e.Message}");
	        }
        }
        
        private static void TryLoadLayerIcon(string layerName, string iconName)
        {
	        try
	        {
		        Texture2D icon = Resources.Load<Texture2D>($"Icons/{iconName}");
		        if (icon != null)
		        {
			        predefinedLayerIcons[layerName] = icon;
			        return;
		        }
		        
		        GUIContent customIcon;
		        if (customIconCache.TryGetValue(iconName, out customIcon))
		        {
			        icon = customIcon.image as Texture2D;
			        if (icon != null)
			        {
				        predefinedLayerIcons[layerName] = icon;
				        return;
			        }
		        }

		        string customIconPath = GetCustomIconPath(iconName);
		        if (!string.IsNullOrEmpty(customIconPath))
		        {
			        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(customIconPath);
			        if (icon != null)
			        {
				        predefinedLayerIcons[layerName] = icon;
			        }
		        }
	        }
	        catch (System.Exception e)
	        {
		        Debug.LogWarning($"[HierarchyPlus] Failed to load layer icon {iconName} for layer {layerName}: {e.Message}");
	        }
        }
        
        private static string GetCustomIconPath(string iconName)
        {
	        if (string.IsNullOrWhiteSpace(iconFolderPath) || string.IsNullOrEmpty(iconName))
		        return null;
		        
	        string[] extensions = { ".png", ".jpg", ".jpeg", ".tga", ".tiff" };
	        
	        foreach (string ext in extensions)
	        {
		        string fullPath = Path.Combine(iconFolderPath, iconName + ext).Replace('\\', '/');
		        if (AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath) != null)
		        {
			        return fullPath;
		        }
	        }
	        
	        return null;
        }
        
        #endregion
        
        #region Custom Component Icon Registration
        
        // Adapted from BluHierarchy.
        public static void SetCustomIcon(System.Type componentType, Texture2D icon)
        {
            if (componentType == null) return;
            
            if (icon == null)
            {
                customComponentIcons.Remove(componentType);
            }
            else
            {
                customComponentIcons[componentType] = icon;
            }
        }
        
        // Adapted from BluHierarchy.
        public static Texture2D GetCustomIcon(System.Type componentType)
        {
            if (componentType == null) return null;
            
            customComponentIcons.TryGetValue(componentType, out var icon);
            return icon;
        }
        
        public static void ClearCustomIcons()
        {
            customComponentIcons.Clear();
        }
        
        #endregion

        #region Initialization


        [InitializeOnLoadMethod]
        private static void InitializeGUI()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI = OnHierarchyItemGUI + EditorApplication.hierarchyWindowItemOnGUI;
            EditorApplication.update -= OnCustomUpdate;
            EditorApplication.update += OnCustomUpdate;
            
            EditorApplication.delayCall += OnDelayedInitialization;
            
            AssetDatabase.importPackageCompleted += OnPackageImported;
            AssetDatabase.importPackageFailed += OnPackageImportFailed;
            
            RefreshIconsDelayed = RefreshIconsIfNeeded;
        }
        
        private static void OnDelayedInitialization()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) UpdateHierarchyTitle(window);
            if (!hasInitializedOnLoad)
            {
                hasInitializedOnLoad = true;
                InitializeAll();
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        private static void OnPackageImported(string packageName)
        {
            RefreshIconsIfNeeded();
        }
        
        private static void OnPackageImportFailed(string packageName, string errorMessage)
        {
            RefreshIconsIfNeeded();
        }
        
        private static void RefreshIconsIfNeeded()
        {
            if (EditorApplication.isPlaying && !settings.renderInPlayMode) 
            {
                return;
            }
            
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastRefreshTime > 1.0)
            {
                lastRefreshTime = currentTime;
                InitializeAll();
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        private static EditorWindow lastFocusedWindow;

        private static void UpdateHierarchyTitle(EditorWindow window)
        {
            if (window != null && window.GetType().FullName == "UnityEditor.SceneHierarchyWindow" && window.titleContent.text != "Hierarchy+")
                window.titleContent = new GUIContent(window.titleContent) { text = "Hierarchy+" };
        }

        private static void OnCustomUpdate() 
        { 
            ranOnceThisFrame = false;
            if (lastFocusedWindow != EditorWindow.focusedWindow)
            {
                lastFocusedWindow = EditorWindow.focusedWindow;
                UpdateHierarchyTitle(lastFocusedWindow);
            }
            
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastIconCheckTime > 5.0)
            {
                lastIconCheckTime = currentTime;
                CheckForIconChanges();
            }
        }
        
        private static void CheckForIconChanges()
        {
            if (!string.IsNullOrWhiteSpace(iconFolderPath))
            {
                var currentFiles = AssetDatabase.FindAssets("t:Texture2D", new[] { iconFolderPath });
                if (!customIconAssetGuids.SetEquals(currentFiles))
                {
                    RefreshIconsIfNeeded();
                }
            }
        }

        internal static void InitializeAll()
        {
	        iconCache.Clear();
	        layerIconCache.Clear();
	        ComponentIconHandler.ClearCache(); 
            InitializeIconFolderPath();
            InitializeCustomIcons();
            InitializeSpecialIcons();
            predefinedLayerIcons.Clear();
            layerIconsLoaded = false;
            LoadPredefinedLayerIcons();
            hasInitializedOnLoad = true;
        }

        private static void InitializeIconFolderPath()
        {
            iconFolderPath = string.Empty;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
            if (packageInfo != null)
            {
                var packagePath = packageInfo.assetPath;
                iconFolderPath = Path.Combine(packagePath, PACKAGE_ICON_FOLDER_PATH).Replace('\\', '/');
                
                if (!AssetDatabase.IsValidFolder(iconFolderPath))
                {
                    CustomLog($"Custom Icon folder couldn't be found in {iconFolderPath}. Custom Icons are disabled.");
                    iconFolderPath = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(iconFolderPath))
            {
                foreach (string guid in AssetDatabase.FindAssets("Settings t:MonoScript"))
                {
                    string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                    if (script == null || script.GetClass() != typeof(HierarchyPlusSettingsWindow)) continue;

                    string folder = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
                    while (!string.IsNullOrEmpty(folder) && folder != "Assets" && folder != "Packages")
                    {
                        string candidate = folder + "/" + PACKAGE_ICON_FOLDER_PATH;
                        if (AssetDatabase.IsValidFolder(candidate))
                        {
                            iconFolderPath = candidate;
                            return;
                        }
                        folder = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                    }
                    break;
                }
                CustomLog("Custom Icons folder couldn't be found. Custom icons are disabled.", CustomLogType.Warning);
            }
        }
        
        private static void InitializeSpecialIcons()
        {
	        
	        defaultTextures[0] = EditorGUIUtility.IconContent("cs Script Icon")?.image as Texture2D;
	        defaultTextures[1] = EditorGUIUtility.IconContent("d_cs Script Icon")?.image as Texture2D;
	        defaultTextures[2] = EditorGUIUtility.IconContent("dll Script Icon")?.image as Texture2D;

	        if (!customIconCache.TryGetValue("GameObject", out gameObjectContent))
                gameObjectContent = new GUIContent(AssetPreview.GetMiniTypeThumbnail(typeof(GameObject)));
	        gameObjectContent.tooltip = "GameObject";
            
            if (!customIconCache.TryGetValue(DEFAULT_ICON_NAME, out defaultContent))
                defaultContent = new GUIContent(AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript)));
            
            string missingTooltip = "Missing Script";
            if (customIconCache.TryGetValue(MISSING_SCRIPT_ICON_NAME, out var value))
                missingScriptContent = new GUIContent(value){tooltip = missingTooltip};
            else missingScriptContent = new GUIContent(defaultContent){tooltip = missingTooltip};
        }
        private static void InitializeCustomIcons()
        {            
            customIconCache.Clear();
            customIconAssetGuids.Clear();
            layerIconCache.Clear();
            if (string.IsNullOrWhiteSpace(iconFolderPath)) return;
            
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { iconFolderPath });
            customIconAssetGuids.UnionWith(guids);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (icon != null && !customIconCache.ContainsKey(icon.name))
                {
                    customIconCache[icon.name] = new GUIContent(icon, icon.name);
                    
                    if (icon.name.StartsWith("L_"))
                    {
                        layerIconCache[icon.name] = new GUIContent(icon, icon.name);
                    }
                }
            }
            

        }
        #endregion

        #region Logging
        internal static Color pastelBlueColor = new Color(0.5f, 0.8f, 1);
        internal static Color pastelGreenColor = new Color(0.56f, 0.94f, 0.47f);
        internal static Color pastelRedColor = new Color(1, 0.25f, 0.25f);
        internal static Color pastelYellowColor = new Color(0.99f, 0.95f, 0, 6f);
        internal static Color linkColor = new Color(0.3f, 0.7f, 1);

        internal static bool CustomLog(string message, CustomLogType type = CustomLogType.Regular, bool condition = true)
        {
            if (condition)
            {
                Color finalColor = type == CustomLogType.Regular ? pastelGreenColor : type == CustomLogType.Warning ? pastelYellowColor : pastelRedColor;
                string fullMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(finalColor)}>[{PRODUCT_NAME}]</color> {message.Replace("\\n", "\n")}";
                switch (type)
                {
                    case CustomLogType.Regular:
                        Debug.Log(fullMessage); break;
                    case CustomLogType.Warning:
                        Debug.LogWarning(fullMessage); break;
                    case CustomLogType.Error:
                        Debug.LogError(fullMessage); break;
                }

            }
            return condition;
        }
        internal enum CustomLogType
        {
            Regular,
            Warning,
            Error
        }
        #endregion
    }
}

