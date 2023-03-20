using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GremlinState
{
    IDLE,
    CHASE_PLAYER,
    STUN_PLAYER,
    TARGET_ITEM,
    TARGET_SAFE_ZONE,
    STEAL_ITEM,
    STUNNED
}

public class GremlinAI : MonoBehaviour
{
    
    Transform player;
    Animator anim;
    Rigidbody2D rb;

    public GremlinState state = GremlinState.IDLE;
    public float moveSpeed = 5f;
    public float distToPlayer;

    [Header("Target Values")]
    public float interactionRadius = 3f;
    public float chaseRadius = 5f;

    [Space(10)]
    public float stunAmount = 2;

    [Header("Item Holder")]
    public Transform itemHolderParent;
    public GameObject heldItemObj;
    public float carryMoveSpeed;

    [Header("Safe Zone")]
    public Transform safeZone;
    public float safeZoneRadius = 5f;

    [Header("Idle Zone")]
    public bool idleMoveStarted;
    public Transform idleZone;
    public float idleZoneRadius = 5f;
    public float idle_delay = 2;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        StateMachine();

        // move current held object to holder position
        if (heldItemObj)
        {
            heldItemObj.transform.position = Vector3.Lerp(heldItemObj.transform.position, itemHolderParent.position, carryMoveSpeed * Time.deltaTime); // Move follower towards new position using Lerp
        }

    }

    private void StateMachine()
    {
        // get distance from player
        distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case GremlinState.IDLE:

                anim.SetBool("idle", true);

                // start coroutine
                if (!idleMoveStarted) { StartCoroutine(IdleRoutine()); }


                // target item
                if (GetClosestThrownItem() != null)
                {
                    state = GremlinState.TARGET_ITEM;
                    idleMoveStarted = false;
                }


                // << CHASE RANGE >>
                if (distToPlayer < chaseRadius)
                {
                    state = GremlinState.CHASE_PLAYER;
                    anim.SetBool("idle", false);

                }
                break;
            case GremlinState.CHASE_PLAYER:

                anim.SetBool("chase player", true);

                // << CHASE RANGE >>
                if (distToPlayer < chaseRadius)
                {
                    // target item if found
                    if (GetClosestThrownItem() != null)
                    {
                        state = GremlinState.TARGET_ITEM;
                        idleMoveStarted = false;
                    }

                    // move gremlin
                    MoveTowardsTarget(player);

                    // << ATTACK RANGE >>
                    if (distToPlayer < interactionRadius)
                    {
                        state = GremlinState.STUN_PLAYER;
                        anim.SetBool("chase player", false);
                    }
                }
                else
                {
                    // << IDLE RANGE >>
                    state = GremlinState.IDLE;
                    anim.SetBool("chase player", false);

                }
                break;

            case GremlinState.STUN_PLAYER:
                player.GetComponent<PlayerMovement>().Stun(stunAmount);
                state = GremlinState.STEAL_ITEM;
                break;

            case GremlinState.STEAL_ITEM:

                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                GameObject targetItem = inventory.GetMostExpensiveItem();

                anim.SetBool("chase player", true);


                // if no target item, run away
                if (targetItem == null)
                {
                    state = GremlinState.TARGET_ITEM;
                    anim.SetBool("chase player", false);

                    break;
                }

                // else move towards target
                MoveTowardsTarget(targetItem.transform);

                // pickup item
                // << ATTACK RANGE >>
                if (Vector3.Distance(transform.position, targetItem.transform.position) < interactionRadius)
                {
                    inventory.StealItem(targetItem.gameObject);

                    targetItem.GetComponent<Item>().state = ItemState.STOLEN;


                    targetItem.transform.parent = itemHolderParent;
                    heldItemObj = targetItem.gameObject;

                    state = GremlinState.TARGET_SAFE_ZONE;

                }

                break;

            case GremlinState.TARGET_ITEM:

                inventory = player.GetComponent<PlayerInventory>();
                Transform targetItemTransform = GetClosestThrownItem();

                anim.SetBool("chase player", true);


                // if no target item, run away
                if (targetItemTransform == null) 
                {
                    state = GremlinState.TARGET_SAFE_ZONE;
                    anim.SetBool("chase player", false);

                    break;
                }

                // else move towards target
                MoveTowardsTarget(targetItemTransform);

                // pickup item
                // << ATTACK RANGE >>
                if (Vector3.Distance(transform.position, targetItemTransform.transform.position) < interactionRadius)
                {
                    inventory.StealItem(targetItemTransform.gameObject);

                    targetItemTransform.GetComponent<Item>().state = ItemState.STOLEN;


                    targetItemTransform.parent = itemHolderParent;
                    heldItemObj = targetItemTransform.gameObject;

                    state = GremlinState.TARGET_SAFE_ZONE;

                }


                break;
            case GremlinState.TARGET_SAFE_ZONE:
                // get random point in safe zone
                Vector3 safePoint = safeZone.position + (Vector3)Random.insideUnitCircle * safeZoneRadius;
                MoveTowardsTarget(safePoint);

                // << SAFE RANGE >>
                if (Vector3.Distance(transform.position, safePoint) < interactionRadius)
                {
                    DropItem();
                    anim.SetBool("chase player", false);

                    state = GremlinState.IDLE;
                }

                break;
            default:
                // Invalid state.
                break;
        }
    }

    private IEnumerator IdleRoutine()
    {
        idleMoveStarted = true;

        // get random point in safe zone
        Vector3 idlePoint = idleZone.position + (Vector3)Random.insideUnitCircle * idleZoneRadius;

        // move to point
        while (Vector2.Distance(transform.position, idlePoint) > interactionRadius && state == GremlinState.IDLE)
        {
            MoveTowardsTarget(idlePoint);
            yield return null;
        }


        // delay
        yield return new WaitForSeconds(idle_delay);


        idleMoveStarted = false;
    }


    private void MoveTowardsTarget(Transform target)
    {
        if (transform == null) { Debug.LogWarning("Cannot move to null transform"); return; }

        Vector3 newDirection = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        rb.MovePosition(newDirection);

        if (target.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 newDirection = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        rb.MovePosition(newDirection);

        if (target.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public void DropItem()
    {
        if (heldItemObj != null)
        {
            Item item = heldItemObj.GetComponent<Item>();
            item.transform.parent = null;
            item.state = ItemState.FREE;

            heldItemObj = null;

        }
    }

    public Transform GetClosestThrownItem()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, chaseRadius);
        List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);



        Transform targetItem = null;
        foreach (Collider2D col in collidersInTrigger)
        {
            // if free item
            if (col.tag == "Item" && col.GetComponent<Item>().state == ItemState.THROWN)
            {
                // if no target item or item is closer than curr target item
                if (targetItem == null || Vector2.Distance(transform.position , col.transform.position) < Vector2.Distance(transform.position, targetItem.transform.position))
                {
                    targetItem = col.transform;
                }
            }
        }

        return targetItem;

    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.DrawWireSphere(safeZone.position, safeZoneRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(idleZone.position, idleZoneRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

    }
}
