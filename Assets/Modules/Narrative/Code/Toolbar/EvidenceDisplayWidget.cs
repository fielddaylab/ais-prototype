using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine.UI;
using TMPro;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayWidget : GuiWidget {
        public Image suit;
        public Image Illustration;
        public TMP_Text Content;
        public Image Type; // model info or actionable card
    }
}