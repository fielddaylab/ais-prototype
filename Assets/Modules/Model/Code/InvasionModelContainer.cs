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

        public List<Ecosystem> m_Ecosystems = new List<Ecosystem>();
        public List<Pathway> m_Pathways = new List<Pathway>();
        public List<Cluster> m_SpeciesClusters = new List<Cluster>();

        public void InitInstance()
        {
            Instance = this;
        }

        #region Remove

        public void ClearWorld()
        {
            for (int i = 0; i < m_Ecosystems.Count; i++)
            {
                Destroy(m_Ecosystems[i].gameObject);
            }
            m_Ecosystems.Clear();

            for (int i = 0; i < m_Pathways.Count; i++)
            {
                Destroy(m_Pathways[i].gameObject);
            }
            m_Pathways.Clear();

            for (int i = 0; i < m_SpeciesClusters.Count; i++)
            {
                Destroy(m_SpeciesClusters[i].gameObject);
            }
            m_SpeciesClusters.Clear();

            BGRenderer.sortingOrder = InvasionModelSorting.BG_SORTING;
        }

        public void RemoveSpeciesCluster(Cluster cluster)
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

        public void RegisterSpeciesCluster(Cluster cluster)
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

        public List<Ecosystem> GetAllEcosystems()
        {
            List<Ecosystem> ecosystems = new List<Ecosystem>();

            foreach (var eco in m_Ecosystems)
            {
                if (!eco.IsExternal)
                {
                    ecosystems.Add(eco);
                }
            }

            return ecosystems;
        }

        public List<Pathway> GetAllPathways()
        {
            return m_Pathways;
        }

        #endregion // Query
    }
}