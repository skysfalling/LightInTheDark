using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_1 : LevelManager
{
    public PlayerSpawn_Hand playerSpawn;
    public GrabHandAI endGrabHand;

    bool levelRoutineStarted;

    [Header("Script Lines")]
    public float messageDelay = 2;
    public List<string> flower_lines1;
    public List<string> flower_lines2;
    public List<string> flower_lines3;

    public override void LevelStateMachine()
    {
        if (state != LevelState.INTRO) { UpdateGameClock(); }

        switch (state)
        {
            case (LevelState.INTRO):

                // start introduction routine b
                if (!levelRoutineStarted) { StartCoroutine(LevelRoutine()); }

                break;
        }
    }

    IEnumerator LevelRoutine()
    {
        levelRoutineStarted = true;
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
        gameConsole.NewMessage("use WASD to move , soul");

        // wait for player to pick up light orb
        while (playerInventory.GetTypeCount(ItemType.LIGHT) == 0)
        {
            yield return null;
        }

        // [[ LINES 2 ]]
        lifeFlower.console.MessageList(flower_lines2, messageDelay);
        yield return new WaitForSeconds(flower_lines2.Count * messageDelay); // wait for length of message list

        // wait for flower to be overflowing
        yield return new WaitUntil(() => lifeFlower.state == FlowerState.OVERFLOWING);

        // [[ LINES 3 ]]
        lifeFlower.console.MessageList(flower_lines3, messageDelay);
        yield return new WaitForSeconds(flower_lines3.Count * messageDelay); // wait for length of message list

        // level is complete
        state = LevelState.COMPLETE;
        lifeFlower.state = FlowerState.HEALED;

        yield return new WaitForSeconds(3);

        endGrabHand.canAttack = true;

        yield return new WaitUntil(() => endGrabHand.state == HandState.PLAYER_CAPTURED);


    }
}
