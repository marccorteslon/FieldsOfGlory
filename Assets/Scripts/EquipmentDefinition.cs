using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinition
{
    public EquipmentSlot slot;
    public List<StatModifier> modifiers = new();
}
