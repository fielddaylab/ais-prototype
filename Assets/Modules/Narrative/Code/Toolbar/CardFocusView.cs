using System.Collections;
using System.Collections.Generic;
using AIS.Intervene;
using AIS.Shared;
using BeauUtil;
using FieldDay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative
{
    public class CardFocusView : MonoBehaviour
    {
        public TMP_Text Title;
        public Image CardSuite;
        public TMP_Text CardCost;
        public TMP_Text CardDescription;
        
        public Image[] ScienceBonuses;
        public TMP_Text BonusText;

        public Color GrayedOutColor;

        public void PopulateFocusView(ActionCardData data)
        {
            Title.SetText(data.Title);
            CardCost.SetText("$" + data.Cost.ToStringLookup());

            // temporary for testing, switch to only focus description later
            if (data.FocusDescription != "") {
                CardDescription.SetText(data.FocusDescription);
            } else
            {
                CardDescription.SetText(data.Description);
            }
            
            CardSuite.sprite = CardVisualLookupUtility.LookupSuitIcon(data.Suit);

            // Issues with resizing. Bring up in a design meeting later.
            // RectTransform parentRect = CardDescription.GetComponentInParent<RectTransform>();
            // LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

            

            // Todo: In the future, this needs to read the override effect of the given card, and determine the points needed to be filled for the override.

            // find the first override
            ActionEffectOverride effectOverride = new ActionEffectOverride();
            effectOverride.Condition.Condition = ActionCondition.None;
            bool hasOverride = false;
            if (data.Effects != null)
            {
                foreach (var bundle in data.Effects)
                {
                    if (bundle.EffectOverride.Condition.Condition != ActionCondition.None)
                    {
                        effectOverride = bundle.EffectOverride;
                        hasOverride = true;
                        break;
                    }
                }
            }

            ActionTargetCondition condition = effectOverride.Condition;

            // No override ability: hide the whole group.
            if (!hasOverride)
            {
                foreach (var image in ScienceBonuses)
                {
                    image.gameObject.SetActive(false);
                }
                BonusText.SetText($"No bonus available."); 
                return;
            }
            
            PlayerStatId suite = ActionCardUtility.GetConditionStat(condition.Condition);
            int requirement = ActionCardUtility.GetConditionThreshold(condition);

            PlayerStats stats = Find.State<PlayerStats>();
            PlayerStatBlock block = stats.StatBlock;
            int suitTotal = block[suite];

            for (int i = 0; i < ScienceBonuses.Length; i++)
            {
                ScienceBonuses[i].sprite = CardVisualLookupUtility.LookupSuitIcon(suite);
                ScienceBonuses[i].gameObject.SetActive(true);
                
                if (i + 1 > requirement)
                {
                    ScienceBonuses[i].gameObject.SetActive(false);
                }
                else if (i >= suitTotal) 
                { 
                    ScienceBonuses[i].color = GrayedOutColor;
                } 
                else 
                {
                    ScienceBonuses[i].color = Color.white;
                }
            }

            if (suitTotal >= requirement)
            {
                BonusText.SetText($"{suitTotal}/{requirement} {suite.ToString().ToLower()} -- bonus active!");
            }
            else
            {
                BonusText.SetText($"{suitTotal}/{requirement} {suite.ToString().ToLower()} -- bonus not yet active");
            }

        }
    }
}