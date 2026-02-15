using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public static class InvasionModelSorting
    {
        public static int BG_SORTING = -100;
        public static int ECOSYSTEM_SORTING = 0;
        public static int SPECIES_SORTING = 200;
        public static int PATHWAY_SORTING = 500;
    }

    public class InvasionModel : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private InvasionModelPrefabs m_Prefabs;
        [SerializeField] private InvasionModelContainer m_ModelContainer;

        public InvasionModelSetupData m_InitModelSetupData; // TEMP

        #endregion // Inspector

        private InvasionModelSetupData m_CurrModelSetupData;

        #region UnityCallbacks

        private void Start()
        {
            // TEMP
            SetModelSetupData(m_InitModelSetupData);
            RefreshSetup(0);
        }

        #endregion // Unity Callbacks

        /// <summary>
        /// Call at start of every level
        /// </summary>
        /// <param name="setupData"></param>
        public void SetModelSetupData(InvasionModelSetupData setupData)
        {
            m_CurrModelSetupData = setupData;
            InvasionModelContainer.Instance = m_ModelContainer;
        }

        #region Setup

        /// <summary>
        /// Call every time the player toggle to Model view mode, and when Intervene Phase begins
        /// </summary>
        /// <param name="invasionCurve"></param>
        public void RefreshSetup(float invasionCurve)
        {
            // Clear Previous Setup
            m_ModelContainer.ClearWorld();

            // Setup Ecosystems
            SetupEcosystems();

            // Setup Pathways
            SetupPathways();

            // Setup Invasion Curve-specific Things
            SetupInvasionCurve(invasionCurve);

            // Setup Shared
            SetupShared();
        }

        private void SetupEcosystems()
        {
            foreach (var ecosystemData in m_CurrModelSetupData.Ecosystems)
            {
                var newEcosystem = Instantiate(m_Prefabs.EcosystemPrefab, m_ModelContainer.transform).GetComponent<Ecosystem>();
                newEcosystem.LoadData(ecosystemData, m_Prefabs.TransformPrefab);
                m_ModelContainer.RegisterEcosystem(newEcosystem);
            }
        }

        private void SetupPathways()
        {
            foreach (var pathwayData in m_CurrModelSetupData.Pathways)
            {
                var newPathway = Instantiate(m_Prefabs.PathwayPrefab, m_ModelContainer.transform).GetComponent<Pathway>();
                newPathway.LoadData(pathwayData);
                m_ModelContainer.RegisterPathway(newPathway);
            }
        }

        private void SetupInvasionCurve(float invasionCurve)
        {
            var curveThreshold = FindRelevantThreshold(m_CurrModelSetupData.InvasionCurveThresholds, invasionCurve);

            // Setup Species
            SetupSpecies(curveThreshold.SpeciesSetups);
        }

        private void SetupSpecies(SpeciesSetupData[] setupDatas)
        {
            foreach (var speciesData in setupDatas)
            {
                var relevantEcosystem = m_ModelContainer.GetEcosystem(speciesData.StartingEcosystemId);

                relevantEcosystem.AddPopulation(speciesData.SpeciesId, speciesData.StartingPopulation, speciesData.StartingTravelType);
            }
        }

        private void SetupShared()
        {
            // Setup Shared Species
            SetupSpecies(m_CurrModelSetupData.SharedSpeciesSetups);
        }

        #endregion // Setup

        #region Helpers

        private InvasionCurveThreshold FindRelevantThreshold(InvasionCurveThreshold[] thresholds, float invasionCurve)
        {
            int lowestIndex = -1;
            float lowestThreshold = float.MaxValue;

            // Thresholds trigger at lowest where curve is <= threshold
            for (int i = 0; i < thresholds.Length; i++)
            {
                if ((thresholds[i].Threshold < lowestThreshold) && (invasionCurve <= thresholds[i].Threshold))
                {
                    lowestIndex = i;
                    lowestThreshold = thresholds[i].Threshold;
                }
            }

            if (lowestIndex == -1) { return null; }
            else { return thresholds[lowestIndex]; }
        }

        #endregion // Helpers
    }
}
