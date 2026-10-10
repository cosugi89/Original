using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum InsightEffectType
    {
        MaxHp = 0,
        Attack = 1,
        Defense = 2,
        SkillChargeGain = 3,
    }

    [Serializable]
    public sealed class InsightEffectEntry
    {
        [SerializeField] private InsightEffectType effectType;
        [SerializeField] private int value;

        public InsightEffectType EffectType => effectType;
        public int Value => value;
    }

    /// <summary>
    /// 人間ボス1人に紐付く Insight と Remnant 表示情報の定義。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Boss Insight", fileName = "BossInsightMasterData")]
    public sealed class BossInsightMasterData : ScriptableObject
    {
        [SerializeField, Tooltip("対応する人間ボス Mission の StageId。進捗保存との照合に使う。")]
        private int stageId;
        [SerializeField, Tooltip("Progress カードに表示するボス名。")]
        private string bossName = string.Empty;
        [SerializeField, Tooltip("通常撃破時に Progress カードへ表示する、赤目を含むボス Portrait Sprite。")]
        private Sprite defeatedPortrait;
        [SerializeField, Tooltip("救済済み時に Progress カードへ表示する、救済後のボス Portrait Sprite。")]
        private Sprite redeemedPortrait;
        [SerializeField, Tooltip("一覧での並び順。小さいほど先に表示する。")]
        private int sortOrder;
        [SerializeField, Tooltip("ボス撃破または救済時に自動解放する Insight 名。")]
        private string insightName = string.Empty;
        [SerializeField, TextArea, Tooltip("Insight の短い説明。")]
        private string insightDescription = string.Empty;
        [SerializeField, Tooltip("Insight カード右レールに表示するアイコン。")]
        private Sprite insightIcon;
        [SerializeField, Tooltip("Insight の恒久効果一覧。")]
        private List<InsightEffectEntry> effects = new();
        [SerializeField, TextArea, Tooltip("Remnant で一周目の記録として表示するボス台詞。")]
        private string remnantQuote = string.Empty;

        public int StageId => stageId;
        public string BossName => bossName ?? string.Empty;
        public Sprite DefeatedPortrait => defeatedPortrait;
        public Sprite RedeemedPortrait => redeemedPortrait;
        public int SortOrder => sortOrder;
        public string InsightName => insightName ?? string.Empty;
        public string InsightDescription => insightDescription ?? string.Empty;
        public Sprite InsightIcon => insightIcon;
        public IReadOnlyList<InsightEffectEntry> Effects => effects ??= new List<InsightEffectEntry>();
        public string RemnantQuote => remnantQuote ?? string.Empty;
    }
}
