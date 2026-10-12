using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace CheeseTools.src {
	public class CheeseTools : ModBehaviour {
		public static CheeseTools instance;
		public static IModConsole Console => instance.ModHelper.Console;
		public static string version = "2.0.0";

		public void Awake() {
			instance = this;
		}

		public void Start() {
			new Harmony("CheeseRunner1.CheeseTools").PatchAll(Assembly.GetExecutingAssembly());
			Settings.Start();

			Console.WriteLine($"{nameof(CheeseTools)} v{version} enabled", MessageType.Success);
		}

		public void Update() {
			Settings.Update();
		}

		public void OnGUI() {
			Settings.OnGUI();
		}

		public void OnApplicationQuit() {
			Settings.OnApplicationQuit();
		}
	}
}
