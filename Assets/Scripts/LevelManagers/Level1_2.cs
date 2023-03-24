using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_2 : LevelManager
{
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;

    [Space(10)]
    public bool introComplete;

    [Header("Room 1")]
    public Transform room1Center;
    public Door room1HiddenDoor;
    public List<Spawner> room1Spawners;


    [Header("Room 2")]
    public LifeFlower lifeFlower2;
    public CleansingCrystal cleansingCrystal;
    public List<Spawner> room2Spawners;
    public float roomTimeCountdown = 180;

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
        gameConsole.NewMessage("Level 1.2.1");
        state = LevelState.ROOM1;

        #region [[ ROOM INTRO ]]
        playerMovement.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(lifeFlower.transform);
        yield return new WaitForSeconds(2);

        // << START FLOWER DECAY >>
        StartFlowerDecay(lifeFlower, 0.5f, flowerExclamation);

        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomInTarget(player.transform);

        yield return new WaitForSeconds(1);
        #endregion

        #region [[ ROOM 1 ]]

        camManager.state = CameraState.ROOM_BASED;
        playerMovement.state = PlayerState.IDLE;
        EnableSpawners(room1Spawners);

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
        gameConsole.NewMessage("Level 1.2.2");

        state = LevelState.ROOM2;
        #region [[ UNLOCK DARKLIGHT HALLWAY ]]
        lifeFlower.state = FlowerState.HEALED;
        playerMovement.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(lifeFlower.transform);
        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomOutTarget(room1Center.transform);
        yield return new WaitForSeconds(2);

        // destroy items
        playerInventory.Destroy();
        DestroySpawners(room1Spawners);

        // open hidden door
        room1HiddenDoor.locked = false;

        // shake camera
        camManager.ShakeCamera(2.5f, 0.2f);
        camManager.NewCustomZoomOutTarget(room1HiddenDoor.transform);
        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomInTarget(player.transform);
        yield return new WaitForSeconds(1);

        playerMovement.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;
        #endregion

        #region [[ INTRODUCE CLEANSING ]]
        yield return new WaitUntil(() => playerInventory.GetTypeCount(ItemType.DARKLIGHT) > 0);


        uiManager.NewGameTip("Darklight Orbs are the opposite of light orbs and will hurt your life flower.");
        yield return new WaitForSeconds(2);

        uiManager.DisableGameTip();
        yield return new WaitUntil(() => cleansingCrystal.playerInTrigger);


        // move player to center of rift
        playerMovement.state = PlayerState.INACTIVE;
        player.transform.position = cleansingCrystal.triggerParent.position;

        camManager.NewCustomTarget(cleansingCrystal.transform);

        yield return new WaitUntil(() => cleansingCrystal.itemConverted);
        yield return new WaitForSeconds(cleansingCrystal.conversionDelay + 1);

        camManager.state = CameraState.ROOM_BASED;
        playerMovement.state = PlayerState.IDLE;

        yield return new WaitUntil(() => playerInventory.GetTypeCount(ItemType.GOLDEN) > 0);
        #endregion

        #region [[ START ROOM 2 LOOP ]]

        playerMovement.state = PlayerState.INACTIVE;

        // focus on new flower
        camManager.NewCustomZoomOutTarget(lifeFlower2.transform);

        yield return new WaitForSeconds(1);

        // << START FLOWER DECAY >>
        StartFlowerDecay(lifeFlower2, 0.75f, flowerExclamation);
        EnableSpawners(room2Spawners);

        yield return new WaitForSeconds(2);

        camManager.state = CameraState.ROOM_BASED;
        playerMovement.state = PlayerState.IDLE;
        #endregion

        StartCountdown(roomTimeCountdown);

        // wait until time is up
        yield return new WaitUntil(() => (countdownTimer <= 0 || lifeFlower2.IsDead()));

        // if dead , exit routine
        if (lifeFlower2.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

        // else continue on
        else { StartCoroutine(CompletedLeveRoutine()); }
    }

    IEnumerator FailedLevelRoutine()
    {

        gameConsole.NewMessage("Level Failed");

        yield return null;
    }

    IEnumerator CompletedLeveRoutine()
    {
        gameConsole.NewMessage("Level Completed");

        yield return null;
    }

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
            spawner.StartSpawn();
        }
    }

    public void DestroySpawners(List<Spawner> spawners)
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
