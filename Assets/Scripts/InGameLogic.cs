using Cinemachine;
using PathCreation;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameLogic : MonoBehaviour {

    #region Singleton
    private static InGameLogic _instance;
    public static InGameLogic Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    #region Variables
    private const string PP_TUTSCENE = "tscn";

    private const int TUTCHECKLAYER = 6;
    //private const int TURNCHECKLAYER = 6;
    private const int HITOBJECTSLAYER = 7;
    private const int COINLAYER = 8;
    private const int NITROLAYER = 9;
    private const int ENDLAYER = 10;
    private const float INTROTIME = 0f;
    private readonly float nitroAddTime = .5f;
    //private readonly float turnSmoothVelocityConst = 12f;

    [SerializeField] GameObject pauseBlackBack;
    [SerializeField] RectTransform pausePanel;

    [SerializeField] private GameObject startPanel;
    private GameObject controller;
    private Joystick joystick;
    private Rigidbody rigidBody;
    private GameObject currentPlane;
    private GameObject currentFull;
    private GameObject currentParts;
    private TrailRenderer[] currentTrails;

    private bool nitroUsing = false;
    private readonly float nitroFullTime = 60f;
    private readonly float nitroTime = 5f;
    private float force = 0;
    private float tempForce = 0;
    private readonly float maxSelfRotate = 85;

    private float ZERO = 0;
    private bool startedMove = false;

    private float selfRotateSmoothVelocity = 5;
    private readonly float selfRotateSmoothTime = .4f;//.6
    private float turnSmoothVelocity = 16;//12
    private float turnSmoothTime = 1.3f;//2.0
    //private readonly float sideForceUnit = 12;

    //private bool turnCheck = false;
    private float currentSelfAngle;
    private float currentTurnAngle;
    private float horizontal;
    private float turnCheckAngle;
    private float turnValueAround;

    private AsyncOperation newSceneLoad;
    private AsyncOperation lastSceneUnload;
    private bool levelStart = false;
    private bool gameStart = false;
    private bool paused = false;
    //private bool nextLevelLoaded = false;

    //  TRACK OPTIMIZATION
    PointPath currentPointPath;
    //

    //  LIMIT THE PLAYER
    [SerializeField] private float limitDistance;
    private Vector3 closestPointOnPath;
    private float currentDistanceFromPath;
    //private int limitMovement = 1;
    //

    // CAMERA MOVEMENT AND TRACK
    [SerializeField] private float trackPointOffset;
    [SerializeField] private Rigidbody trackPointRigidbody;

    [SerializeField] private CinemachineVirtualCamera cinemachine;
    private CinemachineTrackedDolly cinemachineTrackedDolly;
    [SerializeField] private CinemachineVirtualCamera cinemachineDeath;
    [SerializeField] private GameObject cinemachinePath;
    private CinemachineSmoothPath cinemachineSmoothPath;
    [SerializeField] private int numberOfPoints;
    [SerializeField] Transform planeParent;
    [SerializeField] private Animator animator;
    //

    //WinPanel
    [SerializeField] private RectTransform[] stars;
    [SerializeField] private Image[] leftArrows;
    [SerializeField] private Image[] rightArrows;
    [SerializeField] private RectTransform backStar;
    private Coroutine arrowCoroutine;
    private int coinsCollectedInRound = 0;
    //

    //  NITRO
    [SerializeField] GameObject nitroEffect;
    [SerializeField] GameObject coinPS;
    [SerializeField] GameObject nitroPS;
    private Coroutine nitroUpdateCoroutine;
    private Coroutine nitroUseCoroutine;
    //

    //  ADDITIONAL VARIABLES TO NOT COLLECT GARBAGE
    private VertexPath trackPath;

    //private Vector3 targetDirection;
    //private Vector3 crossProductForSideCalc;
    //private float dotProductForSideCalc;

    //

    #endregion

    public void SceneLoadingComplete()
    {
        rigidBody = GetComponent<Rigidbody>();
        cinemachineTrackedDolly = cinemachine.GetCinemachineComponent<CinemachineTrackedDolly>();

        ChangePlane();
        ChangeController();

        StartCoroutine(NextLevelLoadingCompletedEnumerator());
    }

    private IEnumerator NextLevelLoadingCompletedEnumerator()
    {
        float distanceUnit = LevelBasedData.Instance.pathCreatorCamera.path.length / numberOfPoints;

        if (cinemachinePath.GetComponent<CinemachineSmoothPath>() != null)
        {
            Destroy(cinemachinePath.GetComponent<CinemachineSmoothPath>());
            yield return new WaitForSeconds(.2f);
        }
        cinemachineSmoothPath = cinemachinePath.AddComponent<CinemachineSmoothPath>();
        cinemachineTrackedDolly.m_Path = cinemachineSmoothPath;
        cinemachineSmoothPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[numberOfPoints];

        //Create camera path from Bezier Path
        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            cinemachineSmoothPath.m_Waypoints[i].position = LevelBasedData.Instance.pathCreatorCamera.path.GetPointAtDistance(i * distanceUnit);
        }
        cinemachineSmoothPath.m_Waypoints[numberOfPoints - 1].position = LevelBasedData.Instance.pathCreatorCamera.path.GetPointAtDistance(LevelBasedData.Instance.pathCreatorCamera.path.length - 1);

        trackPath = LevelBasedData.Instance.pathCreatorTrackPoint.path;

        currentPointPath = new PointPath();
        currentPointPath.ResetPointInfos();
        StartCoroutine(currentPointPath.CreateWithDistance(trackPath, .3f,
                () =>
                {
                    ChangeLevelInfo();
                    transform.SetPositionAndRotation(LevelBasedData.Instance.playerInitialPoint.position, LevelBasedData.Instance.playerInitialPoint.rotation);
                    CommonData.Instance.levelInfoGO.SetActive(true);
                    ActivateTrails();
                    animator.Play("StartAnim");
                    rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                    trackPointRigidbody.MovePosition(currentPointPath.MainPoints[currentPointPath.ClosestPointIndex(LevelBasedData.Instance.playerInitialPoint.position) + 100]);
                    if (!gameStart)
                    {
                        if (Time.time - CommonData.Instance.gameStartTime >= INTROTIME)
                        {
                            CommonData.Instance.introCamera.SetActive(false);
                            CommonData.Instance.introCanvas.SetActive(false);
                            CommonData.Instance.mainMenuCanvas.SetActive(true);
                            CommonData.Instance.inGameMenuCanvas.SetActive(true);
                            gameStart = true;
                        }
                        else
                        {
                            StartCoroutine(StartAfterLoading(INTROTIME - Time.time - CommonData.Instance.gameStartTime));
                        }
                    }
                    else
                    {
                        CommonData.Instance.winPanel.SetActive(false);
                        //Debug.Log(currentPointPath.MainPoints[currentPointPath.ClosestPointIndex(transform.position) + 100]);
                        trackPointRigidbody.MovePosition(currentPointPath.MainPoints[currentPointPath.ClosestPointIndex(transform.position) + 100]);
                        startPanel.SetActive(true);
                        LeanTween.alpha(CommonData.Instance.loadingBlack, 0, .3f).setOnComplete(() => CommonData.Instance.loadingCanvas.SetActive(false));
                    }
                }));
    }

    private IEnumerator StartAfterLoading(float time)
    {
        yield return new WaitForSeconds(time);
        CommonData.Instance.introCamera.SetActive(false);
        CommonData.Instance.introCanvas.SetActive(false);
        CommonData.Instance.mainMenuCanvas.SetActive(true);
        CommonData.Instance.inGameMenuCanvas.SetActive(true);
        MenuLogic.Instance.RotateButtonLeft(CommonData.Instance.shopButtonInMainMenu);
        MenuLogic.Instance.RotateButtonRight(CommonData.Instance.noAdsButtonInMainMenu);
        gameStart = true;
    }

    public void OnClick_StartGame()
    {
        animator.SetTrigger("idle");
        cinemachine.m_Priority = 1;
        cinemachineDeath.m_Priority = 0;
        LevelBasedData.Instance.startCamera.m_Priority = 0;
        LevelBasedData.Instance.endCamera.m_Priority = 0;

        MenuLogic.Instance.addCoinButtonGO.SetActive(false);
        MenuLogic.Instance.optionsButton.interactable = false;

        //turnCheck = false;
        startedMove = false;

        startPanel.SetActive(false);
        cinemachine.m_Lens.FieldOfView = 50f;
        cinemachineTrackedDolly.m_PathOffset.z = -30f;//-20
        CommonData.Instance.nitroButton.interactable = false;
        StartCoroutine(WaitForCameraStart());
    }

    private IEnumerator WaitForCameraStart()
    {
        yield return new WaitForSeconds(.8f);
        AudioManager.Instance.SetVolumePitch(1, .3f, 1f);
        AudioManager.Instance.Play(1);
        levelStart = true;
        nitroUpdateCoroutine = StartCoroutine(FuelUpdate());
        StartCoroutine(LevelInfoUpdate());
        controller.SetActive(true);
        force = 40;
        //turnCheck = false;
        rigidBody.constraints = RigidbodyConstraints.None;
    }

    private IEnumerator LevelInfoUpdate()
    {
        while (levelStart)
        {
            CommonData.Instance.levelInfoFill.value = (float) currentPointPath.CurrentIndex / currentPointPath.NumOfPoints;
            yield return new WaitForSeconds(.2f);
        }
    }

    public void ChangeLevelInfo()
    {
        CommonData.Instance.levelLeft.text = (CommonData.Instance.levelInfo).ToString();
        CommonData.Instance.levelRight.text = (CommonData.Instance.levelInfo + 1).ToString();
    }

    //private void OnDrawGizmos()
    //{
    //    for(int i=0; i < currentPointPath.NumOfPoints; i++)
    //    {
    //        Gizmos.DrawSphere(currentPointPath.MainPoints[i], .1f + 0.002f * i);
    //    }
    //}

    private IEnumerator FuelUpdate()
    {
        while (levelStart)
        {
            if (!paused)
            {
                if (!nitroUsing && CommonData.Instance.nitroFill.value > .2f && !CommonData.Instance.nitroButton.interactable)
                {
                    CommonData.Instance.nitroButton.interactable = true;
                }
                else if (CommonData.Instance.nitroFill.value < .2f && CommonData.Instance.nitroButton.interactable)
                {
                    CommonData.Instance.nitroButton.interactable = false;
                }
                CommonData.Instance.nitroFill.value += Time.deltaTime / nitroFullTime;
            }
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (levelStart)
        {
            trackPointRigidbody.MovePosition(currentPointPath.MainPoints[currentPointPath.ClosestPointIndex(transform.position) + 100]);
            horizontal = joystick.Horizontal;
            if (horizontal != 0 && !startedMove) startedMove = true;
            closestPointOnPath = currentPointPath.LastClosePoint;
            currentDistanceFromPath = Vector3.Distance(closestPointOnPath, transform.position);

            MoveTurn();

            //if (turnCheck) MoveTurn();
            //else MoveSide();
        }
    }

    //private void MoveSide()
    //{
    //    if (currentDistanceFromPath >= limitDistance)
    //    {
    //        if(OnWhichSide() == 1) limitMovement = horizontal > 0 ? 0 : 1;
    //        else limitMovement = horizontal < 0 ? 0 : 1;
    //    }

    //    rigidBody.velocity = limitMovement * horizontal * sideForceUnit * SomeCalculations.RightDirection(transform.forward) + force * transform.forward;

        
    //    currentSelfAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, -maxSelfRotate * horizontal, ref selfRotateSmoothVelocity, selfRotateSmoothTime);
    //    rigidBody.MoveRotation(Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + turnValueAround, currentSelfAngle)));
    //    limitMovement = 1;
    //}

    private void MoveTurn()
    {
        if (currentDistanceFromPath >= limitDistance) GameOver();

        rigidBody.velocity = force * transform.forward;

        if (!startedMove)
        {
            currentSelfAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, -maxSelfRotate * horizontal, ref ZERO, selfRotateSmoothTime);
            currentTurnAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.y + 90 * horizontal, ref ZERO, turnSmoothTime);
        }
        else
        {
            currentSelfAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, -maxSelfRotate * horizontal, ref selfRotateSmoothVelocity, selfRotateSmoothTime);
            currentTurnAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.y + 90 * horizontal, ref turnSmoothVelocity, turnSmoothTime);
        }

        rigidBody.MoveRotation(Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, currentTurnAngle, currentSelfAngle)));
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.gameObject.layer == TURNCHECKLAYER)
        //{
        //    turnCheck = !turnCheck;
        //    if (turnCheck)
        //    {
        //        turnCheckAngle = other.GetComponent<TurnAngle>().turnAngle;
        //    }
        //    else
        //    {
        //        turnCheckAngle = other.GetComponent<TurnAngle>().turnAngle;

        //        turnValueAround = turnCheckAngle - transform.rotation.eulerAngles.y;
        //        Debug.Log("1 : " + turnValueAround);
        //        if(Mathf.Abs(turnValueAround) > 90)
        //        {
        //            if (turnValueAround < 0) turnValueAround += 360;
        //            else turnValueAround -= 360;
        //        }
        //        //turnValueAround = (Mathf.Abs(turnValueAround) > 90) ? turnValueAround - 360 : turnValueAround;
        //        Debug.Log("2 : " + turnValueAround);
        //        turnValueAround /= .7f / Time.fixedDeltaTime;
        //        Debug.Log("3 : " + turnValueAround);

        //        turnSmoothVelocity = turnSmoothVelocityConst;

        //        StartCoroutine(WaitAndResetTurnValueAround());
        //    }
        //}
        if (other.gameObject.layer == COINLAYER)
        {
            AudioManager.Instance.Play(2);
            coinsCollectedInRound += 1;
            CommonData.Instance.coinInt += 100;
            for (int i = 0; i < MenuLogic.Instance.coinsTexts.Length; i++)
            {
                MenuLogic.Instance.coinsTexts[i].text = CommonData.Instance.coinInt.ToString();
            }
            PlayerPrefs.SetInt("coins", CommonData.Instance.coinInt);
            GameObject particle = Instantiate(coinPS, other.transform.position + new Vector3(0, .7f, 0), Quaternion.identity);
            Destroy(particle, 1f);
            Destroy(other.gameObject);
        }
        else if(other.gameObject.layer == NITROLAYER)
        {
            AudioManager.Instance.Play(3);
            GameObject particle = Instantiate(nitroPS, other.transform.position + new Vector3(0, .7f, 0), Quaternion.identity);
            Destroy(particle, 1f);
            StartCoroutine(AddNitro());
            Destroy(other.gameObject);
        }
        else if (other.gameObject.layer == ENDLAYER && levelStart)
        {
            LevelEnd(other.gameObject.transform.position);
        }
        else if (other.gameObject.layer == TUTCHECKLAYER)
        {
            Time.timeScale = 0;
            int ID = other.gameObject.GetComponent<TutCheckID>().ID;
            if (ID == 2) CommonData.Instance.nitroFill.value = 1f;
            TutorialAnim.Instance.PlayNextAnim(ID);
        }
    }

    private void LevelEnd(Vector3 endPosition)
    {
        int lastCoins = coinsCollectedInRound * 100;
        int stars;
        Debug.Log("Collected Coins : " + coinsCollectedInRound + " Level Coins : " + LevelBasedData.Instance.coinsInLevel);
        if (coinsCollectedInRound >= LevelBasedData.Instance.coinsInLevel - 1)
        {
            stars = 3;
            lastCoins += 500;
        }
        else if(coinsCollectedInRound <= 5)
        {
            stars = 1;
            lastCoins += 100;
        }
        else
        {
            stars = 2;
            lastCoins += 250;
        }
        CommonData.Instance.coinEndScreen.text = lastCoins.ToString();
        CommonData.Instance.coinInt += lastCoins - coinsCollectedInRound * 100;
        PlayerPrefs.SetInt("coins", CommonData.Instance.coinInt);
        for (int i = 0; i < MenuLogic.Instance.coinsTexts.Length; i++)
        {
            MenuLogic.Instance.coinsTexts[i].text = CommonData.Instance.coinInt.ToString();
        }

        CommonData.Instance.winPanel.SetActive(true);
        LeanTween.value(rigidBody.rotation.eulerAngles.z > 180 ? rigidBody.rotation.eulerAngles.z - 360 : rigidBody.rotation.eulerAngles.z, 0, .5f).setOnUpdate(value => rigidBody.MoveRotation(Quaternion.Euler(rigidBody.rotation.eulerAngles.x, rigidBody.rotation.eulerAngles.y, value)));
        LeanTween.move(gameObject, new Vector3(endPosition.x, 10, endPosition.z),.5f).setOnComplete(() => 
            {
                animator.Play("PlaneFinishRotation");
                AudioManager.Instance.Stop(1);
                AudioManager.Instance.Play(4);
            });
        controller.SetActive(false);
        MenuLogic.Instance.MoveButtonsLevelEnd();
        StartCoroutine(EndGameStars(stars));
        arrowCoroutine = StartCoroutine(ArrowCoroutine());
        force = 0;
        levelStart = false;
        rigidBody.constraints = RigidbodyConstraints.FreezePosition;
        LevelBasedData.Instance.endCamera.m_Priority = 1;
        cinemachine.m_Priority = 0;

        CommonData.Instance.levelSceneIndex = CommonData.Instance.levelSceneIndex >= CommonData.Instance.MAXLEVELINDEX ? 1 : CommonData.Instance.levelSceneIndex + 1;
        newSceneLoad = SceneManager.LoadSceneAsync(CommonData.Instance.levelSceneIndex, LoadSceneMode.Additive);
        PlayerPrefs.SetInt("lvl", CommonData.Instance.levelSceneIndex);
        
        newSceneLoad.allowSceneActivation = false;
        coinsCollectedInRound = 0;
    }

    public void NextLevel()
    {
        StopCoroutine(arrowCoroutine);
        StartCoroutine(TransitionToNextLevel());
    }

    private IEnumerator TransitionToNextLevel()
    {
        joystick.Reset_FromMe();
        CommonData.Instance.loadingCanvas.SetActive(true);
        LeanTween.alpha(CommonData.Instance.loadingBlack, 1, .5f);
        yield return new WaitForSeconds(.5f);
        for (int i = 0; i < 3; i++)
        {
            stars[i].gameObject.SetActive(false);
        }
        MenuLogic.Instance.ResetButtonsPositions();
        newSceneLoad.allowSceneActivation = true;
        CommonData.Instance.levelInfo++;
        PlayerPrefs.SetInt("lvlinfo", CommonData.Instance.levelInfo);

        Debug.Log(CommonData.Instance.levelSceneIndex);
        if (CommonData.Instance.tutorialScene)
        {
            lastSceneUnload = SceneManager.UnloadSceneAsync(CommonData.Instance.MAXLEVELINDEX+1);
            CommonData.Instance.tutorialScene = false;
            PlayerPrefs.SetInt(PP_TUTSCENE, 0);
        }
        else
        {
            lastSceneUnload = SceneManager.UnloadSceneAsync(CommonData.Instance.levelSceneIndex - 1 == 0 ? CommonData.Instance.MAXLEVELINDEX : CommonData.Instance.levelSceneIndex - 1);
        }

        while (!lastSceneUnload.isDone)
        {
            yield return null;
        }

        StartCoroutine(NextLevelLoadingCompletedEnumerator());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == HITOBJECTSLAYER && levelStart) GameOver();
    }

    //private IEnumerator WaitAndResetTurnValueAround()
    //{
    //    yield return new WaitForSeconds(.7f);
    //    turnValueAround = 0;
    //}

    public void OnClick_Nitro()
    {
        CommonData.Instance.nitroButton.interactable = false;
        nitroUsing = true;
        nitroEffect.SetActive(true);
        LeanTween.value(cinemachine.gameObject,50f, 90f, .5f).setOnUpdate(value => { cinemachine.m_Lens.FieldOfView = value; }).setEaseInOutSine();
        LeanTween.value(cinemachine.gameObject,-30f/*-20f*/, 0f, .5f).setOnUpdate(value => { cinemachineTrackedDolly.m_PathOffset = new Vector3(0,0, value) ; }).setEaseInOutSine();
        LeanTween.value(1, 2, .5f).setOnUpdate(value =>
          {
              AudioManager.Instance.SetVolumePitch(1, .3f * value, value);
          });
        nitroUseCoroutine = StartCoroutine(NitroUpdate());
    }

    private IEnumerator NitroUpdate()
    {
        force = 60;
        turnSmoothVelocity = 12f;
        turnSmoothTime = 1.0f;
        while (true)
        {
            if (paused)
            {
                yield return null;
                continue;
            }
            CommonData.Instance.nitroFill.value -= Time.deltaTime / nitroTime;
            if (CommonData.Instance.nitroFill.value <= 0) break;
            yield return null;
        }
        nitroUsing = false;
        LeanTween.value(cinemachine.gameObject, cinemachine.m_Lens.FieldOfView, 50f, .4f).setOnUpdate(value => { cinemachine.m_Lens.FieldOfView = value; }).setEaseInOutSine();
        LeanTween.value(cinemachine.gameObject, cinemachineTrackedDolly.m_PathOffset.z, -30f/*-20f*/, .4f).setOnUpdate(value => { cinemachineTrackedDolly.m_PathOffset = new Vector3(0, 0, value); });//.setEaseInOutSine();
        LeanTween.value(2, 1, .5f).setOnUpdate(value =>
        {
            AudioManager.Instance.SetVolumePitch(1, .3f * value, value);
        }).setOnComplete(() => nitroEffect.SetActive(false));
        force = 40;
        turnSmoothVelocity = 16f;
        turnSmoothTime = 1.3f;
    }

    private IEnumerator AddNitro()
    {
        float tempNitro = .2f;
        while(tempNitro > 0)
        {
            CommonData.Instance.nitroFill.value += Time.deltaTime / nitroAddTime;
            tempNitro -= Time.deltaTime / nitroAddTime;
            yield return null;
        }
    }

    public void ChangePlane()
    {
        if (currentPlane != null) Destroy(currentPlane);

        currentPlane = Instantiate(CommonData.Instance.planes[CommonData.Instance.currentPlane], planeParent);
        PlaneDetails planeDetails = currentPlane.GetComponent<PlaneDetails>();
        currentFull = planeDetails.full;
        currentParts = planeDetails.parts;
        currentTrails = planeDetails.trails;
    }

    public void ActivateTrails()
    {
        for (int i = 0; i < currentTrails.Length; i++)
        {
            currentTrails[i].enabled = true;
        }
    }
    
    public void DeactivateTrails()
    {
        for (int i = 0; i < currentTrails.Length; i++)
        {
            currentTrails[i].enabled = false;
        }
    }

    public void ChangeController()
    {
        if (CommonData.Instance.controllerOn)
        {
            controller = CommonData.Instance.visibleController;
            joystick = CommonData.Instance.visibleJoystick;
        }
        else
        {
            controller = CommonData.Instance.invisibleController;
            joystick = CommonData.Instance.invisibleJoystick;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over Once");
        AudioManager.Instance.Stop(1);
        AudioManager.Instance.Play(6);
        LeanTween.value(0, 0.7f, 0.5f).setOnUpdate(value => AudioManager.Instance.sounds[6].source.volume = value);
        levelStart = false;
        if(nitroUpdateCoroutine != null) StopCoroutine(nitroUpdateCoroutine);
        if(nitroUseCoroutine != null) StopCoroutine(nitroUseCoroutine);
        nitroEffect.SetActive(false);
        force = 0;
        nitroUsing = false;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;

        currentPointPath.ResetPointInfos();
        cinemachineDeath.transform.position = cinemachine.transform.position;
        cinemachineDeath.m_Priority = 1;
        cinemachine.m_Priority = 0;
        
        currentFull.SetActive(false);
        currentParts.SetActive(true);
        controller.SetActive(false);
        joystick.Reset_FromMe();
        
        InitVariables();
        StartCoroutine(AfterGameOver(1f));
        MenuLogic.Instance.MoveButtonsGameOver();
    }

    private IEnumerator AfterGameOver(float time)
    {
        yield return new WaitForSeconds(time);
        trackPointRigidbody.MovePosition(currentPointPath.MainPoints[currentPointPath.ClosestPointIndex(LevelBasedData.Instance.playerInitialPoint.transform.position) + 100]);
    }

    public void OnClick_Restart()
    {
        AudioManager.Instance.Stop(6);
        AudioManager.Instance.sounds[6].source.volume = 0;
        tempForce = 0;
        levelStart = false;
        controller.SetActive(false);
        currentPointPath.ResetPointInfos();
        StartCoroutine(AfterGameOver(.5f));
        joystick.Reset_FromMe();
        coinsCollectedInRound = 0;
        StartCoroutine(RestartTransition());
    }

    public void OpenPausePanel_OnClick()
    {
        AudioManager.Instance.Stop(1);
        tempForce = force;
        force = 0;
        paused = true;
        pauseBlackBack.SetActive(true);
        LeanTween.scale(pausePanel, Vector2.one, .3f);
    }

    public void ClosePausePanel_OnClick()
    {
        Debug.Log("CLOSED");
        LeanTween.scale(pausePanel, Vector2.zero, .3f).setOnComplete(() => 
        {
            AudioManager.Instance.Play(1);
            pauseBlackBack.SetActive(false);
            paused = false;
            force = tempForce;
            tempForce = 0;
        });
    }

    private IEnumerator RestartTransition()
    {
        CommonData.Instance.loadingCanvas.SetActive(true);
        LeanTween.alpha(CommonData.Instance.loadingBlack, 1, .5f);
        yield return new WaitForSeconds(.5f);
        ChangePlane();
        //Debug.Log(CommonData.Instance.levelSceneIndex);
        Debug.Log(CommonData.Instance.levelSceneIndex);
        SceneManager.UnloadSceneAsync(CommonData.Instance.levelSceneIndex).completed += (asyncOperation) =>
        {
            SceneManager.LoadSceneAsync(CommonData.Instance.levelSceneIndex, LoadSceneMode.Additive).completed += (asyncOperation) =>
            {
                LevelBasedData.Instance.startCamera.m_Priority = 1;
                cinemachineDeath.m_Priority = 0;
                transform.SetPositionAndRotation(LevelBasedData.Instance.playerInitialPoint.position, LevelBasedData.Instance.playerInitialPoint.rotation);
                ActivateTrails();
                MenuLogic.Instance.gameOverPanel.SetActive(false);
                MenuLogic.Instance.ResetButtonsPositions();
                CommonData.Instance.nitroFill.value = 0f;
                pausePanel.localScale = Vector2.zero;
                pauseBlackBack.SetActive(false);
                paused = false;
                tempForce = 0;
                LeanTween.alpha(CommonData.Instance.loadingBlack, 0, .5f).setOnComplete(() =>
                {
                    startPanel.SetActive(true);
                    CommonData.Instance.loadingCanvas.SetActive(false);
                });
            };
        };
    }

    //WinEndGame
    private IEnumerator EndGameStars(int numOfStars)
    {
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < numOfStars; i++)
        {
            stars[i].gameObject.SetActive(true);
            LeanTween.scale(stars[i], 1.3f * Vector2.one, .4f).setEasePunch();
            yield return new WaitForSeconds(.4f);
        }
        DeactivateTrails();
    }

    private IEnumerator ArrowCoroutine()
    {
        int l = 0, r = 0;
        while (true)
        {
            LeanTween.rotate(backStar, -10f, .2f);
            for(int i=2; i >=0; i--)
            {
                if (i == l) leftArrows[i].enabled = false;
                else leftArrows[i].enabled = true;
                if (i == r) rightArrows[i].enabled = false;
                else rightArrows[i].enabled = true;
            }
            l = l == 0 ? 2 : l-1;
            r = r == 0 ? 2 : r-1;
            yield return new WaitForSeconds(.2f);
        }
    }

    //private int OnWhichSide()
    //{
    //    targetDirection = closestPointOnPath - transform.position;
    //    crossProductForSideCalc = Vector3.Cross(targetDirection, transform.forward);
    //    dotProductForSideCalc = Vector3.Dot(crossProductForSideCalc, Vector3.up);
    //    if (dotProductForSideCalc > 0)      return 1;        
    //    else if (dotProductForSideCalc < 0) return -1;
    //    else                                 return 0;
    //}

    private void InitVariables()
    {
        selfRotateSmoothVelocity = 5;
        turnSmoothVelocity = 16;
        turnSmoothTime = 1.3f;
    }
}