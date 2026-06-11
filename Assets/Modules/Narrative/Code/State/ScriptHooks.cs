using BeauUtil;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf.Runtime;
using System.Collections;

namespace AIS.Narrative {
    static public class ScriptHooks {
        [LeafMember("Stat")]
        static public int GetStat(PlayerStatId statId) {
            return Find.State<PlayerStats>().StatBlock[statId];
        }

        [LeafMember("SetStat")]
        static public void SetStat(PlayerStatId statId, int value) {
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            statBlock[statId] = (sbyte) PlayerStatBlock.Clamp(value);
        }

        [LeafMember("SilentAdjustStat")]
        static public void SilentAdjustStat(PlayerStatId statId, int adjustment) {
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            int currentStat = statBlock[statId];
            if (adjustment != 0) {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte) currentStat;
            }
        }

        [LeafMember("AdjustStat")]
        static public IEnumerator AdjustStat([BindThread] ScriptThread thread, PlayerStatId statId, int adjustment) {
            PlayerStats stats = Find.State<PlayerStats>();
            PlayerInventory inv = Find.State<PlayerInventory>();
            PlayerStatBlock statBlock = stats.StatBlock;
            int currentStat = statBlock[statId];
            int originalValue = currentStat;
            if (adjustment != 0) {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte) currentStat;
                stats.StatBlock = statBlock;

                if (thread.IsSkipping()) {
                    yield break;
                }

                DialogueColumn column = (DialogueColumn) thread.GetPrinter();
                if (column) {
                    yield return TextUtility.DisplayStatUpdate(column, statId, originalValue, currentStat);
                    yield return EnsureStatsVisible(inv);
                    yield return column.CompleteLine();
                } else {
                    yield return EnsureStatsVisible(inv);
                }
            }
        }

        [LeafMember("BeginIntervention")]
        static public void LoadIntoInterventionScene() {
            Game.Scenes.LoadMainScene(SceneReference.FromName("Intervene"));
        }

        [LeafMember("GiveEvidenceCard")]
        static public IEnumerator ScriptGiveEvidence([BindThread] ScriptThread thread, StringHash32 id) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            if (inv.EvidenceCards.Add(id)) {
                if (thread.IsSkipping()) {
                    yield break;
                }

                DialogueColumn column = (DialogueColumn) thread.GetPrinter();
                if (column) {
                    yield return TextUtility.DisplayNewEvidence(column, id);
                    yield return EnsureEvidenceVisible(inv);
                    yield return column.CompleteLine();
                } else {
                    yield return EnsureEvidenceVisible(inv);
                }
            }
        }

        [LeafMember("EnableMapButton")]
        static public IEnumerator EnableMapButton()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            yield return EnsureMapVisible(inv);
        }

        [LeafMember("DisableMapButton")]
        static public void DisableMapButton()
        {
            var toolbar = Find.GuiModule<ToolbarPanel>();
            toolbar.MapButton.Fader.blocksRaycasts = false;
            toolbar.MapButton.Fader.alpha = 0;
        }

        [LeafMember("EnableModelButton")]
        static public IEnumerator EnableModelButton()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            yield return EnsureStatsVisible(inv);
        }

        static private IEnumerator EnsureEvidenceVisible(PlayerInventory inv) {
            if ((inv.ToolbarItems & PlayerToolbarMask.Evidence) == 0) {
                inv.ToolbarItems |= PlayerToolbarMask.Evidence;
                ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.EvidenceMissing, toolbar.EvidenceButton);
            }
            return null;
        }

        static private IEnumerator EnsureMapVisible(PlayerInventory inv) {
            if ((inv.ToolbarItems & PlayerToolbarMask.Map) == 0) {
                inv.ToolbarItems |= PlayerToolbarMask.Map;
                ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.MapMissing, toolbar.MapButton);
            }
            return null;
        }
        
        // TODO: stats are not a toolbar tab anymore
        static private IEnumerator EnsureStatsVisible(PlayerInventory inv) {
            if ((inv.ToolbarItems & PlayerToolbarMask.Model) == 0) {
                inv.ToolbarItems |= PlayerToolbarMask.Model;
                ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.ModelMissing, toolbar.ModelButton);
            }
            return null;
        }

        [LeafMember("ClearVisibleLines")]
        static public void ScriptClearVisibleLines([BindThread] ScriptThread thread) {
            DialogueColumn column = (DialogueColumn) thread.GetPrinter();
            if (!column) {
                return;
            }

            TextUtility.ClearAllLines(column.Layout);
        }

        [LeafMember("HasEvidenceCard")]
        static public bool HasEvidenceCard(StringHash32 id) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceCards.Contains(id);
        }
    }
}