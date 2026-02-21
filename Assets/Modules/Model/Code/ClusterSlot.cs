using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public struct ClusterSlotData
    {
        public int SlotIndex;
        public SerializedHash32 ClusterContentsId;
    }

    public class ClusterSlot : MonoBehaviour
    {
        public List<Cluster> Clusters = new List<Cluster>();

        public bool IsOccupied()
        {
            return Clusters.Count > 0;
        }

        public int Occupancy()
        {
            return Clusters.Count;
        }
    }
}
