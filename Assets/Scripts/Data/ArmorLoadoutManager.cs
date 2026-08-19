using System.Collections.Generic;
using UnityEngine;

public class ArmorLoadoutManager : MonoBehaviour
{
    public static ArmorLoadoutManager Instance { get; private set; }

    [Header("Player References")]
    public WeaponHitbox weaponHitbox;
    public Health playerHealth;
    public PlayerController playerController;

    private readonly Dictionary<ArmorSlotType, ArmorData> equipped = new Dictionary<ArmorSlotType, ArmorData>();
    private readonly Dictionary<ArmorSlotType, ArmorSlotUI> slotUIs = new Dictionary<ArmorSlotType, ArmorSlotUI>();
    private readonly Dictionary<string, ArmorInventoryItemUI> inventoryItemsByID = new Dictionary<string, ArmorInventoryItemUI>();

    private void Awake()
    {
        Instance = this;
    }

    // Inventory items call this on Start so the manager can gray/ungray them by ArmorID later
    public void RegisterInventoryItem(string armorID, ArmorInventoryItemUI item)
    {
        inventoryItemsByID[armorID] = item;
    }

    public void RegisterSlot(ArmorSlotType slot, ArmorSlotUI slotUI)
    {
        slotUIs[slot] = slotUI;
    }

    public void Equip(ArmorData armor, ArmorSlotUI slotUI)
    {
        // Swap out whatever's currently in this slot first
        if (equipped.TryGetValue(armor.slot, out ArmorData current) && current != null)
        {
            RemoveStatEffects(current);
            ReturnItemToInventory(current.id);
        }

        ApplyStatEffects(armor);
        MoveItemIntoSlot(armor.id, slotUI.transform);

        equipped[armor.slot] = armor;
        slotUIs[armor.slot] = slotUI;
        slotUI.SetEquipped(armor);
    }

    public void Unequip(ArmorSlotType slot)
    {
        if (!equipped.TryGetValue(slot, out ArmorData armor) || armor == null) return;

        RemoveStatEffects(armor);
        ReturnItemToInventory(armor.id);

        equipped[slot] = null;
        if (slotUIs.TryGetValue(slot, out ArmorSlotUI slotUI))
            slotUI.SetEquipped(null);
    }

    private void ApplyStatEffects(ArmorData armor)
    {
        ApplyStat(armor.positiveStat, armor.positiveAmount);
        ApplyStat(armor.negativeStat, -armor.negativeAmount);
    }

    private void RemoveStatEffects(ArmorData armor)
    {
        ApplyStat(armor.positiveStat, -armor.positiveAmount);
        ApplyStat(armor.negativeStat, armor.negativeAmount);
    }

    // Only Attack/Health are wired up right now, matching the kill-upgrade system.
    // Add more StatType cases here once you decide what other stats armor should touch.
    private void ApplyStat(StatType stat, int amount)
    {
        switch (stat)
        {
            case StatType.Attack:
                if (weaponHitbox != null)
                    weaponHitbox.damageAmount += amount;
                break;

            case StatType.Health:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += amount;
                    playerHealth.currentHealth = Mathf.Clamp(playerHealth.currentHealth + amount, 0, playerHealth.maxHealth);
                }
                break;

            case StatType.Speed:
                if (playerController != null)
                    playerController.ModifyMoveSpeed(amount);
                break;

            default:
                Debug.LogWarning($"ArmorLoadoutManager: StatType.{stat} isn't wired up to anything yet.");
                break;
        }
    }

    private void MoveItemIntoSlot(string armorID, Transform slotTransform)
    {
        if (inventoryItemsByID.TryGetValue(armorID, out ArmorInventoryItemUI item))
            item.MoveIntoSlot(slotTransform);
    }

    private void ReturnItemToInventory(string armorID)
    {
        if (inventoryItemsByID.TryGetValue(armorID, out ArmorInventoryItemUI item))
            item.ReturnToInventory();
    }
}