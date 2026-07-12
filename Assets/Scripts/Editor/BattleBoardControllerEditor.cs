using Assets.Scripts.Features.Battle.ViewControllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(BattleBoardController))]
    public class BattleBoardControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _cellRootProperty;
        private SerializedProperty _cellPrefabProperty;
        private SerializedProperty _gridLayoutGroupProperty;
        private SerializedProperty _editorPreviewWidthProperty;
        private SerializedProperty _editorPreviewHeightProperty;
        private SerializedProperty _authoredCellViewsProperty;

        private void OnEnable()
        {
            _cellRootProperty = serializedObject.FindProperty("cellRoot");
            _cellPrefabProperty = serializedObject.FindProperty("cellPrefab");
            _gridLayoutGroupProperty = serializedObject.FindProperty("gridLayoutGroup");
            _editorPreviewWidthProperty = serializedObject.FindProperty("editorPreviewWidth");
            _editorPreviewHeightProperty = serializedObject.FindProperty("editorPreviewHeight");
            _authoredCellViewsProperty = serializedObject.FindProperty("authoredCellViews");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inspector Board Generation", EditorStyles.boldLabel);

            if (GUILayout.Button("Inspector 盤面を生成 / 更新", GUILayout.Height(28f)))
            {
                GenerateBoardCells();
            }

            if (GUILayout.Button("既存子セルを参照リストに同期", GUILayout.Height(24f)))
            {
                SyncExistingChildCells();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateBoardCells()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            var controller = (BattleBoardController)target;
            var root = ResolveCellRoot(controller);
            var prefab = _cellPrefabProperty.objectReferenceValue as BattleCellView;
            if (root == null || prefab == null)
            {
                Debug.LogWarning("[BattleBoardEditor] cellRoot と cellPrefab を設定してから生成してください。");
                return;
            }

            var existingCount = CountDirectChildCells(root);
            if (existingCount > 0 &&
                !EditorUtility.DisplayDialog(
                    "BattleBoard を再生成",
                    $"cellRoot 直下の BattleCellView {existingCount} 個を削除して作り直します。",
                    "生成する",
                    "キャンセル"))
            {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Generate Battle Board Cells");
            DestroyDirectChildCells(root);

            var width = Mathf.Max(1, _editorPreviewWidthProperty.intValue);
            var height = Mathf.Max(1, _editorPreviewHeightProperty.intValue);
            var total = width * height;
            ConfigureLayout(root, width);

            _authoredCellViewsProperty.arraySize = total;
            for (var i = 0; i < total; i++)
            {
                var x = i % width;
                var y = i / width;
                var instance = PrefabUtility.InstantiatePrefab(prefab.gameObject, root) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Generate Battle Board Cell");
                instance.name = $"BattleCellView ({x},{y})";
                instance.transform.SetSiblingIndex(i);

                if (instance.transform is RectTransform rectTransform)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localRotation = Quaternion.identity;
                }

                _authoredCellViewsProperty
                    .GetArrayElementAtIndex(i)
                    .objectReferenceValue = instance.GetComponent<BattleCellView>();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            MarkSceneDirty(controller);
        }

        private void SyncExistingChildCells()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            var controller = (BattleBoardController)target;
            var root = ResolveCellRoot(controller);
            if (root == null)
            {
                Debug.LogWarning("[BattleBoardEditor] cellRoot を設定してから同期してください。");
                return;
            }

            _authoredCellViewsProperty.ClearArray();
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child == null || !child.TryGetComponent<BattleCellView>(out var view))
                {
                    continue;
                }

                var index = _authoredCellViewsProperty.arraySize;
                _authoredCellViewsProperty.InsertArrayElementAtIndex(index);
                _authoredCellViewsProperty
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue = view;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            MarkSceneDirty(controller);
        }

        private RectTransform ResolveCellRoot(BattleBoardController controller)
        {
            var root = _cellRootProperty.objectReferenceValue as RectTransform;
            if (root == null)
            {
                root = controller.transform as RectTransform;
                _cellRootProperty.objectReferenceValue = root;
            }

            if (_gridLayoutGroupProperty.objectReferenceValue == null && root != null)
            {
                var layout = root.GetComponent<GridLayoutGroup>();
                if (layout == null)
                {
                    layout = Undo.AddComponent<GridLayoutGroup>(root.gameObject);
                }

                _gridLayoutGroupProperty.objectReferenceValue = layout;
            }

            return root;
        }

        private void ConfigureLayout(RectTransform root, int width)
        {
            var layout = _gridLayoutGroupProperty.objectReferenceValue as GridLayoutGroup;
            if (layout == null && root != null)
            {
                layout = root.GetComponent<GridLayoutGroup>();
            }

            if (layout == null)
            {
                return;
            }

            Undo.RecordObject(layout, "Configure Battle Board Grid");
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = width;
            EditorUtility.SetDirty(layout);
        }

        private static int CountDirectChildCells(RectTransform root)
        {
            var count = 0;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child != null && child.GetComponent<BattleCellView>() != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static void DestroyDirectChildCells(RectTransform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child != null && child.GetComponent<BattleCellView>() != null)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        private static void MarkSceneDirty(BattleBoardController controller)
        {
            if (controller != null && controller.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }
        }
    }
}
