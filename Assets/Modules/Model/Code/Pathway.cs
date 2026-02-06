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
        public Vector2 Pos;
        public Sprite Sprite;
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

    public class Pathway : MonoBehaviour
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        #endregion // Inspector

        public PathwaySetupData CurrSetupData { get; private set; }

        public void LoadData(PathwaySetupData setupData)
        {
            CurrSetupData = setupData;

            this.transform.position = CurrSetupData.Pos;
            MainRenderer.sprite = CurrSetupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.PATHWAY_SORTING;
        }
    }
}