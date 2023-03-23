using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_2 : LevelManager
{
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;

    bool levelRoutineStarted;

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

    public override void LevelStateMachine()
    {
        if (state != LevelState.INTRO) { UpdateTimer(); }

        switch (state)
        {
            case (LevelState.INTRO):

                // start introduction routine b
                if (!levelRoutineStarted) { StartCoroutine(StartLevelRoutine()); }

                break;
        }
    }

    IEnumerator StartLevelRoutine()
    {
        levelRoutineStarted = true;
        lifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay

        state = LevelState.INTRO;
        #region [[ INTRO TO DECAY ROOM ]]
        // wait until spawned
        yield return new WaitUntil(() => playerSpawn.playerSpawned);
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

        camManager.state = CameraState.ROOM_BASED;
        playerMovement.state = PlayerState.IDLE;
        EnableSpawners();
        #endregion

        state = LevelState.START;
        #region [[ ROOM 1 ]]
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

        // open doors
        hiddenDoor1.locked = false;
        hiddenDoor2.locked = false;

        // shake camera
        camManager.ShakeCamera(hiddenDoor1.doorSpeed * 1.5f, 0.3f);
        camManager.NewCustomZoomOutTarget(hiddenDoor1.transform);
        yield return new WaitForSeconds(2);

        camManager.NewCustomZoomOutTarget(hiddenDoor2.transform);
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
