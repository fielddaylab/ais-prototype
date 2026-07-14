using AIS.Narrative;
using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class PlayerHand : CardStack
    {
        public List<ActionCard> PlayerCards = new List<ActionCard>();
        public List<int> SelectedCardIndices = new List<int>();
        public bool AllowMultiSelect = false;

        public void AddActionCard(SerializedHash32 actionId)
        {
            if (Game.SharedState.TryGet(out ActionCardsState cardsState))
            {
                if (cardsState.AllActionCards.TryGetValue(actionId, out ActionCardData data))
                {
                    GameObject cardObj = Instantiate(CardInteractionMgr.Instance.UICardPrefab, Visuals.CardContainer);
                    UICard card = cardObj.GetComponent<UICard>();
                    ActionCardUtility.PopulateCardUI(card, data);

                    ActionCard newCard = new ActionCard();
                    newCard.PopulateFromData(data);
                    card.CardData = newCard;

                    PlayerCards.Add(newCard);

                    card.ClickBtn.onClick.AddListener(() => {
                        int index = PlayerCards.IndexOf(newCard); // resolve at click time
                        if (index >= 0) { ToggleSelectAtIndex(index); }
                    });
                }
            }
        }

        public void RemoveActionCard(SerializedHash32 actionId)
        {
            for (int i = 0; i < PlayerCards.Count; i++)
            {
                if (PlayerCards[i].CardID.Equals(actionId))
                {
                    RemoveActionCardAtIndex(i);
                    return;
                }
            }
        }

        private void RemoveActionCardAtIndex(int index)
        {
            if (index < 0 || index >= PlayerCards.Count) { return; }
            PlayerCards.RemoveAt(index);
            
            Destroy(Visuals.CardContainer.transform.GetChild(index).gameObject);
            // Adjust selected indices
            List<int> newSelectedIndices = new List<int>();
            for (int i = SelectedCardIndices.Count - 1; i >= 0; i--)
            {
                if (SelectedCardIndices[i] == index)
                {
                    SelectedCardIndices.RemoveAt(i);
                }
                else if (SelectedCardIndices[i] > index)
                {
                    SelectedCardIndices[i]--;
                }
            }
            SelectedCardIndices = newSelectedIndices;
            UpdateSelectVisuals();
        }

        public void ToggleSelectAtIndex(int index)
        {
            if (SelectedCardIndices.Contains(index))
            {
                DeselectCard(index);
            }
            else if (AllowMultiSelect)
            {
                MultiSelectCard(index);
            }
            else
            {
                SingleSelectCard(index);
            }

            UpdateSelectVisuals();
            AisGame.Events.Dispatch(InterveneEvents.OnUiSelected);
        }

        public void SingleSelectCard(int index)
        {
            SelectedCardIndices.Clear();
            SelectedCardIndices.Add(index);
        }

        public void MultiSelectCard(int index)
        {
            if (SelectedCardIndices.Contains(index)) { return; }

            SelectedCardIndices.Add(index);
        }

        public void DeselectCard(int index)
        {
            if (!SelectedCardIndices.Contains(index)) { return; }

            SelectedCardIndices.Remove(index);
        }

        public void ClearSelections()
        {
            SelectedCardIndices.Clear();

            UpdateSelectVisuals();
        }

        private void UpdateSelectVisuals()
        {
            for (int i = 0; i < PlayerCards.Count; i++)
            {
                if (Visuals.CardContainer.transform.GetChild(i) != null)
                {
                    UICard cardVisual = Visuals.CardContainer.transform.GetChild(i).GetComponent<UICard>();
                    cardVisual.Highlight.GetComponent<Image>().enabled = SelectedCardIndices.Contains(i);
                }
            }
        }

        #region Queries

        public List<CardBase> GetSelectedCards()
        {
            List<CardBase> selected = new List<CardBase>();

            foreach(var index in SelectedCardIndices)
            {
                selected.Add(PlayerCards[index]);

                if (BudgetUtility.CanAfford(InterveneBudgetInterfacer.Instance, selected))
                {
                    for (int i = InterveneBudgetInterfacer.Instance.WorkingBudget.Budget - 1; i >= 0 ; i--)
                    {
                        InterveneBudgetInterfacer.Instance.BudgetGroupTransform.GetChild(i).GetComponent<Image>().color = Color.white;
                    }
                }
                else
                {
                    for (int i = InterveneBudgetInterfacer.Instance.WorkingBudget.Budget - 1; i >= 0 ; i--)
                    {
                        InterveneBudgetInterfacer.Instance.BudgetGroupTransform.GetChild(i).GetComponent<Image>().color = Color.red;
                    }
                }
            }

            return selected;
        }

        #endregion // Queries
    }
}