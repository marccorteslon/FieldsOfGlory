using UnityEngine;

public abstract class ItemDefinition : ScriptableObject
{
    public string id;           // por ahora opcional, útil luego
    public string displayName;
    public Sprite icon;
}
