using UnityEngine;

public class SimpleMountainCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform mountainCenter;

    [Header("Offsets")]
    [SerializeField] private float distanceFromPlayer = 15f;
    [SerializeField] private float height = 5f;
    [SerializeField] private float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null || mountainCenter == null) return;

        // 1. Get direction from mountain center pointing out toward player (ignore height)
        Vector3 outwardDir = player.position - mountainCenter.position;
        outwardDir.y = 0f;
        outwardDir.Normalize();

        // 2. Put camera outside the player, along that line, raised up
        Vector3 targetPos = player.position + (outwardDir * distanceFromPlayer) + (Vector3.up * height);

        // 3. Smoothly slide to target position
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // 4. Look at player's body
        transform.LookAt(player.position + Vector3.up * 1.2f);
    }
}