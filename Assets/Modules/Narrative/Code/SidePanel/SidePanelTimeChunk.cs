using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class SidePanelTimeChunk : BatchedComponent {
        public Image Display;
        public Graphic Flash;

        [Header("Config")]
        public Sprite[] Sprites;
    }
}