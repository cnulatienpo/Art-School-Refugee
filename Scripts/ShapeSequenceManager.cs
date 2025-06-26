using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ShapeSequenceManager : MonoBehaviour
{
    public Transform rotator;
    public TextMeshProUGUI cardText;

    private Dictionary<string, string> shapeText = new Dictionary<string, string>();
    private List<string> shapeOrder = new List<string>();
    private int currentIndex = 0;
    private GameObject currentShape;

    void Start()
    {
        LoadShapeData();

        Transform clearSphere = rotator != null ? rotator.Find("ClearSphere") : null;
        if (clearSphere != null)
        {
            Destroy(clearSphere.gameObject);
        }

        ShowShape(currentIndex);
    }

    void LoadShapeData()
    {
        TextAsset data = Resources.Load<TextAsset>("Data/shape_grammar_cards");
        if (data == null)
        {
            Debug.LogError("shape_grammar_cards.csv not found in Resources/Data");
            return;
        }

        using (StringReader reader = new StringReader(data.text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("name"))
                    continue;

                string[] parts = line.Split(new char[] { ' ' }, 2);
                if (parts.Length < 2)
                    continue;

                string name = parts[0];
                string text = parts[1].Trim();

                if (!shapeText.ContainsKey(name))
                {
                    shapeOrder.Add(name);
                    shapeText.Add(name, text);
                }
            }
        }
    }

    void ShowShape(int index)
    {
        if (index < 0 || index >= shapeOrder.Count)
            return;

        string name = shapeOrder[index];

        if (currentShape != null)
        {
            Destroy(currentShape);
        }

        GameObject prefab = Resources.Load<GameObject>($"Prefabs/Shapes/Shape_{name}");
        if (prefab != null && rotator != null)
        {
            currentShape = Instantiate(prefab, rotator.position, rotator.rotation, rotator);
        }
        else
        {
            Debug.LogWarning($"Prefab for {name} not found");
        }

        if (cardText != null && shapeText.TryGetValue(name, out string text))
        {
            cardText.enableAutoSizing = true;
            cardText.text = text;
        }
    }

    public void ShowNextShape()
    {
        if (shapeOrder.Count == 0)
            return;

        currentIndex = (currentIndex + 1) % shapeOrder.Count;
        ShowShape(currentIndex);
    }
}
