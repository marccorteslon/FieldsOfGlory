using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public ProgressSaveData data;

    void Awake()
    {
        data = ProgressSaveSystem.Load();
    }

    public int Money => data.money;

    public void AddMoney(int amount)
    {
        data.money += Mathf.Max(0, amount);
        ProgressSaveSystem.Save(data);
        Debug.Log($"[Progress] Dinero actual: {data.money}");
    }

    public int CalculateReward(int enemyHpWinPoints, int winRoundNumber)
    {
        float mult = winRoundNumber switch
        {
            1 => 2.0f,
            2 => 1.5f,
            _ => 1.0f // ronda 3 o cualquier otra
        };

        return Mathf.RoundToInt(enemyHpWinPoints * mult);
    }
}