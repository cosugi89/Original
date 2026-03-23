using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* 将来的には Prest の切り替えと保存を実装する予定です。
 * 現状は index[0] のみを使用して、プリセットの保存と読み込みを行っています。 */
namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Preset の切り替えと保存を行う UIコントローラー
    /// </summary>
    public class PanelPartsControl : MonoBehaviour
    {
        public static PanelPartsControl Instance;

        [SerializeField] private GameObject selectFrame;
        [SerializeField] private Sprite[] spriteBgs;

        #region Preset
        [Header("Equipment Preset")]
        [SerializeField] private Button buttonPresetPrev;
        [SerializeField] private Button buttonPresetNext;
        [SerializeField] private TMP_Text textPresetNumber;
        [SerializeField] private PresetData equipmentPresetData;

        private int _currentPresetIndex;
        private PresetData _runtimePresetData;

        internal int CurrentPresetIndex
        {
            get => _currentPresetIndex;
            set => _currentPresetIndex = value;
        }

        internal void UpdatePresetDisplay()
        {
            if (textPresetNumber == null) return;
            textPresetNumber.text = (_currentPresetIndex + 1).ToString("D2");
        }

        public void RefreshCurrentSlot()
        {
            if (_selectedSlot != null && _panelPartsList != null)
                _panelPartsList.Show(_selectedSlot.UICategory);
        }
        #endregion

        private PanelPartsListControl _panelPartsList;
        private PartsSlot[] _partsSlots;
        private PartsSlot _selectedSlot;
        private PartsSlot _focusedSlot;
        private PartsManager _partsManager;

        #region Internal Accessors (for EquipmentPresetEditorGUI)

        internal PartsManager CurrentPartsManager => _partsManager;

        internal PresetData EquipmentPresetData
        {
            get => _runtimePresetData;
            set => _runtimePresetData = value;
        }
        #endregion

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// PartsManager と連動する PanelPartsList を使って、PanelParts を初期化する
        /// 子要素の PartsSlot をセットアップし、最初のスロットを選択する
        /// </summary>
        public void Init(PartsManager pm, PanelPartsListControl panelPartsList)
        {
            _partsManager = pm;
            _panelPartsList = panelPartsList;

            _partsSlots = GetComponentsInChildren<PartsSlot>();
            foreach (var slot in _partsSlots)
                slot.Init(pm, spriteBgs);
            DoSelectSlot(_partsSlots[0]);

            LoadCurrentPreset();
            UpdatePresetDisplay();
            RefreshCurrentSlot();
        }

        public static void SelectSlot(PartsSlot slot) => Instance?.DoSelectSlot(slot);

        public static void UnfocusSlot(PartsSlot slot) => Instance?.DoUnfocusSlot(slot);

        public void RefreshSelectionFrame()
        {
            if (_selectedSlot == null || selectFrame == null)
                return;

            Canvas.ForceUpdateCanvases();
            RepositionSelectFrame(_selectedSlot);
        }

        private void DoSelectSlot(PartsSlot slot)
        {
            if (slot == null || slot == _selectedSlot) return;
            _selectedSlot = slot;

            RepositionSelectFrame(slot);

            _panelPartsList.Show(slot.UICategory);
        }

        private void DoUnfocusSlot(PartsSlot slot)
        {
            if (slot != _focusedSlot) return;
            _focusedSlot = null;
        }

        private void LoadCurrentPreset()
        {
            _runtimePresetData = AvatarPresetJsonStore.CreateRuntimePresetData(equipmentPresetData, out _currentPresetIndex);
        }

        public void SaveCurrentPreset()
        {
            SavePreset(_currentPresetIndex);
        }

        internal void SavePreset(int slot)
        {
            if (_partsManager == null) return;

            if (_runtimePresetData == null)
                LoadCurrentPreset();

            _currentPresetIndex = Mathf.Max(0, slot);
            var item = _partsManager.ToPresetItem();
            _runtimePresetData.SaveItem(_currentPresetIndex, item);
            AvatarPresetJsonStore.Save(_runtimePresetData, _currentPresetIndex);
            UpdatePresetDisplay();
        }

        private void RepositionSelectFrame(PartsSlot slot)
        {
            if (slot == null || selectFrame == null)
                return;

            selectFrame.transform.SetParent(slot.transform, false);
            selectFrame.transform.localPosition = Vector3.zero;
            selectFrame.transform.SetAsLastSibling();
            selectFrame.SetActive(true);
        }
    }
}
