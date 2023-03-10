using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmitItemObject : MonoBehaviour
{
    LifeFlower lifeFlower;
    PlayerInventory player;

    public bool playerInTrigger = true;
    public float triggerSize = 2f;

    [Header("Submission")]
    public bool canSubmit;
    public float submitSpeed = 10; // how fast the submitted item moves

    [Header("Circle Object")]
    private List<GameObject> submissionOverflow = new List<GameObject>();
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 10f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        lifeFlower = GetComponent<LifeFlower>();

        canSubmit = true;
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        // << DRAIN PLAYER INVENTORY >>
        if (playerInTrigger && player.inventory.Count > 0)
        {

            foreach (GameObject item in player.inventory)
            {
                item.transform.parent = transform; // set parent
                
                // add to overflow
                submissionOverflow.Add(item); 
            }

            player.inventory.Clear();

        }





        // << OVERFLOW MANAGER >>
        if (submissionOverflow.Count > 0)
        {
            // circle overflow items
            CircleAroundTransform(submissionOverflow);

            if (canSubmit && !FlowerOverflow())
            {
                StartCoroutine(SubmitItem());
            }
        }


    }

    IEnumerator SubmitItem()
    {
        canSubmit = false;


        // remove from inventory
        GameObject item = submissionOverflow[0];
        submissionOverflow.Remove(item);

        // << MOVE ITEM TO CENTER >>
        while (item.transform.position != transform.position)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, submitSpeed * Time.deltaTime);
            yield return null;
        }


        // << SUBMIT ITEM >>
        submissionOverflow.Remove(item);

        // update values
        if (lifeFlower)
        {
           lifeFlower.lifeForce += item.GetComponent<PickupItem>().lifeForce;
        }

        // destroy item
        Destroy(item);

        yield return new WaitForSeconds(2);

        canSubmit = true;
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
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

    public bool FlowerOverflow()
    {
        if (lifeFlower && lifeFlower.overflowing) { return true; }

        return false;
    }


    public void CircleAroundTransform(List<GameObject> items)
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = transform.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < items.Count; i++)
        {
            float angleRadians = (currCircleAngle + (360f / items.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            items[i].transform.position = Vector3.Lerp(items[i].transform.position, newPos, Time.deltaTime); // Move object towards new position using Lerp
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerSize);
    }
}
