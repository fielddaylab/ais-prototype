using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class Comparable : MonoBehaviour
    {
        public SerializedHash32 Id;
        public GameObject QueriableObj;

        private void Start()
        {
            ComparisonFacilitator.Instance.RegisterComparable(this);
        }

        private void OnDestroy()
        {
            if (AisGame.IsShuttingDown) { return; }

            ComparisonFacilitator.Instance.DeregisterComparable(this);
        }
    }
}