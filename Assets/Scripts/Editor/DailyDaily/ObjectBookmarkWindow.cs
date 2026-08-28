using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    // ブックマークの保持・永続化・選択切り替え。
    // ショートカット（static メソッド）からも呼ぶため、データは static で保持する。
    internal static class ObjectBookmarkData
    {
        // プロジェクト単位で保存。プロジェクト直下の UserSettings 内ファイルに格納する。
        // EditorPrefs はマシン全体で共有されるため、productGUID が衝突・流用される
        // 新規プロジェクト間でブックマークが漏れていた。プロジェクト内ファイルにすることで
        // 新規プロジェクトでは保存ファイルが無く、必ず空の状態から始まる。
        static string SavePath
        {
            get
            {
                var root = System.IO.Directory.GetParent(Application.dataPath).FullName;
                return System.IO.Path.Combine(root, "UserSettings", "DailyDaily", "ObjectBookmark.txt");
            }
        }

        static List<string> _ids;
        public static List<string> Ids
        {
            get { if (_ids == null) Load(); return _ids; }
        }

        // ウィンドウへ再描画を通知
        public static System.Action Changed;

        static void Load()
        {
            _ids = new List<string>();
            if (System.IO.File.Exists(SavePath))
            {
                var raw = System.IO.File.ReadAllText(SavePath);
                if (!string.IsNullOrEmpty(raw))
                    _ids.AddRange(raw.Split('\n'));
            }
        }

        static void Save()
        {
            var dir = System.IO.Path.GetDirectoryName(SavePath);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(SavePath, string.Join("\n", Ids));
            Changed?.Invoke();
        }

        public static bool Add(Object obj)
        {
            if (obj == null) return false;
            var id = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            if (Ids.Contains(id)) return false;
            Ids.Add(id);
            Save();
            return true;
        }

        public static void RemoveAt(int index)
        {
            if (index < 0 || index >= Ids.Count) return;
            Ids.RemoveAt(index);
            Save();
        }

        public static void Clear()
        {
            Ids.Clear();
            Save();
        }

        public static void Move(int from, int to)
        {
            if (from < 0 || from >= Ids.Count || to < 0 || to >= Ids.Count) return;
            var v = Ids[from];
            Ids.RemoveAt(from);
            Ids.Insert(to, v);
            Save();
        }

        public static Object Resolve(string id)
        {
            if (GlobalObjectId.TryParse(id, out var gid))
                return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
            return null;
        }

        // インスペクタの表示先を切り替える（＝選択を変える）
        public static void SelectBookmark(int index)
        {
            if (index < 0 || index >= Ids.Count) return;
            var obj = Resolve(Ids[index]);
            if (obj == null)
            {
                Debug.LogWarning($"[ObjectBookmark] ブックマーク {index + 1} は解決できませんでした（シーン未読込/削除済みの可能性）");
                return;
            }
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        // --- 右クリックメニューからブックマーク追加 ---
        [MenuItem("Assets/Add to Object Bookmark")]
        static void BookmarkAssets()
        {
            foreach (var o in Selection.objects) Add(o);
        }

        static double _lastGoAdd;
        [MenuItem("GameObject/Add to Object Bookmark", false, 30)]
        static void BookmarkGameObjects()
        {
            // GameObject メニューは選択数だけ呼ばれるため、二重実行を抑制
            if (EditorApplication.timeSinceStartup - _lastGoAdd < 0.1) return;
            _lastGoAdd = EditorApplication.timeSinceStartup;
            foreach (var o in Selection.objects) Add(o);
        }

        // --- ショートカット (Alt+1〜9) 既定値。Shortcut Manager で変更可 ---
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 1", KeyCode.Alpha1, ShortcutModifiers.Alt)] static void S1() => SelectBookmark(0);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 2", KeyCode.Alpha2, ShortcutModifiers.Alt)] static void S2() => SelectBookmark(1);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 3", KeyCode.Alpha3, ShortcutModifiers.Alt)] static void S3() => SelectBookmark(2);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 4", KeyCode.Alpha4, ShortcutModifiers.Alt)] static void S4() => SelectBookmark(3);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 5", KeyCode.Alpha5, ShortcutModifiers.Alt)] static void S5() => SelectBookmark(4);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 6", KeyCode.Alpha6, ShortcutModifiers.Alt)] static void S6() => SelectBookmark(5);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 7", KeyCode.Alpha7, ShortcutModifiers.Alt)] static void S7() => SelectBookmark(6);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 8", KeyCode.Alpha8, ShortcutModifiers.Alt)] static void S8() => SelectBookmark(7);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 9", KeyCode.Alpha9, ShortcutModifiers.Alt)] static void S9() => SelectBookmark(8);

        // --- ショートカット (テンキー NUM1〜9) 既定値。Shortcut Manager で変更可 ---
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 1 (Numpad)", KeyCode.Keypad1)] static void N1() => SelectBookmark(0);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 2 (Numpad)", KeyCode.Keypad2)] static void N2() => SelectBookmark(1);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 3 (Numpad)", KeyCode.Keypad3)] static void N3() => SelectBookmark(2);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 4 (Numpad)", KeyCode.Keypad4)] static void N4() => SelectBookmark(3);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 5 (Numpad)", KeyCode.Keypad5)] static void N5() => SelectBookmark(4);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 6 (Numpad)", KeyCode.Keypad6)] static void N6() => SelectBookmark(5);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 7 (Numpad)", KeyCode.Keypad7)] static void N7() => SelectBookmark(6);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 8 (Numpad)", KeyCode.Keypad8)] static void N8() => SelectBookmark(7);
        [Shortcut("DailyDaily/Object Bookmark/Select Bookmark 9 (Numpad)", KeyCode.Keypad9)] static void N9() => SelectBookmark(8);
    }

    public class ObjectBookmarkWindow : EditorWindow
    {
        Vector2 _scroll;

        [MenuItem("Tools/Object Bookmark")]
        static void Open()
        {
            var w = GetWindow<ObjectBookmarkWindow>("Object Bookmark");
            w.minSize = new Vector2(260, 220);
            w.Show();
        }

        void OnEnable() { ObjectBookmarkData.Changed += Repaint; }
        void OnDisable() { ObjectBookmarkData.Changed -= Repaint; }
        void OnSelectionChange() { Repaint(); }

        void OnGUI()
        {
            HandleArrowKeys();
            DrawToolbar();
            DrawDropArea();
            DrawList();
            HandleWindowDragAndDrop();   // ウィンドウのどこに落としても受け付ける
        }

        // ウィンドウがアクティブな時、↑↓で対象ブックマークを切り替える
        void HandleArrowKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (e.keyCode != KeyCode.UpArrow && e.keyCode != KeyCode.DownArrow) return;

            int count = ObjectBookmarkData.Ids.Count;
            if (count == 0) return;

            int cur = CurrentIndex();
            int next = e.keyCode == KeyCode.DownArrow
                ? (cur < 0 ? 0 : Mathf.Min(cur + 1, count - 1))
                : (cur < 0 ? count - 1 : Mathf.Max(cur - 1, 0));

            ObjectBookmarkData.SelectBookmark(next);
            e.Use();      // ボタン間のフォーカス移動に消費させない
            Repaint();
        }

        // 現在インスペクタに表示中のブックマークの index（該当なしは -1）
        static int CurrentIndex()
        {
            var ids = ObjectBookmarkData.Ids;
            for (int i = 0; i < ids.Count; i++)
            {
                var o = ObjectBookmarkData.Resolve(ids[i]);
                if (o != null && o == Selection.activeObject) return i;
            }
            return -1;
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("選択中を追加", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    foreach (var o in Selection.objects) ObjectBookmarkData.Add(o);
                }
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(ObjectBookmarkData.Ids.Count == 0))
                {
                    if (GUILayout.Button("すべてクリア", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    {
                        if (EditorUtility.DisplayDialog("Object Bookmark",
                                "すべてのブックマークを削除しますか？", "削除", "キャンセル"))
                            ObjectBookmarkData.Clear();
                    }
                }
            }
        }

        void DrawDropArea()
        {
            // 受付はウィンドウ全体（HandleWindowDragAndDrop）で行う。ここは案内表示のみ。
            var rect = GUILayoutUtility.GetRect(0, 34, GUILayout.ExpandWidth(true));
            rect.x += 4; rect.width -= 8; rect.y += 4;
            GUI.Box(rect, "オブジェクト / アセットをウィンドウ内にドラッグ＆ドロップ", EditorStyles.helpBox);
        }

        // ウィンドウのどこにドロップしてもブックマークに追加する
        void HandleWindowDragAndDrop()
        {
            var evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform && evt.type != EventType.DragExited)
                return;

            var full = new Rect(0, 0, position.width, position.height);
            if (!full.Contains(evt.mousePosition)) return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    // ドラッグ中は全体を薄くハイライトして「どこでも落とせる」ことを示す
                    GUI.Box(full, GUIContent.none, DropHighlight);
                    evt.Use();
                    Repaint();
                    break;
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    foreach (var o in DragAndDrop.objectReferences) ObjectBookmarkData.Add(o);
                    evt.Use();
                    break;
            }
        }

        void DrawList()
        {
            var ids = ObjectBookmarkData.Ids;
            if (ids.Count == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("ブックマークはまだありません。",
                    EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUILayout.Space(4);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int removeAt = -1, moveFrom = -1, moveTo = -1;
            for (int i = 0; i < ids.Count; i++)
            {
                var obj = ObjectBookmarkData.Resolve(ids[i]);
                bool active = obj != null && Selection.activeObject == obj;

                using (new EditorGUILayout.HorizontalScope(active ? Selected : EditorStyles.helpBox))
                {
                    // 管理ボタン（左：固定幅で先頭に置き、窓を狭めても隠れない）
                    using (new EditorGUI.DisabledScope(i == 0))
                        if (GUILayout.Button("▲", GUILayout.Width(22))) { moveFrom = i; moveTo = i - 1; }
                    using (new EditorGUI.DisabledScope(i == ids.Count - 1))
                        if (GUILayout.Button("▼", GUILayout.Width(22))) { moveFrom = i; moveTo = i + 1; }
                    if (GUILayout.Button("×", GUILayout.Width(22))) removeAt = i;

                    // 名前（中央：クリックで表示先切替。可変幅で残りを埋める）
                    if (obj != null)
                    {
                        var content = EditorGUIUtility.ObjectContent(obj, obj.GetType());
                        content.tooltip = "クリックでインスペクタに表示";
                        if (GUILayout.Button(content, NameButton, GUILayout.Height(20), GUILayout.ExpandWidth(true)))
                        {
                            Selection.activeObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                    }
                    else
                    {
                        GUILayout.Label("(未解決：シーン未読込/削除済み)",
                            EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    }

                    // ショートカット番号（右：表示専用ヒント）
                    GUILayout.Label(i < 9 ? $"⌥{i + 1}" : "  ",
                        EditorStyles.miniBoldLabel, GUILayout.Width(28));
                }
            }

            EditorGUILayout.EndScrollView();

            if (removeAt >= 0) ObjectBookmarkData.RemoveAt(removeAt);
            else if (moveFrom >= 0) ObjectBookmarkData.Move(moveFrom, moveTo);
        }

        // --- スタイル ---
        static GUIStyle _nameButton;
        static GUIStyle NameButton => _nameButton ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(4, 4, 0, 0),
        };

        static GUIStyle _selected;
        static GUIStyle Selected => _selected ??= new GUIStyle(EditorStyles.helpBox)
        {
            normal = { background = MakeTex(new Color(0.24f, 0.37f, 0.59f, 0.5f)) },
        };

        static GUIStyle _dropHighlight;
        static GUIStyle DropHighlight => _dropHighlight ??= new GUIStyle
        {
            normal = { background = MakeTex(new Color(0.24f, 0.5f, 0.85f, 0.15f)) },
        };

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }
    }
}
