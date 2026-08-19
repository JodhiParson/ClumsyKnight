public enum StatType { Attack, Health, Speed }

[System.Serializable]
public class UpgradeData
{
    public string id;
    public StatType stat;
    public int amount;
}
