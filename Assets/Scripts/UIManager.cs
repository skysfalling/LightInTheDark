using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    LevelManager levelManager;
    LevelState state;

    [Header("UI")]
    public TextMeshProUGUI flowerLifeForceUI;
    public TextMeshProUGUI timerUI;
    [Space(10)]
    public Image deathBackground;
    public TextMeshProUGUI deathText;
    public float deathUIFadeDuration = 5;


    // Start is called before the first frame update
    void Start()
    {
        levelManager = GetComponent<LevelManager>();
    }

    private void Update()
    {
        state = levelManager.state;

        UIStateManager();
    }

    void UIStateManager()
    {
        switch(state)
        {
            case (LevelState.COMPLETE):
                break;
            case (LevelState.FAIL):
                DeathScreenFade();
                break;
            default:
                flowerLifeForceUI.text = "life force: " + levelManager.lifeFlower.lifeForce;

                break;
        }
    }

    void DeathScreenFade()
    {
        deathText.gameObject.SetActive(true);
        deathBackground.gameObject.SetActive(true);

        // fade alpha
        Color backgroundFade = deathBackground.color;
        backgroundFade.a = Mathf.Lerp(backgroundFade.a, 255f, Time.deltaTime / deathUIFadeDuration);

        deathBackground.color = backgroundFade;
    }
}
