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
}