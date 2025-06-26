using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Developer-only panel for managing Mess Hall session logs.
/// Attach this to a Canvas GameObject and assign the UI elements
/// in the inspector. The panel is hidden until the secret
/// key combo is pressed (Ctrl+Shift+M or Alt+D).
/// </summary>
public class DataMenu : MonoBehaviour
{
    [Tooltip("Root panel to toggle visibility")] public GameObject panel;
    [Tooltip("Button that opens the data folder")] public Button openFolderButton;
    [Tooltip("Button that deletes all log files")] public Button clearLogsButton;
    [Tooltip("Button that exports all logs to a zip")] public Button exportZipButton;
    [Tooltip("Optional text showing log info")] public Text infoText;

    const string DataFolder = "messhall_data";

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (openFolderButton != null)
            openFolderButton.onClick.AddListener(OpenDataFolder);
        if (clearLogsButton != null)
            clearLogsButton.onClick.AddListener(ClearAllLogs);
        if (exportZipButton != null)
            exportZipButton.onClick.AddListener(ExportAllAsZip);

        UpdateInfoText();
    }

    void Update()
    {
        bool combo1 = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                       (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                       Input.GetKeyDown(KeyCode.M);
        bool combo2 = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) &&
                       Input.GetKeyDown(KeyCode.D);

        if (combo1 || combo2)
            TogglePanel();
    }

    void TogglePanel()
    {
        if (panel == null)
            return;

        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf)
            UpdateInfoText();
    }

    void OpenDataFolder()
    {
        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        Directory.CreateDirectory(dir);

#if UNITY_STANDALONE_WIN
        Process.Start("explorer.exe", dir.Replace('/', '\\'));
#elif UNITY_STANDALONE_OSX
        Process.Start("open", dir);
#elif UNITY_STANDALONE_LINUX
        Process.Start("xdg-open", dir);
#else
        Application.OpenURL($"file://{dir}");
#endif
    }

    void ClearAllLogs()
    {
        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        if (Directory.Exists(dir))
        {
            foreach (string file in Directory.GetFiles(dir, "*.json"))
                File.Delete(file);
        }
        UpdateInfoText();
    }

    void ExportAllAsZip()
    {
        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        if (!Directory.Exists(dir))
        {
            Debug.LogWarning("DataMenu: no data folder to export.");
            return;
        }

        string[] files = Directory.GetFiles(dir, "*.json");
        if (files.Length == 0)
        {
            Debug.LogWarning("DataMenu: no logs to export.");
            return;
        }

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string zipPath = Path.Combine(dir, $"logs_{timestamp}.zip");
        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (string file in files)
                archive.CreateEntryFromFile(file, Path.GetFileName(file));
        }

        Debug.Log($"DataMenu: exported logs to {zipPath}");
        UpdateInfoText();
    }

    void UpdateInfoText()
    {
        if (infoText == null)
            return;

        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        if (!Directory.Exists(dir))
        {
            infoText.text = "Logs: 0";
            return;
        }

        string[] files = Directory.GetFiles(dir, "*.json");
        int count = files.Length;

        string lastFile = "none";
        DateTime lastTime = DateTime.MinValue;
        foreach (string file in files)
        {
            DateTime t = File.GetLastWriteTime(file);
            if (t > lastTime)
            {
                lastTime = t;
                lastFile = Path.GetFileName(file);
            }
        }

        infoText.text = $"Logs: {count}\nLast: {lastFile}";
    }
}

/*
Setup notes:
1. Create a UI Panel (any layout) under a Canvas and add this script.
2. Assign the Panel GameObject itself to the `panel` field.
3. Add Buttons for opening the folder, clearing logs and exporting the zip.
   Hook them up to the respective fields or let this script auto-wire
   by placing the buttons in the inspector.
4. Optional: add a Text component for `infoText` to display log count.
5. The panel starts hidden and appears when pressing Ctrl+Shift+M or Alt+D.
*/
