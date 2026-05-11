using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Events/Event Node Definition")]
public class EventNodeDefinition : ScriptableObject
{
    public string eventId;
    public string title;
    
    [TextArea(5, 10)]
    public string description;
    
    public Sprite eventImage;
    
    public List<EventChoice> choices = new List<EventChoice>();
}
