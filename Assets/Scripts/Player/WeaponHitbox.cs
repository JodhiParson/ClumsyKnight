using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    public int damageAmount = 25;

    // Prevents hitting the same enemy multiple times in one swing
    private HashSet<Collider> hitEnemiesThisSwing = new HashSet<Collider>();
    private Collider hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false; // off by default, turned on during swing
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger hit: " + other.gameObject.name + " tag: " + other.tag);

        if (other.CompareTag("Enemy") && !hitEnemiesThisSwing.Contains(other))
        {
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            Debug.Log("EnemyHealth found: " + (enemyHealth != null));
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                hitEnemiesThisSwing.Add(other);
            }
        }
    }

    // Call this when the swing starts (e.g. from AnimationEventRelay)
    public void EnableHitbox()
    {
        hitEnemiesThisSwing.Clear();
        hitboxCollider.enabled = true;
    }

    // Call this when the swing ends (e.g. from AnimationEventRelay)
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }
}