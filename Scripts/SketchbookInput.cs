using UnityEngine;

/// <summary>
/// Processes pointer input and forwards stroke positions to <see cref="BrushEngine"/>.
/// The class obtains brush settings from <see cref="SketchbookToolPanel"/>,
/// the current drawing texture from <see cref="LayerManager"/> and handles
/// optional symmetry using <see cref="SymmetryHandler"/>.
/// </summary>
public class SketchbookInput : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Panel holding the active tool and brush values")] public SketchbookToolPanel toolPanel;
    [Tooltip("Layer manager providing drawing surfaces")] public LayerManager layerManager;
    [Tooltip("Brush engine responsible for rendering strokes")] public BrushEngine brushEngine;
    [Tooltip("Optional symmetry helper")] public SymmetryHandler symmetryHandler;
    [Tooltip("RectTransform receiving pointer input")] public RectTransform drawingArea;

    Texture2D activeTexture;

    void Start()
    {
        if (toolPanel != null && layerManager != null)
            layerManager.SetActiveLayer(toolPanel.currentLayerIndex);

        UpdateActiveTexture();

        if (brushEngine != null)
        {
            brushEngine.toolPanel = toolPanel;
            brushEngine.SetActiveTexture(activeTexture);
        }

        if (drawingArea == null)
            drawingArea = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (brushEngine == null || toolPanel == null || layerManager == null || drawingArea == null)
            return;

        UpdateActiveTexture();

        if (Input.GetMouseButtonDown(0))
            brushEngine.BeginStroke();

        if (Input.GetMouseButton(0))
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, Input.mousePosition, null, out local);

            bool mirrored = false;
            if (symmetryHandler != null)
            {
                Vector2 mirroredPos = symmetryHandler.GetMirroredPosition(local, drawingArea);
                mirrored = mirroredPos != local;
            }

            Vector2 texPos = LocalToTexCoord(local, activeTexture);
            brushEngine.Draw(texPos, mirrored);
        }
    }

    void UpdateActiveTexture()
    {
        activeTexture = layerManager.GetActiveTexture() as Texture2D;
        if (brushEngine != null)
            brushEngine.SetActiveTexture(activeTexture);
    }

    Vector2 LocalToTexCoord(Vector2 local, Texture2D tex)
    {
        Rect rect = drawingArea.rect;
        float x = (local.x - rect.xMin) / rect.width;
        float y = (local.y - rect.yMin) / rect.height;
        return new Vector2(Mathf.Clamp01(x) * tex.width, Mathf.Clamp01(y) * tex.height);
    }
}
