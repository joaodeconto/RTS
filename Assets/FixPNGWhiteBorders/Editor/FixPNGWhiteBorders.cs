using UnityEngine;
using UnityEditor;
using System.Collections;

public class FixPNGWhiteBorders : MonoBehaviour {
	[MenuItem("Assets/Fix PNG White Borders")]
	public static void FixWhiteBordersMenu() {
		try {
			for (int t=0; t<Selection.objects.Length; t++) {
				Object o = Selection.objects[t];
				Texture2D tex = o as Texture2D;
				if (!tex)
					continue;
				string path = AssetDatabase.GetAssetPath(o);
				if (!path.EndsWith(".png")) {
					Debug.LogError(path + " not a png file!");
					continue;
				}

				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
				bool isReadable = importer.isReadable;
				TextureImporterFormat textureFormat = importer.textureFormat;
				TextureImporterNPOTScale npotScale = importer.npotScale;
				if (!importer.isReadable || importer.textureFormat != TextureImporterFormat.ARGB32 || importer.npotScale != TextureImporterNPOTScale.None) {
					importer.npotScale = TextureImporterNPOTScale.None;
					importer.isReadable = true;
					importer.textureFormat = TextureImporterFormat.ARGB32;
					AssetDatabase.ImportAsset(path);
					tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				}

				if (EditorUtility.DisplayCancelableProgressBar("Processing...", System.IO.Path.GetFileName(path), (float)t / Selection.objects.Length))
					break;

				int w = tex.width, h = tex.height, all = w * h;
				Color32[] colors = tex.GetPixels32(), cs = new Color32[all];
				bool[] hasColor = new bool[all], newHasColor = new bool[all];

				for (int i=0; i<all; i++) {
					if (colors[i].a != 0) {
						hasColor[i] = true;
						newHasColor[i] = true;
						cs[i] = colors[i];
					}
				}

				int count = 2;
				if (importer.mipmapEnabled) {
					count = Mathf.Max(w / 8, 4);
				}

				for (int k=0; k<count; k++) {
					for (int i=0; i<all; i++) {
						if (!hasColor[i]) {
							int cnt = 0, r = 0, g = 0, b = 0;
							if (i > 1 && hasColor[i - 1]) {
								Color32 c = colors[i - 1];
								r+= c.r;
								g+= c.g;
								b+= c.b;
								cnt++;
							}
							if (i < all - 1 && hasColor[i + 1]) {
								Color32 c = colors[i + 1];
								r+= c.r;
								g+= c.g;
								b+= c.b;
								cnt++;
							}
							if (i > w && hasColor[i - w]) {
								Color32 c = colors[i - w];
								r+= c.r;
								g+= c.g;
								b+= c.b;
								cnt++;
							}
							if (i < all - w - 1 && hasColor[i + w]) {
								Color32 c = colors[i + w];
								r+= c.r;
								g+= c.g;
								b+= c.b;
								cnt++;
							}
							if (cnt > 0) {
								cs[i] = new Color32((byte)(r / cnt), (byte)(g / cnt), (byte)(b / cnt), 0);
								newHasColor[i] = true;
							}
						}
					}
					for (int i=0; i<all; i++) {
						colors[i] = cs[i];
						hasColor[i] = newHasColor[i];
					}
				}
				tex.SetPixels32(colors);

				byte[] data = tex.EncodeToPNG();
				if (data != null && data.Length > 0)
					System.IO.File.WriteAllBytes(path, data);

				importer.isReadable = isReadable;
				importer.textureFormat = textureFormat;
				importer.npotScale = npotScale;
			}
		} finally {
			EditorUtility.ClearProgressBar();
			AssetDatabase.Refresh();
		}
	}
}