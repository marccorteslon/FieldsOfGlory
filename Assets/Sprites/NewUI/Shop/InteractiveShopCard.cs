using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InteractiveShopCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatAmount = 5f;
    
    [Header("Selection & Bump Settings")]
    public float scaleAmount = 1.05f;
    public float transitionSpeed = 10f;
    public float bumpStrength = 30f;
    public float bumpRecoverySpeed = 5f;
    
    [Header("References")]
    public ShopUIManager shopManager;
    public int slotIndex;
    
    [Header("UI Elements from this Card")]
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text modifiersText;
    public Image iconImage;
    [HideInInspector] public Image cardBackgroundImage;
    
    private RectTransform rectTransform;
    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private bool isSelected = false;
    private float floatTimer;
    private Vector3 bumpOffset = Vector3.zero;

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
        if (isSelected)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale * scaleAmount, Time.deltaTime * transitionSpeed);
            rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, originalLocalPosition, Time.deltaTime * transitionSpeed);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, originalRotation, Time.deltaTime * transitionSpeed);
            bumpOffset = Vector3.zero;
        }
        else
        {
            floatTimer += Time.deltaTime;
            float newY = originalLocalPosition.y + Mathf.Sin(floatTimer * floatSpeed) * floatAmount;
            Vector3 targetPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z) + bumpOffset;
            
            rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, originalRotation, Time.deltaTime * transitionSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, originalScale, Time.deltaTime * transitionSpeed);
            
            bumpOffset = Vector3.Lerp(bumpOffset, Vector3.zero, Time.deltaTime * bumpRecoverySpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) 
    { 
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
    
    public void OnPointerExit(PointerEventData eventData) 
    { 
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        StartCoroutine(CalculateBumpNextFrame());
    }

    private System.Collections.IEnumerator CalculateBumpNextFrame()
    {
        yield return null;

        GameObject nextSelected = EventSystem.current.currentSelectedGameObject;
        if (nextSelected != null && nextSelected != gameObject)
        {
            Vector3 worldDiff = nextSelected.transform.position - transform.position;
            Vector3 localDiff = transform.InverseTransformDirection(worldDiff);
            localDiff.z = 0;
            bumpOffset = localDiff.normalized * bumpStrength;
        }
        else
        {
            bumpOffset = Vector3.zero;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenDetailed();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OpenDetailed();
    }

    private void OpenDetailed()
    {
        isSelected = false;
        
        if (iconImage != null && (!iconImage.enabled || iconImage.sprite == null)) return;
        
        if (shopManager != null)
        {
            shopManager.OpenDetailedView(this);
        }
    }
    
    public void ResetCard()
    {
        isSelected = false;
        bumpOffset = Vector3.zero;
        rectTransform.localPosition = originalLocalPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;
    }
}



