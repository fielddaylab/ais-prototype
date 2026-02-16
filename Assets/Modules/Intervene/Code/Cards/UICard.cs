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

        [Header("Front")]
        public CanvasGroup FrontGroup;
        public TMP_Text Title;
        public TMP_Text Description;
        public Image Img;
        public Image Highlight;
        public Button ClickBtn;
        public Canvas CanvasOverride;
        public GraphicRaycaster RaycasterOverride;

        [Header("Back")]
        public CanvasGroup BackGroup;
        public Image BackImg;

        [HideInInspector] public int StackIndex;
    }

    public static class CardUIUtility
    {
        public static void PopulateCard(UICard card, ActionCardData cardData)
        {
            card.CardID = cardData.CardID;
            card.Title.SetText(cardData.Title);
            card.Description.SetText(cardData.Description);
            card.Img.sprite = null; // cardData.ImgPath;
        }
    }
}
