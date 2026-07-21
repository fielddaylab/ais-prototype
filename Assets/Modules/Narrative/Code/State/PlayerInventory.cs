using System;
using System.Collections.Generic;
using BeauUtil;
using BeauUtil.Variants;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.SharedState;

namespace AIS.Narrative {
    public sealed class PlayerInventory : ISharedState, IRegistrationCallbacks {
        public readonly HashSet<StringHash32> EvidenceChips;
        public readonly HashSet<StringHash32> ActionCards;
        public readonly HashSet<StringHash32> Scenarios;
        public readonly VariantTable GlobalVars;
        public readonly VariantTable LevelVars;
        public int TimeRemaining;
        public PlayerToolbarMask ToolbarItems;

        public PlayerInventory() {
            EvidenceChips = SetUtils.Create<StringHash32>(24);
            ActionCards = SetUtils.Create<StringHash32>(24);
            Scenarios = SetUtils.Create<StringHash32>(24);
            GlobalVars = new VariantTable("global", 32);
            LevelVars = new VariantTable("level", 32);
            TimeRemaining = 20;
            ToolbarItems = 0;
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

    [Flags]
    public enum PlayerToolbarMask : uint {
        Evidence = 0x01,
        Map = 0x02,
        Model = 0x04,
        Time = 0x08,

        All = Evidence | Map | Model | Time
    }

    static public partial class PlayerUtility {
        static public void DecreaseTime(int chunks) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            if (chunks > 0) {
                inv.TimeRemaining = Math.Max(0, inv.TimeRemaining - chunks);
                Find.GuiModule<ToolbarPanel>().TimeCounter.SetValue(inv.TimeRemaining);
            }
        }

        static public void SetTime(int chunks) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            inv.TimeRemaining = chunks;
            Find.GuiModule<ToolbarPanel>().TimeCounter.SetValue(inv.TimeRemaining);
            ToolbarPanel.ResetTimeGroup(Find.GuiModule<ToolbarPanel>().TimeGroup, true);
        }
    }
}