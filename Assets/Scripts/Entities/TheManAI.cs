using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TheManState { IDLE , CHASE , RETREAT , GRABBED_PLAYER , PLAYER_CAPTURED }

public class TheManAI : MonoBehaviour
{
    PlayerInventory player;
    [HideInInspector]
    public PlayerMovement playerMovement;
    Rigidbody2D rb;
    public SpriteRenderer head;
    public SpriteRenderer body;
    public GameObject deathEffect;

    [Space(10)]
    public TheManState state = TheManState.IDLE;

    [Space(10)]
    public bool playerInOuterTrigger;
    public float outerTriggerSize = 25f;
    [Space(5)]
    public bool playerInInnerTrigger;
    public float innerTriggerSize = 10f;

    [Header("Light Decay")] // if the player gets close, the man slowly starts to "melt" from the light
    public Color startColor = Color.white;
    public Color endColor = Color.grey;
    
    [Space(5)]
    public int cur_health = 5;
    public int max_health = 5;
    private bool lifeCoroutineStarted;
    private bool lifeRestoreStarted;

    [Space(5)]
    int decayAmount = 1;
    public int lightOrbCountNeeded = 3; // light orbs needed in player inventory to start decay
    public float lifeDecayDelay = 1;

    [Header("Move")]
    public float chaseSpeed = 0.2f;
    public float retreatSpeed = 0.4f;
    public float grabSpeed = 0.6f;

    [Space(10)]
    public float timeToGrab = 2;
    private float timeToGrabTimer;
    public float timeToCapture = 5;
    private float timeToCaptureTimer;
    public float grabDelay = 2; // delay next grab



    public bool grabStarted;
    public int breakFree_struggleCount;
    private Vector2 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        playerMovement = player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        playerInOuterTrigger = IsPlayerInTrigger(outerTriggerSize);
        playerInInnerTrigger = IsPlayerInTrigger(innerTriggerSize);

        // << UPDATE LIFE VALUES BASED ON PLAYER DISTANCE AND INVENTORY COUNT >>
        if (!lifeCoroutineStarted)
        {
            if (player.inventory.Count >= lightOrbCountNeeded )
            {
                // inner trigger decay
                if (playerInInnerTrigger) { StartCoroutine(DrainLife(lifeDecayDelay)); }
            }
            else if (player.inventory.Count >= lightOrbCountNeeded * 2)
            {
                // inner trigger decays twice as fast
                if (playerInInnerTrigger) { StartCoroutine(DrainLife(lifeDecayDelay * 0.5f)); }
            }
            // default health restore
            else if (cur_health < max_health) { StartCoroutine(RestoreLife()); }
        }

        StateMachine();


        UpdateVisualDecay();

    }

    void StateMachine()
    {
        // << UPDATE STATES >>
        // player in trigger and not grabbed
        if (state != TheManState.GRABBED_PLAYER && state != TheManState.PLAYER_CAPTURED)
        {
            // << CHASE GRAB AND RETREAT >>
            if (playerInOuterTrigger)
            {
                // determine retreat or chase
                if (player.inventory.Count >= lightOrbCountNeeded)
                {
                    state = TheManState.RETREAT;
                }
                else
                {
                    state = TheManState.CHASE;
                }

                FlipTowardsPlayer();

                // << IF PLAYER IN INNER TRIGGER, START GRAB SEQUENCE >>
                if (playerInInnerTrigger)
                {
                    if (!grabStarted)
                    {
                        timeToGrabTimer += Time.deltaTime;

                        if (timeToGrabTimer >= timeToGrab)
                        {
                            state = TheManState.GRABBED_PLAYER;
                            StartCoroutine(GrabPlayer());
                            timeToGrabTimer = 0;
                        }
                    }
                }
                else { timeToGrabTimer = 0; }
            }
            else { state = TheManState.IDLE; }
        }

        // move differently depending on state
        switch (state)
        {
            case TheManState.IDLE:
                rb.MovePosition(Vector2.MoveTowards(transform.position, startPosition, chaseSpeed * Time.deltaTime));
                break;


            case TheManState.CHASE:

                // chase player
                rb.MovePosition(Vector2.MoveTowards(transform.position, player.transform.position, chaseSpeed * Time.deltaTime));
                break;

            case TheManState.RETREAT:
                // run away from player
                Vector3 oppositeDirection = (transform.position - player.transform.position) * -1f;
                rb.MovePosition(Vector2.MoveTowards(transform.position, transform.position - oppositeDirection, retreatSpeed * Time.deltaTime));
                break;
        }
    }

    IEnumerator DrainLife(float decayDelay)
    {
        lifeCoroutineStarted = true;

        cur_health--; // drain life
        
        // check for death
        if (cur_health <= 0) { Death(); yield return null; }

        yield return new WaitForSeconds(decayDelay); // wait for speed

        lifeCoroutineStarted = false;
    }

    IEnumerator RestoreLife()
    {
        lifeCoroutineStarted = true;

        cur_health++; // restore life

        yield return new WaitForSeconds(lifeDecayDelay); // wait for speed

        lifeCoroutineStarted = false;
    }

    IEnumerator GrabPlayer()
    {
        grabStarted = true;

        playerMovement.state = PlayerState.GRABBED;
        state = TheManState.GRABBED_PLAYER;
        timeToCaptureTimer = 0; // set capture timer

        // while player hasn't broken free
        while (playerMovement.struggleCount < breakFree_struggleCount && 
            state == TheManState.GRABBED_PLAYER && playerMovement.state == PlayerState.GRABBED)
        {
            // HOLD PLAYER
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            player.GetComponentInChildren<Rigidbody2D>().velocity = Vector3.zero;
            player.transform.position = transform.position;

            // PLAYER CAPTURE
            timeToCaptureTimer += Time.deltaTime;
            if (timeToCaptureTimer >= timeToCapture)
            {
                state = TheManState.PLAYER_CAPTURED;
                timeToCaptureTimer = 0;

                grabStarted = false;
                
            }

            yield return null;
        }


        if (state != TheManState.PLAYER_CAPTURED)
        {
            player.transform.parent = null;
            playerMovement.state = PlayerState.IDLE;


            state = TheManState.IDLE;
            rb.constraints = RigidbodyConstraints2D.None;

            yield return new WaitForSeconds(grabDelay);

            grabStarted = false;
        }


    }

    void UpdateVisualDecay()
    {
        head.color = Color.Lerp(endColor, startColor, (float)cur_health / (float)max_health);
        body.color = Color.Lerp(endColor, startColor, (float)cur_health / (float)max_health);

    }

    public void FlipTowardsPlayer()
    {
        if (player.transform.position.x < transform.position.x) // player is to the left
        {

            Quaternion flipRotation = Quaternion.Euler(0f, 180f, 0f); // rotate 180 degrees on the y-axis

            head.transform.rotation = flipRotation;
            body.transform.rotation = flipRotation;

        }
        else // player is to the right
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 0f, 0f); // rotate back to original rotation

            head.transform.rotation = flipRotation;
            body.transform.rotation = flipRotation;
        }
    }

    public bool IsPlayerInTrigger(float size)
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, size);
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

    public void Death()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, outerTriggerSize);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, innerTriggerSize);
    }
}
