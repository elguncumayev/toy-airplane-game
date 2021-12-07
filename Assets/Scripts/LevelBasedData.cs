using PathCreation;
using UnityEngine;
using Cinemachine;

public class LevelBasedData : MonoBehaviour
{
    #region Singleton
    private static LevelBasedData _instance;
    public static LevelBasedData Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    public PathCreator pathCreatorTrackPoint;
    public PathCreator pathCreatorCamera;
    public CinemachineVirtualCamera startCamera;
    public CinemachineVirtualCamera endCamera;

    public Transform playerInitialPoint;

    public int coinsInLevel;
}
