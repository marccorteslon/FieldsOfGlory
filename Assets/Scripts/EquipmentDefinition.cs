using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinition
// Define un item equipable con un slot y una lista de modificadores de estadísticas.
{
    [Header("Visuals")]
    public GameObject visualPrefab;
    public Material visualMaterial;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;

    public EquipmentSlot slot;
    public List<StatModifier> modifiers = new();
}
