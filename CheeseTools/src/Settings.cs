using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using OWML.Common;

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
			public bool suitlessPracticeStates = false;
			public bool originalQuitMenuButtonPosition = false;
			public bool showSectors = false;
		}

		public static Data data;

		private const string FileName = "cheesetools-settings.json";
		private const float AutosaveInterval = 60f;
		private const float UiScale = 1.5f;

		private static string[] tabNames = {"General", "Keybinds", "Practice States", "Custom Practice States"};

		private static string path;
		private static float screenWidth;
		private static float screenHeight;
		private static bool showWindow;
		private static float lastAutosaveTime;
		private static string lastJson;
		private static int selectedTab;

		public static void Start() {
			string dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			path = Path.Combine(dirPath, FileName);
			Load();
		}

		public static void OnApplicationQuit() {
			Save();
		}

		public static void Update() {
			if (Keyboard.current[Key.Backquote].wasPressedThisFrame) {
				showWindow = !showWindow;
			}

			if (Time.unscaledTime - lastAutosaveTime > AutosaveInterval) {
				lastAutosaveTime = Time.unscaledTime;
				Save();
			}
		}

		public static void Load() {
			Data loaded = null;
			try {
				if (File.Exists(path)) {
					loaded = JsonUtility.FromJson<Data>(File.ReadAllText(path));
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

		public static void Save() {
			string json = JsonUtility.ToJson(data, true);
			if (json == lastJson) return;

			File.WriteAllText(path, json);
			lastJson = json;
			CheeseTools.Console.WriteLine("Saved to file: " + Time.unscaledTime);
		}

		public static void OnGUI() {
			if (!showWindow) return;
			GUI.matrix = Matrix4x4.Scale(new Vector3(UiScale, UiScale, 1f));
			screenWidth = Screen.width / UiScale;
			screenHeight = Screen.height / UiScale;
			float windowWidth = screenWidth / 1.5f;
			float windowHeight = screenHeight / 1.5f;
			GUILayout.Window(0, new Rect(screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2, windowWidth, windowHeight), DrawWindow, "CheeseTools Settings");
		}

		private static void DrawWindow(int windowId) {
			selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
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
		}

		private static void GeneralTab() {
			data.watermark = GUILayout.Toggle(data.watermark, "Watermark");
			data.infiniteFuel = GUILayout.Toggle(data.infiniteFuel, "Infinite Fuel");
			data.infiniteOxygen = GUILayout.Toggle(data.infiniteOxygen, "Infinite Oxygen");
			data.playerInvincibility = GUILayout.Toggle(data.playerInvincibility, "Player Invincibility");
			data.shipInvincibility = GUILayout.Toggle(data.shipInvincibility, "Ship Invincibility");
			data.showLoopTime = GUILayout.Toggle(data.showLoopTime, "Show Loop Time");
			data.freezeSupernova = GUILayout.Toggle(data.freezeSupernova, "Freeze Supernova");
			data.createLaunchCodesSave = GUILayout.Toggle(data.createLaunchCodesSave, "Create Launch Codes Save !RESETS SAVEFILE!");
			data.noAltTabPause = GUILayout.Toggle(data.noAltTabPause, "No Alt Tab Pause");
			data.markStranger = GUILayout.Toggle(data.markStranger, "Mark Stranger");
			data.learnQuantumFrequency = GUILayout.Toggle(data.learnQuantumFrequency, "Learn Quantum Frequency");
			data.suitlessPracticeStates = GUILayout.Toggle(data.suitlessPracticeStates, "Suitless Practice States");
			data.originalQuitMenuButtonPosition = GUILayout.Toggle(data.originalQuitMenuButtonPosition, "Original Quit Menu Button Position");
			data.showSectors = GUILayout.Toggle(data.showSectors, "Show Sectors");
		}

		private static void KeybindsTab() {
		}

		private static void PracticeStatesTab() {
		}

		private static void CustomPracticeStatesTab() {
		}
	}
}
