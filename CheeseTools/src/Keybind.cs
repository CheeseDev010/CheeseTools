using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

namespace CheeseTools.src {
    public class Keybind {
        [JsonProperty] private HashSet<Key> _keys = new();
        private HashSet<Key> _defaultKeys = new();
        private bool _wasPressed;

        public HashSet<Key> Get() {
            return _keys;
        }

        public void Set(params Key[] keys) {
            _keys = new(keys);
        }

        public HashSet<Key> GetDefault() {
            return _defaultKeys;
        }

        public void SetDefault(params Key[] keys) {
            if (keys == null || keys.Length == 0) {
                throw new ArgumentException("A keybind needs atleast one key");
            }

            _defaultKeys = new(keys);
            if (_keys.Count == 0) {
                _keys = new(_defaultKeys);
            }
        }

        public bool WasPressedThisFrame() {
            if (_defaultKeys.Count == 0) {
                throw new InvalidOperationException("Default keybind needs to be set before it can be used");
            }
            if (_keys.Count == 0) {
                throw new InvalidOperationException("Keybind has no keys");
            }

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
