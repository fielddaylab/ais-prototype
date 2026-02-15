using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class CardInteractionMgr : MonoBehaviour
    {
        #region Inspector

        public PlayerHand Hand;
        public PlayerActionDeck ActionDeck;
        public PlayerDiscard Discard;

        #endregion // Inspector

        #region Unity Callbacks

        private void Awake()
        {
            AisGame.Events.Register(InterveneEvents.OnShuffleActionDeck, HandleOnShuffleActionDeck);
            AisGame.Events.Register(InterveneEvents.OnDrawFromActionDeck, HandleOnDrawFromActionDeck);
            AisGame.Events.Register(InterveneEvents.OnUseSelectedCards, HandleOnUseSelectedCards);
            AisGame.Events.Register(InterveneEvents.OnRecycleDiscard, HandleOnRecycleDiscard);
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
    }
}
