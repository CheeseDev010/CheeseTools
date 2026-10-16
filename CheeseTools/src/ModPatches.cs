using HarmonyLib;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CheeseTools.src {
	[HarmonyPatch]
	public static class ModPatches {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(SunController), nameof(SunController.Update))]
		public static bool SunController_Update() {
			return !Settings.data.freezeSupernova;
		}
		[HarmonyPrefix]
		[HarmonyPatch(typeof(CursorManager), nameof(CursorManager.RefreshCursorState))]
		public static bool CursorManager_RefreshCursorState() {
			return !SettingsUI.showWindow;
		}
		[HarmonyPrefix]
		[HarmonyPatch(typeof(OWInput), nameof(OWInput.IsNewlyPressed))]
		public static bool OWInput_IsNewlyPressed() {
			return !SettingsUI.showWindow;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Menu), nameof(Menu.EnableMenu))]
		public static void Menu_EnableMenu() {
			if (Settings.data.originalQuitMenuButtonPosition) {
				GameObject.Find("Button-ExitToMainMenu")?.transform?.SetSiblingIndex(4);
			}
		}
		[HarmonyPostfix]
		[HarmonyPatch(typeof(EventSystem), nameof(EventSystem.RaycastAll))]
		public static void EventSystem_RaycastAll(List<RaycastResult> raycastResults) {
			if (SettingsUI.showWindow) {
				raycastResults.Clear();
			}
		}
	}
}
