using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Items/Armor")]
public class ArmorDefinition : EquipmentDefinition
{
    [Header("Armor Mesh Customization")]
    public Mesh armorMesh;

    private void OnValidate()
    {
        slot = EquipmentSlot.Armor;
    }
}