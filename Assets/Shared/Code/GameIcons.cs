using FieldDay.Assets;
using UnityEngine;

namespace AIS {
    [CreateAssetMenu(menuName = "AIS/Game Icons")]
    public sealed class GameIcons : GlobalAsset {
        [Header("Time")]
        public Sprite[] TimeIcons;
        public Color32 TimeColor;

        [Header("Stats")]
        public Sprite[] StatIcons;
        public Color32[] StatColors;
    }
}