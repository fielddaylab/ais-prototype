using BeauUtil;
using FieldDay;
using FieldDay.Assets;
using TMPro;
using UnityEngine;

namespace AIS.Narrative {
    [CreateAssetMenu(menuName = "Narrative/Character Data")]
    public sealed class CharacterData : NamedAsset {
        static private readonly StringHash32 DefaultId = "Default";

        public string DisplayName;
        [AssetName(typeof(TextStyle))] public StringHash32 TextStyle;

        static public CharacterData Get(StringHash32 id) {
            return Find.NamedAsset<CharacterData>(StringHash32.First(id, DefaultId));
        }
    }
}