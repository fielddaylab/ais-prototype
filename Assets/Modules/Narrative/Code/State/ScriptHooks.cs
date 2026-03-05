using BeauUtil;
using FieldDay;
using Leaf.Runtime;

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
        static public bool GiveEvidence(StringHash32 id) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceCards.Add(id);
        }

        [LeafMember("HasEvidenceCard")]
        static public bool HasEvidenceCard(StringHash32 id) {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceCards.Contains(id);
        }
    }
}