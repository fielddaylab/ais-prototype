using FieldDay.Assets;
using UnityEngine;

namespace AIS.Narrative {
    [CreateAssetMenu(menuName ="Narrative/Evidence Card")]
    public sealed class EvidenceCard : NamedAsset {
        [Multiline] public string Label;
    }
}