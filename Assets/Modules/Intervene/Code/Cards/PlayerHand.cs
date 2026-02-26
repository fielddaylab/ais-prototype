using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class PlayerHand : CardStack
    {
        public List<int> SelectedCardIndices = new List<int>();
        public bool AllowMultiSelect = false;

        public void ToggleSelectAtIndex(int index)
        {
            if (SelectedCardIndices.Contains(index))
            {
                DeselectCard(index);
            }
            else if (AllowMultiSelect)
            {
                MultiSelectCard(index);
            }
            else
            {
                SingleSelectCard(index);
            }

            UpdateSelectVisuals();
            AisGame.Events.Dispatch(InterveneEvents.OnUiSelected);
        }

        public void SingleSelectCard(int index)
        {
            SelectedCardIndices.Clear();
            SelectedCardIndices.Add(index);
        }

        public void MultiSelectCard(int index)
        {
            if (SelectedCardIndices.Contains(index)) { return; }

            SelectedCardIndices.Add(index);
        }

        public void DeselectCard(int index)
        {
            if (!SelectedCardIndices.Contains(index)) { return; }

            SelectedCardIndices.Remove(index);
        }

        public void ClearSelections()
        {
            SelectedCardIndices.Clear();

            UpdateSelectVisuals();
        }

        private void UpdateSelectVisuals()
        {
            for (int i = 0; i < Visuals.CardVisuals.Count; i++)
            {
                Visuals.CardVisuals[i].Highlight.enabled = SelectedCardIndices.Contains(i);
            }
        }

        #region Queries

        public List<CardBase> GetSelectedCards()
        {
            List<CardBase> selected = new List<CardBase>();

            foreach(var index in SelectedCardIndices)
            {
                selected.Add(Cards[index]);
            }

            return selected;
        }

        #endregion // Queries
    }
}