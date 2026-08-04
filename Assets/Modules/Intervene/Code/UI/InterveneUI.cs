using BeauRoutine;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene {
    public class InterveneUI : MonoBehaviour
    {
        public static InterveneUI Instance;

        [SerializeField] private Button m_TickSimButton;
        [SerializeField] private Image m_TickSimButtonHighlight;
        [SerializeField] private InterveneDriver m_Driver;
        [SerializeField] private InterveneBudgetInterfacer m_BudgetInterfacer;
        [SerializeField] private IntervenePopulationInterfacer m_PopulationInterfacer;

        [Header("Out of Budget Highlight")]
        public TweenSettings HighlightPulseAnim = new TweenSettings(0.5f, Curve.Smooth);
        [Tooltip("Alpha the highlight dips to at the bottom of each pulse.")]
        public float HighlightPulseAlpha = 0.15f;

        [Header("Sim Phase")]
        public GameObject SimPhaseGroup;
        public TMP_Text SimPhaseText;
        public Color FocusColor;

        [Header("End State")]
        [SerializeField] private Button m_VictoryBtn;
        [SerializeField] private Button m_DefeatBtn;
        [SerializeField] private EndIntervenePanel m_EndPanel;

        public bool SimInProgress;

        public GraphicRaycaster Raycaster;

        private Routine m_HighlightPulseRoutine;

        private void Awake()
        {
            Instance = this;

            m_EndPanel.Hide();
            HideTickSimHighlight();
        }

        private void Start()
        {
            m_TickSimButton.onClick.AddListener(HandleTickSimClicked);

            m_VictoryBtn.onClick.AddListener(HandleDeclareVictoryClicked);
            m_DefeatBtn.onClick.AddListener(HandleDeclareDefeatClicked);

            HideSimPhase();

            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
            AisGame.Events.Register(InterveneEvents.OnInterveneEnd, HandleInterveneEnd);

            AisGame.Events.Register(InterveneEvents.OnBudgetChanged, HandleBudgetChanged);

            // Hide the End Turn button while the player is specifying a selected card's effects.
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyEnd);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyEnd);
        }

        private void OnDisable()
        {
            if (Game.IsShuttingDown) { return; }

            m_HighlightPulseRoutine.Stop();

            m_TickSimButton.onClick.RemoveAllListeners();

            m_VictoryBtn.onClick.RemoveAllListeners();
            m_DefeatBtn.onClick.RemoveAllListeners();

            AisGame.Events.Deregister(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
            AisGame.Events.Deregister(InterveneEvents.OnInterveneEnd, HandleInterveneEnd);

            AisGame.Events.Deregister(InterveneEvents.OnBudgetChanged, HandleBudgetChanged);

            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyEnd);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyEnd);
        }

        public void SetSimInProgress(bool inProgress)
        {
            SimInProgress = inProgress;
        }
        
        public void ShowSimPhase()
        {
            SimPhaseGroup.SetActive(true);
        }

        public void SetSimPhase(string phaseText)
        {
            SimPhaseText.SetText(phaseText);
        }

        public void HideSimPhase()
        {
            SimPhaseGroup.SetActive(false);
        }

        #region Out of Budget Highlight

        /// <summary>
        /// Pulses the End Turn button once the player has nothing left to spend this round.
        /// </summary>
        private void RefreshTickSimHighlight()
        {
            bool outOfBudget = !SimInProgress
                && m_TickSimButton.gameObject.activeSelf
                && m_BudgetInterfacer.WorkingBudget.Budget <= 0;

            if (outOfBudget)
            {
                ShowTickSimHighlight();
            }
            else
            {
                HideTickSimHighlight();
            }
        }

        private void ShowTickSimHighlight()
        {
            if (m_TickSimButtonHighlight.gameObject.activeSelf) { return; }

            m_TickSimButtonHighlight.SetAlpha(1);
            m_TickSimButtonHighlight.gameObject.SetActive(true);
            m_HighlightPulseRoutine.Replace(this, m_TickSimButtonHighlight.FadeTo(HighlightPulseAlpha, HighlightPulseAnim).YoyoLoop());
        }

        private void HideTickSimHighlight()
        {
            m_HighlightPulseRoutine.Stop();
            m_TickSimButtonHighlight.gameObject.SetActive(false);
        }

        #endregion // Out of Budget Highlight

        private void HandleTickSimClicked()
        {
            if (m_Driver.SimRoutine.Exists()) { return; }

            HideTickSimHighlight();

            m_Driver.TickSim();
            m_BudgetInterfacer.BestowBudget();
        }

        private void HandleBudgetChanged()
        {
            RefreshTickSimHighlight();
        }

        private void HandleDeclareVictoryClicked()
        {
            m_EndPanel.SetVictory(true);
            m_EndPanel.Show();
        }

        private void HandleDeclareDefeatClicked()
        {
            m_EndPanel.SetVictory(false);
            m_EndPanel.Show();
        }

        private void HandleEffectSpecifyBegin()
        {
            m_TickSimButton.gameObject.SetActive(false);
            HideTickSimHighlight();
        }

        private void HandleEffectSpecifyEnd()
        {
            m_TickSimButton.gameObject.SetActive(true);

            // the cards just played may have emptied the budget
            RefreshTickSimHighlight();
        }

        private void HandleInterveneEnd()
        {
            HideTickSimHighlight();

            m_EndPanel.ShowResults(InterveneRoundCounterInterfacer.Instance.LastResult);
            m_EndPanel.Show();
        }

        private void HandleInterveneRestart()
        {
            HideTickSimHighlight();

            m_EndPanel.Hide();
        }
    }
}