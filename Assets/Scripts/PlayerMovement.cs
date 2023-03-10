using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE , MOVE_TO_TARGET }

public class PlayerMovement : MonoBehaviour
{

    public PlayerState state = PlayerState.IDLE;

    public float speed = 10;
    public Vector3 moveTarget;
    public Vector2 moveDirection;


    // Start is called before the first frame update
    void Start()
    {
        moveTarget = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        StateMachine();
    }


    public void Inputs()
    {
        if (Input.GetMouseButton(0))
        {
            NewMoveTarget();
        }
    }

    public void StateMachine()
    {

        if (state == PlayerState.MOVE_TO_TARGET)
        {
            // update movement
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

            // if at target, back to idle
            if (transform.position == moveTarget) { state = PlayerState.IDLE; }
        }
    }


    public void NewMoveTarget()
    {
        state = PlayerState.MOVE_TO_TARGET;

        var mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z; // select distance in units from the camera

        // set move target
        moveTarget = Camera.main.ScreenToWorldPoint(mousePos);
        moveTarget.z = transform.position.z;

        // set move direction
        moveDirection = (moveTarget - transform.position).normalized;
    }
}

