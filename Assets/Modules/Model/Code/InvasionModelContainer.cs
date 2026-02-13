using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class InvasionModelContainer : MonoBehaviour
    {
        public static InvasionModelContainer Instance;

        #region Inspector

        public SpriteRenderer BGRenderer;

        #endregion // Inspector

        private List<Ecosystem> m_Ecosystems = new List<Ecosystem>();
        private List<Pathway> m_Pathways = new List<Pathway>();
        private List<SpeciesCluster> m_SpeciesClusters = new List<SpeciesCluster>();

        public void InitInstance()
        {
            Instance = this;
        }

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

        public void RemoveSpeciesCluster(SpeciesCluster cluster)
        {
            m_SpeciesClusters.Remove(cluster);
            Destroy(cluster.gameObject);
        }

        #endregion // Remove

        #region Add

        public void RegisterEcosystem(Ecosystem ecosystem)
        {
            m_Ecosystems.Add(ecosystem);
        }

        public void RegisterPathway(Pathway pathway)
        {
            m_Pathways.Add(pathway);
        }

        public void RegisterSpeciesCluster(SpeciesCluster cluster)
        {
            m_SpeciesClusters.Add(cluster);
        }

        #endregion // Add

        #region Query

        public Ecosystem GetEcosystem(SerializedHash32 ecosystemId)
        {
            for (int i = 0; i < m_Ecosystems.Count; i++)
            {
                if (m_Ecosystems[i].EcosystemId.Equals(ecosystemId))
                {
                    return m_Ecosystems[i];
                }
            }

            return null;
        }

        public List<Pathway> GetAllPathways()
        {
            return m_Pathways;
        }

        #endregion // Query
    }
}