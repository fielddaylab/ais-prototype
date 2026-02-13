using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class CardStack : MonoBehaviour
    {
        public enum StackOrientation
        {
            Vertical,
            Horizontal
        }

        public enum CardFaceDir
        {
            Visible,
            Hidden
        }

        public StackOrientation Orientation;     // rotation (vertical, horizontal)
        public CardFaceDir FacingDir;            // face-up or face-down
    }

    public static class CardStackUtility
    {
        public static void MergeStacks(CardStack srcStack, CardStack destStack, bool useDestFacingDir = true, bool useDestOrientation = true)
        {
            // TODO
        }

        public static void ShuffleStack(CardStack toShuffle)
        {
            // TODO
        }
    }
}