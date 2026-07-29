using AIS.Narrative;
using AIS.Model;
using FieldDay;
using BeauRoutine;
using BeauUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class InterveneCardSelectionInterfacer : MonoBehaviour
    {
        public static InterveneCardSelectionInterfacer Instance;

        #region Inspector 

        public GameObject CardSelectionPanel;
        public ActionCardDeckWidget DeckWidget;
        public PlayerHand Hand;
        public Button AddBtn;              // "ADD TO DECK" button
        public Button ConfirmBtn;
        public int RequiredCount = 4;

        public TweenSettings CardTravelAnim = new TweenSettings(0.2f, Curve.Smooth);
        public RectTransform HandTravelTarget;

        #endregion

        [NonSerialized] public bool IsComplete;

        private readonly List<StringHash32> m_Cards = new List<StringHash32>(24);
        private readonly HashSet<StringHash32> m_Selected = new HashSet<StringHash32>();

        private Routine m_MoveRoutine;
        private StackHoverZone handHoverZone;

        private readonly Dictionary<UICard, Routine> m_Flying = new Dictionary<UICard, Routine>();
        private Routine m_RestackRoutine;

        // Card currently highlighted in the deck widget; null when nothing is focused.
        private UICard m_Focused;

        private void Awake()
        {
            Instance = this;
            CardSelectionPanel.SetActive(false);
            ConfirmBtn.onClick.AddListener(HandleConfirm);
            AddBtn.onClick.AddListener(HandleAdd);
        }

        public void Load(IEnumerable<StringHash32> cardIds)
        {
            FinishPendingFlights();

            IsComplete = false;
            m_Selected.Clear();
            m_Cards.Clear();

            InterveneSwapDeckInterfacer.Instance.gameObject.SetActive(false);
            //InterveneSwapDeckInterfacer.Instance.SwapBtn.interactable = false;

            // Widget spawns and lays out the cards as usual
            DeckWidget.Populate(cardIds);
            DeckWidget.LayoutStackedCards();
            ClearFocus();
            DeckWidget.gameObject.SetActive(true);

            foreach (UICard card in DeckWidget.ActionCardPool.ActiveObjects)
            {
                m_Cards.Add(card.CardID);
            }

            RebindCardButtons();

            CardSelectionPanel.SetActive(true);
            RefreshUI();

            Hand.OnCardClickedOverride = ReturnCardToDeck;
            handHoverZone = Hand.GetComponentInChildren<StackHoverZone>(true);
            handHoverZone.enabled = false;
            handHoverZone.MoveRoutine.Stop();
            m_MoveRoutine.Replace(this,
                handHoverZone.ToMove.MoveTo(handHoverZone.FocusedY, 0.1f, Axis.Y, Space.Self));
        }

        #region Focus

        // Clicking a card in the deck only focuses it now - adding happens through AddBtn.
        private void HandleCardClicked(UICard card)
        {
            if (card == null) { return; }
            if (m_Selected.Contains(card.CardID)) { return; }
            if (m_Flying.ContainsKey(card)) { return; }

            FocusCard(card);
        }

        private void FocusCard(UICard card)
        {
            ActionCardData data;
            if (Game.SharedState.TryGet(out ActionCardsState actionCards)
                && actionCards.AllActionCards.TryGetValue(card.CardID, out data))
            {
                m_Focused = card;
                if (DeckWidget.FocusView != null)
                {
                    DeckWidget.FocusView.gameObject.SetActive(true);
                    DeckWidget.FocusView.PopulateFocusView(data);

                    // force vertical layout to update
                    RectTransform root = (RectTransform)DeckWidget.FocusView.transform;
                    foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        text.ForceMeshUpdate();
                    }

                    LayoutRebuilder.ForceRebuildLayoutImmediate(root);
                }

                if (DeckWidget.FocusSlot != null)
                {
                    ActionCardUtility.PopulateCardUI(DeckWidget.FocusSlot, data);
                }
                
                if (DeckWidget.FocusDescriptionText != null)
                {
                    DeckWidget.FocusDescriptionText.SetText(data.FocusDescription);
                }

                if (DeckWidget.FocusEmpty != null) { DeckWidget.FocusEmpty.SetActive(false); }
            }

            RefreshUI();
        }

        private void ClearFocus()
        {
            m_Focused = null;
            DeckWidget.ClearFocus();

            if (DeckWidget.FocusSlot != null) { DeckWidget.FocusSlot.gameObject.SetActive(false); }
            if (DeckWidget.FocusView != null) { DeckWidget.FocusView.gameObject.SetActive(false); }
            if (DeckWidget.FocusEmpty != null) { DeckWidget.FocusEmpty.SetActive(true); }
        }

        #endregion // Focus

        private void HandleAdd()
        {
            UICard card = m_Focused;
            if (card == null) { return; }
            if (m_Selected.Count >= RequiredCount) { return; }
            if (m_Flying.ContainsKey(card)) { return; }
            if (m_Selected.Contains(card.CardID)) { return; }

            StringHash32 id = card.CardID;
            m_Selected.Add(id);

            Button clickBtn = card.GetComponentInChildren<Button>(true);
            if (clickBtn != null) { clickBtn.interactable = false; }

            ClearFocus();

            m_Flying[card] = Routine.Start(this, FlyCardToHand(card, id));
            RefreshUI();
        }

        private IEnumerator FlyCardToHand(UICard card, StringHash32 cardId)
        {
            RectTransform rect = (RectTransform)card.transform;
            yield return CardTravelUtility.TravelToRect(rect, HandTarget(), CardTravelAnim);

            m_Flying.Remove(card);
            DeckWidget.ActionCardPool.Free(card);
            Hand.AddActionCard(cardId);
            m_RestackRoutine.Replace(this, CardTravelUtility.AnimatedRestack(DeckWidget, CardTravelAnim));
            RefreshUI();
        }

        private RectTransform HandTarget()
        {
            return HandTravelTarget != null ? HandTravelTarget : (RectTransform)Hand.transform;
        }

        private void ReturnCardToDeck(StringHash32 id)
        {
            FinishPendingFlights();
            m_RestackRoutine.Stop();
            if (!m_Selected.Remove(id)) { return; }

            // Repopulating recycles the pooled cards, so any focused instance is now stale.
            ClearFocus();

            Hand.RemoveActionCard(id);

            List<StringHash32> inDeckIds = new List<StringHash32>(m_Cards.Count);
            foreach (StringHash32 otherId in m_Cards)
            {
                if (!m_Selected.Contains(otherId)) { inDeckIds.Add(otherId); }
            }

            DeckWidget.Populate(inDeckIds);
            DeckWidget.LayoutStackedCards();
            RebindCardButtons();

            int index = 0;
            foreach (UICard card in DeckWidget.ActionCardPool.ActiveObjects)
            {
                if (card.CardID == id)
                {
                    RectTransform rect = (RectTransform)card.transform;
                    Vector2 slot = CardTravelUtility.StackedPosition(index, DeckWidget.StackedCardSpacing);
                    int slotSibling = index;

                    rect.anchoredPosition = CardTravelUtility.AnchoredPositionOver(rect, HandTarget());
                    rect.SetAsLastSibling();
                    m_Flying[card] = Routine.Start(this, SlideIntoSlot(card, rect, slot, slotSibling));
                }
                index++;
            }

            RefreshUI();
        }

        private void RebindCardButtons()
        {
            foreach (UICard card in DeckWidget.ActionCardPool.ActiveObjects)
            {
                FieldNoteCardPointer pointer = card.GetComponent<FieldNoteCardPointer>();
                if (pointer != null)
                {
                    pointer.OnClick = null;
                    pointer.OnEnter = null;
                    pointer.OnExit = null;
                }

                Button clickBtn = card.GetComponentInChildren<Button>(true);
                if (clickBtn != null)
                {
                    UICard captured = card;
                    clickBtn.interactable = true;
                    clickBtn.onClick.RemoveAllListeners();
                    clickBtn.onClick.AddListener(() => HandleCardClicked(captured));
                }
            }
        }

        private void RefreshUI()
        {
            bool canAdd = m_Focused != null
                && m_Selected.Count < RequiredCount
                && !m_Selected.Contains(m_Focused.CardID)
                && !m_Flying.ContainsKey(m_Focused);

            if (AddBtn != null) { AddBtn.interactable = canAdd; }
            ConfirmBtn.interactable = m_Selected.Count == RequiredCount;
        }

        private void FinishPendingFlights()
        {
            foreach (var kvp in m_Flying)
            {
                kvp.Value.Stop();
                StringHash32 id = kvp.Key.CardID;
                if (m_Selected.Contains(id))
                {
                    DeckWidget.ActionCardPool.Free(kvp.Key);
                    Hand.AddActionCard(id);
                }
            }
            m_Flying.Clear();
            m_Focused = null;
        }

        private IEnumerator SlideIntoSlot(UICard card, RectTransform rect, Vector2 slot, int sibling)
        {
            yield return rect.AnchorPosTo(slot, CardTravelAnim);
            rect.SetSiblingIndex(sibling);
            m_Flying.Remove(card);
            RefreshUI();
        }

        private void HandleConfirm()
        {
            FinishPendingFlights();
            m_RestackRoutine.Stop();

            List<StringHash32> selectedIds = new List<StringHash32>(RequiredCount);
            List<StringHash32> remainingIds = new List<StringHash32>(m_Cards.Count);

            foreach (StringHash32 cardId in m_Cards)
            {
                if (m_Selected.Contains(cardId)) { selectedIds.Add(cardId); }
                else { remainingIds.Add(cardId); }
            }

            DeckWidget.Populate(remainingIds);
            DeckWidget.LayoutStackedCards();
            ClearFocus();
            DeckWidget.gameObject.SetActive(false);   // hidden until the swap button opens it

            CardSelectionPanel.SetActive(false);
            IsComplete = true;
            RefreshUI();

            m_MoveRoutine.Replace(handHoverZone,
                handHoverZone.ToMove.MoveTo(handHoverZone.HiddenY, 0.1f, Axis.Y, Space.Self));
            handHoverZone.enabled = true;
            Hand.OnCardClickedOverride = null;

            //InterveneSwapDeckInterfacer.Instance.SwapBtn.interactable = true;
            InterveneSwapDeckInterfacer.Instance.gameObject.SetActive(true);
        }
    }

    public sealed class CardTravelUtility
    {
        static public Vector2 StackedPosition(int index, float spacing)
        {
            return new Vector2(0f, -index * spacing);
        }

        static public Vector2 AnchoredPositionOver(RectTransform cardRect, RectTransform target)
        {
            Vector3 targetCenter = target.TransformPoint(target.rect.center);
            Vector3 cardCenter = cardRect.TransformPoint(cardRect.rect.center);
            Vector2 delta = ((RectTransform)cardRect.parent).InverseTransformVector(targetCenter - cardCenter);
            return cardRect.anchoredPosition + delta;
        }

        static public void PrepareStackedRect(RectTransform rect, int index)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.SetSiblingIndex(index);
        }

        static public IEnumerator AnimatedRestack(ActionCardDeckWidget deck, TweenSettings anim)
        {
            List<IEnumerator> tweens = new List<IEnumerator>();
            int index = 0;
            foreach (UICard card in deck.ActionCardPool.ActiveObjects)
            {
                RectTransform rect = (RectTransform)card.transform;
                PrepareStackedRect(rect, index);
                tweens.Add(rect.AnchorPosTo(StackedPosition(index, deck.StackedCardSpacing), anim));
                index++;
            }
            return Routine.Combine(tweens);
        }

        static public IEnumerator TravelToRect(RectTransform cardRect, RectTransform target, TweenSettings anim)
        {
            cardRect.SetAsLastSibling();
            return cardRect.AnchorPosTo(AnchoredPositionOver(cardRect, target), anim);
        }
    }
}