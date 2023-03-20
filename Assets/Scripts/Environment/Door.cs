using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool locked;

    [Space(10)]
    public Transform doorTrigger;
    public Vector2 doorTriggerSize = new Vector2(50, 50);
    public bool playerInTrigger;

    [Space(10)]
    public GameObject doorObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();


        if (!locked)
        {
            if (playerInTrigger) { doorObject.SetActive(false); }
            else { doorObject.SetActive(true); }
        }
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapBoxAll(doorTrigger.position, doorTriggerSize, 0);
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

    private void OnDrawGizmosSelected()
    {
        if (doorTrigger != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(doorTrigger.position, doorTriggerSize);
        }
    }
}
