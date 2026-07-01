using AIS.Model;
using AIS.Narrative;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AIS.Narrative
{
    public sealed class MapDisplayPanel : SharedPanel
    {
        public Button travelButton;

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
            travelButton.onClick.AddListener(OnTravelButtonClicked);
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
            travelPointsDisplays[selectedHubIdx].TravelToSelectedLocation(false);

            selectedHubIdx = currentHubIdx;
            TravelToSelectedHub(false);
            
            // Zoom out
            //Camera.main.transform.position = hubSelectionDisplay.cameraTransform;
            //Camera.main.orthographicSize = 5f;

            // TODO: current code is temporary -- implement proper animation later
            //travelButton.GetComponent<RoundedRectGraphic>().color = Color.white;
            //modelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.gray : Color.white;
        }

        /// <summary>
        /// Travels to the location currently selected on the active hub, then closes the map
        /// and restores the dialogue panel.
        /// </summary>
        private void OnTravelButtonClicked()
        {
            // userTriggered: true so the travel fires the OnLocationChanged script event and advances the narrative.
            travelPointsDisplays[currentHubIdx].TravelToSelectedLocation(true);

            Hide();
            Find.GuiModule<DialoguePanel>().SetVisible(true);
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
                TravelToSelectedHub(true);
            else
                hubSelectionDisplay.SelectLocation(index);
        }

        public void TravelToSelectedHub(bool userTriggered)
        {
            // Zoom in
            Camera.main.transform.position = travelPointsDisplays[selectedHubIdx].cameraTransform;
            Camera.main.orthographicSize = 2.5f;

            currentHubIdx = selectedHubIdx;
            hubSelectionDisplay.gameObject.SetActive(false);
            hubSelectionDisplay.TravelToSelectedLocation(userTriggered);
            travelPointsDisplays[currentHubIdx].gameObject.SetActive(true);
        }

        //TODO: control location accessibility via script hooks
        public void UnlockLocation(MapLocation location)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            int locationIdx = currentHub.IndexOfLocation(location);

            currentHub.UnlockedLocations.Add(currentHub.locations[locationIdx].MainImg);
            currentHub.locations[locationIdx].TimeDisplay.SetActive(true);
            currentHub.locations[locationIdx].NextCardToFind.SetActive(true);
        }

        public void SetInThreadLocks()
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            currentHub.UnlockedLocations = new List<Image>();
        }

        public void ReturnTo(int locationIdx)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            currentHub.EnableReturnTo(locationIdx);
        }

        public void SetTravelTimeFromCurrentTo(MapLocation location, int chunks, bool isRevealed)
        {
            int destinationIdx = travelPointsDisplays[currentHubIdx].IndexOfLocation(location);
            travelPointsDisplays[currentHubIdx].locations[destinationIdx].SetTravelTime(chunks);
            travelPointsDisplays[currentHubIdx].locations[destinationIdx].TimeDisplay.SetActive(isRevealed);
        }

        // TODO: Delete this in later development
        public void SetNextCardAtLocation(int locationIdx, PlayerStatId suit, bool isActionable)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            TravelPoint location = currentHub.locations[locationIdx];
            location.SetNextCardToFind(suit, isActionable);
        }

        public void SetNextCardAtLocation(MapLocation location, PlayerStatId suit, bool isActionable)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            int locationIdx = currentHub.IndexOfLocation(location);
            TravelPoint travelPoint = currentHub.locations[locationIdx];
            travelPoint.SetNextCardToFind(suit, isActionable);
        }

        public void ClearNextCardAtLocation(int locationIdx)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            currentHub.locations[locationIdx].NextCardToFind.SetActive(false);
        }
    }
}