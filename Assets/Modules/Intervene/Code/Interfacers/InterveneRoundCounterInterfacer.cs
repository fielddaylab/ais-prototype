using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class InterveneRoundCounterInterfacer : MonoBehaviour
    {
        public static InterveneRoundCounterInterfacer Instance;
        
        #region Inspector
        public TMP_Text RoundCounter;
        public TMP_Text Turn;
        #endregion // Inspector

        private void Awake()
        {
            Instance = this;
            RoundCounter.text = $"Round 1/5";
            Turn.text = $"Player Turn";
        }
        public void SetRound(int round)
        {
            RoundCounter.text = $"Round {round}/5";
            ;
        }

        public void SetTurn(string activeSide)
        {
            Turn.text = $"{activeSide} Turn";
        }

        public void TakeTurn()
        {
            if (Turn.text.Equals("Ecosystem Turn"))
                Turn.text = "Player Turn";
            else
                Turn.text = "Ecosystem Turn";
        }
    }
}