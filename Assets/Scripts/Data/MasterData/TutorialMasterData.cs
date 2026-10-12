using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum TutorialSignal
    {
        None, HomeReady, TabReady, DialogReady, UserDataChanged, BattleReady,
        TurnReady, PathConfirmed, SkillSelected, SkillUsed, AttackResolved,
        GoalResolved, HazardHit, TurnResolved, BattleEnded, PathRejected,
        ResurgenceStarted, BossResultRecorded, HigherBeingReached, Manual
    }

    public enum TutorialRepeat { OncePerSave, OncePerBattle, EveryOccurrence }
    public enum TutorialAdvance { TargetButton = 1, Signal = 2, ScreenTap = 3 }
    public enum TutorialConditionType
    {
        StageCleared, StageUnlocked, CurrentStage, TurnAtLeast, PlayerHpPercentAtMost,
        TutorialCompleted, IsResurgence, HasReachedHigherBeing, ItemCountAtLeast, BossResolution, AnyBossRedeemed,
        EquipmentOwned, TraitNodeLevelAtLeast, SkillReady
    }

    [Serializable]
    public sealed class TutorialCondition
    {
        public TutorialConditionType Type;
        public StageMasterData Stage;
        public TutorialMasterData Tutorial;
        public EquipmentMasterData Equipment;
        public AbilityTreeMasterData Tree;
        [Tooltip("TraitNodeLevelAtLeast用。既存AbilityのTree配置Nodeの固定ID。")]
        public string TreeNodeId = "";
        [Tooltip("ItemCountAtLeastではItem ID。SkillReadyでは0始まりのSlot番号。")]
        public int Id;
        [Tooltip("ターン数、HP割合(0〜100)、所持数、BossResolutionの値、Traitの必要Level。")]
        public int Value = 1;
        [Tooltip("条件結果がこの値と一致すると成立。falseで未クリア等を表す。")]
        public bool Expected = true;
    }

    [Serializable]
    public sealed class TutorialTargetReference
    {
        [Tooltip("空なら開始元のScope。別画面へ進むStepではそのScopeKeyを指定。")]
        public string ScopeKey = "";
        public string TargetKey = "";
    }

    [Serializable]
    public sealed class TutorialStep
    {
        [TextArea(2, 6)] public string Message = "";
        public string Speaker = "";
        public bool MessageAtBottom;
        [TextArea(2, 6), Tooltip("任意。Resurgence中の差分。TMPのrich textで赤字等を指定可能。")]
        public string ResurgenceMessage = "";
        public List<TutorialTargetReference> Highlights = new();
        [Tooltip("任意。RawImageのTextureアルファに沿って暗幕を抜く対象。矩形のHighlightsとは別。")]
        public TutorialTargetReference AlphaHighlight = new();
        public TutorialTargetReference InteractionTarget = new();
        public TutorialTargetReference HandTarget = new();
        public bool ShowHand;
        [Tooltip("盤面のトレース等で子UIとドラッグを許可する。通常のButtonではfalse。")]
        public bool AllowDescendantsAndDrag;
        public Vector2 HighlightPadding = new(12, 12);
        public Vector2 HandOffset = new(25, -45);
        public TutorialAdvance Advance = TutorialAdvance.ScreenTap;
        public TutorialSignal CompletionSignal;
        [Tooltip("空なら任意のKey。対象タブ、ダイアログ、Skillスロット等の通知Key。")]
        public string CompletionKey = "";
        [Min(0.1f), Tooltip("画面遷移後に次の対象を待つ上限。失効時は未完了で中断。")]
        public float TargetWaitSeconds = 10;
    }

    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Tutorial", fileName = "TutorialMasterData")]
    public sealed class TutorialMasterData : ScriptableObject
    {
        [Min(1)] public int TutorialId;
        public bool IsEnabled = true;
        [Tooltip("Editor / Development Buildで完了記録を無視して繰り返す初期設定。通常ビルドには適用しない。")]
        public bool RepeatForDebug;
        public TutorialSignal Trigger;
        [Tooltip("通知Keyによる絞り込み。空なら任意。")]
        public string TriggerKey = "";
        [Tooltip("UserDataChanged等の通知でも、このScopeが表示中の時だけ開始する。")]
        public string RequiredScopeKey = "";
        public bool OnlyFirstVisit;
        public TutorialRepeat Repeat = TutorialRepeat.OncePerSave;
        public int Priority;
        public List<TutorialCondition> Conditions = new();
        public List<TutorialStep> Steps = new();
    }
}
