using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Scripting;
using Leaf;

namespace AIS.Narrative {
    /// <summary>
    /// Filtering and fallback resolution for dialogue choices.
    /// </summary>
    static public class DialogueChoiceUtility {
        public const string OutOfTimeNodeName = "Fallback.OutOfTime";
        public const string OutOfOptionsNodeName = "Fallback.OutOfOptions";

        static public readonly LeafChoice.OptionPredicate SelectablePredicate = IsSelectable;

        /// <summary>
        /// Returns if the option should be displayed at all. Hidden when its script
        /// condition failed, or when it is marked "Once" and its target has been visited.
        /// (Time/stat requirement failures gray the option out instead - see IsSelectable.)
        /// </summary>
        static public bool IsVisible(LeafChoice choice, LeafChoice.Option option) {
            if (!option.IsAvailable) {
                return false;
            }

            if (!choice.HasCustomData(option.Index, "Once")) {
                return true;
            }

            return !Find.State<ScriptMemory>().VisitedNodeIds.Contains(option.TargetId.AsStringHash());
        }

        /// <summary>
        /// Returns if the option is visible and the player meets its time and stat requirements.
        /// </summary>
        static public bool IsSelectable(LeafChoice choice, LeafChoice.Option option) {
            if (!IsVisible(choice, option)) {
                return false;
            }

            DialogueChoiceRequirements req = DialogueChoiceRequirements.Read(choice, option.Index);
            if (req.TimeConsumed > 0 && Find.State<PlayerInventory>().TimeRemaining < req.TimeConsumed) {
                return false;
            }
            if (req.StatId != PlayerStatId.Invalid && req.StatThreshold > PlayerStatBlock.MinValue
                && Find.State<PlayerStats>().StatBlock[req.StatId] < req.StatThreshold) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resolves the id of the fallback node to redirect to when no choices are selectable.
        /// Prefers a node within the current node's package (e.g. "MyChapter.Fallback.OutOfTime")
        /// over a globally exposed one. Returns an empty hash if neither exists.
        /// </summary>
        static public StringHash32 ResolveFallbackNode(ScriptNode currentNode, bool outOfTime) {
            string fallbackName = outOfTime ? OutOfTimeNodeName : OutOfOptionsNodeName;

            if (currentNode != null) {
                string rootPath = currentNode.Package().RootPath();
                if (!string.IsNullOrEmpty(rootPath)) {
                    StringHash32 localId = new StringHash32(rootPath + "." + fallbackName);
                    if (ScriptDBUtility.TryLookupNode(ScriptUtility.DB, currentNode, localId, out ScriptNode _)) {
                        return localId;
                    }
                }
            }

            StringHash32 globalId = new StringHash32(fallbackName);
            if (ScriptDBUtility.TryLookupNode(ScriptUtility.DB, currentNode, globalId, out ScriptNode _)) {
                return globalId;
            }

            return default;
        }

        /// <summary>
        /// Resolves an offered choice to the given node without presenting any options.
        /// </summary>
        static public void Redirect(LeafChoice choice, StringHash32 nodeId) {
            choice.Reset();
            choice.AddOption(new LeafChoice.Option(nodeId, default(StringHash32)));
            choice.Offer();
            choice.Choose(0);
        }

        /// <summary>
        /// Redirects a running thread to its resolved fallback node (package-local override
        /// preferred, else the global one). Returns false if no fallback node exists.
        /// </summary>
        static public bool GotoFallback(ScriptThread thread, bool outOfTime) {
            ScriptNode currentNode = thread.PeekNode();
            StringHash32 fallbackId = ResolveFallbackNode(currentNode, outOfTime);
            if (fallbackId.IsEmpty) {
                Log.Error("[DialogueChoiceUtility] No fallback node found from '{0}'", currentNode?.Id());
                return false;
            }
            if (ScriptDBUtility.TryLookupNode(ScriptUtility.DB, currentNode, fallbackId, out ScriptNode fallbackNode)) {
                thread.GotoNode(fallbackNode);
                return true;
            }
            return false;
        }
    }
}
