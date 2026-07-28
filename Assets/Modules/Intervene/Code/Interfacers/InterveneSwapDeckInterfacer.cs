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
        public Image BG;
        public Button SwapBtn;
        public Button ConfirmSwapBtn;
        public int SwapCost;

        [Header("Visuals")]
        public ActionCardDeckWidget SwapDeckWidget;
        public TweenSettings CardTravelAnim = new TweenSettings(0.2f, Curve.Smooth);
        public bool AnimateHandCard = true;

        [HideInInspector] public bool hasSwapped = false;

        // True while the swap-from-deck panel is open. Used to block "Use Selected" until the
        // player completes or closes the swap.
        public bool IsSwapDeckOpen => SwapDeckWidget != null && SwapDeckWidget.gameObject.activeSelf;

        private bool m_SwapArmed;
        private StringHash32 m_OrigHandId;
        private StringHash32 m_OrigDeckId;
        private int m_HandIndex;
        private readonly List<StringHash32> m_DeckIds = new List<StringHash32>(24);

        private Routine m_DeckAnimRoutine;
        private Routine m_HandAnimRoutine;
        private RectTransform m_AnimatingHandCard;
        private Vector2 m_HandCardHome;

        private void Awake()
        {
            Instance = this;
            SwapDeckWidget.gameObject.SetActive(false);
            ConfirmSwapBtn.gameObject.SetActive(false);

            BG.gameObject.SetActive(true);

            SwapBtn.GetComponent<Button>().onClick.AddListener(SwapCardOnclick);
            ConfirmSwapBtn.GetComponent<Button>().onClick.AddListener(ConfirmSwapOnclick);
        }

        private void Update()
        {
            if (!SwapDeckWidget.gameObject.activeSelf) { return; }

            PlayerHand hand = CardInteractionMgr.Instance.Hand;

            SwapBtn.interactable = !hasSwapped
                && !hand.SelectionLocked
                && BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, SwapCost);
            ConfirmSwapBtn.interactable = m_SwapArmed;

            int highlight = m_SwapArmed ? m_HandIndex
                : (hand.SelectedCardIndices.Count > 0 ? hand.SelectedCardIndices[0] : -1);

            Transform container = hand.Visuals.CardContainer;
            for (int i = 0; i < container.childCount; i++)
            {
                UICard card = container.GetChild(i).GetComponent<UICard>();
                if (card == null) { continue; }

                HighlightCard(card, i == highlight);
            }

            // locked into the current card while specifying effects -- no swapping allowed
            if (hasSwapped || m_SwapArmed || hand.SelectionLocked) { return; }

            if (SwapDeckWidget.HasFocusedCard && hand.SelectedCardIndices.Count > 0)
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

            StopTravelAnimations();
            RectTransform handCardRect = HandCardRect(m_HandIndex);
            RectTransform focusedRect = (RectTransform)SwapDeckWidget.FocusSlot.transform;
            Vector2 handStart = CardTravelUtility.AnchoredPositionOver(handCardRect, focusedRect);

            SetHandCard(m_HandIndex, m_OrigDeckId);

            int deckIndex = m_DeckIds.IndexOf(m_OrigDeckId);
            if (deckIndex >= 0) { m_DeckIds[deckIndex] = m_OrigHandId; }
            SwapDeckWidget.Populate(m_DeckIds);
            SwapDeckWidget.LayoutStackedCards();

            AnimateIncomingDeckCard(m_OrigHandId, handCardRect);
            AnimateHandCardFrom(handCardRect, handStart);

            m_SwapArmed = true;
            hand.ClearSelections();
            ApplyHandVisuals(m_HandIndex);

            // Unlock all other cards in swap deck
            hand.OnCardClickedOverride = (id) => {
                    if (m_SwapArmed && id == m_OrigDeckId) { CancelSwap(); }
                };
            ResetCardStates();

            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                var pointer = card.GetComponent<FieldNoteCardPointer>();
                if (pointer != null)
                { 
                    pointer.OnClick = null; 
                    pointer.OnEnter = null;
                    pointer.OnExit = null;
                }

                bool isSwapped = card.CardID == m_OrigHandId;
                GrayOutCard(card, !isSwapped);
                HighlightCard(card, isSwapped);

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

            StopTravelAnimations();

            RectTransform handCardRect = HandCardRect(m_HandIndex);
            RectTransform deckCardRect = FindDeckCardRect(m_OrigHandId);
            Vector2 handStart = deckCardRect != null ? CardTravelUtility.AnchoredPositionOver(handCardRect, deckCardRect)
                : handCardRect.anchoredPosition;

            SetHandCard(m_HandIndex, m_OrigHandId);
            int deckIdx = m_DeckIds.IndexOf(m_OrigHandId);
            if (deckIdx >= 0) { m_DeckIds[deckIdx] = m_OrigDeckId; }

            SwapDeckWidget.Populate(m_DeckIds);
            SwapDeckWidget.LayoutStackedCards();

            AnimateIncomingDeckCard(m_OrigDeckId, handCardRect);
            AnimateHandCardFrom(handCardRect, handStart);

            m_SwapArmed = false;
            hand.OnCardClickedOverride = null;
            ApplyHandVisuals(-1);

            ResetCardStates();
        }

        private void ConfirmSwapOnclick()
        {
            if (!m_SwapArmed) { return; }
            StopTravelAnimations();
            BudgetUtility.Spend(InterveneBudgetInterfacer.Instance, SwapCost);

            m_SwapArmed = false;
            hasSwapped = true;
            SwapBtn.interactable = false;

            CardInteractionMgr.Instance.Hand.OnCardClickedOverride = null;
            SwapDeckWidget.ClearFocus();
            ResetCardStates();
            ApplyHandVisuals(-1);

            SwapDeckWidget.gameObject.SetActive(false);
            ConfirmSwapBtn.gameObject.SetActive(false);
            BG.gameObject.SetActive(true);
        }

        private void SwapCardOnclick()
        {
            // locked into the current card while specifying effects -- can't open the swap deck
            if (CardInteractionMgr.Instance.Hand.SelectionLocked) { return; }

            StopTravelAnimations();
            if (SwapDeckWidget.gameObject.activeSelf)
            {
                if (m_SwapArmed)
                { 
                    CancelSwap();
                    ApplyHandVisuals(-1);
                }
                SwapDeckWidget.gameObject.SetActive(false);
                ConfirmSwapBtn.gameObject.SetActive(false);
                BG.gameObject.SetActive(true);
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
                SwapDeckWidget.LayoutStackedCards();
                ResetCardStates();

                SwapDeckWidget.gameObject.SetActive(true);
                ConfirmSwapBtn.gameObject.SetActive(true);
                BG.gameObject.SetActive(false);
                ConfirmSwapBtn.interactable = false;
            }
        }

        public void LoadSwapDeck(IEnumerable<StringHash32> cardIds)
        {
            m_DeckIds.Clear();
            m_DeckIds.AddRange(cardIds);
            SwapDeckWidget.Populate(m_DeckIds);
            SwapDeckWidget.LayoutStackedCards();

            ResetCardStates();
        }

        #region Helpers

        private RectTransform HandCardRect(int index)
        {
            return (RectTransform)CardInteractionMgr.Instance.Hand.Visuals.CardContainer.GetChild(index);
        }

        private RectTransform FindDeckCardRect(StringHash32 id)
        {
            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                if (card.CardID == id) { return (RectTransform)card.transform; }
            }
            return null;
        }

        private void HighlightCard(UICard card, bool toHighlight)
        {
            Transform highlight = card.transform.Find("Highlight");
            if (highlight == null) { return; }
            highlight.gameObject.SetActive(toHighlight);
        }

        private void GrayOutCard(UICard card, bool toGrayOut)
        {
            CanvasGroup group = card.GetComponent<CanvasGroup>();
            if (group == null)
            {
                if (!toGrayOut) { return; }
                group = card.gameObject.AddComponent<CanvasGroup>();
            }

            group.alpha = toGrayOut ? 0.4f : 1f;
        }
        private void ResetCardStates()
        {
            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                GrayOutCard(card, false);
                HighlightCard(card, false);

                Button btn = card.GetComponentInChildren<Button>(true);
                if (btn != null) { btn.interactable = true; }
            }
        }

        private void ApplyHandVisuals(int highlightIndex)
        {
            Transform container = CardInteractionMgr.Instance.Hand.Visuals.CardContainer;

            for (int i = 0; i < container.childCount; i++)
            {
                UICard card = container.GetChild(i).GetComponent<UICard>();
                if (card == null) { continue; }

                bool isSwapped = i == highlightIndex;
                GrayOutCard(card, highlightIndex >= 0 && !isSwapped);
                HighlightCard(card, isSwapped);
            }
        }

        private void StopTravelAnimations()
        {
            m_DeckAnimRoutine.Stop();
            m_HandAnimRoutine.Stop();
            if (m_AnimatingHandCard != null)
            {
                m_AnimatingHandCard.anchoredPosition = m_HandCardHome;
                m_AnimatingHandCard = null;
            }
        }

        private void AnimateIncomingDeckCard(StringHash32 id, RectTransform sourceRect)
        {
            int index = 0;
            foreach (UICard card in SwapDeckWidget.ActionCardPool.ActiveObjects)
            {
                if (card.CardID == id)
                {
                    RectTransform rect = (RectTransform)card.transform;
                    Vector2 slot = CardTravelUtility.StackedPosition(index, SwapDeckWidget.StackedCardSpacing);
                    rect.anchoredPosition = CardTravelUtility.AnchoredPositionOver(rect, sourceRect);
                    rect.SetAsLastSibling();
                    m_DeckAnimRoutine.Replace(this, SlideDeckCardIntoSlot(rect, slot, index));
                    return;
                }
                index++;
            }
        }

        private IEnumerator SlideDeckCardIntoSlot(RectTransform rect, Vector2 slot, int sibling)
        {
            yield return rect.AnchorPosTo(slot, CardTravelAnim);
            rect.SetSiblingIndex(sibling);
        }

        private void AnimateHandCardFrom(RectTransform handCardRect, Vector2 startPos)
        {
            if (!AnimateHandCard) { return; }
            m_HandCardHome = handCardRect.anchoredPosition;
            m_AnimatingHandCard = handCardRect;
            handCardRect.anchoredPosition = startPos;
            m_HandAnimRoutine.Replace(this, SlideHandCardHome(handCardRect));
        }

        private IEnumerator SlideHandCardHome(RectTransform rect)
        {
            yield return rect.AnchorPosTo(m_HandCardHome, CardTravelAnim);
            m_AnimatingHandCard = null;
        }

        #endregion
    }
}