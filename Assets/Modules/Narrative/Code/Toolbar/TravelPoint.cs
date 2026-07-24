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

        [Header("Time Display")]
        [SerializeField] private Sprite[] m_UnitSprites;

        private ToolbarTimeChunk m_TimeChunk;
        private int m_CurrentUnits = -1;
        [SerializeField] private float m_FlashDuration = 0.35f;

        public void UpdateTimeBlockVisual(int units, bool animate = false)
        {
            if (m_TimeChunk == null)
            {
                m_TimeChunk = TimeDisplay.GetComponentInChildren<ToolbarTimeChunk>();
                if (m_TimeChunk == null)
                {
                    Debug.LogWarning($"[TravelPoint] {name}: Time object has no ToolbarTimeChunk", this);
                    TimeDisplay.SetActive(units > 0);
                    return;
                }
            }

            units = Mathf.Clamp(units, 0, m_UnitSprites.Length - 1);
            bool decreased = m_CurrentUnits > 0 && units < m_CurrentUnits;
            int previousUnits = m_CurrentUnits;
            m_CurrentUnits = units;

            TimeDisplay.SetActive(units > 0);
            if (units <= 0)
            {
                m_TimeChunk.Display.enabled = false;
            }
            else
            {
                m_TimeChunk.Display.enabled = true;
                m_TimeChunk.Display.sprite = m_UnitSprites[units];
            }

            // flash the lost wedges
            if (animate && decreased)
            {
                Image flashImg = m_TimeChunk.Flash as Image;
                if (flashImg != null)
                {
                    flashImg.sprite = m_UnitSprites[previousUnits];
                }
                m_TimeChunk.Flash.gameObject.SetActive(true);
                m_TimeChunk.Flash.canvasRenderer.SetAlpha(1f);
                m_TimeChunk.Flash.CrossFadeAlpha(0f, m_FlashDuration, true);
            }
            else
            {
                m_TimeChunk.Flash.gameObject.SetActive(false);
            }
        }

        public void SetTravelTime(int chunks)
        {
            // TODO: decide the exact amount of time needed
            this.Chunks = chunks;
        }

        public void SetNextCardToFind(PlayerStatId suit, bool isActionable)
        {
            //location.NextCardToFind.GetChild(0).GetComponent<Image>().color = isActionable ? new Color(255, 165, 0, 255) : Color.blue;
            //location.NextCardToFind.GetComponent<Image>()[1].sprite = CardVisualLookupUtility.LookupSuitIcon(suit);

            NextCardToFind.SetActive(true);
            Transform BG = NextCardToFind.gameObject.transform.GetChild(0);
            Transform SuitIcon = NextCardToFind.gameObject.transform.GetChild(1);

            Image BGImage = BG.GetComponent<Image>();
            BGImage.color = isActionable ? new Color(1.0f, 0.647f, 0.0f, 1.0f) : new Color(0.2956123f, 0.6010253f, 0.8584906f, 1f);
            SuitIcon.gameObject.GetComponent<Image>().sprite = CardVisualLookupUtility.LookupSuitIcon(suit);
        }
    }
}
