using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectManager : MonoBehaviour
{
    public enum JoustEffectType
    {
        Fog,
        Rain
    }

    [System.Serializable]
    public class JoustEffect
    {
        public JoustEffectType type;
        public string displayName;
        public bool canAppear = true;
    }

    [Header("Effects Pool")]
    public List<JoustEffect> possibleEffects = new List<JoustEffect>();

    [Header("Active Bools")]
    public bool fogIsActive;
    public bool rainIsActive;

    [Header("Fog")]
    public bool originalFogState;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.04f;

    [Header("Rain UI")]
    public Image rainImage;

    [Header("Round Text")]
    public TextMeshProUGUI effectText;
    public float textDuration = 2f;

    private Coroutine textCoroutine;

    void Awake()
    {
        originalFogState = RenderSettings.fog;
        DisableAllEffects();
    }

    public void ChooseRandomEffect()
    {
        DisableAllEffects();

        List<JoustEffect> availableEffects = new List<JoustEffect>();

        foreach (JoustEffect effect in possibleEffects)
        {
            if (effect.canAppear)
                availableEffects.Add(effect);
        }

        if (availableEffects.Count == 0)
            return;

        JoustEffect chosenEffect = availableEffects[Random.Range(0, availableEffects.Count)];

        ActivateEffect(chosenEffect.type);
        ShowEffectText(chosenEffect.displayName);
    }

    void ActivateEffect(JoustEffectType effectType)
    {
        switch (effectType)
        {
            case JoustEffectType.Fog:
                fogIsActive = true;
                RenderSettings.fog = true;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                break;

            case JoustEffectType.Rain:
                rainIsActive = true;

                if (rainImage != null)
                    rainImage.gameObject.SetActive(true);
                break;
        }
    }

    public void DisableAllEffects()
    {
        fogIsActive = false;
        rainIsActive = false;

        RenderSettings.fog = originalFogState;

        if (rainImage != null)
            rainImage.gameObject.SetActive(false);
    }

    void ShowEffectText(string effectName)
    {
        if (effectText == null)
            return;

        if (textCoroutine != null)
            StopCoroutine(textCoroutine);

        textCoroutine = StartCoroutine(EffectTextRoutine(effectName));
    }

    IEnumerator EffectTextRoutine(string effectName)
    {
        effectText.gameObject.SetActive(true);
        effectText.text = "Efecto de la ronda: " + effectName;

        yield return new WaitForSeconds(textDuration);

        effectText.gameObject.SetActive(false);
    }
}