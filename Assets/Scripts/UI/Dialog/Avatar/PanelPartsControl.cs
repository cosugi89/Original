using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Dialog.Avatar;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    /// <summary>
    /// Character parts panel controller.
    /// </summary>
    public class PanelPartsControl : MonoBehaviour
    {
        public static PanelPartsControl Instance;

        [SerializeField] private GameObject selectFrame;
        [SerializeField] private Sprite[] spriteBgs;

        private PanelPartsListControl _panelPartsList;
        private PartsSlot[] _partsSlots;
        private PartsSlot _selectedSlot;
        private PartsSlot _focusedSlot;
        private PartsManager _partsManager;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// PartsManager と連動する PanelPartsList を使って、PanelParts を初期化する
        /// 子要素の PartsSlot をセットアップし、最初のスロットを選択する
        /// </summary>
        public void Init(PartsManager pm, PanelPartsListControl panelPartsList, AvatarEditorPresenter presenter = null)
        {
            _partsManager = pm;
            _panelPartsList = panelPartsList;

            _partsSlots = GetComponentsInChildren<PartsSlot>();
            foreach (var slot in _partsSlots)
                slot.Init(pm, spriteBgs, presenter);
            DoSelectSlot(_partsSlots[0]);

            RefreshCurrentSlot();
        }

        public static void SelectSlot(PartsSlot slot) => Instance?.DoSelectSlot(slot);

        public static void UnfocusSlot(PartsSlot slot) => Instance?.DoUnfocusSlot(slot);

        public void RefreshCurrentSlot()
        {
            if (_selectedSlot != null && _panelPartsList != null)
                _panelPartsList.Show(_selectedSlot.UICategory);
        }

        public void RefreshSelectionFrame()
        {
            if (_selectedSlot == null || selectFrame == null)
                return;

            Canvas.ForceUpdateCanvases();
            RepositionSelectFrame(_selectedSlot);
        }

        public void SaveCurrentAppearance()
        {
            if (_partsManager == null)
                return;

            AvatarRenderService.EnsureInitialized().CaptureFrom(_partsManager);
            GameSaveService.EnsureInitialized().SaveSession();
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
