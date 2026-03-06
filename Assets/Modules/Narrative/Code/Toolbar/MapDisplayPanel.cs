using AIS.Model;
using AIS.Narrative;
using FieldDay;
using FieldDay.UI;
using UnityEngine;
using UnityEngine.UI;
using BeauUtil.UI;

namespace AIS.Narrative {
    public sealed class MapDisplayPanel : SharedPanel {
        private enum MapMode {
            Travel, // move between locations
            Model // view the invasion model
        }

        private MapMode? currMode = null;
        public Button travelButton, modelButton;
        public GameObject travelPointContainer;

        protected override void Awake() {
            base.Awake();
            Hide();

            travelButton.onClick.AddListener(() => SwitchMapMode(MapMode.Travel));
            modelButton.onClick.AddListener(() => SwitchMapMode(MapMode.Model));
        }

        public override void Show() {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);

            SwitchMapMode(MapMode.Travel); // default
        }

        public override void Hide() {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();

            if (InvasionModel.Instance != null)
                SwitchMapMode(MapMode.Model); // enable all icons in InvasionModel before closing
        }

        private void SwitchMapMode(MapMode newMode) {
            if (currMode == newMode) return;

            currMode = newMode;
            Debug.Log($"[MapDisplayPanel] Switch map to {newMode}");

            Transform container = InvasionModel.Instance.gameObject.transform.GetChild(1);
            bool isTravelMode = newMode == MapMode.Travel;

            for (int i = 0; i < container.childCount; i++) {
                GameObject child = container.GetChild(i).gameObject;
                // Travel mode: Hide icons
                // Model mode: Show icons
                if (child.name.StartsWith("Pathway")) {
                    child.transform.GetChild(1).gameObject.SetActive(!isTravelMode);
                    child.transform.GetChild(2).gameObject.SetActive(!isTravelMode);
                }
                else if (child.name.StartsWith("Species")) {
                    child.SetActive(!isTravelMode);
                }
            }

            travelPointContainer.SetActive(isTravelMode);

            // TODO: current code is temporary -- implement proper animation later
            travelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.white : Color.gray;
            modelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.gray : Color.white;
        }
    }
}