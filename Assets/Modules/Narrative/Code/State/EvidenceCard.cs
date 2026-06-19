using BeauUtil;
using FieldDay.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public enum MapLocation: byte
    {
        FishingDocks,
        TownHall,
        FishHatchery,
        DNROffice,
        BarrierSite,
        FieldStation,
        ResearchLab,
        ArmyCorps,
        None = 255
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
    }
}