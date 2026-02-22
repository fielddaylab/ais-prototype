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

    public class Cluster : MonoBehaviour, IReducible, IIncreasable, IRemovable
    {
        public SpriteRenderer BGRenderer;
        public SpriteRenderer IconRenderer;
        public TMP_Text PopulationText;

        public ModelTag ActionTag;

        [HideInInspector] public SerializedHash32 ContentsId;
        [HideInInspector] public int Population;
        [HideInInspector] public PathwayType TravelType;
        [HideInInspector] public ActionTarget TargetType;

        [HideInInspector] public Ecosystem ParentEcosystem;

        /*
        public void LoadData(SpeciesSetupData setupData)
        {
            Init(setupData.SpeciesId, setupData.StartingPopulation, setupData.StartingTravelType, setupData.StartingTargetType);
        }
        */

        public void Init(SerializedHash32 contentsId, int population, PathwayType travelType, ActionTarget targetType, Ecosystem ecosystem)
        {
            ContentsId = contentsId;
            Population = population;
            TravelType = travelType;
            TargetType = targetType;

            ActionTag.TargetType = TargetType;

            ParentEcosystem = ecosystem;

            IconRenderer.sprite = ModelSpriteLookup.Instance.LookupSpeciesIcon(contentsId);
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

        public bool TryReduce(List<float> amts, ModifierType modType)
        {
            bool isSecondary = TargetType == ActionTarget.Nest || TargetType == ActionTarget.Trap;

            if (modType == ModifierType.Fixed)
            {
                ParentEcosystem.ReleasePopulation(ContentsId, (int)amts[0], isSecondary: isSecondary);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int releaseAmt = Mathf.CeilToInt(Population * amts[0]);
                // rounded up, at least 1
                releaseAmt = Mathf.Max(1, releaseAmt);

                ParentEcosystem.ReleasePopulation(ContentsId, (int)releaseAmt, isSecondary: isSecondary);

                return true;
            }

            return false;
        }

        // IIncreasable

        public bool TryIncrease(List<float> amts, ModifierType modType)
        {
            bool isSecondary = TargetType == ActionTarget.Nest || TargetType == ActionTarget.Trap;

            if (modType == ModifierType.Fixed)
            {
                ParentEcosystem.AddPopulation(ContentsId, (int)amts[0], TravelType, TargetType, isSecondary: isSecondary);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(Population * amts[0]);
                // rounded down, but at least 1
                addAmt = Mathf.Max(1, addAmt);

                ParentEcosystem.AddPopulation(ContentsId, (int)addAmt, TravelType, TargetType, isSecondary: isSecondary);

                return true;
            }

            return false;
        }

        // IRemovable

        public bool TryRemove()
        {
            bool isSecondary = TargetType == ActionTarget.Nest || TargetType == ActionTarget.Trap;

            ParentEcosystem.ReleasePopulation(ContentsId, Population, isSecondary: isSecondary);

            return true;
        }

        #endregion // Interfaces
    }
}