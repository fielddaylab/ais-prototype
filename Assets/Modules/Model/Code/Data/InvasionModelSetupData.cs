using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    /// <summary>
    ///  Defines level setup according to inputs, such as the Invasion Curve
    /// </summary>
    [CreateAssetMenu(menuName = "Invasion Model/New Invasion Model Setup Data")]
    public class InvasionModelSetupData : ScriptableObject
    {
        public EcosystemSetupData[] Ecosystems;
        public PathwaySetupData[] Pathways;
        public InvasionCurveThreshold[] InvasionCurveThresholds;
        public SpeciesSetupData[] SharedSpeciesSetups;
    }
}
