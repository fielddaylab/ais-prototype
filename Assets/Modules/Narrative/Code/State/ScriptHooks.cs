using BeauUtil;
using FieldDay;
using Leaf.Runtime;

namespace AIS.Narrative {
    static public class ScriptHooks {
        [LeafMember("Stat")]
        static public int GetStat(PlayerStatId statId) {
            return Find.State<PlayerStats>().StatBlock[statId];
        }

        [LeafMember("BeginIntervention")]
        static public void LoadIntoInterventionScene() {
            Game.Scenes.LoadMainScene(SceneReference.FromName("Intervene"));
        }
    }
}