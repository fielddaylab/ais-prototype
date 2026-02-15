using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class PlayerHand : CardStack
    {
        public List<int> SelectedCardIndices = new List<int>();
        public bool AllowMultiSelect; // whether the player can select multiple cards simultaneously (combo possibility)

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