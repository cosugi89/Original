using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Core;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// StageBattlePatternMasterData の盤面をクリック編集する専用ウィンドウ。
    /// 保存するまでは asset 本体を変更せず、このウィンドウ内だけでドラフト編集する。
    /// </summary>
    public class StageBattlePatternBoardEditorWindow : EditorWindow
    {
        private enum PaintTool
        {
            Start,
            Erase,
            Attack,
            Jump,
            Roll,
            Dance,
            HazardNormal,
            HazardSkill,
            Goal,
        }

        private sealed class PatternDraft
        {
            public int PatternId;
            public string Label = "Turn";
            public string Description = string.Empty;
            public BattleEnemyActionType EnemyAction = BattleEnemyActionType.NormalAttack;
            public string ConfirmText = string.Empty;
            public int DamageMultiplier = 100;
            public int Width = 5;
            public int Height = 6;
            public int StartX = 2;
            public int StartY = 5;
            public readonly Dictionary<Vector2Int, BattleNodeType> Placements = new();

            public void Normalize()
            {
                Width = Mathf.Max(1, Width);
                Height = Mathf.Max(1, Height);
                StartX = Mathf.Clamp(StartX, 0, Width - 1);
                StartY = Mathf.Clamp(StartY, 0, Height - 1);
                DamageMultiplier = Mathf.Max(0, DamageMultiplier);

                var invalidKeys = new List<Vector2Int>();
                foreach (var pair in Placements)
                {
                    if (pair.Key.x < 0 ||
                        pair.Key.x >= Width ||
                        pair.Key.y < 0 ||
                        pair.Key.y >= Height ||
                        (pair.Key.x == StartX && pair.Key.y == StartY) ||
                        pair.Value == BattleNodeType.Empty ||
                        pair.Value == BattleNodeType.Start)
                    {
                        invalidKeys.Add(pair.Key);
                    }
                }

                for (var i = 0; i < invalidKeys.Count; i++)
                {
                    Placements.Remove(invalidKeys[i]);
                }
            }
        }

        private enum ValidationSeverity
        {
            Error,
            Warning,
            Info,
        }

        private readonly struct ValidationIssue
        {
            public ValidationIssue(ValidationSeverity severity, string message)
            {
                Severity = severity;
                Message = message;
            }

            public ValidationSeverity Severity { get; }

            public string Message { get; }
        }

        private const string MenuPath = "Tools/Original/Battle Pattern Board Editor";
        private const float MinCellSize = 44f;
        private const float MaxCellSize = 84f;

        private StageBattlePatternMasterData _pattern;
        private PatternDraft _draft;
        private PaintTool _paintTool = PaintTool.Attack;
        private Vector2 _scrollPosition;
        private float _cellSize = 60f;
        private bool _hasUnsavedChanges;

        [MenuItem(MenuPath)]
        private static void OpenWindow()
        {
            var window = GetWindow<StageBattlePatternBoardEditorWindow>();
            window.titleContent = new GUIContent("Battle Pattern");
            window.minSize = new Vector2(640f, 520f);
            window.TryAssignFromSelection();
        }

        private void OnEnable()
        {
            TryAssignFromSelection();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is not StageBattlePatternMasterData selectedPattern)
            {
                return;
            }

            if (_hasUnsavedChanges && selectedPattern != _pattern)
            {
                ShowNotification(new GUIContent("未保存の変更があるため自動切替を保留しています"));
                return;
            }

            AssignPattern(selectedPattern);
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();

            if (_pattern == null || _draft == null)
            {
                EditorGUILayout.HelpBox("StageBattlePatternMasterData を選ぶと、ここで盤面をクリック編集できます。", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("基本情報", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _draft.PatternId = EditorGUILayout.IntField("Pattern Id", _draft.PatternId);
            _draft.Label = EditorGUILayout.TextField("Label", _draft.Label ?? string.Empty);
            EditorGUILayout.LabelField("Description");
            _draft.Description = EditorGUILayout.TextArea(_draft.Description ?? string.Empty, GUILayout.MinHeight(54f));
            _draft.EnemyAction = (BattleEnemyActionType)EditorGUILayout.EnumPopup("Enemy Action", _draft.EnemyAction);
            _draft.ConfirmText = EditorGUILayout.TextField("Confirm Text", _draft.ConfirmText ?? string.Empty);
            _draft.DamageMultiplier = Mathf.Max(0, EditorGUILayout.IntField("Damage Multiplier (%)", _draft.DamageMultiplier));
            _draft.Width = Mathf.Max(1, EditorGUILayout.IntField("Width", _draft.Width));
            _draft.Height = Mathf.Max(1, EditorGUILayout.IntField("Height", _draft.Height));
            _draft.StartX = Mathf.Clamp(EditorGUILayout.IntField("Start X", _draft.StartX), 0, _draft.Width - 1);
            _draft.StartY = Mathf.Clamp(EditorGUILayout.IntField("Start Y", _draft.StartY), 0, _draft.Height - 1);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDraftChanged();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("盤面操作", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Start は開始位置を動かします。消去は配置済みノードを取り除きます。危険エリアは隣接する危険マスを runtime で自動グループ化します。", MessageType.None);
            _cellSize = EditorGUILayout.Slider("Cell Size", _cellSize, MinCellSize, MaxCellSize);
            DrawPaintToolbar();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("盤面ノードを全消去", GUILayout.Height(24f)))
            {
                _draft.Placements.Clear();
                MarkDraftChanged();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("変更を破棄", GUILayout.Height(24f)))
            {
                ReloadDraftFromPattern();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("選択 asset を Ping", GUILayout.Height(24f)))
            {
                EditorGUIUtility.PingObject(_pattern);
            }

            if (GUILayout.Button("Assets を保存", GUILayout.Height(24f)))
            {
                SaveDraftToAsset();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            _draft.Normalize();
            DrawValidationSummary(_draft);
            DrawBoard(_draft.Width, _draft.Height, _draft.StartX, _draft.StartY, _draft.Placements);

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Battle Pattern Board Editor", EditorStyles.boldLabel);

            var nextPattern = (StageBattlePatternMasterData)EditorGUILayout.ObjectField("Pattern", _pattern, typeof(StageBattlePatternMasterData), false);
            if (nextPattern != _pattern)
            {
                if (!TryConfirmDiscardDraft())
                {
                    nextPattern = _pattern;
                }
                else
                {
                    AssignPattern(nextPattern);
                }
            }

            if (_pattern != null)
            {
                EditorGUILayout.LabelField(_hasUnsavedChanges ? "Status: 未保存の変更あり" : "Status: 保存済み", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("選択中を使う", GUILayout.Height(22f)))
            {
                if (TryConfirmDiscardDraft())
                {
                    TryAssignFromSelection();
                }
            }

            if (GUILayout.Button("作成メニューを開く", GUILayout.Height(22f)))
            {
                EditorApplication.ExecuteMenuItem("Assets/Create/Original/Master Data/Battle Pattern");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPaintToolbar()
        {
            DrawPaintToolbarRow(PaintTool.Start, PaintTool.Erase, PaintTool.Attack, PaintTool.Goal);
            DrawPaintToolbarRow(PaintTool.Jump, PaintTool.Roll, PaintTool.Dance, PaintTool.HazardNormal, PaintTool.HazardSkill);
        }

        private void DrawPaintToolbarRow(params PaintTool[] tools)
        {
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < tools.Length; i++)
            {
                var tool = tools[i];
                var visual = GetToolVisual(tool);
                var previousColor = GUI.backgroundColor;
                GUI.backgroundColor = _paintTool == tool ? visual.BackgroundColor * 1.05f : visual.BackgroundColor;
                if (GUILayout.Toggle(_paintTool == tool, visual.Label, "Button", GUILayout.Height(34f)))
                {
                    _paintTool = tool;
                }
                GUI.backgroundColor = previousColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBoard(
            int width,
            int height,
            int startX,
            int startY,
            Dictionary<Vector2Int, BattleNodeType> placements)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField($"Board Preview ({width} x {height})", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30f);
            for (var x = 0; x < width; x++)
            {
                GUILayout.Label(x.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(_cellSize));
            }
            EditorGUILayout.EndHorizontal();

            for (var y = 0; y < height; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(y.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(24f));

                for (var x = 0; x < width; x++)
                {
                    var isStart = x == startX && y == startY;
                    var storedNodeType = placements.TryGetValue(new Vector2Int(x, y), out var placedNodeType)
                        ? placedNodeType
                        : BattleNodeType.Empty;
                    var nodeType = isStart ? BattleNodeType.Start : storedNodeType;
                    var visual = GetNodeVisual(nodeType, isStart);
                    var rect = GUILayoutUtility.GetRect(_cellSize, _cellSize, GUILayout.Width(_cellSize), GUILayout.Height(_cellSize));

                    Handles.DrawSolidRectangleWithOutline(rect, visual.BackgroundColor, visual.OutlineColor);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        ApplyPaint(width, height, startX, startY, x, y);
                        GUIUtility.ExitGUI();
                    }

                    var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = true,
                        fontSize = 10,
                        normal = { textColor = visual.TextColor },
                    };
                    GUI.Label(rect, visual.Label, labelStyle);

                    var coordinateRect = new Rect(rect.x + 4f, rect.y + 2f, rect.width - 8f, 12f);
                    var coordinateStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.UpperLeft,
                        normal = { textColor = visual.TextColor * 0.9f },
                    };
                    GUI.Label(coordinateRect, $"{x},{y}", coordinateStyle);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void ApplyPaint(int width, int height, int startX, int startY, int x, int y)
        {
            if (_draft == null)
            {
                return;
            }

            var key = new Vector2Int(x, y);

            if (_paintTool == PaintTool.Start)
            {
                _draft.StartX = x;
                _draft.StartY = y;
                _draft.Placements.Remove(key);
                MarkDraftChanged();
                return;
            }

            if (x == startX && y == startY)
            {
                ShowNotification(new GUIContent("Start マスに別ノードは置けません"));
                return;
            }

            if (_paintTool == PaintTool.Erase)
            {
                _draft.Placements.Remove(key);
            }
            else
            {
                _draft.Placements[key] = ToNodeType(_paintTool);
            }

            MarkDraftChanged();
        }

        private void AssignPattern(StageBattlePatternMasterData pattern)
        {
            _pattern = pattern;
            ReloadDraftFromPattern();
        }

        private void ReloadDraftFromPattern()
        {
            if (_pattern == null)
            {
                _draft = null;
                _hasUnsavedChanges = false;
                Repaint();
                return;
            }

            var serializedPattern = new SerializedObject(_pattern);
            serializedPattern.Update();

            var patternIdProperty = serializedPattern.FindProperty("patternId");
            var labelProperty = serializedPattern.FindProperty("label");
            var descriptionProperty = serializedPattern.FindProperty("description");
            var enemyActionProperty = serializedPattern.FindProperty("enemyAction");
            var confirmTextProperty = serializedPattern.FindProperty("confirmText");
            var damageMultiplierProperty = serializedPattern.FindProperty("damageMultiplier");
            var boardProperty = serializedPattern.FindProperty("board");

            _draft = new PatternDraft
            {
                PatternId = patternIdProperty.intValue,
                Label = labelProperty.stringValue,
                Description = descriptionProperty.stringValue,
                EnemyAction = (BattleEnemyActionType)enemyActionProperty.enumValueIndex,
                ConfirmText = confirmTextProperty.stringValue,
                DamageMultiplier = damageMultiplierProperty.intValue,
                Width = Mathf.Max(1, boardProperty.FindPropertyRelative("width").intValue),
                Height = Mathf.Max(1, boardProperty.FindPropertyRelative("height").intValue),
                StartX = boardProperty.FindPropertyRelative("startX").intValue,
                StartY = boardProperty.FindPropertyRelative("startY").intValue,
            };

            var placements = BuildPlacementMap(
                serializedPattern.FindProperty("cellPlacements"),
                _draft.Width,
                _draft.Height,
                _draft.StartX,
                _draft.StartY);

            foreach (var pair in placements)
            {
                _draft.Placements[pair.Key] = pair.Value;
            }

            _draft.Normalize();
            _hasUnsavedChanges = false;
            Repaint();
        }

        private void SaveDraftToAsset()
        {
            if (_pattern == null || _draft == null)
            {
                return;
            }

            _draft.Normalize();
            var validationIssues = BuildValidationIssues(_draft);
            if (HasBlockingValidationIssue(validationIssues))
            {
                ShowNotification(new GUIContent("保存できない問題があります"));
                return;
            }

            Undo.RecordObject(_pattern, "Save Battle Pattern Board");
            var serializedPattern = new SerializedObject(_pattern);
            serializedPattern.Update();

            serializedPattern.FindProperty("patternId").intValue = _draft.PatternId;
            serializedPattern.FindProperty("label").stringValue = _draft.Label ?? string.Empty;
            serializedPattern.FindProperty("description").stringValue = _draft.Description ?? string.Empty;
            serializedPattern.FindProperty("enemyAction").enumValueIndex = (int)_draft.EnemyAction;
            serializedPattern.FindProperty("confirmText").stringValue = _draft.ConfirmText ?? string.Empty;
            serializedPattern.FindProperty("damageMultiplier").intValue = Mathf.Max(0, _draft.DamageMultiplier);

            var boardProperty = serializedPattern.FindProperty("board");
            boardProperty.FindPropertyRelative("width").intValue = _draft.Width;
            boardProperty.FindPropertyRelative("height").intValue = _draft.Height;
            boardProperty.FindPropertyRelative("startX").intValue = _draft.StartX;
            boardProperty.FindPropertyRelative("startY").intValue = _draft.StartY;

            RewritePlacements(serializedPattern.FindProperty("cellPlacements"), _draft.Placements);
            serializedPattern.ApplyModifiedProperties();
            EditorUtility.SetDirty(_pattern);
            AssetDatabase.SaveAssets();

            _hasUnsavedChanges = false;
            ShowNotification(new GUIContent("Pattern を保存しました"));
            Repaint();
        }

        private void MarkDraftChanged()
        {
            if (_draft == null)
            {
                return;
            }

            _draft.Normalize();
            _hasUnsavedChanges = true;
            Repaint();
        }

        private void DrawValidationSummary(PatternDraft draft)
        {
            var issues = BuildValidationIssues(draft);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("保存前チェック: 問題ありません。", MessageType.Info);
                return;
            }

            for (var i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                EditorGUILayout.HelpBox(
                    $"保存前チェック: {issue.Message}",
                    issue.Severity switch
                    {
                        ValidationSeverity.Error => MessageType.Error,
                        ValidationSeverity.Warning => MessageType.Warning,
                        _ => MessageType.Info,
                    });
            }
        }

        private bool TryConfirmDiscardDraft()
        {
            return !_hasUnsavedChanges ||
                   EditorUtility.DisplayDialog(
                       "未保存の変更があります",
                       "現在のドラフト変更を破棄して切り替えますか？",
                       "破棄して続行",
                       "キャンセル");
        }

        private static List<ValidationIssue> BuildValidationIssues(PatternDraft draft)
        {
            var issues = new List<ValidationIssue>();
            if (draft == null)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, "ドラフトが読み込まれていません。"));
                return issues;
            }

            AddGoalValidationIssue(draft, issues);
            return issues;
        }

        private static void AddGoalValidationIssue(PatternDraft draft, List<ValidationIssue> issues)
        {
            var goalCount = 0;
            foreach (var pair in draft.Placements)
            {
                if (pair.Value == BattleNodeType.Goal)
                {
                    goalCount++;
                }
            }

            if (goalCount <= 0)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, "終了ノード (Goal) を 1 つ以上配置してください。"));
            }
        }

        private static bool HasBlockingValidationIssue(List<ValidationIssue> issues)
        {
            if (issues == null)
            {
                return false;
            }

            for (var i = 0; i < issues.Count; i++)
            {
                if (issues[i].Severity == ValidationSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }

        private static Dictionary<Vector2Int, BattleNodeType> BuildPlacementMap(
            SerializedProperty cellPlacementsProperty,
            int width,
            int height,
            int startX,
            int startY)
        {
            var result = new Dictionary<Vector2Int, BattleNodeType>();
            for (var i = 0; i < cellPlacementsProperty.arraySize; i++)
            {
                var element = cellPlacementsProperty.GetArrayElementAtIndex(i);
                var x = element.FindPropertyRelative("x").intValue;
                var y = element.FindPropertyRelative("y").intValue;
                var nodeType = (BattleNodeType)element.FindPropertyRelative("nodeType").enumValueIndex;
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }

                if ((x == startX && y == startY) || nodeType == BattleNodeType.Empty || nodeType == BattleNodeType.Start)
                {
                    continue;
                }

                result[new Vector2Int(x, y)] = nodeType;
            }

            return result;
        }

        private static void RewritePlacements(SerializedProperty cellPlacementsProperty, Dictionary<Vector2Int, BattleNodeType> placements)
        {
            var orderedPlacements = placements
                .OrderBy(pair => pair.Key.y)
                .ThenBy(pair => pair.Key.x)
                .ToList();

            cellPlacementsProperty.arraySize = orderedPlacements.Count;
            for (var i = 0; i < orderedPlacements.Count; i++)
            {
                var element = cellPlacementsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("x").intValue = orderedPlacements[i].Key.x;
                element.FindPropertyRelative("y").intValue = orderedPlacements[i].Key.y;
                element.FindPropertyRelative("nodeType").enumValueIndex = (int)orderedPlacements[i].Value;
            }
        }

        private void TryAssignFromSelection()
        {
            if (Selection.activeObject is StageBattlePatternMasterData selectedPattern)
            {
                AssignPattern(selectedPattern);
            }
        }

        private static BattleNodeType ToNodeType(PaintTool paintTool)
        {
            return paintTool switch
            {
                PaintTool.Attack => BattleNodeType.Attack,
                PaintTool.Jump => BattleNodeType.Jump,
                PaintTool.Roll => BattleNodeType.Roll,
                PaintTool.Dance => BattleNodeType.Dance,
                PaintTool.HazardNormal => BattleNodeType.HazardNormal,
                PaintTool.HazardSkill => BattleNodeType.HazardSkill,
                PaintTool.Goal => BattleNodeType.Goal,
                _ => BattleNodeType.Empty,
            };
        }

        private static CellVisual GetToolVisual(PaintTool tool)
        {
            return tool switch
            {
                PaintTool.Start => new CellVisual("Start", new Color(0.18f, 0.64f, 0.73f, 0.98f), Color.white, new Color(0.72f, 0.96f, 1f, 1f)),
                PaintTool.Erase => new CellVisual("消去", new Color(0.8f, 0.8f, 0.8f, 0.96f), new Color(0.18f, 0.18f, 0.18f, 1f), new Color(0.45f, 0.45f, 0.45f, 1f)),
                PaintTool.Attack => GetNodeVisual(BattleNodeType.Attack, false),
                PaintTool.Jump => GetNodeVisual(BattleNodeType.Jump, false),
                PaintTool.Roll => GetNodeVisual(BattleNodeType.Roll, false),
                PaintTool.Dance => GetNodeVisual(BattleNodeType.Dance, false),
                PaintTool.HazardNormal => GetNodeVisual(BattleNodeType.HazardNormal, false),
                PaintTool.HazardSkill => GetNodeVisual(BattleNodeType.HazardSkill, false),
                PaintTool.Goal => GetNodeVisual(BattleNodeType.Goal, false),
                _ => GetNodeVisual(BattleNodeType.Empty, false),
            };
        }

        private static CellVisual GetNodeVisual(BattleNodeType nodeType, bool isStart)
        {
            if (isStart || nodeType == BattleNodeType.Start)
            {
                return new CellVisual("START\n開始", new Color(0.18f, 0.64f, 0.73f, 0.98f), Color.white, new Color(0.72f, 0.96f, 1f, 1f));
            }

            return nodeType switch
            {
                BattleNodeType.Goal => new CellVisual("GOAL\n到達", new Color(0.14f, 0.72f, 0.38f, 0.98f), Color.white, new Color(0.73f, 1f, 0.82f, 1f)),
                BattleNodeType.Attack => new CellVisual("ATTACK\n攻撃", new Color(0.84f, 0.24f, 0.22f, 0.98f), Color.white, new Color(1f, 0.77f, 0.72f, 1f)),
                BattleNodeType.Jump => new CellVisual("JUMP\n跳躍", new Color(0.25f, 0.56f, 0.96f, 0.98f), Color.white, new Color(0.77f, 0.88f, 1f, 1f)),
                BattleNodeType.Roll => new CellVisual("ROLL\n回避", new Color(0.93f, 0.58f, 0.15f, 0.98f), Color.white, new Color(1f, 0.87f, 0.68f, 1f)),
                BattleNodeType.Dance => new CellVisual("DANCE\n舞踏", new Color(0.82f, 0.34f, 0.53f, 0.98f), Color.white, new Color(1f, 0.77f, 0.89f, 1f)),
                BattleNodeType.HazardNormal => new CellVisual("DANGER\n危険", new Color(0.25f, 0.26f, 0.3f, 0.98f), new Color(1f, 0.84f, 0.84f, 1f), new Color(1f, 0.36f, 0.36f, 1f)),
                BattleNodeType.HazardSkill => new CellVisual("SKILL\n危険", new Color(0.2f, 0.28f, 0.48f, 0.98f), new Color(0.92f, 0.98f, 1f, 1f), new Color(0.47f, 0.88f, 1f, 1f)),
                _ => new CellVisual(string.Empty, new Color(0.88f, 0.84f, 0.8f, 0.92f), new Color(0.33f, 0.29f, 0.26f, 1f), new Color(0.98f, 0.95f, 0.9f, 1f)),
            };
        }

        private readonly struct CellVisual
        {
            public CellVisual(string label, Color backgroundColor, Color textColor, Color outlineColor)
            {
                Label = label;
                BackgroundColor = backgroundColor;
                TextColor = textColor;
                OutlineColor = outlineColor;
            }

            public string Label { get; }

            public Color BackgroundColor { get; }

            public Color TextColor { get; }

            public Color OutlineColor { get; }
        }
    }
}
