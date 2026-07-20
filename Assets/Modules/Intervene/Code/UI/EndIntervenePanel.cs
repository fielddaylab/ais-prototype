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
        public Transform EvalGroup;
        public GameObject EvalRow;

        public int EvalNum;

        public Button EndInterveneBtn;
        public Button RestartInterveneBtn;

        public void SetVictory(bool victorious)
        {
            if (victorious)
            {
                for (int i = 0; i < EvalNum; i++)
                {
                    GameObject newRow = Instantiate(EvalRow, EvalGroup);
                    newRow.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                }
            }
            else
            {
               for (int i = 0; i < EvalNum; i++)
                {
                    GameObject newRow = Instantiate(EvalRow, EvalGroup);
                    newRow.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
                }
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

        public void SucceedTest(int index)
        {
            if (index < EvalNum && index >= 0)
            {
                EvalGroup.GetChild(index).GetChild(0).GetComponent<Image>().color = Color.yellow;
            }
        }

        public void FailTest(int index)
        {
            if (index < EvalNum && index >= 0)
            {
                EvalGroup.GetChild(index).GetChild(0).GetComponent<Image>().color = Color.grey;
            }
        }
    }
}