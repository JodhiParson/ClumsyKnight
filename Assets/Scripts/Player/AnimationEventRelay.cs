using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void OnAttackAnimationEnd()
    {
        playerController.OnAttackAnimationEnd();
    }
}