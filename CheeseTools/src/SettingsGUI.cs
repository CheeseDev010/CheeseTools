using CheeseTools.src.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

namespace CheeseTools.src {
    public static class SettingsGUI {
        private class PopupSelectorData {
            public string id;
            public string[] options;
            public Rect rect;
            public int selectedIndex = -1;
        }

        public static bool showWindow;
        public static bool mouseOverWindow;
        public static int selectedTab;

        public const int FontSize = 16;
        private const float UIScale = 1.4f;
        private const float WindowRatio = 4f / 3f;

        private static string[] tabNames = {"General", "Keybinds", "Practice States", "Custom Practice States", "Info"};
        private static string[] dreamCampfireNames = {"River Lowlands", "Cinder Isles", "Hidden Gorge", "Reservoir"};

        private static Rect windowRect;
        private static float scale;
        private static bool wasPaused;
        private static PopupSelectorData popupSelectorData;

        public static void Update() {
            if (KeyInput.WasPressedThisFrame(Key.Backquote)) {
                showWindow = !showWindow;
                Locator.GetMenuAudioController().PlayChangeTab();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (Locator.GetPlayerBody() != null) {
                    if (showWindow) {
                        wasPaused = OWTime.IsPaused();
                        if (!wasPaused) {
                            OWTime.Pause(OWTime.PauseType.Menu);
                        }
                    } else {
                        if (!wasPaused) {
                            OWTime.Unpause(OWTime.PauseType.Menu);
                        }
                    }
                }
            }

            if (popupSelectorData != null && Mouse.current.leftButton.wasReleasedThisFrame) {
                popupSelectorData = null;
            }
        }

        public static void OnGUI() {
            if (!showWindow) return;

            scale = 1.3f * (Screen.height / 1080f);
            if (SettingsGUISkin.skin == null) SettingsGUISkin.BuildSkin();
            GUI.skin = SettingsGUISkin.skin;

            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;
            float windowHeight = screenHeight / 1.5f;
            float windowWidth = windowHeight * WindowRatio;
            windowRect = new Rect(screenWidth / 2f - windowWidth / 2f, screenHeight / 2f - windowHeight / 2f, windowWidth, windowHeight);
            windowRect = GUI.Window(0, windowRect, SettingsWindow, "");

            if (popupSelectorData != null) {
                GUI.Window(1, popupSelectorData.rect, PopupSelectorWindow, "", SettingsGUISkin.popupSelectorWindowStyle);
            }
        }


        private static void DrawTopbar() {
            if (Event.current.type == EventType.Repaint) {
                Texture2D topbarTexture = SettingsGUISkin.Solid(new Color(0.16f, 0.29f, 0.48f, 1.00f));
                Rect topbar = new Rect(0, 0, windowRect.width, 16 * scale);
                GUI.DrawTexture(topbar, SettingsGUISkin.accentActiveTexture);
                GUI.Label(new Rect(0, 0, topbar.width, topbar.height), $"CheeseTools v{CheeseTools.Version}");
            }
        }

        private static bool Toggle(bool value, string label) {
            value = GUILayout.Toggle(value, label);
            if (Event.current.type == EventType.Repaint) {
                Rect rect = GUILayoutUtility.GetLastRect();
                float size = 18f;
                Rect box = new(rect.x, rect.y + (rect.height - size) / 2f, size, size);
                GUI.DrawTexture(box, SettingsGUISkin.frameTexture);
                Rect inner = new(box.x + 3f, box.y + 3f, size - 6f, size - 6f);
                GUI.DrawTexture(inner, value ? SettingsGUISkin.accentActiveTexture : SettingsGUISkin.frameTexture);
            }
            return value;
        }

        private static void SettingsWindow(int windowId) {
            GUI.changed = false;

            DrawTopbar();
            GUILayout.Space(10);
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents, GUILayout.ExpandWidth(false));
            switch(selectedTab) {
                case 0:
                    GeneralTab();
                    break;
                case 1:
                    KeybindsTab();
                    break;
                case 2:
                    PracticeStatesTab();
                    break;
                case 3:
                    CustomPracticeStatesTab();
                    break;
            }

            if (GUI.changed) {
                Locator.GetMenuAudioController()?.PlayButtonFocus();
                Settings.OnSettingsChanged();
            }
        }

        private static void GeneralTab() {
            Settings.data.watermark = Toggle(Settings.data.watermark, "Watermark");
            Settings.data.infiniteFuel = Toggle(Settings.data.infiniteFuel, "Infinite Fuel");
            Settings.data.infiniteOxygen = Toggle(Settings.data.infiniteOxygen, "Infinite Oxygen");
            Settings.data.playerInvincibility = Toggle(Settings.data.playerInvincibility, "Player Invincibility");
            Settings.data.shipInvincibility = Toggle(Settings.data.shipInvincibility, "Ship Invincibility");
            Settings.data.showLoopTime = Toggle(Settings.data.showLoopTime, "Show Loop Time");
            Settings.data.freezeSupernova = Toggle(Settings.data.freezeSupernova, "Freeze Supernova");
            Settings.data.createLaunchCodesSave = Toggle(Settings.data.createLaunchCodesSave, "Create Launch Codes Save !RESETS SAVEFILE!");
            Settings.data.noAltTabPause = Toggle(Settings.data.noAltTabPause, "No Alt Tab Pause");
            Settings.data.markStranger = Toggle(Settings.data.markStranger, "Mark Stranger");
            Settings.data.learnQuantumFrequency = Toggle(Settings.data.learnQuantumFrequency, "Learn Quantum Frequency");
            Settings.data.originalQuitMenuButtonPosition = Toggle(Settings.data.originalQuitMenuButtonPosition, "Original Quit Menu Button Position");
            Settings.data.showSectors = Toggle(Settings.data.showSectors, "Show Sectors");
        }

        private static void KeybindsTab() {
            DrawKeybind("Toggle Spacesuit Keybind: ", Settings.data.toggleSpacesuitKeybind);
            DrawKeybind("Fast Load New Expedition", Settings.data.fastLoadNewExpeditionKeybind);
            DrawKeybind("Teleport Ship To Player Keybind: ", Settings.data.teleportShipToPlayerKeybind);
            DrawKeybind("Toggle Speedup", Settings.data.toggleSpeedupKeybind);
            DrawKeybind("Enter/Exit Dreamworld", Settings.data.enterExitDreamworldKeybind);
            Settings.data.dreamCampfireType = (DreamWorldUtil.DreamCampfireType) DrawPopupSelector("Dreamworld Campfire: ", (int)Settings.data.dreamCampfireType, dreamCampfireNames);
        }

        private static void PracticeStatesTab() {
        }

        private static void CustomPracticeStatesTab() {
        }

        private static void DrawKeybind(string label, Keybind keybind) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            string keybindText = keybind == KeybindsManager.rebinding ? "[...]" :
                (keybind.IsUnbound() ? "[Click to Bind]" : keybind.GetDisplayName());
            if (GUILayout.Button(keybindText)) {
                KeybindsManager.rebinding = keybind == KeybindsManager.rebinding ? null : keybind;
                KeybindsManager.rebindKeys = new();
            }
            if (!keybind.IsUsingDefaultKeys()) {
                if (GUILayout.Button("Reset To Default")) {
                    KeybindsManager.rebinding = null;
                    keybind.SetKeys(keybind.GetDefaultKeys());
                    Locator.GetMenuAudioController().PlayRebindKey();
                }
            }
            if (KeybindsManager.IsDuplicateKeybind(keybind)) {
                GUILayout.Label("<color=yellow>Duplicate keybind (Disabled)</color>");
            }
            GUILayout.EndHorizontal();
        }

        private static int DrawPopupSelector(string label, int selectedIndex, string[] options) {
            string longest = "";
            foreach (string str in options) {
                if (str.Length > longest.Length) {
                    longest = str;
                }
            }
            longest += " ▼";
            string buttonText = options[selectedIndex] + " ▼";

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(longest), GUI.skin.button);
            if (GUI.Button(rect, buttonText)) {
                Locator.GetMenuAudioController()?.PlayButtonFocus();
                Vector2 pos = GUIUtility.GUIToScreenPoint(new Vector2(Unscale(rect.x), Unscale(rect.yMax)));
                popupSelectorData = new();
                popupSelectorData.rect = new Rect(pos.x, pos.y, rect.width, rect.height * options.Length);
                popupSelectorData.options = options;
                popupSelectorData.selectedIndex = -1;
            }
            GUILayout.EndHorizontal();
            return popupSelectorData.selectedIndex != -1 ? popupSelectorData.selectedIndex : selectedIndex;
        }

        private static void PopupSelectorWindow(int windowId) {
            for (int i = 0; i < popupSelectorData.options.Length; i++) {
                string option = popupSelectorData.options[i];
                float optionHeight = popupSelectorData.rect.height / popupSelectorData.options.Length;
                Rect rect = new(0, optionHeight * i, popupSelectorData.rect.width, optionHeight);
                if (GUI.Button(rect, option, SettingsGUISkin.popupSelectorOptionStyle)) {
                    popupSelectorData.selectedIndex = i;
                }
            }
        }

        public static float Scale(float num) {
            return num * scale;
        }

        public static float Unscale(float num) {
            return num / scale;
        }

        public static Rect ScaledRect(Rect rect) {
            return new Rect(rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale); 
        }

        public static RectOffset ScaledRectOffset(RectOffset rect) {
            return new RectOffset(Mathf.RoundToInt(rect.left * scale), Mathf.RoundToInt(rect.right * scale), Mathf.RoundToInt(rect.top * scale), Mathf.RoundToInt(rect.bottom * scale));
        }
    }

    [HarmonyPatch]
    public static class SettingsUIPatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseInputManager), nameof(BaseInputManager.GetInputMode))]
        public static bool BaseInputManager_GetInputMode(ref InputMode __result) {
            if (SettingsGUI.showWindow) {
                __result = InputMode.None;
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseInputManager), nameof(BaseInputManager.IsInputMode))]
        public static bool BaseInputManager_IsInputMode(ref bool __result) {
            if (SettingsGUI.showWindow) {
                __result = false;
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventSystem), nameof(EventSystem.RaycastAll))]
        public static void EventSystem_RaycastAll(List<RaycastResult> raycastResults) {
            if (SettingsGUI.showWindow) {
                raycastResults.Clear();
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CursorManager), nameof(CursorManager.RefreshCursorState))]
        public static bool CursorManager_RefreshCursorState() {
            return !SettingsGUI.showWindow;
        }
    }
}

