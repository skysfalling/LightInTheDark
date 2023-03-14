using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    PlayerMovement movement;
    public Animator anim;

    [Space(10)]
    public GameObject stunEffect;


    [Space(10)]
    public SpriteRenderer leftEye;
    public SpriteRenderer rightEye;
    public Sprite closedEye;
    public Sprite halfClosedEye;
    public Sprite openEye;
    public Sprite winceEye;
    public Sprite xEye;


    [Space(10)]
    public Transform bodySprite;
    public float bodyLeanAngle = 10;
    public float leanSpeed = 2;




    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        stunEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // update eye sprite
        StateMachine();
    }

    public void StateMachine()
    {
        switch (movement.state)
        {
            case PlayerState.IDLE:
                leftEye.sprite = closedEye;
                rightEye.sprite = closedEye;

                anim.SetBool("isMoving", false);

                break;
            case PlayerState.MOVING:
                leftEye.sprite = halfClosedEye;
                rightEye.sprite = halfClosedEye;

                anim.SetBool("isMoving", true);

                // update body sprite
                LeanTowardsTarget(movement.moveTarget);
                break;
            case PlayerState.STUNNED:
                leftEye.sprite = winceEye;
                rightEye.sprite = winceEye;
                break;

            default:
                break;

        }
    }

    private void LeanTowardsTarget(Vector3 targetPosition)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        if (targetPosition.x > transform.position.x + 2)
        {
            targetRotation = Quaternion.Euler(0f, 0f, -bodyLeanAngle);
        }
        else if (targetPosition.x < transform.position.x - 2)
        {
            targetRotation = Quaternion.Euler(0f, 0f, bodyLeanAngle);
        }
        else
        {
            targetRotation = Quaternion.Euler(0f, 0f, 0);
        }

        bodySprite.rotation = Quaternion.Lerp(bodySprite.rotation, targetRotation, Time.deltaTime * leanSpeed);
    }
}
