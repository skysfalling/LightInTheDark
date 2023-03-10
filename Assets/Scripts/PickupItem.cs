using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemState { FLOOR, PLAYER_INVENTORY, SUBMITTED }


public class PickupItem : MonoBehaviour
{
    public ItemState state = ItemState.FLOOR;
    public float triggerSize = 0.75f;

    public int lifeForce = 10;

    // Update is called once per frame
    void Update()
    {
        if (state == ItemState.FLOOR)
        {
            Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
            List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

            foreach (Collider2D col in collidersInTrigger)
            {
                if (col.tag == "Player")
                {
                    col.GetComponent<PlayerInventory>().inventory.Add(this.gameObject);
                    transform.parent = col.transform;

                    Debug.Log("Player picked up " + this.gameObject, gameObject);

                    state = ItemState.PLAYER_INVENTORY;
                }
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerSize);
    }
}
