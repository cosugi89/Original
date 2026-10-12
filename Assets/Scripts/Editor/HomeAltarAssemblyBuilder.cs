using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Features.Main;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>物体輪郭から生成した祭壇SpriteをMainSceneへ作成時配線する。</summary>
public static class HomeAltarAssemblyBuilder
{
    private const string ManifestPath = "Assets/Resources/Sprite/Home/AltarBuild/Individual/parts.json";
    private const string PropManifestPath = "Assets/Resources/Sprite/Home/AltarBuild/Individual/props.json";

    [Serializable]
    private class Manifest { public Part[] parts; }

    [Serializable]
    private class Part
    {
        public string name, kind, sprite, reference, bodyVisibleSprite;
        public int stage, order, width, height;
        public float pivotX, pivotY, offsetX, offsetY, startOffsetY;
    }

    [MenuItem("Tools/The Red Fever/Rebuild Home Altar Assembly")]
    public static void Rebuild()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Play Modeを停止してから祭壇を配線してください。");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/MainScene.unity")
            throw new InvalidOperationException("MainSceneを開いてから祭壇を配線してください。");

        var parts = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath)).parts
            .Concat(JsonUtility.FromJson<Manifest>(File.ReadAllText(PropManifestPath)).parts)
            .OrderBy(p => p.order).ToArray();
        foreach (var assetPath in parts.SelectMany(p => new[] { p.sprite, p.bodyVisibleSprite })
                     .Where(path => !string.IsNullOrEmpty(path)).Distinct())
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        var director = UnityEngine.Object.FindObjectsByType<HomeEntranceDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        var serializedDirector = new SerializedObject(director);
        var altar = ((GameObject)serializedDirector.FindProperty("homeStaticAltar").objectReferenceValue).GetComponent<Image>();
        var legacyArray = serializedDirector.FindProperty("legacyPropObjects");
        var legacyProps = Enumerable.Range(0, legacyArray.arraySize)
            .Select(i => (GameObject)legacyArray.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        var propReferences = new Dictionary<string, Image>
        {
            { "Lectern", legacyProps.Single(g => g.name == "Button_SettingsLectern").GetComponent<Image>() },
            { "Memorial", legacyProps.Single(g => g.name == "Button_AchievementMemorial").GetComponent<Image>() }
        };
        var root = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RectTransform>(true))
            .Single(r => r.name == "HomeBuildRoot");
        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Rebuild Home Altar Assembly");
        root.SetAsFirstSibling();
        // 小道具配下の葉は親ごと削除し、それ以外のHomeBuildRootの子は残す。
        foreach (var oldPart in root.Cast<Transform>().Select(t => t.GetComponent<HomeBuildPart>())
                     .Where(p => p != null).ToArray())
            Undo.DestroyObjectImmediate(oldPart.gameObject);

        var propRoots = new Dictionary<string, RectTransform>();
        var propBodies = new List<Image>();
        var created = parts.Select(p =>
        {
            var referenceImage = string.IsNullOrEmpty(p.reference) ? altar : propReferences[p.reference];
            var referenceRect = referenceImage.rectTransform;
            var parent = p.kind == "propLeaf" ? propRoots[p.reference] : root;
            var go = new GameObject("Build_" + p.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HomeBuildPart));
            Undo.RegisterCreatedObjectUndo(go, "Create Home Assembly Part");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(p.pivotX, p.pivotY);
            rect.sizeDelta = new Vector2(p.width, p.height);
            rect.SetPositionAndRotation(referenceRect.TransformPoint(new Vector2(p.offsetX, p.offsetY)), referenceRect.rotation);
            rect.localScale = new Vector3(
                referenceRect.lossyScale.x / parent.lossyScale.x,
                referenceRect.lossyScale.y / parent.lossyScale.y,
                referenceRect.lossyScale.z / parent.lossyScale.z);
            var color = referenceImage.color;
            color.a = 1f;
            var image = go.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p.sprite);
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = false;

            Image bodyImage = null;
            Button button = null;
            if (p.kind == "prop")
            {
                propRoots.Add(p.reference, rect);
                var body = new GameObject("Body_Visible", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(body, "Create Home Prop Visible Body");
                var bodyRect = (RectTransform)body.transform;
                bodyRect.SetParent(rect, false);
                bodyRect.anchorMin = Vector2.zero;
                bodyRect.anchorMax = Vector2.one;
                bodyRect.pivot = new Vector2(.5f, .5f);
                bodyRect.anchoredPosition = Vector2.zero;
                bodyRect.sizeDelta = Vector2.zero;
                bodyImage = body.GetComponent<Image>();
                bodyImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p.bodyVisibleSprite);
                bodyImage.color = color;
                bodyImage.raycastTarget = true;
                bodyImage.preserveAspect = false;
                propBodies.Add(bodyImage);
                button = Undo.AddComponent<Button>(go);
                button.targetGraphic = bodyImage;
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.interactable = true;
            }

            var part = go.GetComponent<HomeBuildPart>();
            var serializedPart = new SerializedObject(part);
            serializedPart.FindProperty("targetTransform").objectReferenceValue = rect;
            serializedPart.FindProperty("referenceTransform").objectReferenceValue = referenceRect;
            serializedPart.FindProperty("partImage").objectReferenceValue = image;
            var additionalImages = serializedPart.FindProperty("additionalImages");
            additionalImages.arraySize = p.kind == "prop" ? 1 : 0;
            if (p.kind == "prop") additionalImages.GetArrayElementAtIndex(0).objectReferenceValue = bodyImage;
            serializedPart.FindProperty("finalOffset").vector2Value = new Vector2(p.offsetX, p.offsetY);
            serializedPart.FindProperty("buildStage").intValue = p.stage;
            serializedPart.FindProperty("startOffsetY").floatValue = p.startOffsetY;
            serializedPart.FindProperty("startScale").floatValue = p.kind == "leaf" || p.kind == "propLeaf" ? .94f : 1f;
            serializedPart.FindProperty("overshootScale").floatValue = 1f;
            serializedPart.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            return new { definition = p, part, button };
        }).ToArray();

        // 葉はUnderlayと可視面の間に置き、物体の可視面を最前面に保つ。
        foreach (var bodyImage in propBodies) bodyImage.transform.SetAsLastSibling();
        var stones = created.Where(p => p.definition.kind == "stone")
            .OrderBy(p => p.definition.stage).ThenBy(p => p.definition.order).ToArray();
        var leaves = created.Where(p => p.definition.kind == "leaf").ToArray();
        var props = created.Where(p => p.definition.kind == "prop").ToArray();
        var propLeaves = created.Where(p => p.definition.kind == "propLeaf").ToArray();
        var stoneArray = serializedDirector.FindProperty("buildParts");
        stoneArray.arraySize = stones.Length;
        for (var i = 0; i < stones.Length; i++) stoneArray.GetArrayElementAtIndex(i).objectReferenceValue = stones[i].part;
        var foliageArray = serializedDirector.FindProperty("foliageParts");
        foliageArray.arraySize = leaves.Length;
        for (var i = 0; i < leaves.Length; i++) foliageArray.GetArrayElementAtIndex(i).objectReferenceValue = leaves[i].part;
        var propArray = serializedDirector.FindProperty("propParts");
        propArray.arraySize = props.Length;
        for (var i = 0; i < props.Length; i++) propArray.GetArrayElementAtIndex(i).objectReferenceValue = props[i].part;
        var propFoliageArray = serializedDirector.FindProperty("propFoliageParts");
        propFoliageArray.arraySize = propLeaves.Length;
        for (var i = 0; i < propLeaves.Length; i++) propFoliageArray.GetArrayElementAtIndex(i).objectReferenceValue = propLeaves[i].part;
        var propButtonArray = serializedDirector.FindProperty("propButtons");
        propButtonArray.arraySize = props.Length;
        for (var i = 0; i < props.Length; i++) propButtonArray.GetArrayElementAtIndex(i).objectReferenceValue = props[i].button;

        var homeScreen = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<HomeScreen>(true)).Single();
        var serializedHomeScreen = new SerializedObject(homeScreen);
        serializedHomeScreen.FindProperty("settingsButton").objectReferenceValue = props.Single(p => p.definition.reference == "Lectern").button;
        serializedHomeScreen.FindProperty("achievementsButton").objectReferenceValue = props.Single(p => p.definition.reference == "Memorial").button;
        serializedHomeScreen.ApplyModifiedPropertiesWithoutUndo();

        serializedDirector.FindProperty("partDuration").floatValue = .30f;
        serializedDirector.FindProperty("partStagger").floatValue = .055f;
        serializedDirector.FindProperty("foliageDelay").floatValue = .06f;
        serializedDirector.FindProperty("foliageDuration").floatValue = .22f;
        serializedDirector.FindProperty("foliageStagger").floatValue = .02f;
        serializedDirector.FindProperty("propDuration").floatValue = .34f;
        serializedDirector.FindProperty("propStagger").floatValue = .10f;
        serializedDirector.FindProperty("assemblyHoldDuration").floatValue = .10f;
        serializedDirector.FindProperty("homeHudFadeDuration").floatValue = .20f;
        var additionalHud = serializedDirector.FindProperty("additionalHudCanvasGroups");
        var hudRoots = new[] { "ResourceBar_Group", "MainTabLayout" };
        additionalHud.arraySize = hudRoots.Length;
        for (var i = 0; i < hudRoots.Length; i++)
        {
            var hudRoot = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RectTransform>(true)).Single(r => r.name == hudRoots[i]);
            var group = hudRoot.GetComponent<CanvasGroup>();
            if (group == null) group = Undo.AddComponent<CanvasGroup>(hudRoot.gameObject);
            additionalHud.GetArrayElementAtIndex(i).objectReferenceValue = group;
        }
        serializedDirector.ApplyModifiedPropertiesWithoutUndo();
        altar.gameObject.SetActive(false);
        altar.raycastTarget = false;
        foreach (var legacyProp in legacyProps) legacyProp.SetActive(false);
        root.gameObject.SetActive(true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Assets.Scripts.Editor.TutorialSetupEditor.SetupHomeEntranceHud();
        Debug.Log($"Home祭壇配線完了: 石材{stones.Length} / 祭壇の葉{leaves.Length} / 小道具{props.Length} / 小道具の葉{propLeaves.Length}");
    }
}
