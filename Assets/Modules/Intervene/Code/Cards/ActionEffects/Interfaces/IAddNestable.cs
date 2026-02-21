using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IAddNestable
    {
        public bool TryAddNest(int amt);
    }
}