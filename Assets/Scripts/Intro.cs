using UnityEngine;

public class Intro : MonoBehaviour
{
    void Start()
    {
        CommonData.Instance.gameStartTime = Time.time;
    }
}
