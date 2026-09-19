using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.UI.Components
{
    public class PartsTabItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Image normalIconImage;
        [SerializeField] private TextMeshProUGUI normalText;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private Image focusedIconImage;
        [SerializeField] private TextMeshProUGUI focusedText;
        [SerializeField] private GameObject alertIcon;

        [Header("Editor Preview")]
        [SerializeField] private Sprite previewIcon;
        [SerializeField] private string previewText;

        public Button Button => button;

        public void SetAlertIconActive(bool isActive)
        {
            alertIcon.SetActive(isActive);
        }

        public void SetNormalState(string text = null, Sprite icon = null)
        {
            if (text != null)
            {
                normalText.text = text;
            }
            if (icon != null)
            {
                normalIconImage.sprite = icon;
            }
            normalState.SetActive(true);
            focusedState.SetActive(false);
        }

        public void SetFocusedState(string text = null, Sprite icon = null)
        {
            if (text != null)
            {
                focusedText.text = text;
            }
            if (icon != null)
            {
                focusedIconImage.sprite = icon;
            }
            normalState.SetActive(false);
            focusedState.SetActive(true);
        }

    #if UNITY_EDITOR
        [ContextMenu("Apply Editor Preview")]
        public void ApplyEditorPreview()
        {
            Undo.RecordObjects(
                new Object[] { this, normalText, focusedText, normalIconImage, focusedIconImage },
                "Apply Parts Tab Preview");

            if (previewText != null)
            {
                normalText.text = previewText;
                focusedText.text = previewText;
            }

            if (previewIcon != null)
            {
                normalIconImage.sprite = previewIcon;
                focusedIconImage.sprite = previewIcon;
            }

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(normalText);
            EditorUtility.SetDirty(focusedText);
            EditorUtility.SetDirty(normalIconImage);
            EditorUtility.SetDirty(focusedIconImage);
        }
    #endif
    }

    #if UNITY_EDITOR
        [CustomEditor(typeof(PartsTabItem))]
    public sealed class PartsTabItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply Editor Preview"))
            {
                ((PartsTabItem)target).ApplyEditorPreview();
            }
        }
    }
    #endif
}