// Example usage:
// Create a UI panel with a HorizontalLayoutGroup. Add a Button for each level
// and a Button with an Image for the sketchbook at the end. Assign all of
// those buttons to the respective lists in the inspector. Connect the
// OnLevelButtonClicked and OnSketchbookClicked events to their buttons.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a row of level buttons and a sketchbook icon. Colors and
/// interactivity are updated based on the current level and the maximum
/// unlocked level.
/// </summary>
public class LevelNavBar : MonoBehaviour
{
    [Tooltip("Button for each level in order")]
    public List<Button> levelButtons = new List<Button>();

    [Header("Sketchbook")]
    public Button sketchbookButton;
    public Image sketchbookImage;
    public Sprite sketchbookActive;
    public Sprite sketchbookLocked;

    [Header("Progress")]
    public int currentLevel = 1;
    public int maxUnlockedLevel = 1;

    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Refreshes button colors and interactivity based on progress values.
    /// </summary>
    public void UpdateUI()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            int levelNumber = i + 1;
            Button btn = levelButtons[i];
            if (btn == null)
                continue;

            bool unlocked = levelNumber <= maxUnlockedLevel;
            bool current = levelNumber == currentLevel;

            btn.interactable = unlocked;

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (current)
                    img.color = Color.black;
                else if (unlocked)
                    img.color = Color.white;
                else
                    img.color = Color.gray;
            }
        }

        if (sketchbookButton != null && sketchbookImage != null)
        {
            bool unlocked = maxUnlockedLevel > 1;
            sketchbookButton.interactable = unlocked;
            sketchbookImage.sprite = unlocked ? sketchbookActive : sketchbookLocked;
            sketchbookImage.color = unlocked ? Color.white : Color.gray;
        }
    }

    /// <summary>
    /// Placeholder event for clicking a level button.
    /// </summary>
    public void OnLevelButtonClicked(int level)
    {
        Debug.Log($"Load level {level}");
    }

    /// <summary>
    /// Placeholder event for opening the Mess Hall from the sketchbook icon.
    /// </summary>
    public void OnSketchbookClicked()
    {
        Debug.Log("Open Mess Hall");
    }
}
