using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class PlayerActionDeck : CardStack
    {
        public void PopulateDeck()
        {
            // TODO
        }

        public void Shuffle()
        {
            CardStackUtility.ShuffleStack(this);
        }
    }
}