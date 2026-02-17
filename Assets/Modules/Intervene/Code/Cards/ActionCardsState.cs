using FieldDay.SharedState;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BeauUtil;
using Leaf.Runtime;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using FieldDay.Scenes;
using BeauRoutine;

namespace AIS.Intervene
{
    public struct ActionCardData
    {
        public SerializedHash32 CardID;
        public string Title;
        public string Description;
        public string ImgPath;

        public float Cost;
        public ActionEffect[] Effects;

        public bool IsValid;

        public ActionCardData(SerializedHash32 cardID, string title, string desc, string imgPath, float cost, ActionEffect[] effects)
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

        private static readonly string TARGET_LINE = "target:";
        private static readonly string VERB_LINE = "verb:";
        private static readonly string IF_KEYWORD = "if";

        private static readonly string ENTRY_SEP = "::";

        private static readonly char GR_CHAR = '>';
        private static readonly string GREQ_CHAR = ">=";
        private static readonly char EQ_CHAR = '=';
        private static readonly char LE_CHAR = '<';
        private static readonly string LEEQ_CHAR = "<=";

        private static readonly char[] END_DELIMS = new char[] { '\r', '\n' };
        private static readonly char[] COMMA_DELIM = new char[] { ',' };

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
            string title = "";
            string desc = "";
            string imgPath = "";

            // Parse into data

            // First line must be card id
            cardID = cardDef.Substring(0, cardDef.IndexOfAny(END_DELIMS));
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
            float cost = 0;
            int costIndex = cardDef.ToLower().IndexOf(COST_TAG);
            if (costIndex != -1)
            {
                string afterCost = cardDef.Substring(costIndex);
                int offset = COST_TAG.Length;
                string costStr = cardDef.Substring(costIndex + offset, afterCost.IndexOfAny(END_DELIMS) - offset).Trim();

                if (!float.TryParse(costStr, out cost))
                {
                    Debug.LogWarning("[CardUtility] Could not parse cost value: " + costStr + ". Defaulting to 0.");
                    cost = 0;
                }
            }

            // Action Effects parsing
            ActionEffect[] effects = ParseEffects(cardDef);

            return new ActionCardData(cardID, title, desc, imgPath, cost, effects);
        }

        #region Effect Parsing Helpers

        static private ActionEffect[] ParseEffects(string cardDef)
        {
            List<ActionEffect> effects = new List<ActionEffect>();

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

                // Parse this effect block
                ActionEffect effect = ParseSingleEffect(effectBlock);
                effects.Add(effect);

                searchStart = effectIndex + EFFECT_TAG.Length;
            }

            return effects.ToArray();
        }

        static private ActionEffect ParseSingleEffect(string effectBlock)
        {
            List<ActionTargetDetails> targets = new List<ActionTargetDetails>();
            List<ActionVerbDetails> verbs = new List<ActionVerbDetails>();

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
            }

            ActionEffect effect = new ActionEffect
            {
                AllTargets = targets.ToArray(),
                Verbs = verbs.ToArray()
            };

            return effect;
        }

        static private ActionTargetDetails ParseTarget(string targetContent)
        {
            // Format: [type], [specificity], [count], [optional conditions]
            // Example: "invasive, specific, 1, if population < 5"

            string[] parts = targetContent.Split(COMMA_DELIM, StringSplitOptions.RemoveEmptyEntries);

            ActionTargetDetails targetDetails = new ActionTargetDetails();

            // Parse target type (required)
            if (parts.Length > 0)
            {
                targetDetails.Target = ParseActionTarget(parts[0].Trim());
            }

            // Parse specificity (required)
            if (parts.Length > 1)
            {
                targetDetails.Specificity = ParseActionSpecificity(parts[1].Trim());
            }

            // Check if next part is a number (count) or a condition
            int conditionStartIndex = 2;
            targetDetails.NumTargets = 1; // default

            if (parts.Length > 2)
            {
                string potentialNumber = parts[2].Trim().ToLower();
                // Check if this is NOT a condition (doesn't start with "if")
                if (!potentialNumber.StartsWith(IF_KEYWORD))
                {
                    if (float.TryParse(potentialNumber, out float count))
                    {
                        targetDetails.NumTargets = count;
                        conditionStartIndex = 3;
                    }
                }
            }

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
            // Format: [verb], [value], [modifierType]
            // Example: "reduce, 10, fixed" or "reveal" (no value/modifier)

            string[] parts = verbContent.Split(COMMA_DELIM, StringSplitOptions.RemoveEmptyEntries);

            ActionVerbDetails verbDetails = new ActionVerbDetails();

            // Parse verb type (required)
            if (parts.Length > 0)
            {
                verbDetails.Verb = ParseActionVerb(parts[0].Trim());
            }

            // Parse value (optional)
            if (parts.Length > 1)
            {
                if (float.TryParse(parts[1].Trim(), out float value))
                {
                    verbDetails.Value = value;
                }
            }

            // Parse modifier type (optional)
            if (parts.Length > 2)
            {
                verbDetails.ModType = ParseModifierType(parts[2].Trim());
            }
            else
            {
                verbDetails.ModType = ModifierType.Fixed; // default
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
                    // Note: You'll need to add PopulationGreaterThan to ActionCondition enum
                    // For now, using PopulationLessThan as placeholder
                    Debug.LogWarning("[CardUtility] Population > operator detected. Consider adding PopulationGreaterThan to ActionCondition enum.");
                    condition.Condition = ActionCondition.PopulationLessThan; // Placeholder
                }
                else if (operatorChar == EQ_CHAR)
                {
                    // Note: You'll need to add PopulationEquals to ActionCondition enum
                    Debug.LogWarning("[CardUtility] Population = operator detected. Consider adding PopulationEquals to ActionCondition enum.");
                    condition.Condition = ActionCondition.PopulationLessThan; // Placeholder
                }

                if (float.TryParse(valueStr, out float numValue))
                {
                    condition.NumericalCheck = numValue;
                }
            }
            // Awareness conditions
            else if (variableName.Contains("aware") || variableName.Contains("awareness"))
            {
                // Note: You'll need to add AwarenessLessThan, AwarenessGreaterThan, etc. to ActionCondition enum
                Debug.LogWarning("[CardUtility] Awareness condition detected. Consider adding Awareness-specific conditions to ActionCondition enum.");
                condition.Condition = ActionCondition.None; // Placeholder until enum is expanded

                if (float.TryParse(valueStr, out float numValue))
                {
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
                case "reveal":
                case "rev":
                    return ActionVerb.Reveal;
                case "addtrap":
                case "trap":
                    return ActionVerb.AddTrap;
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