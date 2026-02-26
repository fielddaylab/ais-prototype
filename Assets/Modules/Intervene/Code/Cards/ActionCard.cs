using AIS.Model;
using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public string ImgPath;

        public ActionEffectBundle[] Effects;

        public override void PopulateCardUI(UICard toPopulate)
        {
            toPopulate.Title.SetText(Title);
            toPopulate.CostText.SetText("$" + GetAdjustedCost().ToStringLookup());
            toPopulate.Description.SetText(Description);
            toPopulate.CardData = this;
            // TODO: img
            // toPopulate.Img.SetText(Title);
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
        public static bool Evaluate(ActionTargetCondition condition, ModelTag tag = null)
        {
            switch (condition.Condition)
            {
                case ActionCondition.PathwayType:
                    return EvaluatePathway(condition, tag);
                // TODO: convert the below to the IComparable system
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
                default:
                    Debug.LogWarning("[ActionCard] No condition matching to evaluate " + condition.Condition.ToString() + "!");
                    return true;
            }
        }

        private static bool EvaluatePathway(ActionTargetCondition condition, ModelTag tag)
        {
            if (tag == null) { return false; }

            // check if pathway
            if (((tag.TargetType & ActionTarget.Pathway) != 0))
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
    }
}