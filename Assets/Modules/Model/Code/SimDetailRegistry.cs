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
        public Sprite SparkleSprite;
        public SparkleEffectPool SparklePool;
        public List<ISimDetail> DetailsToShow; // keep track of all gameobjects that should be revealed
        public Queue<StringHash32> RevealQueue = new Queue<StringHash32>(); // evidence IDs pending animated reveal

        private Dictionary<StringHash32, List<ISimDetail>> m_DetailMap; // built lazily; invalidated on HideAll

        private void Start()
        {
            HideAll();
            //InitDetails();
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

        // Single source of truth for evidence → model detail mappings.
        // Add new entries here when a new mappable evidence card is introduced.
        private void BuildDetailMap()
        {
            m_DetailMap = new Dictionary<StringHash32, List<ISimDetail>>();
            List<Ecosystem> ecosystems = InvasionModel.Instance.m_ModelContainer.m_Ecosystems;
            List<Pathway> pathways = InvasionModel.Instance.m_ModelContainer.m_Pathways;

            StringHash32 invasiveId = InvasionModel.Instance.m_InitModelSetupData.DefaultInvasive.SpeciesId;
            var invasiveClusters = new List<ISimDetail>();
            foreach (Ecosystem ecosystem in ecosystems)
            {
                if (ecosystem == null) continue;
                Cluster c = ecosystem.GetCluster(invasiveId);
                if (c != null) invasiveClusters.Add(c);
            }
            m_DetailMap["Evidence-Lamprey-Discovered"] = invasiveClusters;

            var downstreamPathways = new List<ISimDetail>();
            var upstreamPathways = new List<ISimDetail>();
            foreach (Pathway pathway in pathways)
            {
                if (pathway == null) continue;
                if (pathway.PathwayType == PathwayType.Downstream) downstreamPathways.Add(pathway);
                if (pathway.PathwayType == PathwayType.Upstream) upstreamPathways.Add(pathway);
            }
            m_DetailMap["Evidence-Feed-Downstream"] = downstreamPathways;
            m_DetailMap["Evidence-Spawn-Upstream"] = upstreamPathways;
        }

        public List<ISimDetail> MapEvidenceToDetail(StringHash32 evidenceId)
        {
            if (m_DetailMap == null) BuildDetailMap();
            m_DetailMap.TryGetValue(evidenceId, out List<ISimDetail> targets);
            return targets ?? new List<ISimDetail>();
        }

        // Returns true if evidenceId maps to at least one not-yet-shown SimDetail and was enqueued.
        public bool TryEnqueueReveal(StringHash32 evidenceId)
        {
            List<ISimDetail> targets = MapEvidenceToDetail(evidenceId);
            if (targets.Count == 0) return false;
            foreach (var t in targets)
            {
                if (!DetailsToShow.Contains(t))
                {
                    RevealQueue.Enqueue(evidenceId);
                    return true;
                }
            }
            return false;
        }

        public void HideAll()
        {
            DetailsToShow = new List<ISimDetail>();
            RevealQueue.Clear();
            m_DetailMap = null; // invalidate so stale model references aren't held
        }

        // Call when the model panel opens: shows details already in DetailsToShow, hides all others.
        // Corrects the default-active prefab state so unearned details aren't visible.
        public void ApplyInitialVisibility()
        {
            if (m_DetailMap == null) BuildDetailMap();
            foreach (var kvp in m_DetailMap)
            {
                foreach (ISimDetail target in kvp.Value)
                {
                    if (DetailsToShow.Contains(target))
                        target.Show();
                    else
                        target.Hide();
                }
            }
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
            foreach(StringHash32 card in inv.EvidenceChips)
            {
                foreach(ISimDetail simDetail in MapEvidenceToDetail(card))
                {
                    DetailsToShow.Add(simDetail);
                }
            }
        }
    }
}

