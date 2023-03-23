using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [HideInInspector]
    public SoundManager soundManager;

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
