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

        public CanvasGroup TimeGroup;
        public GuiCounter TimeCounter;

        IEnumerator<WorkSlicer.Result?> IScenePreload.Preload() {
            //ModelButton.Listener.onClick.Register(ToggleModel);
            MapButton.Listener.onClick.Register(PanelUtility.ToggleMap);
            EvidenceButton.Listener.onClick.Register(PanelUtility.ToggleEvidence);

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
    }

    public static class PanelUtility
    {
        public static void ToggleEvidence()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            if (evidencePanel.IsShowing())
            {
                evidencePanel.Hide();
            }
            else
            {
                mapPanel.Hide();
                modelPanel.Hide();
                InvasionModel.Instance.gameObject.SetActive(false);
                evidencePanel.Populate(Find.State<PlayerInventory>());

                var stats = Find.State<PlayerStats>();
                Debug.Log($"PlayerStats: {stats}, StatBlock: {stats?.StatBlock}");
                Debug.Log($"Stats length: {stats.StatBlock.Communicate}");
                evidencePanel.PopulateStats(Find.State<PlayerStats>().StatBlock);
                evidencePanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(true);
            InvasionModel.Instance.gameObject.SetActive(false);
        }

        public static void ToggleMap()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            if (mapPanel.IsShowing())
            {
                mapPanel.Hide();
            }
            else
            {
                modelPanel.Hide();
                InvasionModel.Instance.gameObject.SetActive(false);
                evidencePanel.Hide();
                //if (InvasionModel.Instance != null) {
                //    InvasionModel.Instance.gameObject.SetActive(false);
                //}
                mapPanel.Show();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!mapPanel.IsShowing());
        }


        public static void ToggleModel()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            var modelPanel = Find.Panel<ModelDisplayPanel>();
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();

            if (modelPanel.IsShowing())
            {
                modelPanel.Hide();
                InvasionModel.Instance.gameObject.SetActive(false);
            }
            else
            {
                mapPanel.Hide();
                evidencePanel.Hide();
                //modelPanel.Populate(Find.State<PlayerStats>().StatBlock);
                modelPanel.Show();
                //InvasionModel.Instance.GetComponentInChildren<SimDetailRegistry>().RefreshVisibility();
            }
            Find.GuiModule<DialoguePanel>().SetVisible(!modelPanel.IsShowing());
            //InvasionModel.Instance.gameObject.SetActive(modelPanel.IsShowing());
        }
    }
}