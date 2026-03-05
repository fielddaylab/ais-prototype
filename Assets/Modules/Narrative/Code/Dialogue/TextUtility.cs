using BeauUtil.Tags;
using FieldDay.Audio;
using FieldDay.Scripting;
using FieldDay.UI;
using System.Collections;

namespace AIS.Narrative {
    static public class TextUtility {
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
    }
}