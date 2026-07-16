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
        public bool IsTributary;

        public bool InvasiveHunts;
        public bool InvasiveStarves;
        public bool InvasiveReproduces;

        public Vector2[] MainSlotPoses;
        public Vector2[] SecondarySlotPoses;
    }

    public class Ecosystem : MonoBehaviour, IAddTrapable, IAddNestable, IModifiable
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        public SerializedHash32 EcosystemId;
        public bool IsExternal;
        public bool IsTributary;

        public bool InvasiveHunts;
        public bool InvasiveStarves;
        public bool InvasiveReproduces;

        public List<SerializedHash32> SpeciesInEcosystem = new List<SerializedHash32>();
        public Dictionary<SerializedHash32, ClusterSlotData> SpeciesSlotDict = new Dictionary<SerializedHash32, ClusterSlotData>();
        public Dictionary<SerializedHash32, ClusterSlotData> SecondarySlotDict = new Dictionary<SerializedHash32, ClusterSlotData>();

        public ClusterSlot[] MainSlots; // Slots for active species to occupy
        public ClusterSlot[] SecondarySlots; // Slots for secondary "species" to occupy

        #endregion // Inspector

        public void LoadData(EcosystemSetupData setupData, GameObject transformPrefab)
        {
            EcosystemId = setupData.EcosystemId;
            IsExternal = setupData.IsExternal;
            IsTributary = setupData.IsTributary; // for now, external ecosystems are also tributary

            InvasiveHunts = setupData.InvasiveHunts;
            InvasiveStarves = setupData.InvasiveStarves;
            InvasiveReproduces = setupData.InvasiveReproduces;

            this.transform.position = setupData.Pos;
            MainRenderer.sprite = setupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.ECOSYSTEM_SORTING;

            MainSlots = new ClusterSlot[setupData.MainSlotPoses.Length];
            for (int i = 0; i < setupData.MainSlotPoses.Length; i++)
            {
                var newSlot = Instantiate(transformPrefab, this.transform).AddComponent<ClusterSlot>();
                newSlot.transform.localPosition = setupData.MainSlotPoses[i];
                MainSlots[i] = newSlot;
            }

            SecondarySlots = new ClusterSlot[setupData.SecondarySlotPoses.Length];
            for (int i = 0; i < setupData.SecondarySlotPoses.Length; i++)
            {
                var newSlot = Instantiate(transformPrefab, this.transform).AddComponent<ClusterSlot>();
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
            for (int i = 0; i < SecondarySlots.Length; i++)
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
                if (!SpeciesSlotDict.ContainsKey(speciesId)) { continue; }

                var slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
                var slot = MainSlots[slotIndex];
                foreach (var speciesCluster in slot.Clusters)
                {
                    if ((speciesCluster.TravelType & travelType) != 0)
                    {
                        foundSpecies.Add(new Tuple<SerializedHash32, PathwayType, ActionTarget>(speciesCluster.ContentsId, speciesCluster.TravelType, speciesCluster.TargetType));
                    }
                }
            }

            return foundSpecies;
        }

        public void FindByTargetType(ActionTarget targetType, out List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> targetCounts)
        {
            targetCounts = new List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>>();

            foreach (var speciesId in SpeciesInEcosystem)
            {
                if (!SpeciesSlotDict.ContainsKey(speciesId)) { continue; }

                var slotIndex = SpeciesSlotDict[speciesId].SlotIndex;
                var slot = MainSlots[slotIndex];
                foreach (var speciesCluster in slot.Clusters)
                {
                    if ((speciesCluster.TargetType & targetType) != 0)
                    {
                        targetCounts.Add(new Tuple<SerializedHash32, int, PathwayType, ActionTarget>(speciesCluster.ContentsId, speciesCluster.Population, speciesCluster.TravelType, speciesCluster.TargetType));
                    }
                }
            }
        }

        public void AddPopulation(SerializedHash32 clusterContentsId, int addCount, PathwayType travelType, ActionTarget targetType, bool isSecondary = false, bool isPhantom = false)
        {
            if (addCount == 0 && !isPhantom) { return; }

            Dictionary<SerializedHash32, ClusterSlotData> slotDict = isSecondary ? SecondarySlotDict : SpeciesSlotDict;

            if (!SpeciesInEcosystem.Contains(clusterContentsId))
            {
                // create a new cluster population and register it with ecosystem and InvasionModelContainer
                SpeciesInEcosystem.Add(clusterContentsId);

                if (!slotDict.ContainsKey(clusterContentsId))
                {
                    int newSlotIndex = isSecondary ? FindNextBestSecondarySlot() : FindNextBestSlot();
                    ClusterSlotData slotData = new ClusterSlotData();
                    slotData.SlotIndex = newSlotIndex;
                    slotData.ClusterContentsId = clusterContentsId;
                    slotDict.Add(clusterContentsId, slotData);
                }
                else
                {
                    Debug.LogWarning("[Ecosystem] Misalignment between SpeciesInEcosystem list and SpeciesSlotDict!");
                }

                int slotIndex = slotDict[clusterContentsId].SlotIndex;
                var clusterSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

                var newCluster = InvasionModelPrefabs.Instance.CreateCluster(InvasionModelContainer.Instance.transform, targetType);
                newCluster.Init(clusterContentsId, addCount, travelType, targetType, this);

                var targetPos = clusterSlot.transform.position;
                var yOffset = 1;
                targetPos.y += clusterSlot.Occupancy() * yOffset;
                newCluster.transform.position = targetPos;

                newCluster.ActionTag.IsPhantom = isPhantom;

                clusterSlot.Clusters.Add(newCluster);
                InvasionModelContainer.Instance.RegisterSpeciesCluster(newCluster);
            }
            else
            {
                // the relevant species cluster already has a population
                // merge populations
                if (slotDict.ContainsKey(clusterContentsId))
                {
                    var clusterSlot = isSecondary ? SecondarySlots[slotDict[clusterContentsId].SlotIndex] : MainSlots[slotDict[clusterContentsId].SlotIndex];
                    for (int i = 0; i < clusterSlot.Clusters.Count; i++)
                    {
                        var cluster = clusterSlot.Clusters[i];
                        if (cluster.ContentsId.Equals(clusterContentsId))
                        {
                            cluster.AdjustPopulation(addCount);
                            if (cluster.ActionTag.IsPhantom) {
                                cluster.ActionTag.IsPhantom = false;
                            }
                        }
                        clusterSlot.Clusters[i] = cluster;
                    }
                }
                else
                {
                    Debug.LogWarning("[Ecosystem] Misalignment between SpeciesInEcosystem list and SpeciesSlotDict!");
                }
            }

            if (isSecondary)
            {
                SecondarySlotDict = slotDict;
            }
            else
            {
                SpeciesSlotDict = slotDict;
            }
        }

        public void ReleasePopulation(SerializedHash32 clusterContentsId, int releaseCount, bool isSecondary = false, bool isPhantom = false)
        {
            if (releaseCount == 0) { return; }

            Dictionary<SerializedHash32, ClusterSlotData> slotDict = isSecondary ? SecondarySlotDict : SpeciesSlotDict;

            if (!slotDict.ContainsKey(clusterContentsId)) { return; }

            int slotIndex = slotDict[clusterContentsId].SlotIndex;
            var clusterSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

            Cluster cluster = null;
            int clusterIndex = 0;
            for (int i = 0; i < clusterSlot.Clusters.Count; i++)
            {
                if (clusterSlot.Clusters[i].ContentsId.Equals(clusterContentsId))
                {
                    cluster = clusterSlot.Clusters[i];
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
                clusterSlot.Clusters[clusterIndex] = cluster;
            }
            else
            {
                // remove cluster
                clusterSlot.Clusters.Remove(cluster);
                SpeciesInEcosystem.Remove(cluster.ContentsId);
                slotDict.Remove(cluster.ContentsId);
                InvasionModelContainer.Instance.RemoveSpeciesCluster(cluster);
            }

            if (isSecondary)
            {
                SecondarySlotDict = slotDict;
            }
            else
            {
                SpeciesSlotDict = slotDict;
            }
        }

        public void RemovePhantomPopulations()
        {
            for (int i = 0; i < 2; i++)
            {
                bool isSecondary = i == 1;
                Dictionary<SerializedHash32, ClusterSlotData> slotDict = isSecondary ? SecondarySlotDict : SpeciesSlotDict;

                List<SerializedHash32> keys = new List<SerializedHash32>();
                foreach (KeyValuePair<SerializedHash32, ClusterSlotData> pair in slotDict)
                {
                    keys.Add(pair.Key);
                }
                for (int k = 0; k < keys.Count; k++)
                {
                    var clusterContentsId = keys[k];
                    int slotIndex = slotDict[clusterContentsId].SlotIndex;
                    var clusterSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

                    Cluster cluster = null;
                    int clusterIndex = 0;
                    for (int c = 0; c < clusterSlot.Clusters.Count; c++)
                    {
                        if (clusterSlot.Clusters[c].ContentsId.Equals(clusterContentsId))
                        {
                            cluster = clusterSlot.Clusters[c];
                            clusterIndex = c;
                        }
                    }

                    if (cluster == null)
                    {
                        Debug.LogWarning("[Ecosystem] Tried to release phantom population from a species cluster that does not exist!");
                    }

                    // Do not modify external
                    if (cluster.Population == -1) { return; }

                    if (!cluster.ActionTag.IsPhantom) { continue; }

                    // remove cluster
                    clusterSlot.Clusters.Remove(cluster);
                    SpeciesInEcosystem.Remove(cluster.ContentsId);
                    slotDict.Remove(cluster.ContentsId);
                    InvasionModelContainer.Instance.RemoveSpeciesCluster(cluster);

                    if (isSecondary)
                    {
                        SecondarySlotDict = slotDict;
                    }
                    else
                    {
                        SpeciesSlotDict = slotDict;
                    }
                }
            }






            
        }

        public Cluster GetCluster(SerializedHash32 speciesId, bool isSecondary = false)
        {
            if (isSecondary && !SecondarySlotDict.ContainsKey(speciesId))
            {
                return null;
            }

            if (!isSecondary && !SpeciesSlotDict.ContainsKey(speciesId))
            {
                return null;
            }

            int slotIndex = isSecondary ? SecondarySlotDict[speciesId].SlotIndex : SpeciesSlotDict[speciesId].SlotIndex;
            var speciesSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

            foreach (var cluster in speciesSlot.Clusters)
            {
                if (cluster.ContentsId.Equals(speciesId))
                {
                    return cluster;
                }
            }

            Debug.LogWarning("[Ecosystem] Tried to query cluster on a species not in ecosystem!");
            return null;
        }

        public int GetPopulation(SerializedHash32 speciesId, bool isSecondary = false)
        {
            if (isSecondary && !SecondarySlotDict.ContainsKey(speciesId))
            {
                return 0;
            }

            if (!isSecondary && !SpeciesSlotDict.ContainsKey(speciesId))
            {
                return 0;
            }

            int slotIndex = isSecondary ? SecondarySlotDict[speciesId].SlotIndex : SpeciesSlotDict[speciesId].SlotIndex;
            var speciesSlot = isSecondary ? SecondarySlots[slotIndex] : MainSlots[slotIndex];

            foreach (var speciesCluster in speciesSlot.Clusters)
            {
                if (speciesCluster.ContentsId.Equals(speciesId))
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
            AddPopulation("nest", amt, 0, ActionTarget.Nest, isSecondary: true);

            return true;
        }

        // IModifiable

        public bool TryModify(List<float> amts, ModifierType modType)
        {
            if (amts.Count == 3)
            {
                // modify invasives, predators, prey

                // Invasive

                var defaultInvasive = InvasionModel.Instance.CurrModelSetupData.DefaultInvasive;
                int modInvasiveAmt = 0;

                if (modType == ModifierType.Fixed) {
                    modInvasiveAmt = Mathf.FloorToInt(amts[0]);
                }
                else if (modType == ModifierType.Ratio) {
                    modInvasiveAmt = Mathf.CeilToInt(GetPopulation(defaultInvasive.SpeciesId) * amts[0]);
                    // rounded up, at least 1
                    modInvasiveAmt = Mathf.Max(1, modInvasiveAmt);
                }

                if (modInvasiveAmt < 0) {
                    ReleasePopulation(defaultInvasive.SpeciesId, -modInvasiveAmt);
                }
                else if (modInvasiveAmt > 0) {
                    AddPopulation(defaultInvasive.SpeciesId, modInvasiveAmt, defaultInvasive.StartingTravelType, defaultInvasive.StartingTargetType);
                }

                // Predator

                int modPredatorAmt = 0;
                var defaultPredator = InvasionModel.Instance.CurrModelSetupData.DefaultPredator;

                if (modType == ModifierType.Fixed)
                {
                    modPredatorAmt = Mathf.FloorToInt(amts[1]);
                }
                else if (modType == ModifierType.Ratio)
                {
                    modPredatorAmt = Mathf.CeilToInt(GetPopulation(defaultPredator.SpeciesId) * amts[1]);
                    // rounded up, at least 1
                    modPredatorAmt = Mathf.Max(1, modPredatorAmt);
                }

                if (modPredatorAmt < 0) {
                    ReleasePopulation(defaultPredator.SpeciesId, -modPredatorAmt);
                }
                else if (modPredatorAmt > 0) {
                    AddPopulation(defaultPredator.SpeciesId, modPredatorAmt, defaultPredator.StartingTravelType, defaultPredator.StartingTargetType);
                }

                // Prey

                int modPreyAmt = 0;
                var defaultPrey = InvasionModel.Instance.CurrModelSetupData.DefaultPrey;

                if (modType == ModifierType.Fixed)
                {
                    modPreyAmt = Mathf.FloorToInt(amts[2]);
                }
                else if (modType == ModifierType.Ratio)
                {
                    modPreyAmt = Mathf.CeilToInt(GetPopulation(defaultPrey.SpeciesId) * amts[2]);
                    // rounded up, at least 1
                    modPreyAmt = Mathf.Max(1, modPreyAmt);
                }

                if (modPreyAmt < 0)
                {
                    ReleasePopulation(defaultPrey.SpeciesId, -modPreyAmt);
                }
                else if (modPreyAmt > 0) {
                    AddPopulation(defaultPrey.SpeciesId, modPreyAmt, defaultPrey.StartingTravelType, defaultPrey.StartingTargetType);
                }

                return true;
            }

            return false;
        }

        #endregion // Interfaces
    }
}