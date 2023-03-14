using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab; // The prefab to spawn
    public GameObject spawnedObject; // The object that has been spawned
    public float spawnDelay = 1f; // The delay before spawning a new object

    void Start()
    {
        StartCoroutine(SpawnObjectWithDelay());
    }

    IEnumerator SpawnObjectWithDelay()
    {
        // Check if the object has been spawned already
        if (spawnedObject == null)
        {
            // Spawn the object
            spawnedObject = Instantiate(prefab, transform.position, transform.rotation);
        }
        else
        {
            // check if been picked up
            Item item = spawnedObject.GetComponent<Item>();
            if (item && item.state == ItemState.PLAYER_INVENTORY) { spawnedObject = null; }
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(spawnDelay);

        // Start the coroutine again to spawn another object with delay
        StartCoroutine(SpawnObjectWithDelay());
    }
}
