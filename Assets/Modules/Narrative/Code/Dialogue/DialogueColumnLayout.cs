using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scripting;
using FieldDay.UI;
using FieldDay.UI.Animation;
using Leaf.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent]
    public sealed class DialogueColumnLayout : MonoBehaviour {
        public DialogueLine.Pool LinePool;

        public DialogueChoiceButton DefaultNextButton;
        public DialogueChoiceButton[] Choices;

        public float LineBaseOffset = 0;
        public float LineSpacing = 300;
        public int MaxLines = 3;

        public RingBuffer<DialogueLine> ActiveLines = new RingBuffer<DialogueLine>(8);

        public void RecomputePositioning() {
            while(ActiveLines.Count > MaxLines) {
                LinePool.Free(ActiveLines.PopFront());
            }

            int totalLineCount = ActiveLines.Count;
            for(int i = 0; i < totalLineCount; i++) {
                RectTransform position = (RectTransform) ActiveLines[i].transform;
                int index = totalLineCount - i;
                position.anchoredPosition = new Vector2(0, LineBaseOffset + LineSpacing * index);
            }
        }
    }
}