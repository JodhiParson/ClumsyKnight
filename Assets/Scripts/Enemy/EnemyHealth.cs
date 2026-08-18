using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 100;
    public int maxHealth = 100;
    public Slider enemyhealthBar;
    public UpgradeUI upgradeUI;

    private void Start()
    {
        enemyhealthBar.maxValue = maxHealth;
        enemyhealthBar.value = currentHealth;
    }
    void Update()
    {
        enemyhealthBar.value = currentHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        enemyhealthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (UpgradeUI.Instance != null)
        {
            UpgradeUI.Instance.ToggleUpgradeUI();
        }
        else
        {
            Debug.LogWarning("No UpgradeUI instance found in scene.");
        }

        Destroy(gameObject);
    }
}