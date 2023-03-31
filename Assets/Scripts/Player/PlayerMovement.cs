using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE , MOVING , THROWING, CHARGING, STUNNED, GRABBED, INACTIVE}

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
    public Vector2 aimDirection;
    public float throwingMoveSpeed = 10;
    public float throwForce = 20;
    private bool inThrow;
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
        if (state != PlayerState.STUNNED && state != PlayerState.GRABBED
            && state != PlayerState.INACTIVE && state != PlayerState.THROWING)
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


        if (moveDirection != Vector2.zero)
        {
            aimDirection = moveDirection.normalized;
        }

        // << THROW ACTION DOWN >>
        input_system.aAction.started += ctx =>
        {
            // << GET THROW OBJECT >>
            if (throwObject == null && !inThrow)
            {
                NewThrowObject();
                state = PlayerState.THROWING;
            }
        };

        // << THROW ACTION UP >>
        input_system.aAction.canceled += ctx =>
        {
            // << THROW OBJECT >>
            if (throwObject != null)
            {
                ThrowObject();
            }

            state = PlayerState.IDLE;
            throwParent.gameObject.SetActive(false);

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

        // << DISABLE COLLIDER >>
        if (state == PlayerState.GRABBED || state == PlayerState.INACTIVE) { EnableCollider(false); }
        else { EnableCollider(true); }


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

            case PlayerState.THROWING:

                // move player at throw move speed
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
                rb.velocity = moveDirection * throwingMoveSpeed;

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
                    float rotation = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                    throwParent.transform.eulerAngles = new Vector3(0, 0, rotation - 90f);
                }
                
                break;
            default:
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, 0);
                break;
        }
        
    }

    #region << THROW OBJECT >>
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
        if (throwObject != null && !inThrow)
        {
            throwObject.transform.parent = null;

            StartCoroutine(ThrowObject(throwObject, aimDirection, throwForce));

        }
    }

    public IEnumerator ThrowObject(GameObject obj, Vector2 direction, float force)
    {
        inThrow = true;

        Debug.Log("Throw " + obj.name);
        obj.GetComponent<Item>().state = ItemState.THROWN;
        obj.GetComponent<Item>().ResetSortingOrder();

        // remove from inventory and set state
        inventory.RemoveItemToThrow(obj);
        obj.transform.parent = null;

        obj.GetComponent<Item>().SetSortingOrder(throwSortingOrder, throwSortingLayer);
        obj.GetComponent<Rigidbody2D>().AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.25f); // wait for item to get out of player's range -> to stop immediate pickup 
 
        throwObject = null; // set throw object to null

        inThrow = false;
    }
    #endregion

    #region<< STUNNED >>
    public void Stunned(float time)
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
    #endregion

    #region << SET STATES >>

    public void Inactive()
    {
        state = PlayerState.INACTIVE;
    }

    public void Idle()
    {
        state = PlayerState.IDLE;
    }

    public void Grabbed()
    {
        state = PlayerState.GRABBED;
    }

    public void Stunned()
    {
        state = PlayerState.STUNNED;
    }

    public void Moving()
    {
        state = PlayerState.MOVING;
    }

    #endregion


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

    public void EnableCollider(bool enabled)
    {
        GetComponent<CapsuleCollider2D>().enabled = enabled;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

