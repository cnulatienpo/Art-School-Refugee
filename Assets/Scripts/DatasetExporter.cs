using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Utility class that exports drawing layers and session metadata
/// into a dataset folder when a player session ends.
/// </summary>
public static class DatasetExporter
{
    // Size of the textures used by the drawing system
    const int TextureSize = 1024;

    [Serializable]
    class StrokeData
    {
        public List<Vector2> points;
        public Color color;
        public float width;
        public int layerIndex;
    }

    [Serializable]
    class SceneEntry
    {
        public string sceneName;
        public string entryTime;
        public List<string> shapes;
        public List<StrokeData> strokes;
    }

    [Serializable]
    class SessionData
    {
        public string playerID;
        public string sessionStartTime;
        public string sessionEndTime;
        public float totalDrawingTime;
        public List<SceneEntry> sceneEntries;
    }

    [Serializable]
    class LayerInfo
    {
        public int layerIndex;
        public string pngFile;
    }

    [Serializable]
    class LevelInfo
    {
        public string sceneName;
        public List<LayerInfo> layers = new List<LayerInfo>();
    }

    [Serializable]
    class DatasetInfo
    {
        public SessionData session;
        public List<LevelInfo> levels = new List<LevelInfo>();
    }

    /// <summary>
    /// Entry point used by <see cref="PlayerSessionManager"/> to export the dataset.
    /// </summary>
    public static void Export(string sessionJson, string playerId, string timestamp)
    {
        if (string.IsNullOrEmpty(sessionJson))
            return;

        SessionData data = JsonUtility.FromJson<SessionData>(sessionJson);
        if (data == null)
            return;

        string root = Path.Combine(Application.persistentDataPath,
            "ExportedDatasets",
            $"Player_{playerId}_{timestamp}");
        Directory.CreateDirectory(root);

        DatasetInfo dataset = new DatasetInfo();
        dataset.session = data;

        foreach (SceneEntry scene in data.sceneEntries)
        {
            LevelInfo levelInfo = new LevelInfo { sceneName = scene.sceneName };
            Dictionary<int, Texture2D> layers = new Dictionary<int, Texture2D>();

            if (scene.strokes != null)
            {
                foreach (StrokeData stroke in scene.strokes)
                {
                    if (stroke.points == null || stroke.points.Count < 2)
                        continue;

                    if (!layers.TryGetValue(stroke.layerIndex, out Texture2D tex))
                    {
                        tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
                        ClearTexture(tex, new Color(0, 0, 0, 0));
                        layers.Add(stroke.layerIndex, tex);
                    }

                    for (int i = 1; i < stroke.points.Count; i++)
                    {
                        DrawLine(tex, stroke.points[i - 1], stroke.points[i], stroke.color, Mathf.RoundToInt(stroke.width));
                    }
                }
            }

            foreach (var kvp in layers)
            {
                string fileName = $"{scene.sceneName}_Layer_{kvp.Key}.png";
                File.WriteAllBytes(Path.Combine(root, fileName), kvp.Value.EncodeToPNG());

                levelInfo.layers.Add(new LayerInfo
                {
                    layerIndex = kvp.Key,
                    pngFile = fileName
                });
            }

            dataset.levels.Add(levelInfo);
        }

        string metaPath = Path.Combine(root, "dataset.json");
        File.WriteAllText(metaPath, JsonUtility.ToJson(dataset, true));
    }

    static void ClearTexture(Texture2D tex, Color c)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = c;
        tex.SetPixels(pixels);
        tex.Apply();
    }

    static void DrawLine(Texture2D tex, Vector2 a, Vector2 b, Color col, int width)
    {
        int x0 = Mathf.RoundToInt(a.x);
        int y0 = Mathf.RoundToInt(a.y);
        int x1 = Mathf.RoundToInt(b.x);
        int y1 = Mathf.RoundToInt(b.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawBrush(tex, x0, y0, col, width);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        tex.Apply();
    }

    static void DrawBrush(Texture2D tex, int cx, int cy, Color col, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;
                int px = cx + x;
                int py = cy + y;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, col);
            }
        }
    }
}
