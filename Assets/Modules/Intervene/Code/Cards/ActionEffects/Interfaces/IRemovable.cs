using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IRemovable
    {
        public bool TryRemove();
    }
}