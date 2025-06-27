using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays an outro message after finishing Passage 1.
/// Unlocks Level 2 when the Continue button is pressed.
/// </summary>
public class Passage1CompleteUI : MonoBehaviour
{
    [Tooltip("Reference to the level strip UI")] 
    public LevelStripManager levelStrip;

    GameObject panel;

    void Start()
    {
        CreatePanel();
        panel.SetActive(false);
    }

    void CreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        panel = new GameObject("Passage1CompletePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);

        // Title
        GameObject titleGO = new GameObject("Title", typeof(Text));
        titleGO.transform.SetParent(panel.transform, false);
        Text title = titleGO.GetComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        title.fontSize = 32;
        title.alignment = TextAnchor.UpperCenter;
        title.color = Color.white;
        title.text = "End of Passage One";
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.75f);
        titleRT.anchorMax = new Vector2(0.5f, 0.75f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta = new Vector2(800f, 60f);

        // Body text
        GameObject bodyGO = new GameObject("Body", typeof(Text));
        bodyGO.transform.SetParent(panel.transform, false);
        Text body = bodyGO.GetComponent<Text>();
        body.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        body.fontSize = 24;
        body.alignment = TextAnchor.MiddleCenter;
        body.color = Color.white;
        body.text = "You just did the most important thing-you just mastered the Founding Shapes. well done. now on to passage 2";
        RectTransform bodyRT = body.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRT.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRT.anchoredPosition = Vector2.zero;
        bodyRT.sizeDelta = new Vector2(800f, 100f);

        // Continue button
        GameObject buttonGO = new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(panel.transform, false);
        RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 0.15f);
        buttonRT.anchorMax = new Vector2(0.5f, 0.15f);
        buttonRT.anchoredPosition = Vector2.zero;
        buttonRT.sizeDelta = new Vector2(160f, 40f);

        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(buttonGO.transform, false);
        Text btnText = textGO.GetComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.black;
        btnText.text = "Continue";
        btnText.fontSize = 24;
        RectTransform btnTextRT = btnText.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        Button btn = buttonGO.GetComponent<Button>();
        btn.onClick.AddListener(OnContinueClicked);
    }

    public void Show()
    {
        if (panel == null)
            CreatePanel();
        panel.SetActive(true);
    }

    void OnContinueClicked()
    {
        if (panel != null)
            panel.SetActive(false);

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.MarkLevel1Complete();
        }

        if (levelStrip != null)
            levelStrip.UpdateButtonStates();
    }
}

