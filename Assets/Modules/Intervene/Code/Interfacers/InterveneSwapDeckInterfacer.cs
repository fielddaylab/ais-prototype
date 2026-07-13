using AIS.Narrative;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AIS.Narrative.EvidenceDisplayPanel;

namespace AIS.Intervene
{
    public class InterveneSwapDeckInterfacer : MonoBehaviour
    {
        public static InterveneSwapDeckInterfacer Instance;
        public Button SwapBtn;
        public Button ConfirmSwapBtn;
        public int SwapCost;

        [Header("Visuals")]
        public ActionCardDeckWidget SwapDeckWidget;

        [HideInInspector] public bool hasSwapped = false;

        private void Awake()
        {
            Instance = this;
            SwapDeckWidget.gameObject.SetActive(false);
            ConfirmSwapBtn.gameObject.SetActive(false);
            SwapBtn.GetComponent<Button>().onClick.AddListener(SwapCardOnclick);
            ConfirmSwapBtn.GetComponent<Button>().onClick.AddListener(ConfirmSwapOnclick);
        }

        private void Update()
        {
            if (!SwapDeckWidget.gameObject.activeSelf) { return; }
            if (hasSwapped) { SwapBtn.interactable = false; }

            ConfirmSwapBtn.interactable = (!hasSwapped) && (SwapDeckWidget.HasFocusedCard)
                && (CardInteractionMgr.Instance.Hand.SelectedCardIndices.Count != 0)
                && BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, SwapCost);
        }

        private void ConfirmSwapOnclick()
        {
            PlayerHand hand = CardInteractionMgr.Instance.Hand;
            int toDiscardIdx = hand.SelectedCardIndices[0];
            ActionCard toDiscard = hand.PlayerCards[toDiscardIdx];

            UICard toDiscardObj = hand.Visuals.CardContainer.GetChild(toDiscardIdx).GetComponent<UICard>();

            ActionCardsState actionCards;
            if (!Game.SharedState.TryGet(out actionCards)) { return; }

            ActionCardData toInsertActionCardData;
            if (!actionCards.AllActionCards.TryGetValue(SwapDeckWidget.FocusSlot.CardID, out toInsertActionCardData)) { return; }

            ActionCardUtility.PopulateCardUI(toDiscardObj, toInsertActionCardData);
            BudgetUtility.Spend(InterveneBudgetInterfacer.Instance, SwapCost);
            //remove toInsertObj from SwapDeckWidget
            SwapDeckWidget.ClearFocus();
            hasSwapped = false;

            // disable future swaps
            SwapCardOnclick();
            SwapBtn.enabled = false;
        }

        private void SwapCardOnclick()
        {
            if (SwapDeckWidget.gameObject.activeSelf)
            {
                SwapDeckWidget.gameObject.SetActive(false);
                ConfirmSwapBtn.gameObject.SetActive(false);
            }

            else
            {
                SwapDeckWidget.gameObject.SetActive(true);
                ConfirmSwapBtn.gameObject.SetActive(true);
                ConfirmSwapBtn.interactable = false;
            }
        }

        // Only for playtesting
        public void AddActionCards()
        {
            var inv = Find.State<PlayerInventory>();
            inv.ActionCards.Add("example-action-card-1");
            inv.ActionCards.Add("example-action-card-9");
        }

        public void LoadSwapDeck(IEnumerable<StringHash32> cardIds)
        {
            SwapDeckWidget.Populate(cardIds);
        }
    }
}