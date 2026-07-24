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
        [SerializeField] private InterveneDriver m_Driver;
        [SerializeField] private InterveneBudgetInterfacer m_BudgetInterfacer;
        [SerializeField] private IntervenePopulationInterfacer m_PopulationInterfacer;

        [Header("Sim Phase")]
        public GameObject SimPhaseGroup;
        public TMP_Text SimPhaseText;
        public Color FocusColor;

        [Header("End State")]
        [SerializeField] private Button m_VictoryBtn;
        [SerializeField] private Button m_DefeatBtn;
        [SerializeField] private EndIntervenePanel m_EndPanel;

        public GraphicRaycaster Raycaster;

        private void Awake()
        {
            Instance = this;

            m_EndPanel.Hide();
        }

        private void Start()
        {
            m_TickSimButton.onClick.AddListener(HandleTickSimClicked);

            m_VictoryBtn.onClick.AddListener(HandleDeclareVictoryClicked);
            m_DefeatBtn.onClick.AddListener(HandleDeclareDefeatClicked);

            HideSimPhase();

            AisGame.Events.Register(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
            AisGame.Events.Register(InterveneEvents.OnInterveneEnd, HandleInterveneEnd);
        }

        private void OnDisable()
        {
            if (Game.IsShuttingDown) { return; }
            m_TickSimButton.onClick.RemoveAllListeners();

            m_VictoryBtn.onClick.RemoveAllListeners();
            m_DefeatBtn.onClick.RemoveAllListeners();

            AisGame.Events.Deregister(InterveneEvents.OnInterveneRestart, HandleInterveneRestart);
            AisGame.Events.Deregister(InterveneEvents.OnInterveneEnd, HandleInterveneEnd);
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

        private void HandleTickSimClicked()
        {
            if (m_Driver.SimRoutine.Exists()) { return; }

            m_Driver.TickSim();
            m_BudgetInterfacer.BestowBudget();
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

        private void HandleInterveneEnd()
        {
            m_EndPanel.ShowResults(InterveneRoundCounterInterfacer.Instance.LastResult);
            m_EndPanel.Show();
        }

        private void HandleInterveneRestart()
        {
            m_EndPanel.Hide();
        }
    }
}