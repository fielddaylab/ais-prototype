using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Intervene
{
    public class HoverZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Routine MoveRoutine;
        public Transform ToMove;
        public float HiddenY;
        public float FocusedY;

        public void OnPointerEnter(PointerEventData eventData)
        {
            MoveRoutine.Replace(Focus());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MoveRoutine.Replace(Hide());
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