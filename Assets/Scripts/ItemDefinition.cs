using UnityEngine;


public abstract class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Economy & Info")]
    [TextArea(3, 6)]
    public string description;

    public int price;
}
