using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public enum StackOrientation
    {
        Stacked,
        Spread,
    }

    public enum CardFaceDir
    {
        Visible,
        Hidden,
        Mixed
    }

    public class CardStack : MonoBehaviour
    {
        // Card at last index is top of deck
        public List<CardBase> Cards = new List<CardBase>();
        public CardStackVisualsBase Visuals;

        public StackOrientation Orientation;     // rotation (vertical, horizontal)
        public CardFaceDir FacingDir;            // face-up or face-down
    }

    public static class CardStackUtility
    {
        public static void MergeStacks(CardStack srcStack, CardStack destStack, bool toTop = true)
        {
            if (toTop)
            {
                destStack.Cards.AddRange(srcStack.Cards);

                srcStack.Cards.Clear();
            }
            else
            {
                List<CardBase> newDestStack = new List<CardBase>();

                newDestStack.AddRange(srcStack.Cards);
                newDestStack.AddRange(destStack.Cards);

                destStack.Cards = newDestStack;

                srcStack.Cards.Clear();
            }

            CardStackVisualsUtility.RefreshVisuals(srcStack.Visuals, srcStack);
            CardStackVisualsUtility.RefreshVisuals(destStack.Visuals, destStack);
        }

        public static void ShuffleStack(CardStack toShuffle)
        {
            toShuffle.Cards = ListUtility.ShuffleList<CardBase>(toShuffle.Cards);
            CardStackVisualsUtility.RefreshVisuals(toShuffle.Visuals, toShuffle);
        }

        public static CardBase TopCard(CardStack stack)
        {
            if (stack.Cards.Count > 0)
            {
                return stack.Cards[stack.Cards.Count - 1];
            }
            else
            {
                return null;
            }
        }

        public static bool TryDrawFromTop(CardStack toDrawFrom, out CardBase drawn)
        {
            drawn = TopCard(toDrawFrom);
            if (drawn == null) return false;

            toDrawFrom.Cards.RemoveAt(toDrawFrom.Cards.Count - 1);
            CardStackVisualsUtility.RefreshVisuals(toDrawFrom.Visuals, toDrawFrom);
            return true;
        }

        public static void AddToTop(CardStack toAddTo, CardBase toAdd, bool refreshVisuals = true)
        {
            toAddTo.Cards.Add(toAdd);
            if (refreshVisuals)
            {
                CardStackVisualsUtility.RefreshVisuals(toAddTo.Visuals, toAddTo);
            }
        }

        public static void RemoveCards(CardStack stack, List<int> indices)
        {
            while (indices.Count > 0)
            {
                var indexToRemove = indices[indices.Count - 1];
                if (indexToRemove >= stack.Cards.Count)
                {
                    Debug.LogError("[CardStack] index to remove is out of bounds!");
                }
                else
                {
                    stack.Cards.RemoveAt(indexToRemove);

                    // Adjust subsequent indices to account for removed card
                    indices.RemoveAt(indices.Count - 1);
                    for (int j = 0; j < indices.Count; j++)
                    {
                        if (indices[j] > indexToRemove)
                        {
                            indices[j] = indices[j] - 1;
                        }
                    }
                }
            }

            CardStackVisualsUtility.RefreshVisuals(stack.Visuals, stack);
        }
    }

    public static class ListUtility
    {
        public static List<T> ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            return list;
        }
    }
}