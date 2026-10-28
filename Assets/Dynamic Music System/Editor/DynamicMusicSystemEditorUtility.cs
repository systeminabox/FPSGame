using UnityEngine;
using UnityEditor;

namespace MordiAudio.MusicSystem.Utility
{
	static public class Colors
    {
		static public Color
			colHighlight = new Color(0.95f, 0.58f, 0.3f),
			colHighlightAlt = new Color(0.4f, 0.5f, 1f, 1f),
			colWaveformBackground = new Color(0.156f, 0.156f, 0.156f, 0.5f),
			colFaderBackground = new Color(0.2f, 0.2f, 0.2f, 0.25f),
			colTimelineBackground = new Color(0.156f, 0.156f, 0.156f, 1f);
	}

    static public class Assets
    {
        static string GetAssetPath(string name, string type) {
            string[] assetGUIDs = AssetDatabase.FindAssets($"{name} t:{type}");

            if (assetGUIDs == null)
                return null;

            string path = null;
            if (assetGUIDs != null) {
                if (assetGUIDs.Length > 0) {
                    path = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
                }
            }

            return path;
        }

        public static Texture LoadTexture(string name) {
            string path = GetAssetPath(name, "Texture");
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAssetAtPath<Texture>(path);
        }

        public static Material LoadMaterial(string name) {
            string path = GetAssetPath(name, "Material");
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }
    }
}
