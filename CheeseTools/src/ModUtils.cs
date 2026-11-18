using System;
using UnityEngine;

namespace CheeseTools.src {
    public static class ModUtils {
        private const float SpeedupTimeScale = 51f;
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
            ModUtils.afterSceneLoad = afterSceneLoad;
            ModUtils.skipWakeUpAnim = skipWakeUpAnim;
        }

        public static void LoadEyeScene(EyeState eyeState, Action afterSceneLoad) {
            PlayerData.SaveWarpedToTheEye(TimeLoop.GetSecondsRemaining());
            LoadManager.LoadScene(OWScene.EyeOfTheUniverse);
            ModUtils.afterSceneLoad = afterSceneLoad;
            skipWakeUpAnim = true;
            afterSceneLoadEyeState = eyeState;
        }

        public static void ToggleSpeedup() {
            OWTime.SetTimeScale(OWTime.GetTimeScale() != SpeedupTimeScale ? SpeedupTimeScale : 1f);
        }

        public static void TeleportShipToPlayer() {
            OWRigidbody playerBody = Locator.GetPlayerBody();

            TeleportBodyTo(
                    Locator.GetShipBody(),
                    playerBody.transform.position + playerBody.transform.up * 10,
                    playerBody.GetRotation(),
                    playerBody.GetVelocity(),
                    playerBody.GetAngularVelocity()
                    );
        }

        public static void TeleportPlayerToShip() {
            OWRigidbody ship = Locator.GetShipBody();
            TeleportBodyTo(Locator.GetPlayerBody(), ship.GetPosition(), ship.GetRotation(), ship.GetVelocity(), ship.GetAngularVelocity());

            HatchController hatchController = GameObject.FindObjectOfType<HatchController>();
            hatchController.OnEntry(Locator.GetPlayerDetector());

            OWTriggerVolume oxygenVolume = GameObject.Find("ShipAtmosphereVolume").GetComponent<OWTriggerVolume>();
            oxygenVolume.AddObjectToVolume(Locator.GetPlayerDetector());
            oxygenVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());

            OWTriggerVolume gravityVolume = GameObject.Find("ShipGravityVolume").GetComponent<OWTriggerVolume>();
            gravityVolume.AddObjectToVolume(Locator.GetPlayerDetector());
            gravityVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());

            // hacky fix for bug where gravity doesn't apply when seated
            ShipCockpitController cockpitController = GameObject.FindObjectOfType<ShipCockpitController>();
            cockpitController.ExitFlightConsole();
            cockpitController.CompleteExitFlightConsole();
            FixedUpdateDispatcher.FireAfterFixedUpdate(() => cockpitController.OnPressInteract());
        }

        public static void TeleportPlayerTo(OWRigidbody relativeBody, RelativeLocationData relativeLocation) {
            if (PlayerState.IsInsideShip()) {
                ShipCockpitController cockpitController = GameObject.FindObjectOfType<ShipCockpitController>();
                if (cockpitController._playerAtFlightConsole) {
                    cockpitController.ExitFlightConsole();
                    cockpitController.CompleteExitFlightConsole();
                }
                GameObject.FindObjectOfType<HatchController>().OpenHatch();
                GameObject.FindObjectOfType<ShipTractorBeamSwitch>().ActivateTractorBeam();
            }
            TeleportBodyTo(Locator.GetPlayerBody(), relativeBody, relativeLocation);
            Locator.GetPlayerCameraController().SetDegreesY(0f);
        }

        public static void TeleportBodyTo(OWRigidbody body, OWRigidbody relativeBody, RelativeLocationData relativeLocation) {
            Vector3 worldPosition = relativeBody.transform.TransformPoint(relativeLocation.localPosition);
            body.WarpToPositionRotation(worldPosition, relativeBody.transform.rotation * relativeLocation.localRotation);
            body.SetVelocity(relativeBody.GetPointVelocity(worldPosition));
            body.SetAngularVelocity(relativeBody.GetAngularVelocity());
        }

        public static void TeleportBodyTo(OWRigidbody body, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
            body.WarpToPositionRotation(position, rotation);
            body.SetVelocity(velocity);
            body.SetAngularVelocity(angularVelocity);
        }

        public static void ToggleSpacesuit(bool instant) {
            if (!Locator.GetPlayerSuit().IsWearingSuit()) {
                EquipSpacesuit(instant);
            } else {
                RemoveSpacesuit(instant);
            }
        }

        public static void EquipSpacesuit(bool instant) {
            Locator.GetPlayerSuit().SuitUp(false, instant);
            Locator.GetPlayerTransform().GetComponent<PlayerResources>()._jetpackThruster.DebugResetBoostCharge();
        }

        public static void RemoveSpacesuit(bool instant) {
            Locator.GetPlayerSuit().RemoveSuit(instant);
        }

        public static void RepairShip() {
            ShipDamageController damageController = Locator.GetShipTransform()?.GetComponent<ShipDamageController>();
            if (damageController == null) return;

            foreach (ShipHull hull in damageController._shipHulls) {
                hull._integrity = 1f;
                hull.RepairTick();
            }

            foreach(ShipComponent component in damageController._shipComponents) {
                component.SetDamaged(false);
            }
        }

        public static ScreenPrompt GetScreenPrompt(string text, PromptPosition position) {
            foreach (ScreenPrompt prompt in Locator.GetPromptManager().GetScreenPromptList(position)._listPrompts) {
                if (prompt._text == text) {
                    return prompt;
                }
            }
            return null;
        }

        public static void AddScreenText(string text, PromptPosition position) {
            if (GetScreenPrompt(text, position) == null) {
                Locator.GetPromptManager().AddScreenPrompt(new ScreenPrompt(text), position, true);
            }
        }

        public static void RemoveScreenText(string text, PromptPosition position) {
            ScreenPrompt prompt = GetScreenPrompt(text, position);
            if (prompt != null) {
                Locator.GetPromptManager().RemoveScreenPrompt(prompt);
            }
        }

        public static CanvasMarker GetMarker(string label) {
            foreach (CanvasMarker marker in Locator.GetMarkerManager()._activeMarkers) {
                if (marker._label == label) {
                    return marker;
                }
            }
            return null;
        }

        public static CanvasMarker SetMarker(string label, OWRigidbody targetBody) {
            var markerManager = Locator.GetMarkerManager();
            CanvasMarker marker = GetMarker(label);

            if (marker == null) {
                marker = markerManager.InstantiateNewMarker();
                markerManager.RegisterMarker(marker, targetBody, label);
                marker.SetVisibility(true);
            }

            if (marker._rigidbodyTarget != targetBody) {
                marker._rigidbodyTarget.OnDestroyOWRigidbody -= marker.OnDestroyOWRigidbody;
                targetBody.OnDestroyOWRigidbody += marker.OnDestroyOWRigidbody;
                marker._rigidbodyTarget = targetBody;
            }

            return marker;
        }

        public static CanvasMarker SetMarker(string label, OWRigidbody parent, Vector3 localPosition) {
            CanvasMarkerManager markerManager = Locator.GetMarkerManager();
            CanvasMarker marker = GetMarker(label);

            if (marker == null) {
                Transform transform = new GameObject($"CanvasMarker_{label}").transform;
                transform.SetParent(parent.transform);
                transform.localPosition = localPosition;

                marker = markerManager.InstantiateNewMarker();
                markerManager.RegisterMarker(marker, transform, label);
                marker.SetVisibility(true);
            }

            if (marker._visualTarget.localPosition != localPosition || marker._visualTarget.parent != parent.transform) {
                marker._visualTarget.SetParent(parent.transform);
                marker._visualTarget.localPosition = localPosition;
            }
            return marker;
        }

        public static CanvasMarker SetMarker(string label, Vector3 position) {
            CanvasMarkerManager markerManager = Locator.GetMarkerManager();
            CanvasMarker marker = GetMarker(label);

            if (marker == null) {
                Transform transform = new GameObject($"CanvasMarker_{label}").transform;
                transform.position = position;

                marker = markerManager.InstantiateNewMarker();
                markerManager.RegisterMarker(marker, transform, label);
                marker.SetVisibility(true);
            }

            if (marker._visualTarget.position != position) {
                marker._visualTarget.position = position;
            }
            return marker;
        }

        public static void RemoveMarker(string label) {
            GetMarker(label)?.DestroyMarker();
        }
    }
}

