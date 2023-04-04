using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CameraState { 
    NONE, START, PLAYER, ROOM_BASED, 
    CUSTOM_TARGET, CUSTOM_ZOOM_IN_TARGET, 
    CUSTOM_ZOOM_OUT_TARGET, GAMETIP_TARGET,
    PANIC }
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
    public float playerCamOffset = 10;
    public float playerDashCamOffset = 5;
    public Vector2 gameplayTipOffset = new Vector2(-100, 0);

    [Header("Camera Shake")]
    public float normalCamShakeDuration = 0.1f;
    public float normalCamShakeMagnitude = 0.5f;
    private Coroutine camerashake;

    [Header("Panic Camera")]
    public float panicBreatheOffset = 10;
    public float panicBreatheSpeed = 3;
    private bool isBreathing;

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


    #region <<<< CAMERA STATE MACHINE >>>>

    void CameraStateMachine()
    {
        switch(state)
        {
            case (CameraState.START):
                NormalCam();
                FollowTarget(levelManager.camStart);
                break;
            case (CameraState.PLAYER):
                NormalCam();
                FollowPlayer();
                break;
            case (CameraState.ROOM_BASED):
                NormalCam();
                MoveCamToClosestRoom();
                break;
            case (CameraState.CUSTOM_TARGET):
                NormalCam();
                FollowTarget(currTarget);
                break;
            case (CameraState.CUSTOM_ZOOM_IN_TARGET):
                ZoomInCam();
                FollowTarget(currTarget);
                break;
            case (CameraState.CUSTOM_ZOOM_OUT_TARGET):
                ZoomOutCam();
                FollowTarget(currTarget);
                break;
            case (CameraState.GAMETIP_TARGET):
                NormalCam();
                FollowTarget(currTarget, gameplayTipOffset);
                break;
            case (CameraState.PANIC):
                MoveCamToClosestRoom();
                Shiver();

                if (!isBreathing)
                {
                    StartCoroutine(PanicBreathingCamera());
                }

                if (gameManager.levelManager.player.state != PlayerState.PANIC)
                {
                    state = CameraState.PLAYER;
                }
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

    public void FollowPlayer()
    {
        if (currTarget == null) { return; }

        currTarget = player;

        Vector3 targetPos = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        Vector3 offset = Vector3.zero;

        // << MOVE OFFSET >>
        if (player.GetComponent<PlayerMovement>().state != PlayerState.GRABBED ||
            player.GetComponent<PlayerMovement>().state != PlayerState.PANIC )
        {
            offset = player.GetComponent<PlayerMovement>().moveDirection * playerCamOffset;

            if (player.GetComponent<PlayerMovement>().state == PlayerState.DASH)
            {
                offset = player.GetComponent<PlayerMovement>().moveDirection * playerDashCamOffset;
            }
        }

        // update position
        transform.position = Vector3.Lerp(transform.position , targetPos + offset, camSpeed * Time.deltaTime);
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
        if (camerashake != null)
        {
            StopCoroutine(camerashake);
        }
            
        camerashake = StartCoroutine(CameraShake(normalCamShakeDuration, normalCamShakeMagnitude));
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (camerashake != null)
        {
            StopCoroutine(camerashake);
        }
            
        camerashake = StartCoroutine(CameraShake(duration, magnitude));
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

        camerashake = null;

    }

    public void Shiver(float magnitude = 0.5f)
    {
        float x = Random.Range(-1f, 1f) * magnitude;

        transform.localPosition = new Vector3(transform.localPosition.x + x, transform.localPosition.y, transform.localPosition.z);
    }


    IEnumerator PanicBreathingCamera()
    {
        // Debug.Log("Breathing Camera");

        isBreathing = true;

        float currentZoom = normalCamSize;

        float maxZoomIn = currentZoom - panicBreatheOffset;
        float maxZoomOut = currentZoom + panicBreatheOffset;

        // move cam to beginning size
        while (Mathf.Abs(Camera.main.orthographicSize - normalCamSize) > 0.5f)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, normalCamSize, panicBreatheSpeed * Time.deltaTime);
            yield return null;
        }

        // Zoom in
        while (currentZoom > maxZoomIn)
        {
            currentZoom -= panicBreatheSpeed * Time.deltaTime;
            Camera.main.orthographicSize = currentZoom;
            yield return null;
        }

        // Zoom out
        while (currentZoom < maxZoomOut)
        {
            currentZoom += panicBreatheSpeed * Time.deltaTime;
            Camera.main.orthographicSize = currentZoom;
            yield return null;
        }

        // Normal zoom
        while (currentZoom > normalCamSize)
        {
            currentZoom -= panicBreatheSpeed * Time.deltaTime;
            Camera.main.orthographicSize = currentZoom;
            yield return null;
        }

        isBreathing = false;
    }
    #endregion

    public bool IsCamAtTarget(Transform transform, float range = 1)
    {
        if (Vector2.Distance(transform.position, transform.position) < range) { return true; }
        return false;
    }


    #region << SET STATES >>

    public void None()
    {
        state = CameraState.NONE;
    }

    public void StartPoint()
    {
        state = CameraState.START;
    }

    public void Player()
    {
        state = CameraState.PLAYER;
    }

    public void RoomBased()
    {
        state = CameraState.ROOM_BASED;
    }

    public void Panic()
    {
        state = CameraState.PANIC;
    }

    public CameraState GetCurrentState()
    {
        return state;
    }


    #endregion
}
