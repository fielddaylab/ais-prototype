using AIS.Model;
using AIS.Narrative;
using FieldDay;
using FieldDay.UI;
using UnityEngine;
using UnityEngine.UI;
using BeauUtil.UI;
using System.Collections.Generic;

namespace AIS.Narrative {
    public sealed class MapDisplayPanel : SharedPanel {
        private enum MapMode {
            Travel, // move between locations
            Model // view the invasion model
        }

        [System.Serializable]
        public class Hub : Location
        {
            public List<Location> locations = new List<Location>();
            // public List<Path> paths = new List<Path>();
            public Vector3 cameraTransform;
        }

        [System.Serializable]
        public class Location
        {
            public string locationName;
            public Vector2 position = new Vector2();
        }

        // [System.Serializable]
        // public class Path
        // {
        //     public Location location1, location2;
        //     public float travelTime;
        // }

        private Hub currentHub;
        private Location currentLocation;
        public List<Hub> hubs;

        private MapMode? currMode = null;
        public Button travelButton, modelButton;
        public GameObject travelPointContainer, pathContainer;
        public GameObject travelPointPrefab;

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
            SetHubPoints();

            // TODO: current code is temporary -- implement proper animation later
            travelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.white : Color.gray;
            modelButton.GetComponent<RoundedRectGraphic>().color = isTravelMode ? Color.gray : Color.white;
        }

        public void SetHubPoints()
        {
            DestroyAllTravelPoints();
            foreach(Hub hub in hubs) {
                GameObject travelPoint = Instantiate(travelPointPrefab, travelPointContainer.transform);
                travelPoint.GetComponent<Button>().onClick.AddListener(() => SelectHub(hub));
                travelPoint.GetComponent<RectTransform>().anchoredPosition = hub.position;
                travelPoint.GetComponent<CursorHint>().TooltipHeader = hub.locationName;
            }

            // Zoom out
            Camera.main.transform.position = new Vector3(-1, 0, -10);
            Camera.main.orthographicSize = 5f;
        }

        public void SelectHub(Hub hub) {
            Debug.Log($"[MapDisplayPanel] Selected hub: {hub.locationName}");
            currentHub = hub;
            currentLocation = hub.locations[0];

            DestroyAllTravelPoints();
            foreach(Location loc in hub.locations) {
                GameObject travelPoint = Instantiate(travelPointPrefab, travelPointContainer.transform);
                travelPoint.GetComponent<Button>().onClick.AddListener(() => SelectLocation(loc));
                travelPoint.GetComponent<RectTransform>().anchoredPosition = loc.position;
                travelPoint.GetComponent<CursorHint>().TooltipHeader = loc.locationName;
            }

            // Zoom in
            Camera.main.transform.position = hub.cameraTransform;
            Camera.main.orthographicSize = 2.5f;
        }

        public void SelectLocation(Location loc) {
            currentLocation = loc;
            Debug.Log($"[MapDisplayPanel] Selected location: {loc.locationName}");
        }

        private void DestroyAllTravelPoints() {
            foreach (Transform child in travelPointContainer.transform)
                Destroy(child.gameObject);
        }

    }
}