using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Narrative {
    /// <summary>
    /// Relays pointer enter/exit/click events for a single action card in the field notes panel.
    /// Attached to each spawned UICard so the EvidenceDisplayPanel can drive the focus slot.
    /// Uses standard uGUI pointer handlers (the field notes panel has its own GraphicRaycaster),
    /// rather than the manual-raycaster HoverZone used inside the Intervene scene.
    /// </summary>
    public sealed class FieldNoteCardPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        // Index of this card within the spawned action-card list, so the panel can identify it.
        [NonSerialized] public int CardIndex;

        // Set by the panel each time the card is (re)used.
        [NonSerialized] public Action<int> OnEnter;
        [NonSerialized] public Action<int> OnExit;
        [NonSerialized] public Action<int> OnClick;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            OnEnter?.Invoke(CardIndex);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            OnExit?.Invoke(CardIndex);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            OnClick?.Invoke(CardIndex);
        }
    }
}
