using System;

[Serializable]
public struct StatModifier
{
    public StatType stat;
    public StatModType type;
    public float value; // Flat: +X | Percent: 0.10 = +10%
}
