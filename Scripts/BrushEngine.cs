using UnityEngine;

/// <summary>
/// Renders brush strokes to a texture using the current tool settings.
/// The SketchbookInput script should call <see cref="Draw"/> while dragging
/// the pointer across the canvas.
/// </summary>
public class BrushEngine : MonoBehaviour
{
    [Tooltip("Panel providing brush parameters")] public SketchbookToolPanel toolPanel;
    [Tooltip("Active texture that receives drawing")] public Texture2D activeTexture;

    Vector2? lastPos;
    float lastTime;

    /// <summary>Assigns the layer texture to draw on.</summary>
    public void SetActiveTexture(Texture2D tex) => activeTexture = tex;

    /// <summary>Reset cached stroke state at the start of a new drag.</summary>
    public void BeginStroke()
    {
        lastPos = null;
        lastTime = Time.time;
    }

    /// <summary>
    /// Draw at the provided position using the current brush settings.
    /// When <paramref name="mirrored"/> is true the stroke is also drawn on
    /// the opposite side of the texture.
    /// </summary>
    public void Draw(Vector2 position, bool mirrored)
    {
        if (activeTexture == null || toolPanel == null)
            return;

        float speed = 0f;
        if (lastPos.HasValue)
        {
            float dist = Vector2.Distance(position, lastPos.Value);
            float dt = Time.time - lastTime;
            if (dt > 0f) speed = dist / dt;
        }

        switch (toolPanel.currentTool)
        {
            case SketchbookToolPanel.SketchTool.Pencil:
                DrawPencil(position, speed);
                if (mirrored)
                    DrawPencil(MirrorPoint(position, activeTexture.width), speed);
                break;
            case SketchbookToolPanel.SketchTool.Ink:
                DrawInk(position);
                if (mirrored)
                    DrawInk(MirrorPoint(position, activeTexture.width));
                break;
            case SketchbookToolPanel.SketchTool.Airbrush:
                DrawAirbrush(position);
                if (mirrored)
                    DrawAirbrush(MirrorPoint(position, activeTexture.width));
                break;
            case SketchbookToolPanel.SketchTool.Eraser:
                DrawEraser(position);
                if (mirrored)
                    DrawEraser(MirrorPoint(position, activeTexture.width));
                break;
        }

        activeTexture.Apply();
        lastPos = position;
        lastTime = Time.time;
    }

    #region Brush Implementations

    void DrawPencil(Vector2 pos, float speed)
    {
        Color c = toolPanel.brushColor;
        float fade = Mathf.Clamp01(speed / 3000f);     // faster = lighter
        c.a = toolPanel.brushOpacity * (1f - fade * 0.5f);
        int radius = Mathf.RoundToInt(toolPanel.brushSize);

        if (lastPos.HasValue)
            DrawLine(lastPos.Value, pos, c, radius, true);
        else
            DrawCircle(pos, radius, c, true);
    }

    void DrawInk(Vector2 pos)
    {
        Color c = toolPanel.brushColor;
        c.a = toolPanel.brushOpacity;
        int radius = Mathf.RoundToInt(toolPanel.brushSize);

        if (lastPos.HasValue)
            DrawLine(lastPos.Value, pos, c, radius, false);
        else
            DrawCircle(pos, radius, c, false);
    }

    void DrawAirbrush(Vector2 pos)
    {
        Color c = toolPanel.brushColor;
        c.a = toolPanel.brushOpacity * Time.deltaTime * 10f; // builds up
        int count = Mathf.CeilToInt(toolPanel.brushSize * 4f);
        float radius = toolPanel.brushSize;

        for (int i = 0; i < count; i++)
        {
            Vector2 p = pos + Random.insideUnitCircle * radius;
            SetPixelBlend(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), c);
        }
    }

    void DrawEraser(Vector2 pos)
    {
        Color c = new Color(0f, 0f, 0f, 0f);
        int radius = Mathf.RoundToInt(toolPanel.brushSize);

        if (lastPos.HasValue)
            DrawLine(lastPos.Value, pos, c, radius, false);
        else
            DrawCircle(pos, radius, c, false);
    }

    #endregion

    #region Primitive Drawing

    void DrawLine(Vector2 a, Vector2 b, Color col, int radius, bool soft)
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
            DrawCircle(new Vector2(x0, y0), radius, col, soft);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    void DrawCircle(Vector2 center, int radius, Color col, bool soft)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                float sqr = x * x + y * y;
                if (sqr > radius * radius)
                    continue;
                int px = Mathf.RoundToInt(center.x) + x;
                int py = Mathf.RoundToInt(center.y) + y;
                if (px < 0 || px >= activeTexture.width || py < 0 || py >= activeTexture.height)
                    continue;
                if (soft)
                {
                    float dst = Mathf.Sqrt(sqr) / radius;
                    Color softCol = col;
                    softCol.a = Mathf.Lerp(col.a, 0f, dst);
                    SetPixelBlend(px, py, softCol);
                }
                else
                {
                    SetPixelBlend(px, py, col);
                }
            }
        }
    }

    void SetPixelBlend(int x, int y, Color col)
    {
        Color baseCol = activeTexture.GetPixel(x, y);
        Color blended = Color.Lerp(baseCol, col, col.a);
        activeTexture.SetPixel(x, y, blended);
    }

    #endregion

    Vector2 MirrorPoint(Vector2 p, int width) => new Vector2(width - p.x, p.y);
}
