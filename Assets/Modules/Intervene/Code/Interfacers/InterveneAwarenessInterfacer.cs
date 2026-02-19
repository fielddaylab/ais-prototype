using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Intervene
{
    public class InterveneAwarenessInterfacer : MonoBehaviour, IReducible, IIncreasable
    {
        public struct InterveneAwareness
        {
            public int Awareness;
        }

        public TMP_Text ValueText;

        public InterveneAwareness WorkingAwareness = new InterveneAwareness();

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

        public bool TryIncrease(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustAwareness((int)amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(WorkingAwareness.Awareness * amt);

                AdjustAwareness((int)addAmt);

                return true;
            }

            return false;
        }

        public bool TryReduce(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustAwareness(-(int)amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int reduceAmt = Mathf.FloorToInt(WorkingAwareness.Awareness * amt);

                AdjustAwareness((int)reduceAmt);

                return true;
            }

            return false;
        }

        #endregion // Interfaces
    }
}