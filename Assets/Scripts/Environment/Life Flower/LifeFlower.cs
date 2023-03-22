using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LifeFlower : SubmitItemObject
{
    [HideInInspector]
    public LifeFlowerConsole console;
    SpriteRenderer sprite;

    [Header("Flower Values")]
    public bool overflowing;

    [Space(10)]
    public int lifeForce = 60;
    public int maxLifeForce = 60;
    public int deathAmount = -10;

    [Space(10)]
    public bool decayActive;
    public float decay_speed = 1;

    [Header("Animation")]
    public Light2D flowerLight;
    public GameObject innerHex;
    public GameObject pentagram;

    [Space(10)]
    public Color healthyColor = Color.magenta;
    public float healthyLightIntensity = 3;
    public float healthyLightRadius = 75;

    [Space(10)]
    public Color deathColor = Color.black;
    public float deathLightIntensity = 0;
    public float deathLightRadius = 5;

    [Space(10)]
    public Color healedColor = Color.white;
    public float healedLightIntensity = 5;
    public float healedLightRadius = 500;

    [Space(10)]
    public GameObject completedLevelEffect;
    public GameObject failedLevelEffect;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        sprite = GetComponent<SpriteRenderer>();
        console = GetComponent<LifeFlowerConsole>();

        StartCoroutine(Decay());

    }

    // Update is called once per frame 
    new void Update()
    {
        base.Update();

        // << SET OVERFLOWING >>
        overflowing = lifeForce > maxLifeForce;

        // << FLOWER LIGHT >>
        if (!levelManager.IsEndOfLevel())
        {
            sprite.color = Color.Lerp(deathColor, healthyColor, (float)lifeForce / (float)maxLifeForce);

            // scale intensity to current flower health
            flowerLight.pointLightOuterRadius = Mathf.Lerp(deathLightRadius, healthyLightRadius, (float)lifeForce / (float)maxLifeForce);
            flowerLight.intensity = Mathf.Lerp(deathLightIntensity, deathLightRadius, (float)lifeForce / (float)maxLifeForce);
        }
        else
        {
            // << WIN STATE >>
            if (levelManager.state == LevelState.COMPLETE)
            {
                sprite.color = Color.Lerp(sprite.color, healedColor, Time.deltaTime );

                flowerLight.pointLightOuterRadius = Mathf.Lerp(flowerLight.pointLightOuterRadius, healedLightRadius, Time.deltaTime);
                flowerLight.intensity = Mathf.Lerp(flowerLight.intensity, healedLightIntensity, Time.deltaTime);

                completedLevelEffect.SetActive(true);
            }

            // << FAIL STATE >>
            else if (levelManager.state == LevelState.FAIL)
            {
                sprite.color = Color.Lerp(sprite.color, deathColor, Time.deltaTime);

                flowerLight.pointLightOuterRadius = Mathf.Lerp(flowerLight.pointLightOuterRadius, deathLightRadius, Time.deltaTime);
                flowerLight.intensity = Mathf.Lerp(flowerLight.intensity, deathLightIntensity, Time.deltaTime);

                failedLevelEffect.SetActive(true);

            }

        }


        flowerLight.color = sprite.color;

        SubmissionManager();

    }

    public override void SubmissionManager()
    {
        // << DRAIN PLAYERS ENTIRE INVENTORY >>
        if (playerInTrigger && player.inventory.Count > 0)
        {
            List<GameObject> inventory = player.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                // if item type is allowed
                if (submissionTypes.Contains(inventory[i].GetComponent<Item>().type))
                {
                    // add to overflow
                    submissionOverflow.Add(inventory[i]);

                    player.inventory.Remove(inventory[i]);
                }
            }
        }

        // << SUBMISSION OVERFLOW MANAGER >>
        if (submissionOverflow.Count > 0)
        {
            // circle overflow items
            CircleAroundTransform(submissionOverflow);

            if (canSubmit && !overflowing)
            {
                StartCoroutine(SubmitItem());
            }
        }
    }

    public override IEnumerator SubmitItem()
    {

        if (submissionOverflow.Count == 0) { yield return null; }

        canSubmit = false;

        // get item
        Item item = submissionOverflow[0].GetComponent<Item>();

        item.transform.parent = transform; // set parent


        // << MOVE ITEM TO CENTER >>
        while (Vector2.Distance(item.transform.position, transform.position) > 5f)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, submitSpeed * Time.deltaTime);
            //yield return null;
        }

        Debug.Log("Submit Item", item.gameObject);
        submissionOverflow.Remove(item.gameObject);

        // add to life force
        lifeForce += item.lifeForce;

        // << SPAWN EFFECT >>
        submitEffect.GetComponent<ParticleSystem>().startColor = item.GetComponent<SpriteRenderer>().color;
        GameObject effect = Instantiate(submitEffect, transform);
        Destroy(effect, 5);

        // destroy item
        player.inventory.Remove(item.gameObject);
        Destroy(item.gameObject);

        yield return new WaitForSeconds(1);

        canSubmit = true;
    }

    public IEnumerator Decay()
    {
        if (decay_speed <= 0) { yield return null; }

        yield return new WaitForSeconds(decay_speed);

        if (decayActive)
        {
            lifeForce--;
        }

        if (!levelManager.IsEndOfLevel())
        {
            StartCoroutine(Decay());
        }

    }

}
