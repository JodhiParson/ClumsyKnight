using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownWeaponProjectile : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask groundLayers; // leave empty to land on anything
    [SerializeField] private bool freezeOnLand = true;

    private Rigidbody rb;
    private bool hasLanded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (groundLayers.value != 0 && (groundLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        Land();
    }

    private void Land()
    {
        hasLanded = true;

        if (animator != null)
        {
            animator.SetTrigger("Land");
        }

        if (freezeOnLand)
        {
            rb.linearVelocity = Vector3.zero; // use rb.velocity if you're on Unity < 6
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
}