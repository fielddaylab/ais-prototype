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
        private IntervenePopulationGroup[] populationGroups = null;

        public void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnPopulationSnapshotRecorded, UpdatePopulation);
        }

        private void OnDestroy()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnPopulationSnapshotRecorded, UpdatePopulation);
        }

        public void Setup(List<PopulationTrend> populationTrends)
        {
            populationGroups = new IntervenePopulationGroup[populationTrends.Count];

            for (int i = 0; i < populationTrends.Count; i++)
            {
                GameObject popObject = Instantiate(PopulationGroupPrefab);
                popObject.transform.SetParent(PopulationGroupParent, false);

                IntervenePopulationGroup popGroup = popObject.GetComponent<IntervenePopulationGroup>();
                populationGroups[i] = popGroup;
            }
        }

        public void UpdatePopulation()
        {
            var evaluator = InterveneRoundCounterInterfacer.Instance.Evaluator;
            var populationTrends = evaluator.GetTrends(1); // only 1 round of trends

            if (populationGroups == null) Setup(populationTrends);

            for (int i = 0; i < populationTrends.Count; i++)
            {
                PopulationTrend trend = populationTrends[i];

                populationGroups[i].PopulateInfo(trend);
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