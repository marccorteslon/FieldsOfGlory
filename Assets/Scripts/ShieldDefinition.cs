using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Shield")]
public class ShieldDefinition : EquipmentDefinition
{
    private void OnValidate() => slot = EquipmentSlot.Shield;
}
