using AIS.Narrative;
using BeauUtil;
using System;
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
        public Button ConfirmBtn;
        public TMP_Text PromptText;
        public int RequiredCount = 4;
        #endregion

        [NonSerialized] public bool IsComplete;

        private readonly List<UICard> m_Cards = new List<UICard>(24);
        private readonly HashSet<UICard> m_Selected = new HashSet<UICard>();

        private void Awake()
        {
            Instance = this;
            CardSelectionPanel.SetActive(false);
            ConfirmBtn.onClick.AddListener(HandleConfirm);
        }

        public void Load(IEnumerable<StringHash32> cardIds)
        {
            IsComplete = false;
            m_Selected.Clear();
            m_Cards.Clear();

            // Widget spawns and lays out the cards as usual
            DeckWidget.Populate(cardIds);
            DeckWidget.ClearFocus();
            DeckWidget.gameObject.SetActive(true);

            foreach (UICard card in DeckWidget.ActionCardPool.ActiveObjects)
            {
                m_Cards.Add(card);

                Button clickBtn = card.GetComponentInChildren<Button>(true);
                if (clickBtn != null)
                {
                    UICard captured = card;
                    clickBtn.onClick.AddListener(() => ToggleSelection(captured));
                }
            }

            CardSelectionPanel.SetActive(true);
            RefreshUI();
        }

        private void ToggleSelection(UICard card)
        {
            if (m_Selected.Contains(card))
            {
                m_Selected.Remove(card);
                SetHighlight(card, false);
                Hand.RemoveActionCard(card.CardID);   // ← take it back out of the hand
            }
            else if (m_Selected.Count < RequiredCount)
            {
                m_Selected.Add(card);
                SetHighlight(card, true);
                Hand.AddActionCard(card.CardID);      // ← appears in hand immediately
            }

            RefreshUI();
        }

        private void SetHighlight(UICard card, bool on)
        {
            // Adjust to UICard's actual highlight field (Inspector showed "Highlight (Image)")
            if (card.Highlight != null)
            {
                card.Highlight.gameObject.SetActive(on);
            }
        }

        private void RefreshUI()
        {
            ConfirmBtn.interactable = m_Selected.Count == RequiredCount;
            if (PromptText != null)
            {
                PromptText.SetText(string.Format("Select {0} cards ({1}/{0})",
                RequiredCount, m_Selected.Count));
            }
        }

        private void HandleConfirm()
        {
            List<StringHash32> selectedIds = new List<StringHash32>(RequiredCount);
            List<StringHash32> remainingIds = new List<StringHash32>(m_Cards.Count);

            foreach (UICard card in m_Cards)
            {
                if (m_Selected.Contains(card)) { selectedIds.Add(card.CardID); }
                else { remainingIds.Add(card.CardID); }
            }

            DeckWidget.Populate(remainingIds);
            DeckWidget.gameObject.SetActive(false);   // hidden until the swap button opens it

            CardSelectionPanel.SetActive(false);
            IsComplete = true;
        }
    }
}