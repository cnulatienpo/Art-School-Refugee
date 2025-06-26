using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ProgressData
{
    public string level = string.Empty;
    public List<string> shapesSeen = new List<string>();
}

[Serializable]
public class PlayerProfileData
{
    public string playerID;
    public string playerName;
    public string created;
    public string lastSession;
    public ProgressData progress = new ProgressData();
}

/// <summary>
/// Provides access to the current player's profile across scenes.
/// </summary>
public static class PlayerProfile
{
    public static PlayerProfileData Current { get; private set; }

    public static void LoadOrCreate(string username)
    {
        string dir = Path.Combine(Application.persistentDataPath, "Players");
        string path = Path.Combine(dir, $"Player_{username}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Current = JsonUtility.FromJson<PlayerProfileData>(json);
        }
        else
        {
            Directory.CreateDirectory(dir);
            Current = new PlayerProfileData
            {
                playerID = Guid.NewGuid().ToString(),
                playerName = username,
                created = DateTime.UtcNow.ToString("o"),
                lastSession = DateTime.UtcNow.ToString("o"),
                progress = new ProgressData()
            };
            Save();
        }
    }

    public static void StartSession()
    {
        if (Current != null)
        {
            Current.lastSession = DateTime.UtcNow.ToString("o");
            Save();
        }
    }

    public static void Save()
    {
        if (Current == null)
            return;

        string dir = Path.Combine(Application.persistentDataPath, "Players");
        string path = Path.Combine(dir, $"Player_{Current.playerName}.json");
        string json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(path, json);
    }
}
