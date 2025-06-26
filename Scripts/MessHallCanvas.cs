using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Mess Hall drawing surface. A vertical scrollable area acts like
/// a roll of butcher paper. Users can extend the paper with the "New Sheet"
/// button and draw anywhere within the ScrollRect.
/// </summary>
public class MessHallCanvas : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;          // Scroll area for the paper
    public RectTransform content;          // Content inside the scroll rect
    public Image sheetPrefab;              // Prefab used for each sheet background
    public Button newSheetButton;          // Adds a new sheet

    [Header("Drawing Settings")]
    public int sheetPixelHeight = 2048;    // Height in pixels of each sheet
    public int canvasWidth = 2048;         // Width of the drawing texture
    public Color penColor = Color.black;
    public int penSize = 5;

    Texture2D drawingTexture;              // Single texture covering all sheets
    RawImage drawingImage;                 // UI element displaying the texture
    RectTransform drawingRect;

    Vector2? lastPos;                      // Last recorded pen position

    [Header("Session Tracking")]
    public MessHallSessionTracker sessionTracker;

    void Awake()
    {
        if (newSheetButton != null)
            newSheetButton.onClick.AddListener(AddSheet);
    }

    void Start()
    {
        // Ensure there is at least one sheet
        if (content.childCount == 0)
            AddSheet();
        else
            SetupDrawingLayer();
    }

    void SetupDrawingLayer()
    {
        if (drawingImage != null)
            return;

        GameObject go = new GameObject("DrawingLayer", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(content, false);
        go.transform.SetAsLastSibling();

        drawingRect = go.GetComponent<RectTransform>();
        drawingRect.anchorMin = new Vector2(0f, 1f);
        drawingRect.anchorMax = new Vector2(1f, 1f);
        drawingRect.pivot = new Vector2(0.5f, 1f);
        drawingRect.anchoredPosition = Vector2.zero;
        drawingRect.sizeDelta = new Vector2(0f, sheetPixelHeight);

        drawingTexture = new Texture2D(canvasWidth, sheetPixelHeight, TextureFormat.RGBA32, false);
        ClearTexture(drawingTexture, Color.clear);

        drawingImage = go.GetComponent<RawImage>();
        drawingImage.texture = drawingTexture;
    }

    // Adds a new sheet of paper to the bottom of the content.
    public void AddSheet()
    {
        if (sheetPrefab == null || content == null)
            return;

        Image sheet = Instantiate(sheetPrefab, content);
        sheet.rectTransform.anchorMin = new Vector2(0f, 1f);
        sheet.rectTransform.anchorMax = new Vector2(1f, 1f);
        sheet.rectTransform.pivot = new Vector2(0.5f, 1f);
        sheet.rectTransform.anchoredPosition = new Vector2(0f, -GetCurrentHeight());
        sheet.rectTransform.sizeDelta = new Vector2(0f, sheetPixelHeight);

        float newHeight = GetCurrentHeight() + sheetPixelHeight;
        content.sizeDelta = new Vector2(content.sizeDelta.x, newHeight);

        if (drawingImage == null)
            SetupDrawingLayer();
        else
            ExtendDrawingTexture((int)newHeight);
    }

    // Returns the current height of the content rect.
    float GetCurrentHeight()
    {
        return content.sizeDelta.y;
    }

    // Enlarges the drawing texture while preserving existing pixels.
    void ExtendDrawingTexture(int newHeight)
    {
        if (newHeight <= drawingTexture.height)
            return;

        Texture2D newTex = new Texture2D(canvasWidth, newHeight, TextureFormat.RGBA32, false);
        ClearTexture(newTex, Color.clear);
        Color[] oldPixels = drawingTexture.GetPixels();
        newTex.SetPixels(0, newHeight - drawingTexture.height, canvasWidth, drawingTexture.height, oldPixels);
        newTex.Apply();
        drawingTexture = newTex;
        drawingImage.texture = drawingTexture;
        drawingRect.sizeDelta = new Vector2(0f, newHeight);
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
        if (drawingTexture == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            lastPos = GetTextureCoord(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && lastPos.HasValue)
        {
            Vector2 pos = GetTextureCoord(Input.mousePosition);
            DrawLine(drawingTexture, lastPos.Value, pos, penColor, penSize);
            lastPos = pos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastPos = null;
        }
    }

    public void FlipCanvas()
    {
        if (content != null)
        {
            Vector3 scale = content.localScale;
            scale.x *= -1f;
            content.localScale = scale;
        }
        if (sessionTracker != null)
            sessionTracker.RegisterCanvasFlip();
    }

    public void SetZoom(float zoom)
    {
        if (content != null)
            content.localScale = Vector3.one * zoom;
        if (sessionTracker != null)
            sessionTracker.LogZoom(zoom);
    }

    public void Undo()
    {
        // Actual undo logic would go here
        if (sessionTracker != null)
            sessionTracker.RegisterUndo();
    }

    Vector2 GetTextureCoord(Vector2 screen)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, screen, null, out local);
        float x = (local.x + content.sizeDelta.x * 0.5f) / content.sizeDelta.x;
        float y = 1f - ((local.y + content.sizeDelta.y * 0.5f) / content.sizeDelta.y);
        x = Mathf.Clamp01(x) * drawingTexture.width;
        y = Mathf.Clamp01(y) * drawingTexture.height;
        return new Vector2(x, y);
    }

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
    /// Saves the entire drawing area as a PNG file. For PDF export, combine the
    /// PNG with a PDF library of your choice.
    /// </summary>
    public void ExportImage(string path)
    {
        if (drawingTexture == null)
            return;
        byte[] png = drawingTexture.EncodeToPNG();
        File.WriteAllBytes(path, png);
    }
}
