using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IReducible
    {
        public bool TryReduce(List<float> amts, ModifierType modType);
    }
}