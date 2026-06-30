using AIS.Intervene;
using AIS.Shared;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Scripting;
using FieldDay.UI;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    static public class TextUtility {
        // Duration of the card "fly to toolbar" give animation.
        private const float FlyDuration = 0.4f;

        static public NewCardElement SpawnEvidenceCard(DialogueColumn column, StringHash32 evidenceId)
        {
            EvidenceCard data = Find.NamedAsset<EvidenceCard>(evidenceId);
            NewCardElement newElem = column.NewEvidencePool.Alloc();
            column.Layout.ActiveLines.PushBack(newElem.Positioner);
            newElem.DependencyWidget.gameObject.SetActive(true);
            newElem.DependencyWidget.Content.SetText(data.Label);
            newElem.DependencyWidget.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(data.Suit);
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();
            return newElem;
        }

        static public NewCardElement SpawnActionCard(DialogueColumn column, StringHash32 actionId) {
            // Action cards are not NamedAssets; they're parsed at runtime into ActionCardsState.
            ActionCardData data = default;
            bool hasData = false;
            if (Game.SharedState.TryGet(out ActionCardsState cardsState)) {
                hasData = cardsState.AllActionCards.TryGetValue(actionId, out data);
            }
            if (!hasData) {
                Log.Warn("[TextUtility] No ActionCardData found for action '{0}'.", actionId);
            }

            // Find the evidence this action depends on (first source that maps to it).
            // The dependency IS an EvidenceCard NamedAsset, unlike the action itself.
            EvidenceCard dependencyData = null;
            if (Game.SharedState.Has<EvidenceToActionConverterState>()) {
                EvidenceToActionConverterState convState = Find.State<EvidenceToActionConverterState>();
                StringHash32 srcId = EvidenceActionConvertUtility.GetEvidenceSourceForAction(convState, actionId).Hash();
                if (!srcId.IsEmpty) {
                    dependencyData = Find.NamedAsset<EvidenceCard>(srcId);
                }
            }

            NewCardElement newElem = column.NewActionCardPool.Alloc();
            column.Layout.ActiveLines.PushBack(newElem.Positioner);
            newElem.Card.gameObject.SetActive(true);
            if (dependencyData != null) {
                newElem.DependencyWidget.Content.SetText(dependencyData.Label);
                newElem.DependencyWidget.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(dependencyData.Suit);
            }
            if (hasData) {
                ActionCardUtility.PopulateCardUI(newElem.Card, data);
            }
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();
            return newElem;
        }

        static public NewCardElement SpawnScenarioCard(DialogueColumn column, StringHash32 scenarioId)
        {
            ScenarioData scenario = Find.NamedAsset<ScenarioData>(scenarioId);
            NewCardElement newElem = column.NewScenarioPool.Alloc();
            column.Layout.ActiveLines.PushBack(newElem.Positioner);
            newElem.GetComponentInChildren<Image>().sprite = scenario.Illustration;
            newElem.GetComponentInChildren<TMP_Text>().SetText(scenario.OverviewText);
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();
            return newElem;
        }

        // Waits for the player to click the card's confirm button (Add Evidence / Create), then hides it.
        static public IEnumerator WaitForConfirm(NewCardElement card) {
            while (!card.ConsumeClick()) {
                yield return null;
            }

            Sfx.Play("Narrative.Text.Advance");
            card.Button.gameObject.SetActive(false);
        }

        // Flies an action card's UICard widget toward a toolbar button, then hides just the widget.
        // The panel stays in the dialogue column as history; only the given card disappears.
        static public IEnumerator FlyCardToToolbar(NewCardElement card, ToolbarButton target) {
            return FlyWidgetToToolbar(card, (RectTransform) card.Card.transform, target);
        }

        // Flies an evidence chip's display widget toward a toolbar button, then hides just the widget.
        // GiveEvidenceChip populates DependencyWidget (an EvidenceDisplayWidget) rather than Card,
        // so the chip's visible element is the widget, not the UICard.
        static public IEnumerator FlyChipToToolbar(NewCardElement card, ToolbarButton target) {
            return FlyWidgetToToolbar(card, (RectTransform) card.DependencyWidget.transform, target);
        }

        static public IEnumerator FlyScenarioToToolbar(NewCardElement card, ToolbarButton target)
        {
            return FlyWidgetToToolbar(card, (RectTransform) card.ScenarioDetails.transform, target);
        }

        // Shared "fly a single widget to a toolbar button" mechanic used by both card and chip gives.
        // Only the given widget flies/shrinks; the rest of the panel stays put in the dialogue column
        // as history. The widget is reparented onto the sidebar canvas during the flight, then hidden
        // and restored to its original parent/transform so the pooled NewCardElement stays clean.
        static private IEnumerator FlyWidgetToToolbar(NewCardElement card, RectTransform widgetRect, ToolbarButton target) {
            Transform origParent = widgetRect.parent;
            int origSiblingIndex = widgetRect.GetSiblingIndex();
            Vector3 origLocalPos = widgetRect.localPosition;
            Vector3 origLocalScale = widgetRect.localScale;
            LayoutOffset widgetOffset = widgetRect.GetComponent<LayoutOffset>();
            if (widgetOffset) {
                widgetOffset.enabled = false;
            }

            // Reparent onto the sidebar canvas (above the toolbar) so the widget isn't drawn
            // behind it; the column canvas renders behind the sidebar.
            Canvas sidebarCanvas = target.GetComponentInParent<Canvas>();
            if (sidebarCanvas != null) {
                widgetRect.SetParent(sidebarCanvas.transform, true);
                widgetRect.SetAsLastSibling();
            }

            Vector3 dest = target.transform.position;
            card.GiveAnim.Replace(card, Routine.Combine(
                widgetRect.MoveTo(dest, FlyDuration, Axis.XY, Space.World).Ease(Curve.CubeIn),
                widgetRect.ScaleTo(0.2f, FlyDuration)
            ));
            yield return card.GiveAnim;

            // Hide the given widget but leave its panel in the column, and restore the widget
            // back under its panel so the pooled instance is clean if this card is reused.
            widgetRect.gameObject.SetActive(false);
            widgetRect.SetParent(origParent, false);
            widgetRect.SetSiblingIndex(origSiblingIndex);
            widgetRect.localPosition = origLocalPos;
            widgetRect.localScale = origLocalScale;
            if (widgetOffset) {
                widgetOffset.enabled = true;
            }
        }

        static public IEnumerator DisplayStatUpdate(DialogueColumn column, PlayerStatId statId, int originalValue, int newValue) {
            if (originalValue != newValue) {
                StatChangeElement newElem = column.StatChangePool.Alloc();
                GameIcons icons = Find.GlobalAsset<GameIcons>();

                column.Layout.ActiveLines.PushBack(newElem.Positioner);

                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    if (newValue > originalValue) {
                        newElem.Header.SetText("Skill Increased!");
                        psb.Builder.Append("+").AppendNoAlloc((newValue - originalValue));
                    } else {
                        newElem.Header.SetText("Skill Decreased :(");
                        psb.Builder.Append("-").AppendNoAlloc((originalValue - newValue));
                    }
                    newElem.StatText.SetText(psb);
                }

                newElem.StatIcon.sprite = icons.StatIcons[(int) statId];
                newElem.StatIcon.color = icons.StatColors[(int) statId];
                newElem.SetVisible(true);
                column.Layout.RecomputePositioning();

                yield return 0.1f;
            }
        }

        static public IEnumerator DisplayAndWaitForDefaultChoice(DialogueChoiceButton nextButton, string constText, IInputLayer inputLayer) {
            nextButton.Content.Populate("...");
            nextButton.Content.SetCharacterInfo(default, null);
            nextButton.Content.Layout.Sync();
            nextButton.Content.SetVisible(true);

            while (true) {
                if (nextButton.ConsumeClick() || (inputLayer.IsInputEnabled() && InputControls.CheckAdvanceInput())) {
                    break;
                }
                yield return null;
            }

            Sfx.Play("Narrative.Text.Advance");
            nextButton.Content.SetVisible(false);
            yield return 0.1f;
        }

        static public IEnumerator DisplayAndWaitForDialogueChoice(DialogueChoiceButton nextButton, TagString text, DialogueCharacterState charState, IInputLayer inputLayer) {
            nextButton.Content.Populate(text);
            nextButton.Content.SetCharacterInfo(charState, null);
            nextButton.Content.Layout.Sync();
            nextButton.Content.SetVisible(true);

            while (true) {
                if (nextButton.ConsumeClick() || (inputLayer.IsInputEnabled() && InputControls.CheckAdvanceInput())) {
                    break;
                }
                yield return null;
            }

            Sfx.Play("Narrative.Text.Advance");
            nextButton.Content.SetVisible(false);
            yield return 0.1f;
        }

        static public void ClearAllLines(DialogueColumnLayout layout) {
            while(layout.ActiveLines.TryPopFront(out var line)) {
                Pool.TryFree(line);
            }
            layout.ResetAlignment();
        }
    }
}