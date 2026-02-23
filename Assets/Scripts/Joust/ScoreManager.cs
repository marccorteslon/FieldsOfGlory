using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int totalScore = 0;

    // ---------------- Fase Caballo ----------------
    public void AddHorsePhaseScore(string zone, int MV, int mV)
    {
        int points = 0;
        switch (zone)
        {
            case "Verde":
                points = MV;
                break;
            case "Amarillo":
                points = mV;
                break;
            case "Rojo":
                points = 0;
                break;
        }

        totalScore += points;
        Debug.Log($"[Caballo] Zona {zone} → +{points} puntos | Total: {totalScore}");
    }

    // ---------------- Fase Ataque ----------------
    public void AddAttackScore(string enemyTag, int BF, int BL, float chargePercent, int MV, int mV)
    {
        int valorZona = enemyTag switch
        {
            "Head" => 4,
            "Body" => 3,
            "Shield" => 2,
            "Horse" => 1,
            _ => 0
        };

        // Puntos por fuerza: BF * (1 + %Carga/100), redondeado al entero más cercano
        int forcePoints = Mathf.RoundToInt(BF * (1 + chargePercent / 100f));

        // Puntos por localización: BL * ValorZona
        int locationPoints = BL * valorZona;

        int total = forcePoints + locationPoints;
        totalScore += total;

        Debug.Log($"[Ataque] Tag {enemyTag} → Fuerza: {forcePoints} + Localización: {locationPoints} = +{total} puntos | Total: {totalScore}");
    }

    // ---------------- Fase Defensa ----------------
    public void ApplyDefense(bool blockedCorrectly, int BB)
    {
        int penalty = blockedCorrectly ? 4 - BB : 4;
        totalScore -= penalty;
        Debug.Log($"[Defensa] Bloqueo correcto: {blockedCorrectly} → Penalización: {penalty} | Total: {totalScore}");
    }

    public int GetScore() => totalScore;
}