using AIS.Shared;
using BeauRoutine;
using BeauUtil;
using FieldDay.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative
{
    /// <summary>
    /// Which kind of card the map is pointing the player toward at a location.
    /// Drives the color of the location's NextCardToFind pip.
    /// </summary>
    public enum NextAssetType
    {
        Evidence,
        Action,
    }

    /// <summary>
    /// The card a map location is advertising as still waiting to be found there.
    /// Held by the TravelPoint itself, so the location-to-card mapping cannot drift
    /// out of sync with what the pip is displaying.
    /// </summary>
    public struct NextAssetInfo
    {
        public StringHash32 Id;
        public NextAssetType Type;
        public PlayerStatId Suit;
    }

    public class TravelPoint : BatchedComponent
    {
        // Pip background colors: action cards read orange, evidence cards light blue.
        static private readonly Color ActionAssetColor = new Color(1.0f, 0.647f, 0.0f, 1.0f);
        static private readonly Color EvidenceAssetColor = new Color(0.2956123f, 0.6010253f, 0.8584906f, 1f);

        public MapLocation LocationName;
        public Image MainImg;
        public Image EmphasisImg;
        public int Chunks;
        public GameObject TimeDisplay;
        public GameObject NextCardToFind;

        /// <summary>
        /// Where the map's "you are here" pointer sits when the player is at this location.
        /// Authored per-location so the pointer can dodge labels and icons that differ from point to point.
        /// </summary>
        public RectTransform PointerPos;

        [Header("Time Display")]
        [SerializeField] private Sprite[] m_UnitSprites;

        private ToolbarTimeChunk m_TimeChunk;
        private int m_CurrentUnits = -1;
        [SerializeField] private float m_FlashDuration = 0.35f;

        /// <summary>
        /// The card assigned to this location, if any.
        /// </summary>
        public NextAssetInfo NextAsset { get; private set; }

        /// <summary>
        /// Whether a card has been assigned to this location. The map uses this to decide
        /// whether the pip should reappear when the location is unlocked or revisited.
        /// </summary>
        public bool HasNextAsset { get { return !NextAsset.Id.IsEmpty; } }

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
            TimeChunkUtility.Populate(m_TimeChunk, m_UnitSprites, units);

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
        }

        public void SetTravelTime(int chunks)
        {
            // TODO: decide the exact amount of time needed
            this.Chunks = chunks;
        }

        /// <summary>
        /// Shows the pip advertising the asset waiting at this location, colored by card type
        /// and stamped with that card's suit.
        /// </summary>
        public void SetNextAsset(in NextAssetInfo asset)
        {
            NextAsset = asset;
            NextCardToFind.SetActive(true);

            // Children are authored on the NextCardIcon prefab: 0 = BG, 1 = Suit Icon.
            Transform BG = NextCardToFind.transform.GetChild(0);
            Transform SuitIcon = NextCardToFind.transform.GetChild(1);

            BG.GetComponent<Image>().color = asset.Type == NextAssetType.Action ? ActionAssetColor : EvidenceAssetColor;
            SuitIcon.GetComponent<Image>().sprite = CardVisualLookupUtility.LookupSuitIcon(asset.Suit);
        }

        public void ClearNextAsset()
        {
            NextAsset = default;
            NextCardToFind.SetActive(false);
        }
    }

    static public class TimeChunkUtility
    {
        // sprites are indexed by unit count (element 0 unused - zero units hides the display).
        static public void Populate(ToolbarTimeChunk chunk, Sprite[] sprites, int units)
        {
            if (chunk == null || sprites == null || sprites.Length == 0) { return; }

            units = Mathf.Clamp(units, 0, sprites.Length - 1);

            chunk.Display.enabled = units > 0;
            if (units > 0) { chunk.Display.sprite = sprites[units]; }

            if (chunk.Flash != null)
            {
                chunk.Flash.canvasRenderer.SetAlpha(0f);
                chunk.Flash.gameObject.SetActive(false);
            }
        }
    }
}
