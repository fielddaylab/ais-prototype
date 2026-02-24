using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Intervene
{
    [RequireComponent(typeof(Comparable))]
    public class InterveneAwarenessInterfacer : MonoBehaviour, IReducible, IIncreasable, IMatchable, IComparable
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

        public static InterveneAwarenessInterfacer Instance;

        [HideInInspector] public InterveneAwareness WorkingAwareness = new InterveneAwareness();

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // TEMP
            LoadPlayerAwareness(StartingAwareness);
        }

        #endregion // Unity Callbacks

        public void LoadPlayerAwareness(int awarenessLevel)
        {
            ClearAwareness();
            AdjustAwareness(awarenessLevel);
        }

        public void AdjustAwareness(int amt)
        {
            WorkingAwareness.Awareness += amt;

            ValueText.SetText(WorkingAwareness.Awareness.ToStringLookup());
        }

        public void ClearAwareness()
        {
            AdjustAwareness(-WorkingAwareness.Awareness);
        }

        #region Interfaces

        // IIncreasable

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

        // IReducible

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

        // IMatchable

        public bool TryMatch(SerializedHash32 toMatch, SerializedHash32 toMatchWith, float modifier)
        {
            return ComparisonUtility.TryMatch(ComparisonFacilitator.Instance, toMatch, toMatchWith, modifier);
        }

        // IComparable

        public SerializedHash32 GetId()
        {
            return GetComponent<Comparable>().Id;
        }

        public float GetValue(string key = null)
        {
            return WorkingAwareness.Awareness;
        }

        public void SetValue(float val, string key = null)
        {
            AdjustAwareness((int)val - WorkingAwareness.Awareness);
        }

        #endregion // Interfaces
    }
}