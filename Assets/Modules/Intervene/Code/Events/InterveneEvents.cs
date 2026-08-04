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
        public static StringHash32 PostEffectChunkComplete = "post-effect-chunk-complete";
        public static StringHash32 PostEffectChunkCancel = "post-effect-chunk-cancel";

        public static StringHash32 OnEffectSpecifyAllActionsProcessed = "on-effect-specify-all-actions-processed";

        public static StringHash32 OnInterveneStart = "on-intervene-start";
        public static StringHash32 OnInterveneRestart = "on-intervene-restart";
        public static StringHash32 OnActionDeckConstructed = "on-action-deck-constructed";
        public static StringHash32 OnInterveneEnd = "on-intervene-end";

        // Dispatched right after the round-end population snapshot is recorded; a cue for
        // trend UI to re-query InterveneRoundCounterInterfacer.Instance.Evaluator.
        public static StringHash32 OnPopulationSnapshotRecorded = "on-population-snapshot-recorded";

        public static StringHash32 OnInvasionLevelChanged = "on-invasion-level-changed";

        // Dispatched whenever the player's spendable budget actually changes; a cue for UI which
        // reacts to the player running out of budget.
        public static StringHash32 OnBudgetChanged = "on-budget-changed";

        // Events for SFX
        public static StringHash32 OnHoverZone = "on-hover-zone";
        public static StringHash32 OnUiSelected = "on-ui-selected";
        public static StringHash32 OnEndTurn = "on-end-turn";
    
        public static StringHash32 OnPathwayHighlighted = "on-pathway-highlighted";
        public static StringHash32 OnHunt = "on-hunt";
        public static StringHash32 OnStarve = "on-starve";
        public static StringHash32 OnReproduce = "on-reproduce";
        public static StringHash32 OnNestSpawn = "on-nest-spawn";
        public static StringHash32 OnTrapTriggered = "on-trap-triggered";
    }
}
