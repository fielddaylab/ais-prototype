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

            if (stack.FacingDir == CardFaceDir.Visible)
            {
                // populate card
                visuals.CardVisuals[0].FrontGroup.alpha = 1;
                visuals.CardVisuals[0].BackGroup.alpha = 0;
            }
            else if (stack.FacingDir == CardFaceDir.Hidden)
            {
                // TODO: determine card back according to card attributes
                visuals.CardVisuals[0].BackGroup.alpha = 1;
                visuals.CardVisuals[0].FrontGroup.alpha = 0;
            }
            else if (stack.FacingDir == CardFaceDir.Mixed)
            {
                // TODO -- IF NEEDED
            }
        }

        private static void RefreshAllSpreadVisuals(CardStackVisualsBase visuals, CardStack stack)
        {
            if (stack.FacingDir == CardFaceDir.Visible)
            {
                // populate card
            }
            else if (stack.FacingDir == CardFaceDir.Hidden)
            {
                // determine card back according to card attributes
            }
            else if (stack.FacingDir == CardFaceDir.Mixed)
            {
                // TODO -- IF NEEDED
            }
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