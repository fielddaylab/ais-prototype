using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class ComparisonFacilitator : MonoBehaviour
    {
        public static ComparisonFacilitator Instance;

        public Dictionary<SerializedHash32, Comparable> CompareDict = new Dictionary<SerializedHash32, Comparable>();

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterComparable(Comparable comp)
        {
            if (CompareDict.ContainsKey(comp.Id)) { return; }

            CompareDict.Add(comp.Id, comp);
        }

        public void DeregisterComparable(Comparable comp)
        {
            if (!CompareDict.ContainsKey(comp.Id)) { return; }
            
            CompareDict.Remove(comp.Id);
        }
    }

    public static class ComparisonUtility
    {
        /// <summary>
        /// Set toMatch to the value of toMatchWith + modifier
        /// </summary>
        /// <param name="facilitator"></param>
        /// <param name="toMatch"></param>
        /// <param name="toMatchWith"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static bool TryMatch(ComparisonFacilitator facilitator, SerializedHash32 toMatch, SerializedHash32 toMatchWith, float modifier)
        {
            if (facilitator.CompareDict.ContainsKey(toMatch) && facilitator.CompareDict.ContainsKey(toMatchWith))
            {
                var toMatchComparable = facilitator.CompareDict[toMatch];
                var toMatchWithComparable = facilitator.CompareDict[toMatchWith];

                var toMatchIComparable = toMatchComparable.QueriableObj.GetComponent<IComparable>();
                var toMatchWithIComparable = toMatchWithComparable.QueriableObj.GetComponent<IComparable>();

                var setVal = toMatchWithIComparable.GetValue() + modifier;
                toMatchIComparable.SetValue(setVal);

                return true;
            }

            return false;
        }
    }
}
