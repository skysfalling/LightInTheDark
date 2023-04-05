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

    [Header("UI")]
    public TextMeshProUGUI struggleText;

    [Header("Light")]
    public Light2D light;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<TheManAI>();
        light = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (ai.state == TheManState.GRABBED_PLAYER)
        {
            light.enabled = true;

            // enable vortex particles

            // show struggle count down
            struggleText.gameObject.SetActive(true);
            struggleText.text = "" + ( ai.breakFree_struggleCount - ai.playerMovement.struggleCount ) ;

        }
        else
        {
            light.enabled = false;

            struggleText.gameObject.SetActive(false);


        }

    }
}
