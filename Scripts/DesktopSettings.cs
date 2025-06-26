using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Runtime helper that configures desktop specific settings such as
/// canvas scaling and stylus fallback behaviour.
/// </summary>
public class DesktopSettings : MonoBehaviour
{
    void Start()
    {
        ApplyCanvasScaling();
    }

    /// <summary>
    /// Adjust all CanvasScaler components so UI fits the current resolution.
    /// </summary>
    void ApplyCanvasScaling()
    {
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (var scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Screen.currentResolution.width,
                                                    Screen.currentResolution.height);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    /// <summary>
    /// Returns pressure value from stylus if available, otherwise use mouse.
    /// </summary>
    public float GetPressure()
    {
#if ENABLE_INPUT_SYSTEM
        if (Pen.current != null)
            return Pen.current.pressure.ReadValue();
        if (Pointer.current != null)
            return Pointer.current.pressure.ReadValue();
#endif
        return Input.GetMouseButton(0) ? 1f : 0f;
    }

    /// <summary>
    /// Returns stylus tilt if available, approximated from mouse when missing.
    /// </summary>
    public Vector2 GetTilt()
    {
#if ENABLE_INPUT_SYSTEM
        if (Pen.current != null)
            return Pen.current.tilt.ReadValue();
#endif
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 delta = (Vector2)Input.mousePosition - center;
        return delta.normalized;
    }
}
