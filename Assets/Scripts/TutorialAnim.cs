using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAnim : MonoBehaviour
{
    #region Singleton
    private static TutorialAnim _instance;
    public static TutorialAnim Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    [SerializeField] private Image backImage;

    [SerializeField] private Animator animator;

    private bool isPlaying = false;

    private bool animCanEnd = false;

    public void PlayNextAnim(int ID)
    {
        isPlaying = true;
        if(ID == 0)
        {
            animator.Play("Right");
        }
        else if(ID == 1)
        {
            animator.Play("Left");
        }
        else if(ID == 2)
        {
            animator.Play("Nitro");
        }
        backImage.raycastTarget = true;
        StartCoroutine(Timer());
    }

    private void Update()
    {
        if (isPlaying && Input.touchCount > 0 && animCanEnd)
        {
            Debug.Log("!!!Touch Registered!!!");
            animator.SetTrigger("End");
            Time.timeScale = 1;
            isPlaying = false;
            animCanEnd = false;
        }
    }
    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(1f);
        backImage.raycastTarget = false;
        animCanEnd = true;
    }

}
