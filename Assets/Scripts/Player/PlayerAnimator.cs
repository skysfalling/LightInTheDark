using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAnimator : MonoBehaviour
{
    PlayerMovement movement;
    PlayerInventory inv_script;
    public Animator anim;

    [Header("Animation")]
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

    [Header("Effects")]
    public GameObject stunEffect;


    [Header("Lighting")]
    public Light2D playerLight; // player light
    public Light2D outerLight; // outer light

    private Color currLightColor;
    private float currLightIntensity;
    private float currLightRadius;
    
    [Header("-- Player Light")]
    public float player_defaultLightIntensity = 0.7f;
    public float player_inventoryIntensityAddition = 0.1f; // how much light intensity is added per inventory item


    [Header("-- Outer Light")]
    public Color outer_lightColor;
    public float outer_defaultLightIntensity = 0.7f;
    public float outer_defaultLightRange = 5;
    public float outer_inventoryIntensityAddition = 0.3f; // how much light intensity is added per inventory item
    public float outer_inventoryRangeAddition = 0.5f; // how much light intensity is added per inventory item

    [Space(10)]
    public Color outer_chargeColor;
    public float outer_chargingLightIntensity = 5f;
    public float outer_chargingLightRange = 2f;
    public float player_chargingLightIntensity = 3;



    // Start is called before the first frame update
    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        inv_script = GetComponent<PlayerInventory>();
        playerLight = GetComponent<Light2D>();
        stunEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        AnimationStateMachine();
        LightingStateMachine();
    }

    public void AnimationStateMachine()
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

    public void LightingStateMachine()
    {
        switch(movement.state)
        {
            case PlayerState.IDLE:
            case PlayerState.MOVING:
                // raise light intensity with more items
                InventoryCountLightLerp();
                break;

            case PlayerState.CHARGING:
                ChargingLight(Time.deltaTime);
                break;

            default:
                break;

        }
    }

    public void InventoryCountLightLerp()
    {
        float playerLightIntensity = (inv_script.inventory.Count * player_inventoryIntensityAddition) + player_defaultLightIntensity;
        playerLight.intensity = Mathf.Lerp(playerLight.intensity, playerLightIntensity, Time.deltaTime);


        outerLight.color = outer_lightColor;


        float outerLightIntensity = (inv_script.inventory.Count * outer_inventoryIntensityAddition) + outer_defaultLightIntensity;
        outerLight.intensity = Mathf.Lerp(outerLight.intensity, outerLightIntensity, Time.deltaTime);

        float outerLightRadius = (inv_script.inventory.Count * outer_inventoryRangeAddition) + outer_defaultLightRange;
        outerLight.pointLightOuterRadius = Mathf.Lerp(outerLight.pointLightOuterRadius, outerLightRadius, Time.deltaTime * 0.2f);


    }

    public void ChargingLight(float amount)
    {
        outerLight.intensity = Mathf.Lerp(outerLight.intensity, outer_chargingLightIntensity, amount);
        outerLight.pointLightOuterRadius = Mathf.Lerp(outerLight.pointLightOuterRadius, outer_chargingLightRange, amount);
        outerLight.color = Color.Lerp(outerLight.color, outer_chargeColor, amount);
    }
}
