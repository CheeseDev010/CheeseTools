using UnityEngine;

namespace CheeseTools.src {
    public static class ModUtils {
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
