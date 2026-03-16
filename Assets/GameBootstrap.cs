using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public CityDatabase cityDatabase;
    public ShopDatabase shopDatabase;
    public TavernDatabase tavernDatabase;

    [Header("Debug")]
    public bool useMockRepository = false;

    void Awake()
    {
        if (useMockRepository)
        {
            GameManager.dataRepository = new DataMockRepository();
        }
        else
        {
            GameManager.dataRepository = new DataSORepository(cityDatabase, shopDatabase, tavernDatabase);
        }
    }
}