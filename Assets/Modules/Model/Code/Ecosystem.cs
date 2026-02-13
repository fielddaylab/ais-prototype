using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Model
{
    [Serializable]
    public struct EcosystemSetupData
    {
        public SerializedHash32 EcosystemId;
        public Vector2 Pos;
        public Sprite Sprite;

        public Vector2[] MainSlotPoses;
    }

    public class Ecosystem : MonoBehaviour
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        public SerializedHash32 EcosystemId;

        public List<SerializedHash32> SpeciesInEcosystem = new List<SerializedHash32>();
        public Dictionary<SerializedHash32, SpeciesSlotData> SpeciesSlotDict = new Dictionary<SerializedHash32, SpeciesSlotData>();

        public SpeciesSlot[] MainSlots; // Slots for active species to occupy

        #endregion // Inspector

        public void LoadData(EcosystemSetupData setupData, GameObject transformPrefab)
        {
            EcosystemId = setupData.EcosystemId;

            this.transform.position = setupData.Pos;
            MainRenderer.sprite = setupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.ECOSYSTEM_SORTING;

            MainSlots = new SpeciesSlot[setupData.MainSlotPoses.Length];
            for (int i = 0; i < setupData.MainSlotPoses.Length; i++)
            {
                var newSlot = Instantiate(transformPrefab, this.transform).AddComponent<SpeciesSlot>();
                newSlot.transform.localPosition = setupData.MainSlotPoses[i];
                MainSlots[i] = newSlot;
            }
        }

        public int FindNextBestSlot()
        {
            int lowestIndex = -1;
            int lowestOccupancy = int.MaxValue;
            for (int i = 0; i < MainSlots.Length; i++)
            {
                if (MainSlots[i].Occupancy() < lowestOccupancy)
                {
                    lowestIndex = i;
                    lowestOccupancy = MainSlots[i].Occupancy();
                }
            }

            return lowestIndex;
        }

        public List<Tuple<SerializedHash32, PathwayType>> FindSpeciesWhichTravelBy(PathwayType travelType)
        {
            List<Tuple<SerializedHash32, PathwayType>> foundSpecies = new List<Tuple<SerializedHash32, PathwayType>>();

            foreach (var speciesId in SpeciesInEcosystem)
            {
                var slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
                var slot = MainSlots[slotIndex];
                foreach (var speciesCluster in slot.SpeciesClusters)
                {
                    if ((speciesCluster.TravelType & travelType) != 0)
                    {
                        foundSpecies.Add(new Tuple<SerializedHash32, PathwayType>(speciesCluster.SpeciesId, speciesCluster.TravelType));
                    }
                }
            }

            return foundSpecies;
        }

        public void AddPopulation(SerializedHash32 speciesId, int addCount, PathwayType travelType)
        {
            if (addCount == 0) { return; }

            if (!SpeciesInEcosystem.Contains(speciesId))
            {
                // create a new cluster population and register it with ecosystem and InvasionModelContainer
                SpeciesInEcosystem.Add(speciesId);

                if (!SpeciesSlotDict.ContainsKey(speciesId))
                {
                    int newSlotIndex = FindNextBestSlot();
                    SpeciesSlotData slotData = new SpeciesSlotData();
                    slotData.SlotIndex = newSlotIndex;
                    slotData.SpeciesId = speciesId;
                    SpeciesSlotDict.Add(speciesId, slotData);
                }
                else
                {
                    Debug.LogWarning("[Ecosystem] Misalignment between SpeciesInEcosystem list and SpeciesSlotDict!");
                }

                int slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
                var speciesSlot = MainSlots[slotIndex];

                var newCluster = InvasionModelPrefabs.Instance.CreateSpeciesCluster(InvasionModelContainer.Instance.transform);
                newCluster.Init(speciesId, addCount, travelType);

                var targetPos = speciesSlot.transform.position;
                var yOffset = 1;
                targetPos.y += speciesSlot.Occupancy() * yOffset;
                newCluster.transform.position = targetPos;

                speciesSlot.SpeciesClusters.Add(newCluster);
                InvasionModelContainer.Instance.RegisterSpeciesCluster(newCluster);
            }
            else
            {
                // the relevant species cluster already has a population
                // merge populations
                if (SpeciesSlotDict.ContainsKey(speciesId))
                {
                    var speciesSlot = MainSlots[SpeciesSlotDict[speciesId].SlotIndex];
                    for (int i = 0; i < speciesSlot.SpeciesClusters.Count; i++)
                    {
                        var cluster = speciesSlot.SpeciesClusters[i];
                        if (cluster.SpeciesId.Equals(speciesId))
                        {
                            cluster.AdjustPopulation(addCount);
                        }
                        speciesSlot.SpeciesClusters[i] = cluster;
                    }
                }
                else
                {
                    Debug.LogWarning("[Ecosystem] Misalignment between SpeciesInEcosystem list and SpeciesSlotDict!");
                }
            }
        }

        public void ReleasePopulation(SerializedHash32 speciesId, int releaseCount)
        {
            if (releaseCount == 0) { return; }

            int slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
            var speciesSlot = MainSlots[slotIndex];

            SpeciesCluster cluster = null;
            int clusterIndex = 0;
            for (int i = 0; i < speciesSlot.SpeciesClusters.Count; i++)
            {
                if (speciesSlot.SpeciesClusters[i].SpeciesId.Equals(speciesId))
                {
                    cluster = speciesSlot.SpeciesClusters[i];
                    clusterIndex = i;
                }
            }


            if (cluster == null)
            {
                Debug.LogWarning("[Ecosystem] Tried to release population from a species cluster that does not exist!");
            }

            if (releaseCount < cluster.Population)
            {
                // reduce population
                cluster.AdjustPopulation(-releaseCount);
                speciesSlot.SpeciesClusters[clusterIndex] = cluster;
            }
            else
            {
                // remove cluster
                speciesSlot.SpeciesClusters.Remove(cluster);
                SpeciesInEcosystem.Remove(cluster.SpeciesId);
                SpeciesSlotDict.Remove(cluster.SpeciesId);
                InvasionModelContainer.Instance.RemoveSpeciesCluster(cluster);
            }
        }

        public int GetPopulation(SerializedHash32 speciesId)
        {
            int slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
            var speciesSlot = MainSlots[slotIndex];

            foreach (var speciesCluster in speciesSlot.SpeciesClusters)
            {
                if (speciesCluster.SpeciesId.Equals(speciesId))
                {
                    return speciesCluster.Population;
                }
            }

            Debug.LogWarning("[Ecosystem] Tried to query population on a species not in ecosystem!");
            return -1;
        }
    }
}