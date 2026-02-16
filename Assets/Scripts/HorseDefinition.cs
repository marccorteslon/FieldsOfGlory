using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Horse")]
public class HorseDefinition : EquipmentDefinition
{
    private void OnValidate() => slot = EquipmentSlot.Horse;
}
