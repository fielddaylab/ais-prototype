using AIS.Model;
using AIS.Narrative;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            //public Image line;
        }

        public Vector3 cameraTransform;
        public MapDisplayPanel mapDisplayPanel;

        public TravelPoint[] locations;
        [HideInInspector] public List<Image> UnlockedLocations = new List<Image>();
        //public Path[] paths;
        public Image PathLine;

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

        /// <summary>
        /// Returns the index within <see cref="locations"/> whose <see cref="TravelPoint.LocationName"/>
        /// matches the given <paramref name="location"/>, or -1 if none match.
        /// </summary>
        public int IndexOfLocation(MapLocation location)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i].LocationName == location)
                {
                    return i;
                }
            }
            return -1;
        }
        private IEnumerator UpdatePointerPositionNextFrame(int index)
        {
            yield return null; // wait one frame for layout/LayoutOffset to settle
            RectTransform pointerTransform = LocPointer.GetComponent<RectTransform>();
            RectTransform currentLocTransform = locations[index].MainImg.GetComponent<RectTransform>();
            if (pointerTransform != null && currentLocTransform != null)
            {
                pointerTransform.position = currentLocTransform.position;
            }
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

                    // TODO: refine logics for checking if there are any cards waiting to be found at a locations.
                    // If not, disable NextCardToFind.
                    Transform BG = locations[i].NextCardToFind.gameObject.transform.GetChild(0);
                    Transform SuitIcon = locations[i].NextCardToFind.gameObject.transform.GetChild(1);
                    if (BG.GetComponent<Image>().color == Color.white || SuitIcon.GetComponent<Image>().sprite == null)
                        locations[i].NextCardToFind.SetActive(false);
                    else
                        locations[i].NextCardToFind.SetActive(true);

                }
                else
                {
                    locations[i].MainImg.color = Color.grey;
                    locations[i].GetComponent<Button>().interactable = false;
                    locations[i].UpdateTimeBlockVisual(0);
                    locations[i].NextCardToFind.SetActive(false);
                }
            }

            currentLocationIdx = index;
            selectedLocationIdx = index;

            if (isActiveAndEnabled)
            {
                StartCoroutine(UpdatePointerPositionNextFrame(index));
            }

            travelButton.interactable = false;
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

            //locations[index].Time.SetActive(true);

            // Deselect current selected location
            if (selectedLocationIdx != index)
            {
                locations[selectedLocationIdx].MainImg.color = Color.magenta;
                locations[currentLocationIdx].MainImg.color = Color.cyan;
                Path route = new Path()
                {
                    Origin = locations[currentLocationIdx].MainImg,
                    Destination = locations[index].MainImg
                };
                DrawPath(route);
            }
            else
            {
                locations[selectedLocationIdx].MainImg.color = Color.magenta;
                selectedLocationIdx = 0;
                PathLine.gameObject.SetActive(false);
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

            selectedLocationIdx = index;
            locations[index].MainImg.color = Color.yellow;
            travelButton.interactable = true;
        }

        private void DrawPath(Path path)
        {
            Vector2 originPos = path.Origin.GetComponent<RectTransform>().anchoredPosition;
            Vector2 destPos = path.Destination.GetComponent<RectTransform>().anchoredPosition;

            // set path length to be sqrt((originX - destX)^2 + (originY - destY)^2)
            float length = (float) Math.Sqrt(Math.Pow((originPos.x - destPos.x), 2) + Math.Pow((originPos.y - destPos.y), 2));
            RectTransform rt = PathLine.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(length, rt.sizeDelta.y);

            // set path position to be 0.5((originX + destX), (originY + destY))
            rt.anchoredPosition = new Vector2((float) 0.5 * (originPos.x + destPos.x), (float) 0.5 * (originPos.y + destPos.y));

            // calculate z rotation value: sin^-1(diffY / length) * Mathf.Rad2Deg
            // positive z: CCW; negative z: CW
            if (originPos.x >= destPos.x)
                rt.localRotation = Quaternion.Euler(0, 0, (float) Math.Asin((originPos.y - destPos.y) / length) * Mathf.Rad2Deg);
            else
                rt.localRotation = Quaternion.Euler(0, 0, (float) Math.Asin((destPos.y - originPos.y) / length) * Mathf.Rad2Deg);

            PathLine.gameObject.SetActive(true);
        }

        public void EnableReturnTo(int originIdx)
        {
            selectedLocationIdx = originIdx;
            UnlockedLocations = new List<Image>() { locations[originIdx].MainImg };

            returnButton.gameObject.SetActive(true);
            returnButton.interactable = true;
        }

        public void TravelToSelectedLocation(bool userTriggered)
        {
            Debug.Log($"[TravelPointsDisplay] Travel to {locations[selectedLocationIdx]}");

            // bool locationChanged = currentLocationIdx != selectedLocationIdx;
            MapLocation location = locations[selectedLocationIdx].LocationName;
            if (PathLine != null)
            {
                PathLine.gameObject.SetActive(false);
            }

            if (currentLocationIdx != selectedLocationIdx && Game.SharedState.TryGet(out PlayerInventory _))
                PlayerUtility.DecreaseTime(locations[selectedLocationIdx].Chunks);

            SetCurrentLocation(selectedLocationIdx);

            if (userTriggered)
            {
                using (TempVarTable table = TempVarTable.Alloc())
                {
                    table.Set("location", location.ToString());
                    ScriptUtility.Trigger("OnLocationChanged", table);
                }
            }
        }
    }
}