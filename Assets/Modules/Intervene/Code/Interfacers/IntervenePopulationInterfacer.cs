using AIS.Narrative;
using BeauRoutine;
using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class IntervenePopulationInterfacer : MonoBehaviour
    {
        public static IntervenePopulationInterfacer Instance;

        public GameObject PopulationStatus;

        private void Awake()
        {
            Instance = this;
        }

        public void LoadPopulation()
        {
            Instance.gameObject.SetActive(true);
        }

        public void Hide()
        {
            Instance.gameObject.SetActive(false);
        }
    }
}