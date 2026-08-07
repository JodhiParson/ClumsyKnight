using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    public WeaponHitbox weaponHitbox;
    public AudioSource swingSound;

    public void EnableHitbox() => weaponHitbox.EnableHitbox();
    public void DisableHitbox() => weaponHitbox.DisableHitbox();
    public void PlaySwingSound() => swingSound.Play();

    public void OnAttack1AnimationEnd()
    {
        playerController.OnAttack1AnimationEnd();
    }

    public void OnAttack2AnimationEnd()
    {
        playerController.OnAttack2AnimationEnd();
    }
}