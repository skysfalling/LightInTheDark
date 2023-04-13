using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    private GameManager gameManager;
    private Transform playerTransform;

	public StudioEventEmitter backgroundEmitter;
	public EventReference backgroundMusicEvent;
    private EventInstance backgroundMusicInstance;


    // OVERALL VOLUME CONTROL

    // GAME STATE
    public bool lifeFlowerNearDeath;

    public LayerMask enemyLayer;    // the layer where enemies are located
    public float detectionRadius = 40;   // the radius of the sphere used to detect enemies
    public int maxEntityCount = 5;      // the maximum number of entities that can be in the sphere
    public float closestDistance;   // the closest distance to an enemy collider
    public int musicIntensity;      // the music intensity based on the number of entities

    // STATE ONE SHOTS


    // ATTENUATION RANGES


    [Header("FMOD EVENTS")]
	public string lightPickupSound = "event:/lightPickup";

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playerTransform = gameManager.levelManager.player.transform;


        backgroundMusicInstance = FMODUnity.RuntimeManager.CreateInstance(backgroundMusicEvent);
        backgroundMusicInstance.start();

    }

    private void Update()
    {

        SetMusicIntensity();
        backgroundMusicInstance.setParameterByName("musicIntensity", musicIntensity);

        float a;
        backgroundMusicInstance.getParameterByName("musicIntensity", out a);
        Debug.Log( "FMOD background: " + a );

    }

    public void SetMusicIntensity()
    {

        // use an overlap sphere to check all colliders in the detection radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerTransform.position, detectionRadius, enemyLayer);


        // calculate the music intensity based on the percentage of entities in the sphere
        float entityPercentage = (float)hits.Length / maxEntityCount;
        Debug.Log("enemy percentage: " + entityPercentage);


        if (entityPercentage < 0.25f)
        {
            musicIntensity = 0;
        }
        else if (entityPercentage < 0.5f)
        {
            musicIntensity = 1;
        }
        else if (entityPercentage < 0.75f)
        {
            musicIntensity = 2;
        }
        else
        {
            musicIntensity = 3;
        }
    }



	// Play a single clip through the sound effects source.
	public void Play(string path)
	{
		FMODUnity.RuntimeManager.PlayOneShot(path);
	}

    public void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, detectionRadius);
        }
        catch { }

    }

}