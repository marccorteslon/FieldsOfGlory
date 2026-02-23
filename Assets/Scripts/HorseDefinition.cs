using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Horse")]
public class HorseDefinition : EquipmentDefinition
// Define un caballo equipable y fuerza automáticamente su slot a Horse.
{
    private void OnValidate() => slot = EquipmentSlot.Horse;
}
