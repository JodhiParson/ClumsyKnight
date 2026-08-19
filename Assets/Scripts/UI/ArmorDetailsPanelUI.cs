using TMPro;
using UnityEngine;

public class ArmorDetailsPanelUI : MonoBehaviour
{
    public static ArmorDetailsPanelUI Instance { get; private set; }

    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDetails(ArmorData armor)
    {
        if (nameText != null) nameText.text = armor.armorName;
        if (descriptionText != null) descriptionText.text = armor.description;
    }
}
