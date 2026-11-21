using System;

namespace CheeseTools.src.Utils {
    public static class SceneLoadUtil {
        private static Action afterSceneLoad;
        private static bool skipWakeUpAnim;
        private static EyeState afterSceneLoadEyeState;

        public static void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene) {
            if (afterSceneLoad != null) {
                FixedUpdateDispatcher.FireAfterNFixedUpdates(() => {
                    if (skipWakeUpAnim) {
                        // TODO: implement skip wakeup anim
                    }
                    afterSceneLoad();
                    afterSceneLoad = null;
                }, 2);
            }
        }

        public static void LoadSolarSystemScene(Action afterSceneLoad, bool skipWakeUpAnim = true) {
            PlayerData._currentGameSave.warpedToTheEye = false;
            PlayerData.SaveCurrentGame();

            LoadManager.LoadScene(OWScene.SolarSystem);
            SceneLoadUtil.afterSceneLoad = afterSceneLoad;
            SceneLoadUtil.skipWakeUpAnim = skipWakeUpAnim;
        }

        public static void LoadEyeScene(EyeState eyeState, Action afterSceneLoad) {
            PlayerData.SaveWarpedToTheEye(TimeLoop.GetSecondsRemaining());
            LoadManager.LoadScene(OWScene.EyeOfTheUniverse);
            SceneLoadUtil.afterSceneLoad = afterSceneLoad;
            skipWakeUpAnim = true;
            afterSceneLoadEyeState = eyeState;
        }
    }
}

