using UnityEngine;

public enum JoustDifficulty { Easy, Normal, Hard, Epic }

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tournament")]
public class TournamentDefinition : ScriptableObject
{
    public string tournamentId;
    public string displayName;
    public string cityId;
    public int day = 1;
    public int month = 1;
    public JoustDifficulty difficulty = JoustDifficulty.Normal;
}