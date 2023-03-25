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

    public string menuScene;
    public string tutorialScene;
    public string level1Scene;

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

        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        soundManager = GetComponent<SoundManager>();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
