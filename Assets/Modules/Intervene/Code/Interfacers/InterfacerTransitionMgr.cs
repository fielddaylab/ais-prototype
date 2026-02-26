using AIS.Model;
using BeauUtil;
using FieldDay;
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
        public InvasionCurveInterfacer CurveInterfacer;
        [HideInInspector] public InvasionModel InvasionModel;

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);

            Game.Scenes.QueueOnEnable(this, Load);
        }

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

            float inInvasionCurve = 0;

            // Load data
            Budget.LoadPlayerBudget(inBudget);
            Awareness.LoadPlayerAwareness(inAwareness);
            Stats.LoadPlayerStats(inStats[0], inStats[1], inStats[2], inStats[3]);
            CurveInterfacer.LoadCurve(inInvasionCurve);
            InvasionModel.Load(CurveInterfacer.CurrVal);
            CardMgr.LoadSetupData(inEvidenceIds);
        }

        // TODO: any data that needs to be passed to next scene
        public void Return()
        {

        }

        private void HandleInterveneRestart()
        {
            Load();
        }
    }
}