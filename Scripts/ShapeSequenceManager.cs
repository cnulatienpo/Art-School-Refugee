using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShapeSequenceManager : MonoBehaviour
{
    public Transform rotator;
    public TextMeshProUGUI cardText;
    // Reference to the rotation gizmo so we can assign the active object
    [SerializeField]
    public MouseRotateTarget rotationGizmo;

    private Dictionary<string, string> shapeText = new Dictionary<string, string>();
    private List<string> shapeOrder = new List<string>();
    private int currentIndex = 0;
    private GameObject currentShape;

    void Start()
    {
        LoadShapeData();

        // If the rotation gizmo wasn't assigned in the inspector, try to
        // locate it in the scene by name so we can still rotate new shapes
        if (rotationGizmo == null)
        {
            GameObject gizmoObj = GameObject.Find("rotation gizmo");
            if (gizmoObj != null)
            {
                rotationGizmo = gizmoObj.GetComponent<MouseRotateTarget>();
            }
            else
            {
                Debug.LogWarning("Rotation gizmo not found in the scene.");
            }
        }

        Transform clearSphere = rotator != null ? rotator.Find("ClearSphere") : null;
        if (clearSphere != null)
        {
            Destroy(clearSphere.gameObject);
        }

        ShowShape(currentIndex);

        // Setup the NextButton to call ShowNextShape when clicked
        SetupNextButton();

        // Create the text container and close button if they don't already exist
        SetupTextCard();
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
            if (rotationGizmo != null)
            {
                rotationGizmo.SetTarget(currentShape.transform);
            }
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

    /// <summary>
    /// Finds or creates the UI button named "NextButton" and
    /// wires it up to trigger <see cref="ShowNextShape"/> when pressed.
    /// </summary>
    void SetupNextButton()
    {
        Button nextButton = null;

        // Try to find an existing button in the scene
        GameObject existing = GameObject.Find("NextButton");
        if (existing != null)
        {
            nextButton = existing.GetComponent<Button>();
        }

        // Create a new button if one does not already exist
        if (nextButton == null)
        {
            // Find or create a canvas to hold the UI
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            GameObject btnGO = new GameObject("NextButton", typeof(RectTransform), typeof(Button), typeof(Image));
            btnGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 30f);
            rt.sizeDelta = new Vector2(160f, 30f);

            // Add a simple text label
            GameObject textGO = new GameObject("Text", typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI label = textGO.GetComponent<TextMeshProUGUI>();
            label.text = "Next";
            label.alignment = TextAlignmentOptions.Center;

            nextButton = btnGO.GetComponent<Button>();
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextShape);
        }
        else
        {
            Debug.LogWarning("NextButton could not be found or created.");
        }
    }

    /// <summary>
    /// Ensures a TextCard container exists under "ShapeCard/Panel".
    /// Adds a small close button that hides the container when clicked.
    /// </summary>
    void SetupTextCard()
    {
        if (cardText != null)
            return; // already set up in the inspector

        // locate the base panel that already exists in the scene
        GameObject panelGO = GameObject.Find("ShapeCard/Panel");
        if (panelGO == null)
        {
            Debug.LogWarning("Panel 'ShapeCard/Panel' not found for TextCard setup.");
            return;
        }

        // create TextCard container
        GameObject textCardGO = new GameObject("TextCard", typeof(RectTransform));
        textCardGO.transform.SetParent(panelGO.transform, false);

        RectTransform cardRT = textCardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = Vector2.zero;
        cardRT.anchorMax = Vector2.one;
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;

        // create the TMP text element
        GameObject txtGO = new GameObject("Description", typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(textCardGO.transform, false);
        cardText = txtGO.GetComponent<TextMeshProUGUI>();
        cardText.alignment = TextAlignmentOptions.Center;
        cardText.enableAutoSizing = true;

        // create close button anchored bottom right
        GameObject btnGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Button), typeof(Image));
        btnGO.transform.SetParent(textCardGO.transform, false);
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1f, 0f);
        btnRT.anchorMax = new Vector2(1f, 0f);
        btnRT.pivot = new Vector2(1f, 0f);
        btnRT.anchoredPosition = new Vector2(-5f, 5f);
        btnRT.sizeDelta = new Vector2(20f, 20f);

        // style button with a circular sprite and X label
        Image btnImage = btnGO.GetComponent<Image>();
        btnImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        btnImage.type = Image.Type.Simple;

        GameObject xText = new GameObject("Label", typeof(TextMeshProUGUI));
        xText.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI xLabel = xText.GetComponent<TextMeshProUGUI>();
        xLabel.text = "X";
        xLabel.alignment = TextAlignmentOptions.Center;
        xLabel.fontSize = 18f;

        Button closeButton = btnGO.GetComponent<Button>();
        closeButton.onClick.AddListener(() => textCardGO.SetActive(false));
    }
}
