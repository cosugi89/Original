using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// PanelParts の ScrollView を管理する
    /// </summary>
    public class PanelPartsListControl : MonoBehaviour
    {
        [SerializeField] private PartsListSlot slotTemplate;
        [SerializeField] private Transform contentParent;
        [SerializeField] private ColorSelectScrollUIScript colorPicker;
        [SerializeField] private Image imgSelectFrame;
        [SerializeField] private Button buttonReset;

        private readonly List<PartsListSlot> _slots = new();
        private PartsType _activeType;
        private UICategory _activeCategory;
        private PartsType[] _currentSubTypes;
        private PartsManager _partsManager;

        /// <summary>選択・表示中の PartsType</summary>
        public PartsType? ActiveType => _activeType;

        /// <summary>
        /// 指定された <see cref="PartsManager"/> を使ってパーツ一覧パネルを初期化します。
        /// パーツ変更イベントと色変更イベントを購読します。
        /// </summary>
        /// <param name="pm">パーツデータを提供する PartsManager。</param>
        public void Init(PartsManager pm)
        {
            _partsManager = pm;
            if (slotTemplate != null)
                slotTemplate.gameObject.SetActive(false);
            if (buttonReset != null)
                buttonReset.onClick.AddListener(OnClickReset);

            if (_partsManager != null)
            {
                _partsManager.OnPartsChanged += OnPartsChanged;
                _partsManager.OnColorChanged += OnColorChanged;
            }
        }

        /// <summary>
        /// 指定された UI カテゴリのパーツ一覧を表示します。
        /// カテゴリに応じて、グループ表示または単一タイプ表示の処理へ振り分けます。
        /// </summary>
        /// <param name="category">表示する UI カテゴリ。</param>
        public void Show(UICategory category)
        {
            _activeCategory = category;
            _currentSubTypes = UICategoryConfig.GetSubTypes(category);

            if (UICategoryConfig.IsGroup(category))
            {
                ShowGroup(category);
                return;
            }

            if (_currentSubTypes.Length == 1)
                ShowPartsType(_currentSubTypes[0]);
        }

        public void RefreshSelectionFrame()
        {
            Canvas.ForceUpdateCanvases();

            if (contentParent is RectTransform contentRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            UpdateSelectFrame();
        }

        private void ShowGroup(UICategory category)
        {
            // 再構築前に既存スロットをすべて非表示にする
            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);

            // グループ内のすべてのサブタイプ用にスロットを生成または再利用する
            int slotIdx = 0;
            foreach (var type in _currentSubTypes)
            {
                int count = _partsManager.GetPartsCount(type);
                for (int i = 0; i < count; i++)
                {
                    while (_slots.Count <= slotIdx)
                    {
                        var newSlot = Instantiate(slotTemplate, contentParent);
                        _slots.Add(newSlot);
                    }

                    _slots[slotIdx].SetSlot(this, _partsManager.GetThumbnail(type, i), i, type);
                    _slots[slotIdx].gameObject.SetActive(true);
                    slotIdx++;
                }
            }

            // グループではスロットごとの色変更に対応しないため白に戻す
            ChangeColorList(Color.white);
            if (colorPicker != null)
                colorPicker.gameObject.SetActive(false);

            // リセットボタンを一覧の先頭に表示する
            if (buttonReset != null)
            {
                buttonReset.gameObject.SetActive(true);
                buttonReset.transform.SetAsFirstSibling();
            }

            UpdateSelectFrame();
        }

        private void ShowPartsType(PartsType type)
        {
            _activeType = type;

            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);

            int count = _partsManager.GetPartsCount(type);

            while (_slots.Count < count)
            {
                var newSlot = Instantiate(slotTemplate, contentParent);
                _slots.Add(newSlot);
            }

            for (int i = 0; i < count; i++)
            {
                _slots[i].SetSlot(this, _partsManager.GetThumbnail(type, i), i, type);
                _slots[i].gameObject.SetActive(true);
            }

            if (_partsManager.CanChangeColor(type))
            {
                var target = _partsManager.GetColorTarget(type);
                Color color = _partsManager.GetColor(target);
                ChangeColorList(color);
                if (colorPicker != null)
                {
                    colorPicker.gameObject.SetActive(true);
                    colorPicker.SetTarget(target);
                }
            }
            else
            {
                ChangeColorList(Color.white);
                if (colorPicker != null)
                    colorPicker.gameObject.SetActive(false);
            }

            if (buttonReset != null)
            {
                // MEMO: _partsManager.CanToggle(type) でSetActiveを切り替えていた
                // TODO: OnClickReset で UnequipParts するのではなく、初期表示か変更前は表示させるようにする
                buttonReset.gameObject.SetActive(true);
                buttonReset.transform.SetAsFirstSibling();
            }

            UpdateSelectFrame();
        }

        /// <summary>
        /// パーツ一覧スロットの選択を処理します。グループでは選択されたサブタイプのみ表示し、
        /// その他は非表示にします。単一タイプでは装備前に表示状態を整えます。
        /// </summary>
        /// <param name="slot">クリックされたスロット。</param>
        public void SelectSlot(PartsListSlot slot)
        {
            PartsType slotType = slot.PartsType;

            if (UICategoryConfig.IsGroup(_activeCategory))
            {
                // グループ: 選択したタイプのみ表示し、他のタイプは非表示にする
                _partsManager.SetGroupActiveType(_activeCategory, slotType);
                _activeType = slotType;
            }
            else
            {
                // CanToggle を削除するため一旦コメントアウト
                //if (!_partsManager.IsPartsVisible(_activeType) && _partsManager.CanToggle(_activeType))
                //{
                //    // Manager API から表示切り替えが削除されたため、現在のインデックスを再適用して表示を復元する
                //    int idx = _partsManager.GetActiveIndex(_activeType);
                //    _partsManager.EquipParts(_activeType, idx);
                //}
            }

            _partsManager.EquipParts(slotType, slot.SlotIndex);
            UpdateSelectFrame();
        }

        /// <summary>
        /// 現在アクティブなパーツ種別、またはグループ内のすべてのサブタイプをリセット
        /// （装備解除）します。
        /// </summary>
        public void OnClickReset()
        {
            if (UICategoryConfig.IsGroup(_activeCategory))
            {
                foreach (var type in _currentSubTypes)
                    _partsManager.UnequipParts(type);
            }
            else
            {
                _partsManager.UnequipParts(_activeType);
            }
            UpdateSelectFrame();
        }

        private void HideAllSlots()
        {
            foreach (var slot in _slots)
                slot.gameObject.SetActive(false);
        }

        private void UpdateSelectFrame()
        {
            if (imgSelectFrame == null) return;

            if (UICategoryConfig.IsGroup(_activeCategory))
            {
                // グループ内で装備中のサブタイプに対応するスロットを探す
                // 表示がオフでも装備されているアイテムの選択フレームは維持
                foreach (var type in _currentSubTypes)
                {
                    if (!_partsManager.IsEquipped(type)) continue;
                    // 表示されているタイプを優先し、なければ _activeType を使用
                    if (!_partsManager.IsPartsVisible(type) && type != _activeType)
                        continue;

                    int idx = _partsManager.GetActiveIndex(type);
                    foreach (var slot in _slots)
                    {
                        if (slot.gameObject.activeSelf && slot.PartsType == type && slot.SlotIndex == idx)
                        {
                            imgSelectFrame.gameObject.SetActive(true);
                            MoveFrameTo(slot.transform as RectTransform);
                            return;
                        }
                    }
                }
                imgSelectFrame.gameObject.SetActive(false);
            }
            else
            {
                // 単一タイプ: アクティブなインデックスに一致するスロットを探す
                int activeIndex = _partsManager.GetActiveIndex(_activeType);

                if (!_partsManager.IsEquipped(_activeType))
                {
                    imgSelectFrame.gameObject.SetActive(false);
                    return;
                }

                if (activeIndex >= 0 && activeIndex < _slots.Count && _slots[activeIndex].gameObject.activeSelf)
                {
                    imgSelectFrame.gameObject.SetActive(true);
                    MoveFrameTo(_slots[activeIndex].transform as RectTransform);
                }
                else
                {
                    imgSelectFrame.gameObject.SetActive(false);
                }
            }
        }

        private void MoveFrameTo(RectTransform target)
        {
            if (target == null) return;
            imgSelectFrame.transform.SetParent(target, false);
            imgSelectFrame.transform.localPosition = Vector3.zero;
            imgSelectFrame.transform.SetAsFirstSibling();
        }

        private void ChangeColorList(Color color)
        {
            foreach (var slot in _slots)
            {
                if (slot.gameObject.activeSelf)
                    slot.ChangeColor(color);
            }
        }

        private void OnPartsChanged(PartsType type, int index)
        {
            if (UICategoryConfig.IsGroup(_activeCategory))
            {
                foreach (var st in _currentSubTypes)
                {
                    if (st == type) { UpdateSelectFrame(); return; }
                }
            }
            else if (type == _activeType)
            {
                UpdateSelectFrame();
            }
        }

        private void OnColorChanged(ColorTargetType target, Color color)
        {
            if (_partsManager.CanChangeColor(_activeType) &&
                _partsManager.GetColorTarget(_activeType) == target)
            {
                ChangeColorList(color);
            }
        }

        private void OnDestroy()
        {
            if (buttonReset != null)
                buttonReset.onClick.RemoveListener(OnClickReset);

            if (_partsManager != null)
            {
                _partsManager.OnPartsChanged -= OnPartsChanged;
                _partsManager.OnColorChanged -= OnColorChanged;
            }
        }
    }
}

