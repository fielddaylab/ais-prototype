using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Intervene
{
    [RequireComponent(typeof(HoverZone))]
    public class CardHoverZone : MonoBehaviour
    {
        public UICard RootCard;
        public Routine MoveRoutine;
        public float HoverOffsetY = 40f;
        public float SelectedOffsetY = 80f;

        private static CardHoverZone s_CurrentHovered;

        private HoverZone m_hoverZone;
        private RectTransform m_rectTransform;
        private HorizontalLayoutGroup m_layout;
        private float m_OriginalY;
        private float m_FanLayout;

        private void Awake()
        {
            m_hoverZone = GetComponent<HoverZone>();
            m_rectTransform = RootCard.GetComponent<RectTransform>();
            m_layout = transform.parent.GetComponent<HorizontalLayoutGroup>();

            m_hoverZone.OnHoverEnter.AddListener(HandleHoverEnter);
            m_hoverZone.OnHoverExit.AddListener(HandleHoverExit);
            AisGame.Events.Register(InterveneEvents.OnUiSelected, HandleSelectionChanged);
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
            AisGame.Events.Deregister(InterveneEvents.OnUiSelected, HandleSelectionChanged);
        }

        #region Handlers

        private bool IsSelected()
        {
            return CardInteractionMgr.Instance.Hand.SelectedCardIndices.Contains(RootCard.StackIndex);
        }

        private void HandleHoverEnter()
        {
            if (s_CurrentHovered == this) return;

            if (s_CurrentHovered != null && s_CurrentHovered != this)
                s_CurrentHovered.HandleHoverExit();
            s_CurrentHovered = this;

            if (m_FanLayout == 0f)
            {
                m_FanLayout = m_rectTransform.anchoredPosition.y;
            }

            RootCard.CanvasOverride.sortingOrder = 2;
            float targetY = m_FanLayout + (IsSelected() ? SelectedOffsetY : HoverOffsetY);
            MoveRoutine.Replace(MoveTo(targetY));
        }

        private void HandleHoverExit()
        {
            if (s_CurrentHovered == this)
                s_CurrentHovered = null;

            RootCard.CanvasOverride.sortingOrder = 1;
            float targetY = IsSelected() ? m_FanLayout + SelectedOffsetY : m_FanLayout;
            MoveRoutine.Replace(MoveTo(targetY));
        }

        private void HandleSelectionChanged()
        {   
            float targetY = IsSelected() ? m_FanLayout + SelectedOffsetY : m_FanLayout;
            if (s_CurrentHovered == this)
            {
                targetY = m_FanLayout + Mathf.Max(HoverOffsetY, IsSelected() ? SelectedOffsetY : HoverOffsetY);
            }

            MoveRoutine.Replace(MoveTo(targetY));
        }

        #endregion // Handlers

        #region Routines
        public void setFanLayout(float fanY)
        {
            m_FanLayout = fanY;
        }

        private IEnumerator MoveTo(float targetY)
        {
            yield return Tween.Float(m_rectTransform.anchoredPosition.y, targetY, SetAnchoredY, 0.1f);
        }

        private void SetAnchoredY(float y)
        {
            Vector2 pos = m_rectTransform.anchoredPosition;
            pos.y = y;
            m_rectTransform.anchoredPosition = pos;
        }

        #endregion // Routines
    }
}