using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum ItemType { LIGHT, DARKLIGHT, GOLDEN, ETHEREAL}
public enum ItemState { FREE, PLAYER_INVENTORY, SUBMITTED, STOLEN, THROWN }


public class Item : MonoBehaviour
{
    PlayerInventory playerInventory;
    Rigidbody2D rb;
    Light2D light;

    public ItemType type;
    public ItemState state = ItemState.FREE;
    public float triggerSize = 0.75f;
    public int lifeForce = 10;

    [Header("Light")]
    public float default_lightRange = 5;
    public float default_lightIntensity = 5;
    [Space(10)]
    public float inventory_lightRange = 5;
    public float inventory_lightIntensity = 5;
    [Space(10)]
    public float thrown_lightRange = 5;
    public float thrown_lightIntensity = 5;

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        rb = GetComponent<Rigidbody2D>();
        light = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == ItemState.FREE || state == ItemState.THROWN)
        {

            Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
            List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

            foreach (Collider2D col in collidersInTrigger)
            {
                if (col.tag == "Player")
                {
                    playerInventory.AddItemToInventory(this.gameObject);

                    state = ItemState.PLAYER_INVENTORY;
                }
            }
        }

        if (state == ItemState.PLAYER_INVENTORY) { UpdateItemLight(inventory_lightRange, inventory_lightIntensity); }
        else if (state == ItemState.THROWN) { UpdateItemLight(thrown_lightRange, thrown_lightIntensity, 2); }
        else { UpdateItemLight(default_lightRange, default_lightIntensity); }



        // make sure player inventory state is valid
        if (state == ItemState.PLAYER_INVENTORY && !playerInventory.inventory.Contains(this.gameObject))
        {
            transform.parent = null;
            state = ItemState.FREE;
        }





    }

    // immediate change in light
    public void SetItemLight(float outerRange, float intensity)
    {
        light.pointLightOuterRadius = outerRange;
        light.intensity = intensity;
    }

    // for use in update functions
    public void UpdateItemLight(float outerRange, float intensity, float speed = 1)
    {
        light.pointLightOuterRadius = Mathf.Lerp(light.pointLightOuterRadius, outerRange, speed * Time.deltaTime);
        light.intensity = Mathf.Lerp(light.intensity, intensity, speed * Time.deltaTime); ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerSize);
    }
}
