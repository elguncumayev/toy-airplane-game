using UnityEngine;

public class ScaleAnim : MonoBehaviour
{
    [SerializeField] private Vector3 minScale;
    [SerializeField] private Vector3 maxScale;
    [SerializeField] private float scaleSpeed;
    [SerializeField] private RectTransform animObject;

    private bool grow = true;

    private void FixedUpdate()
    {
        if (grow)
        {
            animObject.localScale = new Vector3(animObject.localScale.x + scaleSpeed, animObject.localScale.y + scaleSpeed, 1);
            if (animObject.localScale.magnitude >= maxScale.magnitude) grow = false;
        }
        else
        {
            animObject.localScale = new Vector3(animObject.localScale.x - scaleSpeed*2, animObject.localScale.y - scaleSpeed*2, 1);
            if (animObject.localScale.magnitude <= minScale.magnitude) grow = true;
        }
    }
}
