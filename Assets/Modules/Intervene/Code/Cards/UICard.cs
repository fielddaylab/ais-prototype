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
        public TMP_Text Title;
        public TMP_Text Description;
        public Image Img;
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
