using UnityEngine;

public enum ArmorSlotType { Head, Chest, Hands, Leggings, Boots }

[System.Serializable]
public class ArmorData
{
    public string id;
    public string armorName;
    public string description;
    public ArmorSlotType slot;

    public StatType positiveStat;
    public int positiveAmount;

    public StatType negativeStat;
    public int negativeAmount; // stored as a positive magnitude in the sheet (e.g. 10 means "-10")

    public Sprite icon;
}
