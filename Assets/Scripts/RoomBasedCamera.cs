using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBasedCamera : MonoBehaviour
{
    GameObject player;
    public List<GameObject> rooms;

    [Space(10)]
    public float camSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Room"));
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamToClosestRoom();

    }


    public void MoveCamToClosestRoom()
    {
        GameObject closestRoom = rooms[0];
        float closestDistance = Mathf.Infinity;

        foreach (GameObject room in rooms)
        {
            float dist = Vector3.Distance(player.transform.position, room.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestRoom = room;
            }
        }


        Vector3 target = new Vector3(closestRoom.transform.position.x, closestRoom.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, camSpeed * Time.deltaTime);
    }
}
