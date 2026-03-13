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
    /// System permission information.
    /// </summary>
    public struct SysPermissions {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
        internal BitSet512 ReadComponentMask;
        internal BitSet512 WriteComponentMask;
        internal BitSet512 ReadSharedMask;
        internal BitSet512 WriteSharedMask;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS

        #region Components

        /// <summary>
        /// The system will read data from the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions Read<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadComponentMask.Set(ComponentIndex.Get<TComponent>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will write data to the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions Write<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            WriteComponentMask.Set(ComponentIndex.Get<TComponent>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will read and write data to the given component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadWrite<TComponent>() where TComponent : UnityEngine.Object, IComponentData {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            int index = ComponentIndex.Get<TComponent>();
            ReadComponentMask.Set(index);
            WriteComponentMask.Set(index);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        #endregion // Components

        #region SharedState

        /// <summary>
        /// The system will read data from the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            ReadSharedMask.Set(SharedStateIndex.Get<TShared>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will write data to the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions WriteShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            WriteSharedMask.Set(SharedStateIndex.Get<TShared>());
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        /// <summary>
        /// The system will read and write data to the given SharedState.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SysPermissions ReadWriteShared<TShared>() where TShared : ISharedState {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            int index = SharedStateIndex.Get<TShared>();
            ReadSharedMask.Set(index);
            WriteSharedMask.Set(index);
            return this;
#else
            return default;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }

        #endregion // SharedState

        /// <summary>
        /// Checks if the given permission set overlaps.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckForConflicts(in SysPermissions other) {
#if ECS_VALIDATE_SYSTEM_PERMISSIONS
            return (ReadComponentMask & other.WriteComponentMask)
                | (WriteComponentMask & (other.ReadComponentMask | other.WriteComponentMask))
                | (ReadSharedMask & other.WriteSharedMask)
                | (WriteSharedMask & (other.ReadSharedMask | other.WriteComponentMask));
#else
            return false;
#endif // ECS_VALIDATE_SYSTEM_PERMISSIONS
        }
    }
}