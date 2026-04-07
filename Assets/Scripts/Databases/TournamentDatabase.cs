using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tournament Database")]
public class TournamentDatabase : ScriptableObject
{
    public List<TournamentDefinition> tournaments = new();
}