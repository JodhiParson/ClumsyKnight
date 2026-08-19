using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmorSlotUI : MonoBehaviour, IDropHandler
{
    public ArmorSlotType slotType;
    public Button unequipButton; // optional - assign a small "X" button to unequip this slot

    private ArmorData equippedArmor;

    private void Awake()
    {
        if (unequipButton != null)
            unequipButton.onClick.AddListener(Unequip);
    }

    private void Start()
    {
        ArmorLoadoutManager.Instance?.RegisterSlot(slotType, this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        ArmorInventoryItemUI dragged = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<ArmorInventoryItemUI>()
            : null;

        if (dragged == null || dragged.armorData == null) return;

        if (dragged.armorData.slot != slotType)
        {
            Debug.Log($"ArmorSlotUI: rejected drop - {dragged.armorData.armorName} is a {dragged.armorData.slot} item, this slot is {slotType}.");
            return;
        }

        Debug.Log($"ArmorSlotUI: equipping {dragged.armorData.armorName} into {slotType}.");
        ArmorLoadoutManager.Instance.Equip(dragged.armorData, this);
    }

    // Called by ArmorLoadoutManager after equip/unequip - the item itself now lives inside this
    // slot's transform (moved there via ArmorInventoryItemUI.MoveIntoSlot), so this just tracks
    // which armor is currently here rather than swapping any icon of its own.
    public void SetEquipped(ArmorData armor)
    {
        equippedArmor = armor;
    }

    private void Unequip()
    {
        ArmorLoadoutManager.Instance?.Unequip(slotType);
    }
}