using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    GameManager gameManager;

    [Header("Fullscreen Panic Shader")]
    public Material panicShaderMaterial;
    public float panic_fullscreenIntensity = 0.05f; // The initial value for the _FullscreenIntensity property of the Panic Shader material
    public float panic_transitionSpeed;
    private Coroutine panicCoroutine;

    private void Start()
    {
        gameManager = GetComponentInParent<GameManager>();

        DisablePanicShader();
    }

    private void Update()
    {
        
    }

    public void EnablePanicShader()
    {
        if (panicCoroutine == null)
        {
            panicCoroutine = StartCoroutine(LerpPanicFullscreenIntensity(panic_fullscreenIntensity, panic_transitionSpeed));
        }
    }

    public void DisablePanicShader()
    {
        if (panicCoroutine == null)
        {
            panicCoroutine = StartCoroutine(LerpPanicFullscreenIntensity(0, panic_transitionSpeed));
        }
    }

    IEnumerator LerpPanicFullscreenIntensity(float targetIntensity, float speed)
    {
        float currentIntensity = panicShaderMaterial.GetFloat("_FullscreenIntensity");

        // update intensity
        while (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, speed * Time.deltaTime);
            panicShaderMaterial.SetFloat("_FullscreenIntensity", currentIntensity);
            yield return null;
        }

        panicShaderMaterial.SetFloat("_FullscreenIntensity", targetIntensity);

        panicCoroutine = null;
    }
}
