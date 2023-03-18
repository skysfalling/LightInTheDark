using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandState { IDLE, TRACKING, ATTACK, GRAB }

public class GrabHandAI : MonoBehaviour
{
    public Transform triggerParent;
    public bool playerInTrigger;
    public float triggerSize = 5;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
