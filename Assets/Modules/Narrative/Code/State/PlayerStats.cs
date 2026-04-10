using System;
using System.Runtime.CompilerServices;
using FieldDay;
using FieldDay.SharedState;

namespace AIS.Narrative {
    public sealed class PlayerStats : ISharedState {
        public PlayerStatBlock StatBlock = new PlayerStatBlock() {
            Communicate = 2,
            Research = 2,
            Innovate = 2,
            Ranger = 2,
            Tech = 2
        };
    }

    public enum PlayerStatId : byte {
        Tech,
        Research,
        Innovate,
        Ranger,
        Communicate,
        Invalid = 255
    }

    [Serializable]
    public struct PlayerStatBlock : IEquatable<PlayerStatBlock> {
        public const int MinValue = 0;
        public const int MaxValue = 5;

        public sbyte Tech;
        public sbyte Research;
        public sbyte Innovate;
        public sbyte Ranger;
        public sbyte Communicate;

        public unsafe sbyte this[PlayerStatId id] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                fixed(sbyte* ptr = &Tech) {
                    return ptr[(int) id];
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                fixed (sbyte* ptr = &Tech) {
                    ptr[(int) id] = value;
                }
            }
        }

        static public void Combine(in PlayerStatBlock a, in PlayerStatBlock b, out PlayerStatBlock result) {
            result.Tech = (sbyte) (a.Tech + b.Tech);
            result.Research = (sbyte) (a.Research + b.Research);
            result.Innovate = (sbyte) (a.Innovate + b.Innovate);
            result.Ranger = (sbyte) (a.Ranger + b.Ranger);
            result.Communicate = (sbyte) (a.Communicate + b.Communicate);
        }

        static public void Clamp(ref PlayerStatBlock block) {
            block.Tech = (sbyte) Math.Clamp((int) block.Tech, MinValue, MaxValue);
            block.Research = (sbyte) Math.Clamp((int) block.Research, MinValue, MaxValue);
            block.Innovate = (sbyte) Math.Clamp((int) block.Innovate, MinValue, MaxValue);
            block.Ranger = (sbyte) Math.Clamp((int) block.Ranger, MinValue, MaxValue);
            block.Communicate = (sbyte) Math.Clamp((int) block.Communicate, MinValue, MaxValue);
        }

        static public int Clamp(int statValue) {
            return Math.Clamp(statValue, MinValue, MaxValue);
        }

        public bool Equals(PlayerStatBlock other) {
            return Tech == other.Tech
                & Research == other.Research
                & Innovate == other.Innovate
                & Ranger == other.Ranger
                & Communicate == other.Communicate;
        }
    }
}