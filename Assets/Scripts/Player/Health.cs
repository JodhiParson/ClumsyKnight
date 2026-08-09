using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    [Header("Potions")]
    public int potionCount = 4;
    public float healPercent = 0.4f; // heals 40% of MAX health
    public KeyCode useKey = KeyCode.Alpha1;
    public TextMeshProUGUI potionText; // optional, shows potions left

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        UpdatePotionUI();
    }

    void Update()
    {
        healthText.text = currentHealth.ToString();
        healthBar.value = currentHealth;

        if (Input.GetKeyDown(useKey))
        {
            UsePotion();
        }
    }

    public void UsePotion()
    {
        if (potionCount <= 0)
        {
            Debug.Log("No potions left.");
            return;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("Already at full health.");
            return;
        }

        int healAmount = Mathf.RoundToInt(maxHealth * healPercent);
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        healthBar.value = currentHealth;

        potionCount--;
        UpdatePotionUI();

        Debug.Log($"Used potion. Healed {healAmount}. Current: {currentHealth}/{maxHealth}. Potions left: {potionCount}");
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdatePotionUI()
    {
        if (potionText != null)
        {
            potionText.text = potionCount.ToString();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        healthText.text = currentHealth.ToString();
        healthBar.value = currentHealth;

        if (Input.GetKeyDown(useKey))
        {
            UsePotion();
        }
    }

    public void UsePotion()
    {
        if (potionCount <= 0)
        {
            Debug.Log("No potions left.");
            return;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("Already at full health.");
            return;
        }

        int healAmount = Mathf.RoundToInt(maxHealth * healPercent);
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        healthBar.value = currentHealth;

        potionCount--;
        UpdatePotionUI();

        Debug.Log($"Used potion. Healed {healAmount}. Current: {currentHealth}/{maxHealth}. Potions left: {potionCount}");
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdatePotionUI()
    {
        if (potionText != null)
        {
            potionText.text = potionCount.ToString();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}