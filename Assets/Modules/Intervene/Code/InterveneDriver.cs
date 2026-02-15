using AIS.Model;
using BeauUtil;
using FieldDay;
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
        }

        #endregion // Structs and Enums

        private List<SpeciesTransferAllocation> m_SpeciesTransfers = new List<SpeciesTransferAllocation>();

        public void TickSim()
        {
            if (InvasionModelContainer.Instance == null) { return; }

            // TODO: predator / prey dynamics

            // Transfer species along pathways
            foreach (var pathway in InvasionModelContainer.Instance.GetAllPathways())
            {
                StagePathwayTransfer(pathway);
            }

            // Finalize species movements
            FinalizePathwayTransfers();

            Debug.Log("[InterveneDriver] Sim Progressed by 1 tick");
        }

        #region Simulate & Modify

        private void StagePathwayTransfer(Pathway pathway)
        {
            // for each species in origin which travels along pathway
            var origEco = InvasionModelContainer.Instance.GetEcosystem(pathway.OrigEcosystemId);
            var relevantSpecies = origEco.FindSpeciesWhichTravelBy(pathway.PathwayType);

            foreach (var speciesPair in relevantSpecies)
            {
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
                transferNum = Mathf.Max(1, transferNum); ; // rounded down, but at least 1
                // split species, between orig and dest clusters

                // see if transfer triggers
                if (Random.Range(0, 1.0f) > pathway.TransferTriggerChance)
                {
                    // do not trigger transfer
                    continue;
                }

                var transferAlloc = new SpeciesTransferAllocation();
                transferAlloc.SpeciesId = speciesPair.Item1;
                transferAlloc.DestEcosystemId = pathway.DestEcosystemId;
                transferAlloc.TransferCount = transferNum;
                transferAlloc.TravelType = speciesPair.Item2;
                m_SpeciesTransfers.Add(transferAlloc);

                // Release species from original ecosystem
                origEco.ReleasePopulation(speciesPair.Item1, transferNum);
            }
        }

        private void FinalizePathwayTransfers()
        {
            for (int i = m_SpeciesTransfers.Count - 1; i >= 0; i--)
            {
                var destEco = InvasionModelContainer.Instance.GetEcosystem(m_SpeciesTransfers[i].DestEcosystemId);
                if (!destEco.IsExternal) {
                    destEco.AddPopulation(m_SpeciesTransfers[i].SpeciesId, m_SpeciesTransfers[i].TransferCount, m_SpeciesTransfers[i].TravelType);
                }
                m_SpeciesTransfers.RemoveAt(i);
            }
        }

        #endregion // Simulate & Modify
    }
}