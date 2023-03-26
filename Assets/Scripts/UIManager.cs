using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    GameManager gameManager;
    public LevelManager levelManager;

    [Header("UI")]
    public TextMeshProUGUI countdown;
    public float UI_horzOffset = 100;
    public bool uiMoving;

    [Header("Transition")]
    public Image transition;
    public bool transitionFinished;
    public float gameDissolveAmount = 0.5f;
    public float transitionDelay = 1;
    public TextMeshProUGUI deathText;
    public float transitionSpeed = 5;

    [Header("Dialogue")]
    public GameObject dialogueObject;
    public TextMeshProUGUI dialogueText;
    public GameObject contText;
    public float wordDelay = 0.05f;
    public bool inDialogue;


    private void Awake()
    {
        gameManager = GetComponentInParent<GameManager>();

        transition.material.SetFloat("_Dissolve", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        levelManager = gameManager.levelManager;
    }

    private void Update()
    {
        try
        {
            countdown.text = "" + levelManager.GetCurrentCountdown();
        }
        catch { Debug.Log("Countdown UI Error"); }
    }

    #region LevelTransition
    public void StartTransitionFadeOut()
    {
        StartCoroutine(TransitionFadeOut(transitionDelay));
    }

    public void StartTransitionFadeIn()
    {
        StartCoroutine(TransitionFadeIn(transitionDelay));
    }

    IEnumerator TransitionFadeOut(float delay)
    {
        transitionFinished = false;

        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = 0;
        while (transitionAmount < gameDissolveAmount)
        {
            transitionAmount = Mathf.MoveTowards(transitionAmount, gameDissolveAmount, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);

            yield return null;
        }

        transitionFinished = true;

    }

    IEnumerator TransitionFadeIn(float delay)
    {
        transitionFinished = false;


        yield return new WaitForSeconds(delay);

        transition.gameObject.SetActive(true);

        // dissolve transition
        float transitionAmount = gameDissolveAmount;
        while (transitionAmount > 0)
        {

            transitionAmount = Mathf.MoveTowards(transitionAmount, 0, Time.deltaTime * transitionSpeed);

            transition.material.SetFloat("_Dissolve", transitionAmount);
            yield return null;
        }

        transitionFinished = true;

    }
    #endregion

    #region Dialogue
    public void NewDialogue(string text)
    {
        dialogueObject.SetActive(true);
        string decodedText = gameManager.gameConsole.DecodeColorString(text);

        StartCoroutine(GameDialogueRoutine(dialogueText, decodedText, wordDelay));
            
    }

    public void NewDialogue(List<string> text)
    {
        dialogueObject.SetActive(true);

        StartCoroutine(GameDialogueRoutine(dialogueText, text, wordDelay));

    }

    public void TimedDialogue(List<string> text, float sentenceDelay)
    {
        dialogueObject.SetActive(true);

        StartCoroutine(TimedGameDialogueRoutine(dialogueText, text, wordDelay, sentenceDelay));
    }

    public void DialoguePromptContinue()
    {
        contText.SetActive(true);
    }

    public void DisableDialogue()
    {
        contText.SetActive(false);
        dialogueObject.SetActive(false);
    }

    IEnumerator GameDialogueRoutine(TextMeshProUGUI textComponent, List<string> dialogue , float wordDelay)
    {
        inDialogue = true;

        int currentStringIndex = 0;
        string[] currentWords;

        while (currentStringIndex < dialogue.Count)
        {
            Debug.Log("Dialogue string #" + currentStringIndex);

            // get string
            string decodedText = gameManager.gameConsole.DecodeColorString(dialogue[currentStringIndex]);
            currentWords = decodedText.Split(' ');
            textComponent.text = ""; // reset text

            // iterate through string with delay
            for (int i = 0; i < currentWords.Length; i++)
            {
                textComponent.text += currentWords[i] + " ";
                yield return new WaitForSeconds(wordDelay);
            }

            yield return new WaitForSeconds(0.25f);

            // after string is shown, wait for player input
            bool stringDisplayed = false;
            while (!stringDisplayed)
            {
                if (Input.anyKeyDown)
                {
                    stringDisplayed = true;
                }
                yield return null;
            }

            currentStringIndex++;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator GameDialogueRoutine(TextMeshProUGUI textComponent, string dialogue, float wordDelay)
    {
        inDialogue = true;

        string[] words = gameManager.gameConsole.DecodeColorString(dialogue).Split(' ');
        textComponent.text = "";

        for (int i = 0; i < words.Length; i++)
        {
            textComponent.text += words[i] + " ";
            yield return new WaitForSeconds(wordDelay);
        }

        // after string is shown, wait for player input
        bool stringDisplayed = false;
        while (!stringDisplayed)
        {
            if (Input.anyKeyDown)
            {
                stringDisplayed = true;
            }
            yield return null;
        }

        inDialogue = false;
        DisableDialogue();
    }

    IEnumerator TimedGameDialogueRoutine(TextMeshProUGUI textComponent, List<string> dialogue, float wordDelay, float sentenceDelay)
    {
        inDialogue = true;

        int currentStringIndex = 0;
        string[] currentWords;

        while (currentStringIndex < dialogue.Count)
        {
            Debug.Log("Timed Dialogue string #" + currentStringIndex);

            // get string
            string decodedText = gameManager.gameConsole.DecodeColorString(dialogue[currentStringIndex]);
            currentWords = decodedText.Split(' ');
            textComponent.text = ""; // reset text

            // iterate through string with delay
            for (int i = 0; i < currentWords.Length; i++)
            {
                textComponent.text += currentWords[i] + " ";
                yield return new WaitForSeconds(wordDelay);
            }

            yield return new WaitForSeconds(sentenceDelay);

            currentStringIndex++;
        }

        inDialogue = false;
        DisableDialogue();
    }
    #endregion

}
