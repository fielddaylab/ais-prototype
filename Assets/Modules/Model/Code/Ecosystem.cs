using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Model
{
    [Serializable]
    public struct EcosystemSetupData
    {
        public SerializedHash32 EcosystemId;
        public Vector2 Pos;
        public Sprite Sprite;
    }

    public class Ecosystem : MonoBehaviour
    {
        #region Inspector

        public SpriteRenderer MainRenderer;

        #endregion // Inspector

        public EcosystemSetupData CurrSetupData { get; private set; }

        public void LoadData(EcosystemSetupData setupData)
        {
            CurrSetupData = setupData;

            this.transform.position = CurrSetupData.Pos;
            MainRenderer.sprite = CurrSetupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.ECOSYSTEM_SORTING;
        }

    }
}