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

        [LeafMember("AdjustStat")]
        static public int AdjustStat(PlayerStatId statId, int adjustment) {
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            int currentStat = statBlock[statId];
            if (adjustment != 0) {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte) currentStat;
            }
            return currentStat;
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
                if (!column) {
                    yield break;
                }

                EvidenceCard data = Find.NamedAsset<EvidenceCard>(id);
                NewCardElement newElem = column.NewCardPool.Alloc();
                column.Layout.ActiveLines.PushBack(newElem.Positioner);
                newElem.Widget.Content.SetText(data.Label);
                newElem.Layout.VerticalLayout(LayoutOptions.PreferredSize(4, 1));
                newElem.SetVisible(true);
                column.Layout.RecomputePositioning();

                yield return 0.1f;
                yield return column.CompleteLine();
            }
        }

        [LeafMember("HasEvidenceCard")]
        static public bool HasEvidenceCard(StringHash32 id) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceCards.Contains(id);
        }
    }
}