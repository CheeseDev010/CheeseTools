using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CheeseTools.src {
    public class Keybind {
        [JsonProperty] private HashSet<Key> _keys = new();
        [JsonProperty] private HashSet<Key> _defaultKeys = new();
        private bool _wasPressed;

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
            if (_keys.Count == 0 || (isUsingDefaultKeys)) {
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
            if (_defaultKeys.Count == 0) {
                throw new InvalidOperationException("Default keys must be set before it can be used");
            }
            if (_keys.Contains(Key.None) || KeybindsManager.IsDuplicateKeybind(this)) return false;

            bool isPressed = true;
            bool wasPressed = _wasPressed;
            foreach (Key key in _keys) {
                if (!Keyboard.current[key].IsPressed()) {
                    isPressed = false;
                    break;
                }
            }

            if (isPressed && !wasPressed) {
                _wasPressed = true;
            }
            if (!isPressed) {
                _wasPressed = false;
            }

            return isPressed && !wasPressed;
        }
    }
}

