using AIS.Intervene;
using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class Nest : MonoBehaviour
    {
        public SerializedHash32 SpawnSpeciesId;
        public int SpawnAmt;
        public PathwayType SpawnTravelType;
        public ActionTarget SpawnTargetType;
        public float TriggerOdds;
    }
}

