using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LegalDisclaimerUI : MonoBehaviour
{
    [SerializeField]
    private string nextScene = "level1 intro card";

    void Start()
    {
        // Ensure a camera exists
        if (Camera.main == null)
        {
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
        }

        // Canvas setup
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel for text
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 300);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(1, 1, 1, 0.1f);

        // Disclaimer text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panelGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        Text text = textGO.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = "Data Collection & Usage Notice\n\n" +
                    "This game collects and stores gameplay data, including your drawing progress, brushstrokes, and time spent in the game. A unique, anonymous identifier is used internally to track your progress.\n\n" +
                    "Your drawing data may be used in a public dataset for research or educational purposes.\n\n" +
                    "No identifying personal information is collected, stored, or sold. Your data will never be sold to third parties.\n\n" +
                    "By continuing, you agree to this data collection and use.";
        text.fontSize = 24;
        text.color = Color.black;

        // Button
        GameObject buttonGO = new GameObject("Button");
        buttonGO.transform.SetParent(canvasGO.transform, false);
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = new Vector2(0, -200);
        Image buttonImage = buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();

        // Button text
        GameObject bTextGO = new GameObject("Text");
        bTextGO.transform.SetParent(buttonGO.transform, false);
        RectTransform bTextRect = bTextGO.AddComponent<RectTransform>();
        bTextRect.anchorMin = Vector2.zero;
        bTextRect.anchorMax = Vector2.one;
        bTextRect.offsetMin = Vector2.zero;
        bTextRect.offsetMax = Vector2.zero;
        Text bText = bTextGO.AddComponent<Text>();
        bText.alignment = TextAnchor.MiddleCenter;
        bText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bText.text = "I Understand";
        bText.fontSize = 24;
        bText.color = Color.black;

        // Button click event
        button.onClick.AddListener(() => SceneManager.LoadScene(nextScene));
    }
}
