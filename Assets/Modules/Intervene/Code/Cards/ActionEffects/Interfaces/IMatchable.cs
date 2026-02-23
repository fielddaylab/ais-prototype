using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IMatchable
    {
        public bool TryMatch(SerializedHash32 toMatch, SerializedHash32 toMatchWith, float modifier);
    }
}