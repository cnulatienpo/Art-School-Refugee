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

    SessionData session;
    SceneEntry currentScene;
    bool drawingActive;
    DateTime lastAutosaveTime;
    string autosavePath;

    void Awake()
    {
        // Ensure a persistent player ID exists
        string id;
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            id = PlayerPrefs.GetString("PlayerID");
        }
        else
        {
            id = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("PlayerID", id);
            PlayerPrefs.Save();
        }

        string dir = Path.Combine(Application.persistentDataPath, "PlayerSessions");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        autosavePath = Path.Combine(dir, $"autosave_{id}.json");

        // Attempt to load from autosave if present
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
                playerID = id,
                sessionStartTime = DateTime.UtcNow.ToString("o")
            };
        }

        lastAutosaveTime = DateTime.UtcNow;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Record the currently loaded scene
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

    /// <summary>
    /// Records that the given shape was viewed in the current scene.
    /// </summary>
    public void LogShapeViewed(string shapeName)
    {
        if (currentScene != null && !string.IsNullOrEmpty(shapeName))
        {
            currentScene.shapes.Add(shapeName);
        }
    }

    /// <summary>
    /// Adds a completed stroke to the session data.
    /// </summary>
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
    }

    /// <summary>
    /// Records the current visibility state of a drawing layer.
    /// </summary>
    public void SetLayerVisibility(int layerIndex, bool isVisible)
    {
        if (currentScene != null)
        {
            currentScene.layerVisibility[layerIndex] = isVisible;
        }
    }

    /// <summary>
    /// Allows other systems to report whether drawing is currently active.
    /// </summary>
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

    void SaveSession()
    {
        session.sessionEndTime = DateTime.UtcNow.ToString("o");

        string dir = Path.Combine(Application.persistentDataPath, "PlayerSessions");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(dir, $"{session.playerID}_{timestamp}.json");

        string json = JsonUtility.ToJson(session, true);
        File.WriteAllText(path, json);

        if (File.Exists(autosavePath))
        {
            File.Delete(autosavePath);
        }
    }
}
