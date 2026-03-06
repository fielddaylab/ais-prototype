using System.Collections.Generic;
using BeauUtil;
using BeauUtil.Variants;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.SharedState;

namespace AIS.Narrative {
    public sealed class PlayerInventory : ISharedState, IRegistrationCallbacks {
        public readonly HashSet<StringHash32> EvidenceCards;
        public readonly VariantTable GlobalVars;
        public readonly VariantTable LevelVars;
        public int TimeRemaining;

        public PlayerInventory() {
            EvidenceCards = SetUtils.Create<StringHash32>(24);
            GlobalVars = new VariantTable("global", 32);
            LevelVars = new VariantTable("level", 32);
            TimeRemaining = 20;
        }

        void IRegistrationCallbacks.OnRegister() {
            ScriptUtility.BindTable("global", GlobalVars);
            ScriptUtility.BindTable("level", LevelVars);
        }

        void IRegistrationCallbacks.OnDeregister() {
            ScriptUtility.UnbindTable("global");
            ScriptUtility.UnbindTable("level");
        }
    }
}