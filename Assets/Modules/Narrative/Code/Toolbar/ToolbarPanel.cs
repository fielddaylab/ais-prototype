using System.Collections.Generic;
using AIS.Model;
using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class ToolbarPanel : BaseGuiModule, IScenePreload {
        public ToolbarButton StatsButton;
        public ToolbarButton MapButton;
        public ToolbarButton EvidenceButton;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            StatsButton.Listener.onClick.Register(ToggleStats);
            MapButton.Listener.onClick.Register(ToggleMap);
            EvidenceButton.Listener.onClick.Register(ToggleEvidence);
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
                mapPanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!mapPanel.IsShowing());
            InvasionModel.Instance.gameObject.SetActive(mapPanel.IsShowing());
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
    }
}