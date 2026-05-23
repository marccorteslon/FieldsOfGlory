using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectManager : MonoBehaviour
{
    public enum NegativeType
    {
        Fog,
        Rain,
        IncreaseWinPoints,
        IncreaseDefenseSpeed
    }

    public enum RewardType
    {
        ExtraGoldFlat,
        ExtraGoldMultiplier,
        RandomItem
    }

    [System.Serializable]
    public class EffectChoiceButton
    {
        public Button button;
        [Tooltip("Muestra la descripción de la RECOMPENSA (se mantiene el nombre positiveText para no romper bindings de la escena).")]
        public TextMeshProUGUI positiveText; 
        [Tooltip("Muestra la descripción de la DIFICULTAD/CLIMA (penalizador).")]
        public TextMeshProUGUI negativeText; 

        [HideInInspector] public NegativeType negativeType;
        [HideInInspector] public RewardType rewardType;
        [HideInInspector] public string negativeName;
        [HideInInspector] public string rewardName;

        [HideInInspector] public int extraWinPoints;
        [HideInInspector] public float defenseSpeedMultiplier;
        [HideInInspector] public int flatGoldReward;
        [HideInInspector] public float goldMultiplier;
    }

    [Header("Choice UI")]
    public GameObject choicePanel;
    public EffectChoiceButton[] choiceButtons = new EffectChoiceButton[3];
    
    [Tooltip("Botón opcional para no seleccionar ningún modificador y jugar la justa de forma normal.")]
    public Button noModifierButton;

    [Header("Active Card Selections")]
    public bool hasActiveCard = false;
    public NegativeType activeNegative;
    public RewardType activeReward;

    public int appliedExtraWinPoints = 0;
    public float appliedDefenseSpeedMultiplier = 1f;
    public int appliedFlatGoldReward = 0;
    public float appliedGoldMultiplier = 1f;

    [Header("Active Bools")]
    public bool fogIsActive;
    public bool rainIsActive;

    [Header("Fog settings")]
    public bool originalFogState;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.04f;

    [Header("Rain UI")]
    public Image rainImage;

    [Header("Round Text")]
    public TextMeshProUGUI effectText;
    public float textDuration = 2.5f;

    private Coroutine textCoroutine;
    private Action onEffectsChosen;

    // Mantenemos enums anteriores obsoletos como compatibilidad por si acaso
    public enum JoustEffectType { EfectoPositivo1, EfectoPositivo2, EfectoPositivo3, EfectoPositivo4, Fog, Rain, EfectoNegativo3, EfectoNegativo4 }
    public enum EffectKind { Positive, Negative }
    [System.Serializable] public class JoustEffect { public JoustEffectType type; public EffectKind kind; public string displayName; public bool canAppear = true; }
    [HideInInspector] public List<JoustEffect> positiveEffects = new List<JoustEffect>();
    [HideInInspector] public List<JoustEffect> negativeEffects = new List<JoustEffect>();
    [HideInInspector] public bool efectoPositivo1IsActive, efectoPositivo2IsActive, efectoPositivo3IsActive, efectoPositivo4IsActive, efectoNegativo3IsActive, efectoNegativo4IsActive;

    void Awake()
    {
        originalFogState = RenderSettings.fog;
        DisableAllEffects();
        HideEffectChoices();
        
        // Búsqueda dinámica robusta del botón 'Sin Modificador' si no está asignado
        if (noModifierButton == null && choicePanel != null)
        {
            Button[] buttons = choicePanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string nameLower = b.gameObject.name.ToLower();
                if (nameLower.Contains("no") || nameLower.Contains("skip") || nameLower.Contains("cancel") || nameLower.Contains("sin"))
                {
                    noModifierButton = b;
                    break;
                }
            }
        }
    }

    public void ShowEffectChoices(Action callback)
    {
        // Limpiamos estados de rondas anteriores
        DisableAllEffects();

        onEffectsChosen = callback;

        if (choicePanel != null)
            choicePanel.SetActive(true);

        // Preparamos listas para elegir combinaciones aleatorias y únicas en los 3 botones
        List<NegativeType> availableNegatives = new List<NegativeType>
        {
            NegativeType.Fog,
            NegativeType.Rain,
            NegativeType.IncreaseWinPoints,
            NegativeType.IncreaseDefenseSpeed
        };

        List<RewardType> availableRewards = new List<RewardType>
        {
            RewardType.ExtraGoldFlat,
            RewardType.ExtraGoldMultiplier,
            RewardType.RandomItem
        };

        // Mezclar las listas de forma aleatoria para dar diversidad
        ShuffleList(availableNegatives);
        ShuffleList(availableRewards);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            EffectChoiceButton choiceButton = choiceButtons[i];

            if (choiceButton == null || choiceButton.button == null)
                continue;

            // Obtenemos un tipo negativo y uno positivo (recompensa) de la lista mezclada
            // Usamos modulo por si el pool es menor que el número de botones, aunque tenemos suficientes
            NegativeType neg = availableNegatives[i % availableNegatives.Count];
            RewardType rew = availableRewards[i % availableRewards.Count];

            choiceButton.negativeType = neg;
            choiceButton.rewardType = rew;

            // --- Configuración y asignación de valores de dificultad (Penalizadores) ---
            switch (neg)
            {
                case NegativeType.Fog:
                    choiceButton.negativeName = "Niebla en el Campo";
                    choiceButton.extraWinPoints = 0;
                    choiceButton.defenseSpeedMultiplier = 1f;
                    break;
                case NegativeType.Rain:
                    choiceButton.negativeName = "Lluvia y Viento Tormentoso";
                    choiceButton.extraWinPoints = 0;
                    choiceButton.defenseSpeedMultiplier = 1f;
                    break;
                case NegativeType.IncreaseWinPoints:
                    choiceButton.extraWinPoints = UnityEngine.Random.Range(2, 4) * 10; // +20 o +30 puntos requeridos
                    choiceButton.negativeName = $"+{choiceButton.extraWinPoints} Puntos Requeridos";
                    choiceButton.defenseSpeedMultiplier = 1f;
                    break;
                case NegativeType.IncreaseDefenseSpeed:
                    choiceButton.defenseSpeedMultiplier = 1.5f; // +50% velocidad
                    choiceButton.negativeName = "+50% Velocidad de Ataque Rival";
                    choiceButton.extraWinPoints = 0;
                    break;
            }

            // --- Configuración y asignación de valores de recompensa ---
            switch (rew)
            {
                case RewardType.ExtraGoldFlat:
                    choiceButton.flatGoldReward = UnityEngine.Random.Range(10, 21) * 10; // +100 a +200 monedas
                    choiceButton.rewardName = $"+{choiceButton.flatGoldReward} Oro al Ganar";
                    choiceButton.goldMultiplier = 1f;
                    break;
                case RewardType.ExtraGoldMultiplier:
                    choiceButton.goldMultiplier = 1.5f; // x1.5 monedas
                    choiceButton.rewardName = "+50% Oro Ganado (x1.5)";
                    choiceButton.flatGoldReward = 0;
                    break;
                case RewardType.RandomItem:
                    choiceButton.rewardName = "Objeto de Equipo Gratis";
                    choiceButton.flatGoldReward = 0;
                    choiceButton.goldMultiplier = 1f;
                    break;
            }

            // Actualización de textos en el botón
            if (choiceButton.negativeText != null)
                choiceButton.negativeText.text = choiceButton.negativeName;

            if (choiceButton.positiveText != null)
                choiceButton.positiveText.text = choiceButton.rewardName;

            // Escuchar el clic de la tarjeta
            int buttonIndex = i;
            choiceButton.button.onClick.RemoveAllListeners();
            choiceButton.button.onClick.AddListener(() => ChooseEffectButton(buttonIndex));
        }

        // Configuración del cuarto botón: Saltar Modificadores (Jugar Seguro)
        if (noModifierButton != null)
        {
            noModifierButton.gameObject.SetActive(true);
            noModifierButton.onClick.RemoveAllListeners();
            noModifierButton.onClick.AddListener(() =>
            {
                hasActiveCard = false;
                DisableAllEffects();
                HideEffectChoices();
                
                if (effectText != null)
                {
                    if (textCoroutine != null) StopCoroutine(textCoroutine);
                    textCoroutine = StartCoroutine(SingleTextRoutine("Sin modificadores activos (Ronda Estándar)"));
                }
                
                onEffectsChosen?.Invoke();
            });
        }
    }

    void ChooseEffectButton(int index)
    {
        if (index < 0 || index >= choiceButtons.Length)
            return;

        EffectChoiceButton choiceButton = choiceButtons[index];

        DisableAllEffects();

        // Activamos y guardamos la información de la carta elegida
        hasActiveCard = true;
        activeNegative = choiceButton.negativeType;
        activeReward = choiceButton.rewardType;

        appliedExtraWinPoints = choiceButton.extraWinPoints;
        appliedDefenseSpeedMultiplier = choiceButton.defenseSpeedMultiplier;
        appliedFlatGoldReward = choiceButton.flatGoldReward;
        appliedGoldMultiplier = choiceButton.goldMultiplier;

        // Mostrar texto flotante del reto asumido
        ShowEffectText(choiceButton.negativeName, choiceButton.rewardName);
        HideEffectChoices();

        onEffectsChosen?.Invoke();
    }

    public void ApplyActiveNegativeModifiers()
    {
        if (!hasActiveCard) return;

        JoustManager joust = FindFirstObjectByType<JoustManager>();
        if (joust == null) return;

        // 1. Modificadores climáticos (visuales)
        if (activeNegative == NegativeType.Fog)
        {
            fogIsActive = true;
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            Debug.Log("[EffectManager] Niebla del reto activada.");
        }
        else if (activeNegative == NegativeType.Rain)
        {
            rainIsActive = true;
            if (rainImage != null)
                rainImage.gameObject.SetActive(true);
            Debug.Log("[EffectManager] Lluvia del reto activada.");
        }
        // 2. Modificadores de dificultad (estadísticas)
        else if (activeNegative == NegativeType.IncreaseWinPoints)
        {
            if (joust.winManager != null)
            {
                joust.winManager.winPoints += appliedExtraWinPoints;
                Debug.Log($"[EffectManager] Aumento de puntos por reto: +{appliedExtraWinPoints}. Total requerido: {joust.winManager.winPoints}");
            }
        }
        else if (activeNegative == NegativeType.IncreaseDefenseSpeed)
        {
            if (joust.defensePart != null)
            {
                joust.defensePart.attackMoveSpeed *= appliedDefenseSpeedMultiplier;
                Debug.Log($"[EffectManager] Aumento de velocidad de defensa por reto: x{appliedDefenseSpeedMultiplier}. Nueva velocidad: {joust.defensePart.attackMoveSpeed}");
            }
        }
    }

    public void DisableAllEffects()
    {
        hasActiveCard = false;
        fogIsActive = false;
        rainIsActive = false;

        appliedExtraWinPoints = 0;
        appliedDefenseSpeedMultiplier = 1f;
        appliedFlatGoldReward = 0;
        appliedGoldMultiplier = 1f;

        RenderSettings.fog = originalFogState;

        if (rainImage != null)
            rainImage.gameObject.SetActive(false);
    }

    void HideEffectChoices()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    void ShowEffectText(string negName, string rewName)
    {
        if (effectText == null)
            return;

        if (textCoroutine != null)
            StopCoroutine(textCoroutine);

        textCoroutine = StartCoroutine(EffectTextRoutine(negName, rewName));
    }

    IEnumerator EffectTextRoutine(string negName, string rewName)
    {
        effectText.gameObject.SetActive(true);
        effectText.text = $"Reto: <color=red>{negName}</color>\nPremio: <color=green>{rewName}</color>";

        yield return new WaitForSeconds(textDuration);

        effectText.gameObject.SetActive(false);
    }

    IEnumerator SingleTextRoutine(string text)
    {
        effectText.gameObject.SetActive(true);
        effectText.text = text;

        yield return new WaitForSeconds(textDuration);

        effectText.gameObject.SetActive(false);
    }

    // Algoritmo de mezcla Fisher-Yates para asegurar diversidad en las tarjetas
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}