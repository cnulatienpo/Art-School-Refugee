using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed = 30f;

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}

