using System.IO;
using UnityEngine;

/// <summary>
/// Manages the numbered drawing layers used in sketchbook mode.
/// Other systems query the active texture to know where strokes should be drawn.
/// </summary>
public class LayerManager : MonoBehaviour
{
    [Tooltip("Drawing surfaces for Layer 1, Layer 2 and Layer 3.")] 
    public Texture[] layers = new Texture[3];

    [Header("Session Tracking")]
    public MessHallSessionTracker sessionTracker;

    int activeIndex;

    /// <summary>Switches the active drawing layer.</summary>
    public void SetActiveLayer(int index)
    {
        if (index < 0 || index >= layers.Length)
            return;
        activeIndex = index;
        if (sessionTracker != null)
            sessionTracker.AddLayerUsed((index + 1).ToString());
    }

    /// <summary>Returns the texture currently targeted for drawing.</summary>
    public Texture GetActiveTexture()
    {
        if (activeIndex >= 0 && activeIndex < layers.Length)
            return layers[activeIndex];
        return null;
    }

    /// <summary>
    /// Returns copies of all layer contents as Texture2D objects.
    /// RenderTextures are read back before being returned.
    /// </summary>
    public Texture2D[] ExportAllLayers()
    {
        Texture2D[] results = new Texture2D[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            Texture src = layers[i];
            if (src == null) continue;

            if (src is Texture2D tex2D)
            {
                Texture2D copy = new Texture2D(tex2D.width, tex2D.height, tex2D.format, false);
                copy.SetPixels(tex2D.GetPixels());
                copy.Apply();
                results[i] = copy;
            }
            else if (src is RenderTexture rt)
            {
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;
                results[i] = tex;
            }
        }
        return results;
    }

    /// <summary>
    /// Saves each layer to PNG files inside <paramref name="directory"/>.
    /// </summary>
    public void ExportAllLayers(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        Texture2D[] textures = ExportAllLayers();
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D tex = textures[i];
            if (tex == null) continue;
            string path = Path.Combine(directory, $"layer{i + 1}.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }
    }

    /// <summary>
    /// Merges all layers from bottom to top into a single Texture2D.
    /// Any RenderTextures are read back before blending.
    /// When <paramref name="disableLayers"/> is true the references are
    /// cleared after merging so they no longer receive input.
    /// </summary>
    public Texture2D MergeAllLayers(bool disableLayers = false)
    {
        Texture2D[] textures = ExportAllLayers();
        Texture2D baseTex = null;
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                baseTex = textures[i];
                break;
            }
        }
        if (baseTex == null)
            return null;

        Texture2D merged = new Texture2D(baseTex.width, baseTex.height, TextureFormat.RGBA32, false);
        Color[] result = new Color[baseTex.width * baseTex.height];

        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D tex = textures[i];
            if (tex == null) continue;
            Color[] src = tex.GetPixels();
            for (int p = 0; p < result.Length; p++)
            {
                Color dst = result[p];
                Color col = src[p];
                result[p] = Color.Lerp(dst, col, col.a);
            }
        }

        merged.SetPixels(result);
        merged.Apply();

        if (disableLayers)
        {
            for (int i = 0; i < layers.Length; i++)
                layers[i] = null;
        }

        return merged;
    }
}
