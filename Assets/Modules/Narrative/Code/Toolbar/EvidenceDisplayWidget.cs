using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayWidget : GuiWidget {
        public Image suit;
        public Image Illustration;
        public TMP_Text Content;
        public Image Type; // model info or actionable card

        [Header("Action Slot")]
        public CanvasGroup ActionSlot;
        public CanvasGroup ActionContents;
        public Image SlotImg;
    }
}