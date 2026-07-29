using AIS.Model;
using AIS.Narrative;
using BeauRoutine;
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
                RectTransform parent = (RectTransform)pointerTransform.parent;

                // Convert the location's world position into the pointer's parent local space
                Vector3 world = currentLocTransform.position;
                Vector2 local = parent.InverseTransformPoint(world);

                local.y += currentLocTransform.rect.height * 0.5f;
                pointerTransform.anchoredPosition = local;
            }
        }

        public void SetCurrentLocation(int index)
        {
            currentLocationIdx = index;
            selectedLocationIdx = -1;

            for (int i = 0; i < locations.Length; i++)
            {
                bool unlocked = i == currentLocationIdx || UnlockedLocations.Contains(locations[i].MainImg);

                locations[i].GetComponent<Button>().interactable = unlocked;
                locations[i].UpdateTimeBlockVisual(unlocked ? locations[i].Chunks : 0);

                if (i == currentLocationIdx) { continue; }

                if (unlocked)
                {
                    // TODO: refine logics for checking if there are any cards waiting to be found at a location.
                    Transform BG = locations[i].NextCardToFind.gameObject.transform.GetChild(0);
                    Transform SuitIcon = locations[i].NextCardToFind.gameObject.transform.GetChild(1);
                    locations[i].NextCardToFind.SetActive(
                        BG.GetComponent<Image>().color != Color.white
                        && SuitIcon.GetComponent<Image>().sprite != null);
                }
                else
                {
                    locations[i].NextCardToFind.SetActive(false);
                }
            }

            RefreshTravelPointHighlight();

            if (isActiveAndEnabled)
            {
                StartCoroutine(UpdatePointerPositionNextFrame(index));
            }

            if (LocPointer == null || locations[index].MainImg == null) return;

            RectTransform pointerTransform = LocPointer.GetComponent<RectTransform>();
            RectTransform locTransform = locations[index].MainImg.GetComponent<RectTransform>();
            if (pointerTransform == null || locTransform == null) return;

            RectTransform parent = (RectTransform)pointerTransform.parent;
            Vector3 world = locTransform.position;
            Vector2 local = parent.InverseTransformPoint(world);
            local.y += locTransform.rect.height * 0.5f;
            pointerTransform.anchoredPosition = local;

            travelButton.interactable = false;
        }

        public void SelectLocation(int index)
        {
            if (index == selectedLocationIdx)
            {
                selectedLocationIdx = -1;
                PathLine.gameObject.SetActive(false);
                travelButton.interactable = false;
                mapDisplayPanel.ClearTravelInfo();
                RefreshTravelPointHighlight();
                return;
            }

            if (!UnlockedLocations.Contains(locations[index].MainImg))
            {
                travelButton.interactable = false;
                return;
            }

            selectedLocationIdx = index;
            RefreshTravelPointHighlight();

            if (index == currentLocationIdx)
            {
                // Selecting where you already are: nothing to travel to, no path to draw.
                PathLine.gameObject.SetActive(false);
                travelButton.interactable = false;
                mapDisplayPanel.ClearTravelInfo();
                return;
            }

            DrawPath(new Path() {
                Origin = locations[currentLocationIdx].MainImg,
                Destination = locations[index].MainImg
            });

            mapDisplayPanel.ShowTravelInfo(
                locations[currentLocationIdx].LocationName.ToString(),
                locations[index].LocationName.ToString(),
                locations[index].Chunks);

            travelButton.interactable = true;

            // Return to hub selection if player is currently at hub and selected hub
            /*
            if (currentLocationIdx == 0 && index == 0)
            {
                mapDisplayPanel.ShowHubSelectionPanel();
                return;
            }
            */
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

            RefreshTravelPointHighlight();
        }

        public void TravelToSelectedLocation(bool userTriggered)
        {
            if (selectedLocationIdx < 0) return;

            Debug.Log($"[TravelPointsDisplay] Travel to {locations[selectedLocationIdx]}");

            // bool locationChanged = currentLocationIdx != selectedLocationIdx;
            MapLocation location = locations[selectedLocationIdx].LocationName;

            // If the destination costs more time than the player has, enter the OutOfTime
            // fallback instead of traveling (do not move, spend time, or trigger the location).
            if (currentLocationIdx != selectedLocationIdx
                && Game.SharedState.TryGet(out PlayerInventory inv)
                && inv.TimeRemaining < locations[selectedLocationIdx].Chunks)
            {
                if (PathLine != null)
                {
                    PathLine.gameObject.SetActive(false);
                }
                ScriptUtility.SpawnThread(DialogueChoiceUtility.OutOfTimeNodeName);
                return;
            }

            if (PathLine != null)
            {
                PathLine.gameObject.SetActive(false);
            }

            if (currentLocationIdx != selectedLocationIdx && Game.SharedState.TryGet(out PlayerInventory _))
            {
                PlayerUtility.DecreaseTime(locations[selectedLocationIdx].Chunks);
            }

            SetCurrentLocation(selectedLocationIdx);

            if (userTriggered)
            {
                using (TempVarTable table = TempVarTable.Alloc())
                {
                    table.Set("location", location.ToString());
                    ScriptUtility.Trigger("OnLocationChanged", table);
                }
            }

            // disable all travel point time block displays, allow leaf member to setup travel time
            foreach(TravelPoint travelPoint in locations)
            {
                travelPoint.TimeDisplay.gameObject.SetActive(false);
            }
        }

        public void RefreshTravelPointLocks()
        {
            foreach(TravelPoint location in locations)
            {
                if (UnlockedLocations.Contains(location.MainImg))
                {
                    location.GetComponent<Button>().interactable = true;
                }
                else
                {
                    location.GetComponent<Button>().interactable = false;
                }
            }
        }

        // TODO: Finalize how to highlight accessible locations during threads and selected locations
        private void RefreshTravelPointHighlight()
        {
            for (int i = 0; i < locations.Length; i++)
            {
                Color color;
                if (i == selectedLocationIdx) { color = Color.yellow; }
                else if (i == currentLocationIdx) { color = Color.cyan; }
                else if (UnlockedLocations.Contains(locations[i].MainImg)) { color = Color.magenta; }
                else { color = Color.grey; }

                locations[i].MainImg.color = color;
            }
        }
    }
}