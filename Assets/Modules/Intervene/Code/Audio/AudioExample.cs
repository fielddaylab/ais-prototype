using FieldDay.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class AudioExample : MonoBehaviour
    {
        private void Start()
        {
            AisGame.Events.Register(InterveneEvents.OnHoverZone, HandleZoneHovered);
            AisGame.Events.Register(InterveneEvents.OnDrawFromActionDeck, HandleCardDrawn);

            AisGame.Events.Register(InterveneEvents.OnUiSelected, HandleUiSelected);
            AisGame.Events.Register(InterveneEvents.OnEffectChunkComplete, HandleConfirmButtons);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyConfirm, HandleConfirmButtons);
            AisGame.Events.Register(InterveneEvents.OnEndTurn, HandleTurnEnded);

            AisGame.Events.Register(InterveneEvents.OnPredatorEatPrey, HandlePredatorEatPrey);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnHoverZone, HandleZoneHovered);
            AisGame.Events.Deregister(InterveneEvents.OnDrawFromActionDeck, HandleCardDrawn);

            AisGame.Events.Deregister(InterveneEvents.OnUiSelected, HandleUiSelected);
            AisGame.Events.Deregister(InterveneEvents.OnEffectChunkComplete, HandleConfirmButtons);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyConfirm, HandleConfirmButtons);
            AisGame.Events.Deregister(InterveneEvents.OnEndTurn, HandleTurnEnded);

            AisGame.Events.Deregister(InterveneEvents.OnPredatorEatPrey, HandlePredatorEatPrey);
        }

        private void HandleZoneHovered()
        {
            Sfx.Play("Oneshot.HoverZone");
        }

        private void HandleCardDrawn()
        {
            Sfx.Play("Oneshot.DrawCard");
        }

        private void HandleUiSelected()
        {
            Sfx.Play("Oneshot.Select");
        }

        private void HandleConfirmButtons()
        {
            Sfx.Play("Oneshot.Confirm");
        }

        private void HandleTurnEnded()
        {
            Sfx.Play("Oneshot.EndTurn");
        }

        private void HandlePredatorEatPrey()
        {
            // Sfx.Play("Oneshot.PredatorEatPrey");
        }
    }
}