using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private List<PromptPiece> promptPieces = new List<PromptPiece>();
    private List<string> promptChain = new List<string>();

    void Start()
    {
        string json = Resources.Load<TextAsset>("prompt_list").text;
        promptPieces = JsonUtilityWrapper.FromJsonList<PromptPiece>(json);

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

        promptChain.Add(chunk);
        promptText.text = string.Join(" ", promptChain);
    }
}
