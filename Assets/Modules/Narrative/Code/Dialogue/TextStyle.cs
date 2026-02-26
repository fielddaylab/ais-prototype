using BeauUtil;
using FieldDay;
using FieldDay.Assets;
using TMPro;
using UnityEngine;

namespace AIS.Narrative {
    [CreateAssetMenu(menuName = "Narrative/Text Style")]
    public sealed class TextStyle : NamedAsset {
        [Header("Text Display")]
        public TMP_FontAsset CustomFont;
        public ColorPalette2 Colors = new ColorPalette2(Color.white, Color.black);
        public float RoundingMultiplier = 1;
        public TextStyleTailMode TailMode;
    }

    public enum TextStyleTailMode {
        Hidden,
        Left,
        Right,
        Center
    }
}