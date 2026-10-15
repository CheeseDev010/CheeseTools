using HarmonyLib;
using UnityEngine;

namespace CheeseTools.src {
	[HarmonyPatch]
	public static class ModPatches {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(SunController), nameof(SunController.Update))]
		public static bool SunController_Update() {
			return !Settings.data.freezeSupernova;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Menu), nameof(Menu.EnableMenu))]
		public static void Menu_EnableMenu() {
			if (Settings.data.originalQuitMenuButtonPosition) {
				GameObject.Find("Button-ExitToMainMenu")?.transform?.SetSiblingIndex(4);
			}
		}
	}
}
