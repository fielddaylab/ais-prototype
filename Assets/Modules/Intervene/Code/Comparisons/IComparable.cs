using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface IComparable
    {
        public SerializedHash32 GetId();

        public float GetValue();

        public void SetValue(float val);
    }
}