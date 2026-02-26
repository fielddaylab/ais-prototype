using AIS.Model;
using AIS.Narrative;
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

            int inBudget = 3; // TODO

            int inAwareness = 3; // TODO

            var stats = Find.State<PlayerStats>().StatBlock;
            int[] inStats = new int[4];
            inStats[0] = stats.Communicate;
            inStats[1] = stats.Ranger;
            inStats[2] = stats.Tech;
            inStats[3] = stats.Research;

            var inventory = Find.State<PlayerInventory>();
            StringHash32[] evidenceIds = new StringHash32[inventory.EvidenceCards.Count];
            inventory.EvidenceCards.CopyTo(evidenceIds);
            List<SerializedHash32> inEvidenceIds = new List<SerializedHash32>();
            foreach (var evidenceId in evidenceIds) {
                inEvidenceIds.Add(evidenceId);
            }

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