using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 100;
    public int maxHealth = 100;
    public Slider enemyhealthBar;
    public AugmentUI augmentUI;

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
        if (AugmentUI.Instance != null)
        {
            AugmentUI.Instance.ToggleAugmentUI();
        }
        else
        {
            Debug.LogWarning("No AugmentUI instance found in scene.");
        }

        Destroy(gameObject);
    }
}