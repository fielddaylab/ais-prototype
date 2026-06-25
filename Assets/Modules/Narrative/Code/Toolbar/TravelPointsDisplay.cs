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
using TMPro;

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

        public TravelPoint[] locations;
        [HideInInspector] public List<Image> UnlockedLocations = new List<Image>();
        public Path[] paths;

        private int currentLocationIdx;
        private int selectedLocationIdx;

        public Button travelButton;
        public Button returnButton;
        public Image LocPointer;

        private void Awake()
        {
            returnButton.gameObject.SetActive(false);
            UnlockedLocations.Add(locations[0].MainImg);
            SetCurrentLocation(0);
            locations[currentLocationIdx].MainImg.color = Color.cyan;
            travelButton.interactable = false;
        }

        private void Start()
        {
            RectTransform pointerTransform = LocPointer.GetComponent<RectTransform>();
            RectTransform currentLocTransform = locations[currentLocationIdx].MainImg.GetComponent<RectTransform>();
            pointerTransform.position = currentLocTransform.position;
        }

        public void SetCurrentLocation(int index)
        {
            // Deselect current location
            locations[currentLocationIdx].MainImg.color = Color.cyan;
            for (int i = 0; i < locations.Length; i++)
            {
                if (i == currentLocationIdx) { continue; }

                if (UnlockedLocations.Contains(locations[i].MainImg))
                {
                    locations[i].MainImg.color = Color.magenta;
                    locations[i].GetComponent<Button>().interactable = true;
                }
                else
                {
                    locations[i].MainImg.color = Color.grey;
                    locations[i].GetComponent<Button>().interactable = false;
                    locations[i].NextCardToFind.SetActive(false);
                }
                locations[i].Time.SetActive(false);
            }

            currentLocationIdx = index;
            selectedLocationIdx = index;
            RectTransform pointerTransform = LocPointer.GetComponent<RectTransform>();
            RectTransform currentLocTransform = locations[currentLocationIdx].MainImg.GetComponent<RectTransform>();
            if (pointerTransform != null)
            {
                pointerTransform.position = currentLocTransform.position;
            }

            travelButton.interactable = false;
            //ScriptHooks.DisableMapButton();
        }

        public void SelectLocation(int index)
        {
            if (index == currentLocationIdx)
            {
                locations[currentLocationIdx].MainImg.color = (locations[currentLocationIdx].MainImg.color == Color.yellow) ? Color.cyan : Color.yellow;
                selectedLocationIdx = currentLocationIdx;
                return;
            }

            if (!UnlockedLocations.Contains(locations[index].MainImg))
            {
                travelButton.interactable = false;
                return;
            }

            locations[index].Time.SetActive(true);

            // Deselect current selected location
            if (selectedLocationIdx != index)
            {
                locations[selectedLocationIdx].MainImg.color = Color.magenta;
                locations[currentLocationIdx].MainImg.color = Color.cyan;
            }
            else
            {
                locations[selectedLocationIdx].MainImg.color = Color.magenta;
                selectedLocationIdx = 0;
                return;
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

                // TODO: Set up paths between every pair of locations that can be traveled one to another
            selectedLocationIdx = index;
            foreach (Path path in paths)
            {
                if (path.connectedLocations.Contains(locations[currentLocationIdx].MainImg) &&
                    path.connectedLocations.Contains(locations[index].MainImg))
                {
                    path.line.gameObject.SetActive(true);
                }
                else
                    path.line.gameObject.SetActive(false);
            }
            locations[index].MainImg.color = Color.yellow;

            travelButton.interactable = true;
        }


        public void EnableReturnTo(int originIdx)
        {
            selectedLocationIdx = originIdx;
            UnlockedLocations = new List<Image>() { locations[originIdx].MainImg };

            returnButton.gameObject.SetActive(true);
            returnButton.interactable = true;
        }

        public void TravelToSelectedLocation()
        {   
            Debug.Log($"[TravelPointsDisplay] Travel to {locations[selectedLocationIdx]}");
            SetCurrentLocation(selectedLocationIdx);
        }
    }
}