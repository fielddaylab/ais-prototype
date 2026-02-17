using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public abstract class CardStackVisualsBase : MonoBehaviour
    {
        public Routine VisualsRoutine;
        public List<UICard> CardVisuals = new List<UICard>();
        public Transform CardContainer;
    }

    public static class CardStackVisualsUtility
    {
        public static void RefreshVisuals(CardStackVisualsBase visuals, CardStack stack)
        {
            // if no cards, remove all cards
            if (stack.Cards.Count == 0)
            {
                foreach (var visual in visuals.CardVisuals)
                {
                    GameObject.Destroy(visual.gameObject);
                }
                visuals.CardVisuals.Clear();
                return;
            }

            if (stack.Orientation == StackOrientation.Stacked)
            {
                RefreshStackedVisuals(visuals, stack);
            }
            else if (stack.Orientation == StackOrientation.Spread)
            {
                RefreshAllSpreadVisuals(visuals, stack);
            }
        }

        private static void RefreshStackedVisuals(CardStackVisualsBase visuals, CardStack stack)
        {
            // only show top card
            if (visuals.CardVisuals.Count != 1)
            {
                foreach (var visual in visuals.CardVisuals)
                {
                    GameObject.Destroy(visual.gameObject);
                }
                visuals.CardVisuals.Clear();

                var newCard = UnityEngine.Object.Instantiate(CardInteractionMgr.Instance.UICardPrefab, visuals.CardContainer).GetComponent<UICard>();
                visuals.CardVisuals.Add(newCard);
            }

            visuals.CardVisuals[0].StackIndex = stack.Cards.Count - 1;

            if (stack.FacingDir == CardFaceDir.Visible)
            {
                PopulateCardFront(stack, visuals.CardVisuals[0]);
            }
            else if (stack.FacingDir == CardFaceDir.Hidden)
            {
                PopulateCardBack(stack, visuals.CardVisuals[0]);
            }
            else if (stack.FacingDir == CardFaceDir.Mixed)
            {
                // TODO -- IF NEEDED
            }
        }

        private static void RefreshAllSpreadVisuals(CardStackVisualsBase visuals, CardStack stack)
        {
            for (int i = 0; i < stack.Cards.Count; i++)
            {
                // determine if a visual card needs to be added
                if (i > visuals.CardVisuals.Count - 1)
                {
                    var newCard = UnityEngine.Object.Instantiate(CardInteractionMgr.Instance.UICardHoverablePrefab, visuals.CardContainer).GetComponent<UICard>();
                    newCard.StackIndex = i;
                    int tempIndex = i;
                    newCard.ClickBtn.onClick.AddListener(() => { stack.ClickCall(tempIndex); });
                    visuals.CardVisuals.Add(newCard);
                }

                ClearCard(visuals.CardVisuals[i]);

                if (stack.FacingDir == CardFaceDir.Visible)
                {
                    PopulateCardFront(stack, visuals.CardVisuals[i]);
                }
                else if (stack.FacingDir == CardFaceDir.Hidden)
                {
                    PopulateCardBack(stack, visuals.CardVisuals[i]);
                }
                else if (stack.FacingDir == CardFaceDir.Mixed)
                {
                    // TODO -- IF NEEDED
                }
            }

            // remove excess visual cards
            for (int i = visuals.CardVisuals.Count - 1; i >= stack.Cards.Count; i--)
            {
                GameObject.Destroy(visuals.CardVisuals[i].gameObject);
                visuals.CardVisuals.RemoveAt(i);
            }
        }

        private static void ClearCard(UICard card)
        {
            card.Highlight.enabled = false;
        }

        private static void PopulateCardFront(CardStack stack, UICard card)
        {
            card.FrontGroup.alpha = 1;
            card.FrontGroup.gameObject.SetActive(true);
            card.BackGroup.alpha = 0;
            card.BackGroup.gameObject.SetActive(false);

            stack.Cards[card.StackIndex].PopulateCardUI(card);
        }

        private static void PopulateCardBack(CardStack stack, UICard card)
        {
            card.BackGroup.alpha = 1;
            card.BackGroup.gameObject.SetActive(true);
            card.FrontGroup.alpha = 0;
            card.FrontGroup.gameObject.SetActive(false);

            // determine card back according to card attributes
            Sprite backSprite = null;
            if ((CardStackUtility.TopCard(stack).Attributes & CardAttributes.Action) != 0)
            {
                backSprite = CardInteractionMgr.Instance.CardBackActionSprite;
            }
            card.BackImg.sprite = backSprite;
        }

        private static void RefreshSpreadVisualsAt(CardStackVisualsBase visuals, CardStack stack, int targetIndex)
        {

        }

        public static void PerformShuffleVisuals()
        {
            // TODO -- STRETCH GOAL
        }

        public static void PerformMergeVisuals()
        {
            // TODO -- STRETCH GOAL
        }
    }
}