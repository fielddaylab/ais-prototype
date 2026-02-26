using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauRoutine;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scripting;
using FieldDay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent, RequireComponent(typeof(DialogueLine))]
    public class DialogueChoiceButton: BatchedComponent {
        [Serializable] public sealed class Pool : SerializablePool<DialogueChoiceButton> { }

        public DialogueLine Content;
        public PointerListener Listener;

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

}