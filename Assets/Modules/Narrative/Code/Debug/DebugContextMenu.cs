using System.Collections.Generic;
using AIS.Intervene;
using AIS.Model;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Debugging;
using FieldDay.UI;

namespace AIS.Narrative {
    /// <summary>
    /// Dev-only debug menu for handing the player any evidence chip or action card the chapter
    /// has loaded, one button per item, plus toggles for the toolbar buttons those items unlock.
    /// Chips come from the loaded EvidenceCard named assets, action cards from the parsed card
    /// definitions in ActionCardsState, so new content shows up with no code changes.
    ///
    /// Granting goes straight through the inventory (same data path as ScriptHooks) rather than
    /// the leaf hooks, since those are coroutines that need a live ScriptThread and printer.
    /// </summary>
    static public class DebugContextMenu {
        // Held as fields so the parent's OnEnter can refill them. The menu tree is built once at
        // boot, long before any chapter content is loaded, so the item lists have to be deferred.
        static private readonly DMInfo s_EvidenceMenu = new DMInfo("Evidence Chips", 24);
        static private readonly DMInfo s_ActionMenu = new DMInfo("Action Cards", 24);

        // Scratch lists reused across rebuilds.
        static private readonly List<EvidenceCard> s_EvidenceScratch = new List<EvidenceCard>(24);
        static private readonly List<KeyValuePair<StringHash32, string>> s_ActionScratch = new List<KeyValuePair<StringHash32, string>>(24);

        #region Menu factory

        [DebugMenuFactory]
        static private DMInfo ContextMenu() {
            DMInfo menu = new DMInfo("Context", 4);

            menu.AddText("Chips", GetEvidenceCountText);
            menu.AddText("Cards", GetActionCountText);
            menu.AddDivider();
            menu.AddSubmenu(s_EvidenceMenu);
            menu.AddSubmenu(s_ActionMenu);
            menu.AddDivider();

            // Toolbar buttons normally unlock through narrative beats, so these force either state
            // without replaying the thread that grants them. Model has a mask bit but no button --
            // ToolbarPanel.ModelButton / ModelMissing are commented out -- so it has no toggle.
            menu.AddToggle("Field Notes Button", IsEvidenceVisible, SetEvidenceVisible, HasToolbar);
            menu.AddToggle("Map Button", IsMapVisible, SetMapVisible, HasToolbar);
            menu.AddToggle("Time Counter", IsTimeVisible, SetTimeVisible, HasToolbar);

            // Refill on entering the parent: DMMenuUI populates a submenu's UI when it is pushed,
            // which is always after this fires, so the buttons are current whenever they're shown.
            menu.OnEnter.Register(RebuildItemMenus);
            return menu;
        }

        static private void RebuildItemMenus() {
            s_EvidenceMenu.Clear();
            s_ActionMenu.Clear();

            BuildEvidenceMenu();
            BuildActionMenu();
        }

        static private void BuildEvidenceMenu() {
            s_EvidenceScratch.Clear();
            foreach (EvidenceCard card in Game.Assets.GetAllNamed<EvidenceCard>()) {
                s_EvidenceScratch.Add(card);
            }

            if (s_EvidenceScratch.Count == 0) {
                s_EvidenceMenu.AddText("(no evidence cards loaded)");
                return;
            }

            s_EvidenceMenu.AddButton("Unlock All Evidence", GiveAllEvidenceChips, () => Game.SharedState.TryGet(out PlayerInventory _));
            s_EvidenceMenu.AddDivider();

            s_EvidenceScratch.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            foreach (EvidenceCard card in s_EvidenceScratch) {
                StringHash32 id = card.AssetId;
                s_EvidenceMenu.AddButton(card.name, () => GiveEvidenceChip(id), () => !HasEvidenceChip(id));
            }
        }

        static private void BuildActionMenu() {
            s_ActionScratch.Clear();
            if (Game.SharedState.TryGet(out ActionCardsState cardsState)) {
                foreach (KeyValuePair<StringHash32, ActionCardData> kvp in cardsState.AllActionCards) {
                    // Definitions are keyed by hash; Source() recovers the id the leaf scripts use.
                    string source = kvp.Value.CardID.Source();
                    s_ActionScratch.Add(new KeyValuePair<StringHash32, string>(
                        kvp.Key, string.IsNullOrEmpty(source) ? kvp.Key.ToString() : source
                    ));
                }
            }

            if (s_ActionScratch.Count == 0) {
                s_ActionMenu.AddText("(no action cards loaded)");
                return;
            }

            s_ActionMenu.AddButton("Unlock All Cards", GiveAllActionCards, () => Game.SharedState.TryGet(out PlayerInventory _));
            s_ActionMenu.AddDivider();

            s_ActionScratch.Sort((a, b) => string.CompareOrdinal(a.Value, b.Value));

            foreach (KeyValuePair<StringHash32, string> entry in s_ActionScratch) {
                StringHash32 id = entry.Key;
                s_ActionMenu.AddButton(entry.Value, () => GiveActionCard(id), () => !HasActionCard(id));
            }
        }

        #endregion // Menu factory

        #region Actions

        // Mirrors ScriptHooks.ScriptGiveEvidence minus the dialogue-column presentation.
        static private void GiveEvidenceChip(StringHash32 id) {
            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return; }

            if (inv.EvidenceChips.Add(id)) {
                InvasionModel.Instance?.SimDetailRegistry?.TryEnqueueReveal(id);
            }
        }

        // Mirrors ScriptHooks.ScriptGiveAction minus the dialogue-column presentation.
        static private void GiveActionCard(StringHash32 id) {
            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return; }

            inv.ActionCards.Add(id);
        }

        // Bulk version of the per-chip buttons: hands over every loaded evidence card at once.
        static private void GiveAllEvidenceChips() {
            foreach (EvidenceCard card in Game.Assets.GetAllNamed<EvidenceCard>()) {
                GiveEvidenceChip(card.AssetId);
            }
        }

        // Bulk version of the per-card buttons: hands over every parsed action card at once.
        static private void GiveAllActionCards() {
            if (!Game.SharedState.TryGet(out ActionCardsState cardsState)) { return; }

            foreach (StringHash32 id in cardsState.AllActionCards.Keys) {
                GiveActionCard(id);
            }
        }

        #endregion // Actions

        #region Toolbar visibility

        // ResetToolbarButton is the instant form of the unlock animation ScriptHooks plays, and it
        // restores the "missing" placeholder graphic when hiding, matching the pre-unlock look.
        static private void SetEvidenceVisible(bool visible) {
            if (!TryGetToolbar(out ToolbarPanel toolbar)) { return; }

            ToolbarPanel.ResetToolbarButton(toolbar.EvidenceMissing, toolbar.EvidenceButton, visible);
            SetToolbarMask(PlayerToolbarMask.Evidence, visible);
        }

        static private void SetMapVisible(bool visible) {
            if (!TryGetToolbar(out ToolbarPanel toolbar)) { return; }

            ToolbarPanel.ResetToolbarButton(toolbar.MapMissing, toolbar.MapButton, visible);
            SetToolbarMask(PlayerToolbarMask.Map, visible);
        }

        // The time counter is a plain CanvasGroup rather than a ToolbarButton, and its mask bit is
        // never read, so there is nothing to keep in sync here.
        static private void SetTimeVisible(bool visible) {
            if (!TryGetToolbar(out ToolbarPanel toolbar)) { return; }

            ToolbarPanel.ResetTimeGroup(toolbar.TimeGroup, visible);
        }

        // Keeping the inventory mask in sync matters: ScriptHooks skips the unlock animation when
        // the bit is already set, so showing a button without setting it would let a later beat
        // replay the unlock, and hiding one without clearing it would leave it hidden for good.
        static private void SetToolbarMask(PlayerToolbarMask mask, bool visible) {
            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return; }

            if (visible) {
                inv.ToolbarItems |= mask;
            } else {
                inv.ToolbarItems &= ~mask;
            }
        }

        static private bool IsEvidenceVisible() {
            return TryGetToolbar(out ToolbarPanel toolbar) && IsButtonVisible(toolbar.EvidenceButton);
        }

        static private bool IsMapVisible() {
            return TryGetToolbar(out ToolbarPanel toolbar) && IsButtonVisible(toolbar.MapButton);
        }

        static private bool IsTimeVisible() {
            return TryGetToolbar(out ToolbarPanel toolbar) && toolbar.TimeGroup.gameObject.activeSelf;
        }

        // Reads the on-screen state rather than the inventory mask, since DisableMapButton hides
        // the map button without clearing its bit.
        static private bool IsButtonVisible(ToolbarButton button) {
            return button.gameObject.activeSelf && button.Fader.alpha > 0;
        }

        // Toggle getters are evaluated every frame the menu is open, independent of the predicate,
        // so this has to tolerate being called outside a scene that has a toolbar.
        static private bool TryGetToolbar(out ToolbarPanel toolbar) {
            GuiMgr gui = Game.Gui;
            if (gui == null) {
                toolbar = null;
                return false;
            }
            return gui.TryGetModule(out toolbar);
        }

        static private bool HasToolbar() {
            return TryGetToolbar(out ToolbarPanel _);
        }

        #endregion // Toolbar visibility

        #region Predicates / readout

        // Buttons stay enabled until the player owns the item, so the menu doubles as a readout
        // of what has already been granted.
        static private bool HasEvidenceChip(StringHash32 id) {
            return Game.SharedState.TryGet(out PlayerInventory inv) && inv.EvidenceChips.Contains(id);
        }

        static private bool HasActionCard(StringHash32 id) {
            return Game.SharedState.TryGet(out PlayerInventory inv) && inv.ActionCards.Contains(id);
        }

        static private string GetEvidenceCountText() {
            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return "- (not in a play scene)"; }
            return inv.EvidenceChips.Count.ToString();
        }

        static private string GetActionCountText() {
            if (!Game.SharedState.TryGet(out PlayerInventory inv)) { return "- (not in a play scene)"; }
            return inv.ActionCards.Count.ToString();
        }

        #endregion // Predicates / readout
    }
}
