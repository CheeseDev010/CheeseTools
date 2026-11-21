using UnityEngine;

namespace CheeseTools.src.Utils {
    public static class DreamWorldUtil {
        public enum DreamCampfireType {
            RiverLowlands,
            CinderIsles,
            HiddenGorge,
            Reservoir,
        }

        public static DreamCampfire GetDreamCampfire(DreamCampfireType dreamCampfire) {
            switch (dreamCampfire) {
                case DreamCampfireType.RiverLowlands:
                    return Locator.GetDreamCampfire(DreamArrivalPoint.Location.Zone1);
                case DreamCampfireType.CinderIsles:
                    return Locator.GetDreamCampfire(DreamArrivalPoint.Location.Zone2);
                case DreamCampfireType.HiddenGorge:
                    return Locator.GetDreamCampfire(DreamArrivalPoint.Location.Zone3);
                case DreamCampfireType.Reservoir:
                    return Locator.GetDreamCampfire(DreamArrivalPoint.Location.Zone4);
            }
            return null;
        }

        // public static void EnterDreamWorld() => EnterDreamWorld(GetSettingsCampfireType());

        public static void EnterDreamWorld(DreamCampfireType dreamCampfireType) {
            DreamCampfire dreamCampfire = GetDreamCampfire(dreamCampfireType);
            if (dreamCampfire == null) return;

            OpenSlidingDoor(dreamCampfire);
            RelativeLocationData relativeLocation = new RelativeLocationData(new Vector3(1.7f, 1.5f, -1), new Quaternion(0, 0, 0, 0), Vector3.zero);

            DreamArrivalPoint arrivalPoint = Locator.GetDreamArrivalPoint(dreamCampfire.GetLocation());
            if (ItemsUtil.GetItemTool().GetHeldItemType() != ItemType.Lantern) {
                ItemsUtil.PickUpItem(ItemsUtil.GetDreamLantern());
            }
            Locator.GetDreamWorldController().EnterDreamWorld(dreamCampfire, arrivalPoint, relativeLocation);
        }

        public static void ExitDreamWorld() {
            Locator.GetDreamWorldController().ExitDreamWorld();
        }

        private static void OpenSlidingDoor(DreamCampfire dreamCampfire) {
            SlidingDoor door = null;
            foreach (SlidingDoor currentDoor in Resources.FindObjectsOfTypeAll<SlidingDoor>()) {
                float distance = Vector3.Distance(dreamCampfire.transform.position, currentDoor.transform.position);
                if (distance < 15f) {
                    door = currentDoor;
                }
            }

            if (door == null) return;
            door.Open();
        }
    }
}

