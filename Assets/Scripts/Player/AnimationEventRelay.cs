using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    public WeaponHitbox weaponHitbox;
    public AudioClip swingSound;

    public void EnableHitbox() => weaponHitbox.EnableHitbox();
    public void DisableHitbox() => weaponHitbox.DisableHitbox();
    public void PlaySwingSound() => AudioSource.PlayClipAtPoint(swingSound, transform.position);

    public void OnAttack1AnimationEnd()
    {
        playerController.OnAttack1AnimationEnd();
    }

    public void OnAttack2AnimationEnd()
    {
        playerController.OnAttack2AnimationEnd();
    }
}