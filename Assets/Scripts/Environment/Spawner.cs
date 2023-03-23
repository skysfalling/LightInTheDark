using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool canSpawn = true;
    public bool spawnOnStart;
    public GameObject prefab; // The prefab to spawn
    public GameObject spawnedObject; // The object that has been spawned
    public float spawnDelay = 1f; // The delay before spawning a new object

    private void Start()
    {
        if (spawnOnStart) { StartSpawn(); }
    }

    public void StartSpawn()
    {
        canSpawn = true;
        StartCoroutine(SpawnObjectWithDelay());
    }

    IEnumerator SpawnObjectWithDelay()
    {
        // Check if the object has been spawned already
        if (!spawnedObject && canSpawn)
        {
            // Spawn the object
            spawnedObject = Instantiate(prefab, transform.position, transform.rotation);
        }
        else if (spawnedObject)
        {
            // check if been picked up
            Item item = spawnedObject.GetComponent<Item>();
            if (item && item.state != ItemState.FREE) { spawnedObject = null; }
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(spawnDelay);

        // Start the coroutine again to spawn another object with delay
        StartCoroutine(SpawnObjectWithDelay());
    }

    public void DestroyItem()
    {
        spawnedObject.GetComponent<Item>().Destroy();
        spawnedObject = null;
        canSpawn = false;
    }
}
