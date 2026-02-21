using AIS.Intervene;
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
        public bool IsExternal;

        public Vector2[] MainSlotPoses;
        public Vector2[] SecondarySlotPoses;
    }

    public class Ecosystem : MonoBehaviour, IAddTrapable, IAddNestable
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        public SerializedHash32 EcosystemId;
        public bool IsExternal;

        public List<SerializedHash32> SpeciesInEcosystem = new List<SerializedHash32>();
        public Dictionary<SerializedHash32, SpeciesSlotData> SpeciesSlotDict = new Dictionary<SerializedHash32, SpeciesSlotData>();
        public Dictionary<SerializedHash32, SpeciesSlotData> SpeciesSecondarySlotDict = new Dictionary<SerializedHash32, SpeciesSlotData>();

        public SpeciesSlot[] MainSlots; // Slots for active species to occupy
        public SpeciesSlot[] SecondarySlots; // Slots for secondary "species" to occupy

        #endregion // Inspector

        public void LoadData(EcosystemSetupData setupData, GameObject transformPrefab)
        {
            EcosystemId = setupData.EcosystemId;
            IsExternal = setupData.IsExternal;

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

            SecondarySlots = new SpeciesSlot[setupData.SecondarySlotPoses.Length];
            for (int i = 0; i < setupData.SecondarySlotPoses.Length; i++)
            {
                var newSlot = Instantiate(transformPrefab, this.transform).AddComponent<SpeciesSlot>();
                newSlot.transform.localPosition = setupData.SecondarySlotPoses[i];
                SecondarySlots[i] = newSlot;
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

        public int FindNextBestSecondarySlot()
        {
            int lowestIndex = -1;
            int lowestOccupancy = int.MaxValue;
            for (int i = 0; i < MainSlots.Length; i++)
            {
                if (SecondarySlots[i].Occupancy() < lowestOccupancy)
                {
                    lowestIndex = i;
                    lowestOccupancy = SecondarySlots[i].Occupancy();
                }
            }

            return lowestIndex;
        }

        public List<Tuple<SerializedHash32, PathwayType, ActionTarget>> FindSpeciesWhichTravelBy(PathwayType travelType)
        {
            List<Tuple<SerializedHash32, PathwayType, ActionTarget>> foundSpecies = new List<Tuple<SerializedHash32, PathwayType, ActionTarget>>();

            foreach (var speciesId in SpeciesInEcosystem)
            {
                var slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
                var slot = MainSlots[slotIndex];
                foreach (var speciesCluster in slot.SpeciesClusters)
                {
                    if ((speciesCluster.TravelType & travelType) != 0)
                    {
                        foundSpecies.Add(new Tuple<SerializedHash32, PathwayType, ActionTarget>(speciesCluster.SpeciesId, speciesCluster.TravelType, speciesCluster.TargetType));
                    }
                }
            }

            return foundSpecies;
        }

        public void AddPopulation(SerializedHash32 speciesId, int addCount, PathwayType travelType, ActionTarget targetType, bool isSecondary = false)
        {
            if (addCount == 0) { return; }

            Dictionary<SerializedHash32, SpeciesSlotData> slotDict = isSecondary ? SpeciesSecondarySlotDict : SpeciesSlotDict;

            if (!SpeciesInEcosystem.Contains(speciesId))
            {
                // create a new cluster population and register it with ecosystem and InvasionModelContainer
                SpeciesInEcosystem.Add(speciesId);

                if (!slotDict.ContainsKey(speciesId))
                {
                    int newSlotIndex = isSecondary ? FindNextBestSecondarySlot() : FindNextBestSlot();
                    SpeciesSlotData slotData = new SpeciesSlotData();
                    slotData.SlotIndex = newSlotIndex;
                    slotData.SpeciesId = speciesId;
                    slotDict.Add(speciesId, slotData);
                }
                else
                {
                    Debug.LogWarning("[Ecosystem] Misalignment between SpeciesInEcosystem list and SpeciesSlotDict!");
                }

                int slotIndex = slotDict[speciesId].SlotIndex;
                var speciesSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

                var newCluster = InvasionModelPrefabs.Instance.CreateSpeciesCluster(InvasionModelContainer.Instance.transform);
                newCluster.Init(speciesId, addCount, travelType, targetType, this);

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
                if (slotDict.ContainsKey(speciesId))
                {
                    var speciesSlot = isSecondary ? SecondarySlots[slotDict[speciesId].SlotIndex] : MainSlots[slotDict[speciesId].SlotIndex];
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

            if (isSecondary)
            {
                SpeciesSecondarySlotDict = slotDict;
            }
            else
            {
                SpeciesSlotDict = slotDict;
            }
        }

        public void ReleasePopulation(SerializedHash32 speciesId, int releaseCount, bool isSecondary = false)
        {
            if (releaseCount == 0) { return; }

            Dictionary<SerializedHash32, SpeciesSlotData> slotDict = isSecondary ? SpeciesSecondarySlotDict : SpeciesSlotDict;

            int slotIndex = slotDict[speciesId].SlotIndex;
            var speciesSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

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

            // Do not modify external
            if (cluster.Population == -1) { return; }

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
                slotDict.Remove(cluster.SpeciesId);
                InvasionModelContainer.Instance.RemoveSpeciesCluster(cluster);
            }

            if (isSecondary)
            {
                SpeciesSecondarySlotDict = slotDict;
            }
            else
            {
                SpeciesSlotDict = slotDict;
            }
        }

        public int GetPopulation(SerializedHash32 speciesId, bool isSecondary = false)
        {
            int slotIndex = isSecondary ? SpeciesSecondarySlotDict[speciesId].SlotIndex : SpeciesSlotDict[speciesId].SlotIndex;
            var speciesSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

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

        #region Interfaces

        // IAddTrapable

        public bool TryAddTrap(int amt)
        {
            // TODO: get const trap id
            AddPopulation("trap", amt, 0, ActionTarget.Trap, isSecondary: true);

            return true;
        }

        // IAddNestable

        public bool TryAddNest(int amt)
        {
            // TODO: get const nest id
            AddPopulation("nest", amt, 0, ActionTarget.Trap, isSecondary: true);

            return true;
        }

        #endregion // Interfaces
    }
}