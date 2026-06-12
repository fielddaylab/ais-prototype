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
        private MapMode? currMode = MapMode.Travel;
        public Button travelButton, modelButton;


        public GameObject travelPointsContainer;
        public GameObject mapImage;
        public TravelPointsDisplay hubSelectionDisplay;
        public TravelPointsDisplay[] travelPointsDisplays;
        private int currentHubIdx;
        private int selectedHubIdx;

        protected override void Awake()
        {
            base.Awake();
            Hide();
            //travelButton.onClick.AddListener(() => SwitchMapMode(MapMode.Travel));
            ShowMap(); // default to travel mode

            currentHubIdx = 0;
        }

        public override void Show()
        {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);

            ShowMap();
        }

        public override void Hide()
        {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();

            if (mapImage != null)
            {
                mapImage.SetActive(false);
            }

            //currMode = null;
        }

        private void ShowMap()
        {
            if (mapImage != null)
            {
                mapImage.SetActive(true);
            }

            travelPointsContainer.SetActive(true);
            selectedHubIdx = currentHubIdx;
            travelPointsDisplays[selectedHubIdx].TravelToSelectedLocation();

            selectedHubIdx = currentHubIdx;
            TravelToSelectedHub();
            
            // Zoom out
            //Camera.main.transform.position = hubSelectionDisplay.cameraTransform;
            //Camera.main.orthographicSize = 5f;

            // TODO: current code is temporary -- implement proper animation later
            //travelButton.GetComponent<RoundedRectGraphic>().color = Color.white;
            //modelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.gray : Color.white;
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