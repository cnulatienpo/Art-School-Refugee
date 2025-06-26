using UnityEngine;

public class LockCamera : MonoBehaviour
{
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private float lockedFOV;

    void Awake()
    {
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
        lockedFOV = GetComponent<Camera>().fieldOfView;
    }

    void Update()
    {
        LockEverything();
    }

    void LateUpdate()
    {
        LockEverything();
    }

    void FixedUpdate()
    {
        LockEverything();
    }

    void LockEverything()
    {
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;
        GetComponent<Camera>().fieldOfView = lockedFOV;
    }
}
