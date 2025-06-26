using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI helper that saves the current sketch when clicked.
/// Attach this to a button placed in the Mess Hall UI.
/// </summary>
public class SaveDrawingButton : MonoBehaviour
{
    [Tooltip("Exporter responsible for writing the image file")] public SketchbookExporter exporter;
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
        else
            Debug.LogWarning($"{name} is missing a Button component.");
    }

    void OnClick()
    {
        if (exporter == null)
        {
            Debug.LogWarning("SketchbookExporter reference not set.");
            return;
        }

        string path = GetNextPath();
        exporter.ExportMergedDrawing(false);
        Debug.Log($"Saved to: {path}");
    }

    string GetNextPath()
    {
        string directory;
        if (exporter.saveToDesktop)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            directory = string.IsNullOrEmpty(desktop) ? Application.persistentDataPath : desktop;
        }
        else
        {
            directory = Application.persistentDataPath;
        }
        return SketchbookExporter.GetNextAvailableFilename(directory, "messhall_sketch_", ".png");
    }
}
