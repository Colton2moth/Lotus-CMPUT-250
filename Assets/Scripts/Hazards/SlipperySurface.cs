using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipperySurface : MonoBehaviour
{
    [SerializeField] private float iceFriction = 0.1f; // lower is more slippery

    // When player enters the hitbox (trigger) then check if its a player, if it is then apply iceFriction multiplier (referenced in playerController.cs)
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.surfaceFrictionMultiplier = iceFriction;
        }
    }

    // When player exits the hitbox then set it back to normal (Referenced in playerController.cs)
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.surfaceFrictionMultiplier = 1f;
        }
    }
}
