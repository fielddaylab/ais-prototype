using BeauUtil;
using FieldDay.SharedState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class InterfacerTransitionMgr : SharedStateComponent
    {
        public InterveneBudgetInterfacer Budget;
        public InterveneAwarenessInterfacer Awareness;
        public StatsInterfacer Stats;
        public CardInteractionMgr CardMgr;

        // Data passed into this scene
        public void Load()
        {
            // TODO: convert to parameters
            int inBudget = 0;

            int inAwareness = 0;

            int[] inStats = new int[4];
            inStats[0] = 0;
            inStats[1] = 0;
            inStats[2] = 0;
            inStats[3] = 0;

            List<SerializedHash32> inEvidenceIds = new List<SerializedHash32>();

            // Load data
            Budget.LoadPlayerBudget(inBudget);
            Awareness.LoadPlayerAwareness(inAwareness);
            Stats.LoadPlayerStats(inStats[0], inStats[1], inStats[2], inStats[3]);
            CardMgr.LoadSetupData(inEvidenceIds);
        }

        // TODO: any data that needs to be passed to next scene
        public void Return()
        {

        }
    }
}