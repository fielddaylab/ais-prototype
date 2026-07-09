using AIS.Model;
using AIS.Narrative;
using AIS.Shared;
using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AIS.Intervene {
    #region Structs and Enums

    [Flags]
    public enum ActionVerb
    {
        Reduce,
        Increase,
        Remove,
        Reveal,
        AddTrap,
        AddNest,
        Modify,
        Match,
    }

    [Flags]
    public enum ActionTarget
    {
        None = 0x0,
        Invasive = 0x01,
        Predator = 0x02,
        Prey = 0x04,
        Pathway = 0x08,
        Ecosystem = 0x10,
        Nest = 0x20,
        Awareness = 0x40,
        Budget = 0x80,
        Trap = 0x100,
    }

    public enum ActionSpecificity
    {
        Specific,
        Random,
        All,
    }

    public enum ActionCondition
    {
        None,
        PopulationLessThan,
        PopulationEqualTo,
        PopulationGreaterThan,
        AwarenessLessThan,
        AwarenessEqualTo,
        AwarenessGreaterThan,
        PathwayType,
        // TODO: more below as needed
        PathwayEffectType,
        SocialLessThan,
        SocialEqualTo,
        SocialGreaterThan,
        OutdoorLessThan,
        OutdoorEqualTo,
        OutdoorGreaterThan,
        TechLessThan,
        TechEqualTo,
        TechGreaterThan,
        ResearchLessThan,
        ResearchEqualTo,
        ResearchGreaterThan,
        InnovateLessThan,
        InnovateEqualTo,
        InnovateGreaterThan,
        IsInput,
        IsOutput,
    }

    public enum ModifierType
    {
        Fixed,
        Ratio,
    }

    public struct ActionTargetCondition
    {
        public ActionCondition Condition;
        public float NumericalCheck;
        public string StrCheck;
    }

    public struct ActionTargetDetails
    {
        public ActionTarget Target;
        public ActionTargetCondition[] Conditions;
    }

    public struct ActionVerbDetails
    {
        public ActionVerb Verb;
        public List<float> Values;
        public ModifierType ModType;
        public float Odds; // odds of triggering
        public string RelativeId;
    }

    public struct ActionEffectBundle
    {
        public ActionEffect ActionEffect;
        public ActionEffectOverride EffectOverride;

        public List<ActionVerb> GetAllVerbs()
        {
            return ActionEffect.GetAllVerbs();
        }
    }

    public struct ActionEffectOverride
    {
        public ActionTargetCondition Condition;
        public ActionEffect Override;
        public string Description; // player-facing text describing the override ability
    }

    public struct ActionEffect
    {
        public ActionTargetDetails[] AllTargets;
        public ActionSpecificity Specificity;
        public float MaxTargets; // up to
        public ActionVerbDetails[] Verbs;
        public string HardCodedId; // TEMP system to define advanced card abilities in prototype

        public List<ActionVerb> GetAllVerbs()
        {
            List<ActionVerb> verbs = new List<ActionVerb>();
            foreach (var verb in Verbs)
            {
                if (!verbs.Contains(verb.Verb))
                {
                    verbs.Add(verb.Verb);
                }
            }

            return verbs;
        }

        public bool IsHardCoded()
        {
            return !HardCodedId.Equals(String.Empty);
        }
    }

    #endregion Structs & Enums

    public class ActionCard : CardBase
    {
        public string Title;
        public string Description;
        public string FocusDescription; // extra player-facing text shown in the field notes focus area
        public string ImgPath;
        public PlayerStatId Suit;

        public ActionEffectBundle[] DiscoverResults;
        public ActionEffectBundle[] Effects;

        public override void PopulateCardUI(UICard toPopulate)
        {
            toPopulate.Title.SetText(Title);
            toPopulate.CostText.SetText("$" + GetAdjustedCost().ToStringLookup());
            toPopulate.Description.SetText(Description);
            toPopulate.CardData = this;

            if (toPopulate.Suit != null)
            {
                toPopulate.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(Suit);
            }

            PopulateOverrideUI(toPopulate);
            // TODO: img
            // toPopulate.Img.SetText(Title);
        }

        public void PopulateFromData(in ActionCardData data)
        {
            CardID = data.CardID;      // adjust to CardBase's actual field names
            Title = data.Title;
            Description = data.Description;
            FocusDescription = data.FocusDescription;
            ImgPath = data.ImgPath;
            Suit = data.Suit;
            Effects = data.Effects;
            DiscoverResults = data.DiscoverResults;
        }

        private void PopulateOverrideUI(UICard toPopulate)
        {
            if (toPopulate.OverrideGroup == null) { return; }

            // Find the first effect that carries an override ability.
            ActionEffectOverride effectOverride = new ActionEffectOverride();
            effectOverride.Condition.Condition = ActionCondition.None;
            bool hasOverride = false;
            if (Effects != null)
            {
                foreach (var bundle in Effects)
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
                toPopulate.OverrideGroup.alpha = 0f;
                toPopulate.OverrideGroup.blocksRaycasts = false;
                toPopulate.OverrideGroup.interactable = false;
                return;
            }

            toPopulate.OverrideGroup.alpha = 1f;
            toPopulate.OverrideGroup.blocksRaycasts = true;
            toPopulate.OverrideGroup.interactable = true;

            // Suit = the stat being checked by the override condition.
            if (toPopulate.RequirementSuit != null)
            {
                toPopulate.RequirementSuit.sprite = CardVisualLookupUtility.LookupSuitIcon(
                    ActionCardUtility.GetConditionStat(condition.Condition));
            }

            // Number = the minimum value of that stat needed to trigger the ability.
            if (toPopulate.RequirementNumber != null)
            {
                toPopulate.RequirementNumber.SetText(
                    ActionCardUtility.GetConditionThreshold(condition).ToStringLookup());
            }

            // Description of the override ability (parsed from the "desc:" line in @overrideEffect).
            if (toPopulate.AdditionalDesc != null)
            {
                toPopulate.AdditionalDesc.SetText(effectOverride.Description);
            }
        }

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnInvasionLevelChanged, HandleInvasionLevelChanged);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnInvasionLevelChanged, HandleInvasionLevelChanged);
        }

        private void HandleInvasionLevelChanged()
        {

        }
    }

    public static class ActionCardUtility
    {
        // Populates a UICard directly from an ActionCardData struct (e.g. the field notes panel, which
        // works with parsed ActionCardData rather than runtime ActionCard instances).
        // Uses the static Cost from the data, since GetAdjustedCost() requires the Intervene-scene
        // InvasionCurveInterfacer singleton that is not present in other scenes.
        public static void PopulateCardUI(UICard toPopulate, in ActionCardData data)
        {
            if (toPopulate == null) { return; }

            toPopulate.CardID = data.CardID;
            toPopulate.Title.SetText(data.Title);
            toPopulate.CostText.SetText("$" + data.Cost.ToStringLookup());
            toPopulate.Description.SetText(data.Description);

            if (toPopulate.Suit != null)
            {
                toPopulate.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(data.Suit);
            }

            PopulateOverrideUI(toPopulate, data.Effects);
        }

        // Mirrors ActionCard.PopulateOverrideUI for the ActionCardData path.
        private static void PopulateOverrideUI(UICard toPopulate, ActionEffectBundle[] effects)
        {
            if (toPopulate.OverrideGroup == null) { return; }

            // Find the first effect that carries an override ability.
            ActionEffectOverride effectOverride = new ActionEffectOverride();
            effectOverride.Condition.Condition = ActionCondition.None;
            bool hasOverride = false;
            if (effects != null)
            {
                foreach (var bundle in effects)
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
                toPopulate.OverrideGroup.alpha = 0f;
                toPopulate.OverrideGroup.blocksRaycasts = false;
                toPopulate.OverrideGroup.interactable = false;
                return;
            }

            toPopulate.OverrideGroup.alpha = 1f;
            toPopulate.OverrideGroup.blocksRaycasts = true;
            toPopulate.OverrideGroup.interactable = true;

            if (toPopulate.RequirementSuit != null)
            {
                toPopulate.RequirementSuit.sprite = CardVisualLookupUtility.LookupSuitIcon(
                    GetConditionStat(condition.Condition));
            }

            if (toPopulate.RequirementNumber != null)
            {
                toPopulate.RequirementNumber.SetText(GetConditionThreshold(condition).ToStringLookup());
            }

            if (toPopulate.AdditionalDesc != null)
            {
                toPopulate.AdditionalDesc.SetText(effectOverride.Description);
            }
        }

        // Maps a stat-check condition to the player stat (suit) it checks.
        // Non-stat conditions (population, awareness, pathway, etc.) return Invalid.
        public static PlayerStatId GetConditionStat(ActionCondition condition)
        {
            switch (condition)
            {
                case ActionCondition.SocialLessThan:
                case ActionCondition.SocialEqualTo:
                case ActionCondition.SocialGreaterThan:
                    return PlayerStatId.Communicate;
                case ActionCondition.OutdoorLessThan:
                case ActionCondition.OutdoorEqualTo:
                case ActionCondition.OutdoorGreaterThan:
                    return PlayerStatId.Ranger;
                case ActionCondition.TechLessThan:
                case ActionCondition.TechEqualTo:
                case ActionCondition.TechGreaterThan:
                    return PlayerStatId.Tech;
                case ActionCondition.ResearchLessThan:
                case ActionCondition.ResearchEqualTo:
                case ActionCondition.ResearchGreaterThan:
                    return PlayerStatId.Research;
                case ActionCondition.InnovateLessThan:
                case ActionCondition.InnovateEqualTo:
                case ActionCondition.InnovateGreaterThan:
                    return PlayerStatId.Innovate;
                default:
                    return PlayerStatId.Invalid;
            }
        }

        // The minimum stat value that satisfies the condition.
        // e.g. social > 1 => 2, social == 1 => 1, social < 2 => 1.
        public static int GetConditionThreshold(ActionTargetCondition condition)
        {
            int check = (int)condition.NumericalCheck;

            switch (condition.Condition)
            {
                case ActionCondition.SocialGreaterThan:
                case ActionCondition.OutdoorGreaterThan:
                case ActionCondition.TechGreaterThan:
                case ActionCondition.ResearchGreaterThan:
                case ActionCondition.InnovateGreaterThan:
                    return check + 1;
                case ActionCondition.SocialLessThan:
                case ActionCondition.OutdoorLessThan:
                case ActionCondition.TechLessThan:
                case ActionCondition.ResearchLessThan:
                case ActionCondition.InnovateLessThan:
                    return check - 1;
                default:
                    // EqualTo (and any other) trigger exactly at the check value.
                    return check;
            }
        }

        public static bool Evaluate(ActionTargetCondition condition, ModelTag tag = null)
        {
            switch (condition.Condition)
            {
                case ActionCondition.PathwayType:
                    return EvaluatePathway(condition, tag);
                // TODO: convert the below to the IComparable system
                case ActionCondition.PathwayEffectType:
                    return EvaluatePathwayEffectType(condition, tag);
                case ActionCondition.PopulationLessThan:
                    return tag.QueriableObj.GetComponent<Cluster>().Population < condition.NumericalCheck;
                case ActionCondition.PopulationEqualTo:
                    return tag.QueriableObj.GetComponent<Cluster>().Population == condition.NumericalCheck;
                case ActionCondition.PopulationGreaterThan:
                    return tag.QueriableObj.GetComponent<Cluster>().Population > condition.NumericalCheck;
                case ActionCondition.AwarenessLessThan:
                    return InterveneAwarenessInterfacer.Instance.GetValue() < condition.NumericalCheck;
                case ActionCondition.AwarenessEqualTo:
                    return InterveneAwarenessInterfacer.Instance.GetValue() == condition.NumericalCheck;
                case ActionCondition.AwarenessGreaterThan:
                    return InterveneAwarenessInterfacer.Instance.GetValue() > condition.NumericalCheck;
                case ActionCondition.SocialLessThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.SOCIAL_KEY) < condition.NumericalCheck;
                case ActionCondition.SocialEqualTo:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.SOCIAL_KEY) == condition.NumericalCheck;
                case ActionCondition.SocialGreaterThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.SOCIAL_KEY) > condition.NumericalCheck;
                case ActionCondition.OutdoorLessThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.OUTDOOR_KEY) < condition.NumericalCheck;
                case ActionCondition.OutdoorEqualTo:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.OUTDOOR_KEY) == condition.NumericalCheck;
                case ActionCondition.OutdoorGreaterThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.OUTDOOR_KEY) > condition.NumericalCheck;
                case ActionCondition.TechLessThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.TECH_KEY) < condition.NumericalCheck;
                case ActionCondition.TechEqualTo:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.TECH_KEY) == condition.NumericalCheck;
                case ActionCondition.TechGreaterThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.TECH_KEY) > condition.NumericalCheck;
                case ActionCondition.ResearchLessThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.RESEARCH_KEY) < condition.NumericalCheck;
                case ActionCondition.ResearchEqualTo:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.RESEARCH_KEY) == condition.NumericalCheck;
                case ActionCondition.ResearchGreaterThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.RESEARCH_KEY) > condition.NumericalCheck;
                case ActionCondition.InnovateLessThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.INNOVATE_KEY) < condition.NumericalCheck;
                case ActionCondition.InnovateEqualTo:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.INNOVATE_KEY) == condition.NumericalCheck;
                case ActionCondition.InnovateGreaterThan:
                    return StatsInterfacer.Instance.GetValue(StatsInterfacer.INNOVATE_KEY) > condition.NumericalCheck;
                case ActionCondition.IsInput:
                case ActionCondition.IsOutput:
                    return EvaluatePathDir(condition, tag);
                default:
                    Debug.LogWarning("[ActionCard] No condition matching to evaluate " + condition.Condition.ToString() + "!");
                    return true;
            }
        }

        private static bool EvaluatePathway(ActionTargetCondition condition, ModelTag tag)
        {
            if (tag == null) { return false; }

            // check if pathway
            if ((tag.TargetType & ActionTarget.Pathway) != 0)
            {
                // check if type matches
                Pathway pathway = tag.QueriableObj.GetComponent<Pathway>();
                if (pathway != null)
                {
                    if ((pathway.PathwayType & PathwayUtility.StrToPathwayType(condition.StrCheck)) != 0)
                    {
                        return true;
                    }
                    else if (pathway.IsHidden && condition.StrCheck.Equals("unknown"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EvaluatePathwayEffectType(ActionTargetCondition condition, ModelTag tag)
        {
            if (tag == null) { return false; }

            // check if pathway
            if ((tag.TargetType & ActionTarget.Pathway) != 0)
            {
                // check if effectType matches
                Pathway pathway = tag.QueriableObj.GetComponent<Pathway>();
                if (pathway != null)
                {
                    foreach (var effect in pathway.OnTryMoveFromOrig)
                    {
                        if ((effect.EffectType & PathwayEffectType.Trapped) != 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool EvaluatePathDir(ActionTargetCondition condition, ModelTag tag)
        {
            if (tag == null) { return false; }

            // check if pathway
            if ((tag.TargetType & ActionTarget.Pathway) != 0)
            {
                // check if type matches
                Pathway pathway = tag.QueriableObj.GetComponent<Pathway>();
                if (pathway != null)
                {
                    if ((condition.Condition == ActionCondition.IsInput) && (pathway.Dir == PathDir.Input))
                    {
                        return true;
                    }
                    else if ((condition.Condition == ActionCondition.IsOutput) && (pathway.Dir == PathDir.Output))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}