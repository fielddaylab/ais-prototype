using FieldDay.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [CreateAssetMenu(menuName ="Narrative/Evidence Card")]
    public sealed class EvidenceCard : NamedAsset {
        public PlayerStatId Suit;
        public Image Illustration;
        [Multiline] public string Label;
        public bool isActionable;
    }
}