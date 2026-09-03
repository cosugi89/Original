using System;
using System.Collections.Generic;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public readonly struct DialogDeckLoadoutRequest
    {
    }

    public readonly struct DialogDeckLoadoutResult
    {
        private DialogDeckLoadoutResult(bool isDeckSelected, int deckId)
        {
            IsDeckSelected = isDeckSelected;
            DeckId = deckId;
        }

        public bool IsDeckSelected { get; }
        public int DeckId { get; }

        public static DialogDeckLoadoutResult Selected(int deckId) => new(true, deckId);
        public static DialogDeckLoadoutResult Cancelled => new(false, 0);
    }

    [DialogAddress("Dialog/DeckLoadout")]
    public sealed class DialogDeckLoadout :
        DialogBase<DialogDeckLoadoutResult>,
        IDialogRequestHandler<DialogDeckLoadoutRequest>
    {
        [SerializeField] private Transform deckContent;
        [SerializeField] private DeckLoadoutItemView deckItemTemplate;
        [SerializeField] private GameButton closeButton;

        private readonly List<DeckLoadoutItemView> _deckItemViews = new();
        private DeckManager _deckManager;
        private bool _templateInitialized;

        public void Setup(DialogDeckLoadoutRequest request, DialogContext context)
        {
            _deckManager = UserDataManager.EnsureInitialized().Decks;
            InitializeTemplate();
            Refresh();

            closeButton.onClick.RemoveListener(OnClickClose);
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnClickClose()
        {
            Close(DialogDeckLoadoutResult.Cancelled);
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
                    _deckItemViews[i].Setup(deck, deck.DeckId == _deckManager.CurrentDeckId, OnClickDeck);
                }
            }
        }

        private void OnClickDeck(int deckId)
        {
            if (!_deckManager.TrySelectDeck(deckId))
            {
                return;
            }

            Close(DialogDeckLoadoutResult.Selected(deckId));
        }

        private void InitializeTemplate()
        {
            if (_templateInitialized)
            {
                return;
            }

            deckItemTemplate.gameObject.SetActive(false);
            _templateInitialized = true;
        }

        private void EnsureViewCount(int requiredCount)
        {
            while (_deckItemViews.Count < requiredCount)
            {
                _deckItemViews.Add(Instantiate(deckItemTemplate, deckContent));
            }
        }
    }
}
