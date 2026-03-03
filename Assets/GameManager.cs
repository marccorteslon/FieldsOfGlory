using System;
using UnityEngine;

public class GameManager
{
    //public static GameManager Instance { private set; get; } = new();

#if UNITY_EDITOR
    static public IDataRepository dataRepository = new DataMockRepository("Asssets/QA");
#else
    static public IDataRepository dataRepository = new DataBDDRepository();
#endif

}

public class DummyClass
{
    public void testc()
    {
        City myCity = new City();

        myCity.GetShop(
            onSucces: OnShopSucces,
            onFailure: (error) =>
            {
                Debug.LogError(error);
                throw error;

            });
    }

    public void OnShopSucces(Shop shop)
    {

    }

    public void test2()
    {
        try
        {
            //otras cosas

            testc();

            //Lo que sea
        }
        catch (Exception ex)
        {

        }
    }
}

public class City
{
    public int ID = 0;

    private Shop _shop = null;
    
    public void GetShop(Action<Shop> onSucces, Action<Exception> onFailure)
    {
        if(_shop != null)
        {
            onSucces(_shop);
            return;
        }

        GameManager.dataRepository.GetShopById(ID, 
            onSucces, onFailure);
    }
}

public class Shop
{
    public float a;
    public string b;
}

public interface IDataRepository
{
    #region Shops
    public void GetShopById(int id, Action<Shop> onSucces, Action<Exception> onFailure);

    #endregion

}

public class DataMockRepository : IDataRepository
{
    public DataMockRepository(string path)
    {

    }

    public void GetShopById(int id, Action<Shop> onSucces, Action<Exception> onFailure)
    {
        //Si existe la id
        onSucces(new Shop());

        //Si no existe o cualquier otro error
        onFailure(new());
    }
}

public class DataBDDRepository : IDataRepository
{
    public void GetShopById(int id, Action<Shop> onSucces, Action<Exception> onFailure)
    {
        //Si existe la id en la base de datos
        onSucces(new Shop());

        //Si no existe o cualquier otro error
        onFailure(new());
    }
}