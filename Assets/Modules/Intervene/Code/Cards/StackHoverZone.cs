using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Intervene
{
    [RequireComponent(typeof(HoverZone))]
    public class StackHoverZone : MonoBehaviour
    {
        public Routine MoveRoutine;
        public Transform ToMove;
        public float HiddenY;
        public float FocusedY;

        private HoverZone m_hoverZone;

        private void Awake()
        {
            m_hoverZone = GetComponent<HoverZone>();

            m_hoverZone.OnHoverEnter.AddListener(HandleHoverEnter);
            m_hoverZone.OnHoverExit.AddListener(HandleHoverExit);
        }

        public void Update()
        {
            m_hoverZone.ManualUpdate(InterveneUI.Instance.Raycaster);
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
            MoveRoutine.Replace(Focus());
        }

        private void HandleHoverExit()
        {
            MoveRoutine.Replace(Hide());
        }

        #endregion // Handlers

        #region Routines

        private IEnumerator Focus()
        {
            yield return ToMove.MoveTo(FocusedY, 0.1f, Axis.Y, Space.World);
        }

        private IEnumerator Hide()
        {
            yield return ToMove.MoveTo(HiddenY, 0.1f, Axis.Y, Space.World);
        }

        #endregion // Routine
    }
}