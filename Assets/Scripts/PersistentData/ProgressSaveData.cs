using System;

[Serializable]
public class ProgressSaveData
{
    public int money = 0;

    public string equippedHorseId;
    public string equippedLanceId;
    public string equippedShieldId;
    public string equippedArmorId;

    public string currentCityId;

    public int currentDay = 1;
    public int currentMonth = 1;
}