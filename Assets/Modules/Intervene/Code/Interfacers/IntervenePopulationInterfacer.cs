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
        [HideInInspector] public IntervenePopulationInterfacer Instance;
        
        public GameObject PopulationGroupPrefab;
        public Transform PopulationGroupParent;
        private IntervenePopulationGroup[] populationGroups;

        public void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnPopulationSnapshotRecorded, UpdatePopulation);
        }

        public void Setup()
        {
            // TODO!
        }

        public void UpdatePopulation()
        {
            var evaluator = InterveneRoundCounterInterfacer.Instance.Evaluator;
            var populationTrends = evaluator.GetTrends(1); // only 1 round of trends

            Clear();

            for (int i = 0; i < populationTrends.Count; i++)
            {
                PopulationTrend trend = populationTrends[i];
                GameObject popObject = Instantiate(PopulationGroupPrefab);
                popObject.transform.SetParent(PopulationGroupParent, false);

                IntervenePopulationGroup popGroup = popObject.GetComponent<IntervenePopulationGroup>();
                popGroup.PopulateInfo(trend);
            }

            gameObject.SetActive(true);
        }

        private void Clear()
        {
            for (int i = 1; i < PopulationGroupParent.childCount; i++)
            {
                Destroy(PopulationGroupParent.GetChild(i).gameObject);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}