using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public SoundManager soundManager;


    public string menuScene;
    public string tutorialScene;
    public string level1Scene;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        soundManager = GetComponent<SoundManager>();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene(tutorialScene);
    }


    public void LoadLevel1()
    {
        SceneManager.LoadScene(level1Scene);
    }
}
