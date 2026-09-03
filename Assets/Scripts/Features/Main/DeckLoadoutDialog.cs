using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct DeckLoadoutDialogRequest
    {
    }

    public readonly struct DeckLoadoutDialogResult
    {
        private DeckLoadoutDialogResult(bool isDeckSelected, int deckId)
        {
            IsDeckSelected = isDeckSelected;
            DeckId = deckId;
        }

        public bool IsDeckSelected { get; }
        public int DeckId { get; }

        public static DeckLoadoutDialogResult Selected(int deckId) => new(true, deckId);
        public static DeckLoadoutDialogResult Cancelled => new(false, 0);
    }

    [DialogAddress("Dialog/DeckLoadout")]
    public sealed class DeckLoadoutDialog :
        DialogBase<DeckLoadoutDialogResult>,
        IDialogRequestHandler<DeckLoadoutDialogRequest>
    {
        [SerializeField] private Transform deckContent;
        [SerializeField] private DeckLoadoutItemView deckItemTemplate;
        [SerializeField] private GameButton closeButton;

        private readonly List<DeckLoadoutItemView> _deckItemViews = new();
        private readonly List<PreviewHandle> _avatarPreviewHandles = new();
        private readonly List<GameObject> _avatarPreviewCameras = new();
        private readonly List<RenderTexture> _avatarPreviewTextures = new();
        private DeckManager _deckManager;
        private AppearanceManager _appearanceManager;
        private PreviewStage _avatarPreviewStage;
        private bool _templateInitialized;
        private bool _hasDeckSelection;
        private Vector2 _deckItemSize;

        public void Setup(DeckLoadoutDialogRequest request, DialogContext context)
        {
            _deckManager = UserDataManager.EnsureInitialized().Decks;
            _appearanceManager = UserDataManager.EnsureInitialized().Appearance;
            _hasDeckSelection = false;
            InitializeTemplate();
            Refresh();
            CreateLiveAvatarPreviews(context);

            closeButton.onClick.RemoveListener(OnClickClose);
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnClickClose()
        {
            Close(_hasDeckSelection
                ? DeckLoadoutDialogResult.Selected(_deckManager.CurrentDeckId)
                : DeckLoadoutDialogResult.Cancelled);
        }

        private void Refresh()
        {
            var decks = _deckManager.GetAllDecks();
            EnsureViewCount(decks.Count);
            for (var i = 0; i < _deckItemViews.Count; i++)
            {
                var isVisible = i < decks.Count;
                _deckItemViews[i].gameObject.SetActive(isVisible);
                if (isVisible)
                {
                    var deck = decks[i];
                    _deckItemViews[i].Setup(
                        deck,
                        _deckManager.GetCandidateSkills(deck),
                        deck.DeckId == _deckManager.CurrentDeckId,
                        OnClickDeck);
                }
            }
        }

        private void OnClickDeck(int deckId)
        {
            if (!_deckManager.TrySelectDeck(deckId))
            {
                return;
            }

            _hasDeckSelection = true;
            Refresh();
        }

        private void InitializeTemplate()
        {
            if (_templateInitialized)
            {
                return;
            }

            deckItemTemplate.gameObject.SetActive(false);
            for (var i = 0; i < deckContent.childCount; i++)
            {
                deckContent.GetChild(i).gameObject.SetActive(false);
            }

            _deckItemSize = deckItemTemplate.transform.parent
                .GetComponent<GridLayoutGroup>()
                .cellSize;
            _templateInitialized = true;
        }

        private void EnsureViewCount(int requiredCount)
        {
            while (_deckItemViews.Count < requiredCount)
            {
                var view = Instantiate(deckItemTemplate, deckContent);
                view.GetComponent<RectTransform>().sizeDelta = _deckItemSize;
                _deckItemViews.Add(view);
            }
        }

        private void CreateLiveAvatarPreviews(DialogContext context)
        {
            ClearLiveAvatarPreviews();

            var previewStage = context.AvatarPreviewStage;
            var avatarTemplate = context.AvatarPreviewTemplate;
            if (previewStage == null || avatarTemplate == null)
            {
                return;
            }

            var sourceCamera = previewStage.GetComponentInChildren<Camera>(true);
            _avatarPreviewStage = previewStage;
            var decks = _deckManager.GetAllDecks();
            for (var i = 0; i < decks.Count && i < _deckItemViews.Count; i++)
            {
                var offset = new Vector3((i + 1) * 10f, 0f, 0f);
                var handle = previewStage.Spawn(
                    avatarTemplate,
                    new PreviewPlacement(
                        avatarTemplate.transform.localPosition + offset,
                        avatarTemplate.transform.localRotation,
                        avatarTemplate.transform.localScale));
                var avatar = handle.Get<AppearanceController>();
                avatar.Init();
                PartsManagerAvatarAdapter.ApplyDeckAppearance(
                    avatar,
                    _appearanceManager.GetAppearanceForDeck(decks[i]),
                    _deckManager.InventoryManager);

                var previewCamera = Instantiate(sourceCamera.gameObject, previewStage.transform);
                previewCamera.name = $"DeckPreviewCamera-{decks[i].DeckId}";
                previewCamera.transform.localPosition = sourceCamera.transform.localPosition + offset;
                previewCamera.transform.localRotation = sourceCamera.transform.localRotation;
                previewCamera.transform.localScale = sourceCamera.transform.localScale;

                var camera = previewCamera.GetComponent<Camera>();
                var texture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
                {
                    name = $"DeckPreview-{decks[i].DeckId}",
                };
                texture.Create();
                camera.targetTexture = texture;

                _avatarPreviewHandles.Add(handle);
                _avatarPreviewCameras.Add(previewCamera);
                _avatarPreviewTextures.Add(texture);
                _deckItemViews[i].SetAvatarPreview(texture);
            }
        }

        private void ClearLiveAvatarPreviews()
        {
            for (var i = 0; i < _avatarPreviewHandles.Count; i++)
            {
                _avatarPreviewStage?.Despawn(_avatarPreviewHandles[i]);
            }
            _avatarPreviewHandles.Clear();
            _avatarPreviewStage = null;

            for (var i = 0; i < _avatarPreviewCameras.Count; i++)
            {
                Destroy(_avatarPreviewCameras[i]);
            }
            _avatarPreviewCameras.Clear();

            for (var i = 0; i < _avatarPreviewTextures.Count; i++)
            {
                _avatarPreviewTextures[i].Release();
                Destroy(_avatarPreviewTextures[i]);
            }
            _avatarPreviewTextures.Clear();
        }

        protected override void OnDestroy()
        {
            ClearLiveAvatarPreviews();
            base.OnDestroy();
        }
    }
}
