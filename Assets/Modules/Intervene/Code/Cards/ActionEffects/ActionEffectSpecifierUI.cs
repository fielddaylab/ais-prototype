using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class ActionEffectSpecifierUI : MonoBehaviour
    {
        public Button BeginEffectSpecifyBtn;

        public Button CancelAllBtn;
        public Button ConfirmBtn;

        private void Awake()
        {
            SetUIElementsActive(false);

            ConfirmBtn.onClick.AddListener(HandleConfirmClicked);
            CancelAllBtn.onClick.AddListener(HandleCancelAllClicked);

            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyAllActionsProcessed, HandleEffectSpecifyAllActionsProcessed);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ConfirmBtn.onClick.RemoveAllListeners();
            CancelAllBtn.onClick.RemoveAllListeners();
        }

        #region Handlers

        private void HandleEffectSpecifyBegin()
        {
            SetUIElementsActive(true);
        }

        private void HandleEffectSpecifyAllActionsProcessed()
        {
            ConfirmBtn.interactable = true;
        }

        private void HandleConfirmClicked()
        {
            SetUIElementsActive(false);

            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyConfirm);
        }

        private void HandleCancelAllClicked()
        {
            SetUIElementsActive(false);

            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyCancel);
        }

        #endregion // Handlers

        #region Helpers

        private void SetUIElementsActive(bool effectSpecifyActive)
        {
            ConfirmBtn.gameObject.SetActive(effectSpecifyActive);
            ConfirmBtn.interactable = false;
            CancelAllBtn.gameObject.SetActive(effectSpecifyActive);

            BeginEffectSpecifyBtn.gameObject.SetActive(!effectSpecifyActive);
        }

        #endregion // Helpers
    }
}