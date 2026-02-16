using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Intervene
{
    [RequireComponent(typeof(HoverZone))]
    public class CardHoverZone : MonoBehaviour
    {
        public UICard RootCard;

        private HoverZone m_hoverZone;

        private void Awake()
        {
            m_hoverZone = GetComponent<HoverZone>();

            m_hoverZone.OnHoverEnter.AddListener(HandleHoverEnter);
            m_hoverZone.OnHoverExit.AddListener(HandleHoverExit);
        }

        public void Update()
        {
            m_hoverZone.ManualUpdate(RootCard.RaycasterOverride);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            m_hoverZone.OnHoverEnter.RemoveAllListeners();
            m_hoverZone.OnHoverExit.RemoveAllListeners();
        }

        #region Handlers

        private void HandleHoverEnter()
        {
            RootCard.CanvasOverride.sortingOrder = 2;
        }

        private void HandleHoverExit()
        {
            RootCard.CanvasOverride.sortingOrder = 1;
        }

        #endregion // Handlers
    }
}