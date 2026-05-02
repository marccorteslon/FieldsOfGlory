using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InteractiveShopCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatAmount = 5f;
    
    [Header("Hover Settings")]
    public float maxRotationAngle = 15f;
    public float moveAmount = 15f; // How far it moves towards the mouse
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 10f;
    
    [Header("Item Data & References")]
    public ShopUIManager shopManager;
    public ItemDefinition itemData; // Drop your Scriptable Object here!
    public TMP_Text smallPriceTag; // Drag the text object from the small card here
    
    private RectTransform rectTransform;
    private Image cardImage;
    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private bool isHovered = false;
    private float floatTimer;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
        
        // Automatically set up the card using the ScriptableObject
        if (itemData != null)
        {
            if (cardImage != null) cardImage.sprite = itemData.icon;
            if (smallPriceTag != null) smallPriceTag.text = itemData.price.ToString() + " Gold";
        }
        
        originalLocalPosition = rectTransform.localPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        
        // Randomize the start of the sine wave so cards don't float in perfect sync
        floatTimer = Random.Range(0f, 10f); 
    }

    void Update()
    {
        if (isHovered)
        {
            HandleHoverMovement();
        }
        else
        {
            HandleFloating();
        }
    }

    private void HandleFloating()
    {
        floatTimer += Time.deltaTime;
        
        // Calculate the up and down floating movement
        float newY = originalLocalPosition.y + Mathf.Sin(floatTimer * floatSpeed) * floatAmount;
        Vector3 targetPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);
        
        // Smoothly return to default rotation, scale, and the floating position
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, originalRotation, Time.deltaTime * transitionSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale, Time.deltaTime * transitionSpeed);
    }

    private void HandleHoverMovement()
    {
        // Find where the mouse is relative to the center of the card
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localMousePos);

        // Normalize the position to a -1 to 1 range
        Vector2 normalizedPos = new Vector2(
            Mathf.Clamp(localMousePos.x / (rectTransform.rect.width * 0.5f), -1f, 1f),
            Mathf.Clamp(localMousePos.y / (rectTransform.rect.height * 0.5f), -1f, 1f)
        );

        // Calculate rotation (tilt towards mouse) and position (move slightly towards mouse)
        Quaternion targetRotation = Quaternion.Euler(-normalizedPos.y * maxRotationAngle, normalizedPos.x * maxRotationAngle, 0);
        Vector3 targetPosition = originalLocalPosition + new Vector3(normalizedPos.x * moveAmount, normalizedPos.y * moveAmount, 0);
        
        // Apply rotation, scale, and position smoothly
        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale * scaleAmount, Time.deltaTime * transitionSpeed);
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) { isHovered = true; }
    public void OnPointerExit(PointerEventData eventData) { isHovered = false; }

    public void OnPointerClick(PointerEventData eventData)
    {
        isHovered = false;
        if (shopManager != null)
        {
            shopManager.OpenDetailedView(this);
        }
    }
    
    // Called when closing the shop or backing out
    public void ResetCard()
    {
        isHovered = false;
        rectTransform.localPosition = originalLocalPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;
    }
}
