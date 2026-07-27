using AIS.Model;
using BeauUtil;
using System;
using System.Collections.Generic;

namespace AIS.Intervene
{
    /// <summary>
    /// Result of scoring the player's intervention plan against the GDD end conditions.
    /// One star is earned per satisfied condition.
    /// </summary>
    public struct InterveneEvaluation
    {
        public bool InvasiveControlled;      // invasive at zero population, or decreasing
        public bool NativesStable;           // every native species stable or increasing
        public bool AllEcosystemsHaveNative; // every ecosystem holds at least one native

        public int Stars
        {
            get
            {
                int stars = 0;
                if (InvasiveControlled) { stars++; }
                if (NativesStable) { stars++; }
                if (AllEcosystemsHaveNative) { stars++; }
                return stars;
            }
        }
    }

    /// <summary>
    /// A single species' population trend over a window of recent rounds, suitable for UI
    /// display (e.g. "+/- X walleye over the last N rounds").
    /// </summary>
    public struct PopulationTrend
    {
        public SerializedHash32 SpeciesId;   // which species this describes
        public ActionTarget Role;            // Invasive, Predator, or Prey
        public int CurrentPopulation;        // latest recorded total across all ecosystems
        public int NetChange;                // signed change over the window (newest minus window-start)
        public float AverageChange;          // per-round average change over the window (matches star logic)
        public int WindowRounds;             // rounds the window actually spans (< requested window early on)
    }

    /// <summary>
    /// Tracks per-round, per-species population snapshots, scores the end-of-game star conditions,
    /// and exposes per-population trends for UI. Owned by <see cref="InterveneRoundCounterInterfacer"/>;
    /// deliberately a plain class (not a MonoBehaviour) so it needs no scene wiring.
    /// </summary>
    public class InterveneEndConditionEvaluator
    {
        private struct Snapshot
        {
            public Dictionary<SerializedHash32, int> Populations; // species id -> total population that round
        }

        public const int DEFAULT_TREND_WINDOW = 3;

        // Default number of most-recent round-to-round changes summarized by trends and pass-fail checks.
        private int m_TrendWindow = DEFAULT_TREND_WINDOW;

        private readonly List<Snapshot> m_History = new List<Snapshot>();
        private readonly Dictionary<SerializedHash32, ActionTarget> m_Roles = new Dictionary<SerializedHash32, ActionTarget>();
        private readonly List<SerializedHash32> m_SpeciesOrder = new List<SerializedHash32>();

        /// <summary>
        /// Default window (in rounds) used by scoring and by the parameterless trend queries.
        /// Clamped to at least 1. Individual queries may override it with an explicit window.
        /// </summary>
        public int TrendWindow
        {
            get { return m_TrendWindow; }
            set { m_TrendWindow = value < 1 ? 1 : value; }
        }

        /// <summary>Number of rounds recorded so far.</summary>
        public int RoundsRecorded { get { return m_History.Count; } }

        /// <summary>
        /// Captures the current population of every invasive and native species across all
        /// non-external ecosystems. Called once at the end of each round (post-simulation).
        /// </summary>
        public void RecordSnapshot()
        {
            var container = InvasionModelContainer.Instance;
            if (container == null) { return; }

            Snapshot snap = new Snapshot();
            snap.Populations = new Dictionary<SerializedHash32, int>();

            foreach (var eco in container.GetAllEcosystems())
            {
                AccumulateRole(eco, ActionTarget.Invasive, snap.Populations);
                AccumulateRole(eco, ActionTarget.Predator, snap.Populations);
                AccumulateRole(eco, ActionTarget.Prey, snap.Populations);
            }

            m_History.Add(snap);
        }

        private void AccumulateRole(Ecosystem eco, ActionTarget role, Dictionary<SerializedHash32, int> populations)
        {
            List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> counts;
            eco.FindByTargetType(role, out counts);
            foreach (var count in counts)
            {
                SerializedHash32 id = count.Item1;

                int existing;
                populations.TryGetValue(id, out existing);
                populations[id] = existing + count.Item2;

                if (!m_Roles.ContainsKey(id))
                {
                    m_Roles[id] = role;
                    m_SpeciesOrder.Add(id);
                }
            }
        }

        public void Reset()
        {
            m_History.Clear();
            m_Roles.Clear();
            m_SpeciesOrder.Clear();
        }

        public InterveneEvaluation Evaluate()
        {
            InterveneEvaluation result = new InterveneEvaluation();

            result.InvasiveControlled = EvaluateInvasive();
            result.NativesStable = EvaluateNatives();
            result.AllEcosystemsHaveNative = EvaluateEcosystemNativePresence();

            return result;
        }

        #region Trend queries (for UI)

        /// <summary>Per-population trends over the configured <see cref="TrendWindow"/>, invasive first, then natives.</summary>
        public List<PopulationTrend> GetTrends()
        {
            return GetTrends(m_TrendWindow);
        }

        /// <summary>Per-population trends over an explicit window (in rounds), invasive first, then natives.</summary>
        public List<PopulationTrend> GetTrends(int window)
        {
            window = window < 1 ? 1 : window;

            List<PopulationTrend> trends = new List<PopulationTrend>(m_SpeciesOrder.Count);
            AppendTrends(trends, ActionTarget.Invasive, window);
            AppendTrends(trends, ActionTarget.Predator, window);
            AppendTrends(trends, ActionTarget.Prey, window);
            return trends;
        }

        /// <summary>Trend for a single species over the configured window. False if never recorded.</summary>
        public bool TryGetTrend(SerializedHash32 speciesId, out PopulationTrend trend)
        {
            return TryGetTrend(speciesId, m_TrendWindow, out trend);
        }

        /// <summary>Trend for a single species over an explicit window (in rounds). False if never recorded.</summary>
        public bool TryGetTrend(SerializedHash32 speciesId, int window, out PopulationTrend trend)
        {
            if (!m_Roles.ContainsKey(speciesId))
            {
                trend = default(PopulationTrend);
                return false;
            }

            trend = BuildTrend(speciesId, window < 1 ? 1 : window);
            return true;
        }

        private void AppendTrends(List<PopulationTrend> trends, ActionTarget role, int window)
        {
            foreach (var id in m_SpeciesOrder)
            {
                if ((m_Roles[id] & role) != 0)
                {
                    trends.Add(BuildTrend(id, window));
                }
            }
        }

        private PopulationTrend BuildTrend(SerializedHash32 id, int window)
        {
            List<int> series = BuildSpeciesSeries(id);

            PopulationTrend trend = new PopulationTrend();
            trend.SpeciesId = id;
            trend.Role = m_Roles[id];
            trend.CurrentPopulation = series.Count > 0 ? series[series.Count - 1] : 0;
            trend.WindowRounds = WindowSpan(series.Count, window);
            trend.NetChange = NetChangeOverWindow(series, window);
            trend.AverageChange = AverageOfLastDeltas(series, window);
            return trend;
        }

        #endregion // Trend queries

        #region Scoring

        // Condition 1: invasive is at zero population, or decreasing (strict: average change < 0).
        private bool EvaluateInvasive()
        {
            if (m_History.Count == 0) { return false; }

            List<int> series = BuildAggregateSeries(ActionTarget.Invasive);
            if (series[series.Count - 1] == 0) { return true; }

            return AverageOfLastDeltas(series, m_TrendWindow) < 0f;
        }

        // Condition 2: every native species is stable (|avg change| < 1) or increasing,
        // i.e. average change > -1. A native crashing by 1+ per round on average fails.
        private bool EvaluateNatives()
        {
            foreach (var id in m_SpeciesOrder)
            {
                if (!IsNativeRole(m_Roles[id])) { continue; }

                if (AverageOfLastDeltas(BuildSpeciesSeries(id), m_TrendWindow) <= -1f)
                {
                    return false;
                }
            }

            return true;
        }

        // Condition 3: every non-external ecosystem currently contains at least one native individual.
        private bool EvaluateEcosystemNativePresence()
        {
            var container = InvasionModelContainer.Instance;
            if (container == null) { return false; }

            foreach (var eco in container.GetAllEcosystems())
            {
                List<Tuple<SerializedHash32, int, PathwayType, ActionTarget>> nativeCounts;
                eco.FindByTargetType(ActionTarget.Predator | ActionTarget.Prey, out nativeCounts);

                int total = 0;
                foreach (var count in nativeCounts)
                {
                    total += count.Item2;
                }

                if (total <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsNativeRole(ActionTarget role)
        {
            return (role & (ActionTarget.Predator | ActionTarget.Prey)) != 0;
        }

        #endregion // Scoring

        #region Series & window math

        private List<int> BuildSpeciesSeries(SerializedHash32 id)
        {
            List<int> series = new List<int>(m_History.Count);
            foreach (var snap in m_History)
            {
                int pop;
                snap.Populations.TryGetValue(id, out pop);
                series.Add(pop);
            }
            return series;
        }

        private List<int> BuildAggregateSeries(ActionTarget role)
        {
            List<int> series = new List<int>(m_History.Count);
            foreach (var snap in m_History)
            {
                int total = 0;
                foreach (var kvp in snap.Populations)
                {
                    if ((m_Roles[kvp.Key] & role) != 0)
                    {
                        total += kvp.Value;
                    }
                }
                series.Add(total);
            }
            return series;
        }

        // Number of recent rounds the window actually spans, given how many snapshots exist.
        private int WindowSpan(int seriesCount, int window)
        {
            int deltaCount = seriesCount - 1;
            if (deltaCount <= 0) { return 0; }
            if (window < 1) { window = 1; }

            return deltaCount < window ? deltaCount : window;
        }

        // Signed change over the window: newest value minus the value 'window' rounds earlier.
        private int NetChangeOverWindow(List<int> series, int window)
        {
            int span = WindowSpan(series.Count, window);
            if (span <= 0) { return 0; }

            return series[series.Count - 1] - series[series.Count - 1 - span];
        }

        /// <summary>
        /// Average of the most recent (up to <paramref name="window"/>) consecutive round-to-round
        /// deltas. Returns 0 when there are fewer than two data points.
        /// </summary>
        private float AverageOfLastDeltas(List<int> series, int window)
        {
            int span = WindowSpan(series.Count, window);
            if (span <= 0) { return 0f; }

            int sum = 0;
            for (int i = series.Count - span; i < series.Count; i++)
            {
                sum += series[i] - series[i - 1];
            }

            return sum / (float)span;
        }

        #endregion // Series & window math
    }
}
