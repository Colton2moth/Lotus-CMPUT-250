using UnityEngine;

public class SimpleMountainCamera : MonoBehaviour
{
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private Transform mountainCenter;

    [Header("Offsets")]
    [SerializeField] private float orbitRadius = 15f;
    [SerializeField] private float height = 11f;

    [Header("Camera Settings")]
    [SerializeField] private float angularDeadzone = 20f;
    [SerializeField] private float verticalDeadzone = 2.2f;
    [SerializeField] private float verticalSmoothSpeed = 4f;

    private float lockedAngleX;
    private float lockedBaseHeight;
    private float currentY;

    /* Sources:
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Vector3.SignedAngle.html
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Mathf.DeltaAngle.html
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Quaternion.Euler.html
     * Also see Freya Holmér videos in playercontroller.cs
     */
    private void Start()
    {
        if (lookAtTransform == null || mountainCenter == null) return;

        // Initialize the baseline height so the camera doesn't swoop up at the start
        lockedBaseHeight = lookAtTransform.position.y;
        currentY = lockedBaseHeight + height;

        // Find the starting angle between the mountain center and the player
        Vector3 offset = lookAtTransform.position - mountainCenter.position;
        offset.y = 0;

        // SignedAngle compares the forward angle from mountain (or 'north') and player offset. the vector3.up tells its that we are measuring flat against the ground.
        lockedAngleX = Vector3.SignedAngle(Vector3.forward, offset, Vector3.up);
    }

    void LateUpdate()
    {
        if (lookAtTransform == null || mountainCenter == null) return;

        // this is the horizontal/angular deadzone section
        // calculates the players current angle around the mountain
        Vector3 offset = lookAtTransform.position - mountainCenter.position;
        offset.y = 0;
        float playerAngle = Vector3.SignedAngle(Vector3.forward, offset, Vector3.up);

        // if player moved outside the angular deadzone then pan the camera.
        // DeltaAngle calculates shortest path between two angles which handles the wrap around motion for the camera
        float angleDifference = Mathf.DeltaAngle(lockedAngleX, playerAngle);
        if (Mathf.Abs(angleDifference) > angularDeadzone) 
        {
            float excess = Mathf.Sign(angleDifference) * (Mathf.Abs(angleDifference) - angularDeadzone);
            lockedAngleX += excess;
        }

        // vertical deadzone section
        // if player is higher than current vertical height limit then pan the camera up
        float yChange = lookAtTransform.position.y - lockedBaseHeight;
        if (Mathf.Abs(yChange) > verticalDeadzone)
        {
            float excess = Mathf.Sign(yChange) * (Mathf.Abs(yChange) - verticalDeadzone);
            lockedBaseHeight += excess;
        }

        // smooth the cameras vertical position to match the new base height
        float targetY = lockedBaseHeight + height;
        currentY = Mathf.Lerp(currentY, targetY, verticalSmoothSpeed * Time.deltaTime);

        // converts locked angle back into a 3D vector (from center)
        Quaternion angleRotation = Quaternion.Euler(0f, lockedAngleX, 0f);
        Vector3 lockedOutwardDir = angleRotation * Vector3.forward;

        // position camera along the vector
        Vector3 targetPos = lookAtTransform.position + (lockedOutwardDir * orbitRadius);
        targetPos.y = currentY;
        transform.position = targetPos;

        // aim camera at player
        Vector3 lookTarget = lookAtTransform.position;
        lookTarget.y = currentY - height + 1.2f;

        transform.LookAt(lookTarget);
    }
}