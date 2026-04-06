using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Travel Route")]
public class TravelRouteDefinition : ScriptableObject
{
    public string cityAId;
    public string cityBId;
    public int travelDays = 1;
}