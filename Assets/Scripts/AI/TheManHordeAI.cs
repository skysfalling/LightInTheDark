using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheManHordeAI : MonoBehaviour
{
    public GameObject theManPrefab;




    [Space(10)]
    public bool playerInTrigger;
    public Transform triggerParent;
    public Vector2 triggerSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();
    }

    public bool IsPlayerInTrigger()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapBoxAll(triggerParent.position, triggerSize, 0);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

        foreach (Collider2D col in collidersInTrigger)
        {
            if (col.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (triggerParent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(triggerParent.position, triggerSize);
        }
    }
}
