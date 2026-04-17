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
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            int currentStat = statBlock[statId];
            int originalValue = currentStat;
            if (adjustment != 0) {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte) currentStat;
                if (thread.IsSkipping()) {
                    return null;
                }

                DialogueColumn column = (DialogueColumn) thread.GetPrinter();
                if (!column) {
                    return null;
                }

                return TextUtility.DisplayStatUpdate(column, statId, originalValue, currentStat);
            }

            return null;
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
                    return null;
                }

                DialogueColumn column = (DialogueColumn) thread.GetPrinter();
                if (!column) {
                    return null;
                }

                return TextUtility.DisplayNewEvidence(column, id);
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