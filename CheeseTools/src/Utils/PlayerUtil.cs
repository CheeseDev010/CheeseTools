namespace CheeseTools.src.Utils {
    public static class PlayerUtil {
        public static void ToggleSpacesuit(bool instant) {
            if (!Locator.GetPlayerSuit().IsWearingSuit()) {
                EquipSpacesuit(instant);
            } else {
                RemoveSpacesuit(instant);
            }
        }

        public static void EquipSpacesuit(bool instant) {
            Locator.GetPlayerSuit().SuitUp(false, instant);
            Locator.GetPlayerTransform().GetComponent<PlayerResources>()._jetpackThruster.DebugResetBoostCharge();
        }

        public static void RemoveSpacesuit(bool instant) {
            Locator.GetPlayerSuit().RemoveSuit(instant);
        }
    }
}

