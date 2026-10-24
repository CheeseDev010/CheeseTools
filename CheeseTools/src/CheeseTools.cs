using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace CheeseTools.src {
    public class CheeseTools : ModBehaviour {
        public const string Version = "2.0.0";

        public static CheeseTools instance;
        public static IModConsole Console => instance.ModHelper.Console;

        public void Awake() {
            instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public void Start() {
            LoadManager.OnCompleteSceneLoad += (prev, next) => ModHelper.Events.Unity.FireOnNextUpdate(() => OnCompleteSceneLoad(prev, next));

            Settings.Start();
            Console.WriteLine($"CheeseTools v{Version} enabled!", MessageType.Success);
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene) {
            Settings.OnCompleteSceneLoad(previousScene, newScene);
        }

        public void Update() {
            Settings.Update();
            SettingsUI.Update();
        }

        public void OnGUI() {
            SettingsUI.OnGUI();
        }

        public void OnApplicationQuit() {
            Settings.OnApplicationQuit();
        }
    }
}

