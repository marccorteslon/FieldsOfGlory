using UnityEngine;

public class FloatyUI : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatAmount = 10f; // Default amount
    
    private RectTransform rectTransform;
    private Vector3 originalLocalPosition;
    private float floatTimer;
    private bool isInitialized = false;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (!isInitialized)
        {
            rectTransform = GetComponent<RectTransform>();
            originalLocalPosition = rectTransform.localPosition;
            isInitialized = true;
        }
    }

    void OnEnable()
    {
        // Make sure it's initialized (OnEnable can fire before Awake if toggled quickly)
        Initialize();
        
        // Randomize the start so it looks natural
        floatTimer = Random.Range(0f, 10f);
        
        // Snap back to the center starting position whenever this screen opens
        rectTransform.localPosition = originalLocalPosition; 
    }

    void Update()
    {
        floatTimer += Time.deltaTime;
        
        // Calculate the smooth up and down movement
        float newY = originalLocalPosition.y + Mathf.Sin(floatTimer * floatSpeed) * floatAmount;
        
        // Apply the position instantly (no lerp needed here because the Sine wave is already smooth)
        rectTransform.localPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);
    }
}
