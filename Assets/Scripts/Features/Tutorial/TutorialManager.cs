using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Features.Tutorial
{
    /// <summary>発火判定、実行中のStep、完了記録を管理する。表示の正本は持たない。</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class TutorialManager : MonoBehaviour
    {
        private sealed class Notice
        {
            public TutorialSignal Signal;
            public string Key;
            public GameObject Owner;
            public long Sequence;
            public bool FirstVisit;
            public int OriginGeneration;
        }
        private sealed class Candidate
        {
            public TutorialMasterData Master;
            public Notice Notice;
            public TutorialScope Origin;
            public bool DebugReplay;
        }

        [SerializeField] private TutorialMasterDatabase database;
        [SerializeField] private TutorialController tutorialPrefab;
        [SerializeField] private bool presentationReadyOnStart = true;
        private static TutorialManager _instance;
        private static List<Notice> _pendingStateNotices = new();
        private static readonly Dictionary<int, bool> _debugRepeatOverrides = new();
        private readonly List<TutorialScope> _scopes = new();
        private readonly Queue<Notice> _notices = new();
        private readonly Dictionary<int, Candidate> _candidates = new();
        private readonly HashSet<int> _playedThisBattle = new();
        private readonly HashSet<int> _readySkillSlots = new();
        private readonly List<RectTransform> _highlights = new();
        private UserDataManager _user;
        private TutorialController _view;
        private Candidate _active;
        private int _stepIndex;
        private long _sequence;
        private long _stepSequence;
        private bool _advanceRequested;
        private bool _stepShown;
        private float _waitingSince;
        private int _stageId = -1;
        private int _turn;
        private int _playerHp;
        private int _maximumHp = 1;
        private bool _canPresent = true;
        private bool _inBattle;

        public static TutorialManager Current => _instance;
        public static TutorialManager Instance => _instance != null ? _instance :
            (_instance = FindFirstObjectByType<TutorialManager>());
        public bool IsRunning => _active != null;
        public IReadOnlyList<TutorialMasterData> Tutorials => database.Tutorials;

        public static bool RepeatsForDebug(TutorialMasterData master) =>
            (Application.isEditor || Debug.isDebugBuild) &&
            (_debugRepeatOverrides.TryGetValue(master.TutorialId, out var value) ? value : master.RepeatForDebug);

        public static void SetRepeatForDebug(int tutorialId, bool repeat) => _debugRepeatOverrides[tutorialId] = repeat;

        public bool IsCompleted(int tutorialId) => _user.UserData.Tutorial.CompletedTutorialIds.Contains(tutorialId);

        public void SetCompletedForDebug(int tutorialId, bool completed)
        {
            if (_active != null && _active.Master.TutorialId == tutorialId) Cancel();
            _candidates.Remove(tutorialId);
            var ids = _user.UserData.Tutorial.CompletedTutorialIds;
            if (completed)
            {
                if (!ids.Contains(tutorialId)) ids.Add(tutorialId);
            }
            else
            {
                ids.RemoveAll(id => id == tutorialId);
                _playedThisBattle.Remove(tutorialId);
            }
            _user.MarkDirty();
            _user.UserDataStore.SaveSession();
        }

        /// <summary>現在画面で実行可能なStepだけ再生。発火条件・完了フラグを無視し、完了記録は変更しない。</summary>
        public bool ReplayForDebug(TutorialMasterData master)
        {
            if (IsRunning || !_canPresent || master == null || master.Steps.Count == 0) return false;
            if (!string.IsNullOrEmpty(master.RequiredScopeKey) && FindScope(master.RequiredScopeKey) == null) return false;
            var candidate = new Candidate {
                Master = master, DebugReplay = true,
                Notice = new Notice { Owner = gameObject, Sequence = ++_sequence },
                Origin = string.IsNullOrEmpty(master.RequiredScopeKey) ? null : FindScope(master.RequiredScopeKey)
            };
            if (!TryResolveStep(candidate, 0, out _, out _, out _)) return false;
            if (candidate.Origin != null) candidate.Notice.OriginGeneration = candidate.Origin.Generation;
            _candidates[master.TutorialId] = candidate;
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
            _pendingStateNotices.Clear();
            _debugRepeatOverrides.Clear();
        }

        private void Awake()
        {
            _instance = this;
            _canPresent = presentationReadyOnStart;
            _user = UserDataManager.EnsureInitialized();
            _user.UserDataSaved += OnUserDataSaved;
            _user.UserData.Tutorial ??= new TutorialProgressData();
            Report(TutorialSignal.UserDataChanged);
            ReplayPendingNotices();
        }

        public static void Report(TutorialSignal signal, string key = "", GameObject owner = null)
        {
            var manager = Instance;
            if (signal == TutorialSignal.None) return;
            if (manager == null)
            {
                RememberStateNotice(new Notice { Signal = signal, Key = key ?? "" });
                return;
            }
            manager._notices.Enqueue(new Notice {
                Signal = signal, Key = key ?? "", Owner = owner != null ? owner : manager.gameObject,
                Sequence = ++manager._sequence,
                OriginGeneration = owner != null && owner.TryGetComponent<TutorialScope>(out var scope) ? scope.Generation : 0
            });
        }

        private static void RememberStateNotice(Notice notice)
        {
            if (notice.Signal != TutorialSignal.ResurgenceStarted && notice.Signal != TutorialSignal.BossResultRecorded &&
                notice.Signal != TutorialSignal.HigherBeingReached) return;
            if (!_pendingStateNotices.Exists(item => item.Signal == notice.Signal && item.Key == notice.Key))
                _pendingStateNotices.Add(new Notice { Signal = notice.Signal, Key = notice.Key });
        }

        private void ReplayPendingNotices()
        {
            var notices = _pendingStateNotices.ToArray();
            _pendingStateNotices.Clear();
            foreach (var notice in notices) Report(notice.Signal, notice.Key);
        }

        public void Register(TutorialScope scope)
        {
            if (!_scopes.Contains(scope)) _scopes.Add(scope);
            // 保存後に画面へ戻った場合も、保存状態を使う条件を再照合する。
            Report(TutorialSignal.UserDataChanged, owner: scope.gameObject);
        }
        public void Unregister(TutorialScope scope) => _scopes.Remove(scope);
        private void OnUserDataSaved() => Report(TutorialSignal.UserDataChanged);
        public void SetPresentationReady(bool ready) => _canPresent = ready;
        /// <summary>現在画面で開始できる案内と保存通知による後続案内をすべて待つ。別画面の候補は待たない。</summary>
        public IEnumerator PlayReadyTutorials(TutorialSignal signal, string key, GameObject owner)
        {
            SetPresentationReady(true);
            Report(signal, key, owner);
            // Report直後のIsRunning=falseを終了と扱わず、LateUpdateでの照合を必ず待つ。
            yield return null;
            while (IsRunning || _notices.Count > 0 || _candidates.Values.Any(CanStart)) yield return null;
        }

        private bool CanStart(Candidate candidate)
        {
            return Eligible(candidate.Master, candidate.DebugReplay) && candidate.Notice.Owner != null &&
                candidate.Notice.Owner.activeInHierarchy &&
                (candidate.Origin == null || (candidate.Origin.IsReady &&
                    candidate.Origin.Generation == candidate.Notice.OriginGeneration)) &&
                (string.IsNullOrEmpty(candidate.Master.RequiredScopeKey) || FindScope(candidate.Master.RequiredScopeKey) != null) &&
                TryResolveStep(candidate, 0, out _, out _, out _);
        }
        public void SetSkillReady(int slotIndex, bool ready)
        {
            if (ready) _readySkillSlots.Add(slotIndex);
            else _readySkillSlots.Remove(slotIndex);
        }

        public void SetBattleContext(int stageId, int turn, int hp, int maximumHp, bool canPresent)
        {
            _inBattle = true;
            _stageId = stageId; _turn = turn; _playerHp = hp; _maximumHp = Mathf.Max(1, maximumHp);
            _canPresent = canPresent;
            if (!canPresent && _stepShown)
            {
                _view.Hide();
                _stepShown = false;
            }
        }

        private void LateUpdate()
        {
            var visited = false;
            // Buttonで画面を閉じた場合は、対象消失の判定より先にStepを進める。
            if (_advanceRequested)
            {
                _advanceRequested = false;
                AdvanceStep();
            }
            while (_notices.Count > 0)
            {
                var notice = _notices.Dequeue();
                if (notice.Owner == null || !notice.Owner.activeInHierarchy) continue;
                var origin = notice.Owner.GetComponent<TutorialScope>();
                if (origin != null && origin.Generation != notice.OriginGeneration) continue;
                if (notice.Signal == TutorialSignal.BattleReady) _playedThisBattle.Clear();
                if (notice.Signal == TutorialSignal.TabReady || notice.Signal == TutorialSignal.DialogReady)
                {
                    var visitKey = notice.Signal + ":" + notice.Key;
                    notice.FirstVisit = !_user.UserData.Tutorial.VisitedScopeKeys.Contains(visitKey);
                    if (notice.FirstVisit)
                    {
                        _user.UserData.Tutorial.VisitedScopeKeys.Add(visitKey);
                        _user.MarkDirty();
                        visited = true;
                    }
                }
                if (_active != null && notice.Sequence > _stepSequence)
                {
                    var step = _active.Master.Steps[_stepIndex];
                    if (step.Advance == TutorialAdvance.Signal && step.CompletionSignal == notice.Signal &&
                        (string.IsNullOrEmpty(step.CompletionKey) || step.CompletionKey == notice.Key))
                        AdvanceStep();
                }
                foreach (var master in database.Tutorials)
                {
                    if (master == null || master.Trigger != notice.Signal ||
                        (!string.IsNullOrEmpty(master.TriggerKey) && master.TriggerKey != notice.Key) ||
                        (master.OnlyFirstVisit && !notice.FirstVisit && !RepeatsForDebug(master)) || !Eligible(master)) continue;
                    if (_active != null && _active.Master.TutorialId == master.TutorialId) continue;
                    _candidates[master.TutorialId] = new Candidate { Master = master, Notice = notice, Origin = origin };
                }
            }
            if (visited) _user.UserDataStore.SaveSession();
            if (!_canPresent) return;
            if (_active == null)
            {
                // タブ準備や保存通知がドラッグ中に届いても、指を離すまで開始しない。
                if (Input.GetMouseButton(0) || Input.touchCount > 0) return;
                foreach (var candidate in _candidates.Values.OrderByDescending(item => item.DebugReplay)
                    .ThenByDescending(item => item.Master.Priority)
                    .ThenBy(item => item.Master.TutorialId).ToArray())
                {
                    if (!Eligible(candidate.Master, candidate.DebugReplay) || candidate.Notice.Owner == null ||
                        !candidate.Notice.Owner.activeInHierarchy ||
                        (candidate.Origin != null && candidate.Origin.Generation != candidate.Notice.OriginGeneration))
                    { _candidates.Remove(candidate.Master.TutorialId); continue; }
                    if (!CanStart(candidate)) continue;
                    _candidates.Remove(candidate.Master.TutorialId);
                    _active = candidate;
                    _stepIndex = 0;
                    _waitingSince = Time.unscaledTime;
                    _stepSequence = _sequence;
                    break;
                }
            }
            if (_active == null) return;
            if (_stepShown && _view.HasLostTarget) { Cancel(); return; }
            if (!_stepShown)
            {
                var step = _active.Master.Steps[_stepIndex];
                if (!TryResolveStep(_active, _stepIndex, out var interaction, out var hand, out var alphaHighlight))
                {
                    if (Time.unscaledTime - _waitingSince > step.TargetWaitSeconds) Cancel();
                    return;
                }
                if (_view == null)
                {
                    _view = Instantiate(tutorialPrefab, transform, false);
                    _view.name = "Tutorial";
                }
                _view.Show(step, _highlights, interaction, hand,
                    _user.UserData.Cycle.IsResurgence, () => _advanceRequested = true, alphaHighlight);
                _stepShown = true;
            }
        }

        private bool Eligible(TutorialMasterData master, bool debugReplay = false)
        {
            if (debugReplay) return master.TutorialId > 0 && master.Steps.Count > 0;
            if (!master.IsEnabled || master.TutorialId <= 0 || master.Steps.Count == 0) return false;
            if (!RepeatsForDebug(master) && master.Repeat == TutorialRepeat.OncePerSave &&
                _user.UserData.Tutorial.CompletedTutorialIds.Contains(master.TutorialId)) return false;
            if (master.Repeat == TutorialRepeat.OncePerBattle && (!_inBattle ||
                (!RepeatsForDebug(master) && _playedThisBattle.Contains(master.TutorialId)))) return false;
            foreach (var condition in master.Conditions)
                if (Evaluate(condition) != condition.Expected) return false;
            return true;
        }

        private bool Evaluate(TutorialCondition condition)
        {
            switch (condition.Type)
            {
                case TutorialConditionType.StageCleared: return _user.Progress.IsCleared(condition.Stage.StageId);
                case TutorialConditionType.StageUnlocked: return _user.Progress.IsUnlocked(condition.Stage.StageId);
                case TutorialConditionType.CurrentStage: return _inBattle && _stageId == condition.Stage.StageId;
                case TutorialConditionType.TurnAtLeast: return _inBattle && _turn >= condition.Value;
                case TutorialConditionType.PlayerHpPercentAtMost: return _inBattle && _playerHp * 100f / _maximumHp <= condition.Value;
                case TutorialConditionType.TutorialCompleted: return _user.UserData.Tutorial.CompletedTutorialIds.Contains(condition.Tutorial.TutorialId);
                case TutorialConditionType.IsResurgence: return _user.UserData.Cycle.IsResurgence;
                case TutorialConditionType.HasReachedHigherBeing: return _user.UserData.Cycle.HasReachedHigherBeing;
                case TutorialConditionType.ItemCountAtLeast: return _user.Inventory.GetItemCount(condition.Id) >= condition.Value;
                case TutorialConditionType.BossResolution:
                    return (int)_user.CycleProgress.GetBossResolution(condition.Stage.StageId) == condition.Value;
                case TutorialConditionType.AnyBossRedeemed:
                    return _user.UserData.Cycle.BossResults.Exists(result => result != null && result.Resolution == BossResolution.Redeemed);
                case TutorialConditionType.EquipmentOwned: return _user.Inventory.HasEquipment(condition.Equipment.EquipmentId);
                case TutorialConditionType.TraitNodeLevelAtLeast:
                    return _user.UserData.Ability.NodeLevels.Exists(node => node != null &&
                        node.TreeId == condition.Tree.TreeId && node.TreeNodeId == condition.TreeNodeId && node.Level >= condition.Value);
                case TutorialConditionType.SkillReady: return _inBattle && _readySkillSlots.Contains(condition.Id);
                default: throw new ArgumentOutOfRangeException(nameof(condition.Type));
            }
        }

        private TutorialScope FindScope(string key) => _scopes.LastOrDefault(scope =>
            scope != null && scope.IsReady && scope.isActiveAndEnabled && scope.ScopeKey == key);

        private GameObject Resolve(TutorialTargetReference reference, Candidate candidate)
        {
            if (string.IsNullOrEmpty(reference.TargetKey)) return null;
            var scope = string.IsNullOrEmpty(reference.ScopeKey) ? candidate.Origin : FindScope(reference.ScopeKey);
            if (scope == null && !string.IsNullOrEmpty(candidate.Master.RequiredScopeKey))
                scope = FindScope(candidate.Master.RequiredScopeKey);
            return scope != null && scope.IsReady && scope.isActiveAndEnabled ? scope.Resolve(reference.TargetKey) : null;
        }

        private bool TryResolveStep(Candidate candidate, int index, out GameObject interaction, out RectTransform hand,
            out UnityEngine.UI.RawImage alphaHighlight)
        {
            var step = candidate.Master.Steps[index];
            _highlights.Clear();
            interaction = step.Advance == TutorialAdvance.ScreenTap ? null : Resolve(step.InteractionTarget, candidate);
            hand = Resolve(step.HandTarget, candidate)?.transform as RectTransform;
            alphaHighlight = Resolve(step.AlphaHighlight, candidate)?.GetComponent<UnityEngine.UI.RawImage>();
            if (!string.IsNullOrEmpty(step.AlphaHighlight.TargetKey) &&
                (alphaHighlight == null || !alphaHighlight.isActiveAndEnabled || alphaHighlight.texture == null)) return false;
            if (step.Advance != TutorialAdvance.ScreenTap && !string.IsNullOrEmpty(step.InteractionTarget.TargetKey) && interaction == null) return false;
            if (step.ShowHand && hand == null) return false;
            if (step.Advance == TutorialAdvance.TargetButton && (interaction == null || interaction.GetComponent<UnityEngine.UI.Button>() == null)) return false;
            if (interaction != null)
            {
                var selectable = interaction.GetComponent<UnityEngine.UI.Selectable>();
                if (selectable != null && (!selectable.IsInteractable() || !selectable.enabled)) return false;
                var canvasGroup = interaction.GetComponentsInParent<CanvasGroup>();
                if (canvasGroup.Any(group => !group.blocksRaycasts || group.alpha <= 0)) return false;
            }
            foreach (var reference in step.Highlights)
            {
                var rect = Resolve(reference, candidate)?.transform as RectTransform;
                if (rect == null) return false;
                _highlights.Add(rect);
            }
            return true;
        }

        private void AdvanceStep()
        {
            if (_active == null) return;
            if (_view != null) _view.Hide();
            _stepShown = false;
            _stepIndex++;
            _stepSequence = _sequence;
            _waitingSince = Time.unscaledTime;
            if (_stepIndex < _active.Master.Steps.Count) return;
            var master = _active.Master;
            var debugReplay = _active.DebugReplay;
            _active = null;
            if (debugReplay) return;
            _playedThisBattle.Add(master.TutorialId);
            if (master.Repeat == TutorialRepeat.OncePerSave && !_user.UserData.Tutorial.CompletedTutorialIds.Contains(master.TutorialId))
            {
                _user.UserData.Tutorial.CompletedTutorialIds.Add(master.TutorialId);
                _user.UserDataStore.MarkDirty();
                _user.UserDataStore.SaveSession();
            }
        }

        public void Cancel()
        {
            if (_view != null) _view.Hide();
            _active = null;
            _stepShown = false;
            _advanceRequested = false;
        }

        private void OnDestroy()
        {
            // 保存状態に関わる契機だけは次Sceneへ渡す。閉じたUIや前のBattleの操作は渡さない。
            foreach (var notice in _notices) RememberStateNotice(notice);
            foreach (var candidate in _candidates.Values) RememberStateNotice(candidate.Notice);
            if (_active != null) RememberStateNotice(_active.Notice);
            Cancel();
            _notices.Clear();
            _candidates.Clear();
            if (_user != null) _user.UserDataSaved -= OnUserDataSaved;
            if (_instance == this) _instance = null;
            else if (_instance != null) _instance.ReplayPendingNotices();
        }
    }
}
