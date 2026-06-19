using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    [RequireComponent(typeof(Comparable))]
    public class StatsInterfacer : MonoBehaviour, IComparable
    {
        public static StatsInterfacer Instance;

        public const string SOCIAL_KEY = "social";
        public const string OUTDOOR_KEY = "outdoor";
        public const string TECH_KEY = "tech";
        public const string RESEARCH_KEY = "research";
        public const string INNOVATE_KEY = "innovate";

        public enum StatType
        {
            Social,
            Outdoor,
            Tech,
            Research,
            Innovate
        }

        public struct InterveneStats
        {
            public Dictionary<StatType, int> StatDict;

            public InterveneStats(int social, int outdoor, int tech, int research, int innovate)
            {
                StatDict = new Dictionary<StatType, int>();
                StatDict.Add(StatType.Social, social);
                StatDict.Add(StatType.Outdoor, outdoor);
                StatDict.Add(StatType.Tech, tech);
                StatDict.Add(StatType.Research, research);
                StatDict.Add(StatType.Innovate, innovate);
            }

            public void AdjustStat(StatType stat, int val)
            {
                var newVal = StatDict[stat];
                newVal += val;
                StatDict[stat] = newVal;
            }
        }

        [HideInInspector] public InterveneStats WorkingStats = new InterveneStats();

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // TEMP
            // LoadPlayerStats(0, 0, 0, 0);
        }

        #endregion // Unity Callbacks

        public void LoadPlayerStats(int social, int outdoor, int tech, int research, int innovate)
        {
            WorkingStats = new InterveneStats(social, outdoor, tech, research, innovate);
        }

        public void AdjustPlayerStat(StatType stat, int amt)
        {
            WorkingStats.AdjustStat(stat, amt);
        }

        #region Interfaces

        // IComparable

        public SerializedHash32 GetId()
        {
            return GetComponent<Comparable>().Id;
        }

        public float GetValue(string key = null)
        {
            switch (key)
            {
                case SOCIAL_KEY:
                    return WorkingStats.StatDict[StatType.Social];
                case OUTDOOR_KEY:
                    return WorkingStats.StatDict[StatType.Outdoor];
                case TECH_KEY:
                    return WorkingStats.StatDict[StatType.Tech];
                case RESEARCH_KEY:
                    return WorkingStats.StatDict[StatType.Research];
                case INNOVATE_KEY:
                    return WorkingStats.StatDict[StatType.Innovate];
                default:
                    return -1;
            }
        }

        public void SetValue(float val, string key = null)
        {
            switch (key)
            {
                case SOCIAL_KEY:
                    AdjustPlayerStat(StatType.Social, (int)val - WorkingStats.StatDict[StatType.Social]);
                    break;
                case OUTDOOR_KEY:
                    AdjustPlayerStat(StatType.Outdoor, (int)val - WorkingStats.StatDict[StatType.Outdoor]);
                    break;
                case TECH_KEY:
                    AdjustPlayerStat(StatType.Tech, (int)val - WorkingStats.StatDict[StatType.Tech]);
                    break;
                case RESEARCH_KEY:
                    AdjustPlayerStat(StatType.Research, (int)val - WorkingStats.StatDict[StatType.Research]);
                    break;
                default:
                    break;
            }
        }

        #endregion // Interfaces
    }
}