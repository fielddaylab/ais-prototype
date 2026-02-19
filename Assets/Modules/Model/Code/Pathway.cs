using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AIS.Intervene;

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
    }

    [Flags]
    public enum PathwayType
    {
        Currents = 0x01,
        CaptivityTrade = 0x02,
        BoatHulls = 0x04,
        BaitBuckets = 0x08,
        BallastWater = 0x10,
    }

    public enum RateType
    {
        Ratio,
        Fixed
    }

    public class Pathway : MonoBehaviour, IReducible, IIncreasable
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        public SerializedHash32 OrigEcosystemId;
        public SerializedHash32 DestEcosystemId;
        public PathwayType PathwayType { get; private set; }
        public RateType TransferRateType { get; private set; }
        public float TransferTriggerChance { get; private set; }
        public float TransferRate { get; private set; }

        #endregion // Inspector

        public void LoadData(PathwaySetupData setupData)
        {
            OrigEcosystemId = setupData.OrigEcosystemId;
            DestEcosystemId = setupData.DestEcosystemId;
            PathwayType = setupData.PathwayType;

            this.transform.position = setupData.Pos;
            MainRenderer.sprite = setupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.PATHWAY_SORTING;

            TransferRateType = setupData.TransferRateType;
            SetTransferRate(setupData.StartingTransferRate);
            SetTriggerChance(setupData.StartingTriggerChance);
        }

        public void AddPathwayType(PathwayType type)
        {
            PathwayType |= type;
        }

        public void RemovePathwayType(PathwayType type)
        {
            PathwayType &= ~type;
        }

        public void SetTransferRate(float newRate)
        {
            TransferRate = newRate;
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
        }

        public void SetTriggerChance(float newChance)
        {
            TransferTriggerChance = newChance;
        }

        #region Interfaces

        // IReducible

        public bool TryReduce(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(-amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate - TransferRate * amt;
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        // IIncreasable

        public bool TryIncrease(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate + TransferRate * amt;
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        #endregion // Interfaces
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