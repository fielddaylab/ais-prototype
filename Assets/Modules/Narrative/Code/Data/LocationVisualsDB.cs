using FieldDay.Assets;
using UnityEngine;

namespace AIS.Narrative {
    [CreateAssetMenu(menuName = "Narrative/Location Visuals DB")]
    public class LocationVisualsDB : GlobalAsset
    {
        public Sprite FishingDocks;
        public Sprite TownHall;
        public Sprite FishHatchery;
        public Sprite DNROffice;
        public Sprite BarrierSite;
        public Sprite FieldStationLampricide;
        public Sprite ResearchLab;
        public Sprite ArmyCorps;
        public Sprite FieldStationSterile;
        public Sprite FishAndWildlife;
        public Sprite FieldStationTraps;
    }

    public static class LocationVisualsDBUtility
    {
        public static Sprite LookupLocationSprite(LocationVisualsDB locationDB, MapLocation location)
        {
            switch (location)
            {
                case MapLocation.FishingDocks:
                    return locationDB.FishingDocks;
                case MapLocation.TownHall:
                    return locationDB.TownHall;
                case MapLocation.FishHatchery:
                    return locationDB.FishHatchery;
                case MapLocation.DNROffice:
                    return locationDB.DNROffice;
                case MapLocation.BarrierSite:
                    return locationDB.BarrierSite;
                case MapLocation.FieldStationLampricide:
                    return locationDB.FieldStationLampricide;
                case MapLocation.ResearchLab:
                    return locationDB.ResearchLab;
                case MapLocation.ArmyCorps:
                    return locationDB.ArmyCorps;
                case MapLocation.FieldStationSterile:
                    return locationDB.FieldStationSterile;
                case MapLocation.FishAndWildlife:
                    return locationDB.FishAndWildlife;
                case MapLocation.FieldStationTraps:
                    return locationDB.FieldStationTraps;
                default:
                    return null;
            }
        }
    }
}