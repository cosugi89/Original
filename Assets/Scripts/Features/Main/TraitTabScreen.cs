using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>AbilityタブのTrait Tree表示。配置を保存せず接続定義から実行時に描画する。</summary>
    public sealed class TraitTabScreen : Screen
    {
        [SerializeField] private AbilityTreeMasterData traitTree;
        private AbilityProgressManager _progress;
        private RectTransform _root;
        private Text _pointText;

        private void OnEnable()
        {
            //if (traitTree == null)
            //{
            //    Debug.LogWarning("[AbilityScreen] Trait Tree master is not assigned.");
            //    return;
            //}
            //_progress ??= UserDataManager.EnsureInitialized().GetAbilityProgress(traitTree);
            //Build();
        }

        private void Build()
        {
            if (_root == null)
                CreateRoot();
            for (var i = _root.childCount - 1; i >= 0; i--)
            {
                var child = _root.GetChild(i);
                if (child.gameObject != _pointText?.gameObject && child.name != "Title")
                    Destroy(child.gameObject);
            }

            var derived = _progress.Recalculate();
            _pointText.text = $"Point: {_progress.PointBalance}";
            var content = CreateRect("Tree", _root, Color.clear);
            SetStretch(content, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.85f));
            var cards = new Dictionary<string, RectTransform>(StringComparer.Ordinal);
            var depths = BuildDepths();
            var groups = depths.GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => group.ToArray());
            foreach (var state in derived.NodeStates)
            {
                var card = CreateNodeCard(content, state);
                cards.Add(state.Entry.TreeNodeId, card);
                var depth = depths[state.Entry.TreeNodeId];
                var row = groups[depth];
                var index = Array.FindIndex(row, pair => pair.Key == state.Entry.TreeNodeId);
                card.anchorMin = card.anchorMax = new Vector2((index + 1f) / (row.Length + 1f), 1f - depth * 0.27f);
                card.pivot = new Vector2(0.5f, 0.5f);
                card.sizeDelta = new Vector2(260f, 114f);
            }

            Canvas.ForceUpdateCanvases();
            foreach (var link in traitTree.Links)
            {
                if (link != null && cards.TryGetValue(link.FromTreeNodeId, out var from) && cards.TryGetValue(link.ToTreeNodeId, out var to))
                    CreateLink(content, from, to, link.LinkType == AbilityTreeLinkType.Exclusive);
            }
        }

        private void CreateRoot()
        {
            _root = CreateRect("AbilityTraitTreeRuntime", transform, new Color(0.035f, 0.045f, 0.075f, 0.96f));
            SetStretch(_root, Vector2.zero, Vector2.one);
            _root.SetAsLastSibling();
            var title = CreateText("Title", _root, 30, TextAnchor.MiddleLeft);
            title.text = traitTree.DisplayName + " Tree";
            SetStretch((RectTransform)title.transform, new Vector2(0.05f, 0.87f), new Vector2(0.7f, 0.98f));
            _pointText = CreateText("Point", _root, 26, TextAnchor.MiddleRight);
            SetStretch((RectTransform)_pointText.transform, new Vector2(0.7f, 0.87f), new Vector2(0.95f, 0.98f));
        }

        private RectTransform CreateNodeCard(RectTransform parent, AbilityTreeNodeState state)
        {
            var color = state.IsPurchasable ? new Color(0.15f, 0.37f, 0.25f, 0.98f) : state.CurrentLevel > 0 ? new Color(0.18f, 0.28f, 0.48f, 0.98f) : new Color(0.16f, 0.16f, 0.2f, 0.98f);
            var card = CreateRect(state.Entry.TreeNodeId, parent, color);
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card.GetComponent<Image>();
            button.interactable = state.IsPurchasable;
            button.onClick.AddListener(() => { _progress.TryPurchase(state.Entry.TreeNodeId); Build(); });
            var icon = CreateRect("Icon", card, Color.white);
            icon.GetComponent<Image>().sprite = state.Entry.Node.Icon;
            icon.GetComponent<Image>().preserveAspect = true;
            icon.anchorMin = new Vector2(0.03f, 0.28f); icon.anchorMax = new Vector2(0.25f, 0.86f); icon.offsetMin = icon.offsetMax = Vector2.zero;
            var name = CreateText("Name", card, 18, TextAnchor.UpperLeft);
            name.text = state.Entry.Node.DisplayName;
            SetStretch((RectTransform)name.transform, new Vector2(0.29f, 0.62f), new Vector2(0.97f, 0.92f));
            var level = CreateText("Level", card, 15, TextAnchor.UpperLeft);
            level.text = $"Lv. {state.CurrentLevel} / {state.MaxLevel}    Next: {state.NextRankPointCost} Point";
            SetStretch((RectTransform)level.transform, new Vector2(0.29f, 0.36f), new Vector2(0.97f, 0.64f));
            var reason = CreateText("State", card, 13, TextAnchor.UpperLeft);
            reason.text = state.IsPurchasable ? "購入する" : state.LockedReasonText;
            reason.color = state.IsPurchasable ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.75f, 0.65f);
            SetStretch((RectTransform)reason.transform, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.35f));
            return card;
        }

        private static void CreateLink(RectTransform parent, RectTransform from, RectTransform to, bool exclusive)
        {
            var line = CreateRect(exclusive ? "Exclusive Link" : "Prerequisite Link", parent, exclusive ? new Color(0.82f, 0.3f, 0.3f, 0.9f) : new Color(0.55f, 0.78f, 1f, 0.75f));
            line.SetAsFirstSibling();
            var start = (Vector2)from.localPosition + new Vector2(0f, -from.rect.height * 0.5f);
            var end = (Vector2)to.localPosition + new Vector2(0f, to.rect.height * 0.5f);
            var delta = end - start;
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 1f); line.pivot = new Vector2(0f, 0.5f);
            line.anchoredPosition = start; line.sizeDelta = new Vector2(delta.magnitude, exclusive ? 4f : 3f);
            line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private Dictionary<string, int> BuildDepths()
        {
            var result = traitTree.NodeEntries.Where(entry => entry != null && entry.Node != null).ToDictionary(entry => entry.TreeNodeId, _ => 0, StringComparer.Ordinal);
            for (var pass = 0; pass < result.Count; pass++)
                foreach (var link in traitTree.Links)
                    if (link != null && link.LinkType == AbilityTreeLinkType.Prerequisite && result.ContainsKey(link.FromTreeNodeId) && result.ContainsKey(link.ToTreeNodeId))
                        result[link.ToTreeNodeId] = Mathf.Max(result[link.ToTreeNodeId], result[link.FromTreeNodeId] + 1);
            return result;
        }

        private static RectTransform CreateRect(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false); gameObject.GetComponent<Image>().color = color;
            return (RectTransform)gameObject.transform;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.color = Color.white;
            return text;
        }

        private static void SetStretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
