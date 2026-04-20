using BeauPools;
using BeauUtil;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Audio;
using FieldDay.Scripting;
using FieldDay.UI;
using System.Collections;
using System.Threading;

namespace AIS.Narrative {
    static public class TextUtility {
        static public IEnumerator DisplayNewEvidence(DialogueColumn column, StringHash32 evidenceId) {
            EvidenceCard data = Find.NamedAsset<EvidenceCard>(evidenceId);
            NewCardElement newElem = column.NewCardPool.Alloc();
            column.Layout.ActiveLines.PushBack(newElem.Positioner);
            newElem.Widget.Content.SetText(data.Label);
            newElem.Layout.VerticalLayout(LayoutOptions.PreferredSize(4, 1));
            newElem.SetVisible(true);
            column.Layout.RecomputePositioning();

            yield return 0.1f;
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