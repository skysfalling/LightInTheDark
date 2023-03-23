using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LevelState { INTRO, START, PHASE1, PHASE2, FAIL, COMPLETE }
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
    public LevelState state = LevelState.START;

    [Space(10)]
    public LifeFlower lifeFlower;

    [Space(10)]
    public float currTime;
    float startTime;

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

    public void Start()
    {
        // gameConsole.NewMessage("---->> the flower begins to wilt <<", Color.grey);
    }

    // Update is called once per frame
    public void Update()
    {
        LevelStateMachine();
    }


    public virtual void LevelStateMachine()
    {
        
        if (state != LevelState.INTRO) { UpdateTimer(); }

        switch(state)
        {
            case (LevelState.START):
                // set cam state
                if (camManager.state != CameraState.ROOM_BASED) { camManager.state = CameraState.ROOM_BASED; }
                break;

            default:
                break;

        }
    }

    #region HELPER FUNCTIONS ==========
    public void UpdateTimer()
    {
        float timePassed = Time.time - startTime;
        currTime =  Mathf.Round(timePassed * 10) / 10f;
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
