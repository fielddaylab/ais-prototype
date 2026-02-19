using AIS.Intervene;
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
        public ActionTarget StartingTargetType;

        public Sprite SpeciesSprite;
    }

    public class SpeciesCluster : MonoBehaviour, IReducible, IIncreasable
    {
        public SpriteRenderer BGRenderer;
        public SpriteRenderer IconRenderer;
        public TMP_Text PopulationText;

        public ModelTag ActionTag;

        public SerializedHash32 SpeciesId;
        [HideInInspector] public int Population;
        public PathwayType TravelType;
        public ActionTarget TargetType;

        [HideInInspector] public Ecosystem ParentEcosystem;

        /*
        public void LoadData(SpeciesSetupData setupData)
        {
            Init(setupData.SpeciesId, setupData.StartingPopulation, setupData.StartingTravelType, setupData.StartingTargetType);
        }
        */

        public void Init(SerializedHash32 speciesId, int population, PathwayType travelType, ActionTarget targetType, Ecosystem ecosystem)
        {
            SpeciesId = speciesId;
            Population = population;
            TravelType = travelType;
            TargetType = targetType;

            ActionTag.TargetType = TargetType;

            ParentEcosystem = ecosystem;

            IconRenderer.sprite = ModelSpriteLookup.Instance.LookupSpeciesIcon(speciesId);
            PopulationText.SetText("x" + Population.ToStringLookup());
            ActionTag.Highlight.sortingOrder = InvasionModelSorting.SPECIES_SORTING;
            BGRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 10;
            IconRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 20;
            PopulationText.GetComponent<MeshRenderer>().sortingOrder = InvasionModelSorting.SPECIES_SORTING + 20;
        }

        public void AdjustPopulation(int amt)
        {
            Population += amt;
            PopulationText.SetText("x" + Population.ToStringLookup());
        }


        #region Interfaces

        // IReducible

        public bool TryReduce(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                ParentEcosystem.ReleasePopulation(SpeciesId, (int)amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int releaseAmt = Mathf.FloorToInt(Population * amt);
                // rounded down, but at least 1
                releaseAmt = Mathf.Max(1, releaseAmt);

                ParentEcosystem.ReleasePopulation(SpeciesId, (int)releaseAmt);

                return true;
            }

            return false;
        }

        // IIncreasable

        public bool TryIncrease(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                ParentEcosystem.AddPopulation(SpeciesId, (int)amt, TravelType, TargetType);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(Population * amt);
                // rounded down, but at least 1
                addAmt = Mathf.Max(1, addAmt);

                ParentEcosystem.AddPopulation(SpeciesId, (int)addAmt, TravelType, TargetType);

                return true;
            }

            return false;
        }

        #endregion // Interfaces
    }
}