using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class InvasionModelContainer : MonoBehaviour
    {
        #region Inspector

        public SpriteRenderer BGRenderer;

        #endregion // Inspector

        private List<Ecosystem> m_Ecosystems = new List<Ecosystem>();
        private List<Pathway> m_Pathways = new List<Pathway>();
        private List<SpeciesCluster> m_SpeciesClusters = new List<SpeciesCluster>();

        #region Remove

        public void ClearWorld()
        {
            for (int i = 0; i < m_Ecosystems.Count; i++)
            {
                Destroy(m_Ecosystems[i]);
            }
            m_Ecosystems.Clear();

            for (int i = 0; i < m_Pathways.Count; i++)
            {
                Destroy(m_Pathways[i]);
            }
            m_Pathways.Clear();

            for (int i = 0; i < m_SpeciesClusters.Count; i++)
            {
                Destroy(m_SpeciesClusters[i]);
            }
            m_SpeciesClusters.Clear();

            BGRenderer.sortingOrder = InvasionModelSorting.BG_SORTING;
        }

        #endregion // Remove

        #region Add

        public void AddEcosystem(Ecosystem ecosystem)
        {
            m_Ecosystems.Add(ecosystem);
        }

        public void AddPathway(Pathway pathway)
        {
            m_Pathways.Add(pathway);
        }

        public void AddSpeciesCluster(SpeciesCluster cluster)
        {
            m_SpeciesClusters.Add(cluster);
        }

        #endregion // Add

        #region Query

        public Ecosystem GetEcosystem(SerializedHash32 ecosystemId)
        {
            for (int i = 0; i < m_Ecosystems.Count; i++)
            {
                if (m_Ecosystems[i].CurrSetupData.EcosystemId.Equals(ecosystemId))
                {
                    return m_Ecosystems[i];
                }
            }

            return null;
        }

        #endregion // Query
    }
}