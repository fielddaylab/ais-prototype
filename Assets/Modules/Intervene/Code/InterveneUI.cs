using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene {
    public class InterveneUI : MonoBehaviour
    {
        public static InterveneUI Instance;

        [SerializeField] private Button m_TickSimButton;
        [SerializeField] private InterveneDriver m_Driver;

        public GraphicRaycaster Raycaster;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_TickSimButton.onClick.AddListener(HandleTickSimClicked);
        }

        private void OnDisable()
        {
            if (Game.IsShuttingDown) { return; }
            m_TickSimButton.onClick.RemoveAllListeners();
        }

        private void HandleTickSimClicked()
        {
            m_Driver.TickSim();
        }
    }
}