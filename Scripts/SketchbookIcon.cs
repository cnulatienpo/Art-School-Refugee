using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clicking this icon opens the Mess Hall free drawing mode.
/// 
/// Setup Instructions:
/// - Attach this script to the sketchbook icon GameObject.
/// - Assign the MessHallManager reference in the Inspector.
/// - Ensure the GameObject also has a Button component.
/// </summary>
public class SketchbookIcon : MonoBehaviour
{
    [Tooltip("Manager controlling the Mess Hall UI")] 
    public MessHallManager messHallManager;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClicked);
        else
            Debug.LogWarning($"{name} is missing a Button component.");
    }

    void OnClicked()
    {
        if (messHallManager != null)
            messHallManager.EnterMessHall();
        else
            Debug.LogWarning("MessHallManager reference not set.");
    }
}
