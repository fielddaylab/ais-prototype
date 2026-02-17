using AIS.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    /// <summary>
    /// System that enables the player to specify how to use their actions.
    /// </summary>
    public class ActionEffectSpecifierSystem : MonoBehaviour
    {
        public List<ActionCard> SelectedActionCards = new List<ActionCard>();
        public List<ActionCard> ActionsProcessList = new List<ActionCard>();

        #region Unity Callbacks

        private void Awake()
        {
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyConfirm);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyCancel);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyConfirm);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyCancel);
        }

        #endregion // Unity Callbacks

        #region Phase Management

        public void FilterActionCards()
        {
            SelectedActionCards.Clear();
            ActionsProcessList.Clear();

            List<CardBase> selectedCards = CardInteractionMgr.Instance.Hand.GetSelectedCards();
            foreach(var card in selectedCards)
            {
                ActionCard actionCard = (ActionCard)card;
                if (actionCard != null)
                {
                    SelectedActionCards.Add(actionCard);
                }
            }
        }

        public void ResetChoices()
        {
            ActionsProcessList.Clear();
        }

        public void Exit()
        {
            SelectedActionCards.Clear();
        }

        #endregion // Phase Management

        #region Handlers

        private void HandleEffectSpecifyBegin()
        {
            FilterActionCards();

            SummonHighlights();
        }

        private void HandleEffectSpecifyConfirm()
        {
            DisperseHighlights();
            Exit();
        }

        private void HandleEffectSpecifyCancel()
        {
            DisperseHighlights();
            ResetChoices();
            Exit();
        }

        #endregion // Handlers

        #region Visuals

        private void SummonHighlights()
        {
            if (SelectedActionCards.Count > 0)
            {
                ActionsProcessList.Add(SelectedActionCards[0]);
                ModelTagMgr.Instance.HighlightByTarget(ActionsProcessList[0].Effects[0].AllTargets[0].Target);
            }
        }

        private void DisperseHighlights()
        {
            ModelTagMgr.Instance.ClearExistingHighlights();
        }

        #endregion // Visuals
    }
}
