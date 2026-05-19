using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectManager : MonoBehaviour
{
    public enum JoustEffectType
    {
        // Positivos
        EfectoPositivo1,
        EfectoPositivo2,
        EfectoPositivo3,
        EfectoPositivo4,

        // Negativos
        Fog,
        Rain,
        EfectoNegativo3,
        EfectoNegativo4
    }

    public enum EffectKind
    {
        Positive,
        Negative
    }

    [System.Serializable]
    public class JoustEffect
    {
        public JoustEffectType type;
        public EffectKind kind;
        public string displayName;
        public bool canAppear = true;
    }

    [System.Serializable]
    public class EffectChoiceButton
    {
        public Button button;
        public TextMeshProUGUI positiveText;
        public TextMeshProUGUI negativeText;

        [HideInInspector] public JoustEffect positiveEffect;
        [HideInInspector] public JoustEffect negativeEffect;
    }

    [Header("Effects Pool")]
    public List<JoustEffect> positiveEffects = new List<JoustEffect>();
    public List<JoustEffect> negativeEffects = new List<JoustEffect>();

    [Header("Choice UI")]
    public GameObject choicePanel;
    public EffectChoiceButton[] choiceButtons = new EffectChoiceButton[3];

    [Header("Active Bools")]
    public bool fogIsActive;
    public bool rainIsActive;

    public bool efectoPositivo1IsActive;
    public bool efectoPositivo2IsActive;
    public bool efectoPositivo3IsActive;
    public bool efectoPositivo4IsActive;

    public bool efectoNegativo3IsActive;
    public bool efectoNegativo4IsActive;

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
    private Action onEffectsChosen;

    void Awake()
    {
        originalFogState = RenderSettings.fog;
        CreateTestEffectsIfEmpty();
        DisableAllEffects();
        HideEffectChoices();
    }

    void CreateTestEffectsIfEmpty()
    {
        if (positiveEffects.Count == 0)
        {
            positiveEffects.Add(new JoustEffect { type = JoustEffectType.EfectoPositivo1, kind = EffectKind.Positive, displayName = "Efecto Positivo 1" });
            positiveEffects.Add(new JoustEffect { type = JoustEffectType.EfectoPositivo2, kind = EffectKind.Positive, displayName = "Efecto Positivo 2" });
            positiveEffects.Add(new JoustEffect { type = JoustEffectType.EfectoPositivo3, kind = EffectKind.Positive, displayName = "Efecto Positivo 3" });
            positiveEffects.Add(new JoustEffect { type = JoustEffectType.EfectoPositivo4, kind = EffectKind.Positive, displayName = "Efecto Positivo 4" });
        }

        if (negativeEffects.Count == 0)
        {
            negativeEffects.Add(new JoustEffect { type = JoustEffectType.Fog, kind = EffectKind.Negative, displayName = "Niebla" });
            negativeEffects.Add(new JoustEffect { type = JoustEffectType.Rain, kind = EffectKind.Negative, displayName = "Lluvia" });
            negativeEffects.Add(new JoustEffect { type = JoustEffectType.EfectoNegativo3, kind = EffectKind.Negative, displayName = "Efecto Negativo 3" });
            negativeEffects.Add(new JoustEffect { type = JoustEffectType.EfectoNegativo4, kind = EffectKind.Negative, displayName = "Efecto Negativo 4" });
        }
    }

    public void ShowEffectChoices(Action callback)
    {
        DisableAllEffects();

        onEffectsChosen = callback;

        if (choicePanel != null)
            choicePanel.SetActive(true);

        List<JoustEffect> availablePositiveEffects = GetAvailableEffects(positiveEffects);
        List<JoustEffect> availableNegativeEffects = GetAvailableEffects(negativeEffects);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            EffectChoiceButton choiceButton = choiceButtons[i];

            if (choiceButton == null || choiceButton.button == null)
                continue;

            JoustEffect positiveEffect = GetRandomEffect(availablePositiveEffects);
            JoustEffect negativeEffect = GetRandomEffect(availableNegativeEffects);

            choiceButton.positiveEffect = positiveEffect;
            choiceButton.negativeEffect = negativeEffect;

            if (choiceButton.positiveText != null)
                choiceButton.positiveText.text = positiveEffect != null ? positiveEffect.displayName : "Sin efecto positivo";

            if (choiceButton.negativeText != null)
                choiceButton.negativeText.text = negativeEffect != null ? negativeEffect.displayName : "Sin efecto negativo";

            int buttonIndex = i;
            choiceButton.button.onClick.RemoveAllListeners();
            choiceButton.button.onClick.AddListener(() => ChooseEffectButton(buttonIndex));
        }
    }

    List<JoustEffect> GetAvailableEffects(List<JoustEffect> effects)
    {
        List<JoustEffect> availableEffects = new List<JoustEffect>();

        foreach (JoustEffect effect in effects)
        {
            if (effect.canAppear)
                availableEffects.Add(effect);
        }

        return availableEffects;
    }

    JoustEffect GetRandomEffect(List<JoustEffect> availableEffects)
    {
        if (availableEffects.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, availableEffects.Count);
        JoustEffect chosenEffect = availableEffects[randomIndex];

        availableEffects.RemoveAt(randomIndex);

        return chosenEffect;
    }

    void ChooseEffectButton(int index)
    {
        if (index < 0 || index >= choiceButtons.Length)
            return;

        EffectChoiceButton choiceButton = choiceButtons[index];

        DisableAllEffects();

        if (choiceButton.positiveEffect != null)
            ActivateEffect(choiceButton.positiveEffect.type);

        if (choiceButton.negativeEffect != null)
            ActivateEffect(choiceButton.negativeEffect.type);

        ShowEffectText(choiceButton.positiveEffect, choiceButton.negativeEffect);
        HideEffectChoices();

        onEffectsChosen?.Invoke();
    }

    void HideEffectChoices()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    void ActivateEffect(JoustEffectType effectType)
    {
        switch (effectType)
        {
            case JoustEffectType.EfectoPositivo1:
                efectoPositivo1IsActive = true;
                break;

            case JoustEffectType.EfectoPositivo2:
                efectoPositivo2IsActive = true;
                break;

            case JoustEffectType.EfectoPositivo3:
                efectoPositivo3IsActive = true;
                break;

            case JoustEffectType.EfectoPositivo4:
                efectoPositivo4IsActive = true;
                break;

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

            case JoustEffectType.EfectoNegativo3:
                efectoNegativo3IsActive = true;
                break;

            case JoustEffectType.EfectoNegativo4:
                efectoNegativo4IsActive = true;
                break;
        }
    }

    public void DisableAllEffects()
    {
        fogIsActive = false;
        rainIsActive = false;

        efectoPositivo1IsActive = false;
        efectoPositivo2IsActive = false;
        efectoPositivo3IsActive = false;
        efectoPositivo4IsActive = false;

        efectoNegativo3IsActive = false;
        efectoNegativo4IsActive = false;

        RenderSettings.fog = originalFogState;

        if (rainImage != null)
            rainImage.gameObject.SetActive(false);
    }

    void ShowEffectText(JoustEffect positiveEffect, JoustEffect negativeEffect)
    {
        if (effectText == null)
            return;

        if (textCoroutine != null)
            StopCoroutine(textCoroutine);

        string positiveName = positiveEffect != null ? positiveEffect.displayName : "Ninguno";
        string negativeName = negativeEffect != null ? negativeEffect.displayName : "Ninguno";

        textCoroutine = StartCoroutine(EffectTextRoutine(positiveName, negativeName));
    }

    IEnumerator EffectTextRoutine(string positiveName, string negativeName)
    {
        effectText.gameObject.SetActive(true);
        effectText.text = "Efecto positivo: " + positiveName + "\nEfecto negativo: " + negativeName;

        yield return new WaitForSeconds(textDuration);

        effectText.gameObject.SetActive(false);
    }
}