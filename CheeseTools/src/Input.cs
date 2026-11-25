using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CheeseTools.src {
    public static class KeyInput {
        private static Dictionary<Key, InputAction> inputActions = new();
        public static InputAction anyKey = new(type: InputActionType.Button, binding: "<Keyboard>/anyKey");

        public static void Start() {
            anyKey.Enable();
        }

        public static bool WasPressedThisFrame(Key key) => Get(key)?.WasPressedThisFrame() == true;
        public static bool IsPressed(Key key) => Get(key)?.IsPressed() == true;
        public static bool WasReleasedThisFrame(Key key) => Get(key)?.WasReleasedThisFrame() == true;

        private static InputAction Get(Key key) {
            if (Keyboard.current == null) return null;
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
        [JsonProperty] private HashSet<Key> _keys = new();
        [JsonProperty] private HashSet<Key> _defaultKeys = new();

        [JsonConstructor]
        public Keybind() {
            KeybindsManager.RegisterKeybind(this);
        }

        public HashSet<Key> GetKeys() {
            return _keys;
        }

        public void SetKeys(params HashSet<Key> keys) {
            _keys = new(keys);
        }

        public HashSet<Key> GetDefaultKeys() {
            return _defaultKeys;
        }

        public void SetDefaultKeys(params HashSet<Key> keys) {
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
            if (_defaultKeys.Count == 0 || _keys.Count == 0 || _keys.Contains(Key.None) || KeybindsManager.IsDuplicateKeybind(this) || KeybindsManager.rebinding != null) return false;
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
        public static HashSet<Key> rebindKeys;

        private static HashSet<Keybind> keybinds = new();
        private static bool anyKeybindWasPressedThisFrame;
        private static int anyKeybindLastUpdateFrameCount;

        public static void Update() {
            UpdateRebinding();
        }

        public static void RegisterKeybind(Keybind keybind) {
            keybinds.Add(keybind);
        }

        public static bool IsDuplicateKeybind(Keybind keybind) {
            foreach (Keybind loopKeybind in keybinds) {
                if (keybind != loopKeybind && !keybind.GetKeys().Contains(Key.None) && keybind.GetKeys().SetEquals(loopKeybind.GetKeys())) {
                    return true;
                }
            }
            return false;
        }

        public static bool AnyKeybindWasPressedThisFrame() {
            if (anyKeybindLastUpdateFrameCount == Time.frameCount) return anyKeybindWasPressedThisFrame;
            bool anyWasPressed = false;
            foreach (Keybind keybind in keybinds) {
                if (keybind.WasPressedThisFrame()) {
                    anyWasPressed = true;
                }
            }
            anyKeybindWasPressedThisFrame = anyWasPressed;
            anyKeybindLastUpdateFrameCount = Time.frameCount;
            return anyKeybindWasPressedThisFrame;
        }

        private static void UpdateRebinding() {
            if (rebinding == null) return;
            
            if (!SettingsGUI.showWindow || SettingsGUI.selectedTab != 1 || KeyInput.WasPressedThisFrame(Key.Escape)) {
                rebinding = null;
                return;
            }

            if (KeyInput.WasPressedThisFrame(Key.Backspace)) {
                rebinding.SetKeys(Key.None);
                Locator.GetMenuAudioController().PlayRebindKey();
                rebinding = null;
                return;
            }

            foreach (KeyControl key in Keyboard.current.allKeys) {
                if (KeyInput.WasPressedThisFrame(key.keyCode)) {
                    rebindKeys.Add(key.keyCode);
                }
            }

            if (KeyInput.anyKey.WasReleasedThisFrame()) {
                if (rebindKeys.Count > 0) {
                    rebinding.SetKeys(rebindKeys);
                    Locator.GetMenuAudioController().PlayRebindKey();
                }
                rebinding = null;
            }
        }
    }

    [HarmonyPatch]
    public static class InputPatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OWInput), nameof(OWInput.IsNewlyPressed))]
        public static bool OWInput_IsNewlyPressed(IInputCommands command) {
            return !KeybindsManager.AnyKeybindWasPressedThisFrame();
        }
    }
}

