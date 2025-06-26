using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerProfileData
{
    public string playerName;
    public string uuid;
    public string progress;
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
                playerName = username,
                uuid = Guid.NewGuid().ToString(),
                progress = string.Empty
            };
            string json = JsonUtility.ToJson(Current, true);
            File.WriteAllText(path, json);
        }
    }
}
