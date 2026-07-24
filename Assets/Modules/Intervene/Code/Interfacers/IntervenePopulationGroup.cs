using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AIS.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene {
public class IntervenePopulationGroup : MonoBehaviour
    {
        public TMP_Text PopulationLabel;
        public Image PopulationImage;

        public TMP_Text PopulationNumber;
        public Image PopulationTrendImage;

        public Sprite UpTrend, DownTrend;

        public void PopulateInfo(PopulationTrend trend)
        {
            string speciesName = trend.SpeciesId.ToDebugString();
            string rawName = speciesName.Replace("-", " ");
            PopulationLabel.text = string.Join(" ", rawName.Split(' ')
                .Select(word => word.Length > 0 
                    ? char.ToUpper(word[0]) + word.Substring(1).ToLower() 
                    : word));

            PopulationImage.sprite = ModelSpriteLookup.Instance.LookupSpeciesIcon(trend.SpeciesId);

            PopulationNumber.text = trend.CurrentPopulation + "M";

            int trendDirection = trend.NetChange;
            if (trendDirection < 0)
            {
                PopulationTrendImage.sprite = DownTrend;
            } else
            {
                PopulationTrendImage.sprite = UpTrend;
            }
        }
    }
}