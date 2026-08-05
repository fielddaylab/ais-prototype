using BeauUtil;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    [RequireComponent(typeof(Comparable))]
    public class InterveneBudgetInterfacer : MonoBehaviour, IReducible, IIncreasable, IMatchable, IComparable
    {
        #region Structs

        public struct InterveneBudget
        {
            public int BudgetLevel;  // how many units of budget the player is granted each round
            public int Budget;       // how many of those units are still unspent
        }

        #endregion // Structs

        public static InterveneBudgetInterfacer Instance;

        #region Inspector

        public int StartingBudget;
        public BudgetUnit BudgetUnitPrefab;
        public Transform BudgetGroupTransform;

        #endregion // Inspector

        [HideInInspector] public InterveneBudget WorkingBudget = new InterveneBudget();

        // The spawned units, ordered left to right. This list -- not the child count of
        // BudgetGroupTransform -- is the authority on which units exist, so a deferred Destroy
        // can never desync the display from WorkingBudget.
        private readonly List<BudgetUnit> m_Units = new List<BudgetUnit>();

        // What the player's current card selection would cost. Purely a preview: it reserves
        // units in the display without spending them.
        private int m_ReservedCost;

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        #endregion // Unity Callbacks

        /// <summary>
        /// Resets to a full budget at the given level. Falls back to StartingBudget when the
        /// caller has no level to hand in yet.
        /// </summary>
        public void LoadPlayerBudget(int budgetLevel)
        {
            ClearBudget();
            SetBudgetLevel(budgetLevel > 0 ? budgetLevel : StartingBudget);
            BestowBudget();
        }

        public void ClearBudget()
        {
            m_ReservedCost = 0;
            SetBudgetLevel(0);
        }

        #region Budget Level

        public void AdjustBudgetLevel(int amt)
        {
            SetBudgetLevel(WorkingBudget.BudgetLevel + amt);
        }

        /// <summary>
        /// Sets how many units of budget the player is granted each round, spawning or removing
        /// units to match. Unspent budget is clamped to the new level.
        /// </summary>
        public void SetBudgetLevel(int level)
        {
            WorkingBudget.BudgetLevel = Mathf.Max(0, level);

            SyncUnits();

            // Re-apply the current value so it stays inside the new level, and repaint.
            SetBudgetValue(WorkingBudget.Budget);
        }

        private void SyncUnits()
        {
            while (m_Units.Count > WorkingBudget.BudgetLevel)
            {
                int last = m_Units.Count - 1;
                BudgetUnit unit = m_Units[last];
                m_Units.RemoveAt(last);

                if (unit == null) { continue; }

                // Destroy is deferred; deactivate so the layout group drops it this frame.
                unit.gameObject.SetActive(false);
                Destroy(unit.gameObject);
            }

            while (m_Units.Count < WorkingBudget.BudgetLevel)
            {
                m_Units.Add(Instantiate(BudgetUnitPrefab, BudgetGroupTransform));
            }
        }

        #endregion // Budget Level

        #region Budget Value

        public void AdjustBudgetValue(int amt)
        {
            SetBudgetValue(WorkingBudget.Budget + amt);
        }

        /// <summary>
        /// Sets how much unspent budget the player has, clamped to the current budget level.
        /// </summary>
        public void SetBudgetValue(int value)
        {
            int clamped = Mathf.Clamp(value, 0, WorkingBudget.BudgetLevel);
            bool changed = clamped != WorkingBudget.Budget;

            WorkingBudget.Budget = clamped;

            RefreshDisplay();

            if (changed)
            {
                AisGame.Events.Dispatch(InterveneEvents.OnBudgetChanged);
            }
        }

        /// <summary>
        /// Refills the player's unspent budget up to their budget level.
        /// </summary>
        public void BestowBudget()
        {
            SetBudgetValue(WorkingBudget.BudgetLevel);
        }

        public void Spend(int amt)
        {
            // The reserved preview has been paid for; it is no longer pending.
            m_ReservedCost = 0;

            AdjustBudgetValue(-amt);
        }

        #endregion // Budget Value

        #region Display

        /// <summary>
        /// Previews the cost of the player's current selection, marking that many units as
        /// reserved (or flagging the whole remaining budget when the selection is unaffordable).
        /// Absolute rather than incremental, so repeated selections cannot leave stale units behind.
        /// </summary>
        public void SetReservedCost(int cost)
        {
            cost = Mathf.Max(0, cost);

            if (cost == m_ReservedCost) { return; }

            m_ReservedCost = cost;

            RefreshDisplay();
        }

        public void ClearReservedCost()
        {
            SetReservedCost(0);
        }

        /// <summary>
        /// Repaints every unit from scratch. The display is a pure function of the budget level,
        /// the unspent budget, and the reserved cost, so it cannot drift out of sync.
        /// </summary>
        private void RefreshDisplay()
        {
            bool unaffordable = m_ReservedCost > WorkingBudget.Budget;

            // Budget is spent right to left, so the reserved units are the rightmost unspent ones.
            int firstReserved = WorkingBudget.Budget - m_ReservedCost;

            for (int i = 0; i < m_Units.Count; i++)
            {
                if (m_Units[i] == null) { continue; }

                m_Units[i].SetState(GetUnitState(i, unaffordable, firstReserved));
            }
        }

        private BudgetUnitState GetUnitState(int index, bool unaffordable, int firstReserved)
        {
            if (index >= WorkingBudget.Budget) { return BudgetUnitState.Spent; }

            if (unaffordable) { return BudgetUnitState.Unaffordable; }

            if (m_ReservedCost > 0 && index >= firstReserved) { return BudgetUnitState.Reserved; }

            return BudgetUnitState.Available;
        }

        #endregion // Display

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
                int addAmt = Mathf.FloorToInt(WorkingBudget.BudgetLevel * amts[0]);

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
                int reduceAmt = Mathf.FloorToInt(WorkingBudget.BudgetLevel * amts[0]);

                AdjustBudgetLevel(-reduceAmt);

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
            SetBudgetValue((int)val);
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

            return CanAfford(budget, totalCost);
        }

        public static bool CanAfford(InterveneBudgetInterfacer budget, int cost)
        {
            return budget.WorkingBudget.Budget >= cost;
        }

        public static void Spend(InterveneBudgetInterfacer budget, int amt)
        {
            budget.Spend(amt);
        }

        /// <summary>
        /// Shows what the player's current selection would cost. Pass 0 (or nothing selected) to
        /// drop the preview.
        /// </summary>
        public static void PreviewSelectionCost(InterveneBudgetInterfacer budget, int cost)
        {
            budget.SetReservedCost(cost);
        }

        public static void ClearSelectionPreview(InterveneBudgetInterfacer budget)
        {
            budget.ClearReservedCost();
        }
    }
}
