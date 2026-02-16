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
    }
}

