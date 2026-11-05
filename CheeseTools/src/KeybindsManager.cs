using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CheeseTools.src {
    public static class KeybindsManager {
        public static Keybind rebinding;
        public static HashSet<Key> rebindKeys;

        private static HashSet<Keybind> keybinds = new();

        public static void RegisterKeybind(Keybind keybind) {
            keybinds.Add(keybind);
        }

        public static bool IsDuplicateKeybind(Keybind keybind) {
            foreach (Keybind loopKeybind in keybinds) {
                if (keybind == loopKeybind) continue;
                if (keybind.GetKeys().SetEquals(loopKeybind.GetKeys())) {
                    return true;
                }
            }
            return false;
        }

        public static void Update() {
            UpdateRebinding();
        }

        private static void UpdateRebinding() {
            if (rebinding == null) return;
            
            if (!SettingsUI.showWindow || SettingsUI.selectedTab != 1 || Keyboard.current[Key.Escape].isPressed) {
                rebinding = null;
                return;
            }

            if (Keyboard.current[Key.Backspace].isPressed) {
                rebinding.SetKeys(Key.None);
                Locator.GetMenuAudioController().PlayRebindKey();
                rebinding = null;
                return;
            }

            foreach (KeyControl key in Keyboard.current.allKeys) {
                if (key.isPressed) {
                    rebindKeys.Add(key.keyCode);
                }
            }

            if (Keyboard.current.anyKey.wasReleasedThisFrame) {
                if (rebindKeys.Count > 0) {
                    rebinding.SetKeys(rebindKeys);
                    Locator.GetMenuAudioController().PlayRebindKey();
                }
                rebinding = null;
            }
        }

    }
}

