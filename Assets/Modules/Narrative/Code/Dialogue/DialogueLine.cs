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
using FieldDay.Scenes;
using FieldDay.Scripting;
using FieldDay.UI;
using FieldDay.UI.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent]
    public class DialogueLine : BatchedComponent, IPoolAllocHandler, IPoolConstructHandler {
        [Serializable] public sealed class Pool : SerializablePool<DialogueLine> { }

        [Header("Layout")]
        public DialogueColumnLayoutElement Positioner;
        public LayoutSizeGroup Layout;
        public LayoutStyleInfo Padding;
        public RectTransform RootPivot;
        public CanvasGroup Visibility;
        public LayoutOffset Offset;

        [Header("Content")]
        public TMP_Text Text;

        [Header("Header")]
        public TMP_Text CharacterName;
        public LayoutSizeGroup CharacterLayout;

        [Header("Tail")]
        public RectTransform Tail;
        public bool AdjustPivots;

        [Header("Styling")]
        public Graphic[] BackgroundColor;
        public Graphic[] ContentColor;
        public RoundedRectGraphic[] Rounding;

        [NonSerialized] private float[] m_RoundingBase;

        private void Awake() {
            SetVisible(false);
        }

        public void Populate(TagString tagData) {
            Text.SetText(tagData.RichText);
        }

        public void Populate(string constText) {
            Text.SetText(constText);
        }

        public void SetCharacterInfo(DialogueCharacterState charState, TextStyleTailMode? tailOverride) {
            CharacterData data = CharacterData.Get(charState.CharacterId);

            if (CharacterName) {
                if (!string.IsNullOrEmpty(charState.OverrideName)) {
                    CharacterName.SetText(charState.OverrideName);
                    CharacterLayout.gameObject.SetActive(true);
                } else if (data != null && !string.IsNullOrEmpty(data.DisplayName)) {
                    CharacterName.SetText(data.DisplayName);
                    CharacterLayout.gameObject.SetActive(true);
                } else {
                    CharacterLayout.gameObject.SetActive(false);
                }

                if (Padding) {
                    Padding.Style.MarginUpper.y = CharacterLayout.gameObject.activeSelf ? CharacterName.preferredHeight + CharacterLayout.Padding.y : 0;
                }
            }

            SetTextStyle(Find.NamedAsset<TextStyle>(data.TextStyle), tailOverride);
        }

        public void SetVisible(bool visible) {
            Visibility.alpha = visible ? 1 : 0;
            Visibility.blocksRaycasts = visible;
            if (visible) {
                PopAnim.Play(Offset, PopAnim.Default);
            }
        }

        #region Styling

        public void SetTextStyle(TextStyle style, TextStyleTailMode? tailOverride) {
            Assert.NotNullOrDestroyed(style);

            SetContentColor(style.Colors.Content);
            SetBackgroundColor(style.Colors.Background);
            SetRoundingMultiplier(style.RoundingMultiplier);
            SetTailMode(tailOverride.GetValueOrDefault(style.TailMode), AdjustPivots);
        }

        public void SetBackgroundColor(Color color) {
            for(int i = 0; i < BackgroundColor.Length; i++) {
                BackgroundColor[i].color = color;
            }
        }

        public void SetContentColor(Color color) {
            for (int i = 0; i < ContentColor.Length; i++) {
                ContentColor[i].color = color;
            }
        }

        public void SetRoundingMultiplier(float multiplier) {
            if (m_RoundingBase == null) {
                m_RoundingBase = new float[Rounding.Length];
                for(int i = 0; i < m_RoundingBase.Length; i++) {
                    m_RoundingBase[i] = Rounding[i].CornerRadius;
                }
            }

            for(int i = 0; i < Rounding.Length; i++) {
                Rounding[i].CornerRadius = m_RoundingBase[i] * multiplier;
            }
        }

        public void SetTailMode(TextStyleTailMode tailMode, bool adjustPivots) {
            if (!Tail) {
                return;
            }

            Tail.gameObject.SetActive(tailMode != TextStyleTailMode.Hidden);
            switch (tailMode) {
                case TextStyleTailMode.Center: {
                    Positioning.SetAnchorOffsetX(Tail, 0.5f, 0);
                    if (adjustPivots) {
                        Positioning.SetAnchor(RootPivot, TextAnchor.LowerCenter);
                        Positioning.SetPivot(RootPivot, TextAnchor.MiddleCenter);
                    }
                    break;
                }
                case TextStyleTailMode.Left: {
                    Positioning.SetAnchorOffsetX(Tail, 0, 32f);
                    if (adjustPivots) {
                        Positioning.SetAnchor(RootPivot, TextAnchor.LowerLeft);
                        Positioning.SetPivot(RootPivot, TextAnchor.MiddleLeft);
                    }
                    break;
                }
                case TextStyleTailMode.Right: {
                    Positioning.SetAnchorOffsetX(Tail, 1, -32f);
                    if (adjustPivots) {
                        Positioning.SetAnchor(RootPivot, TextAnchor.LowerRight);
                        Positioning.SetPivot(RootPivot, TextAnchor.MiddleRight);
                    }
                    break;
                }
                case TextStyleTailMode.Hidden: {
                    if (adjustPivots) {
                        Positioning.SetAnchor(RootPivot, TextAnchor.LowerCenter);
                        Positioning.SetPivot(RootPivot, TextAnchor.MiddleCenter);
                    }
                    break;
                }
            }
            if (Padding) {
                Padding.Style.MarginLower.y = tailMode != TextStyleTailMode.Hidden ? 32 : 0;
            }
        }

        #endregion // Styling

        void IPoolConstructHandler.OnConstruct() {

        }

        void IPoolConstructHandler.OnDestruct() {

        }

        void IPoolAllocHandler.OnAlloc() {
            
        }

        void IPoolAllocHandler.OnFree() {
            
        }
    }

}