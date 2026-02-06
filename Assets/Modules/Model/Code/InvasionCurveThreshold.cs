using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    [Serializable]
    public class InvasionCurveThreshold
    {
        public float Threshold; // Triggers at an invasion curve value <= this threshold
        public SpeciesSetupData[] SpeciesSetups;
    }
}
