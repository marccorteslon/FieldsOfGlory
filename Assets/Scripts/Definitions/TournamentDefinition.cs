using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tournament")]
public class TournamentDefinition : ScriptableObject
{
    public string tournamentId;
    public string displayName;
    public string cityId;
    public int day = 1;
    public int month = 1;
}