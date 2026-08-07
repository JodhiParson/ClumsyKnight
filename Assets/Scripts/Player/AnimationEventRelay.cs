using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void OnAttack1AnimationEnd()
    {
        playerController.OnAttack1AnimationEnd();
    }

    public void OnAttack2AnimationEnd()
    {
        playerController.OnAttack2AnimationEnd();
    }
}