using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TheManAnimator : MonoBehaviour
{
    PlayerMovement playerMovement;
    TheManAI ai;
    public Animator anim;
    public Canvas canvas;

    [Header("UI")]
    public TextMeshProUGUI struggleText;

    [Header("Light")]
    public Light2D light;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<TheManAI>();
        playerMovement = ai.playerMovement;
        light = GetComponent<Light2D>();
        canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement == null)
        {
            playerMovement = ai.playerMovement;
        }


        // << ANIM VALUES >>
        anim.SetBool("Idle", ai.state == TheManState.IDLE || ai.state == TheManState.RETREAT);
        anim.SetBool("Grab", ai.state == TheManState.GRABBED_PLAYER || ai.state == TheManState.PLAYER_CAPTURED);
        anim.SetBool("Chase", ai.state == TheManState.CHASE || ai.state == TheManState.FOLLOW);


        switch (ai.state)
        {
            case TheManState.GRABBED_PLAYER:
            case TheManState.PLAYER_CAPTURED:
                // enable vortex particles

                // show struggle count down
                if (ai.state == TheManState.PLAYER_CAPTURED) { struggleText.text = "help"; }
                else
                {
                    // << STRUGGLE >>
                    struggleText.text = "" + (ai.breakFree_struggleCount - ai.playerMovement.struggleCount);
                }
                struggleText.gameObject.SetActive(true);
                break;

            case TheManState.CHASE:
            case TheManState.RETREAT:
            case TheManState.IDLE:

                if (ai.state == TheManState.CHASE)
                {
                    // flip
                    FlipTowardsPlayer();
                }

                // struggle reset
                struggleText.gameObject.SetActive(false);
                break;
        }

    }


    public void FlipTowardsPlayer()
    {

        /*
        if (playerMovement.transform.position.x < transform.position.x) // player is to the left
        {

            Quaternion flipRotation = Quaternion.Euler(0f, 180f, 0f); // rotate 180 degrees on the y-axis

            transform.rotation = flipRotation;

        }
        else // player is to the right
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 0f, 0f); // rotate back to original rotation

            transform.rotation = flipRotation;
        }
        */

        // Set the rotation speed and the offset from the player
        float rotationSpeed = 10f;
        float flipOffset = 50;

        // player is to the left
        if (playerMovement.transform.position.x < transform.position.x - flipOffset)
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 180f, 0f); // rotate 180 degrees on the y-axis

            transform.rotation = Quaternion.Lerp(transform.rotation, flipRotation, Time.deltaTime * rotationSpeed);
        }
        // player is to the right
        else if (playerMovement.transform.position.x > transform.position.x + flipOffset) 
        {
            Quaternion flipRotation = Quaternion.Euler(0f, 0f, 0f); // rotate back to original rotation

            transform.rotation = Quaternion.Lerp(transform.rotation, flipRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
