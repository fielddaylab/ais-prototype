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
    public class StatChangeElement : BatchedComponent {
        [Serializable] public sealed class Pool : SerializablePool<StatChangeElement> { }

        [Header("Layout")]
        public DialogueColumnLayoutElement Positioner;
        public CanvasGroup Visibility;
        public LayoutOffset Offset;

        [Header("Content")]
        public TMP_Text Header;
        public Image StatIcon;
        public TMP_Text StatText;

        private void Awake() {
            SetVisible(false);
        }

        public void SetVisible(bool visible) {
            Visibility.alpha = visible ? 1 : 0;
            Visibility.blocksRaycasts = visible;
            if (visible) {
                PopAnim.Play(Offset, PopAnim.Default);
            }
        }
    }

}