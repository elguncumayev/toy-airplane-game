using UnityEngine;

public class CoinNitroRotate : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 4);
    }
}
