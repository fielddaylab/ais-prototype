using AIS.Intervene;
using AIS.Narrative;
using AIS.Shared;
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
        [EcosystemId] public SerializedHash32 StartingEcosystemId;
        public int StartingPopulation;
        public PathwayType StartingTravelType;
        public ActionTarget StartingTargetType;
    }

    public class Cluster : MonoBehaviour, IReducible, IIncreasable, IRemovable, IReproductionModifiable, ISimDetail
    {
        //public SpriteRenderer BGRenderer;
        public SpriteRenderer IconRenderer;
        public TMP_Text PopulationText;
        public SpriteRenderer TextHider;

        public ModelTag ActionTag;

        [HideInInspector] public SerializedHash32 ContentsId;
        [HideInInspector] public int Population;
        [HideInInspector] public PathwayType TravelType;
        [HideInInspector] public ActionTarget TargetType;
        [HideInInspector] public Ecosystem ParentEcosystem;
        [HideInInspector] public int NumEgg;
        
        [HideInInspector]public bool IsSpawnable; // spawn condition placeholder

        // Suit stamp on the text hider, cached off the hider's first child.
        private SpriteRenderer m_TextHiderSuitIcon;

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
            NumEgg = 1;
            IsSpawnable = true; // placeholder for now

            ActionTag.TargetType = TargetType;

            ParentEcosystem = ecosystem;

            IconRenderer.sprite = ModelSpriteLookup.Instance.LookupSpeciesIcon(contentsId);
            PopulationText.SetText("x" + Population.ToStringLookup() + "M");
            ActionTag.Highlight.sortingOrder = InvasionModelSorting.SPECIES_SORTING;
            //BGRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 10;

            TextHider.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 17;
            m_TextHiderSuitIcon = TextHider.transform.GetChild(0).GetComponent<SpriteRenderer>();
            m_TextHiderSuitIcon.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 18;

            IconRenderer.sortingOrder = InvasionModelSorting.SPECIES_SORTING + 20;
            PopulationText.GetComponent<MeshRenderer>().sortingOrder = InvasionModelSorting.SPECIES_SORTING + 15;

            // Clusters are created and destroyed all through the sim, so each one asks for its own
            // visibility rather than the registry tracking instances that will not outlive a tick.
            InvasionModel.Instance?.SimDetailRegistry?.ApplyTo(this);
        }

        public void AdjustPopulation(int amt)
        {
            Population += amt;
            PopulationText.SetText("x" + Population.ToStringLookup() + "M");
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

        // IReproductionModifiable

        public bool TryModifyReproduction(List<float> amts, ModifierType modType)
        {
            if (amts.Count == 0) { return false; }

            // The sim reproduces a whole species type at once, so the modifier lives on the ecosystem
            // and outlives this cluster (which is destroyed whenever its population empties out).
            ParentEcosystem.AddReproductionModifier(TargetType, amts[0], modType);

            return true;
        }

        // IRemovable

        public bool TryRemove()
        {
            bool isSecondary = TargetType == ActionTarget.Nest || TargetType == ActionTarget.Trap;

            ParentEcosystem.ReleasePopulation(ContentsId, Population, isSecondary: isSecondary);

            return true;
        }

        // ISimDetail

        public void SetDisplay(SimDetailDisplay display, PlayerStatId lockSuit)
        {
            this.gameObject.SetActive(display != SimDetailDisplay.Hidden);

            // The population text keeps updating underneath -- the hider just sorts above it.
            bool obscurePopulation = display == SimDetailDisplay.Obscured;
            TextHider.gameObject.SetActive(obscurePopulation);

            if (obscurePopulation && lockSuit != PlayerStatId.Invalid && m_TextHiderSuitIcon != null)
            {
                // use question marks by default
                // m_TextHiderSuitIcon.sprite = CardVisualLookupUtility.LookupSuitIcon(lockSuit);
            }
        }

        public StringHash32 Id()
        {
            return ContentsId;
        }

        #endregion // Interfaces
    }
}