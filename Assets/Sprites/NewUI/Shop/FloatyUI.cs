using UnityEngine;

public class FloatyUI : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatAmount = 10f;
    
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
        Initialize();
        
        floatTimer = Random.Range(0f, 10f);
        
        rectTransform.localPosition = originalLocalPosition; 
    }

    void Update()
    {
        floatTimer += Time.deltaTime;
        
        float newY = originalLocalPosition.y + Mathf.Sin(floatTimer * floatSpeed) * floatAmount;
        
        rectTransform.localPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);
    }
}
