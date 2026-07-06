using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public interface ISimDetail
    {
        public void Show();
        public void Hide();
        public StringHash32 Id();
    }
}