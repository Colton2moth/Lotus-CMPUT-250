using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    //[SerializeField] private bool pauseWater = true;
    [Header("Respawn Settings")]
    [SerializeField] private Transform customSpawnPoint;

    [Header("Water Settings")]
    [SerializeField] private RisingWater water;
    [SerializeField] private float waterOffsetBelowCheckpoint = 2.5f;
    [SerializeField] private float waterRiseSpeed = 3f; // For moving the water to the checkpoint

    private bool hasBeenActivated = false;
    private void OnTriggerEnter(Collider other)
    {
        // Thank you for finding this bug Cass.
        if (hasBeenActivated) return;

        // Ensure your player has the "Player" tag in the Inspector
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {

                // Update the player's respawn position to this checkpoint's exact location
                if (customSpawnPoint != null)
                {
                    player.currentCheckpointPosition = customSpawnPoint.position;
                }
                else
                {
                    player.currentCheckpointPosition = transform.position;
                }

                if (water != null)
                {
                    // Calculate where the water should be at below the checkpoint
                    float targetWaterLevel = player.currentCheckpointPosition.y - waterOffsetBelowCheckpoint;
                    water.RiseToCheckpoint(targetWaterLevel, waterRiseSpeed);
                }

                hasBeenActivated = true;
            }
        }
    }
}
