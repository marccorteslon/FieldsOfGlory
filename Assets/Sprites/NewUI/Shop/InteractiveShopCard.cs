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
    
    [Header("References")]
    public ShopUIManager shopManager;
    public int slotIndex; // Important! Set to 0, 1, 2, or 3 depending on which card this is
    
    [Header("UI Elements from this Card")]
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text modifiersText; // This holds the description/stats from ShopPanelController
    public Image iconImage;
    [HideInInspector] public Image cardBackgroundImage; // Automatically grabbed
    
    private RectTransform rectTransform;
    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private bool isHovered = false;
    private float floatTimer;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardBackgroundImage = GetComponent<Image>();
        originalLocalPosition = rectTransform.localPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        
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
        float newY = originalLocalPosition.y + Mathf.Sin(floatTimer * floatSpeed) * floatAmount;
        Vector3 targetPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);
        
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, originalRotation, Time.deltaTime * transitionSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale, Time.deltaTime * transitionSpeed);
    }

    private void HandleHoverMovement()
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localMousePos);

        Vector2 normalizedPos = new Vector2(
            Mathf.Clamp(localMousePos.x / (rectTransform.rect.width * 0.5f), -1f, 1f),
            Mathf.Clamp(localMousePos.y / (rectTransform.rect.height * 0.5f), -1f, 1f)
        );

        Quaternion targetRotation = Quaternion.Euler(-normalizedPos.y * maxRotationAngle, normalizedPos.x * maxRotationAngle, 0);
        Vector3 targetPosition = originalLocalPosition + new Vector3(normalizedPos.x * moveAmount, normalizedPos.y * moveAmount, 0);
        
        rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale * scaleAmount, Time.deltaTime * transitionSpeed);
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) { isHovered = true; }
    public void OnPointerExit(PointerEventData eventData) { isHovered = false; }

    public void OnPointerClick(PointerEventData eventData)
    {
        isHovered = false;
        
        // Don't open if this slot is empty (ShopPanelController disables the image or sprite)
        if (iconImage != null && (!iconImage.enabled || iconImage.sprite == null)) return;
        
        if (shopManager != null)
        {
            shopManager.OpenDetailedView(this);
        }
    }
    
    public void ResetCard()
    {
        isHovered = false;
        rectTransform.localPosition = originalLocalPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;
    }
}
