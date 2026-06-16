using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene {
    public class UICard : MonoBehaviour
    {
        [HideInInspector] public SerializedHash32 CardID;

        [HideInInspector] public CardBase CardData;

        [Header("Front")]
        public CanvasGroup FrontGroup;
        public Image Suit;
        public GameObject SuitRequirement;
        public TMP_Text Title;
        public TMP_Text CostText;
        public TMP_Text Description;
        public TMP_Text AdditionalDesc;
        public Image Img;
        public Image Highlight;
        public Button ClickBtn;
        public Canvas CanvasOverride;
        public GraphicRaycaster RaycasterOverride;

        [Header("Back")]
        public CanvasGroup BackGroup;
        public Image BackImg;

        [HideInInspector] public int StackIndex;

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnInvasionLevelChanged, HandleInvasionLevelChanged);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnInvasionLevelChanged, HandleInvasionLevelChanged);
        }

        private void HandleInvasionLevelChanged()
        {
            if (CardData == null) { return; }

            CostText.SetText("$" + CardData.GetAdjustedCost().ToStringLookup());
        }
    }
}
