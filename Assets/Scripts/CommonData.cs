using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonData : MonoBehaviour
{
    #region Singleton
    private static CommonData _instance;
    public static CommonData Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    public readonly int MAXLEVELINDEX = 9;
    public readonly int TUTORIALSCENEINDEX = 10;
    [HideInInspector] public bool tutorialScene = false;

    public GameObject mainMenuCanvas;
    public GameObject inGameMenuCanvas;

    public GameObject introCamera;
    public GameObject introCanvas;
    public GameObject loadingCanvas;
    public RectTransform loadingBlack;

    public GameObject winPanel;
    [HideInInspector] public int levelSceneIndex;

    public TMP_Text levelLeft;
    public TMP_Text levelRight;
    public GameObject levelInfoGO;
    public Slider levelInfoFill;

    //Buttons
    public GameObject shopButtonInMainMenu;
    public GameObject noAdsButtonInMainMenu;

    //InGameInfo
    [HideInInspector] public int levelInfo;
    public TMP_Text coinUI;
    public Slider nitroFill;
    public Button nitroButton;
    public TMP_Text coinEndScreen;

    public GameObject[] planes;
    public GameObject[] planesShopPositions;

    //Controller
    public bool controllerOn;
    public GameObject visibleController;
    public GameObject invisibleController;
    public Joystick visibleJoystick;
    public Joystick invisibleJoystick;


    [HideInInspector] public int coinInt;
    public int[] savedPlanes;
    public int currentPlane;
    public int[] planePrices;

    [HideInInspector] public float gameStartTime;


}
