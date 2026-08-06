using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthBar;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
    }
}
