using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Lance")]
public class LanceDefinition : EquipmentDefinition
{
    private void OnValidate() => slot = EquipmentSlot.Lance;
}
