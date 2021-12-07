using UnityEngine;

public class DroneBehavior : MonoBehaviour
{
    [SerializeField] float speed = 20;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up * speed);
    }
}
