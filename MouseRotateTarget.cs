using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MouseRotateTarget : MonoBehaviour
{
    public float rotationSpeed = 100f;
    // Parent that contains the current shape prefab
    public Transform rotator;

    private bool isSelected = false;
    private Camera mainCam;
    // The object we actually rotate instead of the gizmo itself
    private Transform target;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged 'MainCamera'.");
        }
        UpdateTargetFromRotator();
    }

    void Update()
    {
        UpdateTargetFromRotator();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                isSelected = (hit.transform == transform);
            }
            else
            {
                isSelected = false;
            }
        }

        if (isSelected && Input.GetMouseButton(0) && target != null)
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Rotate the target object, keeping the gizmo itself fixed
            target.Rotate(mainCam.transform.up, -rotX, Space.World);
            target.Rotate(mainCam.transform.right, rotY, Space.World);
        }
    }

    /// <summary>
    /// Explicitly sets the object this gizmo controls.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Keep track of the current object under the rotator, if available
    void UpdateTargetFromRotator()
    {
        if (rotator != null && rotator.childCount > 0)
        {
            Transform newTarget = rotator.GetChild(0);
            if (newTarget != target)
            {
                target = newTarget;
            }
        }
        else if (rotator != null && rotator.childCount == 0)
        {
            target = null;
        }
    }
}
