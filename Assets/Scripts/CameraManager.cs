using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CameraState { NONE, START, PLAYER, ROOM_BASED, CUSTOM_TARGET, CUSTOM_ZOOM_IN_TARGET, CUSTOM_ZOOM_OUT_TARGET, GAMETIP_TARGET }
public class CameraManager : MonoBehaviour
{
    GameManager gameManager;
    LevelManager levelManager;
    Transform player;
    List<GameObject> rooms;

    public CameraState state = CameraState.NONE;
    public Transform currTarget;
    public float camSpeed = 10;

    [Space(5)]
    public float zoomInCamSize = 25;
    public float normalCamSize = 40;
    public float zoomOutCamSize = 60;

    [Space(10)]
    public Vector2 gameplayTipOffset = new Vector2(-100, 0);

    [Header("Camera Shake")]
    public float normalCamShakeDuration = 0.1f;
    public float normalCamShakeMagnitude = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponentInParent<GameManager>();
    }

    public void NewSceneReset()
    {
        // get new level manager
        levelManager = gameManager.levelManager;
        player = levelManager.player.transform;
        rooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Room"));

        normalCamSize = Camera.main.orthographicSize;

        state = CameraState.START;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CameraStateMachine();
    }


    void CameraStateMachine()
    {
        // update zoom
        if (state == CameraState.CUSTOM_ZOOM_IN_TARGET || state == CameraState.GAMETIP_TARGET) { ZoomInCam(); }
        else if (state == CameraState.CUSTOM_ZOOM_OUT_TARGET) { ZoomOutCam(); }
        else { NormalCam(); }

        switch(state)
        {
            case (CameraState.START):
                FollowTarget(levelManager.camStart);
                break;
            case (CameraState.PLAYER):
                FollowTarget(player);
                break;
            case (CameraState.ROOM_BASED):
                MoveCamToClosestRoom();
                break;
            case (CameraState.CUSTOM_TARGET):
            case (CameraState.CUSTOM_ZOOM_IN_TARGET):
            case (CameraState.CUSTOM_ZOOM_OUT_TARGET):
                FollowTarget(currTarget);
                break;
            case (CameraState.GAMETIP_TARGET):
                FollowTarget(currTarget, gameplayTipOffset);
                break;
        }
    }

    public void MoveCamToClosestRoom()
    {
        if (rooms.Count == 0) { return; }

        GameObject closestRoom = rooms[0];
        float closestDistance = Mathf.Infinity;

        foreach (GameObject room in rooms)
        {
            float dist = Vector3.Distance(player.position, room.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestRoom = room;
            }
        }

        Vector3 target = new Vector3(closestRoom.transform.position.x, closestRoom.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, camSpeed * Time.deltaTime);
    }

    public void FollowTarget(Transform newTarget)
    {
        if (currTarget == null) { return; }

        currTarget = newTarget;

        Vector3 targetPos = new Vector3(newTarget.transform.position.x, newTarget.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, camSpeed * Time.deltaTime);
    }

    public void FollowTarget(Transform newTarget, Vector3 offset)
    {
        if (currTarget == null) { return; }

        currTarget = newTarget;

        Vector3 targetPos = new Vector3(newTarget.transform.position.x, newTarget.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos + offset, camSpeed * Time.deltaTime);
    }

    public void NewCustomTarget(Transform customTarget)
    {
        state = CameraState.CUSTOM_TARGET;
        currTarget = customTarget;
    }

    public void NewCustomZoomInTarget(Transform customTarget)
    {
        state = CameraState.CUSTOM_ZOOM_IN_TARGET;
        currTarget = customTarget;
    }

    public void NewCustomZoomOutTarget(Transform customTarget)
    {
        state = CameraState.CUSTOM_ZOOM_OUT_TARGET;
        currTarget = customTarget;
    }

    public void NewGameTipTarget(Transform customTarget)
    {
        state = CameraState.GAMETIP_TARGET;
        currTarget = customTarget;
    }

    void NormalCam()
    {
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, normalCamSize, Time.deltaTime);
    }

    void ZoomInCam()
    {
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoomInCamSize, Time.deltaTime);
    }

    void ZoomOutCam()
    {
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoomOutCamSize, Time.deltaTime);
    }

    public void ShakeCamera()
    {
        StartCoroutine(CameraShake(normalCamShakeDuration, normalCamShakeMagnitude));
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(CameraShake(duration, magnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(transform.localPosition.x + x, transform.localPosition.y + y, transform.localPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

    }

    public bool IsCamAtTarget(Transform transform, float range = 1)
    {
        if (Vector2.Distance(transform.position, transform.position) < range) { return true; }
        return false;
    }
}
