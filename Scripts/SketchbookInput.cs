using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles real-time drawing input for the Mess Hall sketchbook.
///
/// Setup notes:
/// - Attach this to a GameObject under the drawing canvas.
/// - Assign the <see cref="toolPanel"/> reference and each layer RawImage.
/// - Each layer should use a readable Texture2D so pixels can be modified.
/// </summary>
public class SketchbookInput : MonoBehaviour
{
    [Tooltip("UI panel providing current brush settings")]
    public SketchbookToolPanel toolPanel;

    [Header("Drawing Layers")]
    public RawImage formLayer;
    public RawImage inkLayer;
    public RawImage lightLayer;

    Texture2D formTex;
    Texture2D inkTex;
    Texture2D lightTex;
    RectTransform canvasRect;

    Vector2? lastPos;

    void Start()
    {
        if (formLayer != null)
            formTex = GetWritableTexture(formLayer);
        if (inkLayer != null)
            inkTex = GetWritableTexture(inkLayer);
        if (lightLayer != null)
            lightTex = GetWritableTexture(lightLayer);

        canvasRect = GetComponent<RectTransform>();
        if (canvasRect == null)
            canvasRect = formLayer != null ? formLayer.rectTransform : null;
    }

    Texture2D GetWritableTexture(RawImage img)
    {
        Texture2D tex = img.texture as Texture2D;
        if (tex == null && img.texture is RenderTexture rt)
        {
            tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            img.texture = tex;
        }
        return tex;
    }

    void Update()
    {
        if (toolPanel == null)
            return;

        Texture2D tex = GetActiveTexture();
        if (tex == null || canvasRect == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            lastPos = GetTextureCoord(Input.mousePosition, tex);
        }
        else if (Input.GetMouseButton(0) && lastPos.HasValue)
        {
            Vector2 pos = GetTextureCoord(Input.mousePosition, tex);
            Color col = toolPanel.brushColor;
            if (toolPanel.currentTool == SketchbookToolPanel.SketchTool.Eraser)
                col = new Color(0, 0, 0, 0);
            else
                col.a = toolPanel.brushOpacity;

            int size = Mathf.RoundToInt(toolPanel.brushSize);

            DrawLine(tex, lastPos.Value, pos, col, size);

            if (toolPanel.mirrorSymmetry)
            {
                Vector2 a = MirrorPoint(lastPos.Value, tex.width);
                Vector2 b = MirrorPoint(pos, tex.width);
                DrawLine(tex, a, b, col, size);
            }

            lastPos = pos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastPos = null;
        }
    }

    Texture2D GetActiveTexture()
    {
        switch (toolPanel.currentLayerIndex)
        {
            case 0: return formTex;
            case 1: return inkTex;
            case 2: return lightTex;
            default: return null;
        }
    }

    Vector2 GetTextureCoord(Vector2 screen, Texture2D tex)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out local);
        Rect rect = canvasRect.rect;
        float x = (local.x - rect.xMin) / rect.width;
        float y = (local.y - rect.yMin) / rect.height;
        return new Vector2(Mathf.Clamp01(x) * tex.width, Mathf.Clamp01(y) * tex.height);
    }

    Vector2 MirrorPoint(Vector2 p, int width)
    {
        return new Vector2(width - p.x, p.y);
    }

    void DrawLine(Texture2D tex, Vector2 a, Vector2 b, Color color, int radius)
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
            DrawBrush(tex, x0, y0, color, radius);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        tex.Apply();
    }

    void DrawBrush(Texture2D tex, int cx, int cy, Color color, int radius)
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
                {
                    if (color.a <= 0f)
                    {
                        tex.SetPixel(px, py, new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        Color baseCol = tex.GetPixel(px, py);
                        Color blended = Color.Lerp(baseCol, color, color.a);
                        tex.SetPixel(px, py, blended);
                    }
                }
            }
        }
    }
}

