using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ArmorInventoryItemUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ArmorData armorData;
    public Image iconImage;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Transform originalParent;
    private Transform homeContainer; // the inventory grid it belongs to - captured once at spawn
    private int originalSiblingIndex;
    private bool isEquipped;
    private bool isDragging;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        homeContainer = transform.parent;

        if (iconImage != null && armorData != null)
            iconImage.sprite = armorData.icon;

        if (ArmorLoadoutManager.Instance != null && armorData != null)
            ArmorLoadoutManager.Instance.RegisterInventoryItem(armorData.id, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (armorData == null) return;
        ArmorTooltipUI.Instance?.Show(armorData.armorName, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ArmorTooltipUI.Instance?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (armorData == null) return;
        ArmorDetailsPanelUI.Instance?.ShowDetails(armorData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEquipped) return; // already worn - unequip via the slot's button instead of dragging it out

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        isDragging = true;

        canvasGroup.blocksRaycasts = false; // let raycasts pass through to whatever slot is underneath

        // Pull it out of the Grid Layout Group / Scroll View mask entirely while dragging, so the
        // layout group can't fight our manual positioning and it can't get clipped off-screen.
        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);
        transform.SetAsLastSibling(); // render above everything else while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Assumes a Screen Space - Overlay canvas, where screen position maps directly to UI position.
        // If your Canvas is Screen Space - Camera or World Space, this needs RectTransformUtility instead.
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;

        if (!isEquipped)
        {
            // Drop failed or landed somewhere invalid - return to its inventory grid spot
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = Vector2.zero; // Grid Layout Group re-asserts the real position next layout pass
        }
        // If it did get equipped, ArmorLoadoutManager already called MoveIntoSlot() from OnDrop - nothing more to do here.
    }

    // Called by ArmorLoadoutManager when this item gets equipped into a slot - physically moves
    // the item itself into the slot, centered, rather than leaving a copy behind in the inventory.
    public void MoveIntoSlot(Transform slotTransform)
    {
        isEquipped = true;
        canvasGroup.alpha = 1f; // fully visible - it's the only visual instance of this item now
        canvasGroup.blocksRaycasts = true; // stays hoverable/clickable for tooltip + details while equipped

        transform.SetParent(slotTransform, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    // Called by ArmorLoadoutManager when this item gets unequipped - sends it back to its
    // original inventory grid, where the Grid Layout Group repositions it automatically.
    public void ReturnToInventory()
    {
        isEquipped = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(homeContainer, false);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}