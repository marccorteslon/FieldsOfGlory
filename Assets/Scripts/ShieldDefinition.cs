using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Shield")]
public class ShieldDefinition : EquipmentDefinition
// Define un escudo equipable y fuerza automáticamente su slot a Shield.
{
    private void OnValidate() => slot = EquipmentSlot.Shield;
}
