using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Simple helper that swaps drawing objects without reloading the scene.
/// </summary>
public class DrawingObjectSwitcher : MonoBehaviour
{
    public GameObject currentObject;
    public Transform spawnPoint;
    public TextMeshProUGUI objectLabel;

    readonly HashSet<string> availableShapes = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        LoadShapeNames();
    }

    void LoadShapeNames()
    {
        availableShapes.Clear();
        TextAsset data = Resources.Load<TextAsset>("Data/shape_grammar_cards");
        if (data == null)
            return;

        using (StringReader reader = new StringReader(data.text))
        {
            string line;
            bool first = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (first)
                {
                    first = false;
                    continue; // skip header
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(',');
                if (parts.Length > 0)
                {
                    string name = parts[0].Trim();
                    if (!string.IsNullOrEmpty(name))
                        availableShapes.Add(name.ToLower());
                }
            }
        }
    }

    /// <summary>
    /// Replaces the current drawing object with <paramref name="nextPrefab"/>.
    /// </summary>
    public void SwitchToNextObject(GameObject nextPrefab, string labelText)
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        ClearCanvas();

        if (nextPrefab != null && spawnPoint != null)
        {
            currentObject = Instantiate(nextPrefab, spawnPoint.position,
                                        spawnPoint.rotation, spawnPoint);
        }

        if (objectLabel != null)
        {
            objectLabel.text = labelText;
        }
    }

    /// <summary>
    /// Loads and instantiates a shape prefab by name.
    /// </summary>
    public void LoadShapeByName(string shapeName)
    {
        if (string.IsNullOrEmpty(shapeName))
            return;

        if (availableShapes.Count == 0)
            LoadShapeNames();

        string lookup = shapeName.ToLower();
        if (!availableShapes.Contains(lookup))
        {
            Debug.LogWarning($"Shape '{shapeName}' not found in data list.");
            return;
        }

        string prefabName = "the" + lookup;
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"Prefab '{prefabName}' not found in Resources.");
            return;
        }

        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        ClearCanvas();

        if (spawnPoint != null)
        {
            currentObject = Instantiate(prefab, spawnPoint.position,
                                        spawnPoint.rotation, spawnPoint);
        }

        if (objectLabel != null)
        {
            objectLabel.text = shapeName;
        }
    }
}
