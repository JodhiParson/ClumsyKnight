using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }

    public GameObject upgradeUIPanel;

    [Header("Option Buttons (exactly 2)")]
    public Button option1Button;
    public TMP_Text option1Label;
    public Button option2Button;
    public TMP_Text option2Label;

    [Header("Player References")]
    public WeaponHitbox weaponHitbox; // Attack upgrades raise weaponHitbox.damageAmount
    public Health playerHealth;       // Health upgrades raise playerHealth.maxHealth (and heal by the same amount)

    private List<UpgradeData> currentOptions;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleUpgradeUI()
    {
        if (UpgradeDatabase.Instance == null)
        {
            Debug.LogWarning("UpgradeUI: no UpgradeDatabase found in scene, can't roll upgrades.");
            return;
        }

        currentOptions = UpgradeDatabase.Instance.GetRandomUpgrades(2);

        SetupOption(0, option1Button, option1Label);
        SetupOption(1, option2Button, option2Label);

        upgradeUIPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SetupOption(int index, Button button, TMP_Text label)
    {
        if (currentOptions == null || index >= currentOptions.Count)
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);
        UpgradeData upgrade = currentOptions[index];
        label.text = $"{upgrade.stat} +{upgrade.amount}";

        // Re-bind fresh each time so it always points at this roll's upgrade, not a stale one from before
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectUpgrade(upgrade));
    }

    private void SelectUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.stat)
        {
            case StatType.Attack:
                if (weaponHitbox != null)
                    weaponHitbox.damageAmount += upgrade.amount;
                else
                    Debug.LogWarning("UpgradeUI: no WeaponHitbox assigned, attack upgrade not applied.");
                break;

            case StatType.Health:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += upgrade.amount;
                    playerHealth.currentHealth += upgrade.amount; // heal by the same amount so it's felt immediately
                }
                else
                    Debug.LogWarning("UpgradeUI: no Health assigned, health upgrade not applied.");
                break;
        }

        // Advance this stat's progression so the next roll offers the next tier, not this one again
        if (UpgradeDatabase.Instance != null)
            UpgradeDatabase.Instance.MarkUpgradeTaken(upgrade);

        Debug.Log($"Applied upgrade {upgrade.id}: {upgrade.stat} +{upgrade.amount}");

        CloseUpgradeUI();
    }

    public void CloseUpgradeUI()
    {
        upgradeUIPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}