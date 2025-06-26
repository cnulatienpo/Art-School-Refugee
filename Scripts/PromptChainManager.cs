using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

[System.Serializable]
public class PromptPiece
{
    public string shape;
    public string modifier;
    public string doodle_addition;
    public string rendering;
    public string rendering_image;
    public string connector_phrase;
    public string relative_position;
}

public class PromptChainManager : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public Button nextButton;
    public Image renderingImageUI;

    private List<PromptPiece> promptPieces = new List<PromptPiece>();
    private List<string> promptChain = new List<string>();
    private Dictionary<string, string> renderingLookup = new Dictionary<string, string>();

    Dictionary<string, string> ParseReferenceJson(string json)
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(json))
            return dict;

        json = json.Trim().Trim('{', '}');
        var entries = json.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var kv = entry.Split(new char[] { ':' }, 2);
            if (kv.Length != 2)
                continue;
            string key = kv[0].Trim().Trim('"');
            string value = kv[1].Trim().Trim('"');
            dict[key] = value;
        }
        return dict;
    }

    void UpdateRenderingImage(string rendering)
    {
        if (renderingImageUI == null)
            return;

        if (string.IsNullOrEmpty(rendering))
        {
            renderingImageUI.gameObject.SetActive(false);
            return;
        }

        if (renderingLookup.TryGetValue(rendering, out string file))
        {
            string path = System.IO.Path.Combine("RenderingSwatches", System.IO.Path.GetFileNameWithoutExtension(file));
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                renderingImageUI.sprite = sprite;
                renderingImageUI.gameObject.SetActive(true);
                return;
            }
        }

        // Hide if lookup or load failed
        renderingImageUI.gameObject.SetActive(false);
    }

    void Start()
    {
        string json = Resources.Load<TextAsset>("prompt_list").text;
        promptPieces = JsonUtilityWrapper.FromJsonList<PromptPiece>(json);

        // Load the rendering reference dictionary
        TextAsset refAsset = Resources.Load<TextAsset>("RenderingSwatches/rendering_reference");
        if (refAsset != null)
            renderingLookup = ParseReferenceJson(refAsset.text);
        else
            Debug.LogWarning("Rendering reference JSON not found under Resources/RenderingSwatches/");

        nextButton.onClick.AddListener(AddNextPromptPiece);
        AddNextPromptPiece(); // Start with one
    }

    public void AddNextPromptPiece()
    {
        var piece = promptPieces[Random.Range(0, promptPieces.Count)];
        string chunk = piece.connector_phrase + " draw a " + piece.shape.ToLower();

        if (!string.IsNullOrEmpty(piece.modifier))
            chunk += " that is " + piece.modifier.ToLower();

        if (!string.IsNullOrEmpty(piece.relative_position))
            chunk += " " + piece.relative_position;

        if (!string.IsNullOrEmpty(piece.doodle_addition))
            chunk += ", and " + piece.doodle_addition.ToLower();

        if (!string.IsNullOrEmpty(piece.rendering))
            chunk += ". Cover it in " + piece.rendering.ToLower() + " \uD83D\uDDBC\uFE0F";

        UpdateRenderingImage(piece.rendering);

        promptChain.Add(chunk);
        UpdatePromptDisplay();
    }

    void UpdatePromptDisplay()
    {
        if (promptText == null)
            return;

        promptText.richText = true;
        var sb = new System.Text.StringBuilder();
        int count = promptChain.Count;
        for (int i = 0; i < count; i++)
        {
            int age = count - 1 - i; // 0 is newest
            float intensity = Mathf.Clamp01(1f - age * 0.15f);
            Color color = new Color(intensity, intensity, intensity);
            string hex = ColorUtility.ToHtmlStringRGB(color);
            sb.Append("<color=#" + hex + ">" + promptChain[i] + "</color>");
            if (i < count - 1)
                sb.Append(" ");
        }

        promptText.text = sb.ToString();
    }
}
