using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IReducible
    {
        public bool TryReduce(float amt, ModifierType modType);
    }
}