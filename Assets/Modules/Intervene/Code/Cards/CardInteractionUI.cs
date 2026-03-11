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

        [Header("Messages")]
        public GameObject NotEnoughBudgetMsg;
        public Transform PopupScreen;
        private GameObject m_PopupPrefab;

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
            var selectedCards = CardInteractionMgr.Instance.Hand.GetSelectedCards();
            if (selectedCards.Count == 0) { return; }

            if (!BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, selectedCards))
            {
                ShowNotEnoughBudgetMsg();
                return; 
            }

            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyBegin);
        }

        private void HandleRecycleClicked()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnRecycleDiscard);
        }

        #endregion // Handlers

        private void ShowNotEnoughBudgetMsg()
        {
            if (m_PopupPrefab != null) { return; }
            m_PopupPrefab = Instantiate(NotEnoughBudgetMsg, PopupScreen);

            // Fix the canvas camera reference
            Canvas canvas = m_PopupPrefab.GetComponent<Canvas>();
            if (canvas != null) {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            }

            StopAllCoroutines();
            RectTransform msgRect = m_PopupPrefab.transform.GetChild(0).GetComponent<RectTransform>();
            StartCoroutine(PopupMsg(m_PopupPrefab, msgRect, 1f));
        }

        private IEnumerator PopupMsg(GameObject popup, RectTransform rt, float delay)
        {
            float elapsed = 0f;
            float duration = 0.1f;
            rt.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.SmoothStep(0f, 1f, t);
                rt.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            yield return new WaitForSeconds(delay);

            TMPro.TextMeshProUGUI text = popup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.1f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.SmoothStep(1f, 0f, t);
                rt.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            Destroy(popup);
            m_PopupPrefab = null;
        }
    }
}