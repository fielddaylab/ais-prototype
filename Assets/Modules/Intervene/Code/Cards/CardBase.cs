using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene {
    [Flags]
    public enum CardAttributes
    {
        Action = 0x01,
    }

    public abstract class CardBase
    {
        public CardAttributes Attributes;

        public SerializedHash32 CardID;
        public int BaseCost { get; protected set; }

        public int GetAdjustedCost()
        {
            return (int)(BaseCost + InvasionCurveInterfacer.Instance.CurrVal * InvasionCurveInterfacer.Instance.CostIncreaseRate);
        }

        public void SetBaseCost(int cost)
        {
            BaseCost = cost;
        }

        public abstract void PopulateCardUI(UICard toPopulate);
    }
}

