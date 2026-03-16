using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Shop")]
public class ShopDefinition : ScriptableObject
{
    public string shopId;
    public List<string> itemIds = new();
}