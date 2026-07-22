using System.Collections.Generic;
using AIS.Intervene;
using AIS.Model;
using BeauUtil;
using FieldDay;
using UnityEngine;

namespace AIS.Narrative
{
    /// <summary>
    /// Which phase the model is being displayed for. Locked details vanish during the narrative
    /// phase, but stay on the model with their data masked once the intervention begins.
    /// </summary>
    public enum SimDetailPhase
    {
        Narrative,
        Intervene
    }

    /// <summary>
    /// Tracks which model details the player has earned the right to see, and applies that state
    /// to the live model.
    ///
    /// Locks are keyed by species id and pathway type rather than by object reference: clusters are
    /// destroyed the moment their population empties and rebuilt when they repopulate, so anything
    /// holding references to them goes stale within a sim tick.
    /// </summary>
    public sealed class SimDetailRegistry : MonoBehaviour
    {
        public Sprite SparkleSprite;
        public SparkleEffectPool SparklePool;

        // Evidence ids waiting on an animated reveal the next time the model panel opens.
        public readonly Queue<StringHash32> RevealQueue = new Queue<StringHash32>();

        private readonly Dictionary<StringHash32, StringHash32> m_SpeciesLocks = new Dictionary<StringHash32, StringHash32>(); // speciesId -> evidenceId
        private readonly Dictionary<PathwayType, StringHash32> m_PathwayLocks = new Dictionary<PathwayType, StringHash32>(); // pathwayType -> evidenceId
        private readonly HashSet<StringHash32> m_Revealed = new HashSet<StringHash32>(); // evidence whose reveal has already landed

        private SimDetailPhase m_Phase;

        #region Locks

        // Single source of truth for evidence -> model detail mappings.
        // Add new entries here when a new mappable evidence card is introduced.
        private void BuildDetailMap()
        {
            m_SpeciesLocks.Clear();
            m_PathwayLocks.Clear();

            InvasionModelSetupData setup = InvasionModel.Instance?.CurrModelSetupData;
            if (setup == null) { return; }

            m_SpeciesLocks[setup.DefaultInvasive.SpeciesId] = "Evidence-Lamprey-Discovered";
            m_SpeciesLocks[setup.DefaultPredator.SpeciesId] = "Evidence-Lake-Trout";
            m_SpeciesLocks[setup.DefaultPrey.SpeciesId] = "Evidence-cisco";

            m_PathwayLocks[PathwayType.Downstream] = "Evidence-Feed-Downstream";
            m_PathwayLocks[PathwayType.Upstream] = "Evidence-Spawn-Upstream";
        }

        /// <summary>
        /// Returns the evidence card that unlocks this detail, or a null hash when nothing locks it.
        /// </summary>
        private StringHash32 FindLock(ISimDetail detail)
        {
            if (detail is Cluster cluster)
            {
                m_SpeciesLocks.TryGetValue(cluster.ContentsId, out StringHash32 speciesLock);
                return speciesLock;
            }

            if (detail is Pathway pathway)
            {
                foreach (var kvp in m_PathwayLocks)
                {
                    // Pathway types are flags, so one pathway can carry several locked types.
                    // First unearned match wins, so a pathway stays masked until every type is earned.
                    if ((pathway.PathwayType & kvp.Key) == 0) { continue; }
                    if (!m_Revealed.Contains(kvp.Value)) { return kvp.Value; }
                }
            }

            return StringHash32.Null;
        }

        #endregion // Locks

        #region State

        /// <summary>
        /// Call before the model is built for a phase. Rebuilds the lock map off the setup data the
        /// model is about to load, so details created during setup can apply themselves.
        /// </summary>
        public void SetPhase(SimDetailPhase phase)
        {
            m_Phase = phase;
            BuildDetailMap();
        }

        /// <summary>
        /// Drops all reveal progress. Not needed on a fresh load -- the model scene is rebuilt on
        /// every phase change, so a new registry already starts blank.
        /// </summary>
        public void ResetReveals()
        {
            m_Revealed.Clear();
            RevealQueue.Clear();
        }

        /// <summary>
        /// Treats every evidence chip the player owns as already revealed, with nothing left to
        /// animate. Used when entering the intervene phase, where reveals do not play out.
        /// </summary>
        public void SyncFromInventory()
        {
            ResetReveals();

            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return; }

            foreach (StringHash32 evidenceId in inv.EvidenceChips)
            {
                m_Revealed.Add(evidenceId);
            }
        }

        /// <summary>
        /// Queues an animated reveal. Returns true if this evidence unlocks something the player
        /// has not seen yet, so callers can tell whether anything is pending.
        /// </summary>
        public bool TryEnqueueReveal(StringHash32 evidenceId)
        {
            if (m_Revealed.Contains(evidenceId)) { return false; }
            if (!LocksAnything(evidenceId)) { return false; }
            if (RevealQueue.Contains(evidenceId)) { return false; }

            RevealQueue.Enqueue(evidenceId);
            return true;
        }

        public void MarkRevealed(StringHash32 evidenceId)
        {
            m_Revealed.Add(evidenceId);
        }

        private bool LocksAnything(StringHash32 evidenceId)
        {
            foreach (var kvp in m_SpeciesLocks)
            {
                if (kvp.Value == evidenceId) { return true; }
            }

            foreach (var kvp in m_PathwayLocks)
            {
                if (kvp.Value == evidenceId) { return true; }
            }

            return false;
        }

        #endregion // State

        #region Apply

        /// <summary>
        /// Brings one detail in line with what the player has earned. Details call this on
        /// themselves as they are created, which is what keeps mid-sim cluster churn correct.
        /// </summary>
        public void ApplyTo(ISimDetail detail)
        {
            if (detail == null) { return; }

            StringHash32 lockId = FindLock(detail);

            if (lockId.IsEmpty || m_Revealed.Contains(lockId))
            {
                detail.SetDisplay(SimDetailDisplay.Revealed, PlayerStatId.Invalid);
                return;
            }

            if (m_Phase == SimDetailPhase.Narrative)
            {
                detail.SetDisplay(SimDetailDisplay.Hidden, PlayerStatId.Invalid);
                return;
            }

            detail.SetDisplay(SimDetailDisplay.Obscured, LookupSuit(lockId));
        }

        /// <summary>
        /// Re-applies visibility across the whole live model. Cheap enough to run whenever the
        /// model is shown, and it corrects the default-active prefab state of anything new.
        /// </summary>
        public void ApplyAll()
        {
            InvasionModelContainer container = InvasionModel.Instance?.m_ModelContainer;
            if (container == null) { return; }

            foreach (Cluster cluster in container.m_SpeciesClusters)
            {
                ApplyTo(cluster);
            }

            foreach (Pathway pathway in container.m_Pathways)
            {
                ApplyTo(pathway);
            }
        }

        /// <summary>
        /// Fills output with the live details this evidence unlocks. Resolved fresh each call, so
        /// it never hands back a destroyed cluster.
        /// </summary>
        public void CollectTargets(StringHash32 evidenceId, List<ISimDetail> output)
        {
            output.Clear();

            InvasionModelContainer container = InvasionModel.Instance?.m_ModelContainer;
            if (container == null) { return; }

            foreach (Cluster cluster in container.m_SpeciesClusters)
            {
                if (m_SpeciesLocks.TryGetValue(cluster.ContentsId, out StringHash32 speciesLock) && speciesLock == evidenceId)
                {
                    output.Add(cluster);
                }
            }

            foreach (var kvp in m_PathwayLocks)
            {
                if (kvp.Value != evidenceId) { continue; }

                foreach (Pathway pathway in container.m_Pathways)
                {
                    if ((pathway.PathwayType & kvp.Key) != 0 && !output.Contains(pathway))
                    {
                        output.Add(pathway);
                    }
                }
            }
        }

        private static PlayerStatId LookupSuit(StringHash32 evidenceId)
        {
            EvidenceCard card = Find.NamedAsset<EvidenceCard>(evidenceId);
            return card != null ? card.Suit : PlayerStatId.Invalid;
        }

        #endregion // Apply
    }
}
