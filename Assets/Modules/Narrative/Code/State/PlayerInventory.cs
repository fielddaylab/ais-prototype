using System.Collections.Generic;
using BeauUtil;
using BeauUtil.Variants;
using FieldDay.SharedState;

namespace AIS.Narrative {
    public sealed class PlayerInventory : ISharedState {
        public readonly HashSet<StringHash32> EvidenceCards;
        public readonly VariantTable GlobalVars;
        public readonly VariantTable LevelVars;

        public PlayerInventory() {
            EvidenceCards = SetUtils.Create<StringHash32>(24);
            GlobalVars = new VariantTable("global", 32);
            LevelVars = new VariantTable("level", 32);
        }
    }
}