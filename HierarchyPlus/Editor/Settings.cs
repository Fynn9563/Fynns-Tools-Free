using System;
using UnityEditor;
using UnityEngine;
using static HierarchyPlus.SavedSettings;

namespace HierarchyPlus
{
    public class HierarchyPlusSettingsWindow : EditorWindow
    {
        [MenuItem("Tools/Fynn's Tools/Hierarchy+/Settings", false, 95)]
        public static void ShowWindow()
        {
            var window = GetWindow<HierarchyPlusSettingsWindow>("Hierarchy+ Settings");
            window.minSize = new Vector2(400, 500);
        }

        private Vector2 scrollPosition = Vector2.zero;

        private bool colorsFoldout = true;
        private bool iconsFoldout = true;
        private bool labelsFoldout = true;
        private bool mainColorsFoldout = true;
        private bool rowShadingFoldout = true;
        private bool customColorsFoldout = false;
        private bool hiddenComponentsFoldout = false;

        private static readonly Color pastelGreenColor = new Color(0.5f, 1f, 0.5f);

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            DrawHeader();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                settings.enabled.DrawField("HierarchyPlus Enabled");
            }

            if (settings.enabled)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Show in Play Mode", EditorStyles.boldLabel);
                    settings.renderInPlayMode.DrawField("Icons");
                    settings.renderColorsInPlayMode.DrawField("Colors and Lines");
                    settings.renderLabelsInPlayMode.DrawField("Labels");
                    settings.renderCollectionsInPlayMode.DrawField("Collection Badges");
                }
            }

            GUILayout.Space(8);

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                colorsFoldout = DrawFoldoutTitle("Colors", colorsFoldout, settings.colorsEnabled);

                if (colorsFoldout)
                {
                    using (new EditorGUI.DisabledScope(!settings.GetColorsEnabled()))
                    {
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            mainColorsFoldout = EditorGUILayout.Foldout(mainColorsFoldout, "Icon Colors");
                            if (mainColorsFoldout)
                            {
                                EditorGUI.indentLevel++;
                                DrawColorSetting("Active Icon Tint", settings.iconTintColor);
                                DrawColorSetting("Inactive Icon Tint", settings.iconFadedTintColor);
                                DrawColorSetting("Guide Lines", settings.guideLinesColor, settings.guideLinesEnabled);
                                DrawColorSetting("Icon Background", settings.iconBackgroundColor, settings.iconBackgroundColorEnabled);

                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(EditorGUIUtility.labelWidth);
                                    settings.iconBackgroundOverlapOnly.DrawToggle(
                                        settings.iconBackgroundOverlapOnly ? "On Overlap Only" : "Always",
                                        null,
                                        EditorStyles.miniButton,
                                        pastelGreenColor,
                                        Color.gray,
                                        GUILayout.Width(120)
                                    );
                                }
                                EditorGUI.indentLevel--;
                            }
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            rowShadingFoldout = EditorGUILayout.Foldout(rowShadingFoldout, "Row Shading");
                            if (rowShadingFoldout)
                            {
                                EditorGUI.indentLevel++;
                                DrawColorSetting("Odd Row Color", settings.rowOddColor, settings.rowColoringOddEnabled);
                                DrawColorSetting("Even Row Color", settings.rowEvenColor, settings.rowColoringEvenEnabled);
                                EditorGUI.indentLevel--;
                            }
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            customColorsFoldout = EditorGUILayout.Foldout(customColorsFoldout, "Custom Object Colors");
                            if (customColorsFoldout)
                            {
                                EditorGUI.indentLevel++;
                                DrawColorSetting("Color One", settings.colorOne, settings.colorOneEnabled);
                                DrawColorSetting("Color Two", settings.colorTwo, settings.colorTwoEnabled);
                                DrawColorSetting("Color Three", settings.colorThree, settings.colorThreeEnabled);
                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                }
            }

            GUILayout.Space(5);

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                iconsFoldout = DrawFoldoutTitle("Icons", iconsFoldout, settings.iconsEnabled);

                if (iconsFoldout)
                {
                    using (new EditorGUI.DisabledScope(!settings.GetIconsEnabled()))
                    {
                        EditorGUIUtility.labelWidth = 200;

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Displayed Icons", EditorStyles.boldLabel);
                            settings.showGameObjectIcon.DrawField("Show GameObject Icon");

                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("GameObject Toggle Style", GUILayout.Width(200));
                                string[] toggleOptions = {"Cube Icon", "Tick Box", "Assigned Icon"};
                                int currentStyle = Mathf.RoundToInt(settings.gameObjectToggleStyle.value);
                                int newStyle = EditorGUILayout.Popup(currentStyle, toggleOptions);
                                if (newStyle != currentStyle)
                                    settings.gameObjectToggleStyle.value = (float)newStyle;
                            }

                            settings.showLayerIcon.DrawField("Show Layer Icon");
                            settings.showCollectionsInNormalMode.DrawField("Show Collection Icons in Normal Mode");
                            settings.showTransformIcon.DrawField("Show Transform Icon");
                            settings.showNonBehaviourIcons.DrawField("Show Non-Toggleable Icons");
                            settings.showHiddenComponents.DrawField("Show Hidden Components");
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Interaction", EditorStyles.boldLabel);
                            settings.enableContextClick.DrawField("Enable Context Click");
                            settings.enableDragToggle.DrawField("Enable Drag-Toggle");
                            using (new EditorGUI.DisabledScope(!settings.enableDragToggle))
                                settings.requireAltForDragToggle.DrawField("Require Alt for Drag-Toggle");
                            settings.linkCursorOnHover.DrawField("Link Cursor On Hover");
                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
                            settings.alwaysShowIcons.DrawField("Always Render Icons");
                            settings.showTooltips.DrawField("Show Tooltips");
                        }

                        EditorGUIUtility.labelWidth = 0;

                        GUILayout.Space(10);

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            hiddenComponentsFoldout = EditorGUILayout.Foldout(hiddenComponentsFoldout, "Hidden Components");
                            if (hiddenComponentsFoldout)
                            {
                                EditorGUILayout.HelpBox("Component types that should not show icons in the hierarchy.", MessageType.Info);

                                if (settings.hiddenIconTypes != null)
                                {
                                    for (int i = 0; i < settings.hiddenIconTypes.Length; i++)
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            settings.SetHiddenTypeName(i, EditorGUILayout.TextField($"Type {i + 1}", settings.hiddenIconTypes[i].value));
                                            if (GUILayout.Button("X", GUILayout.Width(25)))
                                            {
                                                var newArray = new SavedString[settings.hiddenIconTypes.Length - 1];
                                                for (int j = 0, k = 0; j < settings.hiddenIconTypes.Length; j++)
                                                {
                                                    if (j != i) newArray[k++] = settings.hiddenIconTypes[j];
                                                }
                                                settings.hiddenIconTypes = newArray;
                                                settings.OnHiddenTypesChanged();
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (GUILayout.Button("Add Hidden Type"))
                                {
                                    var newArray = new SavedString[settings.hiddenIconTypes?.Length + 1 ?? 1];
                                    if (settings.hiddenIconTypes != null)
                                    {
                                        for (int i = 0; i < settings.hiddenIconTypes.Length; i++)
                                        {
                                            newArray[i] = settings.hiddenIconTypes[i];
                                        }
                                    }
                                    newArray[newArray.Length - 1] = new SavedString("");
                                    settings.hiddenIconTypes = newArray;
                                    settings.OnHiddenTypesChanged();
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.Space(5);

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                labelsFoldout = DrawFoldoutTitle("Labels", labelsFoldout, settings.labelsEnabled);

                if (labelsFoldout)
                {
                    using (new EditorGUI.DisabledScope(!settings.GetLabelsEnabled()))
                    {
                        EditorGUIUtility.labelWidth = 200;

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            settings.tagLabelEnabled.DrawField("Show Tag Labels");
                            if (settings.tagLabelEnabled)
                            {
                                EditorGUI.indentLevel++;
                                settings.displayUntaggedLabel.DrawField("Show 'Untagged' Label");
                                settings.tagLabelWidth.value = EditorGUILayout.FloatField("Tag Label Width", settings.tagLabelWidth);
                                EditorGUI.indentLevel--;
                            }

                        }

                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            settings.layerLabelEnabled.DrawField("Show Layer Labels");
                            settings.showLayerIcon.DrawField("Show Layer Icons");
                            if (settings.layerLabelEnabled)
                            {
                                EditorGUI.indentLevel++;
                                settings.displayDefaultLayerLabel.DrawField("Show 'Default' Layer Label");
                                settings.displayLayerIndex.DrawField("Show Layer Index");
                                EditorGUI.indentLevel--;
                            }

                        }

                        settings.enableLabelContextClick.DrawField("Enable Label Context Click");

                        EditorGUIUtility.labelWidth = 0;
                    }
                }
            }

            GUILayout.Space(8);

            EditorGUILayout.EndScrollView();
            DrawFooter();
            EditorGUIUtility.labelWidth = previousLabelWidth;

            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private void DrawHeader()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };
                GUILayout.Label("Hierarchy+", titleStyle, GUILayout.Height(28));
                GUILayout.Label("Fynn's Tools", EditorStyles.miniLabel);
            }
            GUILayout.Space(4);
        }

        private void DrawFooter()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load Default Settings"))
                {
                    if (EditorUtility.DisplayDialog("Load Default Settings", "Are you sure?", "Load", "Cancel"))
                        SavedSettings.Clear();
                }
                if (GUILayout.Button(new GUIContent("Refresh Icons", "Reload hierarchy icons")))
                    HierarchyPlus.InitializeAll();
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), EditorStyles.linkLabel))
                    Application.OpenURL("https://github.com/Fynn9563");
            }
        }

        private bool DrawFoldoutTitle(string title, bool foldout, SavedBool enableToggle)
        {
            using (new GUILayout.HorizontalScope())
            {
                foldout = EditorGUILayout.Foldout(foldout, title, true);
                GUILayout.FlexibleSpace();

                if (settings.enabled)
                {
                    enableToggle.DrawToggle(
                        enableToggle ? "Enabled" : "Disabled",
                        null,
                        EditorStyles.miniButton,
                        pastelGreenColor,
                        Color.gray,
                        GUILayout.Width(60)
                    );
                }
            }
            return foldout;
        }

        private void DrawColorSetting(string label, SavedColor color, SavedBool enabled = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (enabled != null)
                {
                    enabled.value = EditorGUILayout.Toggle(label, enabled.value, GUILayout.Width(EditorGUIUtility.labelWidth + 14));
                    using (new EditorGUI.DisabledScope(!enabled.value))
                    {
                        color.color = EditorGUILayout.ColorField(GUIContent.none, color.color, false, true, false, GUILayout.ExpandWidth(true));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth + 14));
                    color.color = EditorGUILayout.ColorField(GUIContent.none, color.color, false, true, false, GUILayout.ExpandWidth(true));
                }
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
