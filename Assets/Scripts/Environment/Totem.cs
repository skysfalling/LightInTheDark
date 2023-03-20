using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Totem : MonoBehaviour
{
    [HideInInspector]
    public PlayerInventory player;

    public Transform triggerParent;
    public bool playerInTrigger;
    public bool playerIsCollecting;
    public float triggerSize = 2f;

    [Space(10)]
    public bool playerInCenter;
    public float playerCenterRange = 2;

    [Header("Submission")]
    public List<ItemType> submissionTypes;
    [Space(10)]
    public List<GameObject> submissionOverflow = new List<GameObject>();

    [Header("Circle Object")]
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 10f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle

    private void Start()
    {
        // << INIT VALUES >>
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
    }


    // Update is called once per frame 
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();


        // if player is in center, collect all items
        if (Vector2.Distance(player.transform.position, triggerParent.position) < playerCenterRange)
        {
            playerIsCollecting = true;

            // move all submission objects to player
            foreach (GameObject obj in submissionOverflow)
            {
                obj.transform.parent = null;

                player.AddItemToInventory(obj);
            }

            submissionOverflow.Clear();
        }
        else
        {
            SubmissionManager();
        }

    }

    public void SubmissionManager()
    {
        // << DRAIN PLAYERS ENTIRE INVENTORY >>
        if (playerInTrigger && player.inventory.Count > 0 && !playerIsCollecting)
        {
            List<GameObject> inventory = player.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                // if item type is allowed
                if (submissionTypes.Contains(inventory[i].GetComponent<Item>().type) && !submissionOverflow.Contains(inventory[i]))
                {
                    // add to overflow
                    submissionOverflow.Add(inventory[i]);

                    // change item state
                    inventory[i].GetComponent<Item>().state = ItemState.FREE;

                    player.RemoveItem(inventory[i]);
                }
            }
        }
        else if (playerInTrigger && player.inventory.Count <= 0)
        {
            playerIsCollecting = true;
        }
        else if (!playerInTrigger)
        {
            playerIsCollecting = false;
        }

        // << REMOVE NOT FREE STATE ITEMS >>
        for (int i = 0; i < submissionOverflow.Count; i++)
        {
            if (submissionOverflow[i].GetComponent<Item>().state != ItemState.FREE)
            {
                submissionOverflow.Remove(submissionOverflow[i]);
            }
        }

        // collect all free items in trigger
        CollectFreeItemsInTrigger();

        // << SUBMISSION OVERFLOW MANAGER >>
        if (submissionOverflow.Count > 0)
        {
            // circle overflow items
            CircleAroundTransform(submissionOverflow);
        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(triggerParent.position, triggerSize);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            if (col.tag == "Player")
            {
                return true;
            }
        }

        return false;
    }

    public void CollectFreeItemsInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(triggerParent.position, triggerSize);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            // if free item

            if (col.tag == "Item" && col.GetComponent<Item>() )
            {
                Item item = col.GetComponent<Item>();

                // if not in submission overflow
                if (!submissionOverflow.Contains(col.gameObject) &&
                    (col.GetComponent<Item>().state == ItemState.FREE || col.GetComponent<Item>().state == ItemState.THROWN))
                {
                    // add to overflow
                    submissionOverflow.Add(col.gameObject);
                    col.GetComponent<Item>().state = ItemState.FREE;

                    if (player.inventory.Contains(col.gameObject)) { player.inventory.Remove(col.gameObject); }
                }
            }
        }
    }

    public void CircleAroundTransform(List<GameObject> items)
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = triggerParent.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < items.Count; i++)
        {

            items[i].transform.parent = triggerParent;

            float angleRadians = (currCircleAngle + (360f / items.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            items[i].transform.position = Vector3.Lerp(items[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (triggerParent != null)
        {
            Gizmos.DrawWireSphere(triggerParent.position, triggerSize);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, triggerSize);
        }

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(triggerParent.position, playerCenterRange);

    }
}
