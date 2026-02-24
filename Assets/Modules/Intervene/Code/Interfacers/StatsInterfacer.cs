using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class StatsInterfacer : MonoBehaviour
    {
        public static StatsInterfacer Instance;

        public enum StatType
        {
            Social,
            Outdoor,
            Tech,
            Research,
        }

        public struct InterveneStats
        {
            public Dictionary<StatType, int> StatDict;

            public InterveneStats(int social, int outdoor, int tech, int research)
            {
                StatDict = new Dictionary<StatType, int>();
                StatDict.Add(StatType.Social, social);
                StatDict.Add(StatType.Outdoor, social);
                StatDict.Add(StatType.Tech, social);
                StatDict.Add(StatType.Research, social);
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
            LoadPlayerStats(0, 0, 0, 0);
        }

        #endregion // Unity Callbacks

        public void LoadPlayerStats(int social, int outdoor, int tech, int research)
        {
            WorkingStats = new InterveneStats(social, outdoor, tech, research);
        }

        public void AdjustPlayerStat(StatType stat, int amt)
        {
            WorkingStats.AdjustStat(stat, amt);
        }
    }
}