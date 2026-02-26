using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class InvasionCurveInterfacer : MonoBehaviour
    {
        public static InvasionCurveInterfacer Instance;

        [Serializable]
        public struct CurveThreshold
        {
            public float Threshold;
            public float CurveVal; // effect severity
        }

        public float CurrVal { get; private set; }
        public CurveThreshold[] TimeThresholds;
        public CurveThreshold[] InvasiveThresholds;
        [HideInInspector] public float CurrThresholdVal; // turns elapsed or population count
        public bool BasedOnInvasives = false;
        public float CostIncreaseRate = 1;

        [Header("Visuals")]
        public float MaxCurveVal;
        public Image Fill;

        public void LoadCurve(float thresholdVal)
        {
            CurrThresholdVal = thresholdVal;

            if (BasedOnInvasives)
            {
                // TODO
                /*  A little confusing bc invasive populations are not set until we know the severity of the invasion curve,
                    but then later severity is checked against the invasive populations */
            }
            else
            {
                SetCurveVal(FindClosestTimeThreshold());
            }
        }

        private void Start()
        {
            Instance = this;

            AisGame.Events.Register(InterveneEvents.OnEndTurn, HandleTurnEnded);
        }

        public void ProgressCurveOnTime()
        {
            CurrThresholdVal += 1;
            SetCurveVal(FindClosestTimeThreshold());
        }

        public float FindClosestTimeThreshold()
        {
            return FindClosestThreshold(TimeThresholds);
        }

        public float FindClosestInvasiveThreshold()
        {
            return FindClosestThreshold(InvasiveThresholds);
        }

        public float FindClosestThreshold(CurveThreshold[] thresholds)
        {
            int highestIndex = -1;
            float highestThreshold = float.MinValue;

            // Thresholds trigger at lowest where curve is >= threshold
            for (int i = 0; i < thresholds.Length; i++)
            {
                if ((thresholds[i].Threshold > highestThreshold) && (CurrThresholdVal >= thresholds[i].Threshold))
                {
                    highestIndex = i;
                    highestThreshold = thresholds[i].Threshold;
                }
            }

            if (highestIndex == -1)
            {
                return 0;
            }
            else
            {
                return thresholds[highestIndex].CurveVal;
            }
        }

        public void SetCurveOnInvasivePop(int invasivePop)
        {

        }

        public void SetCurveVal(float val)
        {
            CurrVal = val;

            Fill.fillAmount = CurrVal / MaxCurveVal;

            AisGame.Events.Dispatch(InterveneEvents.OnInvasionLevelChanged);
        }

        private void HandleTurnEnded()
        {
            ProgressCurveOnTime();
        }
    }
}