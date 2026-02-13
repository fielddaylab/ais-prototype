using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class CardInteractionMgr : MonoBehaviour
    {
        #region Inspector

        public PlayerHand Hand;
        public PlayerActionDeck Deck;
        public PlayerDiscard Discard;

        #endregion // Inspector

        #region Core Card Mechanics

        public void ShuffleDeck(PlayerActionDeck deck)
        {
            Deck.Shuffle();
        }

        public void DrawCard()
        {
            // TODO
        }

        public void DiscardCard()
        {
            // TODO
        }

        public void RecycleDiscard()
        {
            CardStackUtility.MergeStacks(Discard, Deck);
            Deck.Shuffle();
        }

        #endregion // Core Card Mechanics
    }
}
