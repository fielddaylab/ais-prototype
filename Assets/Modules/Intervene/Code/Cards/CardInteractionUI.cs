using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class CardInteractionUI : MonoBehaviour
    {
        public Button ShuffleActionDeckBtn;
        //public Button DrawFromDeckBtn;
        public Button UseSelectedBtn;
        public Button RecycleDiscardBtn;

        public void Awake()
        {
            ShuffleActionDeckBtn.onClick.AddListener(HandleShuffleActionDeckClicked);
            //DrawFromDeckBtn.onClick.AddListener(HandleDrawClicked);
            UseSelectedBtn.onClick.AddListener(HandleUseClicked);
            RecycleDiscardBtn.onClick.AddListener(HandleRecycleClicked);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ShuffleActionDeckBtn.onClick.RemoveAllListeners();
            //DrawFromDeckBtn.onClick.RemoveAllListeners();
            UseSelectedBtn.onClick.RemoveAllListeners();
            RecycleDiscardBtn.onClick.RemoveAllListeners();
        }

        #region Handlers

        private void HandleShuffleActionDeckClicked()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnShuffleActionDeck);
        }

        private void HandleDrawClicked()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnDrawFromActionDeck);
        }

        private void HandleUseClicked()
        {
            // Must finish or close a swap-from-deck before using a selected card.
            if (InterveneSwapDeckInterfacer.Instance != null && InterveneSwapDeckInterfacer.Instance.IsSwapDeckOpen) { return; }
            // Cannot use selected cards during sim phases
            if (InterveneUI.Instance.SimInProgress) { return; }

            var selectedCards = CardInteractionMgr.Instance.Hand.GetSelectedCards();
            if (selectedCards.Count == 0) { return; }

            if (!BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, selectedCards))
            {
                //UseSelectedBtn.interactable = false;
                return; 
            }

            //if (Game.SharedState.TryGet(out ActionCardsState cardsState))
            //{
            //    if (cardsState.AllActionCards.TryGetValue(selectedCards[0].CardID, out ActionCardData data))
            //    {
            //        BudgetUtility.Spend(InterveneBudgetInterfacer.Instance, data.Cost);
            //    }
            //}
            //CardInteractionMgr.Instance.Hand.ClearSelections();
            UseSelectedBtn.interactable = InterveneBudgetInterfacer.Instance.WorkingBudget.Budget > 0 ? true : false;

            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyBegin);
        }

        private void HandleRecycleClicked()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnRecycleDiscard);
        }

        #endregion // Handlers
    }
}