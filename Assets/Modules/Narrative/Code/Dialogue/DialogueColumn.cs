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
using Leaf;
using Leaf.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent]
    public sealed class DialogueColumn : BaseDialoguePrinter, IDialogueChoicePresenter {
        public DialogueColumnLayout Layout;

        [NonSerialized] private IInputLayer m_InputLayer;
        [NonSerialized] private DialogueLine m_CurrentLine;

        public override void OnRegister() {
            base.OnRegister();
            m_InputLayer = IInputLayer.Find(this);
        }

        public override IEnumerator TypeLine(TagString text, TagTextData textData) {
            if (m_CurrentLine.Visibility.alpha <= 0) {
                m_CurrentLine.SetVisible(true);
                Layout.RecomputePositioning();
                yield return 0.1f;
            }
        }

        public override void UpdateCharacter(DialogueCharacterState character) {
            if (!m_CurrentLine) {
                return;
            }

            m_CurrentLine.SetCharacterInfo(character, null);
        }

        protected override void ConfigureEventHandler(TagStringEventHandler handler) {
            base.ConfigureEventHandler(handler);
        }

        protected override void PrepareTextDisplay(TagString text, DialogueCharacterState character) {
            m_CurrentLine = Layout.LinePool.Alloc();
            Layout.ActiveLines.PushBack(m_CurrentLine);
            m_CurrentLine.SetCharacterInfo(character, null);
            m_CurrentLine.Populate(text);

            if (m_CurrentLine.CharacterLayout.isActiveAndEnabled) {
                m_CurrentLine.CharacterLayout.Sync();
            }
            m_CurrentLine.Layout.Sync();
            Positioning.SetAnchor((RectTransform) m_CurrentLine.transform, TextAnchor.LowerCenter);
            m_CurrentLine.SetVisible(false);
        }

        public override void FastForwardLine(int visibleCount, int richCount) { }

        public override IEnumerator CompleteLine() {
            m_CurrentLine = null;

            if (LeafRuntime.PredictChoice(ThreadOwner.GetThread())) {
                yield break;
            }

            Layout.DefaultNextButton.Content.Populate("...");
            Layout.DefaultNextButton.Content.SetCharacterInfo(default, null);
            Layout.DefaultNextButton.Content.Layout.Sync();
            Layout.DefaultNextButton.Content.SetVisible(true);

            while (true) {
                if (Layout.DefaultNextButton.ConsumeClick() || (m_InputLayer.IsInputEnabled() && InputControls.CheckAdvanceInput())) {
                    break;
                }

                yield return null;
            }

            Layout.DefaultNextButton.Content.SetVisible(false);
            yield return 0.1f;
        }

        public IEnumerator ShowOptions(LeafChoice choice, LeafNode node, ScriptThread thread, DialogueCharacterState character) {
            for(int i = 0; i < choice.Count; i++) {
                var data = choice[i];
                DialogueChoiceButton btn = Layout.Choices[i];
                btn.gameObject.SetActive(true);

                ScriptUtility.ReadText(thread.TagString, node, data.LineCode);
                btn.Content.Populate(thread.TagString);
                btn.Content.Layout.Sync();
                btn.Content.SetVisible(true);

                btn.Listener.enabled = data.IsAvailable;
            }

            for (int i = choice.Count; i < Layout.Choices.Length; i++) {
                DialogueChoiceButton btn = Layout.Choices[i];
                btn.gameObject.SetActive(false);
            }

            bool chosen = false;
            while(!chosen) {
                for(int i = 0; i < choice.Count; i++) {
                    if (Layout.Choices[i].ConsumeClick()) {
                        choice.Choose(i);
                        chosen = true;
                        break;
                    }
                }
                yield return null;
            }

            for(int i = 0; i < Layout.Choices.Length; i++) {
                Layout.Choices[i].Content.SetVisible(false);
                Layout.Choices[i].gameObject.SetActive(false);
            }
        }
    }
}