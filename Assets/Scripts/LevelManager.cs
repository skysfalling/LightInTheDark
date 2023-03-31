using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LevelState { INTRO, ROOM1, ROOM2, FAIL, COMPLETE }
public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public GameManager gameManager;
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public SoundManager soundManager;
    [HideInInspector]
    public PlayerMovement player;
    [HideInInspector]
    public PlayerInventory playerInventory;
    [HideInInspector]
    public PlayerAnimator playerAnim;
    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public CameraManager camManager;
    [HideInInspector]
    public DialogueManager dialogueManager;

    [Header("Game Values")]
    public LevelState state = LevelState.INTRO;

    [Space(10)]
    public LifeFlower currLifeFlower;
    public List<LifeFlower> lifeFlowers;

    [Header("Timer")]
    public float gameClock;
    float startTime;

    [Header("Countdown Timer")]
    public bool countdownStarted;
    public float countdownTimer;

    [Header("Spawn")]
    public Transform camStart;
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;


    public void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        uiManager = gameManager.GetComponentInChildren<UIManager>();
        camManager = gameManager.GetComponentInChildren<CameraManager>();
        gameConsole = gameManager.gameConsole;
        soundManager = gameManager.soundManager;
        dialogueManager = gameManager.dialogueManager;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerInventory = player.gameObject.GetComponent<PlayerInventory>();
        playerAnim = player.gameObject.GetComponent<PlayerAnimator>();

        startTime = Time.time;


    }

    // Update is called once per frame
    public virtual void Update()
    {
        LevelStateMachine();

        if (state != LevelState.INTRO) { UpdateGameClock(); }

        if (countdownStarted && countdownTimer > 0) { UpdateCountdown(); }
    }

    public virtual void StartLevelFromPoint(LevelState state)
    {
        Debug.Log("Start Level From Point is not set up");
    }

    public virtual void LevelStateMachine()
    {
        


    }

    #region << DIALOGUE >>
    public void NewDialogue(string dialogue)
    {
        uiManager.NewDialogue(dialogue);
    }

    public void NewDialogue(List<string> dialogue)
    {
        uiManager.NewDialogue(dialogue);
    }

    public void NewRandomDialogue(List<string> dialogue)
    {
        uiManager.NewDialogue(dialogue[Random.Range(0, dialogue.Count)]);
    }

    public void NewTimedDialogue(List<string> dialogue, float sentenceDelay)
    {
        uiManager.TimedDialogue(dialogue, sentenceDelay);
    }

    public void NewDialogue(string dialogue, GameObject focusObject)
    {
        camManager.NewGameTipTarget(focusObject.transform);
        uiManager.NewDialogue(dialogue);
    }

    public void NewDialogue(List<string> dialogue, GameObject focusObject)
    {
        camManager.NewGameTipTarget(focusObject.transform);
        uiManager.NewDialogue(dialogue);
    }

    public void NewRandomDialogue(List<string> dialogue, GameObject focusObject)
    {
        uiManager.NewDialogue(dialogue[Random.Range(0, dialogue.Count)]);
        camManager.NewGameTipTarget(focusObject.transform);
    }
    #endregion

    public void StartFlowerDecay(LifeFlower lifeFlower, float healthPercent = 0.5f, string exclamation = "OW!")
    {
        lifeFlower.decayActive = true; // start decay
        lifeFlower.lifeForce = Mathf.FloorToInt(lifeFlower.maxLifeForce * healthPercent);
        lifeFlower.DamageReaction();
    }

    public void EnableSpawners(List<Spawner> spawners)
    {
        foreach (Spawner spawner in spawners)
        {
            if (spawner == null) { continue; }
            spawner.StartSpawn();
        }
    }

    public void DestroySpawners(List<Spawner> spawners)
    {
        foreach (Spawner spawner in spawners)
        {
            if (spawner.spawnedObject)
            {
                spawner.DestroySpawnedObject();
            }
        }
    }

    public void UpdateGameClock()
    {
        float timePassed = Time.time - startTime;
        gameClock =  Mathf.Round(timePassed * 10) / 10f;
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

    public void StopCountdown()
    {
        countdownStarted = false;
    }

    public float GetCurrentCountdown()
    {
        return  Mathf.Round(countdownTimer * 10) / 10f;
    }

    public bool CountdownOver()
    {
        return countdownTimer <= 0;
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

}
