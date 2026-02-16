using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class CardInteractionMgr : MonoBehaviour
    {
        public static CardInteractionMgr Instance;

        #region Inspector

        public PlayerHand Hand;
        public PlayerActionDeck ActionDeck;
        public PlayerDiscard Discard;

        [Header("Visuals DB")]
        public GameObject UICardPrefab;
        public GameObject UICardHoverablePrefab;
        public Sprite CardBackActionSprite;

        #endregion // Inspector

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;

            AisGame.Events.Register(InterveneEvents.OnShuffleActionDeck, HandleOnShuffleActionDeck);
            AisGame.Events.Register(InterveneEvents.OnDrawFromActionDeck, HandleOnDrawFromActionDeck);
            AisGame.Events.Register(InterveneEvents.OnUseSelectedCards, HandleOnUseSelectedCards);
            AisGame.Events.Register(InterveneEvents.OnRecycleDiscard, HandleOnRecycleDiscard);
        }

        private void Start()
        {
            SetupData();
            SetupVisuals();
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnShuffleActionDeck, HandleOnShuffleActionDeck);
            AisGame.Events.Deregister(InterveneEvents.OnDrawFromActionDeck, HandleOnDrawFromActionDeck);
            AisGame.Events.Deregister(InterveneEvents.OnUseSelectedCards, HandleOnUseSelectedCards);
            AisGame.Events.Deregister(InterveneEvents.OnRecycleDiscard, HandleOnRecycleDiscard);
        }

        #endregion // Unity Callbacks

        #region Core Card Mechanics

        public void ShuffleActionDeck()
        {
            ActionDeck.Shuffle();
        }

        public void DrawCard()
        {
            CardBase drawnCard;
            if (CardStackUtility.TryDrawFromTop(ActionDeck, out drawnCard))
            {
                CardStackUtility.AddToTop(Hand, drawnCard);
            }
            else
            {
                RecycleDiscard();

                if (CardStackUtility.TryDrawFromTop(ActionDeck, out drawnCard))
                {
                    CardStackUtility.AddToTop(Hand, drawnCard);
                }
            }
        }

        public void DiscardSelectedCards()
        {
            List<CardBase> selectedCards = Hand.GetSelectedCards();
            if (selectedCards.Count == 0) { return; }

            CardStackUtility.RemoveCards(Hand, Hand.SelectedCardIndices);
            Hand.SelectedCardIndices.Clear();

            foreach(var discarded in selectedCards)
            {
                CardStackUtility.AddToTop(Discard, discarded);
            }
        }

        public void RecycleDiscard()
        {
            CardStackUtility.MergeStacks(Discard, ActionDeck);
            ActionDeck.Shuffle();
        }

        #endregion // Core Card Mechanics

        #region Setup

        private void SetupData()
        {
            List<StringHash32> evidenceIds = new List<StringHash32>();
            // TODO: get curr evidence cards

            // TEMP DEBUG
            evidenceIds = new List<StringHash32>() {
                "example-evidence-card-1",
                "example-evidence-card-2",
            };

            var actionCardsState = Find.State<ActionCardsState>();
            var actionCards = ActionCardsUtility.GetCardsFromEvidence(actionCardsState, evidenceIds);
            ActionDeck.PopulateDeck(actionCards);

            Hand.ClickCall = (index) => { ToggleSelectHandAtIndex(index); };
        }

        private void SetupVisuals()
        {
            CardStackVisualsUtility.RefreshVisuals(Hand.Visuals, Hand);
            CardStackVisualsUtility.RefreshVisuals(ActionDeck.Visuals, ActionDeck);
            CardStackVisualsUtility.RefreshVisuals(Discard.Visuals, Discard);
        }

        #endregion // Setup

        #region Handlers

        private void HandleOnShuffleActionDeck()
        {
            ShuffleActionDeck();
        }

        private void HandleOnDrawFromActionDeck()
        {
            DrawCard();
        }

        private void HandleOnUseSelectedCards()
        {
            DiscardSelectedCards();
        }

        private void HandleOnRecycleDiscard()
        {
            RecycleDiscard();
        }

        #endregion // Handlers

        #region Stack Calls

        private void ToggleSelectHandAtIndex(int index)
        {
            Hand.ToggleSelectAtIndex(index);
        }

        #endregion // Stack Calls
    }
}
