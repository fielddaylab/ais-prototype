using AIS.Model;
using AIS.Narrative;
using FieldDay;
using FieldDay.UI;
using UnityEngine;
using UnityEngine.UI;
using BeauUtil.UI;

namespace AIS.Narrative
{
    public sealed class MapDisplayPanel : SharedPanel
    {
        private enum MapMode
        {
            Travel, // move between locations
            Model // view the invasion model
        }
        private MapMode? currMode = null;
        public Button travelButton, modelButton;


        public GameObject travelPointsContainer;
        public TravelPointsDisplay hubSelectionDisplay;
        public TravelPointsDisplay[] travelPointsDisplays;
        private int currentHubIdx;
        private int selectedHubIdx;

        protected override void Awake()
        {
            base.Awake();
            Hide();

            travelButton.onClick.AddListener(() => SwitchMapMode(MapMode.Travel));
            modelButton.onClick.AddListener(() => SwitchMapMode(MapMode.Model));
            currentHubIdx = 0;
        }

        public override void Show()
        {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);

            SwitchMapMode(MapMode.Travel); // default
        }

        public override void Hide()
        {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();

            if (InvasionModel.Instance != null)
                SwitchMapMode(MapMode.Model); // enable all icons in InvasionModel before closing
        }

        private void SwitchMapMode(MapMode newMode)
        {
            if (currMode == newMode) return;

            currMode = newMode;
            Debug.Log($"[MapDisplayPanel] Switch map to {newMode}");

            Transform container = InvasionModel.Instance.gameObject.transform.GetChild(1);
            bool isTravelMode = newMode == MapMode.Travel;

            for (int i = 0; i < container.childCount; i++)
            {
                GameObject child = container.GetChild(i).gameObject;
                // Travel mode: Hide icons
                // Model mode: Show icons
                if (child.name.StartsWith("Pathway"))
                {
                    child.transform.GetChild(1).gameObject.SetActive(!isTravelMode);
                    child.transform.GetChild(2).gameObject.SetActive(!isTravelMode);
                }
                else if (child.name.StartsWith("Species"))
                {
                    child.SetActive(!isTravelMode);
                }
            }

            travelPointsContainer.SetActive(isTravelMode);
            if (isTravelMode)
            {
                selectedHubIdx = currentHubIdx;
                TravelToSelectedHub();
            }
            else
            {
                // Zoom out
                Camera.main.transform.position = hubSelectionDisplay.cameraTransform;
                Camera.main.orthographicSize = 5f;
            }

            // TODO: current code is temporary -- implement proper animation later
            travelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.white : Color.gray;
            modelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.gray : Color.white;
        }

        public void ShowHubSelectionPanel()
        {
            // Zoom out
            Camera.main.transform.position = hubSelectionDisplay.cameraTransform;
            Camera.main.orthographicSize = 5f;

            travelPointsDisplays[currentHubIdx].gameObject.SetActive(false);
            hubSelectionDisplay.gameObject.SetActive(true);

            hubSelectionDisplay.SetCurrentLocation(currentHubIdx);
        }

        public void SelectHub(int index)
        {
            selectedHubIdx = index;

            if (currentHubIdx == index)
                TravelToSelectedHub();
            else
                hubSelectionDisplay.SelectLocation(index);
        }

        public void TravelToSelectedHub()
        {
            // Zoom in
            Camera.main.transform.position = travelPointsDisplays[selectedHubIdx].cameraTransform;
            Camera.main.orthographicSize = 2.5f;

            currentHubIdx = selectedHubIdx;
            hubSelectionDisplay.gameObject.SetActive(false);
            hubSelectionDisplay.TravelToSelectedLocation();
            travelPointsDisplays[currentHubIdx].gameObject.SetActive(true);
        }
    }
}