using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE , MOVING , CHARGING, STUNNED}

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerAnimator animator;

    public PlayerState state = PlayerState.IDLE;
    public bool inputIsDown;

    [Space(10)]
    public float speed = 10;
    public float maxVelocity = 10;
    public Vector3 moveTarget;
    public Vector2 moveDirection;
    public float distToTarget;

    [Header("Charge Flash")]
    public float chargeFlashActivateDuration = 1.5f;
    public bool isCharging;

    [Header("Interaction")]
    public float interactionRange = 5;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>();
        moveTarget = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
    }

    private void FixedUpdate()
    {
        StateMachine();
    }

    public void Inputs()
    {
        // clamp velocity when button is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
        }

        // main input
        if (Input.GetMouseButton(0))
        {
            inputIsDown = true;
            NewMoveTarget();
        }
        else { inputIsDown = false; }
    }

    public void StateMachine()
    {
        distToTarget = Vector3.Distance(transform.position, moveTarget);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);


        // if not stunned and new position, move
        if (state != PlayerState.STUNNED && distToTarget > interactionRange )
        {
            state = PlayerState.MOVING;
        }


        switch (state)
        {
            case PlayerState.MOVING:

                isCharging = false;

                // update movement
                Vector3 newDirection = Vector3.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

                rb.MovePosition(newDirection);

                // if at target, back to idle
                if (distToTarget < 2) { state = PlayerState.IDLE; }
                break;

            case PlayerState.IDLE:

                // check for charging
                if (!isCharging && inputIsDown)
                {
                    StartCoroutine(ChargeFlash(chargeFlashActivateDuration));
                }
                break;

            default:
                break;
        }
        
    }


    public void NewMoveTarget()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // select distance in units from the camera

        // set move target
        moveTarget = Camera.main.ScreenToWorldPoint(mousePos);
        moveTarget.z = transform.position.z;

        // set move direction
        moveDirection = (moveTarget - transform.position).normalized;
    }


    // <<<< STUN STATE >>>>
    public void Stun(float time)
    {
        StartCoroutine(StunCoroutine(time));
    }


    private IEnumerator ChargeFlash(float activateDuration)
    {
        float elapsedTime = 0f;

        Debug.Log("Charging....");
        isCharging = true;

        while (elapsedTime < activateDuration && inputIsDown)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        Debug.Log("Boom");
    }

    public IEnumerator StunCoroutine(float time)
    {
        state = PlayerState.STUNNED;

        animator.stunEffect.SetActive(true);

        yield return new WaitForSeconds(time);

        animator.stunEffect.SetActive(false);

        state = PlayerState.IDLE;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

