using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public GameConsole gameConsole;
    [HideInInspector]
    public SoundManager soundManager;
    GameObject player;
    PlayerInventory player_inv;
    public LifeFlower lifeFlower;

    [Header("UI")]
    public TextMeshProUGUI life_forceUI;
    public TextMeshProUGUI timerUI;
    [Space(10)]
    public Image deathBackground;
    public TextMeshProUGUI deathText;
    public float deathUIFadeDuration = 5;

    [Header("General Prefabs")]
    public GameObject lightOrb;
    public GameObject darklightOrb;


    [Header("Game Values")]
    [Space(10)]
    private float startTime;
    public float timer;
    public float levelEndTime;

    [Header("First Encouters")]
    public bool foundLightOrb;
    public List<string> first_lightOrbMessages;

    [Space(10)]
    public bool submittedLight;
    public List<string> first_lightSubmissionMessages;

    [Space(10)]
    public bool foundDarkLight;
    public List<string> first_foundDarklightMessages;

    [Space(10)]
    public bool submittedDarkLight;
    public List<string> first_darklightSubmissionMessages;

    [Space(10)]
    public bool convertedDarkLight;
    public List<string> first_darklightConversionMessages;


    [Header("General Messages")]
    public bool endOfLevel;
    public List<string> endOfLevelMessages;

    public bool death;
    public List<string> deathMessages;



    void Awake()
    {
        gameConsole = GetComponent<GameConsole>();
        soundManager = GetComponentInChildren<SoundManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        player_inv = player.GetComponent<PlayerInventory>();

        startTime = Time.time;

        gameConsole.NewMessage("---->> the flower begins to wilt <<", Color.grey);

    }

    private void Start()
    {
        soundManager.PlayMusic(soundManager.darkTheme);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();

        // << UPDATE FIRST INTERACTION BOOLS >>
        // check inventory for item
        List<GameObject> inventory = player_inv.inventory;
        for (int i = 0; i < inventory.Count; i++)
        {

            Item item = inventory[i].GetComponent<Item>();

            // check item type
            switch (item.type)
            {
                case ItemType.LIGHT:
                    if (!foundLightOrb) 
                    { 
                        foundLightOrb = true;
                        gameConsole.MessageList(first_lightOrbMessages, Color.white, 1f);
                    }
                    break;
                case ItemType.DARKLIGHT:
                    if (!foundDarkLight) 
                    { 
                        foundDarkLight = true;
                        gameConsole.MessageList(first_foundDarklightMessages, Color.white, 1f);
                    }
                    break;
                    
            }
        }

        // check life flower
        if (lifeFlower && lifeFlower.submissionOverflow.Count > 0 && !submittedLight )
        {
            submittedLight = true;
            gameConsole.MessageList(first_lightSubmissionMessages, Color.white, 2f);
        }

        // check for end of level
        if (timer >= levelEndTime && !endOfLevel) 
        { 
            endOfLevel = true;
            gameConsole.MessageList(endOfLevelMessages, Color.white, 2f);

            lifeFlower.SpawnEndEffect();
        }

        // check for death
        if (lifeFlower.lifeForce < lifeFlower.deathAmount && !death)
        {
            death = true;
        }
    }

    private void UpdateUI()
    {
        UpdateTimer();

        life_forceUI.text = "life force: " + lifeFlower.lifeForce;

        if (death)
        {
            deathText.gameObject.SetActive(true);
            deathBackground.gameObject.SetActive(true);

            // fade alpha
            Color backgroundFade = deathBackground.color;
            backgroundFade.a = Mathf.Lerp(backgroundFade.a, 255f, Time.deltaTime / deathUIFadeDuration);

            deathBackground.color = backgroundFade;
        }

    }


    private void UpdateTimer()
    {
        float timePassed = Time.time - startTime;
        timer =  Mathf.Round(timePassed * 10) / 10f;

        timerUI.text = "" + timer;
    }
}
