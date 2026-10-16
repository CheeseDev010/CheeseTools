using UnityEngine;
using UnityEngine.InputSystem;

namespace CheeseTools.src {
	public static class SettingsUI {
		public static bool showWindow;
		public static bool mouseOverWindow;

		private const float UiScale = 1.5f;

		private static string[] tabNames = {"General", "Keybinds", "Practice States", "Custom Practice States"};

		private static Rect windowRect;
		private static int selectedTab;
		private static bool wasPaused;

		public static void Update() {
			if (Keyboard.current[Key.Backquote].wasPressedThisFrame) {
				showWindow = !showWindow;
				Locator.GetMenuAudioController().PlayChangeTab();
				if (showWindow) {
					OWInput.ChangeInputMode(InputMode.None);
					wasPaused = OWTime.IsPaused();
					OWTime.Pause(OWTime.PauseType.Menu);
				} 
				else {
					OWInput.RestorePreviousInputs();
					if (!wasPaused) {
						OWTime.Unpause(OWTime.PauseType.Menu);
					}
				}
			}
		}

		public static void OnGUI() {
			if (!showWindow) return;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			GUI.matrix = Matrix4x4.Scale(new Vector3(UiScale, UiScale, 1f));
			float screenWidth = Screen.width / UiScale;
			float screenHeight = Screen.height / UiScale;
			float windowWidth = screenWidth / 1.5f;
			float windowHeight = screenHeight / 1.5f;
			windowRect = GUILayout.Window(0, new Rect(screenWidth / 2 - windowWidth / 2, screenHeight / 2 - windowHeight / 2, windowWidth, windowHeight), DrawWindow, "CheeseTools Settings");
		}

		private static void DrawWindow(int windowId) {
			GUI.changed = false;

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

			if (GUI.changed) {
				Locator.GetMenuAudioController()?.PlayButtonFocus();
				Settings.OnSettingsChanged();
			}
		}

		private static void GeneralTab() {
			Settings.data.watermark = GUILayout.Toggle(Settings.data.watermark, "Watermark");
			Settings.data.infiniteFuel = GUILayout.Toggle(Settings.data.infiniteFuel, "Infinite Fuel");
			Settings.data.infiniteOxygen = GUILayout.Toggle(Settings.data.infiniteOxygen, "Infinite Oxygen");
			Settings.data.playerInvincibility = GUILayout.Toggle(Settings.data.playerInvincibility, "Player Invincibility");
			Settings.data.shipInvincibility = GUILayout.Toggle(Settings.data.shipInvincibility, "Ship Invincibility");
			Settings.data.showLoopTime = GUILayout.Toggle(Settings.data.showLoopTime, "Show Loop Time");
			Settings.data.freezeSupernova = GUILayout.Toggle(Settings.data.freezeSupernova, "Freeze Supernova");
			Settings.data.createLaunchCodesSave = GUILayout.Toggle(Settings.data.createLaunchCodesSave, "Create Launch Codes Save !RESETS SAVEFILE!");
			Settings.data.noAltTabPause = GUILayout.Toggle(Settings.data.noAltTabPause, "No Alt Tab Pause");
			Settings.data.markStranger = GUILayout.Toggle(Settings.data.markStranger, "Mark Stranger");
			Settings.data.learnQuantumFrequency = GUILayout.Toggle(Settings.data.learnQuantumFrequency, "Learn Quantum Frequency");
			Settings.data.originalQuitMenuButtonPosition = GUILayout.Toggle(Settings.data.originalQuitMenuButtonPosition, "Original Quit Menu Button Position");
			Settings.data.showSectors = GUILayout.Toggle(Settings.data.showSectors, "Show Sectors");
		}

		private static void KeybindsTab() {
		}

		private static void PracticeStatesTab() {
		}

		private static void CustomPracticeStatesTab() {
		}
	}
}
