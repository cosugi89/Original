using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Core;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// Custom inspector for PartsManager that provides auto-setup, sprite loading,
    /// theme selection, color mapping, and preview functionality in the Unity Editor.
    /// </summary>
    [CustomEditor(typeof(PartsManager))]
    public class PartsManagerEditor : UnityEditor.Editor
    {
        private const string DEFAULT_SPRITE_FOLDER = "Assets/Layer Lab/2D Art Maker Unity/AMMinimalGame Character/Sprites/Character";
        private const string PREFS_KEY_SPRITE_FOLDER = "PartsManagerEditor_SpriteFolderPath";

        private SerializedProperty categoriesProp;
        private SerializedProperty animatorProp;
        private SerializedProperty skinRenderersProp;
        private SerializedProperty hairRenderersProp;
        private SerializedProperty eyeRenderersProp;
        private SerializedProperty beardRenderersProp;
        private SerializedProperty selectedThemesProp;

        private Dictionary<int, bool> foldoutStates = new();
        private bool categoriesFoldout = true;
        private bool colorTargetsFoldout = true;
        private string spriteFolderPath = DEFAULT_SPRITE_FOLDER;
        private int previewCategoryIndex = -1;
        private int previewSpriteIndex;

        private static readonly Dictionary<PartsType, string> RendererNameMap = new()
        {
            { PartsType.Eye, "Eye" },
            { PartsType.Hair, "Hair" },
            { PartsType.Helmet, "Helmet" },
            { PartsType.Beard, "Beard" },
            { PartsType.Chest, "Chest" },
            { PartsType.Sword, "Sword" },
            { PartsType.Axe, "Axe" },
            { PartsType.Bow, "Bow" },
            { PartsType.Shield, "Shield" },
            { PartsType.Wand, "Wand" },
            { PartsType.Staff, "Staff" },
            { PartsType.Spear, "Spear" },
            { PartsType.Blunt, "Blunt" },
            { PartsType.Crossbow, "Crossbow" },
            { PartsType.SubItem, "Sub_Item" },
            { PartsType.Arrow, "Arrow" },
            { PartsType.HelmetHair, "Hair_Helmet" },
            { PartsType.Skin, "Body" },
        };

        private static readonly Dictionary<PartsType, string> CommonFolderMap = new()
        {
            { PartsType.Eye, "Eye" },
            { PartsType.Hair, "Hair" },
            { PartsType.Beard, "Beard" },
            { PartsType.HelmetHair, "Helmet_Hair" },
            { PartsType.Skin, "Skin" },
        };

        private static readonly Dictionary<string, PartsType> ThemeFolderMap = new()
        {
            { "Chest", PartsType.Chest },
            { "Helmet", PartsType.Helmet },
            { "Shield", PartsType.Shield },
            { "Sub_Item", PartsType.SubItem },
            { "Sword", PartsType.Sword },
            { "Axe", PartsType.Axe },
            { "Bow", PartsType.Bow },
            { "Wand", PartsType.Wand },
            { "Staff", PartsType.Staff },
            { "Spear", PartsType.Spear },
            { "Blunt", PartsType.Blunt },
            { "Crossbow", PartsType.Crossbow },
            { "Arrow", PartsType.Arrow },
        };

        private static readonly HashSet<PartsType> CommonPartsTypes = new()
        {
            PartsType.Eye,
            PartsType.Hair,
            PartsType.Beard,
            PartsType.HelmetHair,
            PartsType.Skin,
        };

        private void OnEnable()
        {
            categoriesProp = serializedObject.FindProperty("categories");
            animatorProp = serializedObject.FindProperty("animator");
            skinRenderersProp = serializedObject.FindProperty("skinRenderers");
            hairRenderersProp = serializedObject.FindProperty("hairRenderers");
            eyeRenderersProp = serializedObject.FindProperty("eyeRenderers");
            beardRenderersProp = serializedObject.FindProperty("beardRenderers");
            selectedThemesProp = serializedObject.FindProperty("selectedThemes");

            spriteFolderPath = EditorPrefs.GetString(PREFS_KEY_SPRITE_FOLDER, DEFAULT_SPRITE_FOLDER);
        }

        private void SetSpriteFolderPath(string path)
        {
            spriteFolderPath = path;
            EditorPrefs.SetString(PREFS_KEY_SPRITE_FOLDER, path);
        }

        /// <summary>
        /// Draws the custom inspector GUI for PartsManager.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAnimatorField();
            EditorGUILayout.Space(4);
            DrawAutoSetupSection();
            EditorGUILayout.Space(4);
            DrawColorTargetSection();
            EditorGUILayout.Space(8);
            DrawCategoriesSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimatorField()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Animator", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(animatorProp);
            EditorGUILayout.EndVertical();
        }

        private void DrawAutoSetupSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Auto Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Maps renderers, loads sprites and thumbnails in one step.\n" +
                "Renderers are matched by child object name, sprites from the Base Folder.",
                MessageType.Info);

            // Object field for drag & drop folder assignment
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Base Folder");
            var folderAsset = AssetDatabase.IsValidFolder(spriteFolderPath)
                ? AssetDatabase.LoadAssetAtPath<DefaultAsset>(spriteFolderPath)
                : null;
            EditorGUI.BeginChangeCheck();
            var newFolder = EditorGUILayout.ObjectField(folderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck() && newFolder != null)
            {
                string path = AssetDatabase.GetAssetPath(newFolder);
                if (AssetDatabase.IsValidFolder(path))
                    SetSpriteFolderPath(path);
            }
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string absoluteStart = Path.GetFullPath(spriteFolderPath);
                if (!Directory.Exists(absoluteStart))
                    absoluteStart = Path.GetFullPath("Assets");
                string selected = EditorUtility.OpenFolderPanel("Select Sprite Folder", absoluteStart, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    int assetsIndex = selected.IndexOf("Assets", StringComparison.Ordinal);
                    if (assetsIndex >= 0)
                        SetSpriteFolderPath(selected.Substring(assetsIndex));
                }
                GUIUtility.ExitGUI();
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            string editedPath = EditorGUILayout.TextField("Path", spriteFolderPath);
            if (EditorGUI.EndChangeCheck())
                SetSpriteFolderPath(editedPath);

            // Fantasy is always selected by default
            if (selectedThemesProp.arraySize == 0)
                AddTheme(ThemeType.Fantasy);

            if (GUILayout.Button("Auto Setup", GUILayout.Height(28)))
            {
                AutoSetupRenderers();
                LoadSpritesFromFolder();
                LoadThumbnails();
            }

            EditorGUILayout.EndVertical();
        }

        private bool IsThemeSelected(ThemeType theme)
        {
            for (int i = 0; i < selectedThemesProp.arraySize; i++)
            {
                if (selectedThemesProp.GetArrayElementAtIndex(i).enumValueIndex == (int)theme)
                    return true;
            }
            return false;
        }

        private void AddTheme(ThemeType theme)
        {
            if (IsThemeSelected(theme)) return;
            selectedThemesProp.InsertArrayElementAtIndex(selectedThemesProp.arraySize);
            selectedThemesProp.GetArrayElementAtIndex(selectedThemesProp.arraySize - 1).enumValueIndex = (int)theme;
        }

        private void RemoveTheme(ThemeType theme)
        {
            for (int i = selectedThemesProp.arraySize - 1; i >= 0; i--)
            {
                if (selectedThemesProp.GetArrayElementAtIndex(i).enumValueIndex == (int)theme)
                {
                    selectedThemesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
        }

        private void DrawColorTargetSection()
        {
            EditorGUILayout.BeginVertical("box");

            colorTargetsFoldout = EditorGUILayout.Foldout(colorTargetsFoldout, "Color Targets", true, EditorStyles.foldoutHeader);

            if (colorTargetsFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(skinRenderersProp, true);
                EditorGUILayout.PropertyField(hairRenderersProp, true);
                EditorGUILayout.PropertyField(eyeRenderersProp, true);
                EditorGUILayout.PropertyField(beardRenderersProp, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCategoriesSection()
        {
            EditorGUILayout.BeginVertical("box");

            categoriesFoldout = EditorGUILayout.Foldout(categoriesFoldout, "Parts Categories", true, EditorStyles.foldoutHeader);

            if (!categoriesFoldout)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(2);

            if (categoriesProp == null || !categoriesProp.isArray)
            {
                EditorGUILayout.HelpBox("Cannot find the 'categories' field.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                DrawCategoryElement(i);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Category"))
            {
                categoriesProp.InsertArrayElementAtIndex(categoriesProp.arraySize);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawCategoryElement(int index)
        {
            var element = categoriesProp.GetArrayElementAtIndex(index);
            var typeProp = element.FindPropertyRelative("type");
            var displayNameProp = element.FindPropertyRelative("displayName");
            var uiCategoryProp = element.FindPropertyRelative("uiCategory");
            var renderersProp = element.FindPropertyRelative("renderers");
            var canChangeColorProp = element.FindPropertyRelative("canChangeColor");
            var colorTargetProp = element.FindPropertyRelative("colorTarget");

            if (!foldoutStates.ContainsKey(index))
                foldoutStates[index] = false;

            string typeName = typeProp != null ? ((PartsType)typeProp.enumValueIndex).ToString() : "Unknown";
            int spriteCount = GetSpriteCount(renderersProp);
            int rendererCount = renderersProp != null ? renderersProp.arraySize : 0;
            var thumbnailsProp = element.FindPropertyRelative("thumbnails");
            int thumbnailCount = thumbnailsProp != null ? thumbnailsProp.arraySize : 0;
            bool isReady = rendererCount > 0 && (spriteCount > 0 || thumbnailCount > 0);
            string statusIcon = isReady ? "[OK]" : "[--]";

            string info = spriteCount > 0
                ? $"Renderers: {rendererCount}, Sprites: {spriteCount}"
                : $"Renderers: {rendererCount}, Thumbnails: {thumbnailCount}";

            EditorGUILayout.BeginVertical("helpbox");

            EditorGUILayout.BeginHorizontal();
            foldoutStates[index] = EditorGUILayout.Foldout(foldoutStates[index],
                $"{statusIcon} {typeName} - {info}", true);

            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                categoriesProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (foldoutStates[index])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(typeProp);
                EditorGUILayout.PropertyField(displayNameProp);
                EditorGUILayout.PropertyField(uiCategoryProp);
                EditorGUILayout.PropertyField(canChangeColorProp);
                if (canChangeColorProp != null && canChangeColorProp.boolValue)
                    EditorGUILayout.PropertyField(colorTargetProp);
                EditorGUILayout.PropertyField(renderersProp, true);

                if (spriteCount > 0)
                {
                    EditorGUILayout.Space(2);
                    DrawPreviewSection(index, renderersProp);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSection(int catIndex, SerializedProperty renderersProp)
        {
            if (renderersProp == null || renderersProp.arraySize == 0) return;

            var firstRenderer = renderersProp.GetArrayElementAtIndex(0);
            var spritesProp = firstRenderer.FindPropertyRelative("sprites");
            if (spritesProp == null || spritesProp.arraySize == 0) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", GUILayout.Width(50));

            bool isActive = previewCategoryIndex == catIndex;
            int currentIdx = isActive ? previewSpriteIndex : 0;

            EditorGUI.BeginChangeCheck();
            int newIdx = EditorGUILayout.IntSlider(currentIdx, 0, spritesProp.arraySize - 1);
            if (EditorGUI.EndChangeCheck())
            {
                previewCategoryIndex = catIndex;
                previewSpriteIndex = newIdx;
                ApplyPreviewInEditor(renderersProp, newIdx);
            }

            if (GUILayout.Button("Apply", GUILayout.Width(50)))
            {
                previewCategoryIndex = catIndex;
                previewSpriteIndex = newIdx;
                ApplyPreviewInEditor(renderersProp, newIdx);
            }

            EditorGUILayout.EndHorizontal();

            if (isActive && previewSpriteIndex >= 0 && previewSpriteIndex < spritesProp.arraySize)
            {
                var spriteProp = spritesProp.GetArrayElementAtIndex(previewSpriteIndex);
                Sprite sprite = spriteProp.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    Rect previewRect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawPreviewTexture(previewRect, AssetPreview.GetAssetPreview(sprite) ?? sprite.texture);
                }
            }
        }

        private void ApplyPreviewInEditor(SerializedProperty renderersProp, int spriteIndex)
        {
            for (int r = 0; r < renderersProp.arraySize; r++)
            {
                var rendererElement = renderersProp.GetArrayElementAtIndex(r);
                var srProp = rendererElement.FindPropertyRelative("renderer");
                var spritesProp = rendererElement.FindPropertyRelative("sprites");

                if (srProp == null || spritesProp == null) continue;

                SpriteRenderer sr = srProp.objectReferenceValue as SpriteRenderer;
                if (sr == null) continue;
                if (spriteIndex < 0 || spriteIndex >= spritesProp.arraySize) continue;

                Sprite sprite = spritesProp.GetArrayElementAtIndex(spriteIndex).objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    Undo.RecordObject(sr, "Preview Parts");
                    sr.sprite = sprite;
                    EditorUtility.SetDirty(sr);
                }
            }

            SceneView.RepaintAll();
        }

        private int GetSpriteCount(SerializedProperty renderersProp)
        {
            if (renderersProp == null || renderersProp.arraySize == 0) return 0;

            var firstRenderer = renderersProp.GetArrayElementAtIndex(0);
            var spritesProp = firstRenderer.FindPropertyRelative("sprites");
            return spritesProp != null ? spritesProp.arraySize : 0;
        }

        private void AutoSetupRenderers()
        {
            PartsManager pm = (PartsManager)target;
            Transform root = pm.transform;
            Undo.RecordObject(pm, "Auto Setup Renderers");

            var allRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            var partsTypes = Enum.GetValues(typeof(PartsType)).Cast<PartsType>().ToArray();

            categoriesProp.arraySize = partsTypes.Length;

            for (int i = 0; i < partsTypes.Length; i++)
            {
                PartsType pType = partsTypes[i];
                var element = categoriesProp.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("type").enumValueIndex = (int)pType;
                element.FindPropertyRelative("displayName").stringValue = pType.ToString();
                element.FindPropertyRelative("uiCategory").enumValueIndex = (int)GetUICategory(pType);
                element.FindPropertyRelative("exclusiveGroup").enumValueIndex = (int)GetExclusiveGroup(pType);
                element.FindPropertyRelative("defaultVisible").boolValue = GetDefaultVisible(pType);
                element.FindPropertyRelative("isCommon").boolValue = CommonPartsTypes.Contains(pType);

                bool hasColor = pType == PartsType.Hair || pType == PartsType.Beard
                    || pType == PartsType.HelmetHair || pType == PartsType.Skin;
                element.FindPropertyRelative("canChangeColor").boolValue = hasColor;

                if (hasColor)
                {
                    ColorTargetType ct = pType switch
                    {
                        PartsType.Eye => ColorTargetType.Eye,
                        PartsType.Beard => ColorTargetType.Beard,
                        PartsType.Skin => ColorTargetType.Skin,
                        _ => ColorTargetType.Hair
                    };
                    element.FindPropertyRelative("colorTarget").enumValueIndex = (int)ct;
                }

                if (!RendererNameMap.TryGetValue(pType, out string searchName)) continue;

                var matchedRenderers = new List<SpriteRenderer>();
                foreach (var sr in allRenderers)
                {
                    if (MatchRendererName(sr.gameObject.name, searchName, pType))
                        matchedRenderers.Add(sr);
                }

                var rendProp = element.FindPropertyRelative("renderers");

                if (matchedRenderers.Count > 0)
                {
                    rendProp.arraySize = matchedRenderers.Count;
                    for (int r = 0; r < matchedRenderers.Count; r++)
                    {
                        var rendElement = rendProp.GetArrayElementAtIndex(r);
                        rendElement.FindPropertyRelative("renderer").objectReferenceValue = matchedRenderers[r];

                        var spritesProp = rendElement.FindPropertyRelative("sprites");
                        spritesProp.arraySize = 0;
                    }
                }
                else
                {
                    rendProp.arraySize = 0;
                }
            }

            // Auto-map Animator
            if (animatorProp.objectReferenceValue == null)
            {
                Animator anim = root.GetComponent<Animator>();
                if (anim == null)
                    anim = root.GetComponentInChildren<Animator>(true);
                if (anim != null)
                    animatorProp.objectReferenceValue = anim;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
            AutoMapColors();
        }

        private UICategory GetUICategory(PartsType pType)
        {
            return pType switch
            {
                PartsType.Hair => UICategory.Hair,
                PartsType.Eye => UICategory.Eye,
                PartsType.Beard => UICategory.Beard,
                PartsType.Skin => UICategory.Skin,
                PartsType.Helmet => UICategory.Helmet,
                PartsType.Chest => UICategory.Chest,

                PartsType.Sword => UICategory.HandRight,
                PartsType.Axe => UICategory.HandRight,
                PartsType.Bow => UICategory.HandRight,
                PartsType.Wand => UICategory.HandRight,
                PartsType.Staff => UICategory.HandRight,
                PartsType.Spear => UICategory.HandRight,
                PartsType.Blunt => UICategory.HandRight,
                PartsType.Crossbow => UICategory.HandRight,

                PartsType.Shield => UICategory.HandLeft,
                PartsType.SubItem => UICategory.HandLeft,

                // ������ UICategoryConfig �ɓ����Ă��Ȃ��⏕�p�[�c�́A
                // ��������e�ɋ߂��J�e�S���֊񂹂�
                PartsType.HelmetHair => UICategory.Hair,
                PartsType.Arrow => UICategory.HandLeft,

                _ => UICategory.Hair
            };
        }

        private PartsExclusiveGroup GetExclusiveGroup(PartsType pType)
        {
            return pType switch
            {
                PartsType.Sword => PartsExclusiveGroup.HandRight,
                PartsType.Axe => PartsExclusiveGroup.HandRight,
                PartsType.Bow => PartsExclusiveGroup.HandRight,
                PartsType.Wand => PartsExclusiveGroup.HandRight,
                PartsType.Staff => PartsExclusiveGroup.HandRight,
                PartsType.Spear => PartsExclusiveGroup.HandRight,
                PartsType.Blunt => PartsExclusiveGroup.HandRight,
                PartsType.Crossbow => PartsExclusiveGroup.HandRight,

                PartsType.Shield => PartsExclusiveGroup.HandLeft,
                PartsType.SubItem => PartsExclusiveGroup.HandLeft,

                _ => PartsExclusiveGroup.None
            };
        }

        private bool GetDefaultVisible(PartsType pType)
        {
            return pType switch
            {
                PartsType.Arrow => false,
                PartsType.HelmetHair => false,
                _ => true
            };
        }

        private bool MatchRendererName(string objName, string searchName, PartsType pType)
        {
            if (objName == searchName) return true;

            if (pType == PartsType.Bow)
                return objName == "Bow_Line_Up" || objName == "Bow_Line_Down";

            if (pType == PartsType.Crossbow)
                return objName == "Crossbow_Down" || objName == "Crossbow_Line_Up" || objName == "Crossbow_Line_Down";

            if (pType == PartsType.Arrow)
                return objName == "Bolt";

            if (pType == PartsType.Skin)
                return objName == "Body" || objName == "Head";

            return false;
        }

        private void AutoMapColors()
        {
            PartsManager pm = (PartsManager)target;
            Transform root = pm.transform;
            Undo.RecordObject(pm, "Auto Map Colors");

            var allRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            var skinList = new List<SpriteRenderer>();
            var hairList = new List<SpriteRenderer>();
            var eyeList = new List<SpriteRenderer>();
            var beardList = new List<SpriteRenderer>();

            foreach (var sr in allRenderers)
            {
                string name = sr.gameObject.name;

                if (name == "Body" || name == "Head")
                    skinList.Add(sr);
                else if (name == "Hair" || name == "Hair_Helmet")
                    hairList.Add(sr);
                else if (name == "Eye")
                    eyeList.Add(sr);
                else if (name == "Beard")
                    beardList.Add(sr);
            }

            SetRendererArray(skinRenderersProp, skinList);
            SetRendererArray(hairRenderersProp, hairList);
            SetRendererArray(eyeRenderersProp, eyeList);
            SetRendererArray(beardRenderersProp, beardList);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
        }

        private void SetRendererArray(SerializedProperty arrayProp, List<SpriteRenderer> renderers)
        {
            arrayProp.arraySize = renderers.Count;
            for (int i = 0; i < renderers.Count; i++)
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
        }

        private void LoadSpritesFromFolder()
        {
            if (!AssetDatabase.IsValidFolder(spriteFolderPath))
            {
                EditorUtility.DisplayDialog("Error", $"Folder not found:\n{spriteFolderPath}", "OK");
                return;
            }

            PartsManager pm = (PartsManager)target;
            Undo.RecordObject(pm, "Load Sprites");

            // 1. Load Common sprites (Eye, Hair, Beard)
            string commonPath = $"{spriteFolderPath}/Common";
            if (AssetDatabase.IsValidFolder(commonPath))
            {
                string[] subFolders = AssetDatabase.GetSubFolders(commonPath);
                foreach (string subFolder in subFolders)
                {
                    string folderName = Path.GetFileName(subFolder);
                    PartsType? matchedType = null;

                    foreach (var kvp in CommonFolderMap)
                    {
                        if (string.Equals(folderName, kvp.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedType = kvp.Key;
                            break;
                        }
                    }

                    if (!matchedType.HasValue) continue;

                    Sprite[] sprites = LoadSpritesFromPath(subFolder);
                    if (sprites.Length == 0) continue;

                    SetSpritesToCategory(matchedType.Value, sprites);
                }
            }

            // 2. Load Theme sprites (selected themes only)
            string themePath = $"{spriteFolderPath}/Theme";
            if (AssetDatabase.IsValidFolder(themePath))
            {
                var selectedThemes = new List<ThemeType>();
                for (int i = 0; i < selectedThemesProp.arraySize; i++)
                    selectedThemes.Add((ThemeType)selectedThemesProp.GetArrayElementAtIndex(i).enumValueIndex);

                var spritesByType = new Dictionary<PartsType, List<Sprite>>();

                foreach (var theme in selectedThemes)
                {
                    string themeFolder = $"{themePath}/{theme}";
                    if (!AssetDatabase.IsValidFolder(themeFolder)) continue;

                    ScanThemeFolder(themeFolder, spritesByType);
                }

                // Apply to non-common categories
                for (int i = 0; i < categoriesProp.arraySize; i++)
                {
                    var catElement = categoriesProp.GetArrayElementAtIndex(i);
                    var typeProp = catElement.FindPropertyRelative("type");
                    var isCommonProp = catElement.FindPropertyRelative("isCommon");

                    if (typeProp == null) continue;
                    if (isCommonProp != null && isCommonProp.boolValue) continue;

                    PartsType pType = (PartsType)typeProp.enumValueIndex;

                    var rendProp = catElement.FindPropertyRelative("renderers");
                    if (rendProp == null || rendProp.arraySize == 0) continue;

                    Sprite[] allSprites = spritesByType.TryGetValue(pType, out var list)
                        ? list.ToArray()
                        : Array.Empty<Sprite>();

                    for (int r = 0; r < rendProp.arraySize; r++)
                    {
                        var rendElement = rendProp.GetArrayElementAtIndex(r);
                        var srProp = rendElement.FindPropertyRelative("renderer");
                        var spritesProp = rendElement.FindPropertyRelative("sprites");

                        string rendererName = "";
                        if (srProp?.objectReferenceValue is SpriteRenderer sr)
                            rendererName = sr.gameObject.name;

                        Sprite[] filtered = FilterSpritesForRenderer(pType, rendererName, allSprites);

                        spritesProp.arraySize = filtered.Length;
                        for (int s = 0; s < filtered.Length; s++)
                            spritesProp.GetArrayElementAtIndex(s).objectReferenceValue = filtered[s];
                    }

                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
        }

        private void SetSpritesToCategory(PartsType type, Sprite[] sprites)
        {
            int catIndex = FindCategoryIndex(type);
            if (catIndex < 0) return;

            var catElement = categoriesProp.GetArrayElementAtIndex(catIndex);
            var rendProp = catElement.FindPropertyRelative("renderers");

            if (rendProp.arraySize == 0) return;

            for (int r = 0; r < rendProp.arraySize; r++)
            {
                var rendElement = rendProp.GetArrayElementAtIndex(r);
                var spritesProp = rendElement.FindPropertyRelative("sprites");

                spritesProp.arraySize = sprites.Length;
                for (int s = 0; s < sprites.Length; s++)
                    spritesProp.GetArrayElementAtIndex(s).objectReferenceValue = sprites[s];
            }
        }

        private void ScanThemeFolder(string folderPath, Dictionary<PartsType, List<Sprite>> spritesByType)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(folderPath);

            foreach (string subFolder in subFolders)
            {
                string folderName = Path.GetFileName(subFolder);

                if (ThemeFolderMap.TryGetValue(folderName, out PartsType partsType))
                {
                    Sprite[] sprites = LoadSpritesFromPath(subFolder);
                    if (sprites.Length > 0)
                    {
                        if (!spritesByType.ContainsKey(partsType))
                            spritesByType[partsType] = new List<Sprite>();

                        spritesByType[partsType].AddRange(sprites);
                    }
                }
                else
                {
                    // Recursively scan intermediate folders (e.g. HandRight, HandLeft)
                    ScanThemeFolder(subFolder, spritesByType);
                }
            }
        }

        private Sprite[] FilterSpritesForRenderer(PartsType type, string rendererName, Sprite[] allSprites)
        {
            if (type != PartsType.Crossbow && type != PartsType.Bow)
                return allSprites;

            var filtered = new List<Sprite>();
            foreach (var sprite in allSprites)
            {
                string name = sprite.name;
                bool isLine = name.Contains("Line");

                if (type == PartsType.Bow)
                {
                    if (rendererName == "Bow_Line_Up")
                    {
                        if (isLine && name.Contains("Up"))
                            filtered.Add(sprite);
                    }
                    else if (rendererName == "Bow_Line_Down")
                    {
                        if (isLine && name.Contains("Down"))
                            filtered.Add(sprite);
                    }
                    else
                    {
                        if (!isLine)
                            filtered.Add(sprite);
                    }
                }
                else if (type == PartsType.Crossbow)
                {
                    if (rendererName == "Crossbow_Line_Up")
                    {
                        if (isLine && name.Contains("Up"))
                            filtered.Add(sprite);
                    }
                    else if (rendererName == "Crossbow_Line_Down")
                    {
                        if (isLine && name.Contains("Down"))
                            filtered.Add(sprite);
                    }
                    else
                    {
                        if (isLine) continue;

                        bool isDown = name.EndsWith("_Down");
                        if (rendererName == "Crossbow_Down" && isDown)
                            filtered.Add(sprite);
                        else if (rendererName != "Crossbow_Down" && !isDown)
                            filtered.Add(sprite);
                    }
                }
            }

            return filtered.ToArray();
        }

        private Sprite[] LoadSpritesFromPath(string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            var sprites = new List<Sprite>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite)
                        sprites.Add(sprite);
                }
            }

            sprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            return sprites.ToArray();
        }

        private void LoadThumbnails()
        {
            string thumbnailPath = $"{spriteFolderPath}/Thumnail";
            if (!AssetDatabase.IsValidFolder(thumbnailPath))
            {
                EditorUtility.DisplayDialog("Error", $"Thumbnail folder not found:\n{thumbnailPath}", "OK");
                return;
            }

            PartsManager pm = (PartsManager)target;
            Undo.RecordObject(pm, "Load Thumbnails");

            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var catElement = categoriesProp.GetArrayElementAtIndex(i);
                var typeProp = catElement.FindPropertyRelative("type");
                if (typeProp == null) continue;

                PartsType pType = (PartsType)typeProp.enumValueIndex;
                if (!RendererNameMap.TryGetValue(pType, out string folderName)) continue;

                // Skin thumbnail folder uses category name, not renderer name
                string thumbFolderName = pType == PartsType.Skin ? "Skin" : folderName;
                string folderPath = $"{thumbnailPath}/{thumbFolderName}";
                if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                Sprite[] sprites = LoadSpritesFromPath(folderPath);
                if (sprites.Length == 0) continue;

                var thumbnailsProp = catElement.FindPropertyRelative("thumbnails");
                thumbnailsProp.arraySize = sprites.Length;
                for (int s = 0; s < sprites.Length; s++)
                    thumbnailsProp.GetArrayElementAtIndex(s).objectReferenceValue = sprites[s];
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
        }

        private int FindCategoryIndex(PartsType type)
        {
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var element = categoriesProp.GetArrayElementAtIndex(i);
                var typeProp = element.FindPropertyRelative("type");
                if (typeProp != null && typeProp.enumValueIndex == (int)type)
                    return i;
            }
            return -1;
        }
    }
}

