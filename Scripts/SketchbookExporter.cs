using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Saves the merged sketchbook layers to disk.
///
/// Setup notes:
/// - Add this component anywhere in your scene and assign <see cref="layerManager"/>.
/// - Files are written to <c>Application.persistentDataPath</c> by default so
///   standalone builds have write access. Toggle <see cref="saveToDesktop"/>
///   to use the user's desktop instead.
/// - Ensure your project has permission to write to the chosen location on the
///   target platform.
/// </summary>
public class SketchbookExporter : MonoBehaviour
{
    [Tooltip("Layer manager providing the drawing layers")] public LayerManager layerManager;
    [Tooltip("Write files to the user's desktop when possible")] public bool saveToDesktop = false;

    /// <summary>
    /// Merges all drawing layers and writes the result to disk. When
    /// <paramref name="asJPEG"/> is true the file is saved as JPG, otherwise as PNG.
    /// </summary>
    public void ExportMergedDrawing(bool asJPEG)
    {
        if (layerManager == null)
        {
            Debug.LogWarning("SketchbookExporter: LayerManager reference missing.");
            return;
        }

        Texture2D merged = layerManager.MergeAllLayers();
        if (merged == null)
        {
            Debug.LogWarning("SketchbookExporter: nothing to export.");
            return;
        }

        string directory = GetSaveDirectory();
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string extension = asJPEG ? ".jpg" : ".png";
        string path = GetNextAvailableFilename(directory, "messhall_sketch_", extension);

        byte[] data = asJPEG ? merged.EncodeToJPG() : merged.EncodeToPNG();
        File.WriteAllBytes(path, data);

        Debug.Log($"Exported sketch to {path}");
    }

    string GetSaveDirectory()
    {
        if (saveToDesktop)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!string.IsNullOrEmpty(desktop))
                return desktop;
        }
        return Application.persistentDataPath;
    }

    /// <summary>
    /// Returns a file path that doesn't already exist by appending an
    /// incrementing number.
    /// </summary>
    public static string GetNextAvailableFilename(string directory, string baseName, string extension)
    {
        int index = 1;
        string path;
        do
        {
            string number = index.ToString("D3");
            string filename = $"{baseName}{number}{extension}";
            path = Path.Combine(directory, filename);
            index++;
        } while (File.Exists(path));
        return path;
    }
}
