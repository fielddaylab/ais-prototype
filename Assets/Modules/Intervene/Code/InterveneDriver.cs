using AIS.Model;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene {
    public class InterveneDriver : MonoBehaviour
    {
        #region Structs and Enums

        public struct SpeciesTransferAllocation
        {
            public SerializedHash32 SpeciesId;
            public SerializedHash32 DestEcosystemId;
            public int TransferCount;
            public PathwayType TravelType;
            public ActionTarget TargetType;
        }

        #endregion // Structs and Enums

        private const int NEST_THRESHOLD = 3;
        private const float SIM_PHASE_DELAY = 1.2f;
        private const float SIM_PHASE_SHORT_DELAY = 0.8f;

        private List<SpeciesTransferAllocation> m_SpeciesTransfers = new List<SpeciesTransferAllocation>();

        public Routine SimRoutine;

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
        }

        public void TickSim()
        {
            if (InvasionModelContainer.Instance == null) { return; }

            SimRoutine.Replace(TickSimRoutine());
        }

        private IEnumerator TickSimRoutine()
        {
            InterveneUI.Instance.ShowSimPhase();

            // predator / prey dynamics
            /*
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                if (ecosystem.IsExternal) { continue; }
                InterveneUI.Instance.SetSimPhase("Species Dynamics");
                yield return ProcessInterspeciesDynamics(ecosystem);
            }
            */
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                if (ecosystem.IsExternal) { continue; }
                yield return ProcessInvasiveDynamics(ecosystem);
            }
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                if (ecosystem.IsExternal) { continue; }
                yield return ProcessPredatorDynamics(ecosystem);
            }
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                if (ecosystem.IsExternal) { continue; }
                yield return ProcessPreyDynamics(ecosystem);
            }

            // Trigger Traps
            InterveneUI.Instance.SetSimPhase("Trigger Traps");
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                yield return TriggerTraps(ecosystem);
            }

            // Transfer species along pathways
            InterveneUI.Instance.SetSimPhase("Stage Pathway Travels");
            foreach (var pathway in InvasionModelContainer.Instance.GetAllPathways())
            {
                SetPathwayFocused(pathway, true);
                yield return StagePathwayTransfer(pathway);
                SetPathwayFocused(pathway, false);
            }
            yield return SIM_PHASE_DELAY;

            // Finalize species movements
            InterveneUI.Instance.SetSimPhase("Finalize Pathway Travels");
            FinalizePathwayTransfers();

            yield return SIM_PHASE_DELAY;

            // Spawn at Nests
            InterveneUI.Instance.SetSimPhase("Trigger Nests");
            foreach (var ecosystem in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                yield return TriggerNests(ecosystem);
            }

            InterveneUI.Instance.SetSimPhase("COMPLETE!");
            yield return SIM_PHASE_DELAY * 1.5;

            InterveneUI.Instance.HideSimPhase();

            Debug.Log("[InterveneDriver] Sim Progressed by 1 tick");
            AisGame.Events.Dispatch(InterveneEvents.OnEndTurn);

        }

        #region Simulate & Modify

        private IEnumerator ProcessInterspeciesDynamics(Ecosystem eco)
        {
            yield return ProcessInvasiveDynamics(eco);
            yield return SIM_PHASE_DELAY;
            yield return ProcessPredatorDynamics(eco);
            yield return SIM_PHASE_DELAY;
            yield return ProcessPreyDynamics(eco);
            yield return SIM_PHASE_DELAY;
        }

        private IEnumerator ProcessInvasiveDynamics(Ecosystem eco)
        {
            /*
            Hunt: Roll d6 equal to invasive population. For each result lower than the prey population, remove 1 prey and add it to �bank.�
            Starve: If 0 banked prey, roll d12. If the result is less than or equal to invasive population, decrease population by 1.
            Reproduce: If the invasive population is 1, add 1 invasive population for each banked prey. Otherwise, add 1 invasive population for every 2 banked prey.
            */
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> preyCounts;
            eco.FindByTargetType(ActionTarget.Prey, out preyCounts);
            int totalPrey = 0;
            foreach (var count in preyCounts) {
                totalPrey += count.Item2;
            }

            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> invasiveCounts;
            eco.FindByTargetType(ActionTarget.Invasive, out invasiveCounts);
            int totalInvasives = 0;
            foreach (var count in invasiveCounts)
            {
                totalInvasives += count.Item2;
            }

            if (totalInvasives == 0)
            {
                yield break;
            }

            SetEcosystemFocused(eco, true);

            // Hunt
            InterveneUI.Instance.SetSimPhase("Invasive Turn: Hunt");
            int totalPreyConsumed = 0;
            for (int i = 0; i < totalInvasives; i++)
            {
                int rollResult = UnityEngine.Random.Range(1, 7);
                if (rollResult <= totalPrey)
                {
                    totalPreyConsumed++;
                }
            }
            totalPreyConsumed = Mathf.Min(totalPreyConsumed, totalPrey);
            if (totalPreyConsumed > 0) 
            {
                // eco.ReleasePopulation(preyCounts[0].Item1, totalPreyConsumed);
                List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> eachPreyConsumed = new List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>>();
                for (int i = 0; i < totalPreyConsumed; i++)
                {
                    for (int j = 0; j < preyCounts.Count; j++)
                    {
                        if (preyCounts[j].Item2 > 0)
                        {
                            if (UnityEngine.Random.Range(0, 1f) < preyCounts[j].Item2 / (float)totalPrey)
                            {
                                eachPreyConsumed.Add(new Tuple<SerializedHash32, int, PathwayType, ActionTarget>(preyCounts[j].Item1, 1, preyCounts[j].Item3, preyCounts[j].Item4));
                                break;
                            }
                        }
                    }
                }

                foreach (var prey in eachPreyConsumed)
                {
                    eco.ReleasePopulation(prey.Item1, prey.Item2);
                }

                Game.Events.Dispatch(InterveneEvents.OnHunt);
            }

            Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " invasives consumed " + totalPreyConsumed);
            yield return SIM_PHASE_SHORT_DELAY;

            InterveneUI.Instance.SetSimPhase("Invasive Turn: Starve");
            // Starve
            if (totalPreyConsumed == 0)
            {
                int rollResult = UnityEngine.Random.Range(1, 13);
                if (rollResult <= totalInvasives)
                {
                    eco.ReleasePopulation(invasiveCounts[0].Item1, 1);
                    Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " invasives starved 1");
                    AisGame.Events.Dispatch(InterveneEvents.OnStarve);
                }
            }
            yield return SIM_PHASE_SHORT_DELAY;

            InterveneUI.Instance.SetSimPhase("Invasive Turn: Reproduce");
            // Reproduce
            int reproduceNum = totalPreyConsumed;
            if (totalInvasives > 1)
            // check reproduce condition; For Sea Lamprey: “ecosystem must be Tributary”
            // check if numEgg >= 1
            {
                reproduceNum = Mathf.FloorToInt(totalPreyConsumed / 2); // population + 1, numEgg - 1
            }

            for (int i = 0; i < reproduceNum; i++)
            {
                eco.AddPopulation(invasiveCounts[0].Item1, 1, invasiveCounts[0].Item3, invasiveCounts[0].Item4);
                AisGame.Events.Dispatch(InterveneEvents.OnReproduce);
            }
            Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " invasives reproduced " + reproduceNum);
            yield return SIM_PHASE_SHORT_DELAY;

            InterveneUI.Instance.SetSimPhase("Invasive Turn: Spawn Nests");
            // Spawn Nest
            if (totalInvasives >= NEST_THRESHOLD)
            {
                TrySpawnNest(eco);
                AisGame.Events.Dispatch(InterveneEvents.OnNestSpawn);
            }

            yield return SIM_PHASE_DELAY;

            SetEcosystemFocused(eco, false);
        }

        private IEnumerator ProcessPredatorDynamics(Ecosystem eco)
        {
            /*
            Hunt: Roll d6 equal to predator population. For each result lower than the prey population, remove 1 prey and add it to �bank.� 
            Starve: If 0 banked prey, roll d12. If the result is less than or equal to predator population, decrease population by 1.
            Reproduce: If the predator population is 1, add 1 predator population for each banked prey. Otherwise, add 1 predator population for every 2 banked prey.
            */
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> preyCounts;
            eco.FindByTargetType(ActionTarget.Prey, out preyCounts);
            int totalPrey = 0;
            foreach (var count in preyCounts)
            {
                totalPrey += count.Item2;
            }

            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> predatorCounts;
            eco.FindByTargetType(ActionTarget.Predator, out predatorCounts);
            int totalPredators = 0;
            foreach (var count in predatorCounts)
            {
                totalPredators += count.Item2;
            }

            if (totalPredators == 0)
            {
                yield break;
            }

            SetEcosystemFocused(eco, true);

            InterveneUI.Instance.SetSimPhase("Predator Turn: Hunt");
            // Hunt
            int totalPreyConsumed = 0;
            for (int i = 0; i < totalPredators; i++)
            {
                int rollResult = UnityEngine.Random.Range(1, 7);
                if (rollResult <= totalPrey)
                {
                    totalPreyConsumed++;
                }
            }
            totalPreyConsumed = Mathf.Min(totalPreyConsumed, totalPrey);
            if (totalPreyConsumed > 0)
            {
                eco.ReleasePopulation(preyCounts[0].Item1, totalPreyConsumed);
                AisGame.Events.Dispatch(InterveneEvents.OnHunt);
            }

            Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " predators consumed " + totalPreyConsumed);
            yield return SIM_PHASE_SHORT_DELAY;

            InterveneUI.Instance.SetSimPhase("Predator Turn: Starve");
            // Starve
            if (totalPreyConsumed == 0)
            {
                int rollResult = UnityEngine.Random.Range(1, 13);
                if (rollResult <= totalPredators)
                {
                    eco.ReleasePopulation(predatorCounts[0].Item1, 1);
                    Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " predators starved 1");
                    AisGame.Events.Dispatch(InterveneEvents.OnStarve);
                }
            }
            yield return SIM_PHASE_SHORT_DELAY;

            InterveneUI.Instance.SetSimPhase("Predator Turn: Reproduce");
            // Reproduce
            int reproduceNum = totalPreyConsumed;
            if (totalPredators > 1)
            // check reproduce condition;
            // check if numEgg >= 1
            {
                reproduceNum = Mathf.FloorToInt(totalPreyConsumed / 2);
                // population + 1, numEgg - 1
            }

            for (int i = 0; i < reproduceNum; i++)
            {
                eco.AddPopulation(predatorCounts[0].Item1, 1, predatorCounts[0].Item3, predatorCounts[0].Item4);
                AisGame.Events.Dispatch(InterveneEvents.OnReproduce);
            }
            Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " predator reproduced " + reproduceNum);
           
            yield return SIM_PHASE_DELAY;

            SetEcosystemFocused(eco, false);
        }

        private IEnumerator ProcessPreyDynamics(Ecosystem eco)
        {
            /*
            Reproduce: Roll 1d6. If the result is less than or equal to the current prey population, add 1 prey population.
            */
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> preyCounts;
            eco.FindByTargetType(ActionTarget.Prey, out preyCounts);
            int totalPrey = 0;
            foreach (var count in preyCounts)
            {
                totalPrey += count.Item2;
            }

            if (totalPrey == 0)
            {
                yield break;
            }

            SetEcosystemFocused(eco, true);

            InterveneUI.Instance.SetSimPhase("Prey Turn: Reproduce");
            // Reproduce
            int rollResult = UnityEngine.Random.Range(1, 7);
            // check reproduce condition and if numEgg >= 1
            if (rollResult <= totalPrey)
            {
                // TODO: how to divvy if multiple types of prey?
                eco.AddPopulation(preyCounts[0].Item1, 1, preyCounts[0].Item3, preyCounts[0].Item4);
                Debug.Log("[InterveneDriver] [InterspeciesDynamics] eco " + eco.EcosystemId + " prey reproduced 1");
                AisGame.Events.Dispatch(InterveneEvents.OnReproduce);
            }

            yield return SIM_PHASE_DELAY;

            SetEcosystemFocused(eco, false);
        }

        private List<Tuple<SerializedHash32, SerializedHash32, int, SerializedHash32>> transferTracker = new List<Tuple<SerializedHash32, SerializedHash32, int, SerializedHash32>>();
        private IEnumerator StagePathwayTransfer(Pathway pathway)
        {
            // for each species in origin which travels along pathway
            var origEco = InvasionModelContainer.Instance.GetEcosystem(pathway.OrigEcosystemId);
            var relevantSpecies = origEco.FindSpeciesWhichTravelBy(pathway.PathwayType);

            foreach (var speciesPair in relevantSpecies)
            {
                // see if transfer triggers
                if (UnityEngine.Random.Range(0, 1.0f) > pathway.TransferTriggerChance) {
                    // do not trigger transfer
                    continue;
                }

                // allocate species according to pathway rates
                int transferNum = 0;
                if (pathway.TransferRateType == RateType.Ratio) {
                    transferNum = Mathf.FloorToInt(origEco.GetPopulation(speciesPair.Item1) * pathway.TransferRate);
                }
                else if (pathway.TransferRateType == RateType.Fixed) {
                    int origPop = origEco.GetPopulation(speciesPair.Item1);
                    if (origEco.IsExternal) {
                        origPop = int.MaxValue;
                    }
                    transferNum = Mathf.FloorToInt(Mathf.Min(origPop, pathway.TransferRate));
                }
                transferNum = Mathf.Max(1, transferNum); // rounded down, but at least 1

                // Process pathway effects (i.e. ballast treatment: -1 invasive from source instead of move)
                foreach (var onTryMoveFromOrigEffect in pathway.OnTryMoveFromOrig)
                {
                    if ((onTryMoveFromOrigEffect.EffectType & PathwayEffectType.BlockAll) != 0)
                    {
                        transferNum = 0;
                    }
                    if ((onTryMoveFromOrigEffect.EffectType & PathwayEffectType.Remove) != 0)
                    {
                        int origPop = origEco.GetPopulation(speciesPair.Item1);
                        if (origPop > 0)
                        {
                            if ((onTryMoveFromOrigEffect.TargetType & ActionTarget.Invasive) != 0) {
                                // origEco.ReleasePopulation(InvasionModel.Instance.CurrModelSetupData.DefaultInvasive.SpeciesId, (int)onTryMoveFromOrigEffect.Value);
                                // For trapped pathway (defined as Remove pathwayEffectType), trap 1 species.
                                origEco.ReleasePopulation(InvasionModel.Instance.CurrModelSetupData.DefaultInvasive.SpeciesId, 1);
                            }
                        }
                    }
                }

                // split species, between orig and dest clusters
                var transferAlloc = new SpeciesTransferAllocation();
                transferAlloc.SpeciesId = speciesPair.Item1;
                transferAlloc.DestEcosystemId = pathway.DestEcosystemId;
                transferAlloc.TransferCount = transferNum;
                transferAlloc.TravelType = speciesPair.Item2;
                transferAlloc.TargetType = speciesPair.Item3;
                m_SpeciesTransfers.Add(transferAlloc);

                transferTracker.Add(new Tuple<SerializedHash32, SerializedHash32, int, SerializedHash32>(pathway.OrigEcosystemId, pathway.DestEcosystemId, transferNum, speciesPair.Item1));
                // Release species from original ecosystem
                // origEco.ReleasePopulation(speciesPair.Item1, transferNum);
            }

            yield return SIM_PHASE_DELAY;
        }

        private void FinalizePathwayTransfers()
        {
            for (int i = m_SpeciesTransfers.Count - 1; i >= 0; i--)
            {
                var destEco = InvasionModelContainer.Instance.GetEcosystem(m_SpeciesTransfers[i].DestEcosystemId);
                if (!destEco.IsExternal) {
                    destEco.AddPopulation(m_SpeciesTransfers[i].SpeciesId, m_SpeciesTransfers[i].TransferCount, m_SpeciesTransfers[i].TravelType, m_SpeciesTransfers[i].TargetType);
                }


                m_SpeciesTransfers.RemoveAt(i);
            }
        }

        private void TrySpawnNest(Ecosystem eco)
        {
            bool nestExists = false;
            foreach (var slot in eco.SecondarySlots)
            {
                foreach (var cluster in slot.Clusters)
                {
                    if (cluster.TargetType == ActionTarget.Nest)
                    {
                        nestExists = true;
                        break;
                    }
                }
            }

            if (!nestExists)
            {
                eco.TryAddNest(1);
            }
        }

        private IEnumerator TriggerNests(Ecosystem eco)
        {
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> nestCounts;
            eco.FindByTargetType(ActionTarget.Trap, out nestCounts);
            int totalNests = 0;
            foreach (var count in nestCounts)
            {
                totalNests += count.Item2;
            }

            if (totalNests == 0)
            {
                yield break;
            }

            SetEcosystemFocused(eco, true);

            foreach (var slot in eco.SecondarySlots)
            {
                foreach (var cluster in slot.Clusters)
                {
                    if (cluster.TargetType == ActionTarget.Nest)
                    {
                        var nest = cluster.GetComponent<Nest>();
                        if (nest != null)
                        {
                            if (UnityEngine.Random.Range(0, 1f) < nest.TriggerOdds)
                            {
                                eco.AddPopulation(nest.SpawnSpeciesId, nest.SpawnAmt * cluster.Population, nest.SpawnTravelType, nest.SpawnTargetType);
                            }
                        }
                    }
                }
            }

            yield return SIM_PHASE_SHORT_DELAY;

            SetEcosystemFocused(eco, false);
        }

        private IEnumerator TriggerTraps(Ecosystem eco)
        {
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> trapCounts;
            eco.FindByTargetType(ActionTarget.Trap, out trapCounts);
            int totalTraps = 0;
            foreach (var count in trapCounts)
            {
                totalTraps += count.Item2;
            }

            if (totalTraps == 0)
            {
                yield break;
            }

            SetEcosystemFocused(eco, true);

            foreach (var slot in eco.SecondarySlots)
            {
                foreach (var cluster in slot.Clusters)
                {
                    if (cluster.TargetType == ActionTarget.Trap)
                    {
                        var trap = cluster.GetComponent<Trap>();
                        if (trap != null)
                        {
                            if (UnityEngine.Random.Range(0, 1f) < trap.TriggerOdds)
                            {
                                eco.ReleasePopulation(trap.TrapSpeciesId, trap.TrapAmt * cluster.Population);
                                AisGame.Events.Dispatch(InterveneEvents.OnTrapTriggered);
                            }
                        }
                    }
                }
            }

            yield return SIM_PHASE_SHORT_DELAY;

            SetEcosystemFocused(eco, false);
        }

        #endregion // Simulate & Modify

        private void SetEcosystemFocused(Ecosystem eco, bool isFocused)
        {
            ModelTag tag = eco.GetComponentInChildren<ModelTag>();
            if (tag == null) { return; }

            if (isFocused)
            {
                tag.SetCustomHighlight(InterveneUI.Instance.FocusColor);
                tag.ShowHighlight(true);
            }
            else
            {
                tag.HideHighlight();
            }
        }

        private void SetPathwayFocused(Pathway path, bool isFocused)
        {
            ModelTag tag = path.GetComponentInChildren<ModelTag>();
            if (tag == null) { return; }

            if (isFocused)
            {
                tag.SetCustomHighlight(InterveneUI.Instance.FocusColor);
                tag.ShowHighlight(true);
                AisGame.Events.Dispatch(InterveneEvents.OnPathwayHighlighted);
            }
            else
            {
                tag.HideHighlight();
            }
        }

        private void HandleInterveneRestart()
        {
            InterveneUI.Instance.HideSimPhase();
            SimRoutine.Stop();
        }
    }
}