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
        [SerializeField] private int m_MaxRounds = 5;
        [Tooltip("Default number of recent rounds summarized by the population-trend / stability checks.")]
        [SerializeField] private int m_TrendWindow = InterveneEndConditionEvaluator.DEFAULT_TREND_WINDOW;
        #endregion // Inspector

        private int m_CurrentRound;
        private readonly InterveneEndConditionEvaluator m_Evaluator = new InterveneEndConditionEvaluator();

        // Populated the moment the final round completes, before OnInterveneEnd is dispatched.
        public InterveneEvaluation LastResult { get; private set; }

        // Snapshot history + trend queries, for UI that wants per-population trends. Refresh
        // after each round by listening for InterveneEvents.OnPopulationSnapshotRecorded.
        public InterveneEndConditionEvaluator Evaluator { get { return m_Evaluator; } }

        private void Awake()
        {
            Instance = this;
            m_CurrentRound = 1;
            m_Evaluator.TrendWindow = m_TrendWindow;
            RoundCounter.text = $"Round 1/{m_MaxRounds}";
            Turn.text = $"Player Turn";
        }

        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnEndTurn, HandleEndTurn);
            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleRestart);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnEndTurn, HandleEndTurn);
            AisGame.Events.Deregister(InterveneEvents.OnInterveneRestart, HandleRestart);
        }

        public void SetRound(int round)
        {
            RoundCounter.text = $"Round {round}/{m_MaxRounds}";
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

        // One completed simulation tick == one completed round.
        private void HandleEndTurn()
        {
            m_Evaluator.RecordSnapshot();
            AisGame.Events.Dispatch(InterveneEvents.OnPopulationSnapshotRecorded);

            if (m_CurrentRound >= m_MaxRounds)
            {
                // All rounds are done: score the plan and signal the end of the game.
                LastResult = m_Evaluator.Evaluate();
                AisGame.Events.Dispatch(InterveneEvents.OnInterveneEnd);
            }
            else
            {
                m_CurrentRound++;
                SetRound(m_CurrentRound);
            }
        }

        private void HandleRestart()
        {
            m_CurrentRound = 1;
            SetRound(1);
            m_Evaluator.Reset();
        }
    }
}
