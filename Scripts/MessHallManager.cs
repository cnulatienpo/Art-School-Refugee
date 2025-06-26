using UnityEngine;

/// <summary>
/// Controls showing and hiding the Mess Hall interface.
/// Activate the UI, prompt generator and drawing canvas when entered
/// and disable them when exiting.
/// </summary>
public class MessHallManager : MonoBehaviour
{
    [Header("References")]
    public GameObject messHallUI;   // Root canvas for the Mess Hall
    public GameObject promptBar;    // Prompt generator UI
    public GameObject scrollableCanvas; // Drawing surface with ScrollRect

    /// <summary>
    /// Enable the Mess Hall interface.
    /// </summary>
    public void EnterMessHall()
    {
        if (messHallUI != null)
            messHallUI.SetActive(true);
        if (promptBar != null)
            promptBar.SetActive(true);
        if (scrollableCanvas != null)
            scrollableCanvas.SetActive(true);
    }

    /// <summary>
    /// Disable the Mess Hall interface.
    /// </summary>
    public void ExitMessHall()
    {
        if (messHallUI != null)
            messHallUI.SetActive(false);
        if (promptBar != null)
            promptBar.SetActive(false);
        if (scrollableCanvas != null)
            scrollableCanvas.SetActive(false);
    }
}
