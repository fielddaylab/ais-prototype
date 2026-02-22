using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Intervene
{
    public class InterveneAwarenessInterfacer : MonoBehaviour, IReducible, IIncreasable
    {
        #region Structs

        public struct InterveneAwareness
        {
            public int Awareness;
        }

        #endregion // Structs

        #region Inspector

        public TMP_Text ValueText;
        public int StartingAwareness;

        #endregion // Inspector

        [HideInInspector] public InterveneAwareness WorkingAwareness = new InterveneAwareness();

        #region Unity Callbacks

        private void Start()
        {
            LoadPlayerAwareness(StartingAwareness);
        }

        #endregion // Unity Callbacks

        public void LoadPlayerAwareness(int awarenessLevel)
        {
            WorkingAwareness.Awareness = awarenessLevel;
        }

        public void AdjustAwareness(int amt)
        {
            WorkingAwareness.Awareness += amt;

            ValueText.SetText(WorkingAwareness.Awareness.ToStringLookup());
        }

        #region Interfaces

        public bool TryIncrease(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustAwareness((int)amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(WorkingAwareness.Awareness * amts[0]);

                AdjustAwareness((int)addAmt);

                return true;
            }

            return false;
        }

        public bool TryReduce(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustAwareness(-(int)amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int reduceAmt = Mathf.FloorToInt(WorkingAwareness.Awareness * amts[0]);

                AdjustAwareness((int)reduceAmt);

                return true;
            }

            return false;
        }

        #endregion // Interfaces
    }
}