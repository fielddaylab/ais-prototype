using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public static class InterveneEvents
    {
        public static StringHash32 OnShuffleActionDeck = "on-shuffle-action-deck";
        public static StringHash32 OnDrawFromActionDeck = "on-draw-from-action-deck";
        public static StringHash32 OnUseSelectedCards = "on-discard-selected-cards";
        public static StringHash32 OnRecycleDiscard = "on-recycle-discard";
    }
}
