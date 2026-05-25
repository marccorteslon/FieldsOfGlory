using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("Link to Existing Logic")]
    public ShopPanelController shopPanelController; 
    [Header("Shop Title")]
    public TMP_Text shopTitleText;

    [Header("UI Panels")]
    public GameObject cardsGridPanel; 
    public GameObject detailedViewPanel; 
    
    [Header("Controller Support")]
    public GameObject firstSelectedInDetailedView; 
    
    [Header("Detailed View Elements")]
    public Image detailedCardBackgroundImage;
    public Image detailedIconImage; 
    public TMP_Text detailedNameText;
    public TMP_Text detailedPriceText; 
    public TMP_Text detailedDescriptionText;
    
    private InteractiveShopCard currentlySelectedCard;

    void Start()
    {
        if (cardsGridPanel != null) cardsGridPanel.SetActive(true);
        if (detailedViewPanel != null) detailedViewPanel.SetActive(false);
    }

    public void OpenDetailedView(InteractiveShopCard clickedCard)
    {
        currentlySelectedCard = clickedCard;
        
        if (cardsGridPanel != null) cardsGridPanel.SetActive(false);
        if (detailedViewPanel != null) detailedViewPanel.SetActive(true);
        
        if (detailedIconImage != null && clickedCard.iconImage != null) 
        {
            detailedIconImage.sprite = clickedCard.iconImage.sprite;
            detailedIconImage.enabled = clickedCard.iconImage.enabled;
        }
        
        if (detailedCardBackgroundImage != null && clickedCard.cardBackgroundImage != null)
        {
            detailedCardBackgroundImage.sprite = clickedCard.cardBackgroundImage.sprite;
            detailedCardBackgroundImage.color = clickedCard.cardBackgroundImage.color;
        }
            
        if (detailedNameText != null && clickedCard.nameText != null) 
            detailedNameText.text = clickedCard.nameText.text;
            
        if (detailedPriceText != null && clickedCard.priceText != null) 
            detailedPriceText.text = clickedCard.priceText.text;
            
        if (detailedDescriptionText != null && clickedCard.modifiersText != null) 
            detailedDescriptionText.text = clickedCard.modifiersText.text;
            
        if (firstSelectedInDetailedView != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedInDetailedView);
        }
    }

    public void SetShopName(string townName)
    {
        if (shopTitleText != null)
        {
            shopTitleText.text = townName + "'s Shop";
        }
    }

    public void CloseDetailedView()
    {
        
        if (detailedViewPanel != null) detailedViewPanel.SetActive(false);
        if (cardsGridPanel != null) cardsGridPanel.SetActive(true);
        
        if (currentlySelectedCard != null)
        {
            EventSystem.current.SetSelectedGameObject(currentlySelectedCard.gameObject);
            
            currentlySelectedCard.ResetCard();
            currentlySelectedCard = null;
        }
    }
    
    public void BuyItem()
    {
        if (currentlySelectedCard != null && shopPanelController != null)
        {
            shopPanelController.Purchase(currentlySelectedCard.slotIndex);
            
            CloseDetailedView();
        }
    }
}


