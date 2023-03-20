using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LifeFlower : SubmitItemObject
{
    SpriteRenderer sprite;

    [Header("Flower Values")]
    public int lifeForce = 60;
    public int maxLifeForce = 60;
    public int deathAmount = -10;
    public bool overflowing;
    public float decay_speed = 1;

    [Space(10)]
    public Color startColor = Color.yellow;
    public Color endColor = Color.grey;

    [Space(10)]
    public GameObject endLevelEffect;
    public float endFadeOutDuration = 0.2f;

    [Header("Lighting")]
    public Light2D flowerLight;


    // << EVENT LISTENERS >>
    private bool overflowingMessageSent;
    private MessageEventListener begDrainEvent, midDrainEvent, nearEndEvent, endDrainEvent, deathEvent;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        sprite = GetComponent<SpriteRenderer>();

        StartCoroutine(Decay());

        // << EVENT LISTENERS >>
        begDrainEvent = gameConsole.EventMessage(lifeForce, EventValCompare.IS_LESS, maxLifeForce * 0.75f, " my light is fading ", gameConsole.flowerColor);
        midDrainEvent = gameConsole.EventMessage(lifeForce, EventValCompare.IS_LESS, maxLifeForce * 0.5f, " help me ", gameConsole.flowerColor);
        endDrainEvent = gameConsole.EventMessage(lifeForce, EventValCompare.IS_LESS, maxLifeForce * 0.25f, " where are you ? ", gameConsole.flowerColor);
        nearEndEvent = gameConsole.EventMessage(lifeForce, EventValCompare.IS_LESS, maxLifeForce * 0.1f, " i'm scared ", gameConsole.flowerColor);
        deathEvent = gameConsole.EventMessage(lifeForce, EventValCompare.IS_LESS_EQUAL, 0, " goodbye ", gameConsole.flowerColor);
    }

    // Update is called once per frame 
    new void Update()
    {
        base.Update();

        // send overflowing message
        overflowing = lifeForce > maxLifeForce;
        if (overflowing && !overflowingMessageSent)
        {
            gameConsole.NewMessage("---->> overflowing light <<", Color.grey);
            overflowingMessageSent = true;
        }
        else if (lifeForce < maxLifeForce * 0.8f) { overflowingMessageSent = false; }

        // << UPDATE EVENT LISTENERS >>
        begDrainEvent.EventUpdate(lifeForce);
        midDrainEvent.EventUpdate(lifeForce);
        nearEndEvent.EventUpdate(lifeForce);
        endDrainEvent.EventUpdate(lifeForce);
        deathEvent.EventUpdate(lifeForce);


        // << FLOWER LIGHT >>
        if (!gameManager.endOfLevel)
        {
            sprite.color = Color.Lerp(endColor, startColor, (float)lifeForce / (float)maxLifeForce);
        }
        else
        {
            sprite.color = Color.Lerp(sprite.color, Color.white, Time.deltaTime / endFadeOutDuration);

            flowerLight.pointLightOuterRadius = Mathf.Lerp(flowerLight.pointLightOuterRadius, 500, Time.deltaTime / endFadeOutDuration);
            flowerLight.intensity = Mathf.Lerp(flowerLight.intensity, 5, Time.deltaTime / endFadeOutDuration);

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

        // << UPDATE GAMEMANAGER >>
        // submitted darklight
        if (!gameManager.submittedDarkLight && item.type == ItemType.DARKLIGHT) 
        { 
            gameManager.submittedDarkLight = true;
            gameConsole.MessageList(gameManager.first_darklightSubmissionMessages, Color.white, 3f);
        }


        // destroy item
        player.inventory.Remove(item.gameObject);
        Destroy(item.gameObject);

        yield return new WaitForSeconds(1);

        canSubmit = true;
    }

    public IEnumerator Decay()
    {
        yield return new WaitForSeconds(decay_speed);

        lifeForce--;



        if (!gameManager.endOfLevel)
        {
            StartCoroutine(Decay());
        }

    }

    public void SpawnEndEffect()
    {
        GameObject effect = Instantiate(endLevelEffect, transform);
    }

}
