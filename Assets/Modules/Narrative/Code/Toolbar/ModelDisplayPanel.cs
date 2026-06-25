using AIS.Intervene;
using AIS.Model;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class ModelDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerStatBlock> {
        //public StatDisplayWidget[] Widgets;
        [SerializeField] public GameObject ModelContainer;
        public Button ToNotesButton;
        public Button CloseButton;

        private Routine m_RevealRoutine;

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
            m_RevealRoutine.Replace(this, PlayRevealQueue());
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
            if (InvasionModel.Instance != null && InvasionModel.Instance.gameObject.activeSelf && m_RevealRoutine.Exists()) {
                FlushRevealQueue();
            }

            m_RevealRoutine.Stop();

            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();
            ScriptHooks.RevealToolbar();

            // ModelContainer.SetActive(false);

            if (InvasionModel.Instance != null)
            {
                InvasionModel.Instance.gameObject.SetActive(false);
            }
        }

        private IEnumerator PlayRevealQueue() {
            Assert.IsNotNull(InvasionModel.Instance, "InvasionModel.Instance must not be null when playing reveal queue");
            SimDetailRegistry registry = InvasionModel.Instance.SimDetailRegistry;
            Assert.IsNotNull(registry, "SimDetailRegistry must not be null when playing reveal queue");

            while (registry.RevealQueue.Count > 0) {
                StringHash32 evidenceId = registry.RevealQueue.Dequeue();
                foreach (ISimDetail target in registry.MapEvidenceToDetail(evidenceId)) {
                    if (!registry.DetailsToShow.Contains(target)) {
                        registry.DetailsToShow.Add(target);
                        yield return RevealDetailAnimated(target);
                    }
                }
            }
        }

        private IEnumerator RevealDetailAnimated(ISimDetail detail) {
            // TODO: author reveal animation (fade-in, scale pop, etc.)
            detail.Show();
            yield break;
        }

        private void FlushRevealQueue() {
            Assert.IsNotNull(InvasionModel.Instance, "InvasionModel.Instance must not be null when flushing reveal queue");
            SimDetailRegistry registry = InvasionModel.Instance.SimDetailRegistry;
            Assert.IsNotNull(registry, "SimDetailRegistry must not be null when flushing reveal queue");

            while (registry.RevealQueue.Count > 0) {
                StringHash32 evidenceId = registry.RevealQueue.Dequeue();
                foreach (ISimDetail target in registry.MapEvidenceToDetail(evidenceId)) {
                    if (!registry.DetailsToShow.Contains(target)) {
                        registry.DetailsToShow.Add(target);
                        target.Show();
                    }
                }
            }
        }
    }
}