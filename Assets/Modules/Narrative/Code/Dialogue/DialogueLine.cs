using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay.Components;
using FieldDay.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public class DialogueLine : BatchedComponent, IPoolAllocHandler, IPoolConstructHandler {
        [Header("Layout")]
        public LayoutSizeGroup Layout;
        public RectTransform RootPivot;
        public RectTransform ArrowRoot;

        [Header("Content")]
        public TMP_Text Text;

        [Header("Header")]
        public TMP_Text CharacterName;
        public LayoutSizeGroup CharacterLayout;

        [Header("Styling")]
        public Graphic[] BackgroundColor;
        public RoundedRectGraphic[] Rounding;

        [NonSerialized] private float[] m_RoundingBase;

        public void Populate(TagString tagData) {
            Text.SetText(tagData.RichText);
        }

        public void SetBackgroundColor(Color color) {
            for(int i = 0; i < BackgroundColor.Length; i++) {
                BackgroundColor[i].color = color;
            }
        }

        public void SetRoundingMultiplier(float multiplier) {
            if (m_RoundingBase != null) {
                m_RoundingBase = new float[Rounding.Length];
                for(int i = 0; i < m_RoundingBase.Length; i++) {
                    m_RoundingBase[i] = Rounding[i].CornerRadius;
                }
            }

            for(int i = 0; i < Rounding.Length; i++) {
                Rounding[i].CornerRadius = m_RoundingBase[i] * multiplier;
            }
        }

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