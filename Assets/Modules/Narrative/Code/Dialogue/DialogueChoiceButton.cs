using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent, RequireComponent(typeof(DialogueLine))]
    public class DialogueChoiceButton: BatchedComponent {
        [Serializable] public sealed class Pool : SerializablePool<DialogueChoiceButton> { }

        public DialogueLine Content;
        public PointerListener Listener;

        public TMP_Text StatRequirement;
        public LayoutSizeGroup StatGroup;

        public Image TimeRequirement;
        public GameObject TimeGroup;

        public Image SuitIcon;
        public GameObject SuitGroup;

        [NonSerialized] public bool Clicked;

        private void Awake() {
            Listener.onClick.Register(() => Clicked = true);
        }

        public bool ConsumeClick() {
            if (Clicked) {
                Clicked = false;
                return true;
            }

            return false;
        }
    }

    public struct DialogueChoiceRequirements {
        public const int MaxTimeConsumed = 4;

        public PlayerStatId StatId;
        public int StatThreshold;
        public int TimeConsumed;
        public bool Once;
        public bool OpenMap;
        public PlayerStatId Suit;

        static public DialogueChoiceRequirements Read(LeafChoice choice, int choiceIndex) {
            DialogueChoiceRequirements requirements = default;
            requirements.StatId = PlayerStatId.Invalid;
            requirements.Suit = PlayerStatId.Invalid;

            if (choice.TryGetCustomData(choiceIndex, "CheckStat", out var checkStatId)) {
                requirements.StatId = ParseStatId(checkStatId.AsStringHash());
            }

            // Purely cosmetic: tags the choice with a suit icon (see the TimeChoice macro).
            if (choice.TryGetCustomData(choiceIndex, "Suit", out var suitId)) {
                requirements.Suit = ParseStatId(suitId.AsStringHash());
            }

            choice.TryGetCustomData(choiceIndex, "CheckStatValue", out var checkStatValue);
            requirements.StatThreshold = checkStatValue.AsInt();

            choice.TryGetCustomData(choiceIndex, "Time", out var timeValue);
            requirements.TimeConsumed = (int) Math.Min(timeValue.AsUInt(), MaxTimeConsumed);

            requirements.Once = choice.HasCustomData(choiceIndex, "Once");
            requirements.OpenMap = choice.HasCustomData(choiceIndex, "OpenMap");

            return requirements;
        }

        /// <summary>
        /// Maps a suit/stat name as written in leaf ("Ranger", "ranger", ...) to its id.
        /// Returns Invalid for an omitted or unrecognized name.
        /// </summary>
        static public PlayerStatId ParseStatId(StringHash32 statIdHash) {
            if (statIdHash == "Tech" || statIdHash == "tech") {
                return PlayerStatId.Tech;
            }
            if (statIdHash == "Research" || statIdHash == "research") {
                return PlayerStatId.Research;
            }
            if (statIdHash == "Innovate" || statIdHash == "Innovator" || statIdHash == "innovate") {
                return PlayerStatId.Innovate;
            }
            if (statIdHash == "Ranger" || statIdHash == "ranger") {
                return PlayerStatId.Ranger;
            }
            if (statIdHash == "Communicate" || statIdHash == "communicate") {
                return PlayerStatId.Communicate;
            }
            return PlayerStatId.Invalid;
        }
    }

}