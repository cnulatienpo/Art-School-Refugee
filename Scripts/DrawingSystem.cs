using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a simple drawing interface overlaying the existing
/// ShapeCard/Panel. The system supports multiple layers, basic
/// color selection and adjustable brush width. Designed so
/// additional functionality like undo/redo can be added later.
/// </summary>
public class DrawingSystem : MonoBehaviour
{
    /// <summary>Stores a single stroke drawn by the player.</summary>
    [Serializable]
    class Stroke
    {
        public List<Vector2> points = new List<Vector2>();
        public Color color;
        public int width;
        public int layerIndex;
        public int order;
    }

    /// <summary>Represents a single drawable layer.</summary>
    class Layer
    {
        public Texture2D texture;
        public RawImage image;
        public Toggle visibility;
        public List<Stroke> strokes = new List<Stroke>();
    }

    readonly List<Layer> layers = new List<Layer>();
    int activeLayer = -1;

    Canvas canvas;
    RectTransform drawingArea;
    RectTransform layerPanel;

    Color currentColor = Color.black;
    int brushSize = 5;

    Vector2? lastPos;
    Stroke currentStroke;
    int strokeOrder;

    void Start()
    {
        SetupCanvas();
        if (canvas != null)
            AddLayer();
    }

    // Creates the overlay canvas and basic UI.
    void SetupCanvas()
    {
        GameObject panelGO = GameObject.Find("ShapeCard/Panel");
        if (panelGO == null)
        {
            Debug.LogWarning("Panel 'ShapeCard/Panel' not found for drawing.");
            return;
        }

        GameObject canvasGO = new GameObject(
            "DrawingCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(panelGO.transform, false);

        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1;

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // area that receives drawing
        GameObject areaGO = new GameObject("DrawingArea", typeof(RectTransform));
        areaGO.transform.SetParent(canvasGO.transform, false);
        drawingArea = areaGO.GetComponent<RectTransform>();
        drawingArea.anchorMin = Vector2.zero;
        drawingArea.anchorMax = Vector2.one;
        drawingArea.offsetMin = Vector2.zero;
        drawingArea.offsetMax = Vector2.zero;

        CreateToolbar(canvasGO.transform);
        CreateLayerPanel(canvasGO.transform);
    }

    // Builds the top toolbar with color buttons and width slider.
    void CreateToolbar(Transform parent)
    {
        GameObject barGO = new GameObject(
            "Toolbar",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        barGO.transform.SetParent(parent, false);

        RectTransform barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 1f);
        barRT.anchorMax = new Vector2(1f, 1f);
        barRT.pivot = new Vector2(0.5f, 1f);
        barRT.sizeDelta = new Vector2(0f, 40f);

        HorizontalLayoutGroup layout = barGO.GetComponent<HorizontalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 5f;

        // preset color buttons
        CreateColorButton(barGO.transform, Color.black);
        CreateColorButton(barGO.transform, Color.red);
        CreateColorButton(barGO.transform, Color.green);
        CreateColorButton(barGO.transform, Color.blue);

        // brush width slider
        GameObject widthGO = new GameObject(
            "WidthSlider",
            typeof(RectTransform),
            typeof(Slider));
        widthGO.transform.SetParent(barGO.transform, false);
        RectTransform widthRT = widthGO.GetComponent<RectTransform>();
        widthRT.sizeDelta = new Vector2(100f, 20f);

        Slider slider = widthGO.GetComponent<Slider>();
        slider.minValue = 1;
        slider.maxValue = 20;
        slider.value = brushSize;
        slider.onValueChanged.AddListener(v => brushSize = Mathf.RoundToInt(v));
    }

    // Builds the panel holding layer visibility toggles and add button.
    void CreateLayerPanel(Transform parent)
    {
        GameObject panelGO = new GameObject(
            "Layers",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        panelGO.transform.SetParent(parent, false);

        layerPanel = panelGO.GetComponent<RectTransform>();
        layerPanel.anchorMin = new Vector2(1f, 0f);
        layerPanel.anchorMax = new Vector2(1f, 1f);
        layerPanel.pivot = new Vector2(1f, 0.5f);
        layerPanel.sizeDelta = new Vector2(40f, 0f);

        VerticalLayoutGroup layout = panelGO.GetComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 5f;

        // button to create new layers
        GameObject addGO = new GameObject(
            "AddLayer",
            typeof(RectTransform),
            typeof(Button),
            typeof(Image));
        addGO.transform.SetParent(panelGO.transform, false);

        RectTransform addRT = addGO.GetComponent<RectTransform>();
        addRT.sizeDelta = new Vector2(30f, 30f);

        TextMeshProUGUI label = CreateTMP(addGO.transform, "+");
        label.alignment = TextAlignmentOptions.Center;

        Button btn = addGO.GetComponent<Button>();
        btn.onClick.AddListener(AddLayer);
    }

    // Creates a single preset color selection button.
    void CreateColorButton(Transform parent, Color color)
    {
        GameObject btnGO = new GameObject(
            "Color",
            typeof(RectTransform),
            typeof(Button),
            typeof(Image));
        btnGO.transform.SetParent(parent, false);

        Image img = btnGO.GetComponent<Image>();
        img.color = color;

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30f, 30f);

        Button btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() => currentColor = color);
    }

    // Utility to create TMP text under a transform.
    TextMeshProUGUI CreateTMP(Transform parent, string text)
    {
        GameObject go = new GameObject("Text", typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.enableAutoSizing = true;
        return tmp;
    }

    // Adds a new drawing layer.
    void AddLayer()
    {
        if (drawingArea == null)
            return;

        GameObject imgGO = new GameObject(
            "Layer" + layers.Count,
            typeof(RectTransform),
            typeof(RawImage));
        imgGO.transform.SetParent(drawingArea, false);

        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        ClearTexture(tex, new Color(0, 0, 0, 0));

        RawImage img = imgGO.GetComponent<RawImage>();
        img.texture = tex;

        // visibility toggle
        GameObject toggleGO = new GameObject(
            "Toggle",
            typeof(RectTransform),
            typeof(Toggle),
            typeof(Image));
        toggleGO.transform.SetParent(layerPanel, false);

        RectTransform togRT = toggleGO.GetComponent<RectTransform>();
        togRT.sizeDelta = new Vector2(30f, 30f);

        Toggle toggle = toggleGO.GetComponent<Toggle>();
        toggle.isOn = true;
        toggle.onValueChanged.AddListener(v => imgGO.SetActive(v));

        TextMeshProUGUI label = CreateTMP(toggleGO.transform, "\uE8A6"); // eye icon character
        label.alignment = TextAlignmentOptions.Center;

        Layer layer = new Layer
        {
            texture = tex,
            image = img,
            visibility = toggle,
            strokes = new List<Stroke>()
        };
        layers.Add(layer);
        activeLayer = layers.Count - 1;
    }

    void ClearTexture(Texture2D tex, Color c)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = c;
        tex.SetPixels(pixels);
        tex.Apply();
    }

    void Update()
    {
        if (canvas == null || activeLayer < 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            lastPos = GetTextureCoord(Input.mousePosition);
            currentStroke = new Stroke
            {
                color = currentColor,
                width = brushSize,
                layerIndex = activeLayer,
                order = strokeOrder++
            };
            currentStroke.points.Add(lastPos.Value);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentStroke != null)
            {
                Vector2 pos = GetTextureCoord(Input.mousePosition);
                currentStroke.points.Add(pos);
                layers[activeLayer].strokes.Add(currentStroke);
                currentStroke = null;
            }
            lastPos = null;
        }
        else if (Input.GetMouseButton(0) && lastPos.HasValue)
        {
            Vector2 pos = GetTextureCoord(Input.mousePosition);
            DrawLine(layers[activeLayer].texture, lastPos.Value, pos, currentColor, brushSize);
            lastPos = pos;
            if (currentStroke != null)
                currentStroke.points.Add(pos);
        }
    }

    Vector2 GetTextureCoord(Vector2 screen)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingArea, screen, null, out local);
        Rect rect = drawingArea.rect;
        float x = (local.x - rect.xMin) / rect.width;
        float y = (local.y - rect.yMin) / rect.height;
        return new Vector2(
            Mathf.Clamp01(x) * layers[activeLayer].texture.width,
            Mathf.Clamp01(y) * layers[activeLayer].texture.height);
    }

    // Draws a line between two points onto the texture.
    void DrawLine(Texture2D tex, Vector2 a, Vector2 b, Color col, int width)
    {
        int x0 = Mathf.RoundToInt(a.x);
        int y0 = Mathf.RoundToInt(a.y);
        int x1 = Mathf.RoundToInt(b.x);
        int y1 = Mathf.RoundToInt(b.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawBrush(tex, x0, y0, col, width);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        tex.Apply();
    }

    void DrawBrush(Texture2D tex, int cx, int cy, Color col, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;
                int px = cx + x;
                int py = cy + y;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, col);
            }
        }
    }

    /// <summary>
    /// Writes all recorded strokes to a JSON file at <paramref name="path"/>.
    /// </summary>
    public void SaveStrokesToFile(string path)
    {
        StrokeSaveData save = new StrokeSaveData();
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].strokes.Count == 0)
                continue;

            LayerSaveData data = new LayerSaveData
            {
                layerIndex = i,
                strokes = layers[i].strokes
            };
            save.layers.Add(data);
        }

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(path, json);
    }

    [Serializable]
    class LayerSaveData
    {
        public int layerIndex;
        public List<Stroke> strokes = new List<Stroke>();
    }

    [Serializable]
    class StrokeSaveData
    {
        public List<LayerSaveData> layers = new List<LayerSaveData>();
    }
}
