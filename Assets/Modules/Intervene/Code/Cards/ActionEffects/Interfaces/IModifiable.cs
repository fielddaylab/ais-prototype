using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IModifiable
    {
        public bool TryModify(List<float> amts, ModifierType modType);
    }
}