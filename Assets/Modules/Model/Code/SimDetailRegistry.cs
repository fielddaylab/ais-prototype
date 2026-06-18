using System;
using System.Collections.Generic;
using System.Linq;
using AIS.Intervene;
using AIS.Model;
using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using UnityEngine;

namespace AIS.Narrative
{
    public class SimDetail
    {
        public StringHash32 EvidenceId;
        [SerializeField] public List<ISimDetail> Targets;
    }

    public sealed class SimDetailRegistry : MonoBehaviour
    {
        [SerializeField] public SimDetail[] Details = new SimDetail[6]; // length should be number of evidence cards (model info)
        public List<ISimDetail> DetailsToShow; // keep track of all gameobjects that should be revealed

        private void Awake()
        {   
            HideAll();
            InitDetails();
        }

        public void InitDetails()
        {
            for (int i = 0; i < 6; i++) // temp: change 6 to total number of model info evidence cards (of current level)
            {
                // TODO: might need some collections of evidence cards for this to iterate through
                Details[i] = new SimDetail();
                /*
                 * Details[i] = new SimDetail()
                 * {
                 *      EvidenceId = EvidencePacks[i],
                 * }
                 */
                Details[i].Targets = MapEvidenceToDetail(Details[i].EvidenceId);
            }
        }

        public List<ISimDetail> MapEvidenceToDetail(StringHash32 evidenceId)
        {
            List<ISimDetail> targets = new List<ISimDetail>();
            List<Ecosystem> ecosystems = InvasionModelContainer.Instance.m_Ecosystems;
            List<Pathway> pathways = InvasionModelContainer.Instance.m_Pathways;

            //TODO: map the atcual evidence cards to details in InvasionModel components

            if (evidenceId.Equals("Evidence-Lamprey-Discovered"))
            {
                StringHash32 invasiveId = InvasionModel.Instance.m_InitModelSetupData.DefaultInvasive.SpeciesId;
                foreach(Ecosystem ecosystem in ecosystems)
                {
                    Cluster invasiveCluster = ecosystem.GetCluster(invasiveId);
                    targets.Add(invasiveCluster);
                }
            }
            else if (evidenceId.Equals("Evidence-Feed-Downstream")) // Temp: Add all downstream pathway ids
            {
                
                foreach(Pathway pathway in pathways)
                {
                    if (pathway.PathwayType == PathwayType.Downstream)
                    {
                        targets.Add(pathway);
                    }
                }
            }
            else if (evidenceId.Equals("Evidence-Spawn-Upstream"))
            {
                foreach(Pathway pathway in pathways)
                {
                    if (pathway.PathwayType == PathwayType.Upstream)
                    {
                        targets.Add(pathway);
                    }
                }
            }

            return targets;
        }

        public void HideAll()
        {
            DetailsToShow = new List<ISimDetail>();
        }

        public void RevealNewDetails(StringHash32 evidenceId)
        {
            foreach(ISimDetail simDetail in MapEvidenceToDetail(evidenceId))
            {
                DetailsToShow.Add(simDetail);
            }
            // ? Re-Load InvasionModel to refresh
        }

        public void RefreshVisibility()
        {
            HideAll();
            PlayerInventory inv = Find.State<PlayerInventory>();
            foreach(StringHash32 card in inv.EvidenceCards)
            {
                foreach(ISimDetail simDetail in MapEvidenceToDetail(card))
                {
                    DetailsToShow.Add(simDetail);
                }
            }
        }
    }
}

