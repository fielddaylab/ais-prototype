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
        public UICard FocusSlot;
        public GameObject NoActionCardsDefault;
        public TMP_Text FocusDescriptionText;

        [Header("Focus Behavior")]
        [Tooltip("false = hover a deck card to preview it in the focus slot; true = click to focus/return/swap.")]
        public bool ClickToFocus = true;
        public TweenSettings FocusSlideAnim = new TweenSettings(0.2f, Curve.Smooth);
        public float FocusHiddenX = 600f;

        private Routine m_FocusInRoutine;
        private Routine m_FocusOutRoutine;
        [NonSerialized] public int m_FocusedCardIndex = -1;
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

        // Immediately clears the focus slot (no animation) and resets focus state.
        // Call from the owning panel's Hide().
        public void ClearFocus()
        {
            m_FocusInRoutine.Stop();
            m_FocusOutRoutine.Stop();
            m_FocusedCardIndex = -1;
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
            ActionCardUtility.PopulateCardUI(FocusSlot, data);
            SetFocusSlotVisible(true);
            SetFocusDescription(data.FocusDescription);
        }

        private void HandleCardHoverExit(int index)
        {
            ClearFocus();
        }

        private void HandleCardClicked(int index)
        {
            if (!IsValidIndex(index)) { return; }

            if (m_FocusedCardIndex == index)
            {
                m_FocusedCardIndex = -1;
                m_FocusInRoutine.Stop();
                m_FocusOutRoutine.Replace(this, SlideFocusOut());
                return;
            }

            bool swapping = m_FocusedCardIndex != -1;
            m_FocusedCardIndex = index;
            m_FocusInRoutine.Replace(this, FocusOn(index, swapping));
        }

        private IEnumerator FocusOn(int index, bool swapping)
        {
            RectTransform slot = FocusSlot.transform as RectTransform;

            if (swapping)
            {
                yield return slot.AnchorPosTo(FocusHiddenX, FocusSlideAnim, Axis.X);
            }

            ActionCardData data = m_SpawnedCardData[index];
            ActionCardUtility.PopulateCardUI(FocusSlot, data);
            SetFocusDescription(data.FocusDescription);

            SetFocusSlotVisible(true);
            slot.SetAnchorPos(FocusHiddenX, Axis.X);
            yield return slot.AnchorPosTo(0f, FocusSlideAnim, Axis.X);
        }

        private IEnumerator SlideFocusOut()
        {
            RectTransform slot = FocusSlot.transform as RectTransform;
            yield return slot.AnchorPosTo(FocusHiddenX, FocusSlideAnim, Axis.X);
            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
            slot.SetAnchorPos(0f, Axis.X);
        }

        private void SetFocusSlotVisible(bool visible)
        {
            if (FocusSlot != null)
            {
                FocusSlot.gameObject.SetActive(visible);
            }
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