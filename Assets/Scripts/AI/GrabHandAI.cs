using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandState { IDLE, TRACKING, ATTACK, GRAB, GRAB_BROKEN, PLAYER_CAPTURED }

public class GrabHandAI : MonoBehaviour
{
    PlayerMovement player;
    public HandState state = HandState.IDLE;

    public Transform handHome;

    [Header("Trigger")]
    public Transform triggerParent;
    public bool playerInTrigger;
    public float triggerSize = 5;

    [Header("Tracking")]
    public bool x_axis;
    public bool y_axis;
    public bool trackingStarted;
    [Space(5)]
    public float trackingSpeed;
    public float trackingTime;

    [Header("Attack")]
    public bool canAttack = true;
    public bool attackStarted;
    public float attackSpeed = 10;
    public float attackDelay = 1.5f;
    [Space(5)]
    public Vector2 attackPoint;
    public float attackPointRange = 5;

    [Header("Grab")]
    public bool grabStarted;
    public float grab_pullBackSpeed;
    public int breakFree_struggleCount = 4;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInTrigger = IsPlayerInTrigger();

        StateMachine();
    }


    void StateMachine()
    {

        switch (state)
        {
            case HandState.IDLE:

                if (playerInTrigger) { state = HandState.TRACKING; }
                else
                {
                    // move to home position
                    transform.position = Vector3.Lerp(transform.position, handHome.position, trackingSpeed * Time.deltaTime);
                }

                break;

            case HandState.TRACKING:

                if (!playerInTrigger) { state = HandState.IDLE; trackingStarted = false; }
                else if (!trackingStarted)
                {
                    StartCoroutine(Tracking());
                }

                break;

            case HandState.ATTACK:

                if (!attackStarted)
                {
                    StartCoroutine(Attacking(attackPoint, attackSpeed, attackPointRange));
                }

                break;

            case HandState.GRAB:

                if (!grabStarted)
                {
                    StartCoroutine(Grabbing());
                }

                break;

            default:
                break;
        }
    }

    IEnumerator Tracking()
    {
        float trackingTimer = 0f;
        trackingStarted = true;

        // continue following the player for the specified amount of time
        while (trackingTimer < trackingTime && playerInTrigger)
        {
            if (x_axis)
            {
                Vector3 targetPosition = new Vector3(player.transform.position.x, handHome.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, trackingSpeed * Time.deltaTime);
            }
            else if (y_axis)
            {
                Vector3 targetPosition = new Vector3(handHome.position.x, player.transform.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, trackingSpeed * Time.deltaTime);
            }

            trackingTimer += Time.deltaTime;

            attackPoint = player.transform.position;
            
            yield return null;

        }

        if (canAttack)
        {
            // ATTACK if player still in trigger
            state = HandState.ATTACK;
            trackingStarted = false;
        }
    }

    IEnumerator Attacking(Vector2 attackPoint, float attackSpeed, float attackPointRange)
    {
        attackStarted = true;

        // while hand is not at player position
        while (Vector2.Distance(transform.position, attackPoint) > 2)
        {
            transform.position = Vector3.Lerp(transform.position, attackPoint, attackSpeed * Time.deltaTime);

            yield return null;
        }


        // if player in attack point range , grab
        if (Vector2.Distance(player.transform.position, attackPoint) < attackPointRange)
        {
            state = HandState.GRAB;
            player.state = PlayerState.GRABBED;
        }
        else
        {
            state = HandState.IDLE;
        }

        // grab player
        attackStarted = false;
    }

    IEnumerator Grabbing()
    {
        grabStarted = true;
        player.transform.parent = transform;
        player.transform.position = transform.position;

        // move hand back to home position AND player hasn't broken free
        while (Vector2.Distance(transform.position, handHome.position) > 5 && player.struggleCount < breakFree_struggleCount)
        {
            transform.position = Vector3.Lerp(transform.position, handHome.position, grab_pullBackSpeed * Time.deltaTime);

            yield return null;
        }

        // if broken free, release player
        if (player.struggleCount >= breakFree_struggleCount)
        {
            player.transform.parent = null;
            player.state = PlayerState.IDLE;

            state = HandState.IDLE;
        }
        else
        {
            state = HandState.PLAYER_CAPTURED;
        }


        grabStarted = false;


    }

    public void OverrideAttackPlayer(float attackSpeed, float attackRange)
    {
        attackStarted = true;

        // ATTACK if player still in trigger
        state = HandState.ATTACK;
        trackingStarted = false;

        StartCoroutine(Attacking(player.transform.position, attackSpeed, attackRange));
    }

    public void DoomAttackPlayer()
    {
        attackStarted = true;

        // ATTACK if player still in trigger
        state = HandState.ATTACK;
        trackingStarted = false;

        StartCoroutine(Attacking(player.transform.position, 10, 100));

        grab_pullBackSpeed *= 2;
        breakFree_struggleCount = 9999;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (triggerParent != null)
        {
            Gizmos.DrawWireSphere(triggerParent.position, triggerSize);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint, attackPointRange);

    }
}