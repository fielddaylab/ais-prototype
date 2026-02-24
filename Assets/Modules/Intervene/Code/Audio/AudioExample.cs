using FieldDay.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class AudioExample : MonoBehaviour
    {
        private void Start()
        {
            Sfx.Play("Oneshot.Example");
        }
    }
}
