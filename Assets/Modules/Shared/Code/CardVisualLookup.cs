using AIS.Narrative;
using FieldDay;
using FieldDay.Assets;
using UnityEngine;

namespace AIS.Shared {
    /// <summary>
    /// Global lookup for card visuals (suit icons, etc.) authored in the inspector.
    /// Given an EvidenceCard's suit, query for the matching suit icon.
    /// </summary>
    [CreateAssetMenu(menuName = "AIS/Card Visual Lookup")]
    public sealed class CardVisualLookup : GlobalAsset {
        [Header("Suit Icons")]
        public SuitIconEntry[] SuitIcons;

        [Header("Type Icons")]
        public Sprite ModelIcon;      // shown for non-actionable (model info) cards
        public Sprite ActionableIcon; // shown for actionable cards
    }

    /// <summary>
    /// Pairs a stat (suit) with the icon shown for cards of that suit.
    /// Authored on CardVisualLookup; resolved at runtime via CardVisualLookupUtility.
    /// </summary>
    [System.Serializable]
    public struct SuitIconEntry {
        public PlayerStatId Suit;
        public Sprite Icon;
    }

    /// <summary>
    /// Utility class for CardVisualLookup.
    /// </summary>
    public static class CardVisualLookupUtility {
        /// <summary>
        /// Returns the suit icon authored for the given suit, or null if no entry exists.
        /// </summary>
        public static Sprite LookupSuitIcon(PlayerStatId suit, CardVisualLookup lookup) {
            if (lookup == null || lookup.SuitIcons == null) { return null; }

            for (int i = 0; i < lookup.SuitIcons.Length; i++) {
                if (lookup.SuitIcons[i].Suit == suit) {
                    return lookup.SuitIcons[i].Icon;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the suit icon for the given suit using the globally-loaded CardVisualLookup.
        /// </summary>
        public static Sprite LookupSuitIcon(PlayerStatId suit) {
            return LookupSuitIcon(suit, Find.GlobalAsset<CardVisualLookup>());
        }

        /// <summary>
        /// Returns the type icon (actionable vs. model info) authored on the lookup, or null if none.
        /// </summary>
        public static Sprite LookupTypeIcon(bool isActionable, CardVisualLookup lookup) {
            if (lookup == null) { return null; }
            return isActionable ? lookup.ActionableIcon : lookup.ModelIcon;
        }

        /// <summary>
        /// Returns the type icon (actionable vs. model info) using the globally-loaded CardVisualLookup.
        /// </summary>
        public static Sprite LookupTypeIcon(bool isActionable) {
            return LookupTypeIcon(isActionable, Find.GlobalAsset<CardVisualLookup>());
        }
    }
}
