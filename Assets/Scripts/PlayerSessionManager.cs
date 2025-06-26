using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically tracks a player's sessions and drawing activity without
/// any user interaction. Session data is written to a JSON file on disk
/// when the application quits.
/// </summary>
public class PlayerSessionManager : MonoBehaviour
{
    [Serializable]
    class StrokeData
    {
        public List<Vector2> points = new List<Vector2>();
        public Color color;
        public float width;
        public int layerIndex;
    }

    [Serializable]
    class SceneEntry
    {
        public string sceneName;
        public string entryTime;
        public List<string> shapes = new List<string>();
        public List<StrokeData> strokes = new List<StrokeData>();
        public Dictionary<int, bool> layerVisibility = new Dictionary<int, bool>();
    }

    [Serializable]
    class SessionData
    {
        public string playerID;
        public string sessionStartTime;
        public string sessionEndTime;
        public float totalDrawingTime;
        public List<SceneEntry> sceneEntries = new List<SceneEntry>();
    }

    [Serializable]
    class SessionSummary
    {
        public string playerID;
        public int shapesSeen;
        public int layersUsed;
        public int strokes;
        public int drawingTime;
        public bool sessionComplete;
    }

    SessionData session;
    SceneEntry currentScene;
    bool drawingActive;
    DateTime lastAutosaveTime;
    string autosavePath;
    HashSet<int> usedLayers = new HashSet<int>();
    int shapesViewed;
    int strokeCount;
    bool sessionComplete;
    bool sessionSaved;
    bool summarySaved;

    void Start()
    {
        if (PlayerProfile.Current == null)
        {
            PlayerProfile.LoadOrCreate("Player");
        }

        PlayerProfile.StartSession();

        string playerID = PlayerProfile.Current.playerID;

        string dir = Path.Combine(Application.persistentDataPath, "PlayerSessions");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        autosavePath = Path.Combine(dir, $"autosave_{playerID}.json");

        if (File.Exists(autosavePath))
        {
            try
            {
                string json = File.ReadAllText(autosavePath);
                SessionData loaded = JsonUtility.FromJson<SessionData>(json);
                if (loaded != null)
                {
                    session = loaded;
                }
            }
            catch (Exception)
            {
                session = null;
            }

            File.Delete(autosavePath);
        }

        if (session == null)
        {
            session = new SessionData
            {
                playerID = playerID,
                sessionStartTime = DateTime.UtcNow.ToString("o")
            };
        }

        lastAutosaveTime = DateTime.UtcNow;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void Update()
    {
        if (drawingActive)
        {
            session.totalDrawingTime += Time.deltaTime;
        }

        if ((DateTime.UtcNow - lastAutosaveTime).TotalMinutes >= 2)
        {
            SaveAutosave();
            lastAutosaveTime = DateTime.UtcNow;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = new SceneEntry
        {
            sceneName = scene.name,
            entryTime = DateTime.UtcNow.ToString("o")
        };
        session.sceneEntries.Add(currentScene);
    }

    public void LogShapeViewed(string shapeName)
    {
        if (currentScene != null && !string.IsNullOrEmpty(shapeName))
        {
            currentScene.shapes.Add(shapeName);
            shapesViewed++;
        }
    }

    public void LogStroke(List<Vector2> points, Color color, float width, int layerIndex)
    {
        if (currentScene == null || points == null)
            return;

        StrokeData stroke = new StrokeData
        {
            points = new List<Vector2>(points),
            color = color,
            width = width,
            layerIndex = layerIndex
        };
        currentScene.strokes.Add(stroke);

        strokeCount++;
        usedLayers.Add(layerIndex);
    }

    public void SetLayerVisibility(int layerIndex, bool isVisible)
    {
        if (currentScene != null)
        {
            currentScene.layerVisibility[layerIndex] = isVisible;
        }
    }

    public void SetDrawingActive(bool active)
    {
        drawingActive = active;
    }

    void OnApplicationQuit()
    {
        SaveSession();
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            SaveSession();
        }
    }

    void SaveAutosave()
    {
        if (session == null)
            return;

        string json = JsonUtility.ToJson(session, true);
        File.WriteAllText(autosavePath, json);
    }

    void SaveSummary(string timestamp)
    {
        if (summarySaved)
            return;
        summarySaved = true;

        SessionSummary summary = new SessionSummary
        {
            playerID = session.playerID,
            shapesSeen = shapesViewed,
            layersUsed = usedLayers.Count,
            strokes = strokeCount,
            drawingTime = Mathf.RoundToInt(session.totalDrawingTime),
            sessionComplete = sessionComplete
        };

        string dir = Path.Combine(Application.persistentDataPath, "SessionSummaries");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"{session.playerID}_{timestamp}.json");
        File.WriteAllText(path, JsonUtility.ToJson(summary, true));
    }

    public void MarkSessionComplete()
    {
        if (sessionComplete)
            return;

        sessionComplete = true;
        session.sessionEndTime = DateTime.UtcNow.ToString("o");

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        SaveSession();
        SaveSummary(timestamp);
    }

    void SaveSession()
    {
        if (sessionSaved)
            return;
        sessionSaved = true;

        if (string.IsNullOrEmpty(session.sessionEndTime))
            session.sessionEndTime = DateTime.UtcNow.ToString("o");

        string dir = Path.Combine(Application.persistentDataPath, "PlayerSessions");
        Directory.CreateDirectory(dir);

        string playerID = session.playerID;
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string json = JsonUtility.ToJson(session, true);
        string path = Path.Combine(dir, $"session_{playerID}_{timestamp}.json");
        File.WriteAllText(path, json);

        if (File.Exists(autosavePath))
        {
            File.Delete(autosavePath);
        }

        PlayerProfile.Save();

        DatasetExporter.Export(json, session.playerID, timestamp);
    }
}
