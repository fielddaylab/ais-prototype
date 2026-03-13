#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD

#if DEVELOPMENT
#define ECS_VALIDATE_SYSTEM_PERMISSIONS
#endif // DEVELOPMENT

using System;
using System.Runtime.CompilerServices;
using BeauUtil;
using FieldDay.Components;
using FieldDay.SharedState;
using UnityEngine.Scripting;

namespace FieldDay.Systems {
    /// <summary>
    /// System function pointer.
    /// </summary>
    public delegate void SystemFunction(float deltaTime);

    /// <summary>
    /// Attribute defining system update order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false), Preserve]
    public sealed class SysUpdateAttribute : PreserveAttribute {
        public readonly GameLoopPhaseMask PhaseMask;
        public readonly int Order;
        public readonly int CategoryMask;
        public bool AllowExecutionDuringLoad;

        public SysUpdateAttribute(GameLoopPhase phase, int order = 0, int updateMask = Bits.All32) {
            PhaseMask = (GameLoopPhaseMask) (1 << (int) phase);
            Order = order;
            CategoryMask = updateMask;
        }

        public SysUpdateAttribute(GameLoopPhaseMask phaseMask, int order = 0, int updateMask = Bits.All32) {
            PhaseMask = phaseMask;
            Order = order;
            CategoryMask = updateMask;
        }
    }

    /// <summary>
    /// System execution flags.
    /// </summary>
    [Flags]
    public enum SysFlags : uint {
        DuringLoading = 0x01,
    }

    /// <summary>
    /// System update information.
    /// </summary>
    public readonly struct SysUpdate {
        public readonly GameLoopPhaseMask PhaseMask;
        public readonly int Order;
        public readonly int CategoryMask;
        public readonly SysFlags Flags;
    }
}