using System.Collections.Generic;
using BeauUtil;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.SharedState;
using Leaf;

namespace AIS.Narrative {
    /// <summary>
    /// Game-owned record of visited script nodes. Persists for the whole play session;
    /// reset only on new game.
    /// </summary>
    public sealed class ScriptMemory : ISharedState, IRegistrationCallbacks {
        public readonly HashSet<StringHash32> VisitedNodeIds = SetUtils.Create<StringHash32>(256);

        void IRegistrationCallbacks.OnRegister() {
            ScriptUtility.Runtime.OnTaggedLineProcessed.Register(OnLineProcessed);
            ScriptUtility.Runtime.OnLeafChoiceChosen.Register(OnChoiceChosen);
        }

        void IRegistrationCallbacks.OnDeregister() {
            ScriptUtility.Runtime.OnTaggedLineProcessed.Deregister(OnLineProcessed);
            ScriptUtility.Runtime.OnLeafChoiceChosen.Deregister(OnChoiceChosen);
        }

        // approximates node-enter recording: fires for every processed line, including skipped ones;
        // only blind spot is a node with no text lines entered solely via $goto
        private void OnLineProcessed(ScriptThread thread, TagString line) {
            ScriptNode node = thread.PeekNode();
            if (node != null) {
                VisitedNodeIds.Add(node.Id());
            }
        }

        // covers line-less nodes targeted directly by a choice
        private void OnChoiceChosen(ScriptThread thread, LeafChoice choice) {
            VisitedNodeIds.Add(choice.ChosenTarget().AsStringHash());
        }
    }
}
