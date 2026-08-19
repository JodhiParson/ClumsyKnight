using TMPro;
using UnityEngine;

public class ArmorTooltipUI : MonoBehaviour
{
    public static ArmorTooltipUI Instance { get; private set; }

    public GameObject tooltipPanel;
    public TMP_Text tooltipText;
    public RectTransform tooltipRect;
    public Vector2 cursorOffset = new Vector2(15f, -15f);

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text, Vector2 screenPosition)
    {
        tooltipPanel.SetActive(true);
        tooltipText.text = text;
        tooltipRect.position = screenPosition + cursorOffset;
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
            tooltipRect.position = (Vector2)Input.mousePosition + cursorOffset;
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
