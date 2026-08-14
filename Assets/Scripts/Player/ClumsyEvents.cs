using System.Collections;
using UnityEngine;

public class ClumsyEvents : MonoBehaviour
{
    public PlayerController playerController;
    [Range(0f, 1f)] public float tripChance = 0.1f;
    public float checkInterval = 2f;

    [Range(0f, 1f)] public float weaponThrowChance = 0.1f;

    private void OnEnable()
    {
        StartCoroutine(TripCheckLoop());
    }

    private IEnumerator TripCheckLoop()
    {
        var wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;

            if (playerController.IsMoving && Random.value < tripChance)
            {
                playerController.TryTrip();
            }
        }
    }

    // Hook this to an Animation Event on non-attack clips (e.g. Walk footsteps)
    // if you still want a chance to trigger the voluntary-style weapon throw.
    public void OnAnimationEventClumsyCheck()
    {
        if (Random.value < weaponThrowChance)
        {
            playerController.TryThrowWeapon();
        }
    }

    // Hook this to an Animation Event mid-swing on Attack1/Attack2.
    // On success, the attack is cancelled and the weapon is thrown instead.
    public void OnAttackAnimationEventClumsyCheck()
    {
        if (Random.value < weaponThrowChance)
        {
            playerController.CancelAttackAndThrowWeapon();
        }
    }
}