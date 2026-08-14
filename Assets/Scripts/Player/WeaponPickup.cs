using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private float pickupActivationDelay = 0.3f;
    [SerializeField] private GameObject weaponRootToDeactivate; // usually the parent weapon entity

    private bool canBePickedUp = false;

    private void OnEnable()
    {
        canBePickedUp = false;
        Invoke(nameof(ActivatePickup), pickupActivationDelay);
    }

    private void ActivatePickup()
    {
        canBePickedUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canBePickedUp) return;

        if (other.TryGetComponent(out PlayerController player))
        {
            player.EquipWeapon();

            if (weaponRootToDeactivate != null)
            {
                weaponRootToDeactivate.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}