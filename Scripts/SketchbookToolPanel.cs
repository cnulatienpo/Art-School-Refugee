using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI panel for selecting drawing tools in the Mess Hall sketchbook mode.
///
/// Setup notes:
/// - Create a panel with:
///   * Dropdown "brushDropdown" listing Pencil, Ink, Airbrush and Eraser.
///   * Slider "sizeSlider" for brush size.
///   * Slider "opacitySlider" for brush opacity.
///   * Color picker "brushColorPicker" to change the brush color.
///   * Color picker "canvasColorPicker" to change the canvas/background color.
///   * Toggle "symmetryToggle" for vertical symmetry.
///   * Dropdown "layerDropdown" with options Form, Ink and Light.
/// - Assign this script to the panel GameObject and wire up the UI references.
/// - Provide three drawing layers (Form, Ink, Light) using separate RenderTextures
///   or GameObjects. Assign them to the fields below.
/// - Use the selected values with your drawing logic. LineRenderer, RenderTexture
///   or a custom shader can be used to render brush strokes with the chosen
///   parameters.
/// </summary>
public class SketchbookToolPanel : MonoBehaviour
{
    public enum SketchTool { Pencil, Ink, Airbrush, Eraser }

    [Header("UI References")]
    public Dropdown brushDropdown;
    public Slider sizeSlider;
    public Slider opacitySlider;
    public Image brushColorPicker;
    public Image canvasColorPicker;
    public Toggle symmetryToggle;
    public Dropdown layerDropdown;

    [Header("Drawing Layers")]
    public GameObject formLayer;
    public GameObject inkLayer;
    public GameObject lightLayer;

    public SketchTool currentTool = SketchTool.Pencil;
    public float brushSize = 10f;
    public float brushOpacity = 1f;
    public Color brushColor = Color.black;
    public Color canvasColor = Color.white;
    public bool mirrorSymmetry;
    public int currentLayerIndex;

    void Start()
    {
        InitDropdowns();
        ApplyCanvasColor(canvasColor);
    }

    void InitDropdowns()
    {
        if (brushDropdown != null)
        {
            brushDropdown.ClearOptions();
            brushDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Pencil",
                "Ink",
                "Airbrush",
                "Eraser"
            });
            brushDropdown.onValueChanged.AddListener(OnBrushChanged);
            brushDropdown.value = (int)currentTool;
        }

        if (layerDropdown != null)
        {
            layerDropdown.ClearOptions();
            layerDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Form",
                "Ink",
                "Light"
            });
            layerDropdown.onValueChanged.AddListener(OnLayerChanged);
            layerDropdown.value = currentLayerIndex;
        }

        if (sizeSlider != null)
            sizeSlider.onValueChanged.AddListener(v => brushSize = v);
        if (opacitySlider != null)
            opacitySlider.onValueChanged.AddListener(v => brushOpacity = v);
        if (symmetryToggle != null)
            symmetryToggle.onValueChanged.AddListener(v => mirrorSymmetry = v);
    }

    public void OnBrushChanged(int index)
    {
        currentTool = (SketchTool)index;
    }

    public void OnLayerChanged(int index)
    {
        currentLayerIndex = index;
        if (formLayer != null) formLayer.SetActive(index == 0);
        if (inkLayer != null) inkLayer.SetActive(index == 1);
        if (lightLayer != null) lightLayer.SetActive(index == 2);
    }

    public void SetBrushColor(Color color)
    {
        brushColor = color;
        if (brushColorPicker != null)
            brushColorPicker.color = color;
    }

    public void SetCanvasColor(Color color)
    {
        canvasColor = color;
        ApplyCanvasColor(color);
    }

    void ApplyCanvasColor(Color color)
    {
        if (canvasColorPicker != null)
            canvasColorPicker.color = color;
        if (formLayer != null)
        {
            Image img = formLayer.GetComponent<Image>();
            if (img != null)
                img.color = color;
        }
    }
}
