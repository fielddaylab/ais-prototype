using AIS.Shared;
using BeauRoutine;
using FieldDay.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative
{

    public class TravelPoint : BatchedComponent
    {
        public MapLocation LocationName;
        public Image MainImg;
        public Image EmphasisImg;
        public int Chunks;
        public GameObject TimeDisplay;
        public GameObject NextCardToFind;

        public void SetTravelTime(int chunks)
        {
            // TODO: decide the exact amount of time needed
            this.Chunks = chunks;
        }

        public void SetNextCardToFind(PlayerStatId suit, bool isActionable)
        {
            //location.NextCardToFind.GetChild(0).GetComponent<Image>().color = isActionable ? new Color(255, 165, 0, 255) : Color.blue;
            //location.NextCardToFind.GetComponent<Image>()[1].sprite = CardVisualLookupUtility.LookupSuitIcon(suit);

            this.NextCardToFind.SetActive(true);
            Transform BG = this.NextCardToFind.gameObject.transform.GetChild(0);
            Transform SuitIcon = this.NextCardToFind.gameObject.transform.GetChild(1);

            Image BGImage = BG.GetComponent<Image>();
            BGImage.color = isActionable ? new Color(1.0f, 0.647f, 0.0f, 1.0f) : new Color(0.2956123f, 0.6010253f, 0.8584906f, 1f);
            SuitIcon.gameObject.GetComponent<Image>().sprite = CardVisualLookupUtility.LookupSuitIcon(suit);
        }
    }
}
