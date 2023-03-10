using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeFlower : MonoBehaviour
{
    SpriteRenderer sprite;
    SubmitItemObject submitManager;

    public int lifeForce = 60;
    private int startLifeForce;
    public bool overflowing;

    [Space(10)]
    public Color startColor = Color.yellow;
    public Color endColor = Color.grey;



    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();

        startLifeForce = lifeForce;
        InvokeRepeating("NormalDecay", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeForce >= startLifeForce) { overflowing = true; } else { overflowing = false; }

    }

    public void NormalDecay()
    {
        lifeForce--;

        sprite.color = Color.Lerp(endColor, startColor, (float)lifeForce / (float)startLifeForce);
    }



}
