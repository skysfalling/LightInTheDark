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
    public CleansingCrystal cleansingCrystal;
    public List<Spawner> room2Spawners;
    public float roomTimeCountdown = 180;
    public Door levelExitDoor;

    [Header("Script Lines")]
    public float messageDelay = 2;
    public string flowerExclamation = "OW!";
    public List<string> flower_lines1;
    public List<string> flower_lines2;
    public List<string> flower_lines3;

    public void Start()
    {
        currLifeFlower = lifeFlowers[0];
        currLifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay

        if (state == LevelState.INTRO) { StartCoroutine(Intro()); }
        else if (state == LevelState.ROOM2) { Room2SavePoint(); }

    }

    void Room2SavePoint()
    {
        player.transform.parent = null;
        player.transform.position = cleansingCrystal.riftSprite.transform.position;

        uiManager.StartTransitionFadeOut();

        StartCoroutine(Room2());
    }


    IEnumerator Intro(bool debug = false)
    {
        introComplete = false; // dont call coroutine again

        gameConsole.NewMessage("Intro");
        state = LevelState.INTRO;

        playerSpawn.StartSpawnRoutine(); ;

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
        player.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // << START FLOWER DECAY >>
        StartFlowerDecay(currLifeFlower, 0.5f, flowerExclamation);

        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomInTarget(player.transform);

        yield return new WaitForSeconds(1);
        #endregion

        #region [[ ROOM 1 ]]

        camManager.state = CameraState.ROOM_BASED;
        player.state = PlayerState.IDLE;
        EnableSpawners(room1Spawners);

        // flower starting lines
        currLifeFlower.console.MessageList(flower_lines1, messageDelay);

        // wait until flower is overflowing 
        yield return new WaitUntil(() => (currLifeFlower.IsOverflowing() || currLifeFlower.IsDead()) );

        // if dead , exit routine
        if (currLifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

        // else continue on
        else { StartCoroutine(Hallway()); }
        #endregion
    }

    IEnumerator Hallway()
    {
        #region [[ UNLOCK DARKLIGHT HALLWAY ]]
        currLifeFlower.state = FlowerState.HEALED;
        player.state = PlayerState.INACTIVE;

        camManager.NewCustomZoomInTarget(currLifeFlower.transform);
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

        player.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;
        #endregion

        #region [[ INTRODUCE CLEANSING ]]

        // << DARKLIGHT WITNES DIALOGUE >>
        yield return new WaitUntil(() => playerInventory.GetTypeCount(ItemType.DARKLIGHT) > 0);
        NewDialogue(dialogueManager.witness_darkLightTip, playerInventory.GetFirstItemOfType(ItemType.DARKLIGHT));
        player.state = PlayerState.INACTIVE;

        yield return new WaitUntil(() => !uiManager.inDialogue);
        player.state = PlayerState.IDLE;
        camManager.state = CameraState.ROOM_BASED;

        // << SHOW CLEANSING PROCESS >>
        yield return new WaitUntil(() => cleansingCrystal.playerInTrigger);

        // move player to center of rift
        player.state = PlayerState.INACTIVE;
        player.transform.position = cleansingCrystal.triggerParent.position;

        // focus on crystal
        camManager.NewCustomTarget(cleansingCrystal.transform);

        // wait until conversion is finished
        yield return new WaitUntil(() => cleansingCrystal.itemConverted);
        yield return new WaitForSeconds(cleansingCrystal.conversionDelay + 1);

        // let player move
        camManager.state = CameraState.ROOM_BASED;
        player.state = PlayerState.IDLE;

        // golden orb dialogue
        yield return new WaitUntil(() => playerInventory.GetTypeCount(ItemType.GOLDEN) > 0);
        NewDialogue(dialogueManager.witness_goldenOrbTip, playerInventory.GetFirstItemOfType(ItemType.GOLDEN));
        player.state = PlayerState.INACTIVE;
        yield return new WaitUntil(() => !uiManager.inDialogue);
        yield return new WaitForSeconds(0.5f);

        #endregion

        StartCoroutine(Room2());
    }

    IEnumerator Room2()
    {
        gameConsole.NewMessage("Level 1.2.2");

        state = LevelState.ROOM2;

        #region [[ ROOM 2 INTRO ]]
        player.state = PlayerState.INACTIVE;

        // << NEW LIFE FLOWER >>
        currLifeFlower = lifeFlowers[1];
        camManager.NewCustomZoomOutTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(1);

        // comment on flower
        string witnessStartcomment = dialogueManager.witness_start_1_3[0];
        NewDialogue(witnessStartcomment);
        yield return new WaitUntil(() => !uiManager.inDialogue);
        yield return new WaitForSeconds(1);

        // << START FLOWER DECAY >>
        StartFlowerDecay(currLifeFlower, 0.75f, flowerExclamation);
        currLifeFlower.decayActive = false;

        // continued comment on flower
        List<string> continuedComment = dialogueManager.witness_start_1_3;
        continuedComment.RemoveAt(0);
        NewDialogue(continuedComment, currLifeFlower.gameObject);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // move camera to player
        camManager.state = CameraState.ROOM_BASED;
        EnableSpawners(room2Spawners);

        yield return new WaitForSeconds(1);
        #endregion

        currLifeFlower.decayActive = true;
        player.state = PlayerState.IDLE;

        StartCountdown(roomTimeCountdown);
        StartCoroutine(DamageToLifeFlowerReaction());

        // wait until time is up
        yield return new WaitUntil(() => (countdownTimer <= 0 || currLifeFlower.IsDead()));

        // if dead , exit routine
        if (currLifeFlower.IsDead()) { StartCoroutine(FailedLevelRoutine()); }

        // else continue on
        else { StartCoroutine(CompletedLeveRoutine()); }
    }

    IEnumerator DamageToLifeFlowerReaction()
    {
        yield return new WaitUntil(() => currLifeFlower.darklightDamage == true);

        NewTimedDialogue(dialogueManager.witness_darklightSubmit, 2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        player.SetSlowed(5);
        NewTimedDialogue(dialogueManager.witness_startSoulPanic, 2);
        yield return new WaitUntil(() => !uiManager.inDialogue);

    }

    IEnumerator FailedLevelRoutine()
    {
        gameConsole.NewMessage("Level Failed");

        camManager.NewCustomZoomInTarget(currLifeFlower.transform);
        NewRandomDialogue(dialogueManager.witness_onFail);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        endGrabHand.canAttack = true;

        yield return new WaitUntil(() => endGrabHand.state == HandState.PLAYER_CAPTURED);

        state = LevelState.FAIL;
    }

    IEnumerator CompletedLeveRoutine()
    {
        gameConsole.NewMessage("Level Completed");
        player.Inactive();

        currLifeFlower.canSubmit = false;
        currLifeFlower.state = FlowerState.HEALED;

        // zoom in to flower
        camManager.NewCustomZoomInTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // zoom out of flower
        camManager.NewCustomZoomOutTarget(currLifeFlower.transform);
        yield return new WaitForSeconds(2);

        // destroy items
        playerInventory.Destroy();
        DestroySpawners(room2Spawners);
        yield return new WaitForSeconds(1);

        // new dialogue
        NewDialogue(dialogueManager.witness_end_1_3);
        yield return new WaitUntil(() => !uiManager.inDialogue);

        // open exit door
        levelExitDoor.locked = false;
        camManager.NewCustomZoomInTarget(levelExitDoor.transform);
        yield return new WaitForSeconds(2);

        camManager.state = CameraState.ROOM_BASED;
        player.Idle();

        yield return new WaitUntil(() => levelExitDoor.playerInTrigger);

        Debug.Log("Finished Level 1.2");
        state = LevelState.COMPLETE;

    }

}
