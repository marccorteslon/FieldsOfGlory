using System;

[Serializable]
public struct StatModifier
// Representa una modificación concreta a una stat (tipo, modo y valor).
{
    public StatType stat;
    public StatModType type;
    public float value; // Flat: +X | Percent: 0.10 = +10%
}
