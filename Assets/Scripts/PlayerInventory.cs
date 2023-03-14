using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerInventory : MonoBehaviour
{
    PlayerMovement movement;
    Light2D light;
    SoundManager soundManager;
    LevelManager levelManager;

    public List<GameObject> inventory;

    [Header("Follow Player")]
    public float speed = 30;
    public float spacing = 1;

    [Header("Circle Player")]
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 1f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle



    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        light = GetComponent<Light2D>();

        levelManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelManager>();
        soundManager = levelManager.soundManager;
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.state == PlayerState.MOVE_TO_TARGET) { InventoryFollowPlayer(); }
        else if (movement.state == PlayerState.IDLE) { InventoryCirclePlayer(); }

        // raise light intensity with more items
        light.intensity = (inventory.Count * 0.1f) + 0.7f;

    }

    public void NewItem(GameObject itemObject)
    {
        itemObject.transform.parent = transform;
        Item item = itemObject.GetComponent<Item>();

        inventory.Add(itemObject);

        Debug.Log("Player picked up " + this.gameObject, itemObject);


        // Play Sound
        if (item.type == ItemType.LIGHT)
        {
            soundManager.PlayRandomEffectFromList(soundManager.lightOrbPickups);
        }
        else if (item.type == ItemType.DARKLIGHT)
        {
            soundManager.Play(soundManager.darklightOrbPickup);
        }
        else if (item.type == ItemType.GOLDEN)
        {
            soundManager.Play(soundManager.goldenOrbPickup);
        }
        else if (item.type == ItemType.ETHEREAL)
        {
            soundManager.Play(soundManager.etherealOrbPickup);
        }

    }

    public void InventoryFollowPlayer()
    {
        Vector3 direction = movement.moveDirection; // get direction of target movement

        Vector3 prevFollowerPos = transform.position; // Set initial previous follower position to current position

        foreach (GameObject obj in inventory)
        {
            Vector3 newPos = prevFollowerPos - (direction * spacing); // Calculate new follower position in opposite direction of target movement
            obj.transform.position = Vector3.Lerp(obj.transform.position, newPos, speed * Time.deltaTime); // Move follower towards new position using Lerp

            prevFollowerPos = obj.transform.position; // Update previous follower position to current follower position
        }
    }

    public void InventoryCirclePlayer()
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = transform.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < inventory.Count; i++)
        {
            float angleRadians = (currCircleAngle + (360f / inventory.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            inventory[i].transform.position = Vector3.Lerp(inventory[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp
        }
    }


}
