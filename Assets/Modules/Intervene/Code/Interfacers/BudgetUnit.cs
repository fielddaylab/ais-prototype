using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    /// <summary>
    /// How a single unit of budget reads to the player.
    /// </summary>
    public enum BudgetUnitState
    {
        Spent,          // capacity the player does not have to spend this round
        Available,      // free to spend
        Reserved,       // claimed by the current card selection, but not yet paid
        Unaffordable    // the current selection costs more than what is left
    }

    /// <summary>
    /// One unit in the budget readout. Owns the visuals for a single unit of budget so
    /// InterveneBudgetInterfacer never has to walk the hierarchy to find and recolor it.
    /// </summary>
    public class BudgetUnit : MonoBehaviour
    {
        #region Inspector

        [Header("Components")]
        public Image Fill;

        [Header("State Colors")]
        public Color SpentColor = new Color(0.72955966f, 0.72955966f, 0.72955966f, 1f);
        public Color AvailableColor = Color.yellow;
        public Color ReservedColor = Color.white;
        public Color UnaffordableColor = Color.red;

        #endregion // Inspector

        [HideInInspector] public BudgetUnitState State = BudgetUnitState.Spent;

        public void SetState(BudgetUnitState state)
        {
            State = state;

            if (Fill == null) { return; }

            Fill.color = GetStateColor(state);
        }

        private Color GetStateColor(BudgetUnitState state)
        {
            switch (state)
            {
                case BudgetUnitState.Available:
                    return AvailableColor;
                case BudgetUnitState.Reserved:
                    return ReservedColor;
                case BudgetUnitState.Unaffordable:
                    return UnaffordableColor;
                default:
                    return SpentColor;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Convenience for duplicated / hand-built units: the fill is the unit's own graphic.
            if (Fill == null) { Fill = GetComponent<Image>(); }
        }
#endif // UNITY_EDITOR
    }
}
