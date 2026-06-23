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

        [Header("Horizontal Shift")]
        public RectTransform ColumnRoot;
        public float ColumnShift = 250;
        public TweenSettings ShiftAnim = new TweenSettings(0.2f, Curve.Smooth);

        public RingBuffer<DialogueColumnLayoutElement> ActiveLines = new RingBuffer<DialogueColumnLayoutElement>(32);

        [NonSerialized] public DialogueColumnAlignment CurrentAlignment;
        [NonSerialized] private Routine m_ShiftRoutine;

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

        #region Horizontal Shift

        /// <summary>
        /// Slides the entire dialogue column to the given horizontal alignment.
        /// </summary>
        public IEnumerator ShiftTo(DialogueColumnAlignment alignment) {
            if (alignment == CurrentAlignment) {
                yield break;
            }

            Assert.NotNull(ColumnRoot, "[DialogueColumnLayout] ColumnRoot is not assigned - cannot shift dialogue layout");

            CurrentAlignment = alignment;

            m_ShiftRoutine.Replace(this, ColumnRoot.AnchorPosTo(GetAlignmentX(alignment), ShiftAnim, Axis.X));
            yield return m_ShiftRoutine.Wait();
        }

        /// <summary>
        /// Immediately applies the given horizontal alignment, with no animation.
        /// </summary>
        public void SnapTo(DialogueColumnAlignment alignment) {
            if (alignment == CurrentAlignment) {
                return;
            }

            Assert.NotNull(ColumnRoot, "[DialogueColumnLayout] ColumnRoot is not assigned - cannot snap dialogue layout");

            m_ShiftRoutine.Stop();
            CurrentAlignment = alignment;
            ColumnRoot.SetAnchorPos(GetAlignmentX(alignment), Axis.X);
        }

        /// <summary>
        /// Snaps the dialogue column back to center, with no animation.
        /// </summary>
        public void ResetAlignment() {
            SnapTo(DialogueColumnAlignment.Center);
        }

        private float GetAlignmentX(DialogueColumnAlignment alignment) {
            switch (alignment) {
                case DialogueColumnAlignment.Left: {
                    return -ColumnShift;
                }
                case DialogueColumnAlignment.Right: {
                    return ColumnShift;
                }
                default: {
                    return 0;
                }
            }
        }

        #endregion // Horizontal Shift
    }

    public enum DialogueColumnAlignment {
        Center,
        Left,
        Right
    }
}