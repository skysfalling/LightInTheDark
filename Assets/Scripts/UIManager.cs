using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    LevelManager levelManager;
    public LevelState state;

    public TextMeshProUGUI flowerLifeForceUI;
    public TextMeshProUGUI timerUI;

    [Header("Transition")]
    public Image transition;

    [Space(5)]
    public bool startTransition;
    public bool endTransition;

    [Space(5)]
    public float gameDissolveAmount = 0.5f;
    public float transitionDelay = 1;

    [Space(10)]
    public TextMeshProUGUI deathText;
    public float transitionSpeed = 5;

    private void Awake()
    {
        transition.material.SetFloat("_Dissolve", 0);
    }

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
            case (LevelState.INTRO):
                if (!startTransition)
                {
                    startTransition = true;
                    StartCoroutine(TransitionFadeOut(transitionDelay));
                }
                break;
            case (LevelState.COMPLETE):
                if (!endTransition)
                {
                    endTransition = true;
                    StartCoroutine(TransitionFadeIn(transitionDelay));
                }
                break;
            case (LevelState.FAIL):
                if (!endTransition)
                {
                    endTransition = true;
                    StartCoroutine(TransitionFadeIn(transitionDelay));
                }
                break;
            default:
                flowerLifeForceUI.text = "life force: " + levelManager.lifeFlower.lifeForce;

                break;
        }
    }




    IEnumerator TransitionFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = 0;
        while (transitionAmount < gameDissolveAmount)
        {
            transitionAmount = Mathf.Lerp(transitionAmount, gameDissolveAmount, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);

            yield return null;
        }
    }

    IEnumerator TransitionFadeIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = gameDissolveAmount;
        while (transitionAmount > 0)
        {

            transitionAmount = Mathf.Lerp(transitionAmount, 0, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);
            yield return null;
        }
    }
}
