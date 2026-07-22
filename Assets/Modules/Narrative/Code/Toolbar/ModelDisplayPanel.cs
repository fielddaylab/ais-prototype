using AIS.Intervene;
using AIS.Model;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class ModelDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerStatBlock> {
        //public StatDisplayWidget[] Widgets;
        [SerializeField] public GameObject ModelContainer;
        public Button ToNotesButton;
        public Button CloseButton;

        [Header("Evidence Widget Reveal")]
        public EvidenceDisplayWidget EvidenceWidget;
        public TweenSettings WidgetSlideAnim = new TweenSettings(0.3f, Curve.Smooth);
        [Tooltip("Off-screen X (anchored, widget local space) the card slides from / to.")]
        public float WidgetHiddenX = -600f;

        private Routine m_RevealRoutine;

        // Reused across dequeues -- targets are resolved fresh from the live model each time.
        private readonly List<ISimDetail> m_RevealScratch = new List<ISimDetail>(8);

        protected override void Awake() {
            base.Awake();
            ToNotesButton.onClick.AddListener(PanelUtility.ToggleEvidence);
            CloseButton.onClick.AddListener(PanelUtility.ToggleModel);
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
            Assert.IsNotNull(InvasionModel.Instance.SimDetailRegistry, "SimDetailRegistry must not be null when showing ModelDisplayPanel");
            InvasionModel.Instance.SimDetailRegistry.ApplyAll();
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
            EvidenceWidget.gameObject.SetActive(false);

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

                PopulateEvidenceWidget(evidenceId);
                yield return SlideWidgetIn();

                registry.CollectTargets(evidenceId, m_RevealScratch);
                // Marked up front so each ApplyTo below resolves to Revealed.
                registry.MarkRevealed(evidenceId);

                foreach (ISimDetail target in m_RevealScratch) {
                    yield return RevealDetailAnimated(target);
                }

                yield return SlideWidgetOut();
            }
        }

        private void PopulateEvidenceWidget(StringHash32 evidenceId) {
            EvidenceCard card = Find.NamedAsset<EvidenceCard>(evidenceId);
            Assert.IsNotNull(card, "EvidenceCard not found for queued reveal");
            EvidenceDisplayWidgetUtility.Populate(EvidenceWidget, card);
            EvidenceWidget.gameObject.SetActive(true);
            (EvidenceWidget.transform as RectTransform).SetAnchorPos(WidgetHiddenX, Axis.X);
        }

        private IEnumerator SlideWidgetIn() {
            yield return (EvidenceWidget.transform as RectTransform).AnchorPosTo(-380f, WidgetSlideAnim, Axis.X);
        }

        private IEnumerator SlideWidgetOut() {
            yield return (EvidenceWidget.transform as RectTransform).AnchorPosTo(WidgetHiddenX, WidgetSlideAnim, Axis.X);
            EvidenceWidget.gameObject.SetActive(false);
        }

        private IEnumerator RevealDetailAnimated(ISimDetail detail) {
            MonoBehaviour detailMono = detail as MonoBehaviour;
            Assert.IsNotNull(detailMono, "ISimDetail implementation must be a MonoBehaviour for sparkle reveal");

            SimDetailRegistry registry = InvasionModel.Instance.SimDetailRegistry;
            SparkleEffect effect = registry.SparklePool.Alloc();
            // Positioned before the reveal: an inactive GameObject still has a valid transform.
            effect.Prepare(detailMono.transform.position, registry.SparkleSprite);
            registry.ApplyTo(detail);
            yield return effect.Play();
            registry.SparklePool.Free(effect);
        }

        private void FlushRevealQueue() {
            Assert.IsNotNull(InvasionModel.Instance, "InvasionModel.Instance must not be null when flushing reveal queue");
            SimDetailRegistry registry = InvasionModel.Instance.SimDetailRegistry;
            Assert.IsNotNull(registry, "SimDetailRegistry must not be null when flushing reveal queue");

            while (registry.RevealQueue.Count > 0) {
                StringHash32 evidenceId = registry.RevealQueue.Dequeue();

                registry.CollectTargets(evidenceId, m_RevealScratch);
                registry.MarkRevealed(evidenceId);

                foreach (ISimDetail target in m_RevealScratch) {
                    registry.ApplyTo(target);
                }
            }
        }
    }
}