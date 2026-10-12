#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Tutorial;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editor
{
    /// <summary>実ファイルに保存しない独立した一時Sceneで機能を確認する。</summary>
    public static class TutorialSmokeChecks
    {
        private sealed class MemoryStore : IUserDataStore
        {
            public string SavePath => "memory";
            public string Json;
            public bool Exists() => Json != null;
            public UserData Load() => JsonConvert.DeserializeObject<UserData>(Json);
            public void Save(UserData userData) => Json = JsonConvert.SerializeObject(userData);
        }
        private static int _assertions;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [MenuItem("The Red Fever/Tutorial/Run Smoke Checks")]
        public static string Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Modeで実行してください。");
            _assertions = 0;
            var previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var previousManager = GetStatic(typeof(TutorialManager), "_instance");
            var previousPending = GetStatic(typeof(TutorialManager), "_pendingStateNotices");
            var repeatOverrides = (Dictionary<int, bool>)GetStatic(typeof(TutorialManager), "_debugRepeatOverrides");
            var previousOverrides = new Dictionary<int, bool>(repeatOverrides);
            repeatOverrides.Clear();
            SetStatic(typeof(TutorialManager), "_pendingStateNotices", Activator.CreateInstance(previousPending.GetType()));
            var previousEventSystem = EventSystem.current;
            var previousEventSystems = GetStatic(typeof(EventSystem), "m_EventSystems");
            var previousSelection = previousEventSystem != null ? previousEventSystem.currentSelectedGameObject : null;
            var assets = new List<Object>();
            try
            {
                var root = Root(scene, "TutorialChecks");
                Child(root, "FixtureCamera", typeof(Camera)).GetComponent<Camera>().enabled = false;
                Child(root, "FixtureLight", typeof(Light)).GetComponent<Light>().type = LightType.Directional;
                SetStatic(typeof(EventSystem), "m_EventSystems", new List<EventSystem> {
                    Child(root, "FixtureEventSystem").AddComponent<EventSystem>()
                });
                var fixture = Child(root, "Fixture");
                var user = fixture.AddComponent<UserDataManager>();
                Set(user, "<UserData>k__BackingField", new UserData { Tutorial = null });
                var memory = new MemoryStore();
                var store = new UserDataJsonStore(user, memory, new DefaultUserDataFactory());
                Set(user, "<UserDataStore>k__BackingField", store);
                store.SaveSession();
                Check(user.UserData.Tutorial != null && user.UserData.Tutorial.CompletedTutorialIds.Count == 0, "旧セーブのTutorial欄を補完");

                var manager = fixture.AddComponent<TutorialManager>();
                Set(manager, "_user", user);
                SetStatic(typeof(TutorialManager), "_instance", manager);
                var db = ScriptableObject.CreateInstance<TutorialMasterDatabase>(); assets.Add(db);
                Set(manager, "database", db);
                Set(manager, "tutorialPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Tutorial.prefab").GetComponent<TutorialController>());
                var scope = fixture.AddComponent<TutorialScope>();
                Set(scope, "scopeKey", "Checks"); Set(scope, "<IsReady>k__BackingField", true);
                manager.Register(scope);

                var high = Master(401, TutorialSignal.TabReady, "Checks", assets);
                high.Priority = 10; high.Steps[0].Advance = TutorialAdvance.Signal;
                high.Steps[0].CompletionSignal = TutorialSignal.SkillUsed;
                var low = Master(402, TutorialSignal.TabReady, "Checks", assets);
                Set(db, "tutorials", new List<TutorialMasterData> { low, high });
                TutorialManager.Report(TutorialSignal.TabReady, "Checks", fixture);
                Tick(manager);
                Check(ActiveId(manager) == 401, "同時候補は優先度順");
                Check(user.UserData.Tutorial.VisitedScopeKeys.Contains("TabReady:Checks"), "初回訪問を完了と独立して記録");
                TutorialManager.Report(TutorialSignal.SkillSelected, owner: fixture); Tick(manager);
                Check(ActiveId(manager) == 401, "Skill選択だけでは使用成立のStepを進めない");
                TutorialManager.Report(TutorialSignal.SkillUsed, owner: fixture); Tick(manager);
                Check(user.UserData.Tutorial.CompletedTutorialIds.Contains(401) && ActiveId(manager) == 402, "使用成立後に完了保存し次候補へ");
                ClickScreen(manager); Tick(manager);
                Check(!manager.IsRunning && memory.Load().Tutorial.CompletedTutorialIds.Contains(402), "全面タップで完了しJSONに保存");
                TutorialManager.Report(TutorialSignal.TabReady, "Checks", fixture); Tick(manager);
                Check(!manager.IsRunning, "完了済みTutorialの再発火を防止");

                var retry = Master(403, TutorialSignal.TabReady, "Checks", assets);
                Set(db, "tutorials", new List<TutorialMasterData> { retry });
                TutorialManager.Report(TutorialSignal.TabReady, "Checks", fixture); Tick(manager);
                manager.Cancel();
                Check(!user.UserData.Tutorial.CompletedTutorialIds.Contains(403), "中断を完了扱いにしない");
                TutorialManager.Report(TutorialSignal.TabReady, "Checks", fixture); Tick(manager);
                Check(ActiveId(manager) == 403, "中断した案内は次の適切な機会に再生");
                manager.Cancel();

                var literalFirst = Master(404, TutorialSignal.TabReady, "First", assets);
                literalFirst.OnlyFirstVisit = true;
                Set(db, "tutorials", new List<TutorialMasterData> { literalFirst });
                TutorialManager.Report(TutorialSignal.TabReady, "First", fixture); Tick(manager);
                Check(ActiveId(manager) == 404, "厳密な初回表示条件"); manager.Cancel();
                TutorialManager.Report(TutorialSignal.TabReady, "First", fixture); Tick(manager);
                Check(!manager.IsRunning, "厳密な初回だけの案内は再訪時に発火しない");

                var perBattle = Master(405, TutorialSignal.TurnReady, "", assets);
                perBattle.Repeat = TutorialRepeat.OncePerBattle;
                perBattle.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.TurnAtLeast, Value = 2 });
                Set(db, "tutorials", new List<TutorialMasterData> { perBattle });
                manager.SetBattleContext(99, 1, 50, 100, true);
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "ターン条件未達では開始しない");
                manager.SetBattleContext(99, 2, 50, 100, false);
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "演出中は開始を待機");
                manager.SetBattleContext(99, 2, 50, 100, true); Tick(manager);
                Check(ActiveId(manager) == 405, "操作可能な区切りで候補を再判定して開始");
                ClickScreen(manager); Tick(manager);
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "同じ戦闘では一度だけ");
                TutorialManager.Report(TutorialSignal.BattleReady, owner: fixture);
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(ActiveId(manager) == 405, "新しい戦闘では再生可能"); manager.Cancel();

                var resurgence = Master(406, TutorialSignal.HomeReady, "", assets);
                resurgence.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.IsResurgence });
                resurgence.Steps[0].ResurgenceMessage = "<color=red>Changed guidance.</color>";
                Set(db, "tutorials", new List<TutorialMasterData> { resurgence });
                TutorialManager.Report(TutorialSignal.HomeReady, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "通常周回ではResurgence専用案内を開始しない");
                user.UserData.Cycle.IsResurgence = true;
                TutorialManager.Report(TutorialSignal.HomeReady, owner: fixture); Tick(manager);
                var view = (TutorialController)Get(manager, "_view");
                Check(((TMP_Text)Get(view, "messageText")).text.Contains("color=red"), "Resurgenceの台詞差分とrich text");
                manager.Cancel();
                Check(user.UserData.Tutorial.CompletedTutorialIds.Contains(401), "周回状態変更で既存の完了記録を消さない");

                Set(user, "<Inventory>k__BackingField", new Assets.Scripts.Systems.GameData.InventoryManager(store, Array.Empty<Assets.Scripts.Data.DTO.EquipmentData>()));
                var equipment = ScriptableObject.CreateInstance<EquipmentMasterData>(); assets.Add(equipment);
                Set(equipment, "equipmentId", 999);
                var tree = AssetDatabase.LoadAssetAtPath<AbilityTreeMasterData>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:AbilityTreeMasterData")[0]));
                var growth = Master(407, TutorialSignal.UserDataChanged, "", assets);
                growth.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.EquipmentOwned, Equipment = equipment });
                growth.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.TraitNodeLevelAtLeast, Tree = tree, TreeNodeId = "check-node", Value = 2 });
                Set(db, "tutorials", new List<TutorialMasterData> { growth });
                TutorialManager.Report(TutorialSignal.UserDataChanged, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "装備・Trait条件未達では開始しない");
                user.UserData.Inventory.Equipments.Add(new InventoryEntryData { EquipmentId = 999, IsUnlocked = true, Quantity = 1 });
                user.UserData.Ability.NodeLevels.Add(new AbilityTreeNodeLevelData { TreeId = tree.TreeId, TreeNodeId = "check-node", Level = 2 });
                TutorialManager.Report(TutorialSignal.UserDataChanged, owner: fixture); Tick(manager);
                Check(ActiveId(manager) == 407, "保存済みの装備とTree配置NodeのLevelを照合"); manager.Cancel();

                var readySkill = Master(408, TutorialSignal.TurnReady, "", assets);
                readySkill.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.SkillReady, Id = 0 });
                Set(db, "tutorials", new List<TutorialMasterData> { readySkill });
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "Skill未準備のスロットを案内しない");
                manager.SetSkillReady(0, true);
                TutorialManager.Report(TutorialSignal.TurnReady, owner: fixture); Tick(manager);
                Check(ActiveId(manager) == 408, "Skill準備完了のスロットを条件に開始"); manager.Cancel();

                var manual = Master(409, TutorialSignal.Manual, "equipment-guide", assets);
                Set(db, "tutorials", new List<TutorialMasterData> { manual });
                TutorialManager.Report(TutorialSignal.Manual, "other-guide", fixture); Tick(manager);
                Check(!manager.IsRunning, "手動通知もKeyで絞り込む");
                TutorialManager.Report(TutorialSignal.Manual, "equipment-guide", fixture); Tick(manager);
                Check(ActiveId(manager) == 409, "任意のタイミングで手動通知から開始"); manager.Cancel();

                var transition = Master(410, TutorialSignal.ResurgenceStarted, "", assets);
                transition.RequiredScopeKey = "NextScene";
                Set(db, "tutorials", new List<TutorialMasterData> { transition });
                TutorialManager.Report(TutorialSignal.ResurgenceStarted, owner: fixture); Tick(manager);
                Check(!manager.IsRunning, "遷移先の画面準備ができるまでは開始しない");
                typeof(TutorialManager).GetMethod("OnDestroy", PrivateInstance).Invoke(manager, null);
                var pending = GetStatic(typeof(TutorialManager), "_pendingStateNotices");
                Check((int)pending.GetType().GetProperty("Count").GetValue(pending) == 1, "Scene終了時に状態通知だけ引き継ぐ");
                SetStatic(typeof(TutorialManager), "_instance", manager);
                Set(scope, "scopeKey", "NextScene");
                typeof(TutorialManager).GetMethod("ReplayPendingNotices", PrivateInstance).Invoke(manager, null);
                Tick(manager);
                Check(ActiveId(manager) == 410, "次Sceneの準備後に状態通知を再照合して開始"); manager.Cancel();

                TestInput(root, view);
                TestHoles(root);
                // 実際のHome導入マスターをメモリセーブだけで先頭から完了まで実行する。
                var introduction = AssetDatabase.LoadAssetAtPath<TutorialMasterData>("Assets/Resources/MasterData/Tutorial/Home-AngelIntroduction.asset");
                TutorialManager.SetRepeatForDebug(introduction.TutorialId, false);
                Set(scope, "scopeKey", "Main.Home");
                var previewCanvas = Child(root, "HomePreviewCanvas", typeof(RectTransform), typeof(Canvas));
                var angel = Child(previewCanvas, "Angel", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
                angel.texture = Texture2D.whiteTexture;
                scope.Bind("AngelPreview", angel.gameObject);
                Set(db, "tutorials", new List<TutorialMasterData> { introduction });
                TutorialManager.Report(TutorialSignal.HomeReady, "Main.Home", fixture); Tick(manager);
                Check(!manager.IsRunning, "Home通常導入はResurgenceに発火しない");
                user.UserData.Cycle.IsResurgence = false;
                TutorialManager.Report(TutorialSignal.HomeReady, "Main.Home", fixture); Tick(manager);
                Check(ActiveId(manager) == introduction.TutorialId, "HomeReadyで登録済みAngel導入を開始");
                for (var i = 0; i < introduction.Steps.Count; i++)
                {
                    Check(((TMP_Text)Get(view, "messageText")).text == introduction.Steps[i].Message,
                        "導入の台詞を順番に表示 " + i);
                    Set(view, "_shownFrame", Time.frameCount - 1);
                    ClickToAdvance(view); Tick(manager);
                }
                Check(!manager.IsRunning && memory.Load().Tutorial.CompletedTutorialIds.Contains(introduction.TutorialId),
                    "導入の最後のタップで完了を保存");
                TutorialManager.Report(TutorialSignal.HomeReady, "Main.Home", fixture); Tick(manager);
                Check(!manager.IsRunning, "Home再入場では完了済み導入を再生しない");
                TutorialManager.SetRepeatForDebug(introduction.TutorialId, true);
                TutorialManager.Report(TutorialSignal.HomeReady, "Main.Home", fixture); Tick(manager);
                Check(ActiveId(manager) == introduction.TutorialId && manager.IsCompleted(introduction.TutorialId),
                    "デバッグ繰り返しは完了フラグを維持してHomeReadyで再生"); manager.Cancel();
                TutorialManager.SetRepeatForDebug(introduction.TutorialId, false);
                manager.SetCompletedForDebug(introduction.TutorialId, false);
                Check(!memory.Load().Tutorial.CompletedTutorialIds.Contains(introduction.TutorialId), "個別の未完了設定を即時保存");
                manager.SetCompletedForDebug(introduction.TutorialId, true);
                Check(memory.Load().Tutorial.CompletedTutorialIds.Contains(introduction.TutorialId), "個別の完了設定を即時保存");
                Check(manager.ReplayForDebug(introduction), "完了済み導入を発火通知なしでデバッグ再生"); Tick(manager);
                Check(ActiveId(manager) == introduction.TutorialId, "デバッグ再生候補を現在のScopeで開始");
                manager.Cancel();
                manager.SetCompletedForDebug(introduction.TutorialId, false);
                Check(manager.ReplayForDebug(introduction), "未完了の導入もデバッグ再生できる"); Tick(manager);
                for (var i = 0; i < introduction.Steps.Count; i++)
                {
                    Set(view, "_shownFrame", Time.frameCount - 1);
                    ClickToAdvance(view); Tick(manager);
                }
                Check(!manager.IsRunning && !manager.IsCompleted(introduction.TutorialId), "デバッグ再生完了で未完了フラグを変更しない");
                angel.gameObject.SetActive(false);
                Check(!manager.ReplayForDebug(introduction), "対象が未表示の場合はデバッグ再生も開始しない");
                TestHomeEntryOrdering(root, manager, db, user, assets);
                var result = "Tutorial checks passed: " + _assertions + " assertions. No user save files written.";
                Debug.Log(result);
                return result;
            }
            finally
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var view in root.GetComponentsInChildren<TutorialController>(true)) view.Hide();
                if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
                EditorSceneManager.CloseScene(scene, true);
                foreach (var asset in assets) Object.DestroyImmediate(asset);
                SetStatic(typeof(TutorialManager), "_instance", previousManager);
                SetStatic(typeof(TutorialManager), "_pendingStateNotices", previousPending);
                repeatOverrides.Clear();
                foreach (var entry in previousOverrides) repeatOverrides.Add(entry.Key, entry.Value);
                SetStatic(typeof(EventSystem), "m_EventSystems", previousEventSystems);
                if (previousEventSystem != null) previousEventSystem.SetSelectedGameObject(previousSelection);
            }
        }

        private static void TestHomeEntryOrdering(GameObject root, TutorialManager manager, TutorialMasterDatabase db,
            UserDataManager user, List<Object> assets)
        {
            var hud = Child(root, "HomeHud").AddComponent<CanvasGroup>();
            var tabs = Child(root, "HomeTabs").AddComponent<CanvasGroup>();
            var prop = Child(root, "PropButton", typeof(RectTransform), typeof(Button)).GetComponent<Button>();
            prop.interactable = false;
            var director = Child(root, "EntryDirector").AddComponent<Assets.Scripts.Features.Main.HomeEntranceDirector>();
            Set(director, "homeHudCanvasGroup", hud); Set(director, "additionalHudCanvasGroups", new[] { tabs });
            Set(director, "propButtons", new[] { prop }); Set(director, "homeHudFadeDuration", 0f);
            foreach (var field in new[] { "buildParts", "foliageParts", "propParts", "propFoliageParts" })
                Set(director, field, Array.Empty<Assets.Scripts.Features.Main.HomeBuildPart>());
            typeof(Assets.Scripts.Features.Main.HomeEntranceDirector).GetMethod("SetHudVisibility", PrivateInstance)
                .Invoke(director, new object[] { 0f, false });
            var events = new List<string>();
            Set(director, "_finishLoading", (Func<System.Collections.IEnumerator>)(() => FinishLoadingForCheck(events)));
            var first = Master(420, TutorialSignal.HomeReady, "Main.Home", assets);
            first.Priority = 20;
            first.Steps.Add(new TutorialStep { Message = "Second line." });
            var second = Master(421, TutorialSignal.HomeReady, "Main.Home", assets);
            var elsewhere = Master(422, TutorialSignal.HomeReady, "Main.Home", assets);
            elsewhere.RequiredScopeKey = "OtherScreen";
            var followup = Master(423, TutorialSignal.UserDataChanged, "", assets);
            followup.Conditions.Add(new TutorialCondition { Type = TutorialConditionType.TutorialCompleted, Tutorial = first });
            Set(db, "tutorials", new List<TutorialMasterData> { first, second, elsewhere, followup });
            var saved = (Action)Delegate.CreateDelegate(typeof(Action), manager,
                typeof(TutorialManager).GetMethod("OnUserDataSaved", PrivateInstance));
            user.UserDataSaved += saved;
            try
            {
                var play = (System.Collections.IEnumerator)typeof(Assets.Scripts.Features.Main.HomeEntranceDirector)
                    .GetMethod("PlayRoutine", PrivateInstance).Invoke(director, new object[] { (Action)(() => events.Add("Angel")) });
                play.MoveNext(); // 葉の待機。
                play.MoveNext(); // 完成保持。
                play.MoveNext();
                var loading = (System.Collections.IEnumerator)play.Current;
                loading.MoveNext();
                Check(events.SequenceEqual(new[] { "LoadingStart" }) && hud.alpha == 0 && tabs.alpha == 0,
                    "Loadingフェード中はAngelと全HUDを表示しない");
                loading.MoveNext();
                play.MoveNext();
                var home = (System.Collections.IEnumerator)play.Current;
                home.MoveNext(); home.MoveNext();
                Check(events.SequenceEqual(new[] { "LoadingStart", "LoadingEnd", "Angel" }) && hud.alpha == 0,
                    "Loading終了後にAngelだけ登場しHUDは非表示");
                home.MoveNext();
                var batch = (System.Collections.IEnumerator)home.Current;
                Check(batch.MoveNext(), "発火通知の照合前にチュートリアル無しと判断しない");
                Tick(manager);
                Check(ActiveId(manager) == 420 && batch.MoveNext() && !hud.blocksRaycasts && !tabs.interactable,
                    "HomeReadyの最初の案内中はHUD表示と操作を待つ");
                ClickScreen(manager); Tick(manager);
                Check(ActiveId(manager) == 420 && batch.MoveNext() && hud.alpha == 0,
                    "台詞を1つ送っただけではHUDを表示しない");
                ClickScreen(manager); Tick(manager);
                Check(ActiveId(manager) == 421 && batch.MoveNext(), "最初の案内完了後も次の案内を待つ");
                ClickScreen(manager); Tick(manager);
                Check(ActiveId(manager) == 423 && batch.MoveNext(), "完了保存で発火した後続案内も待つ");
                ClickScreen(manager); Tick(manager);
                Check(!batch.MoveNext(), "別画面でしか開始できない候補を待ち続けない");
                Check(!home.MoveNext() && hud.alpha == 1 && tabs.alpha == 1 && hud.blocksRaycasts && prop.interactable,
                    "全案内終了後にHUDと小道具の操作を解放する");

                Set(db, "tutorials", new List<TutorialMasterData>());
                var empty = manager.PlayReadyTutorials(TutorialSignal.HomeReady, "Main.Home", director.gameObject);
                empty.MoveNext(); Tick(manager);
                Check(!empty.MoveNext(), "チュートリアル無しならそのままHUD表示へ進める");
            }
            finally { user.UserDataSaved -= saved; manager.Cancel(); }
        }

        private static System.Collections.IEnumerator FinishLoadingForCheck(List<string> events)
        {
            events.Add("LoadingStart");
            yield return null;
            events.Add("LoadingEnd");
        }

        private static void TestInput(GameObject root, TutorialController view)
        {
            var camera = root.GetComponentInChildren<Camera>();
            camera.enabled = false;
            var texture = new RenderTexture(Screen.width, Screen.height, 24);
            camera.targetTexture = texture;
            try
            {
            var es = Child(root, "TestEventSystem").AddComponent<EventSystem>();
            SetStatic(typeof(EventSystem), "m_EventSystems", new List<EventSystem> { es });
            var source = Child(root, "SourceCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(ScrollRect));
            source.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            source.GetComponent<Canvas>().worldCamera = camera;
            source.GetComponent<Canvas>().planeDistance = 1;
            source.GetComponent<Canvas>().sortingOrder = 100;
            var overlay = view.GetComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceCamera;
            overlay.worldCamera = camera;
            overlay.planeDistance = 1;
            var target = Child(source, "Allowed", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)target.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(100, 100);
            var other = Child(source, "Other", typeof(RectTransform), typeof(Image), typeof(Button));
            ((RectTransform)other.transform).sizeDelta = new Vector2(100, 100);
            ((RectTransform)other.transform).anchoredPosition = new Vector2(200, 0);
            var initiallyDisabled = Child(root, "DisabledCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            initiallyDisabled.GetComponent<GraphicRaycaster>().enabled = false;
            es.SetSelectedGameObject(target);
            var step = new TutorialStep { Advance = TutorialAdvance.TargetButton, ShowHand = true };
            view.Show(step, new[] { rect }, target, rect, false, () => {});
            Canvas.ForceUpdateCanvases();
            camera.Render();
            typeof(TutorialController).GetMethod("RefreshPositions", PrivateInstance).Invoke(view, null);
            var hand = (RectTransform)Get(view, "tutorialHand");
            var initialHand = hand.anchoredPosition;
            var backdrop = (TutorialBackdropGraphic)Get(view, "backdrop");
            var initialHole = ((List<Rect>)Get(backdrop, "_holes"))[0];
            rect.anchoredPosition = new Vector2(40, 0);
            Canvas.ForceUpdateCanvases(); camera.Render();
            typeof(TutorialController).GetMethod("RefreshPositions", PrivateInstance).Invoke(view, null);
            Check(hand.anchoredPosition.x > initialHand.x && ((List<Rect>)Get(backdrop, "_holes"))[0].center.x > initialHole.center.x,
                "手と背景の穴が対象UIの移動に追従する");
            rect.anchoredPosition = Vector2.zero;
            Canvas.ForceUpdateCanvases(); camera.Render();
            typeof(TutorialController).GetMethod("RefreshPositions", PrivateInstance).Invoke(view, null);
            var gate = (TutorialInputRaycaster)Get(view, "inputRaycaster");
            var pointer = new PointerEventData(es);
            var results = new List<RaycastResult>();
            pointer.position = RectTransformUtility.WorldToScreenPoint(camera, rect.position);
            gate.Raycast(pointer, results);
            Check(results.Count == 1 && results[0].gameObject == target,
                "実際の対象Buttonへ入力を通す (hits=" + string.Join(",", results.Select(hit => hit.gameObject.name)) +
                ", depth=" + target.GetComponent<Image>().depth + ", position=" + pointer.position +
                ", graphics=" + GraphicRegistry.GetGraphicsForCanvas(source.GetComponent<Canvas>()).Count + ")");
            results.Clear(); pointer.position = RectTransformUtility.WorldToScreenPoint(camera, other.transform.position);
            gate.Raycast(pointer, results);
            Check(results.Count == 1 && results[0].gameObject != other, "対象外Buttonの入力をブロック");
            Check(!es.sendNavigationEvents && es.currentSelectedGameObject == null, "キーとゲームパッドによる別UI操作をブロック");
            Check(!source.GetComponent<ScrollRect>().enabled, "Buttonから親ScrollRectへの操作伝播を停止");
            var nested = Child(target, "NestedOtherButton", typeof(RectTransform), typeof(Image), typeof(Button));
            Canvas.ForceUpdateCanvases(); camera.Render(); results.Clear();
            pointer.position = RectTransformUtility.WorldToScreenPoint(camera, rect.position);
            gate.Raycast(pointer, results);
            Check(results.Count == 1 && results[0].gameObject != nested, "入れ子の別Buttonを許可対象と取り違えない");
            nested.SetActive(false);
            var occluder = Child(source, "Occluder", typeof(RectTransform), typeof(Image));
            ((RectTransform)occluder.transform).sizeDelta = new Vector2(100, 100);
            Canvas.ForceUpdateCanvases(); results.Clear();
            camera.Render();
            pointer.position = RectTransformUtility.WorldToScreenPoint(camera, rect.position);
            gate.Raycast(pointer, results);
            Check(results.Count == 1 && results[0].gameObject != target && results[0].gameObject != occluder, "対象を覆う別UIを透過して操作させない");
            occluder.SetActive(false);
            var newCanvas = Child(root, "LateCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            newCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var lateRaycaster = newCanvas.GetComponent<GraphicRaycaster>();
            results.Clear(); gate.Raycast(pointer, results);
            Check(!lateRaycaster.enabled, "途中で生成されたCanvasも操作制限の対象");
            view.Hide();
            Check(source.GetComponent<GraphicRaycaster>().enabled && lateRaycaster.enabled, "終了時に元のRaycasterを復元");
            Check(!initiallyDisabled.GetComponent<GraphicRaycaster>().enabled, "元から無効なRaycasterは無効のまま");
            Check(source.GetComponent<ScrollRect>().enabled, "終了時に親ScrollRectを復元");
            Check(es.sendNavigationEvents && es.currentSelectedGameObject == target, "終了時にナビゲーションと選択を復元");

            var preview = Child(source, "AlphaPreview", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            preview.rectTransform.sizeDelta = new Vector2(100, 100);
            var mask = new Texture2D(4, 1, TextureFormat.RGBA32, false);
            var maskTexture = new RenderTexture(4, 1, 0, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            var previousTexture = RenderTexture.active;
            var readback = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            try
            {
                mask.filterMode = FilterMode.Point;
                mask.SetPixels(new[] { Color.white, Color.white, new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0) });
                mask.Apply(); Graphics.Blit(mask, maskTexture);
                preview.texture = maskTexture;
                var sent = 0;
                view.Show(new TutorialStep { Message = "Angel speaks.", MessageAtBottom = true, Advance = TutorialAdvance.ScreenTap },
                    Array.Empty<RectTransform>(), null, null, false, () => sent++, preview);
                Check(view.GetComponentsInChildren<Button>(true).Length == 0 && !hand.gameObject.activeSelf,
                    "Tutorial PrefabにContinueボタンがなく、会話の手も表示しない");
                var typingText = (TMP_Text)Get(view, "messageText");
                Check(view.IsTyping && typingText.maxVisibleCharacters == 0, "新しい台詞は0文字から表示を開始");
                typeof(TutorialController).GetMethod("UpdateTypewriter", PrivateInstance).Invoke(view, new object[] { 0.025f });
                Check(typingText.maxVisibleCharacters == 1, "文字送り速度に応じて1文字表示する");
                Canvas.ForceUpdateCanvases(); camera.Render();
                typeof(TutorialController).GetMethod("RefreshPositions", PrivateInstance).Invoke(view, null);
                pointer.position = RectTransformUtility.WorldToScreenPoint(camera, preview.rectTransform.position);
                results.Clear(); gate.Raycast(pointer, results);
                Check(results.Count == 1 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(results[0].gameObject) == view.gameObject,
                    "アルファの穴の中でも背後のPreviewではなくTutorialがタップを受ける");
                pointer.position = RectTransformUtility.WorldToScreenPoint(camera, other.transform.position);
                results.Clear(); gate.Raycast(pointer, results);
                Check(results.Count == 1 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(results[0].gameObject) == view.gameObject,
                    "対象外Button上でも背後を操作せず会話送りを受ける");
                pointer.position = RectTransformUtility.WorldToScreenPoint(camera, ((GameObject)Get(view, "messageRoot")).transform.position);
                results.Clear(); gate.Raycast(pointer, results);
                Check(results.Count == 1 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(results[0].gameObject) == view.gameObject,
                    "メッセージ上のタップでも会話を送れる");
                view.OnPointerClick(pointer);
                Check(sent == 0, "表示したフレームのクリックを次の台詞に流さない");
                Set(view, "_shownFrame", Time.frameCount - 1);
                pointer.pressPosition = pointer.position;
                pointer.dragging = true; view.OnPointerClick(pointer); pointer.dragging = false;
                Check(sent == 0, "ドラッグの指離しは台詞送りにしない");
                pointer.pressPosition = pointer.position + Vector2.one * (es.pixelDragThreshold + 1);
                view.OnPointerClick(pointer);
                Check(sent == 0, "DragHandlerが無い全面入力でもスワイプを台詞送りにしない");
                pointer.pressPosition = pointer.position;
                view.OnPointerClick(pointer);
                Check(sent == 0 && !view.IsTyping && typingText.maxVisibleCharacters == typingText.textInfo.characterCount,
                    "文字送り中のタップは全文を表示し、次の台詞へ進まない");
                pointer.pointerId = 1; view.OnPointerClick(pointer);
                Check(sent == 0, "全文表示した同じフレームの別タップで台詞を送らない");
                Set(view, "_shownFrame", Time.frameCount - 1);
                view.OnPointerClick(pointer); view.OnPointerClick(pointer);
                Check(sent == 1, "全文表示後の次のタップで一度だけ台詞を送る");

                camera.Render(); RenderTexture.active = texture;
                readback.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); readback.Apply();
                var left = RectTransformUtility.WorldToScreenPoint(camera, preview.rectTransform.TransformPoint(new Vector3(-37.5f, 0)));
                var right = RectTransformUtility.WorldToScreenPoint(camera, preview.rectTransform.TransformPoint(new Vector3(37.5f, 0)));
                var partial = RectTransformUtility.WorldToScreenPoint(camera, preview.rectTransform.TransformPoint(new Vector3(12.5f, 0)));
                var opaqueValue = readback.GetPixel((int)left.x, (int)left.y).r;
                var transparentValue = readback.GetPixel((int)right.x, (int)right.y).r;
                var partialValue = readback.GetPixel((int)partial.x, (int)partial.y).r;
                Check(opaqueValue > 0.85f && transparentValue < 0.65f && opaqueValue > transparentValue + 0.3f,
                    "GPU描画でAngelの不透明部だけ明るく、透明部は暗い (opaque=" + opaqueValue + ", transparent=" + transparentValue + ")");
                Check(partialValue > transparentValue + 0.1f && partialValue < opaqueValue - 0.1f,
                    "半透明の輪郭はアルファに応じてなめらかに暗幕を除く");
                // RenderTextureの更新をそのまま反映し、以前の輪郭を残さない。
                mask.SetPixels(new[] { Color.clear, Color.clear, Color.clear, Color.clear });
                mask.Apply(); Graphics.Blit(mask, maskTexture); camera.Render();
                RenderTexture.active = texture;
                readback.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); readback.Apply();
                Check(readback.GetPixel((int)left.x, (int)left.y).r < 0.65f, "RenderTextureの動的更新に暗幕の輪郭が追従する");
                preview.gameObject.SetActive(false);
                Check(view.HasLostTarget, "Angelの非表示で会話の対象消失を検知");
                view.Hide();
                view.Show(new TutorialStep { Advance = TutorialAdvance.ScreenTap }, Array.Empty<RectTransform>(), null, null, false, () => {});
                Check(backdrop.material != Get(backdrop, "_alphaMaterial"), "次の通常Stepにアルファ穴のMaterialを持ち越さない");
                view.Show(new TutorialStep { Message = "<color=red>ABC</color>", Advance = TutorialAdvance.ScreenTap },
                    Array.Empty<RectTransform>(), null, null, true, () => {});
                typeof(TutorialController).GetMethod("UpdateTypewriter", PrivateInstance).Invoke(view, new object[] { 0.025f });
                Check(typingText.textInfo.characterCount == 3 && typingText.maxVisibleCharacters == 1 && typingText.text.EndsWith("</color>"),
                    "rich textのタグを文字送りに含めず、文字装飾を保つ");
                view.Hide();
            }
            finally
            {
                RenderTexture.active = previousTexture;
                Object.DestroyImmediate(mask); Object.DestroyImmediate(maskTexture); Object.DestroyImmediate(readback);
            }
            }
            finally
            {
                view.Hide();
                camera.targetTexture = null;
                Object.DestroyImmediate(texture);
            }
        }

        private static void TestHoles(GameObject root)
        {
            var go = Child(root, "BackdropGeometry", typeof(RectTransform), typeof(TutorialBackdropGraphic));
            ((RectTransform)go.transform).sizeDelta = new Vector2(300, 200);
            var graphic = go.GetComponent<TutorialBackdropGraphic>();
            graphic.SetHoles(new[] { new Rect(-75, -50, 100, 100), new Rect(-25, -50, 100, 100) });
            using (var vh = new VertexHelper())
            {
                typeof(TutorialBackdropGraphic).GetMethod("OnPopulateMesh", PrivateInstance | BindingFlags.DeclaredOnly).Invoke(graphic, new object[] { vh });
                var mesh = new Mesh();
                try
                {
                    vh.FillMesh(mesh);
                    var vertices = mesh.vertices; var triangles = mesh.triangles; var area = 0f;
                    for (var i = 0; i < triangles.Length; i += 3)
                        area += Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]], vertices[triangles[i + 2]] - vertices[triangles[i]]).magnitude * 0.5f;
                    Check(Mathf.Abs(area - 45000) < 0.1f, "複数の穴が重なっても暗幕の面積と透過領域が正しい");
                }
                finally { Object.DestroyImmediate(mesh); }
            }
        }
        private static TutorialMasterData Master(int id, TutorialSignal trigger, string key, List<Object> assets)
        {
            var master = ScriptableObject.CreateInstance<TutorialMasterData>(); assets.Add(master);
            master.TutorialId = id; master.Trigger = trigger; master.TriggerKey = key;
            master.Steps.Add(new TutorialStep { Message = "Check guidance.", Advance = TutorialAdvance.ScreenTap });
            return master;
        }
        private static void ClickScreen(TutorialManager manager) => ClickToAdvance((TutorialController)Get(manager, "_view"));
        private static void ClickToAdvance(TutorialController view)
        {
            typeof(TutorialController).GetMethod("UpdateTypewriter", PrivateInstance).Invoke(view, new object[] { 1000f });
            Set(view, "_shownFrame", Time.frameCount - 1);
            view.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        private static void Tick(TutorialManager manager) => typeof(TutorialManager).GetMethod("LateUpdate", PrivateInstance).Invoke(manager, null);
        private static int ActiveId(TutorialManager manager)
        {
            var active = Get(manager, "_active");
            return active == null ? -1 : ((TutorialMasterData)active.GetType().GetField("Master").GetValue(active)).TutorialId;
        }
        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Tutorial check failed: " + message);
            _assertions++;
        }
        private static GameObject Root(Scene scene, string name)
        {
            var root = new GameObject(name); SceneManager.MoveGameObjectToScene(root, scene); return root;
        }
        private static GameObject Child(GameObject parent, string name, params Type[] types)
        {
            var go = new GameObject(name, types); go.transform.SetParent(parent.transform, false); return go;
        }
        private static object Get(object target, string field) => target.GetType().GetField(field, PrivateInstance).GetValue(target);
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, PrivateInstance).SetValue(target, value);
        private static object GetStatic(Type type, string field) => type.GetField(field, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        private static void SetStatic(Type type, string field, object value) => type.GetField(field, BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
    }
}
#endif
