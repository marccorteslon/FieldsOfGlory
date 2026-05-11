using UnityEngine;

[System.Serializable]
public struct EventChoice
{
    [TextArea(2, 4)]
    public string choiceText;
    public EventNodeDefinition nextEvent; // Reference to the next narrative page
    public int moneyReward; // Positive to give money, negative to take money
}
