using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroductionLevel : LevelManager
{
    public PlayerSpawn_Flower playerSpawn;

    bool introRoutineStarted;

    [Header("Script Lines")]
    public float messageDelay = 2;
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
                if (!introRoutineStarted) { StartCoroutine(IntroRoutine()); }

                break;

            case (LevelState.START):

                // set cam state
                if (camManager.state != CameraState.ROOM_BASED) { camManager.state = CameraState.ROOM_BASED; }
                break;

            default:
                break;

        }
    }

    IEnumerator IntroRoutine()
    {
        introRoutineStarted = true;
        lifeFlower.console.SetFullFadeDuration(messageDelay * 0.9f); // set the full fade duration of the text to less than message delay

        // wait until spawned
        yield return new WaitUntil(() => playerSpawn.playerSpawned);
        playerMovement.state = PlayerState.INACTIVE;
        camManager.state = CameraState.ROOM_BASED;

        // [[ LINES 1 ]]
        lifeFlower.console.MessageList(flower_lines1, messageDelay);
        yield return new WaitForSeconds(flower_lines1.Count * messageDelay); // wait for length of message list

        // let player move
        playerMovement.state = PlayerState.IDLE;

        // wait for player to pick up light orb
        while (playerInventory.GetTypeCount(ItemType.LIGHT) == 0)
        {
            yield return null;
        }

        // [[ LINES 2 ]]
        lifeFlower.console.MessageList(flower_lines2, messageDelay);
        yield return new WaitForSeconds(flower_lines2.Count * messageDelay); // wait for length of message list

        // wait for flower to be overflowing
        yield return new WaitUntil(() => lifeFlower.overflowing);

        // [[ LINES 3 ]]
        lifeFlower.console.MessageList(flower_lines3, messageDelay);
        yield return new WaitForSeconds(flower_lines3.Count * messageDelay); // wait for length of message list

        // level is complete
        state = LevelState.COMPLETE;

        // wait until player is inside life flower light
        while (Vector2.Distance(player.transform.position, lifeFlower.transform.position) > lifeFlower.healthyLightRadius)
        {
            yield return null;
        }

        playerMovement.state = PlayerState.INACTIVE;

        // << MOVE PLAYER TO SPAWN POINT >>
        while (Vector2.Distance(player.transform.position, lifeFlower.transform.position) > 2)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, lifeFlower.transform.position, Time.deltaTime);

            yield return null;
        }


    }
}
