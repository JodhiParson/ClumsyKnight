using UnityEngine;

public class ArmorInventorySpawner : MonoBehaviour
{
    [Tooltip("The ArmorInventoryItemUI prefab to spawn one of per armor piece")]
    public ArmorInventoryItemUI itemPrefab;

    [Tooltip("Parent to spawn items under - typically a panel with a Grid Layout Group")]
    public Transform inventoryContainer;

    private void Start()
    {
        SpawnAll();
    }

    public void SpawnAll()
    {
        if (ArmorDatabase.Instance == null)
        {
            Debug.LogWarning("ArmorInventorySpawner: no ArmorDatabase found in scene.");
            return;
        }

        if (itemPrefab == null || inventoryContainer == null)
        {
            Debug.LogWarning("ArmorInventorySpawner: itemPrefab or inventoryContainer not assigned.");
            return;
        }

        foreach (ArmorData armor in ArmorDatabase.Instance.AllArmor)
        {
            ArmorInventoryItemUI item = Instantiate(itemPrefab, inventoryContainer);
            item.armorData = armor; // set before this item's own Start() runs, so it picks up the icon/registers correctly
            item.name = $"Item_{armor.id}";
        }
    }
}
