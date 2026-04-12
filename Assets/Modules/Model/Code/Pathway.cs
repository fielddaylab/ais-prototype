using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AIS.Intervene;
using TMPro;

namespace AIS.Model
{
    [Serializable]
    public struct PathwaySetupData
    {
        public SerializedHash32 PathwayId;
        public SerializedHash32 OrigEcosystemId;
        public SerializedHash32 DestEcosystemId;
        public Vector2 Pos;
        public Sprite Sprite;

        public RateType TransferRateType;
        public float StartingTransferRate;
        public float StartingTriggerChance;
        public PathwayType PathwayType;
        public bool IsNotHidden;
        public PathDir StartingDir;
    }

    [Flags]
    public enum PathwayType
    {
        // Currents = 0x01,
        PetTrade = 0x02,
        BoatHulls = 0x04,
        BaitBuckets = 0x08,
        BallastWater = 0x10,
        Aquarium = 0x20,
        Upstream = 0x40,
        Downstream = 0x80,
    }

    public enum RateType
    {
        Ratio,
        Fixed
    }

    [Flags]
    public enum PathwayEffectType
    {
        BlockAll = 0x01,
        Remove = 0x02,
    }

    public struct PathwayEffect
    {
        public string EffectId;
        public PathwayEffectType EffectType;
        public ActionTarget TargetType;
        public float Value;
    }

    public enum PathDir
    {
        None,
        Input,
        Output
    }

    public class Pathway : MonoBehaviour, IReducible, IIncreasable, IRemovable, IRevealable
    {
        #region Inspector

        public SpriteRenderer MainRenderer;
        public SpriteRenderer PathwayTypeBGRenderer;
        public SpriteRenderer PathwayTypeRenderer;
        public TMP_Text TransferRateText;

        public List<PathwayEffect> OnTryMoveFromOrig = new List<PathwayEffect>();

        public SerializedHash32 OrigEcosystemId;
        public SerializedHash32 DestEcosystemId;
        public PathwayType PathwayType { get; private set; }
        public RateType TransferRateType { get; private set; }
        public float TransferTriggerChance { get; private set; }
        public float TransferRate { get; private set; }
        public bool IsHidden { get; private set; }
        public bool IsTrapped { get; private set; }
        public PathDir Dir { get; private set; }

        #endregion // Inspector

        public void LoadData(PathwaySetupData setupData)
        {
            OrigEcosystemId = setupData.OrigEcosystemId;
            DestEcosystemId = setupData.DestEcosystemId;
            PathwayType = setupData.PathwayType;
            SetIsHidden(!setupData.IsNotHidden);
            SetIsTrapped(false);
            Dir = setupData.StartingDir;

            this.transform.position = setupData.Pos;
            MainRenderer.sprite = setupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.PATHWAY_SORTING;
            PathwayTypeBGRenderer.sortingOrder = InvasionModelSorting.PATHWAY_ICON_BG_SORTING;
            PathwayTypeRenderer.sortingOrder = InvasionModelSorting.PATHWAY_ICON_SORTING;

            TransferRateType = setupData.TransferRateType;
            SetTransferRate(setupData.StartingTransferRate);
            SetTriggerChance(setupData.StartingTriggerChance);

            UpdateVisuals();
        }

        public void AddPathwayType(PathwayType type)
        {
            PathwayType |= type;

            UpdateVisuals();
        }

        public void RemovePathwayType(PathwayType type)
        {
            PathwayType &= ~type;

            UpdateVisuals();
        }

        public void SetTransferRate(float newRate)
        {
            TransferRate = newRate;

            UpdateVisuals();
        }

        public void AdjustTransferRate(float adjustAmt)
        {
            bool wasShut = TransferRate == 0;

            TransferRate += adjustAmt;
            TransferRate = Mathf.Max(TransferRate, 0);
            
            if (!wasShut && TransferRate == 0)
            {
                // TODO: trigger pathway shutting visuals
            }
            else if (wasShut && TransferRate != 0)
            {
                // TODO: trigger pathway opening visuals
            }

            UpdateVisuals();
        }

        public void SetTriggerChance(float newChance)
        {
            TransferTriggerChance = newChance;
        }

        public void SetIsHidden(bool isHidden)
        {
            IsHidden = isHidden;

            UpdateVisuals();
        }

        public void SetIsTrapped(bool isTrapped)
        {
            IsTrapped = isTrapped;

            UpdateVisuals();
        }

        public void AddEffectOnTryMoveFromOrig(PathwayEffect toAdd)
        {
            OnTryMoveFromOrig.Add(toAdd);
        }

        public bool OnTryMoveFromOrigContains(string effectId)
        {
            foreach (var effect in OnTryMoveFromOrig)
            {
                if (effect.EffectId.Equals(effectId))
                {
                    return true;
                }
            }

            return false;
        }

        #region Interfaces

        // IReducible

        public bool TryReduce(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(-amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate - TransferRate * amts[0];
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        // IIncreasable

        public bool TryIncrease(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate + TransferRate * amts[0];
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        // IRemovable

        public bool TryRemove()
        {
            AdjustTransferRate(-TransferRate);

            return true;
        }

        // IRevealable

        public bool TryReveal()
        {
            SetIsHidden(false);

            return true;
        }

        #endregion // Interfaces

        #region Visuals

        private void UpdateVisuals()
        {
            if (IsHidden)
            {
                // update icon to hidden
                PathwayTypeRenderer.sprite = ModelSpriteLookup.Instance.LookupPathwayIcon(PathwayType, true);
                // hide transfer rate
                TransferRateText.gameObject.SetActive(false);
            }
            else
            {
                // update icon
                PathwayTypeRenderer.sprite = ModelSpriteLookup.Instance.LookupPathwayIcon(PathwayType);
                // update transfer rate
                TransferRateText.gameObject.SetActive(true);
                string text = TransferRate + " per turn";
                if (TransferRateType == RateType.Ratio) {
                    text = TransferRate * 100 + "% per turn";
                }
                TransferRateText.SetText(text);
            }
        }

        #endregion // Visuals
    }

    public static class PathwayUtility
    {
        public static PathwayType StrToPathwayType(string toParse)
        {
            PathwayType type = 0;

            if (Enum.TryParse<PathwayType>(toParse, true, out PathwayType result))
            {
                return result;
            }

            return type;
        }
    }
}