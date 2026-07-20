using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface ITrapModifiable
    {
        public bool TryModifyTrap(List<float> amts, ModifierType modType);
    }
}
