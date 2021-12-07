using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLogic : MonoBehaviour
{
    private const string PP_LEVEL = "lvl";
    private const string PP_TUTSCENE = "tscn";

    private void Start()
    {
        if (PlayerPrefs.HasKey(PP_LEVEL))
        {
            CommonData.Instance.levelSceneIndex = PlayerPrefs.GetInt("lvl");

            SceneManager.LoadSceneAsync(CommonData.Instance.levelSceneIndex, LoadSceneMode.Additive).completed +=
            (asyncOperation) =>
            {
                InGameLogic.Instance.SceneLoadingComplete();
            };
        }
        else
        {
            //CommonData.Instance.levelSceneIndex = 1;
            //PlayerPrefs.SetInt("lvl", CommonData.Instance.levelSceneIndex);
            PlayerPrefs.SetInt(PP_TUTSCENE, 1);
            CommonData.Instance.tutorialScene = true;
            CommonData.Instance.levelSceneIndex = CommonData.Instance.TUTORIALSCENEINDEX;
            SceneManager.LoadSceneAsync(CommonData.Instance.TUTORIALSCENEINDEX, LoadSceneMode.Additive).completed +=
            (asyncOperation) =>
            {
                InGameLogic.Instance.SceneLoadingComplete();
            };
        }
    }
}
