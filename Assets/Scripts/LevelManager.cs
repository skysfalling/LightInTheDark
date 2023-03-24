using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LevelState { INTRO, ROOM1, ROOM2, FAIL, COMPLETE }
public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public SoundManager soundManager;
    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public PlayerMovement playerMovement;
    [HideInInspector]
    public PlayerInventory playerInventory;
    [HideInInspector]
    public PlayerAnimator playerAnim;
    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public CameraManager camManager;

    [Header("Game Values")]
    public LevelState state = LevelState.INTRO;

    [Space(10)]
    public LifeFlower lifeFlower;

    [Header("Timer")]
    public float gameTime;
    float startTime;

    [Header("Countdown Timer")]
    public bool countdownStarted;
    public float countdownTimer;


    public void Awake()
    {
        gameConsole = GetComponent<GameConsole>();
        soundManager = GetComponentInChildren<SoundManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerInventory = player.GetComponent<PlayerInventory>();
        playerAnim = player.GetComponent<PlayerAnimator>();
        uiManager = GetComponent<UIManager>();
        camManager = GetComponentInChildren<CameraManager>();

        startTime = Time.time;
    }

    // Update is called once per frame
    public void Update()
    {
        LevelStateMachine();

        if (state != LevelState.INTRO) { UpdateGameClock(); }

        if (countdownStarted && countdownTimer > 0) { UpdateCountdown(); }


    }

    public virtual void LevelStateMachine()
    {
        


    }

    #region HELPER FUNCTIONS ==========
    public void UpdateGameClock()
    {
        float timePassed = Time.time - startTime;
        gameTime =  Mathf.Round(timePassed * 10) / 10f;
    }

    public void StartCountdown(float count)
    {
        countdownTimer = count;
        countdownStarted = true;
    }

    public void UpdateCountdown()
    {
        countdownTimer -= Time.deltaTime;
    }

    public bool IsEndOfLevel()
    {
        if (state == LevelState.FAIL || state == LevelState.COMPLETE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
