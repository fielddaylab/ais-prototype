using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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

    public class Pathway : MonoBehaviour
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        public SerializedHash32 OrigEcosystemId;
        public SerializedHash32 DestEcosystemId;
        public PathwayType PathwayType { get; private set; }
        public RateType TransferRateType { get; private set; }
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
    }
}