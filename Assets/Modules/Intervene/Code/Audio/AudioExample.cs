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

            // UI buttons
            AisGame.Events.Register(InterveneEvents.OnUiSelected, HandleUiSelected);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyBegin, HandleUiSelected);
            AisGame.Events.Register(InterveneEvents.OnEffectChunkComplete, HandleConfirmButtons);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyConfirm, HandleConfirmButtons);

            // Simulation
            AisGame.Events.Register(InterveneEvents.OnHunt, HandleHunt);
            AisGame.Events.Register(InterveneEvents.OnStarve, HandleStarve);
            AisGame.Events.Register(InterveneEvents.OnReproduce, HandleReproduce);
            AisGame.Events.Register(InterveneEvents.OnNestSpawn, HandleNestSpawn);
            AisGame.Events.Register(InterveneEvents.OnTrapTriggered, HandleTrapTriggered);
            AisGame.Events.Register(InterveneEvents.OnPathwayHighlighted, HandlePathwayHighlighted);
            AisGame.Events.Register(InterveneEvents.OnEndTurn, HandleTurnEnded);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnHoverZone, HandleZoneHovered);
            AisGame.Events.Deregister(InterveneEvents.OnDrawFromActionDeck, HandleCardDrawn);

            // UI buttons
            AisGame.Events.Deregister(InterveneEvents.OnUiSelected, HandleUiSelected);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyBegin, HandleUiSelected);
            AisGame.Events.Deregister(InterveneEvents.OnEffectChunkComplete, HandleConfirmButtons);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyConfirm, HandleConfirmButtons);

            // Simulation
            AisGame.Events.Deregister(InterveneEvents.OnHunt, HandleHunt);
            AisGame.Events.Deregister(InterveneEvents.OnStarve, HandleStarve);
            AisGame.Events.Deregister(InterveneEvents.OnReproduce, HandleReproduce);
            AisGame.Events.Deregister(InterveneEvents.OnNestSpawn, HandleNestSpawn);
            AisGame.Events.Deregister(InterveneEvents.OnTrapTriggered, HandleTrapTriggered);
            AisGame.Events.Deregister(InterveneEvents.OnPathwayHighlighted, HandlePathwayHighlighted);
            AisGame.Events.Deregister(InterveneEvents.OnEndTurn, HandleTurnEnded);
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

        private void HandlePathwayHighlighted()
        {
            Sfx.Play("Oneshot.HighlightPathway");
        }

        private void HandleHunt()
        {
            Sfx.Play("Oneshot.Hunt");
        }

        private void HandleStarve()
        {
            Sfx.Play("Oneshot.Starve");
        }

        private void HandleReproduce()
        {
            Sfx.Play("Oneshot.Reproduce");
        }

        private void HandleNestSpawn()
        {
            Sfx.Play("Oneshot.SpawnNest");
        }

        private void HandleTrapTriggered()
        {
            Sfx.Play("Oneshot.TrapTriggered");
        }
    }
}