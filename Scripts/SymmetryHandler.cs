using UnityEngine;

/// <summary>
/// Computes mirrored brush positions when vertical symmetry is enabled.
///
/// Setup notes:
/// - Add this component anywhere in your scene and assign <see cref="toolPanel"/>
///   so the handler knows whether symmetry is active.
/// - Convert pointer positions to the drawing area's local space using
///   RectTransformUtility.ScreenPointToLocalPointInRectangle before calling
///   <see cref="GetMirroredPosition"/>.
/// </summary>
public class SymmetryHandler : MonoBehaviour
{
    [Tooltip("Panel containing the mirrorSymmetry toggle")]
    public SketchbookToolPanel toolPanel;

    /// <summary>
    /// Returns the mirrored position across the vertical center of
    /// <paramref name="drawingArea"/> when symmetry is enabled.
    /// The input coordinates must use the same local space as the RectTransform.
    /// </summary>
    public Vector2 GetMirroredPosition(Vector2 originalPosition, RectTransform drawingArea)
    {
        if (toolPanel != null && toolPanel.mirrorSymmetry && drawingArea != null)
        {
            Rect rect = drawingArea.rect;
            float centerX = rect.xMin + rect.width * 0.5f;
            float offset = originalPosition.x - centerX;
            float mirroredX = centerX - offset;
            return new Vector2(mirroredX, originalPosition.y);
        }
        return originalPosition;
    }
}
