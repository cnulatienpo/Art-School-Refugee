using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds a growing prompt sentence from JSON prompt pieces each time the
/// player clicks the "Next" button. The resulting text is displayed in a
/// TextMeshProUGUI element acting as a prompt bar at the top of the screen.
/// </summary>
public class PromptChainManager : MonoBehaviour
{
    [Serializable]
    public class PromptPiece
    {
        public string shape;
        public string modifier;
        public string doodle_addition;
        public string rendering;
        public string rendering_image;
        public string connector_phrase;
        public string relative_position;

        /// <summary>
        /// Converts the piece data into a natural language sentence.
        /// </summary>
        public string ToSentence(bool includeConnector)
        {
            List<string> parts = new List<string>();

            if (!string.IsNullOrEmpty(shape))
            {
                string s = $"Draw a {shape}";
                if (!string.IsNullOrEmpty(modifier))
                    s += $" that is {modifier.ToLower()}";
                s += ".";
                parts.Add(s);
            }

            if (!string.IsNullOrEmpty(doodle_addition))
            {
                string addition = doodle_addition.Trim().TrimEnd('.');
                addition = char.ToUpper(addition[0]) + addition.Substring(1) + ".";
                parts.Add(addition);
            }

            if (!string.IsNullOrEmpty(rendering))
            {
                string rend = $"Cover it in {rendering.ToLower()}";
                if (!string.IsNullOrEmpty(rendering_image))
                    rend += " \uD83D\uDDBC\uFE0F"; // picture emoji
                rend += ".";
                parts.Add(rend);
            }

            if (!string.IsNullOrEmpty(relative_position))
            {
                string pos = $"Place it {relative_position.Trim().TrimEnd('.')}";
                pos += ".";
                parts.Add(pos);
            }

            string sentence = string.Join(" ", parts);
            if (includeConnector && !string.IsNullOrEmpty(connector_phrase))
                sentence = connector_phrase.Trim() + " " + sentence;
            return sentence;
        }
    }

    [Tooltip("JSON file containing an array of prompt pieces")]
    public TextAsset promptJson;

    public TextMeshProUGUI promptText;

    readonly List<PromptPiece> availablePieces = new List<PromptPiece>();
    readonly List<string> builtSentences = new List<string>();

    void Start()
    {
        LoadPromptPieces();
        UpdatePromptText();
        SetupNextButton();
    }

    void LoadPromptPieces()
    {
        if (promptJson == null)
            return;

        try
        {
            PromptPiece[] data = JsonArrayHelper.FromJson<PromptPiece>(promptJson.text);
            if (data != null)
                availablePieces.AddRange(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse prompt JSON: " + e.Message);
        }
    }

    /// <summary>
    /// Called by the Next button to append a new prompt segment.
    /// </summary>
    public void AddNextPromptPiece()
    {
        if (availablePieces.Count == 0 || promptText == null)
            return;

        PromptPiece piece = availablePieces[UnityEngine.Random.Range(0, availablePieces.Count)];
        string sentence = piece.ToSentence(builtSentences.Count > 0);
        builtSentences.Add(sentence);
        UpdatePromptText();

        // Placeholder for fading out old segments over time
        // StartCoroutine(FadeOldSegments());
    }

    void UpdatePromptText()
    {
        if (promptText != null)
            promptText.text = string.Join(" ", builtSentences);
    }

    void SetupNextButton()
    {
        GameObject obj = GameObject.Find("NextButton");
        if (obj != null)
        {
            Button btn = obj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(AddNextPromptPiece);
        }
    }

    IEnumerator FadeOldSegments()
    {
        // TODO: implement fade effect for earlier text segments
        yield break;
    }

    /// <summary>
    /// Helper to allow JsonUtility to parse top level arrays.
    /// </summary>
    public static class JsonArrayHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrapped = "{\"array\":" + json + "}";
            Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return w.array;
        }

        [Serializable]
        class Wrapper<T>
        {
            public T[] array;
        }
    }
}
