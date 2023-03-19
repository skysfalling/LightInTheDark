using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE , MOVING , CHARGING, STUNNED, GRABBED}

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerAnimator animator;
    PlayerInventory inventory;

    Vector3 mouseWorldPos;

    public PlayerState state = PlayerState.IDLE;
    public bool inputIsDown;

    [Space(10)]
    public float speed = 10;
    public float maxVelocity = 10;
    public Vector3 moveTarget;
    public Vector2 moveDirection;
    public float distToTarget;

    [Header("Throw Ability")]
    public Transform throwParent;
    public GameObject throwObject;
    public float throwSpeed = 20;
    public float throwDistMultiplier = 2;
    public float throwStateDuration = 5;


    [Header("Charge Flash")]
    public float chargeFlashActivateDuration = 1.5f;
    public bool chargeDisabled;
    public float chargeDisableTime = 1;
    public float chargeLightIntensity = 3;


    [Header("Interaction")]
    public float interactionRange = 5;


    [Header("Grabbed")]
    GrabHandAI grabbedHand;
    public int struggleCount;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>();
        inventory = GetComponent<PlayerInventory>();
        moveTarget = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        // if not stunned and new position, move
        if (state != PlayerState.STUNNED && state != PlayerState.GRABBED && distToTarget > interactionRange)
        {
            state = PlayerState.MOVING;
        }

        // throw object move to parent
        if (throwObject != null)
        {
            throwParent.gameObject.SetActive(true);

            Vector3 newDirection = Vector3.MoveTowards(throwObject.transform.position, throwParent.transform.position, Time.deltaTime);
            throwObject.transform.position = newDirection;

            // rotate parent and UI towards throw point
            Vector2 direction = mouseWorldPos - transform.position;
            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            throwParent.transform.eulerAngles = new Vector3(0, 0, rotation - 90f);
        }
        else
        {
            throwParent.gameObject.SetActive(false);
        }

    }

    private void FixedUpdate()
    {
        StateMachine();
    }

    public void Inputs()
    {
        // << MOVE >>
        // clamp velocity when button is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
        }

        var mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // select distance in units from the camera

        // set move target
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = transform.position.z;

        // main input
        if (Input.GetMouseButton(0))
        {
            inputIsDown = true;

            // choose new move target
            if (state != PlayerState.STUNNED && state != PlayerState.GRABBED)
            {
                NewMoveTarget();
            }
        }
        else { inputIsDown = false; }


        // << THROW >>
        if (Input.GetMouseButtonDown(1))
        {
            if (throwObject == null)
            {
                NewThrowObject();
            }
            else
            {
                ThrowObject();
            }
        }

        // << STRUGGLE >>
        if (state == PlayerState.GRABBED && Input.GetMouseButtonDown(0))
        {
            struggleCount++;
        }
        else if (state != PlayerState.GRABBED)
        {
            struggleCount = 0;
        }
    }

    public void StateMachine()
    {
        distToTarget = Vector3.Distance(transform.position, moveTarget);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);

        switch (state)
        {
            case PlayerState.MOVING:

                // update movement
                Vector3 newDirection = Vector3.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

                rb.MovePosition(newDirection);

                // if at target, back to idle
                if (distToTarget < 2) { state = PlayerState.IDLE; }
                break;

            case PlayerState.IDLE:

                // << CHARGE STATE >>
                if (state != PlayerState.CHARGING && inputIsDown && !chargeDisabled)
                {
                    StartCoroutine(ChargeCoroutine(chargeFlashActivateDuration));
                }
                break;

            default:
                break;
        }
        
    }


    public void NewMoveTarget()
    {
        // set move target
        moveTarget = mouseWorldPos;

        // set move direction
        moveDirection = (moveTarget - transform.position).normalized;
    }

    public void NewThrowObject()
    {
        if (inventory.inventory.Count > 0)
        {
            throwObject = inventory.RemoveItemToThrow(inventory.inventory[0]); // remove 0 index item
            throwObject.transform.parent = throwParent;
        }
    }

    public void ThrowObject()
    {
        if (throwObject != null)
        {
            throwObject.transform.parent = null;

            StartCoroutine(ThrowObject(throwObject, (mouseWorldPos - transform.position).normalized * throwDistMultiplier, throwSpeed, throwStateDuration));

            throwObject = null;
        }
    }

    public IEnumerator ThrowObject(GameObject obj, Vector2 direction, float speed, float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = obj.transform.position;

        while (elapsed < duration)
        {
            obj.transform.position = Vector2.Lerp(obj.transform.position, startPos + direction, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.GetComponent<Item>().state = ItemState.THROWN;


        obj.transform.position = startPos + direction;
    }


    // <<<< STUN STATE >>>>
    public void Stun(float time)
    {
        StartCoroutine(StunCoroutine(time));
    }

    public IEnumerator StunCoroutine(float time)
    {
        state = PlayerState.STUNNED;

        animator.stunEffect.SetActive(true);

        yield return new WaitForSeconds(time);

        animator.stunEffect.SetActive(false);

        state = PlayerState.IDLE;
    }

    private IEnumerator ChargeCoroutine(float activateDuration)
    {
        float elapsedTime = 0f;

        Debug.Log("Charging....");
        state = PlayerState.CHARGING;

        // iterate until time is reached
        while (elapsedTime < activateDuration && inputIsDown)
        {
            yield return null;


            elapsedTime += Time.deltaTime;


        }

        Debug.Log("Boom");
        state = PlayerState.IDLE;
        chargeDisabled = true;

        yield return new WaitForSeconds(chargeDisableTime);
        chargeDisabled = false;

    }



    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

