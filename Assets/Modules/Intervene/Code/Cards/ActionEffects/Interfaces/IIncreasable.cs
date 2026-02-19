using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IIncreasable
    {
        public bool TryIncrease(float amt, ModifierType modType);
    }
}