using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Intervene
{
    [RequireComponent(typeof(Comparable))]
    public class InterveneBudgetInterfacer : MonoBehaviour, IReducible, IIncreasable, IMatchable, IComparable
    {
        #region Structs

        public struct InterveneBudget
        {
            public int BudgetLevel;
            public int Budget;
        }

        #endregion // Structs

        public static InterveneBudgetInterfacer Instance;

        #region Inspector

        public TMP_Text LevelValueText;
        public TMP_Text ValueText;
        public int StartingBudget;

        #endregion // Inspector

        [HideInInspector] public InterveneBudget WorkingBudget = new InterveneBudget();

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // TEMP
            LoadPlayerBudget(StartingBudget);
        }

        #endregion // Unity Callbacks

        public void LoadPlayerBudget(int budgetLevel)
        {
            ClearBudget();
            AdjustBudgetLevel(budgetLevel);
            BestowBudget();
        }

        public void AdjustBudgetLevel(int amt)
        {
            WorkingBudget.BudgetLevel += amt;

            LevelValueText.SetText("$" + WorkingBudget.BudgetLevel.ToStringLookup() + " per turn");
        }

        public void ClearBudget()
        {
            AdjustBudgetLevel(-WorkingBudget.BudgetLevel);
            AdjustBudgetValue(-WorkingBudget.Budget);
        }

        public void AdjustBudgetValue(int amt)
        {
            WorkingBudget.Budget += amt;

            ValueText.SetText("$" + WorkingBudget.Budget.ToStringLookup());
        }

        public void BestowBudget()
        {
            AdjustBudgetValue(WorkingBudget.BudgetLevel);
        }

        public void Spend(int amt)
        {
            AdjustBudgetValue(-amt);
        }


        #region Interfaces

        // IIncreasable
        public bool TryIncrease(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustBudgetLevel((int)amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(WorkingBudget.Budget * amts[0]);

                AdjustBudgetLevel(addAmt);

                return true;
            }

            return false;
        }

        // IReducible

        public bool TryReduce(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustBudgetLevel(-(int)amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int reduceAmt = Mathf.FloorToInt(WorkingBudget.Budget * amts[0]);

                AdjustBudgetLevel((int)-reduceAmt);

                return true;
            }

            return false;
        }

        // IMatchable

        public bool TryMatch(SerializedHash32 toMatch, SerializedHash32 toMatchWith, float modifier)
        {
            return ComparisonUtility.TryMatch(ComparisonFacilitator.Instance, toMatch, toMatchWith, modifier);
        }

        // IComparable

        public SerializedHash32 GetId()
        {
            return GetComponent<Comparable>().Id;
        }

        public float GetValue(string key = null)
        {
            return WorkingBudget.Budget;
        }

        public void SetValue(float val, string key = null)
        {
            AdjustBudgetValue((int)val - WorkingBudget.Budget);
        }

        #endregion // Interfaces

    }

    public static class BudgetUtility
    {
        public static bool CanAfford(InterveneBudgetInterfacer budget, List<CardBase> toAfford)
        {
            int totalCost = 0;

            foreach (var card in toAfford)
            {
                totalCost += card.Cost;
            }

            return totalCost <= budget.WorkingBudget.Budget;
        }

        public static void Spend(InterveneBudgetInterfacer budget, int amt)
        {
            budget.Spend(amt);
        }
    }
}