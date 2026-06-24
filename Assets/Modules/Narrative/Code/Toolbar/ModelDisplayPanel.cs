using AIS.Model;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class ModelDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerStatBlock> {
        //public StatDisplayWidget[] Widgets;
        [SerializeField] public GameObject ModelContainer;
        public Button ToNotesButton;
        public Button CloseButton;

        protected override void Awake() {
            base.Awake();
            ToNotesButton.onClick.AddListener(PanelUtility.ToggleEvidence);
            CloseButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Populate(in PlayerStatBlock parms) {
            //for(int i = 0; i < Widgets.Length; i++) {
            //    StatDisplayWidget widget = Widgets[i];
            //    widget.StatValueCounter.SetValue(parms[widget.StatId], GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);
            //}
        }

        public override void Show() {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);
            DisplayModel();
            ScriptHooks.HideToolbar();
        }

        private void DisplayModel()
        {
            if (InvasionModel.Instance == null)
            {
                Instantiate(InvasionModel.Instance, ModelContainer.transform);
            }

            // InvasionModel.Instance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            InvasionModel.Instance.transform.localPosition = new Vector3(100f, 0f, 0f);
            InvasionModel.Instance.RenderCam.transform.localPosition = new Vector3(100f, 0f, -10f);

            if (InvasionModel.Instance != null)
                InvasionModel.Instance.gameObject.SetActive(true);

            //Camera.main.orthographicSize = 5f;
        }

        public override void Hide() {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();
            ScriptHooks.RevealToolbar();

            // ModelContainer.SetActive(false);

            if (InvasionModel.Instance != null)
            {
                InvasionModel.Instance.gameObject.SetActive(false);
            }
        }
    }
}