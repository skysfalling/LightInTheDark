using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [HideInInspector]
    public SoundManager soundManager;
    [HideInInspector]
    public LevelManager levelManager;
    [HideInInspector]
    public CameraManager camManager;
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public UIManager uiManager;
    [HideInInspector]
    public DialogueManager dialogueManager;

    public bool sceneReady;
    public LevelState levelSavePoint;

    [Space(10)]
    public string menuScene;
    public string tutorialScene;

    [Space(10)]
    public string level_1_1;
    public string level_1_2;
    public string level_1_3;

    [Space(10)]
    public string level_2;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        soundManager = GetComponent<SoundManager>();
        gameConsole = GetComponent<GameConsole>();
        dialogueManager = GetComponent<DialogueManager>();
        camManager = GetComponentInChildren<CameraManager>();
        uiManager = GetComponentInChildren<UIManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += NewSceneReset;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= NewSceneReset;
    }


    public void NewSceneReset(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("New scene loaded: " + scene.name);

        StartCoroutine(SceneSetup());
    }

    IEnumerator SceneSetup()
    {
        sceneReady = false;

        gameConsole.Clear();

        // get new level manager
        while (levelManager == null)
        {
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();

            yield return null;
        }

        levelManager.gameConsole = GetComponent<GameConsole>();
        levelManager.soundManager = GetComponent<SoundManager>();
        levelManager.dialogueManager = GetComponent<DialogueManager>();
        levelManager.camManager = GetComponentInChildren<CameraManager>();
        levelManager.uiManager = GetComponentInChildren<UIManager>();

        // set uiManager
        uiManager.levelManager = levelManager;

        // setup camera
        camManager.NewSceneReset();
        camManager.currTarget = levelManager.camStart;

        while (!camManager.IsCamAtTarget(camManager.currTarget, 2))
        {
            yield return null;
        }

        levelManager.StartLevelFromPoint(levelSavePoint);

        sceneReady = true;
    }


    public void StartGame()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void RestartLevelFromSavePoint()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
