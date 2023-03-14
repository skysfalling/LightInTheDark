using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAnimator : MonoBehaviour
{
    public bool rotate;
    public Vector2 rotationSpeed = new Vector2(-20, 20);
    private float speed;

    // Start is called before the first frame update
    void Start()
    {
        if (rotate)
        {
            // init with random rotation
            transform.rotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

            speed = Random.Range(rotationSpeed.x, rotationSpeed.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }
}
