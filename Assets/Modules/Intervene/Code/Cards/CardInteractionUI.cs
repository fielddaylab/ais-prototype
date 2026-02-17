using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class CardInteractionUI : MonoBehaviour
    {
        public Button ShuffleActionDeckBtn;
        public Button DrawFromDeckBtn;
        public Button UseSelectedBtn;
        public Button RecycleDiscardBtn;

        public void Awake()
        {
            ShuffleActionDeckBtn.onClick.AddListener(HandleShuffleActionDeckClicked);
            DrawFromDeckBtn.onClick.AddListener(HandleDrawClicked);
            UseSelectedBtn.onClick.AddListener(HandleUseClicked);
            RecycleDiscardBtn.onClick.AddListener(HandleRecycleClicked);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ShuffleActionDeckBtn.onClick.RemoveAllListeners();
            DrawFromDeckBtn.onClick.RemoveAllListeners();
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
            if (CardInteractionMgr.Instance.Hand.GetSelectedCards().Count == 0) { return; }

            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyBegin);
        }

        private void HandleRecycleClicked()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnRecycleDiscard);
        }

        #endregion // Handlers
    }
}