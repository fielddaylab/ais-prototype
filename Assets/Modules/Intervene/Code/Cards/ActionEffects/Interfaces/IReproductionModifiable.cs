using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IReproductionModifiable
    {
        public bool TryModifyReproduction(List<float> amts, ModifierType modType);
    }
}
