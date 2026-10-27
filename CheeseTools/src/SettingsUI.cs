using HarmonyLib;
using System.Collections.Generic;
using System.Data.SqlClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace CheeseTools.src {
    public static class SettingsUI {
        public static bool showWindow;
        public static bool mouseOverWindow;

        private const float UIScale = 1.5f;
        private const float WindowRatio = 4f / 3f;
        private const int FontSize = 16;

        private static string[] tabNames = {"General", "Keybinds", "Practice States", "Custom Practice States", "Info"};

        private static Rect windowRect;
        private static float scale;
        private static int selectedTab;
        private static bool wasPaused;

        private static GUISkin skin;
        private static Texture2D windowTexture = Solid(new Color(0.06f, 0.06f, 0.06f, 0.94f));
        private static Texture2D frameTexture = Solid(new Color(0.22f, 0.18f,0.18f, 1f));
        private static Texture2D accentTexture = Solid(new Color(0.98f, 0.59f, 0.26f, 0.40f));
        private static Texture2D accentHoverTexture = Solid(new Color(0.98f, 0.59f, 0.26f, 1.00f));
        private static Texture2D accentActiveTexture = Solid(new Color(0.98f, 0.53f, 0.06f, 1.00f));

        public static void Update() {
            if (Keyboard.current[Key.Backquote].wasPressedThisFrame) {
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
        }

        public static void OnGUI() {
            if (!showWindow) return;

            scale = 1.3f * (Screen.height / 1080f);
            if (skin == null) BuildSkin();
            GUI.skin = skin;

            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float screenWidth = Screen.width / scale;
            float screenHeight = Screen.height / scale;
            float windowHeight = screenHeight / 1.5f;
            float windowWidth = windowHeight * WindowRatio;
            windowRect = new Rect(screenWidth / 2f - windowWidth / 2f, screenHeight / 2f - windowHeight / 2f, windowWidth, windowHeight);
            windowRect = GUI.Window(0, windowRect, DrawWindow, "");
        }

        private static Texture2D Solid(Color color) {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static void BuildSkin() {
            skin = Object.Instantiate(GUI.skin);
            skin.hideFlags = HideFlags.HideAndDontSave;

            GUIStyle window = skin.window;
            window.normal.background = window.onNormal.background = windowTexture;
            window.padding = ScaledRectOffset(new(7, 7, 14, 7));

            GUIStyle button = skin.button;
            button.normal.background = accentTexture;
            button.hover.background = accentHoverTexture;
            button.active.background = accentActiveTexture;
            button.onNormal.background = accentActiveTexture;
            button.onHover.background = accentHoverTexture;
            button.onActive.background = accentActiveTexture;
            button.stretchWidth = false;
            button.wordWrap = false;
            button.fontSize = FontSize;
            
            GUIStyle toggle = skin.toggle;
            toggle.normal.background = toggle.onNormal.background = null;
            toggle.hover.background = toggle.onHover.background = null;
            toggle.active.background = toggle.onActive.background = null;
            toggle.padding = new RectOffset(22, 0, 0, 0);
            toggle.stretchWidth = false;
            toggle.wordWrap = false;
            toggle.fontSize = FontSize;

            GUIStyle label = skin.label;
            label.stretchWidth = false;
            label.wordWrap = false;
            label.fontSize = FontSize;
        }

        private static void DrawTopbar() {
            if (Event.current.type == EventType.Repaint) {
                Texture2D topbarTexture = Solid(new Color(0.16f, 0.29f, 0.48f, 1.00f));
                Rect topbar = new Rect(0, 0, windowRect.width, 16 * scale);
                GUI.DrawTexture(topbar, accentActiveTexture);
                GUIStyle style = new(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, 0, topbar.width, topbar.height), $"CheeseTools v{CheeseTools.Version}", style);
            }
        }

        private static bool Toggle(bool value, string label) {
            value = GUILayout.Toggle(value, label);
            if (Event.current.type == EventType.Repaint) {
                Rect rect = GUILayoutUtility.GetLastRect();
                float size = 18f;
                Rect box = new(rect.x, rect.y + (rect.height - size) / 2f, size, size);
                GUI.DrawTexture(box, frameTexture);
                Rect inner = new(box.x + 2f, box.y + 2f, size - 4f, size - 4f);
                GUI.DrawTexture(inner, value ? accentActiveTexture : frameTexture);
            }
            return value;
        }

        private static void DrawWindow(int windowId) {
            GUI.changed = false;

            DrawTopbar();
            GUILayout.Space(10);
            CheeseTools.Console.WriteLine($"unity version: {Application.unityVersion}");
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
            MakeKeybind("Toggle Spacesuit Keybind: ", Settings.data.toggleSpacesuitKeybind);
        }

        private static void PracticeStatesTab() {
        }

        private static void CustomPracticeStatesTab() {
        }

        private static void MakeKeybind(string label, Keybind keybind) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.Button("CTRL + R");
            GUILayout.EndHorizontal();
        }

        private static float Scale(float num) {
            return num * scale;
        }

        private static float Unscale(float num) {
            return num / scale;
        }

        private static Rect ScaledRect(Rect rect) {
            return new Rect(rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale); 
        }

        private static RectOffset ScaledRectOffset(RectOffset rect) {
            return new RectOffset(Mathf.RoundToInt(rect.left * scale), Mathf.RoundToInt(rect.right * scale), Mathf.RoundToInt(rect.top * scale), Mathf.RoundToInt(rect.bottom * scale));
        }
    }

    [HarmonyPatch]
    public static class SettingsUIPatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseInputManager), nameof(BaseInputManager.GetInputMode))]
        public static bool BaseInputManager_GetInputMode(ref InputMode __result) {
            if (SettingsUI.showWindow) {
                __result = InputMode.None;
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseInputManager), nameof(BaseInputManager.IsInputMode))]
        public static bool BaseInputManager_IsInputMode(ref bool __result) {
            if (SettingsUI.showWindow) {
                __result = false;
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventSystem), nameof(EventSystem.RaycastAll))]
        public static void EventSystem_RaycastAll(List<RaycastResult> raycastResults) {
            if (SettingsUI.showWindow) {
                raycastResults.Clear();
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CursorManager), nameof(CursorManager.RefreshCursorState))]
        public static bool CursorManager_RefreshCursorState() {
            return !SettingsUI.showWindow;
        }
    }
}
