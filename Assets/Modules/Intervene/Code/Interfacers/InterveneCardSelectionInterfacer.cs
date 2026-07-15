using AIS.Narrative;
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
        public Button ConfirmBtn;
        public TMP_Text PromptText;
        public int RequiredCount = 4;

        #endregion

        [NonSerialized] public bool IsComplete;

        private readonly List<StringHash32> m_Cards = new List<StringHash32>(24);
        private readonly HashSet<StringHash32> m_Selected = new HashSet<StringHash32>();

        private Routine m_MoveRoutine;
        private StackHoverZone handHoverZone;

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
                m_Cards.Add(card.CardID);

                Button clickBtn = card.GetComponentInChildren<Button>(true);
                if (clickBtn != null)
                {
                    UICard captured = card;
                    clickBtn.onClick.RemoveAllListeners();
                    clickBtn.onClick.AddListener(() => ToggleSelection(captured));
                }
            }

            CardSelectionPanel.SetActive(true);
            RefreshUI();

            Hand.OnCardClickedOverride = ReturnCardToDeck;
            handHoverZone = Hand.GetComponentInChildren<StackHoverZone>(true);
            handHoverZone.enabled = false;
            handHoverZone.MoveRoutine.Stop();
            m_MoveRoutine.Replace(this, 
                handHoverZone.ToMove.MoveTo(handHoverZone.FocusedY, 0.1f, Axis.Y, Space.Self));
        }

        private void ToggleSelection(UICard card)
        {
            if (m_Selected.Contains(card.CardID)) { return; }
            if (m_Selected.Count >= RequiredCount) { return; }

            StringHash32 id = card.CardID;
            m_Selected.Add(id);
            Hand.AddActionCard(id);

            RectTransform container = (RectTransform)card.transform.parent;
            DeckWidget.ActionCardPool.Free(card);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);

            RefreshUI();
        }
        
        private void ReturnCardToDeck(StringHash32 id)
        {
            if (!m_Selected.Remove(id)) { return; }

            Hand.RemoveActionCard(id);

            List<StringHash32> inDeckIds = new List<StringHash32>(m_Cards.Count);
            foreach(StringHash32 otherId in m_Cards)
            {
                if (!m_Selected.Contains(otherId)) { inDeckIds.Add(otherId); }
            }
            DeckWidget.Populate(inDeckIds);

            // rebind each card's button component with listener
            foreach (UICard card in DeckWidget.ActionCardPool.ActiveObjects)
            {
                Button clickBtn = card.GetComponentInChildren<Button>(true);
                if (clickBtn != null)
                {
                    UICard captured = card;
                    clickBtn.onClick.RemoveAllListeners();
                    clickBtn.onClick.AddListener(() => ToggleSelection(captured));
                }
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            ConfirmBtn.interactable = m_Selected.Count == RequiredCount;
            if (PromptText != null)
            {
                PromptText.SetText(string.Format("Select {0} cards ({1}/{0}) to use",
                RequiredCount, m_Selected.Count));
            }
        }

        private void HandleConfirm()
        {
            List<StringHash32> selectedIds = new List<StringHash32>(RequiredCount);
            List<StringHash32> remainingIds = new List<StringHash32>(m_Cards.Count);

            foreach (StringHash32 cardId in m_Cards)
            {
                if (m_Selected.Contains(cardId)) { selectedIds.Add(cardId); }
                else { remainingIds.Add(cardId); }
            }

            DeckWidget.gameObject.SetActive(false);   // hidden until the swap button opens it

            CardSelectionPanel.SetActive(false);
            IsComplete = true;

            m_MoveRoutine.Replace(handHoverZone,
                handHoverZone.ToMove.MoveTo(handHoverZone.HiddenY, 0.1f, Axis.Y, Space.Self));
            handHoverZone.enabled = true;
            Hand.OnCardClickedOverride = null;
        }
    }
}