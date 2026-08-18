using System.Collections.Generic;
using UnityEngine;

// Drag Upgrades_-_Sheet1.csv into your Assets folder (Unity auto-imports .csv as a TextAsset),
// then assign it to `upgradeCsv` on this component in the Inspector.
public class UpgradeDatabase : MonoBehaviour
{
    public static UpgradeDatabase Instance { get; private set; }

    [Tooltip("The Upgrades CSV, imported as a TextAsset (UpgradeID,Stat,Amount)")]
    public TextAsset upgradeCsv;

    private readonly List<UpgradeData> allUpgrades = new List<UpgradeData>();

    private void Awake()
    {
        Instance = this;
        ParseCsv();
    }

    private void ParseCsv()
    {
        allUpgrades.Clear();

        if (upgradeCsv == null)
        {
            Debug.LogWarning("UpgradeDatabase: no CSV assigned in the Inspector.");
            return;
        }

        string[] lines = upgradeCsv.text.Split('\n');

        // Row 0 is the header (UpgradeID,Stat,Amount) - skip it
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 3)
            {
                Debug.LogWarning($"UpgradeDatabase: skipping malformed row {i}: '{line}'");
                continue;
            }

            if (!System.Enum.TryParse(cols[1].Trim(), out StatType stat))
            {
                Debug.LogWarning($"UpgradeDatabase: unrecognized stat '{cols[1]}' on row {i}, skipping.");
                continue;
            }

            if (!int.TryParse(cols[2].Trim(), out int amount))
            {
                Debug.LogWarning($"UpgradeDatabase: invalid amount '{cols[2]}' on row {i}, skipping.");
                continue;
            }

            allUpgrades.Add(new UpgradeData
            {
                id = cols[0].Trim(),
                stat = stat,
                amount = amount
            });
        }
    }

    // Returns up to `count` distinct random upgrades from the full pool (no duplicates within one roll)
    public List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> pool = new List<UpgradeData>(allUpgrades);
        List<UpgradeData> picks = new List<UpgradeData>();

        count = Mathf.Min(count, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            picks.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return picks;
    }
}