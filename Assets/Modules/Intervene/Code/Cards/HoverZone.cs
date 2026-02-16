using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class HoverZone : MonoBehaviour
    {
        protected bool Hovering = false;

        public UnityEvent OnHoverEnter;
        public UnityEvent OnHoverExit;

        public void ManualUpdate(GraphicRaycaster raycaster)
        {
            bool isHoveringThisFrame = IsPointerOverSpecificElement(this.gameObject, raycaster);

            if (isHoveringThisFrame && !Hovering)
            {
                OnHoverEnter?.Invoke();
            }
            else if (!isHoveringThisFrame && Hovering)
            {
                OnHoverExit?.Invoke();
            }
            Hovering = isHoveringThisFrame;
        }

        private bool IsPointerOverSpecificElement(GameObject targetGameObject, GraphicRaycaster raycaster)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);
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