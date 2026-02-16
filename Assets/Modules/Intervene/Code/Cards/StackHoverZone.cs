using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class StackHoverZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Routine MoveRoutine;
        public Transform ToMove;
        public float HiddenY;
        public float FocusedY;

        private bool Hovering = false;

        private void Update()
        {
            bool isHoveringThisFrame = IsPointerOverSpecificElement(this.gameObject);

            if (isHoveringThisFrame && !Hovering)
            {
                MoveRoutine.Replace(Focus());
            }
            else if (!isHoveringThisFrame && Hovering)
            {
                MoveRoutine.Replace(Hide());
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
            InterveneUI.Instance.Raycaster.Raycast(pointerData, results);
            foreach (RaycastResult raycastResult in results)
            {
                if (raycastResult.gameObject == targetGameObject)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //MoveRoutine.Replace(Focus());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //MoveRoutine.Replace(Hide());
        }

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