using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject cardsGridPanel; // The parent object holding your 4 small cards
    public GameObject detailedViewPanel; // The parent object for the big card, Buy, and Back buttons
    
    [Header("Detailed View Elements")]
    public Image bigItemImage; 
    public TMP_Text detailedNameText;
    public TMP_Text detailedPriceText; 
    public TMP_Text detailedDescriptionText;
    
    private InteractiveShopCard currentlySelectedCard;

    void Start()
    {
        // Ensure starting state is correct
        if (cardsGridPanel != null) cardsGridPanel.SetActive(true);
        if (detailedViewPanel != null) detailedViewPanel.SetActive(false);
    }

    public void OpenDetailedView(InteractiveShopCard clickedCard)
    {
        currentlySelectedCard = clickedCard;
        
        // Hide the 4 small cards
        if (cardsGridPanel != null) cardsGridPanel.SetActive(false);
        
        // Show the big centered card panel
        if (detailedViewPanel != null) detailedViewPanel.SetActive(true);
        
        // Map the ScriptableObject data to the big UI
        if (clickedCard.itemData != null)
        {
            if (bigItemImage != null) bigItemImage.sprite = clickedCard.itemData.icon;
            if (detailedNameText != null) detailedNameText.text = clickedCard.itemData.displayName;
            if (detailedPriceText != null) detailedPriceText.text = clickedCard.itemData.price.ToString() + " Gold";
            if (detailedDescriptionText != null) detailedDescriptionText.text = clickedCard.itemData.description;
        }
    }

    public void CloseDetailedView()
    {
        if (detailedViewPanel != null) detailedViewPanel.SetActive(false);
        if (cardsGridPanel != null) cardsGridPanel.SetActive(true);
        
        // Instantly reset the card we just backed out of so it doesn't get stuck rotated
        if (currentlySelectedCard != null)
        {
            currentlySelectedCard.ResetCard();
            currentlySelectedCard = null;
        }
    }
    
    public void BuyItem()
    {
        if (currentlySelectedCard != null && currentlySelectedCard.itemData != null)
        {
            Debug.Log("Bought item: " + currentlySelectedCard.itemData.displayName + " for " + currentlySelectedCard.itemData.price + " Gold");
            // Add your purchase logic/currency deduction here
        }
    }
}
