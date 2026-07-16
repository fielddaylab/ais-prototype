using AIS.Narrative;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using OGD;
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

        private bool m_SwapArmed;
        private StringHash32 m_OrigHandId;
        private StringHash32 m_OrigDeckId;
        private int m_HandIndex;
        private readonly List<StringHash32> m_DeckIds = new List<StringHash32>(24);

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

            SwapBtn.interactable = !hasSwapped 
                && BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, SwapCost);
            ConfirmSwapBtn.interactable = m_SwapArmed;

            if (hasSwapped || m_SwapArmed) { return; }

            if (SwapDeckWidget.HasFocusedCard && CardInteractionMgr.Instance.Hand.SelectedCardIndices.Count > 0)
            {
                PerformSwap();
            }
        }

        private void PerformSwap()
        {
            PlayerHand hand = CardInteractionMgr.Instance.Hand;

            m_HandIndex = hand.SelectedCardIndices[0];
            m_OrigHandId = hand.PlayerCards[m_HandIndex].CardID;
            m_OrigDeckId = SwapDeckWidget.FocusSlot.CardID;

            SetHandCard(m_HandIndex, m_OrigDeckId);

            int deckIndex = m_DeckIds.IndexOf(m_OrigDeckId);
            if (deckIndex >= 0) { m_DeckIds[deckIndex] = m_OrigHandId; }
            SwapDeckWidget.Populate(m_DeckIds);

            m_SwapArmed = true;
            hand.ClearSelections();

            // Unlock all other cards in swap deck
            hand.OnCardClickedOverride = (id) => {
                    if (m_SwapArmed && id == m_OrigDeckId) { CancelSwap(); }
                };

            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                var pointer = card.GetComponent<FieldNoteCardPointer>();
                if (pointer != null)
                { 
                    pointer.OnClick = null; 
                    pointer.OnEnter = null;
                    pointer.OnExit = null;
                }

                Button btn = card.GetComponentInChildren<Button>(true);
                if (btn == null) { continue; }

                if (card.CardID == m_OrigHandId)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(CancelSwap);
                    btn.interactable = true;
                }
                else
                {
                    btn.onClick.RemoveAllListeners();
                    btn.interactable = false;
                }
            }
        }

        private void SetHandCard(int index, StringHash32 cardId)
        {
            PlayerHand hand = CardInteractionMgr.Instance.Hand;
            if (!Game.SharedState.TryGet(out ActionCardsState actionCards)) { return; }
            if (!actionCards.AllActionCards.TryGetValue(cardId, out ActionCardData actionCardData)) { return; }

            UICard cardObj = hand.Visuals.CardContainer.GetChild(index).GetComponent<UICard>();
            ActionCardUtility.PopulateCardUI(cardObj, actionCardData);
            hand.PlayerCards[index].PopulateFromData(actionCardData);
        }

        private void CancelSwap()
        {
            PlayerHand hand = CardInteractionMgr.Instance.Hand;

            SetHandCard(m_HandIndex, m_OrigHandId);
            int deckIdx = m_DeckIds.IndexOf(m_OrigHandId);
            if (deckIdx >= 0) { m_DeckIds[deckIdx] = m_OrigDeckId; }
            SwapDeckWidget.Populate(m_DeckIds);


            m_SwapArmed = false;
            hand.OnCardClickedOverride = null;

            // unlock all cards in swap deck for future swaps
            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                Button btn = card.GetComponentInChildren<Button>(true);
                if (btn != null) { btn.interactable = true; }
            }
        }

        private void ConfirmSwapOnclick()
        {
            if (!m_SwapArmed) { return; }
            BudgetUtility.Spend(InterveneBudgetInterfacer.Instance, SwapCost);

            m_SwapArmed = false;
            hasSwapped = true;
            SwapBtn.interactable = false;

            CardInteractionMgr.Instance.Hand.OnCardClickedOverride = null;
            SwapDeckWidget.ClearFocus();
            SwapDeckWidget.gameObject.SetActive(false);
            ConfirmSwapBtn.gameObject.SetActive(false);
        }

        private void SwapCardOnclick()
        {
            if (SwapDeckWidget.gameObject.activeSelf)
            {
                if (m_SwapArmed) { CancelSwap(); }
                SwapDeckWidget.gameObject.SetActive(false);
                ConfirmSwapBtn.gameObject.SetActive(false);
            }

            else
            {
                PlayerHand hand = CardInteractionMgr.Instance.Hand;
                HashSet<StringHash32> inHand = new HashSet<StringHash32>();
                foreach(ActionCard card in hand.PlayerCards) { inHand.Add(card.CardID); }

                for (int i = m_DeckIds.Count - 1; i >= 0; i--)
                {
                    if (inHand.Contains(m_DeckIds[i]))
                    {
                        m_DeckIds.RemoveAt(i);
                    }
                }
                SwapDeckWidget.Populate(m_DeckIds);

                SwapDeckWidget.gameObject.SetActive(true);
                ConfirmSwapBtn.gameObject.SetActive(true);
                ConfirmSwapBtn.interactable = false;
            }
        }

        // Only for playtesting
        public void AddActionCards()
        {
            var inv = Find.State<PlayerInventory>();

            // For playtesting
            inv.ActionCards.Add("example-action-card-1");
            inv.ActionCards.Add("example-action-card-2");
            inv.ActionCards.Add("example-action-card-4");
            inv.ActionCards.Add("example-action-card-6");
            inv.ActionCards.Add("example-action-card-7");
            inv.ActionCards.Add("example-action-card-9");
        }

        public void LoadSwapDeck(IEnumerable<StringHash32> cardIds)
        {
            m_DeckIds.Clear();
            m_DeckIds.AddRange(cardIds);
            SwapDeckWidget.Populate(m_DeckIds);
        }
    }
}