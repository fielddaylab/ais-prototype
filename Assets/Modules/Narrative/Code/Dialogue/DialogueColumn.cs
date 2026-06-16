using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf;
using Leaf.Runtime;
using System;
using System.Collections;
using UnityEngine;

namespace AIS.Narrative {
    [DisallowMultipleComponent]
    public sealed class DialogueColumn : BaseDialoguePrinter, IDialogueChoicePresenter {
        public DialogueLine.Pool LinePool;
        public NewCardElement.Pool NewCardPool;
        public StatChangeElement.Pool StatChangePool;

        public DialogueColumnLayout Layout;

        [NonSerialized] private bool m_AutoContinue;

        [NonSerialized] private IInputLayer m_InputLayer;
        [NonSerialized] private DialogueLine m_CurrentLine;

        public override void OnRegister() {
            base.OnRegister();
            m_InputLayer = IInputLayer.Find(this);
        }

        public override IEnumerator TypeLine(TagString text, TagTextData textData, DialogueCharacterState character) {
            if (m_CurrentLine.Visibility.alpha > 0) {
                yield break;
            }

            if (character.CharacterId == "Player") {
                yield return TextUtility.DisplayAndWaitForDialogueChoice(Layout.DefaultNextButton, text, character, m_InputLayer);
            }

            m_CurrentLine.SetVisible(true);
            Layout.RecomputePositioning();
            yield return 0.1f;
        }

        public override void UpdateCharacter(DialogueCharacterState character) {
            if (!m_CurrentLine) {
                return;
            }

            m_CurrentLine.SetCharacterInfo(character, null);
        }

        protected override void ConfigureEventHandler(TagStringEventHandler handler) {
            base.ConfigureEventHandler(handler);

            handler.Register("auto-continue", () => m_AutoContinue = true);
        }

        protected override void PrepareTextDisplay(TagString text, DialogueCharacterState character) {
            m_CurrentLine = LinePool.Alloc();
            Layout.ActiveLines.PushBack(m_CurrentLine.Positioner);
            m_CurrentLine.SetCharacterInfo(character, null);
            m_CurrentLine.Populate(text);

            if (m_CurrentLine.CharacterLayout.isActiveAndEnabled) {
                m_CurrentLine.CharacterLayout.Sync();
            }
            m_CurrentLine.Layout.Sync();
            m_CurrentLine.SetVisible(false);

            m_AutoContinue = false;
        }

        public override void FastForwardLine(int visibleCount, int richCount) { }

        public override IEnumerator CompleteLine() {
            m_CurrentLine = null;

            if (m_AutoContinue) {
                yield return 0.1f;
                yield break;
            }

            if (LeafRuntime.PredictChoice(CurrentThread)) {
                yield break;
            }

            StringHash32 nextLine = LeafRuntime.PredictLine(CurrentThread);
            if (!nextLine.IsEmpty) {
                ScriptUtility.ReadText(CurrentThread.TagString, ThreadOwner, nextLine);
                if (ScriptUtility.GetCharacterId(CurrentThread.TagString, CurrentThread.GetCharacterState().CharacterId) == "Player") {
                    yield break;
                }
            }

            yield return TextUtility.DisplayAndWaitForDefaultChoice(Layout.DefaultNextButton, "...", m_InputLayer);
        }

        public IEnumerator ShowOptions(LeafChoice choice, LeafNode node, ScriptThread thread, DialogueCharacterState character) {
            PlayerStatBlock currentStats = Find.State<PlayerStats>().StatBlock;
            PlayerInventory inv = Find.State<PlayerInventory>();
            GameIcons icons = Find.GlobalAsset<GameIcons>();

            int[] visibleOptions = new int[Layout.Choices.Length];
            int choiceCount = 0;
            bool anySelectable = false;

            for (int i = 0; i < choice.Count; i++) {
                if (!DialogueChoiceUtility.IsVisible(choice, choice[i])) {
                    continue;
                }
                if (choiceCount >= Layout.Choices.Length) {
                    Log.Warn("[DialogueColumn] Too many choices");
                    break;
                }
                visibleOptions[choiceCount++] = i;
                anySelectable |= DialogueChoiceUtility.IsSelectable(choice, choice[i]);
            }

            if (!anySelectable) {
                StringHash32 fallbackId = DialogueChoiceUtility.ResolveFallbackNode((ScriptNode) node, inv.TimeRemaining <= 0);
                if (!fallbackId.IsEmpty) {
                    DialogueChoiceUtility.Redirect(choice, fallbackId);
                    yield break;
                }

                Log.Error("[DialogueColumn] No selectable choices on node '{0}' and no fallback node found", node.Id());
                if (choiceCount == 0) {
                    choice.Choose(0);
                    yield break;
                }
            }

            using (PooledStringBuilder psb = PooledStringBuilder.Create()) {

                for (int i = 0; i < choiceCount; i++) {
                    int optionIndex = visibleOptions[i];
                    var data = choice[optionIndex];
                    DialogueChoiceButton btn = Layout.Choices[i];
                    btn.gameObject.SetActive(true);

                    ScriptUtility.ReadText(thread.TagString, node, data.LineCode);
                    btn.Content.Populate(thread.TagString);
                    bool choiceAvailable = data.IsAvailable;

                    DialogueChoiceRequirements req = DialogueChoiceRequirements.Read(choice, optionIndex);
                    if (req.TimeConsumed > 0) {
                        btn.TimeGroup.SetActive(true);
                        btn.TimeRequirement.sprite = icons.TimeIcons[req.TimeConsumed];
                        choiceAvailable &= inv.TimeRemaining >= req.TimeConsumed;
                    } else {
                        btn.TimeGroup.SetActive(false);
                    }

                    if (req.StatId != PlayerStatId.Invalid && req.StatThreshold > PlayerStatBlock.MinValue) {
                        btn.StatGroup.gameObject.SetActive(true);
                        btn.StatRequirement.color = icons.StatColors[(int)req.StatId];
                        switch (req.StatId) {
                            case PlayerStatId.Tech: {
                                psb.Builder.Append("Tech");
                                break;
                            }
                            case PlayerStatId.Innovate: {
                                psb.Builder.Append("Innovate");
                                break;
                            }
                            case PlayerStatId.Research: {
                                psb.Builder.Append("Research");
                                break;
                            }
                            case PlayerStatId.Ranger: {
                                psb.Builder.Append("Ranger");
                                break;
                            }
                            case PlayerStatId.Communicate: {
                                psb.Builder.Append("Communicate");
                                break;
                            }
                        }
                        psb.Builder.Append(' ').AppendNoAlloc(req.StatThreshold);
                        btn.StatRequirement.SetText(psb);
                        psb.Builder.Clear();
                        btn.StatGroup.Sync();
                        choiceAvailable &= currentStats[req.StatId] >= req.StatThreshold;
                    } else {
                        btn.StatGroup.gameObject.SetActive(false);
                    }

                    btn.Listener.enabled = choiceAvailable;

                    if (choiceAvailable) {
                        btn.Content.SetCharacterInfo(ScriptUtility.GetCharacterState(thread.TagString, new DialogueCharacterState() { CharacterId = "_PlayerAction" }), null);
                    } else {
                        btn.Content.SetTextStyle(Find.NamedAsset<TextStyle>("DisabledChoice"), null);
                    }

                    btn.Content.Layout.Sync();
                    btn.Content.SetVisible(true);
                }
            }

            for (int i = choiceCount; i < Layout.Choices.Length; i++) {
                DialogueChoiceButton btn = Layout.Choices[i];
                btn.gameObject.SetActive(false);
            }

            using (var query = Layout.ChoiceGroup.QueryLayoutChildren()) {
                Positioning.HorizontalLayout(query, Layout.ChoiceLayout);
            }

            bool chosen = false;
            while(!chosen) {
                for(int i = 0; i < choiceCount; i++) {
                    if (Layout.Choices[i].ConsumeClick()) {
                        choice.Choose(visibleOptions[i]);
                        chosen = true;
                        break;
                    }
                }
                yield return null;
            }

            for(int i = 0; i < choiceCount; i++) {
                Layout.Choices[i].Content.SetVisible(false);
                Layout.Choices[i].gameObject.SetActive(false);
            }

            var chosenRequirements = DialogueChoiceRequirements.Read(choice, choice.ChosenIndex());
            if (chosenRequirements.TimeConsumed > 0) {
                PlayerUtility.DecreaseTime(chosenRequirements.TimeConsumed);
            }
        }
    }
}