using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { LIGHT, DARKLIGHT, GOLDEN, ETHEREAL}
public enum ItemState { FREE, PLAYER_INVENTORY, SUBMITTED, STOLEN }


public class Item : MonoBehaviour
{
    Rigidbody2D rb;


    public ItemType type;
    public ItemState state = ItemState.FREE;
    public float triggerSize = 0.75f;
    public int lifeForce = 10;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == ItemState.FREE)
        {
            Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
            List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

            foreach (Collider2D col in collidersInTrigger)
            {
                if (col.tag == "Player")
                {
                    col.GetComponent<PlayerInventory>().NewItem(this.gameObject);


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
