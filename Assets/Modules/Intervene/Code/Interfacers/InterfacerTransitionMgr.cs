using AIS.Model;
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
        [HideInInspector] public InvasionModel InvasionModel;

        // Data passed into this scene
        public void Load()
        {
            // TODO: manage proper dependency sequence
            InvasionModel = InvasionModel.Instance;

            // TODO: convert to parameters
            int inBudget = 3;

            int inAwareness = 3;

            int[] inStats = new int[4];
            inStats[0] = 0;
            inStats[1] = 0;
            inStats[2] = 0;
            inStats[3] = 0;

            List<SerializedHash32> inEvidenceIds = new List<SerializedHash32>();

            int inInvasionCurve = 0;

            // Load data
            Budget.LoadPlayerBudget(inBudget);
            Awareness.LoadPlayerAwareness(inAwareness);
            Stats.LoadPlayerStats(inStats[0], inStats[1], inStats[2], inStats[3]);
            CardMgr.LoadSetupData(inEvidenceIds);
            InvasionModel.Load(inInvasionCurve);
        }

        // TODO: any data that needs to be passed to next scene
        public void Return()
        {

        }

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
        }

        private void HandleInterveneRestart()
        {
            Load();
        }
    }
}