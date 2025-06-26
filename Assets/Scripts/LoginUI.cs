using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    private string nextScene = "legal disclaimer";

    InputField nameField;

    void Start()
    {
        // Ensure a camera exists
        if (Camera.main == null)
        {
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
        }

        // Setup canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Input field background
        GameObject inputGO = new GameObject("NameInput");
        inputGO.transform.SetParent(canvasGO.transform, false);
        RectTransform inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(200, 40);
        Image inputImage = inputGO.AddComponent<Image>();
        inputImage.color = Color.white;

        // Input field component
        nameField = inputGO.AddComponent<InputField>();
        nameField.targetGraphic = inputImage;
        Text textComp = CreateText("Text", inputGO.transform, "");
        nameField.textComponent = textComp;
        Text placeholder = CreateText("Placeholder", inputGO.transform, "Enter Name");
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.75f);
        nameField.placeholder = placeholder;

        // Start button
        GameObject buttonGO = new GameObject("StartButton");
        buttonGO.transform.SetParent(canvasGO.transform, false);
        RectTransform btnRect = buttonGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 40);
        btnRect.anchoredPosition = new Vector2(0, -60);
        Image btnImage = buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = btnImage;
        Text btnText = CreateText("Text", buttonGO.transform, "Start");
        btnText.alignment = TextAnchor.MiddleCenter;
        button.onClick.AddListener(OnStartClicked);
    }

    Text CreateText(string name, Transform parent, string value)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.text = value;
        txt.alignment = TextAnchor.MiddleLeft;
        return txt;
    }

    void OnStartClicked()
    {
        string username = string.IsNullOrWhiteSpace(nameField.text) ? "Player" : nameField.text.Trim();
        PlayerProfile.LoadOrCreate(username);
        SceneManager.LoadScene(nextScene);
    }
}
