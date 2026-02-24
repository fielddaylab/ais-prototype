using FieldDay;
using Leaf.Runtime;

namespace AIS.Narrative {
    static public class ScriptHooks {
        [LeafMember("GetStat")]
        static public int GetStat(PlayerStatId statId) {
            return Find.State<PlayerStats>().StatBlock[statId];
        }
    }
}