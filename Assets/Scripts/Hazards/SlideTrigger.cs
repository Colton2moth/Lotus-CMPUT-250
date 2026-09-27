using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlideTrigger : MonoBehaviour
{
    [Tooltip("Leave as (0,0,0) to slide in the direction this object points (blue forward arrow).")]
    [SerializeField] private Vector3 customSlideDirection = Vector3.zero;

    // Normal helper method instead of a property getter
    public Vector3 GetSlideDirection()
    {
        if (customSlideDirection != Vector3.zero)
        {
            return customSlideDirection.normalized;
        }

        // Defaults to the object's forward direction
        return transform.forward;
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetSliding(true, GetSlideDirection());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetSliding(false, Vector3.zero);
        }
    }

    // Draws an arrow in the Scene view so you can see where the slide points
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 direction = GetSlideDirection();
        Gizmos.DrawRay(transform.position, direction * 3f);
    }
}