using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_2 : LevelManager
{
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;

    private bool introComplete;
    private bool gameStarted;

    public List<Spawner> spawners;

    [Space(10)]
    public Transform room1Center;

    [Space(10)]
    public Door hiddenDoor1;
    public Door hiddenDoor2;

    [Header("Script Lines")]
    public float messageDelay = 2;
    public string flowerExclamation = "OW!";
    public List<string> flower_lines1;
    public List<string> flower_lines2;
    public List<string> flower_lines3;

    public void Start()
    {
        lifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay

        if (state == LevelState.INTRO) { StartCoroutine(Intro()); }
        else if (state == LevelState.ROOM2) { StartCoroutine(DebugRoom2()); }


    }

    public override void LevelStateMachine()
    {
        if (state != LevelState.INTRO) { UpdateTimer(); }

    }

    IEnumerator DebugRoom2()
    {
        StartCoroutine(Intro(true));

        gameConsole.NewMessage("debug rm2");

        yield return new WaitUntil(() => introComplete);

        StartCoroutine(Room2());
    }


    IEnumerator Intro(bool debug = false)
    {
        introComplete = false; // dont call coroutine again

        gameConsole.NewMessage("Intro");
        state = LevelState.INTRO;

        // wait until spawned
        yield return new WaitUntil(() => playerSpawn.playerSpawned);

        introComplete = true;

        // if not in debug mode, continue to next room
        if (!debug)
        {
            StartCoroutine(Room1());
        }
    }


    IEnumerator Room1()
    {
        gameConsole.NewMessage("Level 1.2");
        state = LevelState.ROOM1;

        #region [[ ROOM INTRO ]]
        playerMovement.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(lifeFlower.transform);
        yield return new WaitForSeconds(2);

        // << SCARE PLAYER BY STARTING FLOWER DECAY >>
        lifeFlower.decayActive = true; // start decay
        lifeFlower.lifeForce = lifeFlower.maxLifeForce / 2;
        lifeFlower.anim.SpawnAggressiveBurstEffect();
        camManager.ShakeCamera();
        lifeFlower.console.NewMessage(flowerExclamation);

        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomInTarget(player.transform);

        yield return new WaitForSeconds(1);
        #endregion

        #region [[ ROOM 1 ]]

        camManager.state = CameraState.ROOM_BASED;
        playerMovement.state = PlayerState.IDLE;
        EnableSpawners();

        // flower starting lines
        lifeFlower.console.MessageList(flower_lines1, messageDelay);

        // wait until flower is overflowing 
        yield return new WaitUntil(() => ( lifeFlower.IsOverflowing() || lifeFlower.IsDead()) );

        // if dead , exit routine
        if (lifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

        // else continue on
        else { StartCoroutine(Room2()); }
        #endregion
    }

    IEnumerator Room2()
    {
        gameConsole.NewMessage("Level 1.3");

        state = LevelState.ROOM2;
        #region [[ UNLOCK ROOM 2 ]]
        lifeFlower.state = FlowerState.HEALED;
        playerMovement.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(lifeFlower.transform);
        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomOutTarget(room1Center.transform);
        yield return new WaitForSeconds(2);

        // destroy items
        playerInventory.Destroy();
        DestroySpawners();

        // open hidden door
        hiddenDoor1.locked = false;

        // shake camera
        camManager.ShakeCamera(hiddenDoor1.doorSpeed, 0.2f);
        camManager.NewCustomZoomOutTarget(hiddenDoor1.transform);
        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomInTarget(player.transform);
        yield return new WaitForSeconds(3);

        playerMovement.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;
        #endregion


    }

    IEnumerator FailedLevelRoutine()
    {
        yield return null;
    }

    public void EnableSpawners()
    {
        foreach (Spawner spawner in spawners)
        {
            spawner.StartSpawn();
        }
    }

    public void DestroySpawners()
    {
        foreach (Spawner spawner in spawners)
        {
            if (spawner.spawnedObject)
            {
                spawner.DestroyItem();
            }
        }
    }
}
