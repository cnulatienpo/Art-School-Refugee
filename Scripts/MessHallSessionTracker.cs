using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Tracks player behaviour during a Mess Hall drawing session and
/// writes the data to a JSON file when the session ends.
/// This is intended only for internal analytics.
/// </summary>
public class MessHallSessionTracker : MonoBehaviour
{
    [Serializable]
    class SessionData
    {
        // Prompt & Setup
        public string promptID = "none";
        public List<string> shapeAssetsUsed = new List<string>();
        public List<string> modifierTags = new List<string>();
        public bool symmetryEnabled;
        public List<string> layersUsed = new List<string>();

        // Session Behaviour
        public float totalDrawingTime;
        public bool sessionAbandoned;
        public int canvasFlipCount;
        public List<float> zoomBehavior = new List<float>();
        public int undoCount;
        public List<string> toolSwitches = new List<string>();

        // Drawing Insights
        public int[][] strokeDensityMap;
        public float avgStrokeSpeed;
        public float eraserVolume;
        public List<string> formZones = new List<string>();
        public List<string> shapeUsageClusters = new List<string>();
    }

    const float AbandonThreshold = 0.05f; // <5% drawing considered abandoned
    const string DataFolder = "messhall_data";

    readonly SessionData data = new SessionData();
    DateTime startTime;
    bool consentGiven;
    string lastTool;
    float activityRatio;

    void Start()
    {
        startTime = DateTime.UtcNow;
    }

    void Update()
    {
        data.totalDrawingTime += Time.deltaTime;
    }

    /// <summary>Call once when the session begins.</summary>
    public void BeginSession(string promptId, bool symmetry, bool insightsConsent)
    {
        data.promptID = string.IsNullOrEmpty(promptId) ? "none" : promptId;
        data.symmetryEnabled = symmetry;
        consentGiven = insightsConsent;
    }

    /// <summary>Record the use of a shape asset.</summary>
    public void AddShapeAsset(string name)
    {
        if (!string.IsNullOrEmpty(name) && !data.shapeAssetsUsed.Contains(name))
            data.shapeAssetsUsed.Add(name);
    }

    /// <summary>Record a modifier tag.</summary>
    public void AddModifierTag(string tag)
    {
        if (!string.IsNullOrEmpty(tag) && !data.modifierTags.Contains(tag))
            data.modifierTags.Add(tag);
    }

    /// <summary>Record that a layer was used.</summary>
    public void AddLayerUsed(string layer)
    {
        if (!string.IsNullOrEmpty(layer) && !data.layersUsed.Contains(layer))
            data.layersUsed.Add(layer);
    }

    /// <summary>Increment the canvas flip counter.</summary>
    public void RegisterCanvasFlip() => data.canvasFlipCount++;

    /// <summary>Record a zoom level.</summary>
    public void LogZoom(float zoom) => data.zoomBehavior.Add(zoom);

    /// <summary>Increment undo count.</summary>
    public void RegisterUndo() => data.undoCount++;

    /// <summary>Record a tool switch.</summary>
    public void SwitchTool(string newTool)
    {
        if (string.IsNullOrEmpty(newTool))
            return;
        if (lastTool != null && newTool != lastTool)
            data.toolSwitches.Add($"{lastTool}\u2192{newTool}");
        lastTool = newTool;
    }

    /// <summary>Update drawing activity ratio (0..1).</summary>
    public void UpdateDrawingActivity(float ratio)
    {
        activityRatio = Mathf.Clamp01(ratio);
    }

    /// <summary>Store drawing insight data if the user consented.</summary>
    public void SetDrawingInsights(int[][] densityMap, float avgSpeed, float erasePct,
        List<string> zones, List<string> clusters)
    {
        if (!consentGiven)
            return;
        data.strokeDensityMap = densityMap;
        data.avgStrokeSpeed = avgSpeed;
        data.eraserVolume = erasePct;
        if (zones != null) data.formZones = new List<string>(zones);
        if (clusters != null) data.shapeUsageClusters = new List<string>(clusters);
    }

    /// <summary>Call when the session ends to write the JSON file.</summary>
    public void EndSession()
    {
        data.sessionAbandoned = activityRatio < AbandonThreshold;

        if (!consentGiven)
        {
            data.strokeDensityMap = null;
            data.formZones.Clear();
            data.shapeUsageClusters.Clear();
            data.avgStrokeSpeed = 0f;
            data.eraserVolume = 0f;
        }

        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        Directory.CreateDirectory(dir);
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(dir, $"{timestamp}.json");
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }
}
