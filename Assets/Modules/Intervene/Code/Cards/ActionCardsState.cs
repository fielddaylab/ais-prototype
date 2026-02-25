using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Debugging;
using FieldDay.SharedState;
using Leaf.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public struct ActionCardData
    {
        public SerializedHash32 CardID;
        public string Title;
        public string Description;
        public string ImgPath;

        public int Cost;
        public ActionEffectBundle[] Effects;

        public bool IsValid;

        public ActionCardData(SerializedHash32 cardID, string title, string desc, string imgPath, int cost, ActionEffectBundle[] effects)
        {
            CardID = cardID;
            Title = title;
            Description = desc;
            ImgPath = imgPath;

            Cost = cost;
            Effects = effects;

            IsValid = true;
        }
    }

    [SharedStateInitOrder(10)]
    public sealed class ActionCardsState : SharedStateComponent
    {
        [HideInInspector] public Dictionary<StringHash32, ActionCardData> AllActionCards;
        [HideInInspector] public List<StringHash32> UnlockedActionCards;

        public TextAsset[] CardSources;

        private void Awake()
        {
            // Initialize Lists
            AllActionCards = new Dictionary<StringHash32, ActionCardData>();
            UnlockedActionCards = new List<StringHash32>();

            // Populate Card data
            ActionCardsUtility.PopulateCards(this);
        }
    }

    static public class ActionCardsUtility
    {

        #region Card Definition Parsing

        private static readonly string TITLE_TAG = "@title";
        private static readonly string DESC_TAG = "@desc";
        private static readonly string IMAGE_PATH_TAG = "@path";
        private static readonly string COST_TAG = "@cost";
        private static readonly string EFFECT_TAG = "@effect";
        private static readonly string OVERRIDE_EFFECT_TAG = "@overrideeffect";

        private static readonly string HARDCODE_LINE = "hardcoded";
        private static readonly string TARGET_LINE = "target:";
        private static readonly string VERB_LINE = "verb:";
        private static readonly string SPEC_LINE = "specificity:";
        private static readonly string MAX_TARGETS_LINE = "maxtargets:";
        private static readonly string IF_KEYWORD = "if";
        private static readonly string ODDS_KEYWORD = "odds";
        private static readonly string RELATIVE_KEYWORD = "relative";

        private static readonly string ENTRY_SEP = "::";

        private static readonly char GR_CHAR = '>';
        private static readonly string GREQ_CHAR = ">=";
        private static readonly char EQ_CHAR = '=';
        private static readonly char LE_CHAR = '<';
        private static readonly string LEEQ_CHAR = "<=";

        private static readonly char[] END_DELIMS = new char[] { '\r', '\n' };
        private static readonly char[] COMMA_DELIM = new char[] { ',' };
        private static readonly char[] MODIFY_DELIM = new char[] { ';' };

        #endregion // Card Definition Parsing

        static public void PopulateCards(ActionCardsState cardsState)
        {
            List<string> cardStrings;

            foreach (TextAsset cardSource in cardsState.CardSources)
            {
                cardStrings = TextIO.TextAssetToList(cardSource, ENTRY_SEP);

                foreach (string str in cardStrings)
                {
                    try
                    {
                        ActionCardData newCard = ConvertDefToCard(str);

                        cardsState.AllActionCards.Add(newCard.CardID, newCard);

                        Debug.Log("[CardUtility] added " + newCard.CardID + ".");
                    }
                    catch (Exception e)
                    {
                        Debug.Log("[CardUtility] Parsing error! " + e.Message);
                    }
                    // yield return null;
                }
            }
        }

        static private ActionCardData ConvertDefToCard(string cardDef)
        {
            Debug.Log("[CardUtility] converting card: " + cardDef);
            SerializedHash32 cardID = "";
            string cardIdStr = "";
            string title = "";
            string desc = "";
            string imgPath = "";

            // Parse into data

            // First line must be card id
            cardIdStr = cardDef.Substring(0, cardDef.IndexOfAny(END_DELIMS));
            cardID = cardIdStr;
            Debug.Log("[CardUtility] parsed card id : " + cardID);

            // Title comes after @title
            int titleIndex = cardDef.ToLower().IndexOf(TITLE_TAG);
            if (titleIndex != -1)
            {
                string afterTitle = cardDef.Substring(titleIndex);
                int offset = TITLE_TAG.Length;
                title = cardDef.Substring(titleIndex + offset, afterTitle.IndexOfAny(END_DELIMS) - offset).Trim();
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] title syntax error!");

                throw new Exception("Title");
            }

            // Description comes after @desc
            int descIndex = cardDef.ToLower().IndexOf(DESC_TAG);

            if (descIndex != -1)
            {
                string afterDesc = cardDef.Substring(descIndex);
                int offset = DESC_TAG.Length;
                desc = cardDef.Substring(descIndex + offset, afterDesc.IndexOfAny(END_DELIMS) - offset).Trim();
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] description syntax error!");

                throw new Exception("Description");
            }


            // Image Path comes after @path
            int imgPathIndex = cardDef.ToLower().IndexOf(IMAGE_PATH_TAG);

            if (imgPathIndex != -1)
            {
                string afterPathIndex = cardDef.Substring(imgPathIndex);
                int offset = IMAGE_PATH_TAG.Length;
                string imgPathStr = cardDef.Substring(imgPathIndex + offset, afterPathIndex.IndexOfAny(END_DELIMS) - offset).Trim();

                imgPath = imgPathStr;
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] image path syntax error!");

                throw new Exception("Image Path");
            }

            // Cost parsing
            int cost = 0;
            int costIndex = cardDef.ToLower().IndexOf(COST_TAG);
            if (costIndex != -1)
            {
                string afterCost = cardDef.Substring(costIndex);
                int offset = COST_TAG.Length;
                string costStr = cardDef.Substring(costIndex + offset, afterCost.IndexOfAny(END_DELIMS) - offset).Trim();

                if (!int.TryParse(costStr, out cost))
                {
                    Debug.LogWarning("[CardUtility] Could not parse cost value: " + costStr + ". Defaulting to 0.");
                    cost = 0;
                }
            }

            // Action Effects parsing
            ActionEffectBundle[] effects = ParseEffects(cardDef, cardIdStr);

            return new ActionCardData(cardID, title, desc, imgPath, cost, effects);
        }

        #region Effect Parsing Helpers

        static private ActionEffectBundle[] ParseEffects(string cardDef, string cardIdStr)
        {
            List<ActionEffectBundle> effects = new List<ActionEffectBundle>();

            // Find all @effect blocks
            int searchStart = 0;
            while (true)
            {
                int effectIndex = cardDef.ToLower().IndexOf(EFFECT_TAG, searchStart);
                if (effectIndex == -1) break;

                // Find the next @effect or end of string to determine this effect's boundaries
                int nextEffectIndex = cardDef.ToLower().IndexOf(EFFECT_TAG, effectIndex + EFFECT_TAG.Length);
                string effectBlock;

                if (nextEffectIndex == -1)
                {
                    effectBlock = cardDef.Substring(effectIndex);
                }
                else
                {
                    effectBlock = cardDef.Substring(effectIndex, nextEffectIndex - effectIndex);
                }

                // Parse this effect block (which may contain @overrideEffect)
                ActionEffectBundle effect = ParseSingleEffectBundle(effectBlock, cardIdStr);
                effects.Add(effect);

                searchStart = effectIndex + EFFECT_TAG.Length;
            }

            return effects.ToArray();
        }

        static private ActionEffectBundle ParseSingleEffectBundle(string effectBlock, string cardIdStr)
        {
            // Check if there's an @overrideEffect block
            int overrideIndex = effectBlock.ToLower().IndexOf(OVERRIDE_EFFECT_TAG);

            string primaryEffectBlock;
            string overrideEffectBlock = null;
            ActionTargetCondition overrideTargetCondition = new ActionTargetCondition();
            overrideTargetCondition.Condition = ActionCondition.None;

            if (overrideIndex != -1)
            {
                // Split into primary and override blocks
                primaryEffectBlock = effectBlock.Substring(0, overrideIndex);
                overrideEffectBlock = effectBlock.Substring(overrideIndex);

                // Parse the condition from @overrideEffect line
                // Format: "@overrideEffect if awareness > 2"
                string overrideLine = overrideEffectBlock.Substring(0, overrideEffectBlock.IndexOfAny(END_DELIMS));
                string conditionPart = overrideLine.Substring(OVERRIDE_EFFECT_TAG.Length).Trim();

                if (conditionPart.ToLower().StartsWith(IF_KEYWORD))
                {
                    overrideTargetCondition = ParseCondition(conditionPart);
                }
            }
            else
            {
                primaryEffectBlock = effectBlock;
            }

            // Parse primary effect
            ActionEffect primaryEffect = ParseEffect(primaryEffectBlock, cardIdStr);

            ActionEffectBundle effectBundle = new ActionEffectBundle
            {
                ActionEffect = primaryEffect
            };

            // Parse override effect if present
            if (overrideEffectBlock != null)
            {
                ActionEffect overrideEffect = ParseEffect(overrideEffectBlock, cardIdStr);

                ActionEffectOverride effectOverride = new ActionEffectOverride
                {
                    Condition = overrideTargetCondition,
                    Override = overrideEffect
                };

                effectBundle.EffectOverride = effectOverride;
            }

            return effectBundle;
        }

        static private ActionEffect ParseEffect(string effectBlock, string cardIdStr)
        {
            List<ActionTargetDetails> targets = new List<ActionTargetDetails>();
            ActionSpecificity specificity = ActionSpecificity.Specific;
            int maxTargets = 1;
            List<ActionVerbDetails> verbs = new List<ActionVerbDetails>();
            string hardCodedId = String.Empty;

            // Split into lines
            string[] lines = effectBlock.Split(END_DELIMS, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim().ToLower();

                if (trimmedLine.StartsWith(TARGET_LINE))
                {
                    string targetContent = line.Substring(line.ToLower().IndexOf(TARGET_LINE) + TARGET_LINE.Length).Trim();
                    ActionTargetDetails target = ParseTarget(targetContent);
                    targets.Add(target);
                }
                else if (trimmedLine.StartsWith(VERB_LINE))
                {
                    string verbContent = line.Substring(line.ToLower().IndexOf(VERB_LINE) + VERB_LINE.Length).Trim();
                    ActionVerbDetails verb = ParseVerb(verbContent);
                    verbs.Add(verb);
                }
                else if (trimmedLine.StartsWith(SPEC_LINE))
                {
                    string specificityContent = line.Substring(line.ToLower().IndexOf(SPEC_LINE) + SPEC_LINE.Length).Trim();
                    specificity = ParseActionSpecificity(specificityContent.Trim());
                }
                else if (trimmedLine.StartsWith(MAX_TARGETS_LINE))
                {
                    string maxTargetContent = line.Substring(line.ToLower().IndexOf(MAX_TARGETS_LINE) + MAX_TARGETS_LINE.Length).Trim();
                    if (int.TryParse(maxTargetContent, out int count))
                    {
                        maxTargets = count;
                    }
                }
                else if (trimmedLine.StartsWith(HARDCODE_LINE))
                {
                    hardCodedId = cardIdStr;
                }
            }

            ActionEffect effect = new ActionEffect
            {
                AllTargets = targets.ToArray(),
                Specificity = specificity,
                MaxTargets = maxTargets,
                Verbs = verbs.ToArray(),
                HardCodedId = hardCodedId,
            };

            return effect;
        }

        static private ActionTargetDetails ParseTarget(string targetContent)
        {
            // Format: [type], [optional conditions]
            // Example: "invasive, if population < 5"

            string[] parts = targetContent.Split(COMMA_DELIM, StringSplitOptions.RemoveEmptyEntries);

            ActionTargetDetails targetDetails = new ActionTargetDetails();

            // Parse target type (required)
            if (parts.Length > 0)
            {
                targetDetails.Target = ParseActionTarget(parts[0].Trim());
            }

            // Check if next part is a number (count) or a condition
            int conditionStartIndex = 1;

            // Parse conditions (everything after count that starts with "if")
            List<ActionTargetCondition> conditions = new List<ActionTargetCondition>();
            for (int i = conditionStartIndex; i < parts.Length; i++)
            {
                string conditionStr = parts[i].Trim();
                if (conditionStr.ToLower().StartsWith(IF_KEYWORD))
                {
                    ActionTargetCondition condition = ParseCondition(conditionStr);
                    conditions.Add(condition);
                }
            }
            targetDetails.Conditions = conditions.ToArray();

            return targetDetails;
        }

        static private ActionVerbDetails ParseVerb(string verbContent)
        {
            // Format: [verb], [value], [modifierType], odds [oddsValue]
            // For "modify" verb: [verb], ([value1], [value2], ...), [modifierType], odds [oddsValue]
            // Examples: 
            //   "reduce, 10, fixed, odds 0.75"
            //   "reveal, odds 0.5"
            //   "modify, (-2, -1, -1), fixed"
            //   "modify, (5, 10), fixed, odds 0.8"

            string[] parts = verbContent.Split(COMMA_DELIM, StringSplitOptions.RemoveEmptyEntries);

            ActionVerbDetails verbDetails = new ActionVerbDetails();
            verbDetails.Odds = 1; // 100% by default
            verbDetails.Values = new List<float>(); // Initialize list
            verbDetails.ModType = ModifierType.Fixed;

            // Parse verb type (required)
            if (parts.Length > 0)
            {
                verbDetails.Verb = ParseActionVerb(parts[0].Trim());
            }

            bool valuesSet = false;

            // Process remaining parts, looking for "odds" keyword or parentheses for modify verb
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                string partLower = part.ToLower();

                // Check if this part starts with "odds"
                if (partLower.StartsWith(ODDS_KEYWORD))
                {
                    // Extract the odds value after "odds"
                    string oddsStr = part.Substring(ODDS_KEYWORD.Length).Trim(); // Remove "odds" keyword
                    if (float.TryParse(oddsStr, out float odds))
                    {
                        verbDetails.Odds = odds;
                    }
                    else
                    {
                        Debug.LogWarning("[CardUtility] Could not parse odds value: " + oddsStr + ". Defaulting to 1.0 (100%).");
                    }
                }
                // Check if this part starts with "relative"
                else if (partLower.StartsWith(RELATIVE_KEYWORD))
                {
                    // Extract the id after "relative"
                    string relativeId = part.Substring(RELATIVE_KEYWORD.Length).Trim(); // Remove "relative" keyword
                    if (relativeId.Equals("aware"))
                    {
                        // account for shorthand
                        relativeId = "awareness";
                    }
                    verbDetails.RelativeId = relativeId;
                }
                // Check if this is a list of values in parentheses (for modify verb)
                else if (part.Contains("(") && part.Contains(")"))
                {
                    // Extract content between parentheses
                    int startIndex = part.IndexOf('(');
                    int endIndex = part.IndexOf(')');
                    string valuesList = part.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Split by comma and parse each value
                    string[] valueStrings = valuesList.Split(MODIFY_DELIM, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string valueStr in valueStrings)
                    {
                        if (float.TryParse(valueStr.Trim(), out float value))
                        {
                            verbDetails.Values.Add(value);
                        }
                        else
                        {
                            Debug.LogWarning("[CardUtility] Could not parse value in list: " + valueStr.Trim());
                        }
                    }
                    valuesSet = true;
                }
                // Parse single value (first non-odds number, if not already set by list)
                else if (!valuesSet && i == 1 && float.TryParse(part, out float value))
                {
                    verbDetails.Values.Add(value);
                    valuesSet = true;
                }
                // Parse modifier type (look for modifier keywords)
                else
                {
                    verbDetails.ModType = ParseModifierType(part);
                }
            }

            return verbDetails;
        }

        static private ActionTargetCondition ParseCondition(string conditionStr)
        {
            // Format: "if population < 5", "if awareness > 10", "if type = water"
            // Remove "if" keyword
            string content = conditionStr.ToLower().Replace(IF_KEYWORD, "").Trim();

            ActionTargetCondition condition = new ActionTargetCondition();

            // Determine operator and split content
            char operatorChar = '\0';
            string[] parts = null;

            if (content.Contains(LEEQ_CHAR))
            {
                operatorChar = '≤'; // Use special char to represent <=
                parts = content.Split(new string[] { LEEQ_CHAR }, StringSplitOptions.None);
            }
            else if (content.Contains(GREQ_CHAR))
            {
                operatorChar = '≥'; // Use special char to represent >=
                parts = content.Split(new string[] { GREQ_CHAR }, StringSplitOptions.None);
            }
            else if (content.Contains(LE_CHAR))
            {
                operatorChar = LE_CHAR;
                parts = content.Split(LE_CHAR);
            }
            else if (content.Contains(GR_CHAR))
            {
                operatorChar = GR_CHAR;
                parts = content.Split(GR_CHAR);
            }
            else if (content.Contains(EQ_CHAR))
            {
                operatorChar = EQ_CHAR;
                parts = content.Split(EQ_CHAR);
            }

            if (parts == null || parts.Length < 2)
            {
                condition.Condition = ActionCondition.None;
                return condition;
            }

            // Extract left side (variable name) and right side (value)
            string variableName = parts[0].Trim();
            string valueStr = parts[1].Trim();

            // Determine condition type based on variable name and operator
            // Population conditions
            if (variableName.Contains("pop") || variableName.Contains("population"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.PopulationLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.PopulationGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.PopulationEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Awareness conditions
            else if (variableName.Contains("aware") || variableName.Contains("awareness"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.AwarenessLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.AwarenessGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.AwarenessEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Social/Outreach conditions
            else if (variableName.Contains("social"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.SocialLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.SocialGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.SocialEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Ranger/Outdoor conditions
            else if (variableName.Contains("outdoor"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.OutdoorLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.OutdoorGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.OutdoorEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Tech/Gear conditions
            else if (variableName.Contains("tech"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.TechLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.TechGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.TechEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Research/Science conditions
            else if (variableName.Contains("research"))
            {
                if (operatorChar == LE_CHAR || operatorChar == '≤')
                {
                    condition.Condition = ActionCondition.ResearchLessThan;
                }
                else if (operatorChar == GR_CHAR || operatorChar == '≥')
                {
                    condition.Condition = ActionCondition.ResearchGreaterThan;
                }
                else if (operatorChar == EQ_CHAR)
                {
                    condition.Condition = ActionCondition.ResearchEqualTo;
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    if (operatorChar == '≤')
                    {
                        numValue++;
                    }
                    else if (operatorChar == '≥')
                    {
                        numValue--;
                    }
                    condition.NumericalCheck = numValue;
                }
            }
            // Pathway Type conditions (only with = operator)
            else if ((variableName.Contains("type") || variableName.Contains("pathway")) && operatorChar == EQ_CHAR)
            {
                condition.Condition = ActionCondition.PathwayType;
                condition.StrCheck = valueStr;
            }
            // Generic string comparison (for future extensibility)
            else if (operatorChar == EQ_CHAR)
            {
                // Could be any string-based condition
                // For now, attempt to determine if it's a pathway type
                if (variableName.Contains("path") || variableName.Contains("type"))
                {
                    condition.Condition = ActionCondition.PathwayType;
                }
                else
                {
                    Debug.LogWarning("[CardUtility] Unknown string condition variable: " + variableName);
                    condition.Condition = ActionCondition.None;
                }
                condition.StrCheck = valueStr;
            }
            else
            {
                condition.Condition = ActionCondition.None;
                Debug.LogWarning("[CardUtility] Could not parse condition: " + conditionStr);
            }

            return condition;
        }

        static private ActionTarget ParseActionTarget(string targetStr)
        {
            targetStr = targetStr.ToLower().Trim();

            // Support abbreviations
            switch (targetStr)
            {
                case "invasive":
                case "inv":
                    return ActionTarget.Invasive;
                case "predator":
                case "pred":
                    return ActionTarget.Predator;
                case "prey":
                    return ActionTarget.Prey;
                case "pathway":
                case "path":
                    return ActionTarget.Pathway;
                case "ecosystem":
                case "eco":
                    return ActionTarget.Ecosystem;
                case "nest":
                    return ActionTarget.Nest;
                case "awareness":
                case "aware":
                    return ActionTarget.Awareness;
                case "budget":
                    return ActionTarget.Budget;
                default:
                    Debug.LogWarning("[CardUtility] Unknown target type: " + targetStr);
                    return ActionTarget.Invasive; // default fallback
            }
        }

        static private ActionSpecificity ParseActionSpecificity(string specificityStr)
        {
            specificityStr = specificityStr.ToLower().Trim();

            switch (specificityStr)
            {
                case "specific":
                case "spec":
                    return ActionSpecificity.Specific;
                case "random":
                case "rand":
                    return ActionSpecificity.Random;
                case "all":
                    return ActionSpecificity.All;
                default:
                    Debug.LogWarning("[CardUtility] Unknown specificity: " + specificityStr);
                    return ActionSpecificity.Specific; // default fallback
            }
        }

        static private ActionVerb ParseActionVerb(string verbStr)
        {
            verbStr = verbStr.ToLower().Trim();

            switch (verbStr)
            {
                case "reduce":
                case "red":
                    return ActionVerb.Reduce;
                case "increase":
                case "inc":
                    return ActionVerb.Increase;
                case "remove":
                case "rem":
                    return ActionVerb.Remove;
                case "reveal":
                case "rev":
                    return ActionVerb.Reveal;
                case "addtrap":
                case "trap":
                    return ActionVerb.AddTrap;
                case "addnest":
                case "nest":
                    return ActionVerb.AddNest;
                case "modify":
                case "mod":
                    return ActionVerb.Modify;
                case "match":
                    return ActionVerb.Match;
                default:
                    Debug.LogWarning("[CardUtility] Unknown verb: " + verbStr);
                    return ActionVerb.Reduce; // default fallback
            }
        }

        static private ModifierType ParseModifierType(string modifierStr)
        {
            modifierStr = modifierStr.ToLower().Trim();

            switch (modifierStr)
            {
                case "fixed":
                case "fix":
                    return ModifierType.Fixed;
                case "ratio":
                case "rat":
                case "percent":
                case "percentage":
                    return ModifierType.Ratio;
                default:
                    Debug.LogWarning("[CardUtility] Unknown modifier type: " + modifierStr);
                    return ModifierType.Fixed; // default fallback
            }
        }

        #endregion // Effect Parsing Helpers

        static public List<ActionCardData> GetUnlockedCards(ActionCardsState state)
        {
            List<ActionCardData> unlockedCards = new List<ActionCardData>();

            foreach (var cardId in state.UnlockedActionCards)
            {
                unlockedCards.Add(state.AllActionCards[cardId]);
            }

            Debug.Log("[Action Cards State] num unlocked cards: " + unlockedCards.Count);

            return unlockedCards;
        }

        static public List<ActionCardData> GetCardsFromEvidence(ActionCardsState state, List<StringHash32> evidenceCardIds)
        {
            List<ActionCardData> relevantCards = new List<ActionCardData>();
            EvidenceToActionConverterState converterState = Find.State<EvidenceToActionConverterState>();

            foreach (var evidenceCardId in evidenceCardIds)
            {
                // lookup evidence card  and extract action cards it unlocks
                List<SerializedHash32> actionIds = EvidenceActionConvertUtility.ConvertEvidenceToActionCardIds(converterState, evidenceCardId);
                // add each action card here
                foreach (var actionId in actionIds)
                {
                    var actionCard = state.AllActionCards[actionId];
                    if (relevantCards.Contains(actionCard))
                    {
                        // TODO: special handling for duplicates?
                    }
                    relevantCards.Add(actionCard);
                }
            }

            return relevantCards;
        }

        static public List<ActionCardData> GetAllCards(ActionCardsState state)
        {
            List<ActionCardData> relevantCards = new List<ActionCardData>();
            EvidenceToActionConverterState converterState = Find.State<EvidenceToActionConverterState>();

            foreach (var key in state.AllActionCards.Keys)
            {
                relevantCards.Add(state.AllActionCards[key]);
            }

            return relevantCards;
        }

        [LeafMember("UnlockCard")]
        static public void UnlockCardLeaf(StringHash32 cardId)
        {
            UnlockCard(Game.SharedState.Get<ActionCardsState>(), cardId);
        }

        [LeafMember("NumCardsUnlocked")]
        static public int NumCardsUnlocked()
        {
            return Game.SharedState.Get<ActionCardsState>().UnlockedActionCards.Count;
        }

        static public void UnlockCard(ActionCardsState state, StringHash32 id)
        {
            if (!state.UnlockedActionCards.Contains(id))
            {
                state.UnlockedActionCards.Add(id);
            }
        }

        [DebugMenuFactory]
        static private DMInfo ActionCardUnlockDebugMenu()
        {
            DMInfo info = new DMInfo("Action Cards");
            info.AddButton("Unlock All Cards", () => {
                var c = Game.SharedState.Get<ActionCardsState>();
                foreach (var cardId in c.AllActionCards.Keys)
                {
                    ActionCardsUtility.UnlockCard(c, cardId);
                }
            }, () => Game.SharedState.TryGet(out ActionCardsState c));

            info.AddButton("Print Num Unlocked", () => {
                var c = Game.SharedState.Get<ActionCardsState>();
                GetUnlockedCards(c);
            }, () => Game.SharedState.TryGet(out ActionCardsState c));

            return info;
        }
    }
}