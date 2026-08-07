using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
    }
    void Update()
    {
        healthText.text = currentHealth.ToString();
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
