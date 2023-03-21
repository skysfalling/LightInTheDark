using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE , MOVING , CHARGING, STUNNED, GRABBED}

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerAnimator animator;
    PlayerInventory inventory;
    CustomPlayerInput input_system;


    public PlayerState state = PlayerState.IDLE;
    public bool inputIsDown;

    [Space(10)]
    public float speed = 10;
    private float defaultSpeed;
    public float maxVelocity = 10;
    public Vector3 moveTarget;
    public float distToTarget;

    [Space(10)]
    public Vector2 moveDirection;

    [Header("Slowed Values")]
    public bool slowed;
    public float slowedSpeed;
    public float slowedTimer;

    [Header("Dash Values")]
    public bool isDashing;
    public float dashSpeed;
    public float dashDuration;
    public float dashTimer;

    [Header("Throw Ability")]
    public Transform throwParent;
    public GameObject throwObject;
    public float throwSpeed = 20;
    public float throwDistMultiplier = 2;
    public float throwStateDuration = 5;
    private bool throwStarted;
    [Space(5)]
    public string throwSortingLayer = "Player";
    public int throwSortingOrder = 5;


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
        input_system = GetComponent<CustomPlayerInput>();


        moveTarget = transform.position;

        defaultSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        // if not stunned and new position, move
        if (state != PlayerState.STUNNED && state != PlayerState.GRABBED)
        {
            state = PlayerState.MOVING;
        }

        // throw object move to parent
        if (throwObject != null)
        {
            throwParent.gameObject.SetActive(true);


            // if not thrown yet, move object towards throw parent
            if (throwObject.GetComponent<Item>().state == ItemState.PLAYER_INVENTORY)
            {
                Vector3 newDirection = Vector3.MoveTowards(throwObject.transform.position, throwParent.transform.position, inventory.circleSpeed * Time.deltaTime);
                throwObject.transform.position = newDirection;
            }


            // rotate parent and UI towards throw point
            float rotation = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
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

        // << BASIC MOVE >>
        // set move target
        moveDirection = input_system.direction;

        // if move input down , move
        if (state != PlayerState.STUNNED && state != PlayerState.GRABBED)
        {
            if (moveDirection != Vector2.zero)
            {
                state = PlayerState.MOVING;
            }
            else
            {
                state = PlayerState.IDLE;
            }
        }

        // << THROW ACTION >>
        input_system.aAction.started += ctx =>
        {
            // << THROW OBJECT >>
            if (throwObject == null)
            {
                NewThrowObject();
            }
            else
            {
                ThrowObject();
            }


        };


        // << DASH ACTION >>
        input_system.bAction.started += ctx =>
        {
            // << START DASH >>
            if (!isDashing) { StartDash(); }

            // << STRUGGLE >>
            if (state == PlayerState.GRABBED)
            {
                struggleCount++;
            }
            else if (state != PlayerState.GRABBED)
            {
                struggleCount = 0;
            }
        };




        /*
        // << MOVE >>
        // clamp velocity when button is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
        }

        var mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // select distance in units from the camera


        //aimDirection = Camera.main.ScreenToWorldPoint(mousePos);
        //aimDirection.z = transform.position.z;

        // main input
        if (Input.GetMouseButton(0))
        {
            inputIsDown = true;


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
        */
    }

    public void StateMachine()
    {

        // << ADJUST SPEED IF SLOWED >>
        if (slowed && slowedTimer > 0)
        {
            slowedTimer -= Time.deltaTime;

            speed = slowedSpeed;
        }
        else if (slowed && slowedTimer <= 0)
        {
            slowedTimer = 0;
            slowed = false;
            speed = defaultSpeed;
        }

        // << ADJUST SPEED IF DASH >>
        if (isDashing && dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;

            speed = dashSpeed;
        }
        else if (isDashing && dashTimer <= 0)
        {
            dashTimer = 0;
            isDashing = false;
            speed = defaultSpeed;
        }

        switch (state)
        {
            case PlayerState.MOVING:
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);

                rb.velocity = moveDirection * speed;

                break;

            case PlayerState.IDLE:
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);

                // << CHARGE STATE >>
                if (state != PlayerState.CHARGING && inputIsDown && !chargeDisabled)
                {
                    StartCoroutine(ChargeCoroutine(chargeFlashActivateDuration));
                }
                break;

            default:
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
                break;
        }
        
    }

    public void NewThrowObject()
    {
        if (inventory.inventory.Count > 0)
        {
            throwObject = inventory.RemoveItemToThrow(inventory.inventory[0]); // remove 0 index item
            throwObject.transform.parent = throwParent;

            throwObject.GetComponent<Item>().SetSortingOrder(throwSortingOrder, throwSortingLayer);

        }
    }

    public void ThrowObject()
    {
        if (throwObject != null && !throwStarted)
        {
            throwObject.transform.parent = null;

            StartCoroutine(ThrowObject(throwObject, moveDirection * throwDistMultiplier, throwSpeed, throwStateDuration));

        }
    }

    public IEnumerator ThrowObject(GameObject obj, Vector2 direction, float speed, float duration)
    {
        throwStarted = true;

        float elapsed = 0f;
        Vector2 startPos = obj.transform.position;

        Debug.Log("Throw " + obj.name);
        obj.GetComponent<Item>().state = ItemState.THROWN;
        obj.GetComponent<Item>().ResetSortingOrder();

        // remove from inventory and set state
        inventory.RemoveItemToThrow(obj);
        obj.transform.parent = null;

        // move object 
        while (elapsed < duration && obj != null)
        {
            obj.transform.position = Vector2.Lerp(obj.transform.position, startPos + direction, speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }



        // object is destroyed if submitted, so check for this
        if (obj != null)
        {
            obj.transform.position = startPos + direction; // solidify end position
            obj.GetComponent<Item>().SetSortingOrder(throwSortingOrder, throwSortingLayer);

        }

        throwObject = null; // set throw object to null

        throwStarted = false;
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

    public void SetSlowed(float timer)
    {
        slowed = true;
        slowedTimer = timer;
    }

    public void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

