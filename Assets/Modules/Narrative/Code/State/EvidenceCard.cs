using BeauUtil;
using FieldDay.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public enum MapLocation
    {
        FishingDocks,
        TownHall,
        FishHatchery,
        DNROffice,
        BarrierSite,
        FieldStationLampricide,
        ResearchLab,
        ArmyCorps,
        None,
        FieldStationSterile,
        FishAndWildlife,
        FieldStationTraps,
        Home,
    }


    [CreateAssetMenu(menuName ="Narrative/Evidence Card")]
    public sealed class EvidenceCard : NamedAsset {
        public StringHash32 Id;
        public PlayerStatId Suit;
        public Image Illustration;
        [Multiline] public string Label;

        public bool isActionable;
        [EnableIfField(nameof(isActionable))]
        public MapLocation ActivateLocation;

        [EnableIfField(nameof(isActionable))]
        private bool isActivated = false;

        public bool getIsActivated() { return this.isActivated; }
        public void activateCard() { this.isActivated = true; }
    }
}