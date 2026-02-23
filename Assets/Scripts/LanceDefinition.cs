using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Lance")]
public class LanceDefinition : EquipmentDefinition
// Define una lanza equipable y fuerza automáticamente su slot a Lance.
{
    private void OnValidate() => slot = EquipmentSlot.Lance;
}
