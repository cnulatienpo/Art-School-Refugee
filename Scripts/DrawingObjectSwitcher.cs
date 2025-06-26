using UnityEngine;
using TMPro;

/// <summary>
/// Simple helper that swaps drawing objects without reloading the scene.
/// </summary>
public class DrawingObjectSwitcher : MonoBehaviour
{
    public GameObject currentObject;
    public Transform spawnPoint;
    public TextMeshProUGUI objectLabel;

    /// <summary>
    /// Replaces the current drawing object with <paramref name="nextPrefab"/>.
    /// </summary>
    public void SwitchToNextObject(GameObject nextPrefab, string labelText)
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        ClearCanvas();

        if (nextPrefab != null && spawnPoint != null)
        {
            currentObject = Instantiate(nextPrefab, spawnPoint.position,
                                        spawnPoint.rotation, spawnPoint);
        }

        if (objectLabel != null)
        {
            objectLabel.text = labelText;
        }
    }
}
