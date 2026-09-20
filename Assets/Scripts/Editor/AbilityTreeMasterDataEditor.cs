#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(AbilityNodeMasterData))]
    public sealed class AbilityNodeMasterDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeId"), new GUIContent("Node識別子", "Node種類を示す不変IDです。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("表示名"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("説明"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent("アイコン"));
            var progressionType = serializedObject.FindProperty("progressionType");
            var progressionIndex = EditorGUILayout.Popup("成長タイプ", progressionType.enumValueIndex, new[] { "一度だけ解放", "段階強化" });
            progressionType.enumValueIndex = progressionIndex;
            DrawRanks(serializedObject.FindProperty("ranks"));
            serializedObject.ApplyModifiedProperties();
            AbilityTreeMasterDataEditor.DrawValidation(
                target as AbilityNodeMasterData,
                "このNode定義では問題は見つかっていません。");
        }

        private static void DrawRanks(SerializedProperty ranks)
        {
            EditorGUILayout.LabelField("ランク一覧", EditorStyles.boldLabel);
            for (var i = 0; i < ranks.arraySize; i++)
            {
                var rank = ranks.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"ランク {i + 1}", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(rank.FindPropertyRelative("pointCost"), new GUIContent("必要ポイント"));
                    EditorGUILayout.PropertyField(rank.FindPropertyRelative("battleStatModifiers"), new GUIContent("能力値補正"), true);
                    if (GUILayout.Button("このランクを削除"))
                    {
                        ranks.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            if (GUILayout.Button("ランクを追加")) ranks.arraySize++;
        }
    }

    [CustomEditor(typeof(AbilityTreeMasterData))]
    public sealed class AbilityTreeMasterDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (target is not AbilityTreeMasterData tree)
            {
                return;
            }

            DrawValidation(tree, "このTree定義では問題は見つかっていません。");

            EditorGUILayout.Space();
            if (GUILayout.Button("Tree編集ウィンドウを開く"))
            {
                AbilityTreeBuilderWindow.Open(tree);
            }
        }

        internal static void DrawValidation(ValidatedMasterDataAsset asset, string noIssueMessage)
        {
            if (asset == null)
            {
                return;
            }

            var issues = asset.GetValidationIssues();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("検証結果", EditorStyles.boldLabel);

            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox(noIssueMessage, MessageType.Info);
                return;
            }

            for (var i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                EditorGUILayout.HelpBox(issue.Message, ToMessageType(issue.Severity));
                EditorGUILayout.LabelField(issue.Code, EditorStyles.miniLabel);
            }
        }

        internal static MessageType ToMessageType(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Error => MessageType.Error,
                ValidationSeverity.Warning => MessageType.Warning,
                _ => MessageType.Info,
            };
        }
    }

    [Serializable]
    public sealed class AbilityTreeGraphLayoutNode
    {
        [SerializeField] private AbilityNodeMasterData node;
        [SerializeField] private Vector2 position;

        public AbilityNodeMasterData Node => node;
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public AbilityTreeGraphLayoutNode(AbilityNodeMasterData node, Vector2 position)
        {
            this.node = node;
            this.position = position;
        }
    }

    /// <summary>
    /// Editor Graphだけが使う自由配置情報。ゲーム用の縦Row配置は AbilityTreeMasterData.DisplayRows で管理する。
    /// </summary>
    public sealed class AbilityTreeGraphLayout : ScriptableObject
    {
        [SerializeField] private AbilityTreeMasterData tree;
        [SerializeField] private List<AbilityTreeGraphLayoutNode> nodes = new();

        public AbilityTreeMasterData Tree => tree;
        public IReadOnlyList<AbilityTreeGraphLayoutNode> Nodes => nodes;

        public void Initialize(AbilityTreeMasterData targetTree)
        {
            tree = targetTree;
            nodes ??= new List<AbilityTreeGraphLayoutNode>();
        }

        public void SyncNodes()
        {
            nodes ??= new List<AbilityTreeGraphLayoutNode>();
            if (tree == null)
            {
                return;
            }

            var treeNodes = new HashSet<AbilityNodeMasterData>();
            for (var i = 0; i < tree.NodeEntries.Count; i++)
            {
                var node = tree.NodeEntries[i]?.Node;
                if (node == null || !treeNodes.Add(node))
                {
                    continue;
                }

                if (Find(node) == null)
                {
                    var index = nodes.Count;
                    nodes.Add(new AbilityTreeGraphLayoutNode(node, new Vector2(40 + (index % 3) * 220, 40 + (index / 3) * 150)));
                }
            }

            nodes.RemoveAll(layoutNode => layoutNode == null || layoutNode.Node == null || !treeNodes.Contains(layoutNode.Node));
        }

        public AbilityTreeGraphLayoutNode Find(AbilityNodeMasterData node)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i]?.Node == node)
                {
                    return nodes[i];
                }
            }

            return null;
        }
    }

    public sealed class AbilityTreeGraphEditorWindow : EditorWindow
    {
        private const float ToolbarHeight = 56f;
        private const float NodeWidth = 180f;
        private const float NodeHeight = 96f;
        private const string LayoutFolder = "Assets/Editor/AbilityTreeLayouts";

        private AbilityTreeMasterData tree;
        private AbilityTreeGraphLayout layout;
        private AbilityNodeMasterData nodeToAdd;
        private AbilityNodeMasterData selectedNode;
        private AbilityNodeMasterData connectionSource;
        private AbilityTreeLinkType connectionType = AbilityTreeLinkType.Prerequisite;

        public static void Open(AbilityTreeMasterData targetTree)
        {
            AbilityTreeBuilderWindow.Open(targetTree);
        }

        private void OnEnable()
        {
            if (tree != null)
            {
                SetTree(tree);
            }
        }

        private void SetTree(AbilityTreeMasterData targetTree)
        {
            tree = targetTree;
            layout = GetOrCreateLayout(tree);
            SyncLayout();
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (tree == null || layout == null)
            {
                EditorGUILayout.HelpBox("編集する AbilityTreeMasterData を指定してください。", MessageType.Info);
                return;
            }

            var graphRect = new Rect(0f, ToolbarHeight, position.width, Mathf.Max(0f, position.height - ToolbarHeight));
            GUI.Box(graphRect, GUIContent.none, EditorStyles.helpBox);
            DrawLinks(graphRect.position);
            DrawNodes(graphRect.position);
            DrawConnectionHint(graphRect);

            if (Event.current.type == EventType.MouseUp)
            {
                EditorUtility.SetDirty(layout);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(ToolbarHeight)))
            {
                EditorGUI.BeginChangeCheck();
                var selectedTree = (AbilityTreeMasterData)EditorGUILayout.ObjectField(
                    tree,
                    typeof(AbilityTreeMasterData),
                    false,
                    GUILayout.Width(220f));
                if (EditorGUI.EndChangeCheck())
                {
                    SetTree(selectedTree);
                }

                if (tree == null)
                {
                    return;
                }

                nodeToAdd = (AbilityNodeMasterData)EditorGUILayout.ObjectField(
                    nodeToAdd,
                    typeof(AbilityNodeMasterData),
                    false,
                    GUILayout.Width(180f));
                if (GUILayout.Button("Add Node", EditorStyles.toolbarButton, GUILayout.Width(75f)))
                {
                    AddNode(nodeToAdd);
                }

                if (GUILayout.Button("Sync", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                {
                    SyncLayout();
                }

                GUILayout.FlexibleSpace();
                connectionType = (AbilityTreeLinkType)EditorGUILayout.EnumPopup(connectionType, GUILayout.Width(105f));
                if (GUILayout.Button("Connect", EditorStyles.toolbarButton, GUILayout.Width(65f)))
                {
                    connectionSource = selectedNode;
                }
            }
        }

        private void DrawLinks(Vector2 graphOrigin)
        {
            if (tree == null || layout == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Handles.BeginGUI();
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                var from = link?.FromNode == null ? null : layout.Find(link.FromNode);
                var to = link?.ToNode == null ? null : layout.Find(link.ToNode);
                if (from == null || to == null)
                {
                    continue;
                }

                var fromPosition = graphOrigin + from.Position + new Vector2(NodeWidth, NodeHeight * 0.5f);
                var toPosition = graphOrigin + to.Position + new Vector2(0f, NodeHeight * 0.5f);
                var color = link.LinkType == AbilityTreeLinkType.Exclusive ? new Color(0.9f, 0.35f, 0.35f) : new Color(0.65f, 0.75f, 0.9f);
                Handles.color = color;
                Handles.DrawAAPolyLine(3f, fromPosition, toPosition);

                if (link.LinkType == AbilityTreeLinkType.Prerequisite)
                {
                    var labelPosition = (fromPosition + toPosition) * 0.5f;
                    GUI.Label(new Rect(labelPosition.x - 28f, labelPosition.y - 18f, 56f, 18f), $"Lv.{link.RequiredLevel}", EditorStyles.miniLabel);
                }
            }

            Handles.EndGUI();
        }

        private void DrawNodes(Vector2 graphOrigin)
        {
            BeginWindows();
            for (var i = 0; i < layout.Nodes.Count; i++)
            {
                var layoutNode = layout.Nodes[i];
                if (layoutNode?.Node == null)
                {
                    continue;
                }

                var nodeRect = new Rect(graphOrigin + layoutNode.Position, new Vector2(NodeWidth, NodeHeight));
                var updatedRect = GUI.Window(i, nodeRect, DrawNodeWindow, layoutNode.Node.DisplayName);
                var updatedPosition = updatedRect.position - graphOrigin;
                if (updatedPosition != layoutNode.Position)
                {
                    layoutNode.Position = updatedPosition;
                }
            }

            EndWindows();
        }

        private void DrawNodeWindow(int windowId)
        {
            var node = layout.Nodes[windowId].Node;
            EditorGUILayout.LabelField(node.NodeId, EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"{node.ProgressionType}  Lv.0/{node.MaxLevel}", EditorStyles.miniLabel);
            if (GUILayout.Button(selectedNode == node ? "Selected" : "Select"))
            {
                if (connectionSource == null)
                {
                    selectedNode = node;
                }
                else if (connectionSource != node)
                {
                    AddLink(connectionSource, node, connectionType);
                    connectionSource = null;
                    selectedNode = node;
                }
            }

            GUI.DragWindow(new Rect(0f, 0f, NodeWidth, 22f));
        }

        private void DrawConnectionHint(Rect graphRect)
        {
            if (connectionSource == null)
            {
                return;
            }

            var label = $"{connectionSource.DisplayName} から {connectionType} 接続するNodeを選択";
            GUI.Label(new Rect(graphRect.x + 12f, graphRect.y + 8f, graphRect.width - 24f, 20f), label, EditorStyles.boldLabel);
        }

        private void AddNode(AbilityNodeMasterData node)
        {
            if (tree == null || node == null || ContainsNode(node))
            {
                return;
            }

            var serializedTree = new SerializedObject(tree);
            var entries = serializedTree.FindProperty("nodeEntries");
            entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            entry.FindPropertyRelative("node").objectReferenceValue = node;
            entry.FindPropertyRelative("prerequisiteMode").enumValueIndex = (int)AbilityPrerequisiteMode.All;
            serializedTree.ApplyModifiedProperties();
            SyncLayout();
        }

        private void AddLink(AbilityNodeMasterData from, AbilityNodeMasterData to, AbilityTreeLinkType type)
        {
            if (tree == null || from == null || to == null || from == to || !ContainsNode(from) || !ContainsNode(to))
            {
                return;
            }

            var serializedTree = new SerializedObject(tree);
            var links = serializedTree.FindProperty("links");
            links.arraySize++;
            var link = links.GetArrayElementAtIndex(links.arraySize - 1);
            link.FindPropertyRelative("linkType").enumValueIndex = (int)type;
            link.FindPropertyRelative("fromNode").objectReferenceValue = from;
            link.FindPropertyRelative("toNode").objectReferenceValue = to;
            link.FindPropertyRelative("requiredLevel").intValue = 1;
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
        }

        private bool ContainsNode(AbilityNodeMasterData node)
        {
            if (tree == null || node == null)
            {
                return false;
            }

            for (var i = 0; i < tree.NodeEntries.Count; i++)
            {
                if (tree.NodeEntries[i]?.Node == node)
                {
                    return true;
                }
            }

            return false;
        }

        private void SyncLayout()
        {
            if (layout == null)
            {
                return;
            }

            Undo.RecordObject(layout, "Sync Ability Tree Graph Layout");
            layout.SyncNodes();
            EditorUtility.SetDirty(layout);
        }

        private static AbilityTreeGraphLayout GetOrCreateLayout(AbilityTreeMasterData targetTree)
        {
            if (targetTree == null)
            {
                return null;
            }

            EnsureFolder("Assets/Editor");
            EnsureFolder(LayoutFolder);
            var treePath = AssetDatabase.GetAssetPath(targetTree);
            var treeGuid = AssetDatabase.AssetPathToGUID(treePath);
            var suffix = string.IsNullOrEmpty(treeGuid) ? targetTree.name : treeGuid[..8];
            var layoutPath = $"{LayoutFolder}/{targetTree.name}_{suffix}GraphLayout.asset";
            var existingLayout = AssetDatabase.LoadAssetAtPath<AbilityTreeGraphLayout>(layoutPath);
            if (existingLayout != null)
            {
                existingLayout.Initialize(targetTree);
                return existingLayout;
            }

            var newLayout = CreateInstance<AbilityTreeGraphLayout>();
            newLayout.Initialize(targetTree);
            AssetDatabase.CreateAsset(newLayout, layoutPath);
            return newLayout;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    /// <summary>
    /// Treeを上から順に組み立てるためのEditorWindow。
    /// 左側は現在のTree、右側は選択Nodeへの直列・二股Branch追加を扱う。
    /// </summary>
    public sealed class AbilityTreeBuilderWindow : EditorWindow
    {
        private const float GraphNodeWidth = 164f;
        private const float GraphNodeHeight = 62f;
        private const float GraphHorizontalSpacing = 24f;
        private const float GraphVerticalSpacing = 52f;
        private const float GraphPadding = 28f;

        private AbilityTreeMasterData tree;
        private AbilityNodeMasterData rootNodeToAdd;
        private AbilityNodeMasterData nextNodeToAdd;
        private AbilityNodeMasterData firstBranchNodeToAdd;
        private AbilityNodeMasterData secondBranchNodeToAdd;
        private string selectedTreeNodeId;
        private string existingTargetTreeNodeId;
        private bool createBranch;
        private Vector2 treeScrollPosition;
        private Vector2 builderScrollPosition;
        private bool showUsage;
        private bool showAdvancedAdd;
        private bool showPlacementNodeMaster;
        private bool showConnectionEditor;
        private bool showDangerActions;
        private bool showSelectedNodeDefinition;
        private UnityEditor.Editor selectedNodeEditor;
        private string builderMessage = string.Empty;

        [MenuItem("Tools/Original/Ability Tree Graph")]
        public static void OpenFromMenu()
        {
            GetWindow<AbilityTreeBuilderWindow>("Ability Tree編集").Show();
        }

        public static void Open(AbilityTreeMasterData targetTree)
        {
            var window = GetWindow<AbilityTreeBuilderWindow>("Ability Tree編集");
            window.tree = targetTree;
            window.selectedTreeNodeId = string.Empty;
            window.Show();
        }

        private void OnGUI()
        {
            DrawTreeSelector();
            if (tree == null)
            {
                EditorGUILayout.HelpBox("最初に AbilityTreeMasterData をアタッチしてください。", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(position.width * 0.55f)))
                {
                    DrawTreePreview();
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    builderScrollPosition = EditorGUILayout.BeginScrollView(builderScrollPosition);
                    DrawBuilderPanel();
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void DrawTreeSelector()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                var selectedTree = (AbilityTreeMasterData)EditorGUILayout.ObjectField(
                    "編集するTree",
                    tree,
                    typeof(AbilityTreeMasterData),
                    false);
                if (EditorGUI.EndChangeCheck())
                {
                    tree = selectedTree;
                    selectedTreeNodeId = string.Empty;
                }
            }
        }

        private void DrawTreePreview()
        {
            EditorGUILayout.LabelField("現在のTree", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("下が開始点、上が後続Nodeです。カードを選択すると右側で編集できます。", EditorStyles.miniLabel);
            treeScrollPosition = EditorGUILayout.BeginScrollView(treeScrollPosition, true, true);

            var nodes = GetTreeNodes();
            if (nodes.Count == 0)
            {
                EditorGUILayout.HelpBox("まだNodeがありません。右側でRoot Nodeを追加してください。", MessageType.Info);
            }
            else
            {
                DrawTreeGraph(nodes);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTreeGraph(IReadOnlyList<AbilityTreeNodeEntry> nodes)
        {
            var depths = BuildGraphDepths(nodes);
            var nodesByDepth = new Dictionary<int, List<AbilityTreeNodeEntry>>();
            var maxDepth = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var depth = depths[node.TreeNodeId];
                maxDepth = Mathf.Max(maxDepth, depth);
                if (!nodesByDepth.TryGetValue(depth, out var layer))
                {
                    layer = new List<AbilityTreeNodeEntry>();
                    nodesByDepth.Add(depth, layer);
                }

                layer.Add(node);
            }

            var maxLayerCount = 1;
            foreach (var layer in nodesByDepth.Values)
            {
                maxLayerCount = Mathf.Max(maxLayerCount, layer.Count);
            }

            var graphWidth = GraphPadding * 2f + maxLayerCount * GraphNodeWidth + (maxLayerCount - 1) * GraphHorizontalSpacing;
            var graphHeight = GraphPadding * 2f + (maxDepth + 1) * GraphNodeHeight + maxDepth * GraphVerticalSpacing;
            var graphRect = GUILayoutUtility.GetRect(graphWidth, graphHeight, GUILayout.Width(graphWidth), GUILayout.Height(graphHeight));
            GUI.Box(graphRect, GUIContent.none, EditorStyles.helpBox);

            var cardRects = new Dictionary<string, Rect>(StringComparer.Ordinal);
            for (var depth = 0; depth <= maxDepth; depth++)
            {
                if (!nodesByDepth.TryGetValue(depth, out var layer)) continue;
                var layerWidth = layer.Count * GraphNodeWidth + (layer.Count - 1) * GraphHorizontalSpacing;
                var startX = graphRect.x + (graphRect.width - layerWidth) * 0.5f;
                var y = graphRect.yMax - GraphPadding - GraphNodeHeight - depth * (GraphNodeHeight + GraphVerticalSpacing);
                for (var index = 0; index < layer.Count; index++)
                {
                    cardRects[layer[index].TreeNodeId] = new Rect(startX + index * (GraphNodeWidth + GraphHorizontalSpacing), y, GraphNodeWidth, GraphNodeHeight);
                }
            }

            DrawTreeGraphLinks(cardRects);
            for (var i = 0; i < nodes.Count; i++)
            {
                DrawTreeGraphNodeCard(nodes[i], cardRects[nodes[i].TreeNodeId]);
            }
        }

        private Dictionary<string, int> BuildGraphDepths(IReadOnlyList<AbilityTreeNodeEntry> nodes)
        {
            var depths = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < nodes.Count; i++)
            {
                depths[nodes[i].TreeNodeId] = 0;
            }

            for (var pass = 0; pass < nodes.Count; pass++)
            {
                var changed = false;
                for (var i = 0; i < tree.Links.Count; i++)
                {
                    var link = tree.Links[i];
                    if (link?.LinkType != AbilityTreeLinkType.Prerequisite ||
                        !depths.TryGetValue(link.FromTreeNodeId, out var fromDepth) ||
                        !depths.TryGetValue(link.ToTreeNodeId, out var toDepth)) continue;
                    if (toDepth < fromDepth + 1)
                    {
                        depths[link.ToTreeNodeId] = fromDepth + 1;
                        changed = true;
                    }
                }

                if (!changed) break;
            }

            return depths;
        }

        private void DrawTreeGraphLinks(IReadOnlyDictionary<string, Rect> cardRects)
        {
            Handles.BeginGUI();
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link == null || !cardRects.TryGetValue(link.FromTreeNodeId, out var fromRect) ||
                    !cardRects.TryGetValue(link.ToTreeNodeId, out var toRect)) continue;

                var color = link.LinkType == AbilityTreeLinkType.Exclusive
                    ? new Color(0.9f, 0.35f, 0.35f)
                    : new Color(0.65f, 0.75f, 0.9f);
                var from = link.LinkType == AbilityTreeLinkType.Prerequisite
                    ? new Vector3(fromRect.center.x, fromRect.yMin, 0f)
                    : new Vector3(fromRect.center.x, fromRect.center.y, 0f);
                var to = link.LinkType == AbilityTreeLinkType.Prerequisite
                    ? new Vector3(toRect.center.x, toRect.yMax, 0f)
                    : new Vector3(toRect.center.x, toRect.center.y, 0f);
                var fromTangent = link.LinkType == AbilityTreeLinkType.Prerequisite ? from + Vector3.up * 26f : from + Vector3.right * 26f;
                var toTangent = link.LinkType == AbilityTreeLinkType.Prerequisite ? to + Vector3.down * 26f : to + Vector3.left * 26f;
                Handles.DrawBezier(from, to, fromTangent, toTangent, color, null, 2.5f);
            }
            Handles.EndGUI();
        }

        private void DrawTreeGraphNodeCard(AbilityTreeNodeEntry entry, Rect cardRect)
        {
            var selected = selectedTreeNodeId == entry.TreeNodeId;
            GUI.Box(cardRect, GUIContent.none, selected ? EditorStyles.helpBox : EditorStyles.textArea);
            var titleRect = new Rect(cardRect.x + 4f, cardRect.y + 4f, cardRect.width - 8f, 22f);
            if (GUI.Button(titleRect, selected ? $"● {entry.Node.DisplayName}" : entry.Node.DisplayName, EditorStyles.miniButton))
            {
                selectedTreeNodeId = entry.TreeNodeId;
                Repaint();
            }

            GUI.Label(new Rect(cardRect.x + 7f, cardRect.y + 29f, cardRect.width - 14f, 14f), entry.TreeNodeId, EditorStyles.miniLabel);
            GUI.Label(new Rect(cardRect.x + 7f, cardRect.y + 45f, cardRect.width - 14f, 14f), $"前提条件: {FormatPrerequisiteMode(entry.PrerequisiteMode)}", EditorStyles.miniLabel);
        }

        private static string FormatPrerequisiteMode(AbilityPrerequisiteMode mode)
        {
            return mode == AbilityPrerequisiteMode.Any ? "いずれか" : "すべて";
        }

        private void DrawBuilderPanel()
        {
            DrawUsageGuide();
            EditorGUILayout.LabelField("Treeの編集", EditorStyles.boldLabel);

            if (tree.NodeEntries.Count == 0)
            {
                rootNodeToAdd = (AbilityNodeMasterData)EditorGUILayout.ObjectField(
                    new GUIContent("開始Node", "Treeの最初に配置するNode定義を指定します。"),
                    rootNodeToAdd,
                    typeof(AbilityNodeMasterData),
                    false);
                if (GUILayout.Button(new GUIContent("開始Nodeを追加", "選択したNode定義を、このTreeの最初の配置Nodeとして追加します。"))) AddRootNode(rootNodeToAdd);
                DrawMessage();
                DrawSelectedNodeDefinition(rootNodeToAdd);
                return;
            }

            var selectedEntry = tree.FindNodeEntry(selectedTreeNodeId);
            if (selectedEntry?.Node == null)
            {
                EditorGUILayout.HelpBox("左側から、次のNodeを追加したい配置Nodeを選択してください。", MessageType.Info);
                DrawMessage();
                return;
            }

            EditorGUILayout.LabelField("選択中", selectedEntry.Node.DisplayName);
            EditorGUILayout.LabelField(new GUIContent("TreeNodeId", "このTree内でのみ一意な配置NodeのIDです。保存、進捗、接続、排他はこのIDで区別されます。"), new GUIContent(selectedEntry.TreeNodeId));
            DrawMessage();
            DrawPlacementNodeMasterField(selectedEntry);
            var children = GetChildren(selectedEntry.TreeNodeId);
            if (children.Count > 0)
            {
                EditorGUILayout.HelpBox("この配置Nodeには既に後続Nodeがあります。新規追加の前に、下のConnection Editorから既存接続を変更または削除できます。", MessageType.Info);
            }
            else
            {
                DrawSimpleAdd(selectedEntry);
                DrawAdvancedAdd(selectedEntry);
            }

            if (GetParents(selectedEntry.TreeNodeId).Count > 1)
            {
                DrawPrerequisiteMode(selectedEntry.TreeNodeId);
            }
            DrawConnectionEditor(selectedEntry);
            DrawSelectedNodeDefinition(selectedEntry.Node);
            DrawDangerActions(selectedEntry);
        }

        private void DrawUsageGuide()
        {
            showUsage = EditorGUILayout.Foldout(showUsage, "操作ガイド", true);
            if (!showUsage) return;

            EditorGUILayout.HelpBox(
                "左のカードを選択して、右側で後続Nodeを追加します。\n" +
                "分岐・合流、接続の編集、Node定義の編集、削除は必要なときだけ各折りたたみを開いてください。",
                MessageType.Info);
        }

        private void DrawSimpleAdd(AbilityTreeNodeEntry selectedEntry)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("後続Nodeを追加", EditorStyles.boldLabel);
            nextNodeToAdd = (AbilityNodeMasterData)EditorGUILayout.ObjectField(
                new GUIContent("Node定義", "新しい後続配置Nodeに使うNode定義を指定します。"), nextNodeToAdd, typeof(AbilityNodeMasterData), false);
            if (GUILayout.Button(new GUIContent("追加", "選択したNode定義から新しい配置Nodeを作り、選択中Nodeの後続として接続します。"))) AddChildNode(selectedEntry, nextNodeToAdd);
        }

        private void DrawAdvancedAdd(AbilityTreeNodeEntry selectedEntry)
        {
            showAdvancedAdd = EditorGUILayout.Foldout(showAdvancedAdd, "分岐・合流を追加", true);
            if (!showAdvancedAdd) return;

            createBranch = EditorGUILayout.ToggleLeft("2択の排他分岐を作る", createBranch);
            if (createBranch)
            {
                firstBranchNodeToAdd = (AbilityNodeMasterData)EditorGUILayout.ObjectField("分岐AのNode定義", firstBranchNodeToAdd, typeof(AbilityNodeMasterData), false);
                secondBranchNodeToAdd = (AbilityNodeMasterData)EditorGUILayout.ObjectField("分岐BのNode定義", secondBranchNodeToAdd, typeof(AbilityNodeMasterData), false);
                if (GUILayout.Button("排他分岐を追加")) AddExclusiveBranch(selectedEntry, firstBranchNodeToAdd, secondBranchNodeToAdd);
                return;
            }

            DrawExistingNodeLink(selectedEntry);
        }

        private void DrawSelectedNodeDefinition(AbilityNodeMasterData node)
        {
            if (node == null) return;

            showSelectedNodeDefinition = EditorGUILayout.Foldout(showSelectedNodeDefinition, "選択中のNode定義を編集", true);
            if (!showSelectedNodeDefinition) return;

            EditorGUILayout.HelpBox(
                "ここで編集する内容はNode定義Assetそのものです。同じAssetを参照する他の配置Nodeにも反映されます。" +
                "配置ごとに変えたい進捗・前提・排他は、この上のTreeの編集で設定してください。",
                MessageType.None);
            UnityEditor.Editor.CreateCachedEditor(node, typeof(AbilityNodeMasterDataEditor), ref selectedNodeEditor);
            selectedNodeEditor.OnInspectorGUI();
        }

        private void DrawPlacementNodeMasterField(AbilityTreeNodeEntry entry)
        {
            showPlacementNodeMaster = EditorGUILayout.Foldout(showPlacementNodeMaster, "この配置のNode定義を差し替え", true);
            if (!showPlacementNodeMaster) return;
            EditorGUI.BeginChangeCheck();
            var updatedNode = (AbilityNodeMasterData)EditorGUILayout.ObjectField(
                new GUIContent("Node定義", "この配置に割り当てるNode定義を変更します。接続とTreeNodeIdは維持されます。"),
                entry.Node,
                typeof(AbilityNodeMasterData),
                false);
            if (EditorGUI.EndChangeCheck() && updatedNode != null)
            {
                SetPlacementNodeMaster(entry.TreeNodeId, updatedNode);
            }
        }

        private void SetPlacementNodeMaster(string treeNodeId, AbilityNodeMasterData node)
        {
            Undo.RecordObject(tree, "Change Ability Tree Placement Node Master");
            var serializedTree = new SerializedObject(tree);
            var entries = serializedTree.FindProperty("nodeEntries");
            for (var i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("treeNodeId").stringValue != treeNodeId) continue;
                entry.FindPropertyRelative("node").objectReferenceValue = node;
                serializedTree.ApplyModifiedProperties();
                EditorUtility.SetDirty(tree);
                Repaint();
                return;
            }
        }

        private void DrawConnectionEditor(AbilityTreeNodeEntry entry)
        {
            var connectionCount = CountConnections(entry.TreeNodeId);
            showConnectionEditor = EditorGUILayout.Foldout(showConnectionEditor, $"接続を確認・編集 ({connectionCount})", true);
            if (!showConnectionEditor) return;

            var hasConnection = false;
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link == null || (link.FromTreeNodeId != entry.TreeNodeId && link.ToTreeNodeId != entry.TreeNodeId)) continue;
                hasConnection = true;
                DrawConnectionRow(i, link);
            }

            if (!hasConnection) EditorGUILayout.LabelField("この配置Nodeには接続がありません。", EditorStyles.miniLabel);
        }

        private int CountConnections(string treeNodeId)
        {
            var count = 0;
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link != null && (link.FromTreeNodeId == treeNodeId || link.ToTreeNodeId == treeNodeId)) count++;
            }
            return count;
        }

        private void DrawConnectionRow(int linkIndex, AbilityTreeLinkDefinition link)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"{FormatPlacement(link.FromTreeNodeId)}  →  {FormatPlacement(link.ToTreeNodeId)}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(link.LinkType == AbilityTreeLinkType.Prerequisite ? "前提接続" : "排他接続", EditorStyles.miniLabel);
                if (link.LinkType == AbilityTreeLinkType.Prerequisite)
                {
                    EditorGUI.BeginChangeCheck();
                    var requiredLevel = EditorGUILayout.IntField(new GUIContent("必要Level", "接続元Nodeに必要なLevelです。"), link.RequiredLevel);
                    if (EditorGUI.EndChangeCheck()) SetLinkRequiredLevel(linkIndex, requiredLevel);
                }

                if (GUILayout.Button(new GUIContent("接続を削除", "この接続だけを削除します。配置NodeとNode Master Assetは削除しません。")))
                {
                    RemoveLink(linkIndex);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private string FormatPlacement(string treeNodeId)
        {
            var entry = tree.FindNodeEntry(treeNodeId);
            return entry?.Node == null ? $"未設定 [{treeNodeId}]" : $"{entry.Node.DisplayName} [{treeNodeId}]";
        }

        private void SetLinkRequiredLevel(int linkIndex, int requiredLevel)
        {
            Undo.RecordObject(tree, "Set Ability Tree Link Required Level");
            var serializedTree = new SerializedObject(tree);
            var links = serializedTree.FindProperty("links");
            if (linkIndex < 0 || linkIndex >= links.arraySize) return;
            links.GetArrayElementAtIndex(linkIndex).FindPropertyRelative("requiredLevel").intValue = Mathf.Max(1, requiredLevel);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            Repaint();
        }

        private void RemoveLink(int linkIndex)
        {
            Undo.RecordObject(tree, "Remove Ability Tree Connection");
            var serializedTree = new SerializedObject(tree);
            var links = serializedTree.FindProperty("links");
            if (linkIndex < 0 || linkIndex >= links.arraySize) return;
            links.DeleteArrayElementAtIndex(linkIndex);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            Repaint();
        }

        private void DrawRemovePlacementButton(AbilityTreeNodeEntry entry)
        {
            EditorGUILayout.Space(4f);
            if (!GUILayout.Button(new GUIContent("選択中の配置Nodeを削除", "選択中の配置Nodeと、その配置に出入りする接続を削除します。Node Master Assetと他の配置Nodeは残ります。"))) return;
            if (!EditorUtility.DisplayDialog(
                    "配置Nodeを削除",
                    $"{entry.Node.DisplayName} [{entry.TreeNodeId}] をTreeから削除します。\n接続は削除されますが、Node Master Assetと他の配置Nodeは残ります。\n後続Nodeは必要に応じて再接続してください。",
                    "削除",
                    "キャンセル")) return;

            Undo.RecordObject(tree, "Remove Ability Tree Placement");
            var serializedTree = new SerializedObject(tree);
            var links = serializedTree.FindProperty("links");
            for (var i = links.arraySize - 1; i >= 0; i--)
            {
                var link = links.GetArrayElementAtIndex(i);
                if (link.FindPropertyRelative("fromTreeNodeId").stringValue == entry.TreeNodeId ||
                    link.FindPropertyRelative("toTreeNodeId").stringValue == entry.TreeNodeId)
                {
                    links.DeleteArrayElementAtIndex(i);
                }
            }

            var entries = serializedTree.FindProperty("nodeEntries");
            for (var i = entries.arraySize - 1; i >= 0; i--)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("treeNodeId").stringValue == entry.TreeNodeId)
                {
                    entries.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            selectedTreeNodeId = string.Empty;
            existingTargetTreeNodeId = string.Empty;
            Repaint();
            GUIUtility.ExitGUI();
        }

        private void DrawDangerActions(AbilityTreeNodeEntry entry)
        {
            EditorGUILayout.Space(4f);
            showDangerActions = EditorGUILayout.Foldout(showDangerActions, "削除・全消去", true);
            if (!showDangerActions) return;
            DrawRemovePlacementButton(entry);
            if (GUILayout.Button(new GUIContent("Treeをすべてクリア", "このTreeの配置Nodeと接続をすべて削除します。Node定義Asset自体は削除しません。"))) ClearTree();
        }

        private void OnDisable()
        {
            if (selectedNodeEditor != null) DestroyImmediate(selectedNodeEditor);
        }

        private void DrawMessage()
        {
            if (!string.IsNullOrEmpty(builderMessage)) EditorGUILayout.HelpBox(builderMessage, MessageType.Warning);
        }

        private void DrawPrerequisiteMode(string treeNodeId)
        {
            var currentMode = GetPrerequisiteMode(treeNodeId);
            EditorGUI.BeginChangeCheck();
            var selectedIndex = EditorGUILayout.Popup(
                new GUIContent("前提条件", "複数の前提接続がある場合、すべて必要か、いずれか一つでよいかを設定します。"),
                currentMode == AbilityPrerequisiteMode.Any ? 1 : 0,
                new[] { "すべて必要", "いずれか一つで可" });
            var updatedMode = selectedIndex == 1 ? AbilityPrerequisiteMode.Any : AbilityPrerequisiteMode.All;
            if (EditorGUI.EndChangeCheck()) SetPrerequisiteMode(treeNodeId, updatedMode);
        }

        private void AddRootNode(AbilityNodeMasterData node)
        {
            if (node == null) { builderMessage = "開始NodeにAbilityNodeMasterDataを設定してください。"; return; }
            Undo.RecordObject(tree, "Add Ability Tree Root Node");
            var serializedTree = new SerializedObject(tree);
            selectedTreeNodeId = AddNodeEntry(serializedTree, node);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            rootNodeToAdd = null;
            builderMessage = string.Empty;
            Repaint();
        }

        private void AddChildNode(AbilityTreeNodeEntry parent, AbilityNodeMasterData child)
        {
            if (parent?.Node == null || child == null || GetChildren(parent.TreeNodeId).Count > 0)
            {
                builderMessage = child == null ? "後続NodeにAbilityNodeMasterDataを設定してください。" : "この配置Nodeには既に後続Nodeがあります。別の末端Nodeを選択してください。";
                Repaint(); return;
            }

            Undo.RecordObject(tree, "Add Ability Tree Node");
            var serializedTree = new SerializedObject(tree);
            var childId = AddNodeEntry(serializedTree, child);
            AddLink(serializedTree, parent.TreeNodeId, childId, AbilityTreeLinkType.Prerequisite);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            selectedTreeNodeId = childId;
            nextNodeToAdd = null;
            builderMessage = string.Empty;
            Repaint();
        }

        private void AddExclusiveBranch(AbilityTreeNodeEntry parent, AbilityNodeMasterData firstChild, AbilityNodeMasterData secondChild)
        {
            if (parent?.Node == null || firstChild == null || secondChild == null || GetChildren(parent.TreeNodeId).Count > 0)
            {
                builderMessage = "BranchにはAbilityNodeMasterDataを2つ設定し、後続Nodeのない配置Nodeを選択してください。";
                Repaint(); return;
            }

            Undo.RecordObject(tree, "Add Ability Tree Branch");
            var serializedTree = new SerializedObject(tree);
            var firstId = AddNodeEntry(serializedTree, firstChild);
            var secondId = AddNodeEntry(serializedTree, secondChild);
            AddLink(serializedTree, parent.TreeNodeId, firstId, AbilityTreeLinkType.Prerequisite);
            AddLink(serializedTree, parent.TreeNodeId, secondId, AbilityTreeLinkType.Prerequisite);
            AddLink(serializedTree, firstId, secondId, AbilityTreeLinkType.Exclusive);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            selectedTreeNodeId = firstId;
            firstBranchNodeToAdd = null;
            secondBranchNodeToAdd = null;
            createBranch = false;
            builderMessage = string.Empty;
            Repaint();
        }

        private void DrawExistingNodeLink(AbilityTreeNodeEntry parent)
        {
            var candidates = GetTreeNodes();
            if (candidates.Count <= 1) return;

            var labels = new List<string>();
            var ids = new List<string>();
            var unavailableReasons = new List<string>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!TryGetExistingLinkIssue(parent, candidate, out var issue))
                {
                    ids.Add(candidate.TreeNodeId);
                    labels.Add(BuildExistingLinkCandidateLabel(parent, candidate));
                    continue;
                }

                unavailableReasons.Add($"{candidate.Node.DisplayName} [{candidate.TreeNodeId}]: {issue}");
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("既存の配置Nodeへ合流", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("候補はNode Masterではなく、Tree内の個別配置（TreeNodeId）です。", EditorStyles.miniLabel);
            if (ids.Count == 0)
            {
                EditorGUILayout.HelpBox("この末端Nodeから再合流できる配置Nodeはありません。", MessageType.Info);
                DrawUnavailableCandidateReasons(unavailableReasons);
                return;
            }

            var currentIndex = ids.IndexOf(existingTargetTreeNodeId);
            if (currentIndex < 0) currentIndex = 0;
            var updatedIndex = EditorGUILayout.Popup(currentIndex, labels.ToArray());
            existingTargetTreeNodeId = ids[updatedIndex];
            if (GUILayout.Button(new GUIContent("選択した配置Nodeへ接続", "選択した配置Nodeへ前提接続を追加します。"))) AddExistingChildLink(parent, existingTargetTreeNodeId);
            DrawUnavailableCandidateReasons(unavailableReasons);
        }

        private static void DrawUnavailableCandidateReasons(IReadOnlyList<string> unavailableReasons)
        {
            if (unavailableReasons.Count == 0) return;
            EditorGUILayout.LabelField("接続できない配置Node", EditorStyles.miniLabel);
            for (var i = 0; i < unavailableReasons.Count; i++)
            {
                EditorGUILayout.LabelField($"• {unavailableReasons[i]}", EditorStyles.miniLabel);
            }
        }

        private bool TryGetExistingLinkIssue(AbilityTreeNodeEntry parent, AbilityTreeNodeEntry candidate, out string issue)
        {
            if (parent?.Node == null || candidate?.Node == null)
            {
                issue = "配置Nodeが未設定です";
                return true;
            }

            if (parent.TreeNodeId == candidate.TreeNodeId)
            {
                issue = "選択中の配置Node自身です";
                return true;
            }

            if (HasPrerequisiteLink(parent.TreeNodeId, candidate.TreeNodeId))
            {
                issue = "同じ接続が既にあります";
                return true;
            }

            if (HasPath(candidate.TreeNodeId, parent.TreeNodeId, new HashSet<string>(StringComparer.Ordinal)))
            {
                issue = "接続すると循環します";
                return true;
            }

            issue = string.Empty;
            return false;
        }

        private string BuildExistingLinkCandidateLabel(AbilityTreeNodeEntry parent, AbilityTreeNodeEntry candidate)
        {
            var label = $"{candidate.Node.DisplayName} [{candidate.TreeNodeId}]";
            return RequiresAnyPrerequisiteMode(parent, candidate)
                ? $"{label}  ※「いずれか」推奨"
                : label;
        }

        private bool RequiresAnyPrerequisiteMode(AbilityTreeNodeEntry parent, AbilityTreeNodeEntry candidate)
        {
            if (candidate.PrerequisiteMode != AbilityPrerequisiteMode.All) return false;
            var otherParents = GetParents(candidate.TreeNodeId);
            for (var i = 0; i < otherParents.Count; i++)
            {
                if (otherParents[i].TreeNodeId != parent.TreeNodeId &&
                    HasExclusiveLineageConflict(parent.TreeNodeId, otherParents[i].TreeNodeId)) return true;
            }

            return HasExclusiveLink(parent.TreeNodeId, candidate.TreeNodeId);
        }

        private void AddExistingChildLink(AbilityTreeNodeEntry parent, string targetTreeNodeId)
        {
            var target = tree.FindNodeEntry(targetTreeNodeId);
            if (GetChildren(parent?.TreeNodeId).Count > 0 || TryGetExistingLinkIssue(parent, target, out _))
            {
                builderMessage = "既存配置Nodeへ接続できません。末端Nodeを選び、循環しない接続にしてください。";
                Repaint(); return;
            }

            Undo.RecordObject(tree, "Link Existing Ability Tree Node");
            var serializedTree = new SerializedObject(tree);
            AddLink(serializedTree, parent.TreeNodeId, targetTreeNodeId, AbilityTreeLinkType.Prerequisite);
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            selectedTreeNodeId = targetTreeNodeId;
            builderMessage = RequiresAnyPrerequisiteMode(parent, target)
                ? "排他経路から合流しました。合流先の前提条件を「いずれか一つで可」に変更してください。"
                : string.Empty;
            Repaint();
        }

        private void DrawNodeCandidates(Action<AbilityNodeMasterData> selectNode)
        {
            var guids = AssetDatabase.FindAssets("t:AbilityNodeMasterData");
            if (guids.Length == 0)
            {
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("追加できるNode定義", EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var node = AssetDatabase.LoadAssetAtPath<AbilityNodeMasterData>(path);
                    if (node == null)
                    {
                        continue;
                    }

                    if (GUILayout.Button(node.DisplayName, EditorStyles.miniButton))
                    {
                        selectNode(node);
                        builderMessage = string.Empty;
                    }
                }
            }
        }

        private string AddNodeEntry(SerializedObject serializedTree, AbilityNodeMasterData node)
        {
            var entries = serializedTree.FindProperty("nodeEntries");
            var treeNodeId = BuildNextTreeNodeId(entries, node);
            entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            entry.FindPropertyRelative("treeNodeId").stringValue = treeNodeId;
            entry.FindPropertyRelative("node").objectReferenceValue = node;
            entry.FindPropertyRelative("prerequisiteMode").enumValueIndex = (int)AbilityPrerequisiteMode.All;
            return treeNodeId;
        }

        private static string BuildNextTreeNodeId(SerializedProperty entries, AbilityNodeMasterData node)
        {
            var baseId = string.IsNullOrWhiteSpace(node.NodeId) ? "node" : node.NodeId.Trim();
            var candidate = baseId;
            var suffix = 2;
            while (ContainsTreeNodeId(entries, candidate)) candidate = $"{baseId}.{suffix++}";
            return candidate;
        }

        private static bool ContainsTreeNodeId(SerializedProperty entries, string treeNodeId)
        {
            for (var i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("treeNodeId").stringValue == treeNodeId) return true;
            }
            return false;
        }

        private static void AddLink(
            SerializedObject serializedTree,
            string fromTreeNodeId,
            string toTreeNodeId,
            AbilityTreeLinkType linkType)
        {
            var links = serializedTree.FindProperty("links");
            links.arraySize++;
            var link = links.GetArrayElementAtIndex(links.arraySize - 1);
            link.FindPropertyRelative("linkType").enumValueIndex = (int)linkType;
            link.FindPropertyRelative("fromTreeNodeId").stringValue = fromTreeNodeId;
            link.FindPropertyRelative("toTreeNodeId").stringValue = toTreeNodeId;
            link.FindPropertyRelative("requiredLevel").intValue = 1;
        }

        private List<AbilityTreeNodeEntry> GetRootNodes()
        {
            var incomingNodeIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType == AbilityTreeLinkType.Prerequisite && !string.IsNullOrWhiteSpace(link.ToTreeNodeId))
                {
                    incomingNodeIds.Add(link.ToTreeNodeId);
                }
            }

            var roots = new List<AbilityTreeNodeEntry>();
            for (var i = 0; i < tree.NodeEntries.Count; i++)
            {
                var entry = tree.NodeEntries[i];
                if (entry?.Node != null && !incomingNodeIds.Contains(entry.TreeNodeId))
                {
                    roots.Add(entry);
                }
            }

            return roots;
        }

        private List<AbilityTreeNodeEntry> GetTreeNodes()
        {
            var nodes = new List<AbilityTreeNodeEntry>();
            if (tree == null)
            {
                return nodes;
            }

            for (var i = 0; i < tree.NodeEntries.Count; i++)
            {
                var entry = tree.NodeEntries[i];
                if (entry?.Node != null) nodes.Add(entry);
            }

            return nodes;
        }

        private List<AbilityTreeNodeEntry> GetChildren(string treeNodeId)
        {
            var children = new List<AbilityTreeNodeEntry>();
            if (tree == null || string.IsNullOrWhiteSpace(treeNodeId))
            {
                return children;
            }

            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType == AbilityTreeLinkType.Prerequisite && link.FromTreeNodeId == treeNodeId)
                {
                    var child = tree.FindNodeEntry(link.ToTreeNodeId);
                    if (child?.Node != null) children.Add(child);
                }
            }

            return children;
        }

        private List<AbilityTreeNodeEntry> GetParents(string treeNodeId)
        {
            var parents = new List<AbilityTreeNodeEntry>();
            if (tree == null || string.IsNullOrWhiteSpace(treeNodeId)) return parents;
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType != AbilityTreeLinkType.Prerequisite || link.ToTreeNodeId != treeNodeId) continue;
                var parent = tree.FindNodeEntry(link.FromTreeNodeId);
                if (parent?.Node != null) parents.Add(parent);
            }
            return parents;
        }

        private bool HasExclusiveLink(string firstTreeNodeId, string secondTreeNodeId)
        {
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType != AbilityTreeLinkType.Exclusive)
                {
                    continue;
                }

                if ((link.FromTreeNodeId == firstTreeNodeId && link.ToTreeNodeId == secondTreeNodeId) ||
                    (link.FromTreeNodeId == secondTreeNodeId && link.ToTreeNodeId == firstTreeNodeId))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPrerequisiteLink(string fromTreeNodeId, string toTreeNodeId)
        {
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType == AbilityTreeLinkType.Prerequisite &&
                    link.FromTreeNodeId == fromTreeNodeId && link.ToTreeNodeId == toTreeNodeId) return true;
            }
            return false;
        }

        private bool HasExclusiveLineageConflict(string firstTreeNodeId, string secondTreeNodeId)
        {
            var firstLineage = CollectAncestorIds(firstTreeNodeId);
            var secondLineage = CollectAncestorIds(secondTreeNodeId);
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link?.LinkType != AbilityTreeLinkType.Exclusive) continue;
                if ((firstLineage.Contains(link.FromTreeNodeId) && secondLineage.Contains(link.ToTreeNodeId)) ||
                    (firstLineage.Contains(link.ToTreeNodeId) && secondLineage.Contains(link.FromTreeNodeId))) return true;
            }
            return false;
        }

        private HashSet<string> CollectAncestorIds(string treeNodeId)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Stack<string>();
            pending.Push(treeNodeId);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!ids.Add(current)) continue;
                var parents = GetParents(current);
                for (var i = 0; i < parents.Count; i++) pending.Push(parents[i].TreeNodeId);
            }
            return ids;
        }

        private bool HasPath(string fromTreeNodeId, string targetTreeNodeId, HashSet<string> visited)
        {
            if (!visited.Add(fromTreeNodeId)) return false;
            if (fromTreeNodeId == targetTreeNodeId) return true;
            var children = GetChildren(fromTreeNodeId);
            for (var i = 0; i < children.Count; i++)
            {
                if (HasPath(children[i].TreeNodeId, targetTreeNodeId, visited)) return true;
            }
            return false;
        }

        private AbilityPrerequisiteMode GetPrerequisiteMode(string treeNodeId)
        {
            return tree?.FindNodeEntry(treeNodeId)?.PrerequisiteMode ?? AbilityPrerequisiteMode.All;
        }

        private void SetPrerequisiteMode(string treeNodeId, AbilityPrerequisiteMode mode)
        {
            Undo.RecordObject(tree, "Set Ability Tree Prerequisite Mode");
            var serializedTree = new SerializedObject(tree);
            var entries = serializedTree.FindProperty("nodeEntries");
            for (var i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("treeNodeId").stringValue != treeNodeId)
                {
                    continue;
                }

                entry.FindPropertyRelative("prerequisiteMode").enumValueIndex = (int)mode;
                serializedTree.ApplyModifiedProperties();
                EditorUtility.SetDirty(tree);
                return;
            }
        }

        private void ClearTree()
        {
            if (!EditorUtility.DisplayDialog(
                    "Treeをすべてクリア",
                    "このTreeから全Nodeと全接続を削除します。Undoはできます。",
                    "すべてクリア",
                    "キャンセル"))
            {
                return;
            }

            Undo.RecordObject(tree, "Clear Ability Tree");
            var serializedTree = new SerializedObject(tree);
            serializedTree.FindProperty("nodeEntries").ClearArray();
            serializedTree.FindProperty("links").ClearArray();
            serializedTree.ApplyModifiedProperties();
            EditorUtility.SetDirty(tree);
            selectedTreeNodeId = string.Empty;
            existingTargetTreeNodeId = string.Empty;
            rootNodeToAdd = null;
            nextNodeToAdd = null;
            firstBranchNodeToAdd = null;
            secondBranchNodeToAdd = null;
            createBranch = false;
            Repaint();
        }
    }
}
#endif
