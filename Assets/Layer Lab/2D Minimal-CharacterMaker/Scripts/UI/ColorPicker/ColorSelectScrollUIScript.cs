using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Preset-color picker UI component that applies colors to character parts.
    /// </summary>
    public class ColorSelectScrollUIScript : MonoBehaviour
    {
        [SerializeField] private Color[] presetColors = new[]
        {
            new Color(0.13333334f, 0.12941177f, 0.12941177f),
            new Color(0.34901962f, 0.18039216f, 0.18431373f),
            new Color(0.3764706f, 0.27058825f, 0.20784314f),
            new Color(0.54901963f, 0.47843137f, 0.45490196f),
            new Color(0.55f, 0.47843137f, 0.45490196f),
            new Color(0.78039217f, 0.63529414f, 0.45882353f),
            new Color(0.9529412f, 0.72156864f, 0.2509804f),
            new Color(0.9882353f, 0.99215686f, 0.92941177f),
            new Color(0.8980392f, 0.44313726f, 0.1882353f),
            new Color(0.6f, 0.13333334f, 0.05490196f),
            new Color(0.8901961f, 0.23529412f, 0.46666667f),
            new Color(0.9372549f, 0.61960787f, 0.80784315f),
            new Color(0.5294118f, 0.35686275f, 0.94509804f),
            new Color(0.19607843f, 0.32156864f, 0.6745098f),
        };
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject check;

        private const int RequiredPresetButtonCount = 14;

        private readonly List<Button> _presetButtons = new();
        private readonly List<UnityAction> _presetButtonHandlers = new();

        private PartsManager _partsManager;
        private ColorTargetType _currentTarget;
        private bool _hasCurrentTarget;

        private void OnEnable()
        {
            ResolveCheck();
            RefreshPresetButtons();
            SyncSelectionToCurrentTarget();
        }

        private void OnValidate()
        {
            ResolveCheck();
            RefreshPresetButtons();
        }

        /// <summary>
        /// Initializes the color picker with the given PartsManager.
        /// </summary>
        /// <param name="pm">The PartsManager used to apply color changes to character parts.</param>
        public void Init(PartsManager pm)
        {
            _partsManager = pm;
            ResolveCheck();
            RefreshPresetButtons();
            SyncSelectionToCurrentTarget();
        }

        /// <summary>
        /// Sets the current color target type and syncs the preset selection to the target's current color.
        /// </summary>
        /// <param name="target">The color target type (e.g., Skin, Hair, Eye).</param>
        public void SetTarget(ColorTargetType target)
        {
            _currentTarget = target;
            _hasCurrentTarget = true;
            SyncSelectionToCurrentTarget();
        }

        private void RefreshPresetButtons()
        {
            ClearPresetButtonListeners();
            ResolveContentParent();

            if (!IsValidPresetContentParent(contentParent))
                return;

            _presetButtons.AddRange(contentParent.GetComponentsInChildren<Button>(true));

            if (_presetButtons.Count < RequiredPresetButtonCount)
            {
                Debug.LogWarning(
                    $"ColorSelectScrollUIScript expected at least {RequiredPresetButtonCount} preset buttons but found {_presetButtons.Count}.",
                    this);
            }

            if (presetColors == null || presetColors.Length < RequiredPresetButtonCount)
            {
                Debug.LogWarning(
                    $"ColorSelectScrollUIScript expected at least {RequiredPresetButtonCount} preset colors but found {presetColors?.Length ?? 0}.",
                    this);
            }

            int bindCount = presetColors == null ? 0 : Mathf.Min(_presetButtons.Count, presetColors.Length);
            for (int i = 0; i < _presetButtons.Count; i++)
            {
                Button button = _presetButtons[i];
                bool hasPresetColor = i < bindCount;

                SetPresetButtonColor(button, hasPresetColor ? presetColors[i] : Color.clear);
                button.interactable = hasPresetColor;

                if (!Application.isPlaying || !hasPresetColor)
                    continue;

                Color presetColor = presetColors[i];
                UnityAction onClick = () => OnPresetButtonClicked(button, presetColor);
                button.onClick.AddListener(onClick);
                _presetButtonHandlers.Add(onClick);
            }
        }

        private void OnPresetButtonClicked(Button button, Color color)
        {
            if (_partsManager == null || !_hasCurrentTarget)
                return;

            _partsManager.SetColor(_currentTarget, color);
            SelectPresetButton(button);
        }

        private void SetPresetButtonColor(Button button, Color color)
        {
            Image swatch = button.GetComponent<Image>();
            if (swatch != null)
                swatch.color = color;
        }

        private Image FindPresetColorImage(Button button)
        {
            if (button == null)
                return null;

            Transform colorTransform = button.transform.Find("Color");
            if (colorTransform != null && colorTransform.TryGetComponent(out Image colorImage))
                return colorImage;

            foreach (Image image in button.GetComponentsInChildren<Image>(true))
            {
                if (image.gameObject != button.gameObject)
                    return image;
            }

            return button.targetGraphic as Image ?? button.GetComponent<Image>();
        }

        private void ResolveContentParent()
        {
            if (IsValidPresetContentParent(contentParent))
                return;

            contentParent = FindPresetContentParent(transform);
            if (!IsValidPresetContentParent(contentParent) && transform.parent != null)
                contentParent = FindPresetContentParent(transform.parent);
            if (!IsValidPresetContentParent(contentParent) && transform.root != null)
                contentParent = FindPresetContentParent(transform.root);
        }

        private Transform FindPresetContentParent(Transform root)
        {
            Transform bestCandidate = null;
            int bestScore = int.MinValue;

            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                Button[] buttons = candidate.GetComponentsInChildren<Button>(true);
                if (buttons.Length < RequiredPresetButtonCount)
                    continue;

                int swatchCount = 0;
                foreach (Button button in buttons)
                {
                    if (FindPresetColorImage(button) != null)
                        swatchCount++;
                }

                int score = (buttons.Length == RequiredPresetButtonCount ? 1000 : 0)
                    + (swatchCount * 10)
                    - Mathf.Abs(buttons.Length - RequiredPresetButtonCount);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            return bestCandidate;
        }

        private bool IsValidPresetContentParent(Transform candidate)
        {
            return candidate != null && candidate.GetComponentsInChildren<Button>(true).Length >= RequiredPresetButtonCount;
        }

        private void ResolveCheck()
        {
            if (check != null)
                return;

            Transform checkTransform = transform.Find("Check");
            if (checkTransform != null)
                check = checkTransform.gameObject;
        }

        private void SyncSelectionToCurrentTarget()
        {
            if (_partsManager == null || !_hasCurrentTarget || _presetButtons.Count == 0 || presetColors == null)
            {
                ClearPresetSelection();
                return;
            }

            int presetIndex = FindMatchingPresetIndex(_partsManager.GetColor(_currentTarget));
            if (presetIndex < 0 || presetIndex >= _presetButtons.Count)
            {
                ClearPresetSelection();
                return;
            }

            SelectPresetButton(_presetButtons[presetIndex]);
        }

        private int FindMatchingPresetIndex(Color color)
        {
            Color32 target = color;
            for (int i = 0; i < presetColors.Length; i++)
            {
                Color32 preset = presetColors[i];
                if (preset.r == target.r &&
                    preset.g == target.g &&
                    preset.b == target.b &&
                    preset.a == target.a)
                    return i;
            }

            return -1;
        }

        private void SelectPresetButton(Button button)
        {
            if (check == null || button == null)
                return;

            check.SetActive(true);
            check.transform.SetParent(button.transform, false);

            if (check.transform is RectTransform checkRect)
            {
                checkRect.anchoredPosition = Vector2.zero;
                checkRect.localRotation = Quaternion.identity;
                checkRect.localScale = Vector3.one;
            }
        }

        private void ClearPresetSelection()
        {
            if (check != null)
                check.SetActive(false);
        }

        private void ClearPresetButtonListeners()
        {
            int count = Mathf.Min(_presetButtons.Count, _presetButtonHandlers.Count);
            for (int i = 0; i < count; i++)
            {
                if (_presetButtons[i] != null)
                    _presetButtons[i].onClick.RemoveListener(_presetButtonHandlers[i]);
            }

            _presetButtons.Clear();
            _presetButtonHandlers.Clear();
        }

        private void OnDisable()
        {
            ClearPresetButtonListeners();
        }

        private void OnDestroy()
        {
            ClearPresetButtonListeners();
        }
    }
}
