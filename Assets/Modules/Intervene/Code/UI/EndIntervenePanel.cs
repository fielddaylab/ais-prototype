using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public class EndIntervenePanel : MonoBehaviour
    {
        public TMP_Text MainText;
        public Button EndInterveneBtn;
        public Button RestartInterveneBtn;

        public void SetVictory(bool victorious)
        {
            if (victorious)
            {
                MainText.SetText("Victory! :D");
            }
            else
            {
                MainText.SetText("Defeat! D:");
            }
        }

        public void Show()
        {
            this.gameObject.SetActive(true);

            EndInterveneBtn.onClick.AddListener(HandleEndInterveneClicked);
            RestartInterveneBtn.onClick.AddListener(HandleRestartInterveneClicked);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);

            EndInterveneBtn.onClick.RemoveAllListeners();
            RestartInterveneBtn.onClick.RemoveAllListeners();
        }

        private void HandleEndInterveneClicked()
        {
            // TODO: transition to next scene via event dispatch
        }

        private void HandleRestartInterveneClicked()
        {
            // TODO: restart via event dispatch
            AisGame.Events.Dispatch(InterveneEvents.OnInterveneRestart);
        }
    }
}