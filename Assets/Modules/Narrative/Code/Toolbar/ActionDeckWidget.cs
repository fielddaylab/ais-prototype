using AIS.Intervene;
using AIS.Model;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative
{
    public sealed class ActionCardDeckWidget : MonoBehaviour
    {
        [Serializable] public sealed class UICardPool : SerializablePool<UICard> { }

        [Header("Action Cards")]
        public UICardPool ActionCardPool;
        public RectTransform FocusedCard;
        private int _lastFocused = -1;
        public UICard FocusSlot;
        public GameObject FocusEmpty;
        public CardFocusView FocusView;
        public GameObject NoActionCardsDefault;
        public TMP_Text FocusDescriptionText;

        [Header("Deck Layout")]
        [Tooltip("Vertical distance between the tops of consecutive cards when the deck is spread downward as an overlapping stack.")]
        public float StackedCardSpacing = 48f;

        [Header("Focus Behavior")]
        [Tooltip("false = hover a deck card to preview it in the focus slot; true = click to focus/return/swap.")]
        public bool ClickToFocus = true;
        public TweenSettings FocusSlideAnim = new TweenSettings(0.2f, Curve.Smooth);

        private Routine m_FocusInRoutine;   // click mode: card traveling from the deck to the focus slot
        private Routine m_FocusOutRoutine;  // click mode: card traveling from the focus slot back to the deck
        [NonSerialized] public int m_FocusedCardIndex = -1;

        // Click mode: deck position the focused card returns to, captured when it was focused
        // (the card is at rest in the deck at that moment, so this also works for decks
        // positioned by a layout group rather than LayoutStackedCards).
        private Vector2 m_FocusedCardHome;
        // Click mode: card currently sliding back into the deck (-1 if none) and its target.
        private int m_ReturningCardIndex = -1;
        private Vector2 m_ReturningCardHome;

        private readonly List<ActionCardData> m_SpawnedCardData = new List<ActionCardData>(24);
        public bool HasFocusedCard
        {
            get { return m_FocusedCardIndex != -1; }
        }

        public void Populate(IEnumerable<StringHash32> cardIds)
        {
            ClearFocus();
            ActionCardPool.Reset();
            m_SpawnedCardData.Clear();

            ActionCardsState actionCards;
            if (!Game.SharedState.TryGet(out actionCards) || cardIds == null)
            {
                ShowNoActionCards(true);
                return;
            }

            int index = 0;
            foreach (var id in cardIds)
            {
                if (!actionCards.AllActionCards.TryGetValue(id, out ActionCardData data))
                {
                    continue;
                }

                UICard card = ActionCardPool.Alloc();
                RectTransform rect = (RectTransform)card.transform;
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                card.gameObject.SetActive(true);
                ActionCardUtility.PopulateCardUI(card, data);
                m_SpawnedCardData.Add(data);

                WireCardInput(card, index);
                index++;
            }

            ShowNoActionCards(m_SpawnedCardData.Count == 0);

            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
        }

        // Spreads the spawned deck cards downward as an overlapping stack: each card sits
        // StackedCardSpacing below the previous one and renders in front of it. Opt-in —
        // call after Populate. Focus behavior is unaffected in both modes, since hover and
        // click both drive the separate FocusSlot card rather than moving deck cards.
        public void LayoutStackedCards()
        {
            int index = 0;
            foreach (UICard card in ActionCardPool.ActiveObjects)
            {
                RectTransform rect = (RectTransform)card.transform;
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -index * StackedCardSpacing);
                rect.SetSiblingIndex(index);
                index++;
            }
        }

        // Immediately clears focus (no animation): snaps any focused or returning card back
        // to its deck position and resets focus state. Call from the owning panel's Hide().
        public void ClearFocus()
        {
            m_FocusInRoutine.Stop();
            m_FocusOutRoutine.Stop();

            if (IsValidIndex(m_FocusedCardIndex))
            {
                SnapCardHome(m_FocusedCardIndex, m_FocusedCardHome);
            }
            if (IsValidIndex(m_ReturningCardIndex))
            {
                SnapCardHome(m_ReturningCardIndex, m_ReturningCardHome);
            }

            m_FocusedCardIndex = -1;
            m_ReturningCardIndex = -1;
            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
        }

        private void ShowNoActionCards(bool show)
        {
            if (NoActionCardsDefault != null)
            {
                NoActionCardsDefault.SetActive(show);
            }
        }

        private void WireCardInput(UICard card, int index)
        {
            FieldNoteCardPointer pointer = card.GetComponent<FieldNoteCardPointer>();
            if (pointer == null)
            {
                pointer = card.gameObject.AddComponent<FieldNoteCardPointer>();
            }

            pointer.CardIndex = index;

            if (ClickToFocus)
            {
                pointer.OnEnter = null;
                pointer.OnExit = null;
                pointer.OnClick = HandleCardClicked;

                Button clickBtn = card.GetComponentInChildren<Button>(true);
                if (clickBtn != null)
                {
                    int capturedIndex = index; 
                    clickBtn.onClick.RemoveAllListeners();
                    clickBtn.onClick.AddListener(() => HandleCardClicked(capturedIndex));
                }
            }
            else
            {
                pointer.OnEnter = HandleCardHoverEnter;
                pointer.OnExit = HandleCardHoverExit;
                pointer.OnClick = null;
            }
        }

        #region Focus Slot

        private void HandleCardHoverEnter(int index)
        {
            if (!IsValidIndex(index)) { return; }

            ActionCardData data = m_SpawnedCardData[index];

            FocusView.PopulateFocusView(data);

            GameObject hoverCard = ActionCardPool.ActiveObjects[index].gameObject;
            hoverCard.transform.parent = FocusedCard;
            _lastFocused = index;
            
            //ActionCardUtility.PopulateCardUI(FocusSlot, data);

            SetFocusSlotVisible(true);
            SetFocusDescription(data.FocusDescription);
        }

        private void HandleCardHoverExit(int index)
        {
            if (_lastFocused != -1) {
                GameObject hoverCard = FocusedCard.GetChild(0).gameObject;
                hoverCard.transform.parent = ActionCardPool.DefaultSpawnTransform;
                hoverCard.transform.SetSiblingIndex(_lastFocused);
            }
            
            ClearFocus();
        }

        private void HandleCardClicked(int index)
        {
            if (!IsValidIndex(index)) { return; }

            // Clicking the focused card sends it back to its place in the deck.
            if (m_FocusedCardIndex == index)
            {
                m_FocusedCardIndex = -1;
                m_FocusInRoutine.Stop();
                StartReturn(index, m_FocusedCardHome);
                SetFocusDescription(string.Empty);
                return;
            }

            // Swapping: the previously focused card slides home on its own routine while the
            // newly clicked card slides into the focus slot, so both animate simultaneously.
            if (m_FocusedCardIndex != -1)
            {
                StartReturn(m_FocusedCardIndex, m_FocusedCardHome);
            }

            m_FocusedCardIndex = index;

            RectTransform rect = CardRect(index);
            if (m_ReturningCardIndex == index)
            {
                // Re-focusing a card that is mid-slide back into the deck: keep its original
                // home and let the focus tween take over from wherever it currently is.
                m_FocusOutRoutine.Stop();
                m_ReturningCardIndex = -1;
                m_FocusedCardHome = m_ReturningCardHome;
            }
            else
            {
                m_FocusedCardHome = rect.anchoredPosition;
            }

            ActionCardData data = m_SpawnedCardData[index];
            // The FocusSlot stays hidden in click mode — the deck card itself travels to it —
            // but external readers (e.g. the swap deck's FocusSlot.CardID) still need its data.
            
            FocusView.PopulateFocusView(data);
            //ActionCardUtility.PopulateCardUI(FocusSlot, data);

            SetFocusDescription(data.FocusDescription);

            //m_FocusInRoutine.Replace(this, rect.AnchorPosTo(FocusTargetPosition(rect), FocusSlideAnim));
        }

        // Starts sliding a card back to its deck position. Only one card can be mid-return;
        // if another is still traveling home, it snaps there instantly first.
        private void StartReturn(int index, Vector2 home)
        {
            if (m_ReturningCardIndex != -1 && m_ReturningCardIndex != index)
            {
                SnapCardHome(m_ReturningCardIndex, m_ReturningCardHome);
            }

            m_ReturningCardIndex = index;
            m_ReturningCardHome = home;
            m_FocusOutRoutine.Replace(this, SlideCardHome(index, home));
        }

        private IEnumerator SlideCardHome(int index, Vector2 home)
        {
            yield return CardRect(index).AnchorPosTo(home, FocusSlideAnim);
            m_ReturningCardIndex = -1;
        }

        private void SnapCardHome(int index, Vector2 home)
        {
            CardRect(index).anchoredPosition = home;
        }

        private RectTransform CardRect(int index)
        {
            return (RectTransform)ActionCardPool.ActiveObjects[index].transform;
        }

        // Anchored position (in the card's parent space) that centers the card on the FocusSlot.
        // private Vector2 FocusTargetPosition(RectTransform cardRect)
        // {
        //     RectTransform slot = (RectTransform)FocusSlot.transform;
        //     Vector3 slotCenter = slot.TransformPoint(slot.rect.center);
        //     Vector3 cardCenter = cardRect.TransformPoint(cardRect.rect.center);
        //     Vector2 delta = ((RectTransform)cardRect.parent).InverseTransformVector(slotCenter - cardCenter);
        //     return cardRect.anchoredPosition + delta;
        // }

        private void SetFocusSlotVisible(bool visible)
        {
            // if (FocusSlot != null)
            // {
            //     FocusSlot.gameObject.SetActive(visible);
            // }
            FocusEmpty.SetActive(!visible);
            if (ClickToFocus) FocusEmpty.SetActive(false);
            FocusView.gameObject.SetActive(visible);
        }

        private void SetFocusDescription(string text)
        {
            if (FocusDescriptionText != null)
            {
                FocusDescriptionText.SetText(text);
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < m_SpawnedCardData.Count;
        }

        #endregion // Focus Slot
    }
}