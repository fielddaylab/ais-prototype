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
        //public ToolbarButton ModelButton;
        //public Image ModelMissing;
        public ToolbarButton MapButton;
        public Image MapMissing;
        public ToolbarButton EvidenceButton;
        public Image EvidenceMissing;
        public ToolbarButton TimeButton;
        public CanvasGroup TimeGroup;
        public GuiCounter TimeCounter;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            //ModelButton.Listener.onClick.Register(ToggleModel);
            MapButton.Listener.onClick.Register(PanelUtility.ToggleMap);
            EvidenceButton.Listener.onClick.Register(PanelUtility.ToggleEvidence);
            TimeButton.Listener.onClick.Register(PanelUtility.ToggleTime);

            //ResetToolbarButton(ModelMissing, ModelButton, false);
            ResetToolbarButton(MapMissing, MapButton, false);
            ResetToolbarButton(EvidenceMissing, EvidenceButton, false);
            ResetTimeGroup(TimeGroup, false);

            return null;
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

        public void SetTime(int timeRemaining)
        {
            TimeCounter.SetValue(timeRemaining);
            var timePanel = Find.Panel<TimeDisplayPanel>();
            timePanel.SetTime(timeRemaining);
        }
    }

    public static class PanelUtility
    {
        public static void ToggleEvidence()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            var timePanel = Find.Panel<TimeDisplayPanel>();

            if (evidencePanel.IsShowing())
            {
                evidencePanel.Hide();
            }
            else
            {
                mapPanel.Hide();
                modelPanel.Hide();
                timePanel.Hide();
                InvasionModel.Instance.gameObject.SetActive(false);
                evidencePanel.Populate(Find.State<PlayerInventory>());

                var stats = Find.State<PlayerStats>();
                Debug.Log($"PlayerStats: {stats}, StatBlock: {stats?.StatBlock}");
                Debug.Log($"Stats length: {stats.StatBlock.Communicate}");
                evidencePanel.PopulateStats(Find.State<PlayerStats>().StatBlock);
                evidencePanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!(modelPanel.IsShowing() || evidencePanel.IsShowing() || timePanel.IsShowing()));
            InvasionModel.Instance.gameObject.SetActive(false);
        }

        public static void ToggleMap()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            var timePanel = Find.Panel<TimeDisplayPanel>();

            var toolbarPanel = Find.GuiModule<ToolbarPanel>();
            if (mapPanel.IsShowing())
            {
                mapPanel.Hide();
            }
            else
            {
                modelPanel.Hide();
                evidencePanel.Hide();
                timePanel.Hide();
                mapPanel.Show();

                // TODO: reveal time group at the appriopriate point
                ToolbarPanel.ResetTimeGroup(toolbarPanel.TimeGroup, true);
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!mapPanel.IsShowing());
        }


        /// <summary>
        /// Opens the map, hiding the other panels. If the map is already showing (a $MapChoice
        /// opens it as soon as the choice is picked, ahead of the node that sets up the travel),
        /// it stays up and re-reads the travel state instead, so either order leaves the same map.
        /// </summary>
        public static void OpenMap()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            if (mapPanel.IsShowing())
            {
                mapPanel.RefreshTravelPoints();
                return;
            }

            ToggleMap();
        }


        public static void ToggleModel()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            var timePanel = Find.Panel<TimeDisplayPanel>();

            if (modelPanel.IsShowing())
            {
                modelPanel.Hide();
                InvasionModel.Instance.gameObject.SetActive(false);
            }
            else
            {
                mapPanel.Hide();
                evidencePanel.Hide();
                timePanel.Hide();
                modelPanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!(modelPanel.IsShowing() || evidencePanel.IsShowing() || timePanel.IsShowing()));
        }

        public static void ToggleTime()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            var timePanel = Find.Panel<TimeDisplayPanel>();

            if (timePanel.IsShowing())
            {
                timePanel.Hide();

                Find.GuiModule<DialoguePanel>().SetVisible(!(modelPanel.IsShowing() || evidencePanel.IsShowing() || timePanel.IsShowing()));
            }
            else
            {
                mapPanel.Hide();
                evidencePanel.Hide();
                modelPanel.Hide();

                timePanel.Show();

                Find.GuiModule<DialoguePanel>().SetVisible(!(modelPanel.IsShowing() || evidencePanel.IsShowing() || timePanel.IsShowing()));
            }
        }
    }
}