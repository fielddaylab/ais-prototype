using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Model
{
    [Serializable]
    public struct SpeciesSetupData
    {
        public SerializedHash32 SpeciesId;
        public SerializedHash32 StartingEcosystemId;
        public int StartingPopulation;
        public PathwayType StartingTravelType;

        public Sprite SpeciesSprite;
    }

    public class SpeciesCluster : MonoBehaviour
    {
        public SpriteRenderer BGRenderer;
        public SpriteRenderer IconRenderer;
        public TMP_Text PopulationText;

        public SerializedHash32 SpeciesId;
        [HideInInspector] public int Population;
        public PathwayType TravelType;

        public void LoadData(SpeciesSetupData setupData)
        {
            Init(setupData.SpeciesId, setupData.StartingPopulation, setupData.StartingTravelType);
        }

        public void Init(SerializedHash32 speciesId, int population, PathwayType travelType)
        {
            SpeciesId = speciesId;
            Population = population;
            TravelType = travelType;

            IconRenderer.sprite = ModelSpriteLookup.Instance.LookupSpeciesIcon(speciesId);
            PopulationText.SetText("x" + Population.ToStringLookup());
            BGRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING;
            IconRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 10;
            PopulationText.GetComponent<MeshRenderer>().sortingOrder = InvasionModelSorting.SPECIES_SORTING + 20;
        }

        public void AdjustPopulation(int amt)
        {
            Population += amt;
            PopulationText.SetText("x" + Population.ToStringLookup());
        }
    }
}