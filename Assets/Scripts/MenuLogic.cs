using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using GameAnalyticsSDK;

public class MenuLogic : MonoBehaviour
{
    #region Singleton
    private static MenuLogic _instance;
    public static MenuLogic Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    [SerializeField] RectTransform shopAndAds;

    [SerializeField] RectTransform nitroPanel;

    [SerializeField] RectTransform pauseButton;
    [SerializeField] RectTransform nitroButton;

    [SerializeField] RectTransform options;
    [SerializeField] Image optionsBlackBack;
    public Button optionsButton;

    [SerializeField] RectTransform shopMenu;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject inGameMenu;

    [SerializeField] Image mainPlaneImageInShop;
    [SerializeField] GameObject lockImageInShop;

    [SerializeField] Sprite[] planeImagesInScroll;

    [SerializeField] Sprite buySprite;
    [SerializeField] Sprite SelectSprite;
    
    [SerializeField] Image buyOrSelectButtonImage;
    [SerializeField] TMP_Text buyOrSelectText;

    [SerializeField] TMP_Text planePriceText;

    [SerializeField] RectTransform coinInfoRT;
    public GameObject addCoinButtonGO;
    public TMP_Text[] coinsTexts;

    [SerializeField] GameObject buyCoinBack;
    [SerializeField] RectTransform buyCoinPanel;

    public GameObject gameOverPanel;
    [SerializeField] RectTransform gameOverImg;
    [SerializeField] RectTransform restartButton;

    [SerializeField] float minRotationForShopButton;
    [SerializeField] float maxRotationForShopButton;

    #region Scroll view variables
    [SerializeField] RectTransform editAvatarScroll;
    [SerializeField] GraphicRaycaster CanvasRaycaster;
    [SerializeField] RectTransform scrollContent;
    [SerializeField] RectTransform child1;
    [SerializeField] RectTransform child2;
    //[SerializeField] float snapTime = 0.2f;
    [SerializeField] GameObject lastmiddleImage;
    [SerializeField] GameObject lastmiddleImageBefore;
    [SerializeField] float contentPosXForMiddleFirst = -2.5f;
    #endregion

    [Header("Menu variables")]
    [SerializeField] GameObject RedCross;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;
    [SerializeField] Image soundOnOffImage;


    int currentSelectedPlaneInScroll = 0;

    //const string S_soundOn = "sound";
    const string S_ControllerOn = "cOn";
    const string S_Planes = "planes";
    const string S_Coins = "coins";
    const string S_currentSelectedPlane = "cplane";
    const string S_levelInfo = "lvlinfo";
    private const int planePreviewImageWidth = 159;
    private const int numOfPlanes = 10;
    private bool optionsOpen = false;

    Vector2 tempPos;

    private void Start()
    {
        GameAnalytics.Initialize();
        shopMenu.localPosition = Screen.safeArea.position + new Vector2(Screen.width,0);
        scrollContent.GetComponent<HorizontalLayoutGroup>().padding.left = (int)(shopMenu.rect.width / 2) - planePreviewImageWidth;
        scrollContent.GetComponent<HorizontalLayoutGroup>().padding.right = (int)(shopMenu.rect.width / 2) - planePreviewImageWidth;
        if (!PlayerPrefs.HasKey(S_Planes))// if it is first time
        {
            CommonData.Instance.coinInt = 0;
            for (int i = 0; i < coinsTexts.Length; i++)
            {
                coinsTexts[i].text = CommonData.Instance.coinInt.ToString();
            }
            CommonData.Instance.currentPlane = 0;
            CommonData.Instance.savedPlanes = new int[10];
            PlayerPrefs.SetString(S_Planes, "1000000000");
            CommonData.Instance.savedPlanes[0] = 1;
            for (int i = 1; i < numOfPlanes; i++)
            {
                CommonData.Instance.savedPlanes[i] = 0;
            }
            PlayerPrefs.SetInt(S_levelInfo, 1);
            CommonData.Instance.levelInfo = 1;
            CommonData.Instance.levelSceneIndex = CommonData.Instance.MAXLEVELINDEX + 1;
            CommonData.Instance.tutorialScene = true;

            PlayerPrefs.SetInt(S_currentSelectedPlane, 0);
            PlayerPrefs.SetInt(S_Coins, 0);
            PlayerPrefs.SetInt(S_ControllerOn, BoolToInt(true));
            RedCross.SetActive(false);
            CommonData.Instance.controllerOn = true;
            InGameLogic.Instance.ChangeController();
        }
        else
        {
            CommonData.Instance.levelInfo = PlayerPrefs.GetInt(S_levelInfo);
            CommonData.Instance.controllerOn = IntToBool( PlayerPrefs.GetInt(S_ControllerOn) );
            RedCross.SetActive(!CommonData.Instance.controllerOn);
            InGameLogic.Instance.ChangeController();
            CommonData.Instance.savedPlanes = new int[10];
            CommonData.Instance.currentPlane = PlayerPrefs.GetInt(S_currentSelectedPlane);
            string planesString = PlayerPrefs.GetString(S_Planes);
            for (int i = 0; i < planesString.Length; i++)
            {
                CommonData.Instance.savedPlanes[i] = planesString[i] - '0';
            }
            CommonData.Instance.coinInt = PlayerPrefs.GetInt(S_Coins);
            for (int i = 0; i < coinsTexts.Length; i++)
            {
                coinsTexts[i].text = CommonData.Instance.coinInt.ToString();
            }
        }
        mainMenu.SetActive(false);
        inGameMenu.SetActive(false);
    }

    //public void TEMPCOINHACK(int coin)
    //{
    //    CommonData.Instance.coinInt = coin; 
    //}

    public void RotateButtonLeft(GameObject buttonToRotate)
    {
        if (buttonToRotate.activeInHierarchy)
        {
            LeanTween.rotateZ(buttonToRotate, minRotationForShopButton, 1f).setEaseOutSine().setOnComplete(() =>
            {
                RotateButtonRight(buttonToRotate);
            });
        }
    }

    public void RotateButtonRight(GameObject buttonToRotate)
    {
        LeanTween.rotateZ(buttonToRotate, maxRotationForShopButton, 1f).setEaseInSine().setOnComplete(() =>
        {
            RotateButtonLeft(buttonToRotate);
        });
    }

    public void StartGame_OnClick()
    {
        LeanTween.value(shopAndAds.anchoredPosition.x, shopAndAds.anchoredPosition.x + shopAndAds.rect.width*2, .8f).setEaseInOutSine().setOnUpdate(value => shopAndAds.anchoredPosition = new Vector2(value, shopAndAds.anchoredPosition.y));
        LeanTween.value(options.anchoredPosition.y, options.anchoredPosition.y + 200, .8f).setEaseInOutSine().setOnUpdate(value => options.anchoredPosition = new Vector2(options.anchoredPosition.x, value));
        LeanTween.value(pauseButton.anchoredPosition.x, pauseButton.anchoredPosition.x - pauseButton.rect.width, .8f).setEaseInOutSine().setOnUpdate(value => pauseButton.anchoredPosition = new Vector2(value, pauseButton.anchoredPosition.y));
        LeanTween.value(nitroPanel.anchoredPosition.x, nitroPanel.anchoredPosition.x + nitroPanel.rect.width + 20, .8f).setEaseInOutSine().setOnUpdate(value => nitroPanel.anchoredPosition = new Vector2(value, nitroPanel.anchoredPosition.y));
        LeanTween.value(nitroButton.anchoredPosition.x, nitroButton.anchoredPosition.x - nitroButton.rect.width, .8f).setEaseInOutSine().setOnUpdate(value => nitroButton.anchoredPosition = new Vector2(value, nitroButton.anchoredPosition.y));
    }

    public void ResetButtonsPositions()
    {
        addCoinButtonGO.SetActive(true);
        shopAndAds.anchoredPosition = new Vector2( -shopAndAds.rect.width/2, shopAndAds.anchoredPosition.y);
        options.anchoredPosition = new Vector2(options.anchoredPosition.x, 150);
        nitroPanel.anchoredPosition = new Vector2( -nitroPanel.rect.width/2, nitroPanel.anchoredPosition.y);
        pauseButton.anchoredPosition = new Vector2( pauseButton.rect.width/2, pauseButton.anchoredPosition.y);
        nitroButton.anchoredPosition = new Vector2( nitroButton.rect.width/2, nitroButton.anchoredPosition.y);
        gameOverImg.anchoredPosition = new Vector3( 0, gameOverImg.rect.width/2);
        restartButton.anchoredPosition = new Vector3(0, -restartButton.rect.width / 2);
        optionsButton.interactable = true;
        CommonData.Instance.levelInfoFill.value = 0;
    }

    public void MoveButtonsGameOver()
    {
        gameOverPanel.SetActive(true);
        LeanTween.value(nitroPanel.anchoredPosition.x, nitroPanel.anchoredPosition.x - nitroPanel.rect.width - 20, .5f).setEaseInOutSine().setOnUpdate(value => nitroPanel.anchoredPosition = new Vector2(value, nitroPanel.anchoredPosition.y));
        LeanTween.value(nitroButton.anchoredPosition.x, nitroButton.anchoredPosition.x + nitroButton.rect.width, .5f).setEaseInOutSine().setOnUpdate(value => nitroButton.anchoredPosition = new Vector2(value, nitroButton.anchoredPosition.y));
        LeanTween.value(pauseButton.anchoredPosition.x, pauseButton.anchoredPosition.x + pauseButton.rect.width, .5f).setEaseInOutSine().setOnUpdate(value => pauseButton.anchoredPosition = new Vector2(value, pauseButton.anchoredPosition.y));
        LeanTween.value(coinInfoRT.anchoredPosition.x, coinInfoRT.anchoredPosition.x - coinInfoRT.rect.width/2, .4f).setEaseInOutSine().setOnUpdate(value => coinInfoRT.anchoredPosition = new Vector2(value, coinInfoRT.anchoredPosition.y)).setOnComplete(() =>
        {
            LeanTween.value(coinInfoRT.anchoredPosition.x, coinInfoRT.anchoredPosition.x + coinInfoRT.rect.width / 2, .4f).setEaseInOutSine().setOnUpdate(value => coinInfoRT.anchoredPosition = new Vector2(value, coinInfoRT.anchoredPosition.y));
        });
        LeanTween.value(gameOverImg.anchoredPosition.y, -gameOverImg.rect.height/2, .8f).setEaseInOutSine().setOnUpdate(value => gameOverImg.anchoredPosition = new Vector2(  gameOverImg.anchoredPosition.x,value));
        LeanTween.value(restartButton.anchoredPosition.y,restartButton.rect.height , .8f).setEaseInOutSine().setOnUpdate(value => restartButton.anchoredPosition = new Vector2(restartButton.anchoredPosition.x,value));
    }

    public void MoveButtonsLevelEnd()
    {
        CommonData.Instance.levelInfoGO.SetActive(false);
        LeanTween.value(nitroPanel.anchoredPosition.x, nitroPanel.anchoredPosition.x - nitroPanel.rect.width - 20, .5f).setEaseInOutSine().setOnUpdate(value => nitroPanel.anchoredPosition = new Vector2(value, nitroPanel.anchoredPosition.y));
        LeanTween.value(nitroButton.anchoredPosition.x, nitroButton.anchoredPosition.x + nitroButton.rect.width, .5f).setEaseInOutSine().setOnUpdate(value => nitroButton.anchoredPosition = new Vector2(value, nitroButton.anchoredPosition.y));
        LeanTween.value(pauseButton.anchoredPosition.x, pauseButton.anchoredPosition.x + pauseButton.rect.width, .5f).setEaseInOutSine().setOnUpdate(value => pauseButton.anchoredPosition = new Vector2(value, pauseButton.anchoredPosition.y));
    }

    public void OpenSettings_OnClick()
    {
        if (!optionsOpen)
        {
            LeanTween.value(options.anchoredPosition.y, options.anchoredPosition.y - 500, .3f).setEaseInOutSine().setOnUpdate(value => options.anchoredPosition = new Vector2(options.anchoredPosition.x, value));
            optionsBlackBack.enabled = true;
            optionsOpen = true;
        }
        else CloseSettings_OnClick();
    }

    public void CloseSettings_OnClick()
    {
        optionsBlackBack.enabled = false;
        LeanTween.value(options.anchoredPosition.y, options.anchoredPosition.y + 500, .3f).setEaseInOutSine().setOnUpdate(value => options.anchoredPosition = new Vector2(options.anchoredPosition.x, value));
        optionsOpen = false;
    }

    // Main menu
    public void SoundOnOff_OnClick()
    {
        if (AudioManager.Instance.soundOn)
        {
            soundOnOffImage.sprite = soundOffSprite;
            AudioManager.Instance.TurnOnOffSound(false);
        }
        else
        {
            soundOnOffImage.sprite = soundOnSprite;
            AudioManager.Instance.TurnOnOffSound(true);
        }
    }

    public void ControllerOnOff_OnClick()
    {
        if (CommonData.Instance.controllerOn)
        {
            RedCross.SetActive(true);
            CommonData.Instance.controllerOn = false;
        }
        else
        {
            RedCross.SetActive(false);
            CommonData.Instance.controllerOn = true;
        }
        PlayerPrefs.SetInt(S_ControllerOn, BoolToInt(CommonData.Instance.controllerOn));
        InGameLogic.Instance.ChangeController();
    }
    
    public void OpenBuyCoin_OnClick()
    {
        buyCoinBack.SetActive(true);
        LeanTween.scale(buyCoinPanel, Vector3.one, 0.3f);
    }

    public void CloseBuyCoin_OnClick()
    {
        LeanTween.scale(buyCoinPanel, Vector3.zero, 0.3f).setOnComplete(() => buyCoinBack.SetActive(false));
    }

    // Shop 
    public void OpenShop_OnClick()
    {
        tempPos.x = contentPosXForMiddleFirst;
        tempPos.y = 0f;
        scrollContent.anchoredPosition = tempPos;
        shopMenu.gameObject.SetActive(true);
        //mainMenu.SetActive(false);
        mainPlaneImageInShop.sprite = planeImagesInScroll[CommonData.Instance.currentPlane];
        MoveToPositionInitial(CommonData.Instance.planesShopPositions[CommonData.Instance.currentPlane]);

        LeanTween.value(shopMenu.anchoredPosition.x, 0f , .5f).setOnComplete( ()=> { 
        }) .setEaseInOutSine().setOnUpdate(value => shopMenu.anchoredPosition = new Vector2(value, shopMenu.anchoredPosition.y));

        buyOrSelectButtonImage.GetComponent<Button>().interactable = true;
        lockImageInShop.SetActive(false);
        buyOrSelectButtonImage.sprite = SelectSprite;
        buyOrSelectText.text = "Selected";

        //if (CommonData.Instance.savedPlanes[0] == 1) // if we bought 0th plane
        //{
        //    buyOrSelectButtonImage.GetComponent<Button>().interactable = true;
        //    lockImageInShop.SetActive(false);
        //    buyOrSelectButtonImage.sprite = SelectSprite;
        //    if (CommonData.Instance.currentPlane == 0)
        //    {
        //        buyOrSelectText.text = "Selected";
        //    }
        //    else
        //    {
        //        buyOrSelectText.text = "Select";
        //    }
        //}
        //else // if we havent bought 0th plane
        //{
        //    buyOrSelectText.text = "Buy";
        //    lockImageInShop.SetActive(true);
        //    buyOrSelectButtonImage.sprite = buySprite;
        //    if (CommonData.Instance.coinInt >= CommonData.Instance.planePrices[0]) // if we have money to buy it
        //    {
        //        buyOrSelectButtonImage.GetComponent<Button>().interactable = true;
        //    }
        //    else
        //    {
        //        buyOrSelectButtonImage.GetComponent<Button>().interactable = false;
        //    }
        //}
    }

    public void CloseShop_OnClick()
    {
        LeanTween.value(shopMenu.anchoredPosition.x, Screen.width, .5f).setOnComplete(() =>
        {
            shopMenu.gameObject.SetActive(false);
        }).setEaseInOutSine().setOnUpdate(value => shopMenu.anchoredPosition = new Vector2(value, shopMenu.anchoredPosition.y));

        tempPos.x = contentPosXForMiddleFirst;
        tempPos.y = 0f;
        scrollContent.anchoredPosition = tempPos;
    }

    public void BuyOrSelectPlanePressed()
    {
        // Buy
        if (CommonData.Instance.savedPlanes[currentSelectedPlaneInScroll] == 0 && CommonData.Instance.coinInt >= CommonData.Instance.planePrices[currentSelectedPlaneInScroll])
        {
            CommonData.Instance.savedPlanes[currentSelectedPlaneInScroll] = 1;
            CommonData.Instance.coinInt -= CommonData.Instance.planePrices[currentSelectedPlaneInScroll];
            string savingPlanesDatas = "";
            for (int i = 0; i < CommonData.Instance.savedPlanes.Length; i++)
            {
                savingPlanesDatas += CommonData.Instance.savedPlanes[i].ToString();
            }
            for (int i = 0; i < coinsTexts.Length; i++)
            {
                coinsTexts[i].text = CommonData.Instance.coinInt.ToString();
            }
            PlayerPrefs.SetString(S_Planes, savingPlanesDatas);
            PlayerPrefs.SetInt(S_Coins, CommonData.Instance.coinInt);
            lockImageInShop.SetActive(false);
            buyOrSelectButtonImage.sprite = SelectSprite;
            buyOrSelectText.text = "Select";
        }
        // Select
        else
        {
            buyOrSelectText.text = "Selected";
            CommonData.Instance.currentPlane = currentSelectedPlaneInScroll;
            InGameLogic.Instance.ChangePlane();
            InGameLogic.Instance.ActivateTrails();
            PlayerPrefs.SetInt(S_currentSelectedPlane, currentSelectedPlaneInScroll);
        }
    }

    #region scroll view functions

    public void OnScrollFinished()
    {
        if (lastmiddleImage != null)
        {
            currentSelectedPlaneInScroll = lastmiddleImage.GetComponent<InShopPlaneInfo>().inShopPlaneIndex;
            mainPlaneImageInShop.sprite = planeImagesInScroll[currentSelectedPlaneInScroll];

            if (CommonData.Instance.savedPlanes[currentSelectedPlaneInScroll] == 0) // Buy
            {
                lockImageInShop.SetActive(true);
                buyOrSelectButtonImage.sprite = buySprite;
                buyOrSelectText.text = "Buy";
                planePriceText.text = CommonData.Instance.planePrices[currentSelectedPlaneInScroll].ToString();
                if (CommonData.Instance.coinInt >= CommonData.Instance.planePrices[currentSelectedPlaneInScroll]) // if we have money
                {
                    buyOrSelectButtonImage.GetComponent<Button>().interactable = true;
                }
                else
                {
                    buyOrSelectButtonImage.GetComponent<Button>().interactable = false;
                }
            }
            else // Select
            {
                buyOrSelectButtonImage.GetComponent<Button>().interactable = true;
                lockImageInShop.SetActive(false);
                buyOrSelectButtonImage.sprite = SelectSprite;
                if (CommonData.Instance.currentPlane == currentSelectedPlaneInScroll)
                {
                    buyOrSelectText.text = "Selected";
                }
                else
                {
                    buyOrSelectText.text = "Select";
                }
            }

            Debug.Log("CurrentPlane: " + currentSelectedPlaneInScroll);
            MoveToPositionOfTheObjectInScroll(lastmiddleImage);
        }
    }

    public void MoveToPositionOfTheObjectInScroll(GameObject destination)
    {
        if (destination.GetComponent<InShopPlaneInfo>() != null)
        {
            scrollContent.anchoredPosition = new Vector2(contentPosXForMiddleFirst - destination.GetComponent<InShopPlaneInfo>().inShopPlaneIndex * (child2.anchoredPosition.x - child1.anchoredPosition.x), scrollContent.anchoredPosition.y);
            //LeanTween.value(scrollContent.anchoredPosition.x, contentPosXForMiddleFirst - destination.GetComponent<InShopPlaneInfo>().inShopPlaneIndex * (child2.anchoredPosition.x - child1.anchoredPosition.x), snapTime).setEaseInBack().setOnUpdate(UpdateContentX);
            //scrollContent.anchoredPosition = new Vector2(-2.6f - destination.GetComponent<InShopPlaneInfo>().inShopPlaneIndex * 62f, scrollContent.anchoredPosition.y);
        }
    }
    
    public void MoveToPositionInitial(GameObject destination)
    {
        scrollContent.anchoredPosition = new Vector2(contentPosXForMiddleFirst - destination.GetComponent<InShopPlaneInfo>().inShopPlaneIndex * (child2.anchoredPosition.x - child1.anchoredPosition.x), scrollContent.anchoredPosition.y);
    }

    void UpdateContentX(float contentX)
    {
        scrollContent.anchoredPosition = new Vector2(contentX, scrollContent.anchoredPosition.y);
    }

    public void OnScrollviewChanged()
    {
        lastmiddleImageBefore = lastmiddleImage;
        lastmiddleImage = GetLastMiddleImageInScroll();
        if (lastmiddleImage == null)
        {
            lastmiddleImage = lastmiddleImageBefore;
        }
    }

    public GameObject GetLastMiddleImageInScroll()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = editAvatarScroll.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        CanvasRaycaster.Raycast(pointerData, results);
        if (results.Count != 0 && results[0].gameObject.layer == 11)//.CompareTag("a"))
        {
            return results[0].gameObject;
        }

        return null;
    }

    #endregion

    bool IntToBool(int a)
    {
        if (a == 0) return false;
        else return true;
    }

    int BoolToInt(bool a)
    {
        if (!a) return 0;
        else return 1;
    }

}
