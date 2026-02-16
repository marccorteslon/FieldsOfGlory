using System.Collections.Generic;

[System.Serializable]
public class LoadoutStats
{
    private readonly Dictionary<StatType, float> _final = new();

    public float Get(StatType stat) => _final.TryGetValue(stat, out var v) ? v : 0f;

    public void Recalculate(IEnumerable<StatModifier> modifiers)
    {
        _final.Clear();

        // Flats
        foreach (var m in modifiers)
        {
            if (m.type != StatModType.Flat) continue;
            _final[m.stat] = Get(m.stat) + m.value;
        }

        // Percents (si los usáis)
        foreach (var m in modifiers)
        {
            if (m.type != StatModType.Percent) continue;
            _final[m.stat] = Get(m.stat) * (1f + m.value);
        }
    }
}
