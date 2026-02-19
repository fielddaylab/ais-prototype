using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AIS.Intervene
{
    public class InterveneBudgetInterfacer : MonoBehaviour, IReducible, IIncreasable
    {
        public struct InterveneBudget
        {
            public int Budget;
        }

        public TMP_Text ValueText;

        public InterveneBudget WorkingBudget = new InterveneBudget();

        public void LoadPlayerBudget(int budget)
        {
            WorkingBudget.Budget = budget;
        }

        public void AdjustBudget(int amt)
        {
            WorkingBudget.Budget += amt;

            ValueText.SetText("$" + WorkingBudget.Budget.ToStringLookup());
        }

        #region Interfaces

        public bool TryIncrease(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustBudget((int)amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int addAmt = Mathf.FloorToInt(WorkingBudget.Budget * amt);

                AdjustBudget(addAmt);

                return true;
            }

            return false;
        }

        public bool TryReduce(float amt, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustBudget(-(int)amt);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                int reduceAmt = Mathf.FloorToInt(WorkingBudget.Budget * amt);

                AdjustBudget((int)-reduceAmt);

                return true;
            }

            return false;
        }

        #endregion // Interfaces
    }
}