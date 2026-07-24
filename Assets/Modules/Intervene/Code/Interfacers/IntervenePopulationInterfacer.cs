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
        public GameObject PopulationGroupPrefab;
        public Transform PopulationGroupParent;
        private IntervenePopulationGroup[] populationGroups;

        public void Setup()
        {
            // TODO!
        }

        public void UpdatePopulation()
        {
            // TODO!
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}