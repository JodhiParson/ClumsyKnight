public enum StatType { Attack, Health }

[System.Serializable]
public class UpgradeData
{
    public string id;
    public StatType stat;
    public int amount;
}
