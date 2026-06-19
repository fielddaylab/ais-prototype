using AIS.Intervene;
using AIS.Shared;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Scripting;
using FieldDay.UI;
using System.Collections;
using System.Threading;
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
            newElem.Card.gameObject.SetActive(true);
            PopulateCardVisual(newElem.Card, data);
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();
            return newElem;
        }

        static public NewCardElement SpawnActionCard(DialogueColumn column, StringHash32 actionId) {
            EvidenceCard data = Find.NamedAsset<EvidenceCard>(actionId);

            // Find the evidence this action depends on (first source that maps to it).
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
            }
            PopulateCardVisual(newElem.Card, data);
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();
            return newElem;
        }

        // Fills a UICard's face from an EvidenceCard: label as title, plus suit and illustration.
        static private void PopulateCardVisual(UICard card, EvidenceCard data) {
            if (card == null || data == null) { return; }

            if (card.Title != null) {
                card.Title.SetText(data.Label);
            }
            if (card.Suit != null) {
                card.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(data.Suit);
            }
            if (card.Img != null) {
                card.Img.sprite = data.Illustration != null ? data.Illustration.sprite : null;
            }
        }

        // Waits for the player to click the card's confirm button (Add Evidence / Create), then hides it.
        static public IEnumerator WaitForConfirm(NewCardElement card) {
            while (!card.ConsumeClick()) {
                yield return null;
            }

            Sfx.Play("Narrative.Text.Advance");
            card.Button.gameObject.SetActive(false);
        }

        // Flies the card's widget toward a toolbar button, then hides just the widget.
        // The panel stays in the dialogue column as history; only the given card disappears.
        static public IEnumerator FlyCardToToolbar(NewCardElement card, ToolbarButton target) {
            // Only the widget itself flies/shrinks; the rest of the panel stays put.
            RectTransform widgetRect = (RectTransform) card.Card.transform;
            Transform origParent = widgetRect.parent;
            int origSiblingIndex = widgetRect.GetSiblingIndex();
            Vector3 origLocalPos = widgetRect.localPosition;
            Vector3 origLocalScale = widgetRect.localScale;
            LayoutOffset widgetOffset = card.Card.GetComponent<LayoutOffset>();
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
            card.Card.gameObject.SetActive(false);
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
        }
    }
}