using UnityEngine;

public class StylusGizmoClick : MonoBehaviour
{
    // Optional: You can assign a specific object if needed
    public GameObject gizmoTarget;

    void Update()
    {
        // Works for mouse AND stylus (like Wacom)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if it hit the gizmo
                if (gizmoTarget == null || hit.collider.gameObject == gizmoTarget)
                {
                    Debug.Log("🎯 Gizmo tapped at: " + hit.point);

                    // 💥 DO YOUR GIZMO ACTION HERE
                    // For example, rotate it:
                    hit.collider.transform.Rotate(Vector3.up, 45f);
                }
                else
                {
                    Debug.Log("Hit something else: " + hit.collider.name);
                }
            }
        }
    }
}
