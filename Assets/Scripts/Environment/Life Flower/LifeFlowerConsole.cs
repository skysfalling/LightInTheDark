using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeFlowerConsole : EntityConsole
{
    LifeFlower flower;
    MessageEventListener begDrainEvent, midDrainEvent, nearEndEvent, endDrainEvent, deathEvent;

    private bool overflowingMessageSent;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        flower = GetComponent<LifeFlower>();

        // << EVENT LISTENERS >>
        begDrainEvent = EventMessage(flower.lifeForce, EventValCompare.IS_LESS, flower.maxLifeForce * 0.75f, " my light is fading ");
        midDrainEvent = EventMessage(flower.lifeForce, EventValCompare.IS_LESS, flower.maxLifeForce * 0.5f, " help me ");
        endDrainEvent = EventMessage(flower.lifeForce, EventValCompare.IS_LESS, flower.maxLifeForce * 0.25f, " where are you ? ");
        nearEndEvent = EventMessage(flower.lifeForce, EventValCompare.IS_LESS, flower.maxLifeForce * 0.1f, " i'm scared ");
        deathEvent = EventMessage(flower.lifeForce, EventValCompare.IS_LESS_EQUAL, 0, " goodbye ");
    }

    // Update is called once per frame
    void Update()
    {
        // if decay is active, send messages
        if (flower.decayActive)
        {
            /*
            // << UPDATE EVENT LISTENERS >>
            begDrainEvent.EventUpdate(flower.lifeForce);
            midDrainEvent.EventUpdate(flower.lifeForce);
            nearEndEvent.EventUpdate(flower.lifeForce);
            endDrainEvent.EventUpdate(flower.lifeForce);
            deathEvent.EventUpdate(flower.lifeForce);
            */

            // << SEND OVERFLOWING MESSAGE >>
            if (flower.state == FlowerState.OVERFLOWING && !overflowingMessageSent)
            {
                NewMessage("overflowing light", Color.white);
                overflowingMessageSent = true;
            }
            else if (flower.lifeForce < flower.maxLifeForce * 0.8f) { overflowingMessageSent = false; }
        }

    }
}
