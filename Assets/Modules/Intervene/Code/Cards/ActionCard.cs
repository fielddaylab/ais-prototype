using AIS.Model;
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
        Reveal,
        AddTrap,
    }

    [Flags]
    public enum ActionTarget
    {
        None,
        Invasive,
        Predator,
        Prey,
        Pathway,
        Ecosystem,
        Nest,
        Awareness,
        Budget
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
        public ActionSpecificity Specificity;
        public float NumTargets; // up to
        public ActionTargetCondition[] Conditions;
    }

    public struct ActionVerbDetails
    {
        public ActionVerb Verb;
        public float Value;
        public ModifierType ModType;
    }

    public struct ActionEffect
    {
        public ActionTargetDetails[] AllTargets;
        public ActionVerbDetails[] Verbs;
    }

    #endregion Structs & Enums

    public class ActionCard : CardBase
    {
        public string Title;
        public string Description;
        public string ImgPath;

        public int Cost;
        public ActionEffect[] Effects;

        public override void PopulateCardUI(UICard toPopulate)
        {
            toPopulate.Title.SetText(Title);
            toPopulate.Description.SetText(Description);
            // TODO: img
            // toPopulate.Img.SetText(Title);
        }
    }

    public static class ActionCardUtility
    {
        public static bool Evaluate(ActionTargetCondition condition, ModelTag tag)
        {
            switch (condition.Condition)
            {
                case ActionCondition.PathwayType:
                    return EvaluatePathway(condition, tag);
                // TODO: many more
                default:
                    Debug.LogWarning("[ActionCard] No condition matching to evaluate " + condition.Condition.ToString() + "!");
                    return true;
            }
        }

        private static bool EvaluatePathway(ActionTargetCondition condition, ModelTag tag)
        {
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
                }
            }

            return false;
        }
    }
}