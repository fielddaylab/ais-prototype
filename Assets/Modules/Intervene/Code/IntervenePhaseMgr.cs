using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class IntervenePhaseMgr : MonoBehaviour
    {
        public enum IntervenePhase
        {
            Setup,

            Draw,
            SelectCards,
            SpecifyEffects,
            ResolveEffects,
            Discard,

            TickSim,

            Cleanup,
        }
    }
}
