using AIS.Model;
using AIS.Narrative;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
using BeauUtil.UI;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Narrative
{
    public sealed class TravelPointsDisplay : MonoBehaviour
    {
        [System.Serializable]
        public class Path
        {
            //public Image[] connectedLocations;
            public Image Origin;
            public Image Destination;
            public Image[] connectedLocations => new Image[] { Origin, Destination };
            // origin & dest
            //public float travelTime;
            //public GameObject time;
            public Image line;
        }

        public Vector3 cameraTransform;
        public MapDisplayPanel mapDisplayPanel;

        public Image[] locations;
        public Path[] paths;

        private int currentLocationIdx;
        private int selectedLocationIdx;
        private Path selectedPath;

        public Button travelButton;

        private void Awake()
        {
            SetCurrentLocation(0);
            locations[currentLocationIdx].color = Color.cyan;
        }

        public void SetCurrentLocation(int index)
        {
            // Deselect current location
            locations[currentLocationIdx].color = Color.cyan;
            for (int i = 0; i < locations.Length; i++)
            {
                if (i == currentLocationIdx) { continue; }
                locations[i].color = Color.magenta;
            }
            
            // Deselect selected location and path
            //locations[selectedLocationIdx].color = Color.yellow;
            selectedPath = null;
            
            currentLocationIdx = index;
            selectedLocationIdx = index;

            foreach (Image location in locations)
            {
                location.GetComponent<Button>().interactable = true;
            }

            foreach (Path path in paths)
            {
                bool isConnected = path.connectedLocations.Contains(locations[currentLocationIdx]);
                path.line.color = Color.white;
                path.connectedLocations[0].GetComponent<Button>().interactable = true;
                path.connectedLocations[1].GetComponent<Button>().interactable = true;
            }

            //locations[currentLocationIdx].color = Color.magenta;
            travelButton.interactable = false;
        }

        public void SelectLocation(int index)
        {
            // Deselect current selected location
            if (selectedLocationIdx != index)
            {
                locations[selectedLocationIdx].color = Color.magenta;
                locations[currentLocationIdx].color = Color.cyan;
                //selectedPath.line.color = Color.white;
                //selectedPath.time.SetActive(false);
            }

            // Return to hub selection if player is currently at hub and selected hub
            /*
            if (currentLocationIdx == 0 && index == 0)
            {
                mapDisplayPanel.ShowHubSelectionPanel();
                return;
            }
            */

            // Highlight new selected location and path
            selectedLocationIdx = index;
            foreach (Path path in paths)
            {
                if (path.connectedLocations.Contains(locations[currentLocationIdx]) &&
                    path.connectedLocations.Contains(locations[index]))
                    selectedPath = path;
            }
            locations[index].color = Color.yellow;
            //selectedPath.line.color = Color.yellow;
            //selectedPath.time.SetActive(true);

            travelButton.interactable = true;
        }

        public void TravelToSelectedLocation()
        {   
            Debug.Log($"[TravelPointsDisplay] Travel to {locations[selectedLocationIdx]}");
            SetCurrentLocation(selectedLocationIdx);
        }
    }
}