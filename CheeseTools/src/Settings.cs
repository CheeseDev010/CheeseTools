using System;
using System.IO;
using UnityEngine;
using OWML.Common;
using OWML.Utils;
using HarmonyLib;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.InputSystem;

namespace CheeseTools.src {
    public static class Settings {
        public class Data {
            public bool watermark = true;
            public bool infiniteFuel = false;
            public bool infiniteOxygen = false;
            public bool playerInvincibility = false;
            public bool shipInvincibility = false;
            public bool showLoopTime = false;
            public bool freezeSupernova = false;
            public bool createLaunchCodesSave = false;
            public bool noAltTabPause = false;
            public bool markStranger = false;
            public bool learnQuantumFrequency = false;
            public bool originalQuitMenuButtonPosition = false;
            public bool showSectors = false;

            public Keybind toggleSpacesuitKeybind = new();
        }

        public static Data data;

        private const string FileName = "cheesetools-settings.json";
        private const float AutosaveInterval = 1f;

        private static ScreenPrompt watermark = new($"CheeseTools v{CheeseTools.Version}: Enabled");
        private static ScreenPrompt loopTimeText = new("");

        private static string path;
        private static float lastAutosaveTime;
        private static string lastJson;

        public static void Start() {
            string dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(dirPath, FileName);
            Load();

            data.toggleSpacesuitKeybind.SetDefault(Key.Slash, Key.T);
        }

        public static void OnApplicationQuit() {
            Save();
        }

        public static void Update() {
            if (Time.unscaledTime - lastAutosaveTime > AutosaveInterval) {
                lastAutosaveTime = Time.unscaledTime;
                Save();
           }

            UpdateWatermark();
            CheckInput();

            if (Locator.GetPlayerBody() == null) return;

            UpdateInfiniteResources();
            UpdateLoopTimeText();

            if (EntitlementsManager.IsDlcOwned() != EntitlementsManager.AsyncOwnershipStatus.Owned) return;

            UpdateStrangerMarker();
        }

        public static void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene) {
            if (newScene == OWScene.SolarSystem) {
                Locator.GetPlayerSectorDetector().OnEnterSector += OnEnterSector;
                Locator.GetPlayerSectorDetector().OnExitSector += OnExitSector;

                if (data.learnQuantumFrequency) {
                    PlayerData.LearnFrequency(SignalFrequency.Quantum);
                }
            }
            if (newScene == OWScene.TitleScreen) {
                if (data.createLaunchCodesSave && (previousScene == OWScene.SolarSystem || previousScene == OWScene.EyeOfTheUniverse)) {
                    PlayerData.ResetGame();
                    PlayerData.LearnLaunchCodes();
                    PlayerData.SaveLoopCount(3);
                }
            }

            if (Locator.GetPlayerBody() == null) return;
            UpdateInvincibility();
        }

        public static void OnSettingsChanged() {
            Application.runInBackground = data.noAltTabPause;

            if (Locator.GetPlayerBody() == null) return;

            if (data.learnQuantumFrequency) {
                PlayerData.LearnFrequency(SignalFrequency.Quantum);
            }

            UpdateInvincibility();
            UpdateSectorText();
        }

        public static void CheckInput() {
            if (data.toggleSpacesuitKeybind.WasPressedThisFrame()) {
                CheeseTools.Console.WriteLine("Pressed");
            }
        }

        public static void OnEnterSector(Sector sector) {
            if (data.showSectors) {
                ModUtils.AddScreenText(sector.gameObject.name, PromptPosition.BottomCenter);
            }
        }

        public static void OnExitSector(Sector sector) {
            if (data.showSectors) {
                ModUtils.RemoveScreenText(sector.gameObject.name, PromptPosition.BottomCenter);
            }
        }

        private static void Load() {
            Data loaded = null;
            try {
                if (File.Exists(path)) {
                    var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ObjectCreationHandling = ObjectCreationHandling.Replace };
                    loaded = JsonConvert.DeserializeObject<Data>(File.ReadAllText(path), settings);
                }
            } catch (Exception e) {
                CheeseTools.Console.WriteLine("Failed to load settings: " + e.Message, MessageType.Error);
                // TODO: Tell the player in-game that data failed to load
                try {
                    string corruptPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".corrupt.json");
                    File.Copy(path, corruptPath, true);
                    File.Delete(path);
                    CheeseTools.Console.WriteLine("Backed up corrupt settings at: " + corruptPath, MessageType.Error); 
                } catch (Exception ex) {
                    CheeseTools.Console.WriteLine("Failed to backup corrupt settings: " + ex.Message, MessageType.Error);
                }
            }
            data = loaded ?? new Data();
        }

        private static void Save() {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            if (json == lastJson) return;
            File.WriteAllText(path, json);
            lastJson = json;
            CheeseTools.Console.WriteLine($"[{Time.unscaledTime}] Saved to file", MessageType.Success);
        }

        private static void UpdateWatermark() {
            if (data.watermark) {
                if (Locator.GetPromptManager()?.GetScreenPromptList(PromptPosition.LowerLeft)?.Contains(watermark) == false) {
                    Locator.GetPromptManager().AddScreenPrompt(watermark, true);
                }
            } else {
                if (Locator.GetPromptManager()?.GetScreenPromptList(PromptPosition.LowerLeft)?.Contains(watermark) == true) {
                    Locator.GetPromptManager().RemoveScreenPrompt(watermark, PromptPosition.LowerLeft);
                }
            }
        }

        private static void UpdateInfiniteResources() {
            if (Locator.GetPlayerTransform()?.TryGetComponent(out PlayerResources resources) == true) {
                if (data.infiniteFuel)
                    resources.SetValue("_currentFuel", resources.GetValue<float>("_maxFuel"));
                if (data.infiniteOxygen)
                    resources.SetValue("_currentOxygen", resources.GetValue<float>("_maxOxygen"));
            }
        }

        private static void UpdateInvincibility() {
            ShipDamageController damageController = Locator.GetShipTransform()?.GetComponent<ShipDamageController>();
            if (damageController) {
                damageController._invincible = data.shipInvincibility;
                if (data.shipInvincibility) ModUtils.RepairShip();
            }

            PlayerResources resources = Locator.GetPlayerTransform()?.GetComponent<PlayerResources>();
            resources._invincible = data.playerInvincibility;
            if (data.playerInvincibility) {
                resources._currentHealth = PlayerResources._maxHealth;
                resources.PatchAllPunctures();
            }
        }

        private static void UpdateLoopTimeText() {
            if (data.showLoopTime && TimeLoop.GetSecondsElapsed() > 0f) {
                loopTimeText.SetText($"Loop Time: {TimeSpan.FromSeconds(TimeLoop.GetSecondsElapsed()).ToString(@"mm\:ss")} [{(int)TimeLoop.GetSecondsElapsed()}]");
                if (!Locator.GetPromptManager().GetScreenPromptList(PromptPosition.LowerLeft).Contains(loopTimeText)) {
                    Locator.GetPromptManager().AddScreenPrompt(loopTimeText, PromptPosition.LowerLeft, true);
                }
            }
            else {
                Locator.GetPromptManager().RemoveScreenPrompt(loopTimeText);
            }
        }

        private static void UpdateSectorText() {
            if (data.showSectors) {
                foreach (Sector sector in Locator.GetPlayerSectorDetector()._sectorList) {
                    ModUtils.AddScreenText(sector.gameObject.name, PromptPosition.BottomCenter);
                }
            }
            else {
                foreach (Sector sector in Locator.GetPlayerSectorDetector()._sectorList) {
                    ModUtils.RemoveScreenText(sector.gameObject.name, PromptPosition.BottomCenter);
                }
            }
        }

        private static void UpdateStrangerMarker() {
            var stranger = Locator.GetAstroObject(AstroObject.Name.RingWorld)?.GetOWRigidbody();
            if (stranger == null) return;

            var strangerMarker = ModUtils.SetMarker("THE STRANGER", stranger);
            bool visible = data.markStranger && !Locator.GetDreamWorldController()._insideDream && strangerMarker.GetMarkerDistance() > 1000;
            if (strangerMarker.IsVisible() != visible) {
                strangerMarker.SetVisibility(visible);
            }
        }
    }

    [HarmonyPatch]
    public static class SettingsPatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunController), nameof(SunController.Update))]
        public static bool SunController_Update() {
            return !Settings.data.freezeSupernova;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Menu), nameof(Menu.EnableMenu))]
        public static void Menu_EnableMenu() {
            if (Settings.data.originalQuitMenuButtonPosition) {
                GameObject.Find("Button-ExitToCheeseToolsMenu")?.transform?.SetSiblingIndex(4);
            }
        }
    }
}
