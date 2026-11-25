using UnityEngine;

namespace CheeseTools.src {
    public static class SettingsGUISkin {
        public static GUISkin skin;
        public static GUIStyle popupSelectorButtonStyle;
        public static GUIStyle popupSelectorWindowStyle;
        public static GUIStyle popupSelectorOptionStyle;
        public static Texture2D windowTexture = Solid(new Color(0.06f, 0.06f, 0.06f, 0.94f));
        public static Texture2D frameTexture = Solid(new Color(0.22f, 0.18f,0.18f, 1f));
        public static Texture2D accentTexture = Solid(new Color(0.98f, 0.59f, 0.26f, 0.40f));
        public static Texture2D accentHoverTexture = Solid(new Color(0.98f, 0.59f, 0.26f, 1.00f));
        public static Texture2D accentActiveTexture = Solid(new Color(0.98f, 0.53f, 0.06f, 1.00f));

        public static void BuildSkin() {
            skin = Object.Instantiate(GUI.skin);
            skin.hideFlags = HideFlags.HideAndDontSave;

            GUIStyle window = skin.window;
            window.normal.background = window.onNormal.background = windowTexture;
            window.padding = SettingsGUI.ScaledRectOffset(new(7, 7, 14, 7));

            GUIStyle button = skin.button;
            button.normal.background = accentTexture;
            button.hover.background = accentHoverTexture;
            button.active.background = accentActiveTexture;
            button.onNormal.background = accentActiveTexture;
            button.onHover.background = accentHoverTexture;
            button.onActive.background = accentActiveTexture;
            button.stretchWidth = false;
            button.wordWrap = false;
            button.fontSize = SettingsGUI.FontSize;
            
            GUIStyle toggle = skin.toggle;
            toggle.normal.background = toggle.onNormal.background = null;
            toggle.hover.background = toggle.onHover.background = null;
            toggle.active.background = toggle.onActive.background = null;
            toggle.padding = new RectOffset(22, 0, 0, 0);
            toggle.stretchWidth = false;
            toggle.wordWrap = false;
            toggle.fontSize = SettingsGUI.FontSize;

            GUIStyle label = skin.label;
            label.alignment = TextAnchor.MiddleCenter;
            label.stretchWidth = false;
            label.wordWrap = false;
            label.fontSize = SettingsGUI.FontSize;
            label.richText = true;

            popupSelectorButtonStyle = new(button);
            popupSelectorButtonStyle.normal.background = frameTexture;
            popupSelectorButtonStyle.hover.background = accentTexture;
            popupSelectorButtonStyle.active.background = frameTexture;
            popupSelectorButtonStyle.onNormal.background = frameTexture;
            popupSelectorButtonStyle.onHover.background = frameTexture;
            popupSelectorButtonStyle.onActive.background = frameTexture;

            popupSelectorWindowStyle = new(window);
            popupSelectorWindowStyle.padding = new RectOffset();

            popupSelectorOptionStyle = new(button);
            popupSelectorOptionStyle.normal.background = windowTexture;
        }

        public static Texture2D Solid(Color color) {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

    }
}
