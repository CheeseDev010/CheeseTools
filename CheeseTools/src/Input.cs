using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CheeseTools.src {
    public static class KeyInput {
        private static Dictionary<Key, InputAction> inputActions = new();

        public static bool WasPressedThisFrame(Key key) => Get(key).WasPressedThisFrame();
        public static bool IsPressed(Key key) => Get(key).IsPressed();
        public static bool WasReleasedThisFrame(Key key) => Get(key).WasReleasedThisFrame();

        private static InputAction Get(Key key) {
            if (!inputActions.TryGetValue(key, out var inputAction)) {
                inputAction = new InputAction(type: InputActionType.Button, binding: KeyToBindingPath(key));
                inputAction.Enable();
                inputActions[key] = inputAction;
            }
            return inputAction;
        }

        private static string KeyToBindingPath(Key key) {
            return "<Keyboard>/" + Keyboard.current[key].name;
        }
    }

    public class Keybind {
        [JsonProperty] private SortedSet<Key> _keys = new();
        [JsonProperty] private SortedSet<Key> _defaultKeys = new();

        [JsonConstructor]
        public Keybind() {
            KeybindsManager.RegisterKeybind(this);
        }

        public SortedSet<Key> GetKeys() {
            return _keys;
        }

        public void SetKeys(params SortedSet<Key> keys) {
            _keys = new(keys);
        }

        public SortedSet<Key> GetDefaultKeys() {
            return _defaultKeys;
        }

        public void SetDefaultKeys(params SortedSet<Key> keys) {
            bool isUsingDefaultKeys = IsUsingDefaultKeys();
            _defaultKeys = new(keys);
            if (_keys.Count == 0 || isUsingDefaultKeys) {
                SetKeys(_defaultKeys);
            }
        }

        public string GetDisplayName() {
            return string.Join(" + ", _keys.Select(key => Keyboard.current[key].displayName));
        }

        public bool IsUnbound() {
            return _keys.Contains(Key.None);
        }

        public bool IsUsingDefaultKeys() {
            return _keys.SetEquals(_defaultKeys);
        }

        public bool WasPressedThisFrame() {
            bool wasPressed = true;
            int index = 0;
            foreach (Key key in _keys) {
                if (index++ == _keys.Count - 1) {
                    wasPressed = KeyInput.WasPressedThisFrame(key);
                    break;
                }
                if (!KeyInput.IsPressed(key) && !KeyInput.WasPressedThisFrame(key) && !KeyInput.WasReleasedThisFrame(key)) {
                    wasPressed = false;
                    break;
                }
            }
            return wasPressed;
        }
    }

    public static class KeybindsManager {
        public static Keybind rebinding;
        public static SortedSet<Key> rebindKeys;

        private static HashSet<Keybind> keybinds = new();

        public static void RegisterKeybind(Keybind keybind) {
            keybinds.Add(keybind);
        }

        public static bool IsDuplicateKeybind(Keybind keybind) {
            foreach (Keybind loopKeybind in keybinds) {
                if (keybind != loopKeybind && keybind.GetKeys().SetEquals(loopKeybind.GetKeys())) {
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

