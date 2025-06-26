using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MouseRotateTarget : MonoBehaviour
{
    public float rotationSpeed = 100f;

    private bool isSelected = false;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged 'MainCamera'.");
        }
    }

    void Update()
    {
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

        if (isSelected && Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(mainCam.transform.up, -rotX, Space.World);
            transform.Rotate(mainCam.transform.right, rotY, Space.World);
        }
    }
}
