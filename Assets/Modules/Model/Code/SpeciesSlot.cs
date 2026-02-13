using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public struct SpeciesSlotData
    {
        public int SlotIndex;
        public SerializedHash32 SpeciesId;
    }

    public class SpeciesSlot : MonoBehaviour
    {
        public List<SpeciesCluster> SpeciesClusters = new List<SpeciesCluster>();

        public bool IsOccupied()
        {
            return SpeciesClusters.Count > 0;
        }

        public int Occupancy()
        {
            return SpeciesClusters.Count;
        }
    }
}
