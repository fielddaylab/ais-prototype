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
        public int Population;
        public PathwayType TravelType;

        public Sprite SpeciesSprite;
    }

    public class SpeciesCluster : MonoBehaviour
    {
        public SpriteRenderer BGRenderer;
        public SpriteRenderer IconRenderer;
        public TMP_Text PopulationText;

        [HideInInspector] public int Population;

        public SpeciesSetupData CurrSetupData { get; private set; }

        public void LoadData(SpeciesSetupData setupData)
        {
            CurrSetupData = setupData;

            Population = setupData.Population;

            // TODO: assign to pos slot in ecosystem?
            // this.transform.position = m_CurrSetupData.Pos;
            IconRenderer.sprite = CurrSetupData.SpeciesSprite;
            PopulationText.SetText("x" + Population.ToStringLookup());
            BGRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING;
            IconRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 10;
            PopulationText.GetComponent<MeshRenderer>().sortingOrder = InvasionModelSorting.SPECIES_SORTING + 20;
        }
    }
}