using System.Collections.Generic;
using System.Text;
using UnityEngine;

// CSV columns: ArmorID,Name,Slot,Description,PositiveStat,PositiveAmount,NegativeStat,NegativeAmount
// Wrap Description in "quotes" if it needs to contain a comma.
public class ArmorDatabase : MonoBehaviour
{
    public static ArmorDatabase Instance { get; private set; }

    public TextAsset armorCsv;

    [System.Serializable]
    public class IconEntry
    {
        public string armorID;
        public Sprite icon;
    }

    [Tooltip("Map each ArmorID to its icon sprite - CSVs can't hold image references, so this is done by hand")]
    public List<IconEntry> icons = new List<IconEntry>();

    public List<ArmorData> AllArmor { get; private set; } = new List<ArmorData>();

    private void Awake()
    {
        Instance = this;
        ParseCsv();
    }

    private void ParseCsv()
    {
        AllArmor.Clear();

        if (armorCsv == null)
        {
            Debug.LogWarning("ArmorDatabase: no CSV assigned in the Inspector.");
            return;
        }

        Dictionary<string, Sprite> iconLookup = new Dictionary<string, Sprite>();
        foreach (var entry in icons)
        {
            if (!string.IsNullOrEmpty(entry.armorID))
                iconLookup[entry.armorID] = entry.icon;
        }

        string[] lines = armorCsv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // row 0 is the header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = SplitCsvLine(line);
            if (cols.Length < 8)
            {
                Debug.LogWarning($"ArmorDatabase: skipping malformed row {i}: '{line}'");
                continue;
            }

            if (!System.Enum.TryParse(cols[2].Trim(), out ArmorSlotType slot))
            {
                Debug.LogWarning($"ArmorDatabase: unrecognized slot '{cols[2]}' on row {i}, skipping.");
                continue;
            }
            if (!System.Enum.TryParse(cols[4].Trim(), out StatType posStat))
            {
                Debug.LogWarning($"ArmorDatabase: unrecognized positive stat '{cols[4]}' on row {i}, skipping.");
                continue;
            }
            if (!int.TryParse(cols[5].Trim(), out int posAmount))
            {
                Debug.LogWarning($"ArmorDatabase: invalid positive amount on row {i}, skipping.");
                continue;
            }
            if (!System.Enum.TryParse(cols[6].Trim(), out StatType negStat))
            {
                Debug.LogWarning($"ArmorDatabase: unrecognized negative stat '{cols[6]}' on row {i}, skipping.");
                continue;
            }
            if (!int.TryParse(cols[7].Trim(), out int negAmount))
            {
                Debug.LogWarning($"ArmorDatabase: invalid negative amount on row {i}, skipping.");
                continue;
            }

            string id = cols[0].Trim();

            AllArmor.Add(new ArmorData
            {
                id = id,
                armorName = cols[1].Trim(),
                slot = slot,
                description = cols[3].Trim(),
                positiveStat = posStat,
                positiveAmount = posAmount,
                negativeStat = negStat,
                negativeAmount = negAmount,
                icon = iconLookup.TryGetValue(id, out Sprite sprite) ? sprite : null
            });
        }
    }

    // Minimal CSV splitter that respects "quoted, fields" so Description can safely contain commas
    private string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
