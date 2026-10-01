using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    //[SerializeField] private bool pauseWater = true;
    [SerializeField] private Transform customSpawnPoint;
    private void OnTriggerEnter(Collider other)
    {
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

            }
        }
    }
}
