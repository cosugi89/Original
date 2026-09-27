using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public enum AbilityNodeLockReason
    {
        None = 0,
        MaxLevel = 1,
        InsufficientPoints = 2,
        MissingPrerequisites = 3,
        ExclusiveBranch = 4,
        ImpossiblePrerequisites = 5,
        InvalidTreeNode = 6,
    }

    public sealed class AbilityTreeNodeState
    {
        public AbilityTreeNodeEntry Entry { get; init; }
        public bool IsStart { get; init; }
        public int CurrentLevel { get; init; }
        public int MaxLevel { get; init; }
        public bool IsPurchasable { get; init; }
        public bool IsExclusiveClosed { get; init; }
        public AbilityNodeLockReason LockReason { get; init; }
        public string LockedReasonText { get; init; }
        public int NextRankPointCost { get; init; }
    }

    public sealed class AbilityDerivedData
    {
        public IReadOnlyList<AbilityTreeNodeState> NodeStates { get; init; } = Array.Empty<AbilityTreeNodeState>();
        public IReadOnlyList<BattleStatModifierDefinitionEntry> BattleStatModifiers { get; init; } =
            Array.Empty<BattleStatModifierDefinitionEntry>();
    }

    public sealed class AbilityPurchaseResult
    {
        public bool IsPurchased { get; init; }
        public AbilityNodeLockReason FailureReason { get; init; }
        public string Message { get; init; }
        public AbilityDerivedData DerivedData { get; init; }
    }

    /// <summary>
    /// 指定TreeのAbility進捗、購入、導出を一箇所で扱う。保存の正本はNode Levelだけである。
    /// </summary>
    public sealed class AbilityProgressManager
    {
        public UserDataJsonStore UserDataStore { get; }
        public AbilityTreeMasterData Tree { get; }
        private readonly InventoryManager _inventory;
        public AbilityDerivedData DerivedData { get; private set; } = new();

        public int PointBalance => _inventory.GetItemCount(InventoryItemIds.AbilityPoint);

        public AbilityProgressManager(UserDataJsonStore userDataStore, InventoryManager inventory, AbilityTreeMasterData tree)
        {
            UserDataStore = userDataStore ?? throw new ArgumentNullException(nameof(userDataStore));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Tree = tree ?? throw new ArgumentNullException(nameof(tree));
            NormalizeProgress();
            Recalculate();
        }

        public AbilityDerivedData Recalculate()
        {
            var entriesById = BuildEntriesById();
            var levels = GetKnownLevels(entriesById);
            var states = new List<AbilityTreeNodeState>(entriesById.Count);
            var modifiers = new List<BattleStatModifierDefinitionEntry>();

            foreach (var entry in Tree.NodeEntries)
            {
                if (entry == null || entry.Node == null || !entriesById.ContainsKey(entry.TreeNodeId))
                    continue;

                var level = levels.TryGetValue(entry.TreeNodeId, out var savedLevel) ? savedLevel : 0;
                var maxLevel = entry.Node.MaxLevel;
                var isStart = IsStartNode(entry.TreeNodeId);
                var exclusiveClosed = level == 0 && IsExclusiveClosed(entry.TreeNodeId, levels);
                var impossible = level == 0 && HasImpossibleAllPrerequisites(entry.TreeNodeId);
                var prerequisitesMet = ArePrerequisitesMet(entry, levels);
                // 開始Nodeは初期解放用の入口であり、Pointを要求しない。
                // コストも0として導出し、購入処理とUI表示を一致させる。
                var nextCost = isStart || level >= maxLevel ? 0 : entry.Node.Ranks[level]?.PointCost ?? 0;
                var reason = ResolveLockReason(level, maxLevel, exclusiveClosed, impossible, prerequisitesMet, nextCost);

                states.Add(new AbilityTreeNodeState
                {
                    Entry = entry,
                    IsStart = isStart,
                    CurrentLevel = level,
                    MaxLevel = maxLevel,
                    IsPurchasable = reason == AbilityNodeLockReason.None,
                    IsExclusiveClosed = exclusiveClosed,
                    LockReason = reason,
                    LockedReasonText = BuildLockReasonText(entry, reason),
                    NextRankPointCost = nextCost,
                });

                for (var rankIndex = 0; rankIndex < level && rankIndex < entry.Node.Ranks.Count; rankIndex++)
                {
                    var rank = entry.Node.Ranks[rankIndex];
                    if (rank == null)
                        continue;

                    for (var modifierIndex = 0; modifierIndex < rank.BattleStatModifiers.Count; modifierIndex++)
                    {
                        var modifier = rank.BattleStatModifiers[modifierIndex];
                        if (modifier != null)
                            modifiers.Add(modifier);
                    }
                }
            }

            DerivedData = new AbilityDerivedData
            {
                NodeStates = states,
                BattleStatModifiers = modifiers,
            };
            return DerivedData;
        }

        public AbilityPurchaseResult TryPurchase(string treeNodeId)
        {
            var state = Recalculate().NodeStates.FirstOrDefault(candidate =>
                string.Equals(candidate.Entry.TreeNodeId, treeNodeId?.Trim(), StringComparison.Ordinal));
            if (state == null)
            {
                return Failed(AbilityNodeLockReason.InvalidTreeNode, "存在しないTree Nodeです。");
            }

            if (!state.IsPurchasable)
            {
                return Failed(state.LockReason, state.LockedReasonText);
            }

            var progress = Progress;
            if (!_inventory.TryConsumeItem(InventoryItemIds.AbilityPoint, state.NextRankPointCost))
            {
                return Failed(AbilityNodeLockReason.InsufficientPoints, "Ability Pointが不足しています。");
            }

            progress.NodeLevels.Add(new AbilityTreeNodeLevelData
            {
                TreeId = Tree.TreeId,
                TreeNodeId = state.Entry.TreeNodeId,
                Level = state.CurrentLevel + 1,
            });
            NormalizeProgress();
            var derived = Recalculate();
            UserDataStore.SaveSession();
            return new AbilityPurchaseResult { IsPurchased = true, DerivedData = derived };
        }

        private AbilityPurchaseResult Failed(AbilityNodeLockReason reason, string message)
        {
            return new AbilityPurchaseResult
            {
                FailureReason = reason,
                Message = message,
                DerivedData = DerivedData,
            };
        }

        private AbilityProgressData Progress
        {
            get
            {
                UserDataStore.Session.UserData.Ability ??= new AbilityProgressData();
                UserDataStore.Session.UserData.Ability.NodeLevels ??= new List<AbilityTreeNodeLevelData>();
                return UserDataStore.Session.UserData.Ability;
            }
        }

        private void NormalizeProgress()
        {
            var progress = Progress;
            var changed = false;
            var entriesById = BuildEntriesById();
            var levels = progress.NodeLevels;
            var merged = new Dictionary<string, AbilityTreeNodeLevelData>(StringComparer.Ordinal);
            var normalized = new List<AbilityTreeNodeLevelData>();

            // Tree外のデータは将来Tree/旧マスターへ戻った時のため保持する。
            for (var i = 0; i < levels.Count; i++)
            {
                var item = levels[i];
                if (item == null || string.IsNullOrWhiteSpace(item.TreeId) || string.IsNullOrWhiteSpace(item.TreeNodeId))
                {
                    changed = true;
                    continue;
                }

                item.TreeId = item.TreeId.Trim();
                item.TreeNodeId = item.TreeNodeId.Trim();
                if (item.Level <= 0)
                {
                    changed = true;
                    continue;
                }
                if (!string.Equals(item.TreeId, Tree.TreeId, StringComparison.Ordinal) || !entriesById.TryGetValue(item.TreeNodeId, out var entry))
                {
                    normalized.Add(item);
                    continue;
                }

                var clampedLevel = Mathf.Clamp(item.Level, 0, entry.Node.MaxLevel);
                if (clampedLevel <= 0)
                {
                    changed = true;
                    continue;
                }

                var key = item.TreeId + "\u001f" + item.TreeNodeId;
                if (merged.TryGetValue(key, out var existing))
                {
                    existing.Level = Mathf.Max(existing.Level, clampedLevel);
                    changed = true;
                    continue;
                }

                item.Level = clampedLevel;
                merged.Add(key, item);
                normalized.Add(item);
            }

            ResolveExclusiveConflicts(normalized, entriesById);
            normalized.RemoveAll(item => string.Equals(item.TreeId, Tree.TreeId, StringComparison.Ordinal) && item.Level <= 0);
            if (changed || !levels.SequenceEqual(normalized))
            {
                levels.Clear();
                levels.AddRange(normalized);
                UserDataStore.MarkDirty();
            }
        }

        private void ResolveExclusiveConflicts(
            List<AbilityTreeNodeLevelData> levels,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            var activeIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (level.Level <= 0 || !string.Equals(level.TreeId, Tree.TreeId, StringComparison.Ordinal) ||
                    !entriesById.ContainsKey(level.TreeNodeId))
                    continue;

                var hasConflict = false;
                for (var linkIndex = 0; linkIndex < Tree.Links.Count; linkIndex++)
                {
                    var link = Tree.Links[linkIndex];
                    if (link == null || link.LinkType != AbilityTreeLinkType.Exclusive)
                        continue;

                    var otherId = link.FromTreeNodeId == level.TreeNodeId ? link.ToTreeNodeId :
                        link.ToTreeNodeId == level.TreeNodeId ? link.FromTreeNodeId : null;
                    if (otherId != null && activeIds.Contains(otherId))
                    {
                        hasConflict = true;
                        break;
                    }
                }

                if (hasConflict)
                {
                    Debug.LogWarning($"[AbilityProgressManager] 排他矛盾を補正しました: {Tree.TreeId}/{level.TreeNodeId}");
                    level.Level = 0;
                }
                else
                {
                    activeIds.Add(level.TreeNodeId);
                }
            }
        }

        private Dictionary<string, AbilityTreeNodeEntry> BuildEntriesById()
        {
            var result = new Dictionary<string, AbilityTreeNodeEntry>(StringComparer.Ordinal);
            foreach (var entry in Tree.NodeEntries)
            {
                if (entry == null || entry.Node == null || string.IsNullOrWhiteSpace(entry.TreeNodeId))
                    continue;
                if (!result.TryAdd(entry.TreeNodeId, entry))
                    Debug.LogWarning($"[AbilityProgressManager] 重複TreeNodeIdを無視しました: {Tree.TreeId}/{entry.TreeNodeId}");
            }
            return result;
        }

        private Dictionary<string, int> GetKnownLevels(IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var level in Progress.NodeLevels)
            {
                if (level == null || !string.Equals(level.TreeId, Tree.TreeId, StringComparison.Ordinal) ||
                    !entriesById.TryGetValue(level.TreeNodeId, out var entry))
                    continue;
                result[level.TreeNodeId] = Mathf.Max(result.TryGetValue(level.TreeNodeId, out var current) ? current : 0,
                    Mathf.Clamp(level.Level, 0, entry.Node.MaxLevel));
            }
            return result;
        }

        private bool ArePrerequisitesMet(AbilityTreeNodeEntry entry, IReadOnlyDictionary<string, int> levels)
        {
            var prerequisites = Tree.Links.Where(link => link != null && link.LinkType == AbilityTreeLinkType.Prerequisite &&
                                                         link.ToTreeNodeId == entry.TreeNodeId).ToArray();
            if (prerequisites.Length == 0)
                return true;

            return entry.PrerequisiteMode == AbilityPrerequisiteMode.All
                ? prerequisites.All(link => levels.TryGetValue(link.FromTreeNodeId, out var level) && level >= link.RequiredLevel)
                : prerequisites.Any(link => levels.TryGetValue(link.FromTreeNodeId, out var level) && level >= link.RequiredLevel);
        }

        private bool IsStartNode(string treeNodeId)
        {
            return !Tree.Links.Any(link => link != null &&
                link.LinkType == AbilityTreeLinkType.Prerequisite &&
                link.ToTreeNodeId == treeNodeId);
        }

        private bool IsExclusiveClosed(string treeNodeId, IReadOnlyDictionary<string, int> levels)
        {
            return Tree.Links.Any(link => link != null && link.LinkType == AbilityTreeLinkType.Exclusive &&
                ((link.FromTreeNodeId == treeNodeId && levels.TryGetValue(link.ToTreeNodeId, out var fromLevel) && fromLevel > 0) ||
                 (link.ToTreeNodeId == treeNodeId && levels.TryGetValue(link.FromTreeNodeId, out var toLevel) && toLevel > 0)));
        }

        private bool HasImpossibleAllPrerequisites(string treeNodeId)
        {
            var entry = Tree.FindNodeEntry(treeNodeId);
            if (entry == null || entry.PrerequisiteMode != AbilityPrerequisiteMode.All)
                return false;

            var ids = Tree.Links.Where(link => link != null && link.LinkType == AbilityTreeLinkType.Prerequisite && link.ToTreeNodeId == treeNodeId)
                .Select(link => link.FromTreeNodeId).Distinct().ToArray();
            return ids.Any(left => ids.Any(right => left != right && Tree.Links.Any(link => link != null &&
                link.LinkType == AbilityTreeLinkType.Exclusive &&
                ((link.FromTreeNodeId == left && link.ToTreeNodeId == right) || (link.FromTreeNodeId == right && link.ToTreeNodeId == left)))));
        }

        private AbilityNodeLockReason ResolveLockReason(int level, int maxLevel, bool exclusiveClosed, bool impossible, bool prerequisitesMet, int nextCost)
        {
            if (level >= maxLevel)
                return AbilityNodeLockReason.MaxLevel;
            if (exclusiveClosed)
                return AbilityNodeLockReason.ExclusiveBranch;
            if (impossible)
                return AbilityNodeLockReason.ImpossiblePrerequisites;
            if (!prerequisitesMet)
                return AbilityNodeLockReason.MissingPrerequisites;
            return PointBalance < nextCost ? AbilityNodeLockReason.InsufficientPoints : AbilityNodeLockReason.None;
        }

        private string BuildLockReasonText(AbilityTreeNodeEntry entry, AbilityNodeLockReason reason)
        {
            return reason switch
            {
                AbilityNodeLockReason.None => "購入できます。",
                AbilityNodeLockReason.MaxLevel => "最大Levelです。",
                AbilityNodeLockReason.InsufficientPoints => "Pointが不足しています。",
                AbilityNodeLockReason.ExclusiveBranch => "排他の分岐を選択済みです。",
                AbilityNodeLockReason.ImpossiblePrerequisites => "排他Nodeを同時に必要とするため解放できません。",
                AbilityNodeLockReason.MissingPrerequisites => entry.PrerequisiteMode == AbilityPrerequisiteMode.Any
                    ? "いずれかの前提Nodeを必要Levelまで上げてください。"
                    : "すべての前提Nodeを必要Levelまで上げてください。",
                _ => "無効なTree Nodeです。",
            };
        }
    }
}
