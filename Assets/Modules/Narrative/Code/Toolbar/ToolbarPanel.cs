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
            return null;
        }

        private void ToggleStats() {
            var statPanel = Find.Panel<StatDisplayPanel>();
            if (statPanel.IsShowing()) {
                statPanel.Hide();
            } else {
                statPanel.Populate(Find.State<PlayerStats>().StatBlock);
                statPanel.Show();
            }
            InvasionModel.Instance.gameObject.SetActive(false);
            Find.GuiModule<DialoguePanel>().SetVisible(true);
        }

        private void ToggleMap() {
            var statPanel = Find.Panel<StatDisplayPanel>();
            statPanel.Hide();

            bool isActive = InvasionModel.Instance.gameObject.activeSelf;
            InvasionModel.Instance.gameObject.SetActive(!isActive);

            Find.GuiModule<DialoguePanel>().SetVisible(isActive);
        }
    }
}