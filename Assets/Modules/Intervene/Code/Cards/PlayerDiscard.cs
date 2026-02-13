using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class PlayerDiscard : CardStack
    {
        public void Shuffle()
        {
            CardStackUtility.ShuffleStack(this);
        }
    }
}