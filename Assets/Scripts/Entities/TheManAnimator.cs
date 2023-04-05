using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TheManAnimator : MonoBehaviour
{
    PlayerMovement player;
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
        light = GetComponent<Light2D>();
        canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        struggleText.text = "" + (ai.breakFree_struggleCount - ai.playerMovement.struggleCount);

        if (ai.state == TheManState.GRABBED_PLAYER || ai.state == TheManState.PLAYER_CAPTURED)
        {
            light.enabled = true;

            // enable vortex particles

            // show struggle count down
            struggleText.gameObject.SetActive(true);

            if (ai.state == TheManState.PLAYER_CAPTURED) { struggleText.text = "Captured"; }


        }
        else
        {
            light.enabled = false;

            struggleText.gameObject.SetActive(false);


        }

    }
}
