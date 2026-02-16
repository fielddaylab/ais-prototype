using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Intervene
{
    public class CardHoverZone : MonoBehaviour
    {
        public UICard RootCard;

        private bool Hovering = false;

        private void Update()
        {
            bool isHoveringThisFrame = IsPointerOverSpecificElement(this.gameObject);

            if (isHoveringThisFrame && !Hovering)
            {
                RootCard.CanvasOverride.sortingOrder = 2;
            }
            else if (!isHoveringThisFrame && Hovering)
            {
                RootCard.CanvasOverride.sortingOrder = 1;
            }
            Hovering = isHoveringThisFrame;
        }

        private bool IsPointerOverSpecificElement(GameObject targetGameObject)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            RootCard.RaycasterOverride.Raycast(pointerData, results);
            foreach (RaycastResult raycastResult in results)
            {
                if (raycastResult.gameObject == targetGameObject)
                {
                    return true;
                }
            }
            return false;
        }

    }
}