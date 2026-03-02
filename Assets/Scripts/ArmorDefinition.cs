using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Armor")]
public class ArmorDefinition : EquipmentDefinition
{
    private void OnValidate()
    {
        slot = EquipmentSlot.Armor;
    }
}