using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using LayerLab.ArtMakerUnity;

namespace Assets.Scripts.Data.DTO
{
    public sealed class BattleEquipmentDerivedData
    {
        public IReadOnlyList<BattleStatModifierData> BattleStatModifiers { get; init; } =
            Array.Empty<BattleStatModifierData>();

        public IReadOnlyList<BattleStatModifierData> TagBonusStatModifiers { get; init; } =
            Array.Empty<BattleStatModifierData>();

        public IReadOnlyList<string> CategoryTags { get; init; } =
            Array.Empty<string>();

        public int TagBonusEligibleEquipmentCount { get; init; } = 0;

        public IReadOnlyList<BattleEquipmentTagMatchData> TagMatchCandidates { get; init; } =
            Array.Empty<BattleEquipmentTagMatchData>();

        public IReadOnlyList<BattleEquipmentTagMatchData> TagBonusMatches { get; init; } =
            Array.Empty<BattleEquipmentTagMatchData>();
    }

    public sealed class BattleEquipmentTagMatchData
    {
        public string Tag { get; init; } = string.Empty;
        public int EquipmentCount { get; init; } = 0;
        public int EligibleEquipmentCount { get; init; } = 0;
        public bool CoversAllEligibleEquipment { get; init; }
        public bool IsBonusConditionMet { get; init; }
    }

    public static class BattleEquipmentDerivedDataResolver
    {
        private const int MinTagMatchCandidateEquipmentCount = 2;
        private const float DefaultTagBonusAttackMultiplier = 1.1f;

        public static BattleEquipmentDerivedData Resolve(IReadOnlyList<EquipmentData> equipmentDefinitions)
        {
            if (equipmentDefinitions == null || equipmentDefinitions.Count == 0)
            {
                return new BattleEquipmentDerivedData();
            }

            var battleStatModifiers = new List<BattleStatModifierData>();
            var categoryTags = new List<string>();
            var seenCategoryTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tagEquipmentCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var tagBonusEligibleEquipmentCount = 0;

            for (var i = 0; i < equipmentDefinitions.Count; i++)
            {
                var definition = equipmentDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                AddBattleStatModifiers(definition, battleStatModifiers);

                if (!IsTagBonusEligibleEquipment(definition))
                {
                    continue;
                }

                tagBonusEligibleEquipmentCount++;
                AddCategoryTags(
                    definition,
                    categoryTags,
                    seenCategoryTags,
                    tagEquipmentCounts);
            }

            var tagMatchCandidates = BuildTagMatchCandidates(
                tagEquipmentCounts,
                tagBonusEligibleEquipmentCount);
            var tagBonusMatches = BuildTagBonusMatches(tagMatchCandidates);
            var tagBonusStatModifiers = BuildTagBonusStatModifiers(tagBonusMatches);
            var resolvedBattleStatModifiers = MergeBattleStatModifiers(
                battleStatModifiers,
                tagBonusStatModifiers);

            return new BattleEquipmentDerivedData
            {
                BattleStatModifiers = resolvedBattleStatModifiers,
                TagBonusStatModifiers = tagBonusStatModifiers,
                CategoryTags = categoryTags.Count > 0
                    ? categoryTags.ToArray()
                    : Array.Empty<string>(),
                TagBonusEligibleEquipmentCount = tagBonusEligibleEquipmentCount,
                TagMatchCandidates = tagMatchCandidates,
                TagBonusMatches = tagBonusMatches,
            };
        }

        private static void AddBattleStatModifiers(
            EquipmentData definition,
            List<BattleStatModifierData> battleStatModifiers)
        {
            if (definition.BattleStatModifiers == null || definition.BattleStatModifiers.Count == 0)
            {
                return;
            }

            for (var modifierIndex = 0; modifierIndex < definition.BattleStatModifiers.Count; modifierIndex++)
            {
                var modifier = definition.BattleStatModifiers[modifierIndex];
                if (modifier != null)
                {
                    battleStatModifiers.Add(modifier);
                }
            }
        }

        private static bool IsTagBonusEligibleEquipment(EquipmentData definition)
        {
            return definition.PartType == PartsType.Helmet ||
                   definition.PartType == PartsType.Chest ||
                   definition.ExclusiveGroup == PartsExclusiveGroup.HandRight ||
                   definition.ExclusiveGroup == PartsExclusiveGroup.HandLeft;
        }

        private static void AddCategoryTags(
            EquipmentData definition,
            List<string> categoryTags,
            HashSet<string> seenCategoryTags,
            Dictionary<string, int> tagEquipmentCounts)
        {
            var normalizedTags = BuildDistinctTags(definition.CategoryTags);
            for (var tagIndex = 0; tagIndex < normalizedTags.Count; tagIndex++)
            {
                var tag = normalizedTags[tagIndex];
                if (seenCategoryTags.Add(tag))
                {
                    categoryTags.Add(tag);
                }

                tagEquipmentCounts.TryGetValue(tag, out var count);
                tagEquipmentCounts[tag] = count + 1;
            }
        }

        private static IReadOnlyList<string> BuildDistinctTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            var seenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var tagIndex = 0; tagIndex < tags.Count; tagIndex++)
            {
                var tag = tags[tagIndex]?.Trim();
                if (string.IsNullOrEmpty(tag) || !seenTags.Add(tag))
                {
                    continue;
                }

                result.Add(tag);
            }

            return result.Count > 0 ? result : Array.Empty<string>();
        }

        private static IReadOnlyList<BattleEquipmentTagMatchData> BuildTagMatchCandidates(
            Dictionary<string, int> tagEquipmentCounts,
            int eligibleEquipmentCount)
        {
            if (tagEquipmentCounts == null || tagEquipmentCounts.Count == 0)
            {
                return Array.Empty<BattleEquipmentTagMatchData>();
            }

            var result = new List<BattleEquipmentTagMatchData>();
            foreach (var pair in tagEquipmentCounts)
            {
                if (pair.Value < MinTagMatchCandidateEquipmentCount)
                {
                    continue;
                }

                var coversAllEligibleEquipment = CoversAllEligibleEquipment(
                    pair.Value,
                    eligibleEquipmentCount);
                result.Add(new BattleEquipmentTagMatchData
                {
                    Tag = pair.Key,
                    EquipmentCount = pair.Value,
                    EligibleEquipmentCount = eligibleEquipmentCount,
                    CoversAllEligibleEquipment = coversAllEligibleEquipment,
                    IsBonusConditionMet = IsTagBonusConditionMet(
                        pair.Value,
                        eligibleEquipmentCount),
                });
            }

            if (result.Count == 0)
            {
                return Array.Empty<BattleEquipmentTagMatchData>();
            }

            result.Sort(CompareTagMatchCandidate);
            return result.ToArray();
        }

        private static IReadOnlyList<BattleEquipmentTagMatchData> BuildTagBonusMatches(
            IReadOnlyList<BattleEquipmentTagMatchData> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return Array.Empty<BattleEquipmentTagMatchData>();
            }

            var result = new List<BattleEquipmentTagMatchData>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate != null && candidate.IsBonusConditionMet)
                {
                    result.Add(candidate);
                }
            }

            return result.Count > 0
                ? result.ToArray()
                : Array.Empty<BattleEquipmentTagMatchData>();
        }

        private static IReadOnlyList<BattleStatModifierData> BuildTagBonusStatModifiers(
            IReadOnlyList<BattleEquipmentTagMatchData> tagBonusMatches)
        {
            if (tagBonusMatches == null || tagBonusMatches.Count == 0)
            {
                return Array.Empty<BattleStatModifierData>();
            }

            var result = new List<BattleStatModifierData>();
            for (var i = 0; i < tagBonusMatches.Count; i++)
            {
                if (tagBonusMatches[i] == null)
                {
                    continue;
                }

                result.Add(new BattleStatModifierData
                {
                    TargetType = BattleStatModifierTargetType.Parameter,
                    ParameterType = BattleSkillParameterType.Attack,
                    OperationType = BattleStatModifierOperationType.Multiply,
                    Value = DefaultTagBonusAttackMultiplier,
                });
            }

            return result.Count > 0 ? result.ToArray() : Array.Empty<BattleStatModifierData>();
        }

        private static IReadOnlyList<BattleStatModifierData> MergeBattleStatModifiers(
            IReadOnlyList<BattleStatModifierData> equipmentModifiers,
            IReadOnlyList<BattleStatModifierData> tagBonusModifiers)
        {
            var result = new List<BattleStatModifierData>();
            AddModifiers(equipmentModifiers, result);
            AddModifiers(tagBonusModifiers, result);
            return result.Count > 0 ? result.ToArray() : Array.Empty<BattleStatModifierData>();
        }

        private static void AddModifiers(
            IReadOnlyList<BattleStatModifierData> source,
            List<BattleStatModifierData> destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    destination.Add(source[i]);
                }
            }
        }

        private static bool IsTagBonusConditionMet(
            int equipmentCount,
            int eligibleEquipmentCount)
        {
            return eligibleEquipmentCount >= MinTagMatchCandidateEquipmentCount &&
                   CoversAllEligibleEquipment(equipmentCount, eligibleEquipmentCount);
        }

        private static bool CoversAllEligibleEquipment(
            int equipmentCount,
            int eligibleEquipmentCount)
        {
            return eligibleEquipmentCount > 0 && equipmentCount >= eligibleEquipmentCount;
        }

        private static int CompareTagMatchCandidate(
            BattleEquipmentTagMatchData left,
            BattleEquipmentTagMatchData right)
        {
            var countComparison = right.EquipmentCount.CompareTo(left.EquipmentCount);
            if (countComparison != 0)
            {
                return countComparison;
            }

            return string.Compare(left.Tag, right.Tag, StringComparison.OrdinalIgnoreCase);
        }
    }
}
