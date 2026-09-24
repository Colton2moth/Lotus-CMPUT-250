using UnityEngine;

public class DummyInteract : MonoBehaviour
{
    [SerializeField] private OrbitCamera orbitCamera;
    [SerializeField] private RisingWater risingWater;

    private bool isTalking = false;
    private bool playerInRange = false;

    private void Start()
    {
        orbitCamera = FindAnyObjectByType<OrbitCamera>();
        risingWater = FindAnyObjectByType<RisingWater>();
        
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) /*add a must be grounded to chat Noah*/)
        {
            ToggleDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Close dialogue if the player leaves the zone while talking
            if (isTalking)
            {
                ToggleDialogue();
            }
        }
    }

    private void ToggleDialogue()
    {
        isTalking = !isTalking;

        if (isTalking)
        {
            orbitCamera.SetDialogueTarget(transform);
            risingWater.allowedToMove = false;
            
        }
        else
        {
            orbitCamera.ClearDialogueTarget();
            risingWater.allowedToMove = true;
            
        }
    }
}