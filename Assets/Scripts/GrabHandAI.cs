using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandState { IDLE, TRACKING, ATTACK, GRAB }

public class GrabHandAI : MonoBehaviour
{
    PlayerMovement player;
    public HandState state = HandState.IDLE;

    [Space(10)]
    public Transform triggerParent;
    public bool playerInTrigger;
    public float triggerSize = 5;

    [Header("Tracking")]
    public bool x_axis;
    public bool y_axis;
    public bool trackingStarted;
    [Space(5)]
    public float trackingSpeed;
    public float attackDelay;

    [Header("Attack")]
    public bool canAttack;
    public float attackSpeed;


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

                break;

            case HandState.TRACKING:

                if (!playerInTrigger) { state = HandState.IDLE; trackingStarted = false; }
                else if (!trackingStarted)
                {
                    StartCoroutine(Tracking());
                }

                break;

            default:
                break;
        }
    }

    IEnumerator Tracking()
    {
        float trackingTimer = 0f;

        // continue following the player for the specified amount of time
        while (trackingTimer < attackDelay && playerInTrigger)
        {
            Vector3 targetPosition = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, trackingSpeed * Time.deltaTime);

            trackingTimer += Time.deltaTime;
            yield return null;
        }

        // ATTACK if player still in trigger
        state = HandState.ATTACK;
        trackingStarted = false;

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

    }
}
