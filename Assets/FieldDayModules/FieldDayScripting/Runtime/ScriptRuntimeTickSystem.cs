using BeauRoutine;
using BeauUtil.Debugger;
using FieldDay.Systems;

namespace FieldDay.Scripting {
    [SysUpdate(GameLoopPhase.LateUpdate, 10000, ScriptUtility.RuntimeUpdateMask, AllowExecutionDuringLoad = true)]
    internal sealed class ScriptRuntimeTickSystem : ISystem {
        public void ProcessWork(float deltaTime) {
            if (ScriptUtility.Runtime.PauseDepth != 0) {
                return;
            }

            if (ScriptUtility.Runtime.ActiveThreads.Count > 0) {
                //using (Profiling.Time("Leaf Update", ProfileTimeUnits.Microseconds)) {
                    Routine.ManualUpdate(deltaTime);
                //}
            }

            ScriptUtility.Runtime.SignalMap.Flush();
            // TODO: process queue?
        }
    }
}