#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle;
using Assets.Scripts.Features.Battle.ViewControllers;
using Assets.Scripts.Features.Main;
using Assets.Scripts.Features.Tutorial;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    /// <summary>明示的に実行するチュートリアルのPrefab、マスター、Scene配線ツール。</summary>
    public static class TutorialSetupEditor
    {
        private const string PrefabPath = "Assets/Resources/Prefabs/Tutorial.prefab";
        private const string DataFolder = "Assets/Resources/MasterData/Tutorial";

        [MenuItem("The Red Fever/Tutorial/Setup Foundation")]
        public static void Setup()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Play Modeではセットアップできません。");
            var prefab = SetupPrefab();
            EnsureFolder(DataFolder);
            var database = AssetDatabase.LoadAssetAtPath<TutorialMasterDatabase>(DataFolder + "/TutorialMasterDatabase.asset");
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<TutorialMasterDatabase>();
                AssetDatabase.CreateAsset(database, DataFolder + "/TutorialMasterDatabase.asset");
                var home = CreateExample("Example-HomeMissions", 1001, TutorialSignal.HomeReady, "Main.Home");
                home.Priority = 100;
                home.RequiredScopeKey = "Main";
                home.Steps.Add(new TutorialStep {
                    Message = "Choose Missions to enter a battle.",
                    Highlights = new List<TutorialTargetReference> { Target("Main", "MissionsTab") },
                    InteractionTarget = Target("Main", "MissionsTab"),
                    HandTarget = Target("Main", "MissionsTab"), ShowHand = true,
                    Advance = TutorialAdvance.TargetButton
                });
                var character = CreateExample("Example-Character", 1002, TutorialSignal.TabReady, "Main.Character");
                character.Steps.Add(new TutorialStep { Message = "Change your equipment and skills here.", Advance = TutorialAdvance.ScreenTap });
                var battle = CreateExample("Example-BattlePath", 1003, TutorialSignal.BattleReady, "");
                battle.RequiredScopeKey = "Battle";
                battle.Steps.Add(new TutorialStep {
                    Message = "Trace a path from Start to Goal, then release.",
                    Highlights = new List<TutorialTargetReference> { Target("Battle", "Board") },
                    InteractionTarget = Target("Battle", "Board"),
                    AllowDescendantsAndDrag = true,
                    Advance = TutorialAdvance.Signal,
                    CompletionSignal = TutorialSignal.PathConfirmed
                });
                // 正式なチュートリアル内容は未指定。例はInspectorで有効化して動作確認する。
                foreach (var item in new[] { home, character, battle })
                {
                    item.IsEnabled = false;
                    EditorUtility.SetDirty(item);
                }
                var so = new SerializedObject(database);
                var entries = so.FindProperty("tutorials");
                entries.arraySize = 3;
                entries.GetArrayElementAtIndex(0).objectReferenceValue = home;
                entries.GetArrayElementAtIndex(1).objectReferenceValue = character;
                entries.GetArrayElementAtIndex(2).objectReferenceValue = battle;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var originalActive = SceneManager.GetActiveScene();
            SetupScene("Assets/Scenes/MainScene.unity", database, prefab);
            SetupScene("Assets/Scenes/BattleScene.unity", database, prefab);
            if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var dialog = asset.GetComponents<MonoBehaviour>().FirstOrDefault(component => component != null && IsDialog(component.GetType()));
                if (dialog == null) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var scope = Scope(root, dialog.GetType().Name, TutorialSignal.DialogReady);
                    var bindings = root.GetComponentsInChildren<Button>(true).Select(button =>
                        new TutorialTargetBinding { Key = AnimationUtility.CalculateTransformPath(button.transform, root.transform), Target = button.gameObject }).ToList();
                    SetBindings(scope, bindings);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            SetupHomeConversation();
            SetupMessageDesign();
            Debug.Log("[Tutorial] Prefab、Main/Battleの配線、Dialog Scope、無効状態の設定例3件を作成しました。");
        }

        [MenuItem("The Red Fever/Tutorial/Setup Home Conversation")]
        public static void SetupHomeConversation()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Modeで実行してください。");
            const string materialPath = DataFolder + "/TutorialAlphaBackdrop.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Resources/Shaders/UI/TutorialAlphaBackdrop.shader"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Set(prefab.GetComponentInChildren<TutorialBackdropGraphic>(true), "alphaHoleMaterial", material);
                PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }

            var original = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/MainScene.unity");
            var opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            try
            {
                var main = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MainScene>(true)).Single();
                var home = (GameObject)Get(main, "homeTabScreen");
                var preview = home.transform.Find("PanelContent/PanelTop/PreviewImage").GetComponent<RawImage>();
                var scope = home.GetComponent<TutorialScope>();
                scope.Bind("AngelPreview", preview.gameObject);
                EditorUtility.SetDirty(scope);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
                if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            }

            var conversation = AssetDatabase.LoadAssetAtPath<TutorialMasterData>(DataFolder + "/Home-AngelIntroduction.asset");
            if (conversation == null)
            {
                conversation = ScriptableObject.CreateInstance<TutorialMasterData>();
                conversation.TutorialId = 1101;
                conversation.RepeatForDebug = true;
                conversation.Trigger = TutorialSignal.HomeReady;
                conversation.TriggerKey = "Main.Home";
                conversation.RequiredScopeKey = "Main.Home";
                conversation.Priority = 200;
                conversation.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.IsResurgence, Expected = false });
                foreach (var text in new[] {
                    "You have found a quiet place. Rest here while you can.",
                    "Beyond this refuge, the red-eyed grow restless. You will need to fight to make your way through.",
                    "I have gathered requests in Missions. Choose one when you are ready to venture out.",
                    "Before you go, visit Character and Armory to prepare your equipment and skills. I will guide you through your first battle."
                }) conversation.Steps.Add(new TutorialStep {
                    Speaker = "Angel", Message = text, MessageAtBottom = true,
                    Advance = TutorialAdvance.ScreenTap,
                    AlphaHighlight = Target("Main.Home", "AngelPreview")
                });
                AssetDatabase.CreateAsset(conversation, DataFolder + "/Home-AngelIntroduction.asset");
            }
            var database = AssetDatabase.LoadAssetAtPath<TutorialMasterDatabase>(DataFolder + "/TutorialMasterDatabase.asset");
            if (!database.Tutorials.Contains(conversation))
            {
                var so = new SerializedObject(database);
                var entries = so.FindProperty("tutorials");
                var index = entries.arraySize;
                entries.arraySize++;
                entries.GetArrayElementAtIndex(index).objectReferenceValue = conversation;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            SetupHomeEntranceHud();
        }

        [MenuItem("The Red Fever/Tutorial/Setup Home Entrance HUD")]
        public static void SetupHomeEntranceHud()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Modeで実行してください。");
            var original = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/MainScene.unity");
            var opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            try
            {
                var main = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MainScene>(true)).Single();
                var home = (GameObject)Get(main, "homeTabScreen");
                var panel = home.transform.Find("PanelContent").GetComponent<CanvasGroup>();
                panel.alpha = 1; panel.interactable = true; panel.blocksRaycasts = true;
                var bottom = home.transform.Find("PanelContent/PanelBottom");
                var hud = bottom.GetComponent<CanvasGroup>();
                if (hud == null) hud = bottom.gameObject.AddComponent<CanvasGroup>();
                hud.alpha = 1; hud.interactable = true; hud.blocksRaycasts = true;
                var director = (HomeEntranceDirector)Get(main, "homeEntranceDirector");
                Set(director, "homeHudCanvasGroup", hud);
                EditorUtility.SetDirty(panel); EditorUtility.SetDirty(hud);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
                if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            }
        }

        [MenuItem("The Red Fever/Tutorial/Setup Message Design")]
        public static void SetupMessageDesign()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Modeで実行してください。");
            var sample = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs_Sample/Demo/Scenes/Tutorial.prefab");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                UnityEngine.Object.DestroyImmediate(root.transform.Find("TutorialMessage").gameObject);
                var message = UnityEngine.Object.Instantiate(sample.transform.Find("TutorialMessage").gameObject, root.transform, false);
                message.name = "TutorialMessage";
                // サンプルの人物肖像はデモ用。Homeに常駐するAngelをそのまま使う。
                UnityEngine.Object.DestroyImmediate(message.transform.Find("Character").gameObject);
                message.transform.SetSiblingIndex(1);
                var controller = root.GetComponent<TutorialController>();
                var texts = message.GetComponentsInChildren<TextMeshProUGUI>(true);
                var body = texts.First(text => text.transform.parent.name != "NameBox");
                var speaker = texts.First(text => text.transform.parent.name == "NameBox");
                body.text = ""; speaker.text = "";
                foreach (var graphic in message.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
                Set(controller, "messageRoot", message); Set(controller, "messageText", body); Set(controller, "nameText", speaker);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            foreach (var guid in AssetDatabase.FindAssets("t:TutorialMasterData", new[] { DataFolder }))
            {
                var master = AssetDatabase.LoadAssetAtPath<TutorialMasterData>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var step in master.Steps) if ((int)step.Advance == 0) step.Advance = TutorialAdvance.ScreenTap;
                EditorUtility.SetDirty(master);
            }
            AssetDatabase.SaveAssets();
        }

        private static bool IsDialog(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
                if (current.IsGenericType && current.GetGenericTypeDefinition().Name == "DialogBase`1") return true;
            return false;
        }

        private static TutorialMasterData CreateExample(string name, int id, TutorialSignal trigger, string key)
        {
            var item = ScriptableObject.CreateInstance<TutorialMasterData>();
            item.TutorialId = id; item.Trigger = trigger; item.TriggerKey = key;
            AssetDatabase.CreateAsset(item, DataFolder + "/" + name + ".asset");
            return item;
        }
        private static TutorialTargetReference Target(string scope, string key) => new() { ScopeKey = scope, TargetKey = key };

        private static TutorialController SetupPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var controller = root.GetComponent<TutorialController>();
                var canvas = root.GetComponent<Canvas>();
                if (canvas == null) canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = 31000;
                var scaler = root.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = root.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                var raycaster = root.GetComponent<TutorialInputRaycaster>();
                if (raycaster == null) raycaster = root.AddComponent<TutorialInputRaycaster>();
                var dimmed = root.transform.Find("Dimmed").gameObject;
                var oldImage = dimmed.GetComponent<Image>();
                if (oldImage != null) UnityEngine.Object.DestroyImmediate(oldImage);
                var backdrop = dimmed.GetComponent<TutorialBackdropGraphic>();
                if (backdrop == null) backdrop = dimmed.AddComponent<TutorialBackdropGraphic>();
                backdrop.color = new Color(0.04f, 0.05f, 0.08f, 0.65f);
                backdrop.raycastTarget = true;
                dimmed.transform.SetAsFirstSibling();
                var message = root.transform.Find("TutorialMessage");
                var texts = message.GetComponentsInChildren<TextMeshProUGUI>(true);
                var body = texts.First(text => text.transform.parent.name != "NameBox");
                var speaker = texts.First(text => text.transform.parent.name == "NameBox");
                var hand = (RectTransform)root.transform.Find("TutorialHand_01");
                hand.anchorMin = hand.anchorMax = new Vector2(0.5f, 0.5f);
                hand.SetAsLastSibling();
                foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                    graphic.raycastTarget = graphic == backdrop;
                Set(controller, "backdrop", backdrop); Set(controller, "inputRaycaster", raycaster);
                Set(controller, "tutorialHand", hand); Set(controller, "messageRoot", message.gameObject);
                Set(controller, "messageText", body); Set(controller, "nameText", speaker);
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<TutorialController>();
        }

        private static void SetupScene(string path, TutorialMasterDatabase database, TutorialController prefab)
        {
            var scene = SceneManager.GetSceneByPath(path);
            var opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var components = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();
            var manager = components.OfType<TutorialManager>().FirstOrDefault();
            if (manager == null)
            {
                var root = new GameObject("TutorialSystem");
                SceneManager.MoveGameObjectToScene(root, scene);
                manager = root.AddComponent<TutorialManager>();
            }
            Set(manager, "database", database); Set(manager, "tutorialPrefab", prefab);
            var managerSo = new SerializedObject(manager);
            managerSo.FindProperty("presentationReadyOnStart").boolValue = false;
            managerSo.ApplyModifiedPropertiesWithoutUndo();
            if (path.Contains("MainScene"))
            {
                var main = components.OfType<MainScene>().Single();
                var bindings = new List<TutorialTargetBinding>();
                foreach (var name in new[] { "character", "progress", "home", "missions", "armory" })
                {
                    var tab = (PartsTabItem)Get(main, name + "Tab");
                    var key = char.ToUpperInvariant(name[0]) + name.Substring(1);
                    bindings.Add(new TutorialTargetBinding { Key = key + "Tab", Target = tab.Button.gameObject });
                    var screen = (GameObject)Get(main, name + "TabScreen");
                    var scope = Scope(screen, "Main." + key, TutorialSignal.TabReady);
                    SetBindings(scope, new List<TutorialTargetBinding> { new() { Key = "Root", Target = screen } });
                }
                SetBindings(Scope(main.gameObject, "Main", TutorialSignal.None), bindings);
                foreach (var progress in components.OfType<ProgressScreen>())
                    foreach (var name in new[] { "trait", "insights", "remnant" })
                    {
                        var screen = (Component)Get(progress, name + "TabScreen");
                        var key = name == "trait" ? "Traits" : char.ToUpperInvariant(name[0]) + name.Substring(1);
                        SetBindings(Scope(screen.gameObject, "Progress." + key, TutorialSignal.TabReady),
                            new List<TutorialTargetBinding> { new() { Key = "Root", Target = screen.gameObject } });
                    }
            }
            else
            {
                var battle = components.OfType<BattleSceneController>().Single();
                var board = components.OfType<BattleBoardController>().First();
                var bindings = new List<TutorialTargetBinding> {
                    new() { Key = "Board", Target = ((Component)Get(board, "cellRoot")).gameObject }
                };
                var palette = components.OfType<SkillPaletteView>().FirstOrDefault();
                if (palette != null)
                {
                    var slots = new SerializedObject(palette).FindProperty("skillSlots");
                    for (var i = 0; i < slots.arraySize; i++)
                    {
                        var button = (Button)slots.GetArrayElementAtIndex(i).FindPropertyRelative("button").objectReferenceValue;
                        if (button != null) bindings.Add(new TutorialTargetBinding { Key = "SkillSlot" + i, Target = button.gameObject });
                    }
                }
                SetBindings(Scope(battle.gameObject, "Battle", TutorialSignal.None), bindings);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }

        private static TutorialScope Scope(GameObject root, string key, TutorialSignal signal)
        {
            var scope = root.GetComponent<TutorialScope>();
            if (scope == null) scope = root.AddComponent<TutorialScope>();
            var so = new SerializedObject(scope);
            so.FindProperty("scopeKey").stringValue = key;
            so.FindProperty("readySignal").enumValueIndex = (int)signal;
            so.ApplyModifiedPropertiesWithoutUndo();
            return scope;
        }
        private static void SetBindings(TutorialScope scope, List<TutorialTargetBinding> bindings)
        {
            var so = new SerializedObject(scope);
            var entries = so.FindProperty("targets");
            entries.arraySize = bindings.Count;
            for (var i = 0; i < bindings.Count; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Key").stringValue = bindings[i].Key;
                entry.FindPropertyRelative("Target").objectReferenceValue = bindings[i].Target;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static UnityEngine.Object Get(UnityEngine.Object target, string field) =>
            new SerializedObject(target).FindProperty(field).objectReferenceValue;
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void EnsureFolder(string path)
        {
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
#endif
