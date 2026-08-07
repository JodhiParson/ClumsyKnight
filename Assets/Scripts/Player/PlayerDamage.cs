using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public Health health;
    public BoxCollider playerCollider;

    public void Update()
    {
        if (playerCollider.enabled && health.currentHealth <= 0)
        {
            playerCollider.enabled = false;
        }
    }

    public void TakeDamage(int damage)
    {
        health.currentHealth -= damage;
        health.healthBar.value = health.currentHealth;

        if (health.currentHealth <= 0)
        {
            health.Die();
        }
    }
}