using System.Collections.Generic;
using UnityEngine;

// Drag Upgrades_-_Sheet1.csv into your Assets folder (Unity auto-imports .csv as a TextAsset),
// then assign it to `upgradeCsv` on this component in the Inspector.
public class UpgradeDatabase : MonoBehaviour
{
    public static UpgradeDatabase Instance { get; private set; }

    [Tooltip("The Upgrades CSV, imported as a TextAsset (UpgradeID,Stat,Amount)")]
    public TextAsset upgradeCsv;

    // Upgrades grouped by stat, kept in CSV order (i.e. tier order: ATK_1, ATK_2, ATK_3...)
    private readonly Dictionary<StatType, List<UpgradeData>> upgradesByStat = new Dictionary<StatType, List<UpgradeData>>();

    // How many tiers of each stat the player has already taken
    private readonly Dictionary<StatType, int> takenCountByStat = new Dictionary<StatType, int>();

    private void Awake()
    {
        Instance = this;
        ParseCsv();
    }

    private void ParseCsv()
    {
        upgradesByStat.Clear();
        takenCountByStat.Clear();

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

            var upgrade = new UpgradeData
            {
                id = cols[0].Trim(),
                stat = stat,
                amount = amount
            };

            // Each stat's list stays in the order rows appear in the CSV, so ATK_1 comes
            // before ATK_2 etc. as long as the sheet itself is ordered that way (yours is).
            if (!upgradesByStat.ContainsKey(stat))
            {
                upgradesByStat[stat] = new List<UpgradeData>();
                takenCountByStat[stat] = 0;
            }
            upgradesByStat[stat].Add(upgrade);
        }
    }

    // Returns the next un-taken upgrade for this stat, or null if every tier has already been taken
    public UpgradeData GetNextUpgrade(StatType stat)
    {
        if (!upgradesByStat.TryGetValue(stat, out var list)) return null;
        int index = takenCountByStat[stat];
        return index < list.Count ? list[index] : null;
    }

    // Call this once the player actually picks an upgrade, so the next roll offers the
    // following tier instead of repeating or skipping ahead.
    public void MarkUpgradeTaken(UpgradeData upgrade)
    {
        if (!takenCountByStat.ContainsKey(upgrade.stat)) return;

        // Only advance if this really was the next upgrade in line, so re-applying the same
        // upgrade twice (e.g. a stray double-click) can't silently skip a tier.
        UpgradeData next = GetNextUpgrade(upgrade.stat);
        if (next != null && next.id == upgrade.id)
        {
            takenCountByStat[upgrade.stat]++;
        }
    }

    // Returns up to `count` distinct random picks, one per stat track, each being that
    // track's *next* tier (e.g. Attack 2 once Attack 1 is taken) rather than any random row.
    // A track that's already maxed out (all tiers taken) is excluded from the roll.
    public List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> available = new List<UpgradeData>();
        foreach (StatType stat in upgradesByStat.Keys)
        {
            UpgradeData next = GetNextUpgrade(stat);
            if (next != null) available.Add(next);
        }

        count = Mathf.Min(count, available.Count);
        List<UpgradeData> picks = new List<UpgradeData>();
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            picks.Add(available[index]);
            available.RemoveAt(index);
        }

        return picks;
    }
}