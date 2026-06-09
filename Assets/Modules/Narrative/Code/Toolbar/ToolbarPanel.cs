using System.Collections;
using System.Collections.Generic;
using AIS.Model;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class ToolbarPanel : BaseGuiModule, IScenePreload {
        public ToolbarButton StatsButton;
        public Image StatsMissing;
        public ToolbarButton MapButton;
        public Image MapMissing;
        public ToolbarButton EvidenceButton;
        public Image EvidenceMissing;

        public CanvasGroup TimeGroup;
        public GuiCounter TimeCounter;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            StatsButton.Listener.onClick.Register(ToggleStats);
            MapButton.Listener.onClick.Register(ToggleMap);
            EvidenceButton.Listener.onClick.Register(ToggleEvidence);

            ResetToolbarButton(StatsMissing, StatsButton, false);
            ResetToolbarButton(MapMissing, MapButton, false);
            ResetToolbarButton(EvidenceMissing, EvidenceButton, false);
            ResetTimeGroup(TimeGroup, false);

            return null;
        }

        private void ToggleStats() {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var statPanel = Find.Panel<StatDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            if (statPanel.IsShowing()) {
                statPanel.Hide();
            } else {
                mapPanel.Hide();
                evidencePanel.Hide();
                statPanel.Populate(Find.State<PlayerStats>().StatBlock);
                statPanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(true);
            InvasionModel.Instance.gameObject.SetActive(false);
        }

        private void ToggleMap() {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var statPanel = Find.Panel<StatDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            if (mapPanel.IsShowing()) {
                mapPanel.Hide();
            } else {
                statPanel.Hide();
                evidencePanel.Hide();
                if (InvasionModel.Instance != null) {
                    InvasionModel.Instance.gameObject.SetActive(false);
                }
                mapPanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!mapPanel.IsShowing());
            //InvasionModel.Instance.gameObject.SetActive(mapPanel.IsShowing());
        }

        private void ToggleEvidence() {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var statPanel = Find.Panel<StatDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            if (evidencePanel.IsShowing()) {
                evidencePanel.Hide();
            } else {
                mapPanel.Hide();
                statPanel.Hide();
                evidencePanel.Populate(Find.State<PlayerInventory>());
                evidencePanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(true);
            InvasionModel.Instance.gameObject.SetActive(false);
        }
    
        static public void ResetToolbarButton(Image placeholder, ToolbarButton button, bool unlocked) {
            button.gameObject.SetActive(unlocked);
            button.Fader.blocksRaycasts = unlocked;
            button.Fader.alpha = unlocked ? 1 : 0;
            placeholder.enabled = !unlocked;
        }
        
        static public void ResetTimeGroup(CanvasGroup group, bool unlocked) {
            group.gameObject.SetActive(unlocked);
        }

        static public IEnumerator UnlockToolbarButtonAnimation(Image placeholder, ToolbarButton button) {
            button.gameObject.SetActive(true);
            button.Fader.alpha = 0;
            button.Fader.blocksRaycasts = false;

            Color originalPlaceholderColor = placeholder.color;
            placeholder.color = Color.white;

            yield return Routine.Combine(
                placeholder.FadeTo(0, 0.3f),
                button.Fader.FadeTo(1, 0.3f)
                );

            button.Fader.blocksRaycasts = true;
            placeholder.color = originalPlaceholderColor;
            placeholder.enabled = false;
        }
    
        static public IEnumerator UnlockTimeGroupAnimation(CanvasGroup group) {
            group.gameObject.SetActive(true);
            group.alpha = 0;
            group.blocksRaycasts = false;
            yield return group.FadeTo(1, 0.3f);
            group.blocksRaycasts = true;
        }
    }
}