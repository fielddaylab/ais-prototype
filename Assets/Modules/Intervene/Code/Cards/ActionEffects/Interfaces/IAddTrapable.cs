using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IAddTrapable
    {
        public bool TryAddTrap(int amt);
    }
}