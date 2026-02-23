using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinition
// Define un item equipable con un slot y una lista de modificadores de estadísticas.
{
    public EquipmentSlot slot;
    public List<StatModifier> modifiers = new();
}
