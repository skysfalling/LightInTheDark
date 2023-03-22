using System.Collections.Generic;
using UnityEngine;


public enum CameraState { NONE, PLAYER, ROOM_BASED, CUSTOM_TARGET }
public class CameraManager : MonoBehaviour
{
    LevelManager levelManager;
    Transform player;
    List<GameObject> rooms;

    public CameraState state = CameraState.NONE;
    public Transform currTarget;
    public float camSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GetComponentInParent<LevelManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Room"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CameraStateMachine();
    }

    void CameraStateMachine()
    {
        switch(state)
        {
            case (CameraState.PLAYER):

                FollowTarget(player);

                break;
            case (CameraState.ROOM_BASED):

                MoveCamToClosestRoom();

                break;
            case (CameraState.CUSTOM_TARGET):
                
                FollowTarget(currTarget);

                break;


        }
    }

    public void MoveCamToClosestRoom()
    {
        GameObject closestRoom = rooms[0];
        float closestDistance = Mathf.Infinity;

        foreach (GameObject room in rooms)
        {
            float dist = Vector3.Distance(player.position, room.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestRoom = room;
            }
        }

        Vector3 target = new Vector3(closestRoom.transform.position.x, closestRoom.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, camSpeed * Time.deltaTime);
    }

    public void FollowTarget(Transform newTarget)
    {
        currTarget = newTarget;

        Vector3 targetPos = new Vector3(newTarget.transform.position.x, newTarget.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, camSpeed * Time.deltaTime);
    }

    public void NewCustomTarget(Transform customTarget)
    {
        state = CameraState.CUSTOM_TARGET;
        currTarget = customTarget;
    }
}
