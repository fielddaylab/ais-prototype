using AIS.Model;
using AIS.Narrative;
using BeauUtil;
using BeauUtil.UI;
using FieldDay;
using FieldDay.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Travel Info Display")]
        public TMP_Text TravelInfo;
        public Image TravelTimeInfo;
        public Sprite[] TimeChunkSprites;

        protected override void Awake()
        {
            base.Awake();
            Hide();
            ClearTravelInfo();
            travelButton.onClick.AddListener(OnTravelButtonClicked);
            currentHubIdx = 0;
            ShowMap(); // default to travel mode
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

            // reset selections
            selectedHubIdx = currentHubIdx;
            travelPointsDisplays[selectedHubIdx].SelectLocation(currentHubIdx);

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

            foreach(TravelPointsDisplay hub in travelPointsDisplays)
            {
                hub.RefreshTravelPointLocks();
            }
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

        /// <summary>
        /// Display origination, destinaation, travel time info.
        /// </summary>
        /// <param name="originName"></param>
        /// <param name="destinationName"></param>
        /// <param name="chunks"></param>
        public void ShowTravelInfo(string originName, string destinationName, int chunks)
        {
            if (TravelInfo != null)
            {
                TravelInfo.gameObject.SetActive(true);
                TravelInfo.SetText(string.Format("{0} to {1}", originName, destinationName));
            }

            SetTravelTimeSprite(chunks);
        }

        public void ClearTravelInfo()
        {
            if (TravelInfo != null) { TravelInfo.SetText(string.Empty); }
            SetTravelTimeSprite(0);
        }

        private void SetTravelTimeSprite(int units)
        {
            if (TravelTimeInfo == null || TimeChunkSprites == null || TimeChunkSprites.Length == 0) { return; }

            units = Mathf.Clamp(units, 0, TimeChunkSprites.Length - 1);

            TravelTimeInfo.enabled = units > 0;
            if (units > 0) { TravelTimeInfo.sprite = TimeChunkSprites[units]; }
        }

        //TODO: control location accessibility via script hooks
        public void UnlockLocation(MapLocation location)
        {
            if (!TryGetTravelPoint(location, out TravelPoint target)) { return; }

            travelPointsDisplays[currentHubIdx].UnlockedLocations.Add(target.MainImg);
            target.UpdateTimeBlockVisual(target.Chunks);
            // Only advertise a card here if one has actually been assigned to this location.
            target.NextCardToFind.SetActive(target.HasNextAsset);
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
            TravelPoint target = travelPointsDisplays[currentHubIdx].locations[destinationIdx];

            target.TimeDisplay.gameObject.SetActive(isRevealed);
            target.SetTravelTime(chunks);
            target.UpdateTimeBlockVisual(chunks, isRevealed);
        }

        public void SetNextAssetAtLocation(MapLocation location, in NextAssetInfo asset)
        {
            if (!TryGetTravelPoint(location, out TravelPoint travelPoint)) { return; }

            travelPoint.SetNextAsset(asset);
        }

        public void ClearNextAssetAtLocation(MapLocation location)
        {
            if (!TryGetTravelPoint(location, out TravelPoint travelPoint)) { return; }

            travelPoint.ClearNextAsset();
        }

        /// <summary>
        /// Clears the pip from every location advertising the given card. Called once the player
        /// has the card, so the map stops pointing them at something they have already found.
        /// Sweeps all hubs, since the player may collect a card after moving on from the hub
        /// the pip was set on.
        /// </summary>
        public void ClearNextAsset(StringHash32 assetId)
        {
            if (assetId.IsEmpty) { return; }

            foreach (TravelPointsDisplay hub in travelPointsDisplays)
            {
                foreach (TravelPoint travelPoint in hub.locations)
                {
                    if (travelPoint.NextAsset.Id == assetId)
                    {
                        travelPoint.ClearNextAsset();
                    }
                }
            }
        }

        // Locations are authored per hub, so a name that is not on the current hub has no travel point.
        private bool TryGetTravelPoint(MapLocation location, out TravelPoint travelPoint)
        {
            TravelPointsDisplay currentHub = travelPointsDisplays[currentHubIdx];
            int locationIdx = currentHub.IndexOfLocation(location);
            if (locationIdx < 0)
            {
                Debug.LogWarning($"[MapDisplayPanel] No travel point for location '{location}' on the current hub.", this);
                travelPoint = null;
                return false;
            }

            travelPoint = currentHub.locations[locationIdx];
            return true;
        }
    }
}