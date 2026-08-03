using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            ClearEvalRows();
            for (int i = 0; i < EvalNum; i++)
            {
                AddEvalRow(victorious);
            }
        }

        /// <summary>
        /// Displays one star per end-of-game condition, and forces a restart (hides the
        /// continue button) when the player earns zero stars.
        /// </summary>
        public void ShowResults(InterveneEvaluation result)
        {
            ClearEvalRows();
            EvalNum = 3;

            AddEvalRow(result.InvasiveControlled, "Invasive Species decreasing or zero");
            AddEvalRow(result.NativesStable, "All native species stable or increasing");
            AddEvalRow(result.AllEcosystemsHaveNative, "Native species present in each ecosystem");

            if (MainText != null)
            {
                MainText.SetText(GetResultMessage(result.Stars));
            }

            // Zero stars forces a restart: leave only the Restart button available.
            EndInterveneBtn.gameObject.SetActive(result.Stars > 0);
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

            // Restore the continue button in case it was hidden by a zero-star result.
            EndInterveneBtn.gameObject.SetActive(true);

            EndInterveneBtn.onClick.RemoveAllListeners();
            RestartInterveneBtn.onClick.RemoveAllListeners();
        }

        private void ClearEvalRows()
        {
            for (int i = EvalGroup.childCount - 1; i >= 0; i--)
            {
                Destroy(EvalGroup.GetChild(i).gameObject);
            }
        }

        private void AddEvalRow(bool passed, string label = null)
        {
            GameObject newRow = Instantiate(EvalRow, EvalGroup);

            // Child 0 is the star icon; child 1 is the condition description.
            newRow.transform.GetChild(0).GetComponent<Image>().color = passed ? Color.yellow : Color.grey;

            if (label != null)
            {
                TMP_Text desc = newRow.transform.GetChild(1).GetComponent<TMP_Text>();
                if (desc != null)
                {
                    desc.SetText(label);
                }
            }
        }

        private string GetResultMessage(int stars)
        {
            switch (stars)
            {
                case 3: return "Success! Your intervention plan stabilized every ecosystem.";
                case 2: return "Your intervention plan was largely successful.";
                case 1: return "Your intervention plan had limited success.";
                default: return "Your intervention plan failed. You must restart.";
            }
        }

        private void HandleEndInterveneClicked()
        {
            // TODO: transition to next scene via event dispatch
        }

        private void HandleRestartInterveneClicked()
        {
            // TODO: restart via event dispatch
            // AisGame.Events.Dispatch(InterveneEvents.OnInterveneRestart);
            Game.Scenes.ReloadMainScene();
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