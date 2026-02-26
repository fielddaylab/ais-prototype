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

        public static StringHash32 OnEffectSpecifyBegin = "on-effect-specify-begin";
        public static StringHash32 OnEffectSpecifyCancel = "on-effect-specify-cancel";
        public static StringHash32 OnEffectSpecifyConfirm = "on-effect-specify-confirm";

        public static StringHash32 OnEffectChunkBegin = "on-effect-chunk-begin";
        public static StringHash32 OnEffectChunkComplete = "on-effect-chunk-complete";
        public static StringHash32 OnEffectChunkCancel = "on-effect-chunk-cancel";

        public static StringHash32 OnEffectSpecifyAllActionsProcessed = "on-effect-specify-all-actions-processed";

        public static StringHash32 OnInterveneRestart = "on-intervene-restart";
        public static StringHash32 OnInterveneEnd = "on-intervene-end";

        public static StringHash32 OnInvasionLevelChanged = "on-invasion-level-changed";

        public static StringHash32 OnHoverZone = "on-hover-zone";
        public static StringHash32 OnUiSelected = "on-ui-selected";
        public static StringHash32 OnEndTurn = "on-end-turn";
        public static StringHash32 OnPredatorEatPrey = "on-predator-eat-prey";
    }
}
