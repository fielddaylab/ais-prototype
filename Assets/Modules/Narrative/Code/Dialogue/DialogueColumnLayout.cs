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
using FieldDay.Collections;
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
        public DialogueChoiceButton DefaultNextButton;
        public DialogueChoiceButton[] Choices;

        [Header("Choices")]
        public RectTransform ChoiceGroup;
        public LayoutOptions ChoiceLayout;

        [Header("Dialogue Column")]
        public float LineBaseOffset = 0;
        public LayoutOptions VerticalLayout;
        public float CullDistance = 400;

        public RingBuffer<DialogueColumnLayoutElement> ActiveLines = new RingBuffer<DialogueColumnLayoutElement>(32);

        public void RecomputePositioning() {
            using(var lineBuffer = TempReferenceBuffer<RectTransform>.Create(ActiveLines.Count)) {
                for(int i = ActiveLines.Count; i-- > 0;) {
                    lineBuffer.Add(ActiveLines[i].RectTransform);
                }
                Positioning.VerticalLayout(lineBuffer, VerticalLayout, LineBaseOffset);
            }

            CullOffscreenElements();
        }

        public void CullOffscreenElements() {
            while(ActiveLines.TryPeekFront(out DialogueColumnLayoutElement elem)) {
                float y = elem.RectTransform.anchoredPosition.y;
                y += elem.RectTransform.rect.y;
                if (y >= LineBaseOffset + CullDistance) {
                    Pool.TryFree(elem);
                    ActiveLines.PopFront();
                } else {
                    break;
                }
            }
        }
    }
}