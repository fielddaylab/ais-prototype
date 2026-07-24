using System;
using AIS.Shared;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayWidget : GuiWidget {
        public Image Suit;
        public Image Illustration;
        public TMP_Text Content;
        public Image Type; // model info or actionable card

        [Header("Action Slot")]
        public CanvasGroup ActionSlot;
        public CanvasGroup ActionContents;
        public Image SlotImg;
        public Image CardPairIcon;
    }

    /// <summary>
    /// Populates an EvidenceDisplayWidget's visuals from EvidenceCard data.
    /// </summary>
    public static class EvidenceDisplayWidgetUtility {
        /// <summary>
        /// Fills in a widget's suit icon, illustration, label, and type icon from the given card.
        /// Suit and type icons are resolved from the global CardVisualLookup.
        /// </summary>
        public static void Populate(EvidenceDisplayWidget widget, EvidenceCard data) {
            if (widget == null || data == null) { return; }

            if (widget.Suit != null) {
                widget.Suit.sprite = CardVisualLookupUtility.LookupSuitIcon(data.Suit);
            }

            if (widget.Illustration != null) {
                //var locationDB = Find.GlobalAsset<LocationVisualsDB>();
                //widget.Illustration.sprite = LocationVisualsDBUtility.LookupLocationSprite(locationDB, data.ActivateLocation);
                widget.Illustration.enabled = false; // turn off based on new ui changes

            }

            if (widget.CardPairIcon != null)
            {
                widget.CardPairIcon.enabled = false;
            }

            if (widget.Content != null) {
                widget.Content.SetText(data.Label);
            }

            if (widget.Type != null) {
                widget.Type.sprite = CardVisualLookupUtility.LookupTypeIcon(data.isActionable);
            }

            if (data.isActionable && data.ActivateLocation != MapLocation.None)
            {
                widget.ActionContents.gameObject.SetActive(true);
                widget.ActionContents.GetComponentInChildren<TMP_Text>().SetText(data.ActivateLocation.ToString());

                if (data.getIsActivated())
                {
                    widget.ActionSlot.GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
            else
            {
                widget.ActionContents.gameObject.SetActive(false);
            }
        }
    }
}