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

        static public DialogueChoiceRequirements Read(LeafChoice choice, int choiceIndex) {
            DialogueChoiceRequirements requirements = default;
            requirements.StatId = PlayerStatId.Invalid;
            
            if (choice.TryGetCustomData(choiceIndex, "CheckStat", out var checkStatId)) {
                StringHash32 statIdHash = checkStatId.AsStringHash();
                if (statIdHash == "Tech") {
                    requirements.StatId = PlayerStatId.Tech;
                } else if (statIdHash == "Communicate") {
                    requirements.StatId = PlayerStatId.Communicate;
                } else if (statIdHash == "Ranger") {
                    requirements.StatId = PlayerStatId.Ranger;
                } else if (statIdHash == "Research") {
                    requirements.StatId = PlayerStatId.Research;
                } else if (statIdHash == "Innovator") {
                    requirements.StatId = PlayerStatId.Innovate;
                }
            }

            choice.TryGetCustomData(choiceIndex, "CheckStatValue", out var checkStatValue);
            requirements.StatThreshold = checkStatValue.AsInt();

            choice.TryGetCustomData(choiceIndex, "Time", out var timeValue);
            requirements.TimeConsumed = (int) Math.Min(timeValue.AsUInt(), MaxTimeConsumed);

            requirements.Once = choice.HasCustomData(choiceIndex, "Once");

            return requirements;
        }
    }

}