using BeauRoutine;
using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        //public TMP_Text LevelValueText;
        //public TMP_Text ValueText;

        public int StartingBudget;
        public GameObject BudgetUnit;
        public Transform BudgetGroupTransform;

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
            // LoadPlayerBudget(StartingBudget);
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

            if (amt > 0)
            {
                for (int i = 0; i < amt; i++)
                {
                    Instantiate(BudgetUnit, BudgetGroupTransform);
                }
            }
            else
            {
                for (int i = 0; i < -amt && BudgetGroupTransform.childCount > 0; i++)
                {
                    Transform child = BudgetGroupTransform.GetChild(BudgetGroupTransform.childCount - 1);
                    child.SetParent(null);      // Destroy is deferred; detach so childCount is correct now
                    Destroy(child.gameObject);
                }
            }
        }

        public void ClearBudget()
        {
            AdjustBudgetLevel(-WorkingBudget.BudgetLevel);
            AdjustBudgetValue(-WorkingBudget.Budget);
        }

        public void AdjustBudgetValue(int amt)
        {
            int newBudget = Mathf.Clamp(WorkingBudget.Budget + amt, 0, WorkingBudget.BudgetLevel);
            amt = newBudget - WorkingBudget.Budget;

            if (amt < 0) // spend amt units of budget 
            {
                for (int i = WorkingBudget.Budget - 1; i >= WorkingBudget.Budget + amt; i--)
                {
                    var img = BudgetGroupTransform.GetChild(i).GetComponentInChildren<Image>();
                    if (img != null) img.color = Color.grey;
                }
            }

            WorkingBudget.Budget = newBudget;

            if (amt >= 0)
            {
                for (int i = WorkingBudget.Budget - 1; i > WorkingBudget.Budget - 1 - amt; i--)
                {
                    var img = BudgetGroupTransform.GetChild(i).GetComponentInChildren<Image>();
                    if (img != null) img.color = Color.yellow;
                }
            }
        }

        public void BestowBudget()
        {
            AdjustBudgetValue(WorkingBudget.BudgetLevel - WorkingBudget.Budget);
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
                totalCost += card.GetAdjustedCost();
            }

            return totalCost <= budget.WorkingBudget.Budget;
        }

        public static bool CanAfford(InterveneBudgetInterfacer budget, int cost)
        {
            return budget.WorkingBudget.Budget >= cost;
        }

        public static void Spend(InterveneBudgetInterfacer budget, int amt)
        {
            budget.Spend(amt);
        }
    }
}