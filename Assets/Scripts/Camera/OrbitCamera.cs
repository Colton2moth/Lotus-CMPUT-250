using UnityEngine;

public class OrbitCamera : MonoBehaviour
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

    [Header("Dialogue Zoom")]
    [SerializeField] private float dialogueZoomRadius = 8f;
    [SerializeField] private float dialogueHeight = 6f;
    [SerializeField] private float zoomTransitionSpeed = 3.5f;
    private Transform activeNPC;
    private Vector3 currentFocusPoint;
    private float currentRadius;
    private float currentTargetHeight;

    private float lockedAngleX;
    private float lockedBaseHeight;
    private float currentCameraY;

    /* Videos:
     * https://www.youtube.com/watch?v=9dzBrLUIF8g - Sasquatch B Studios - How to make a camera like hollow knight, not too useful but can teach about direction bias, interpolation, camera ledge detection, etc.
     * https://www.youtube.com/watch?v=LSNQuFEDOyQ - Freya Holmér - How Lerp works under the hood and why its framerate dependent.
     * 
     * Documentation:
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Vector3.SignedAngle.html
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Mathf.DeltaAngle.html
     * https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Quaternion.Euler.html
     * Also see Freya Holmér videos in playercontroller.cs
     */

    private void Start()
    {
        if (lookAtTransform == null || mountainCenter == null) return;

        // Initialize the variables with the standard 
        lockedBaseHeight = lookAtTransform.position.y;
        currentCameraY = lockedBaseHeight + height;
        currentRadius = orbitRadius;
        currentTargetHeight = height;
        currentFocusPoint = lookAtTransform.position;

        // Find the starting angle between the mountain center and the player
        Vector3 offset = lookAtTransform.position - mountainCenter.position;
        offset.y = 0;

        // SignedAngle compares the forward angle from mountain (or 'north') and player offset. the vector3.up tells its that we are measuring flat against the ground.
        lockedAngleX = Vector3.SignedAngle(Vector3.forward, offset, Vector3.up);
    }

    void LateUpdate()
    {
        if (lookAtTransform == null || mountainCenter == null) return;

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

        // if player is higher than current vertical height limit then pan the camera up, this activeNPC == null is there so it doesnt override the dialogue camera
        if (activeNPC == null)
        {
            float yChange = lookAtTransform.position.y - lockedBaseHeight;
            if (Mathf.Abs(yChange) > verticalDeadzone)
            {
                float excess = Mathf.Sign(yChange) * (Mathf.Abs(yChange) - verticalDeadzone);
                lockedBaseHeight += excess;
            }
        }

        // converts locked angle back into a 3D vector (from center)
        Quaternion angleRotation = Quaternion.Euler(0f, lockedAngleX, 0f);
        Vector3 lockedOutwardDir = angleRotation * Vector3.forward;

        // if interacting with an NPC, get the mid point and focus on there and set camera settings to dialogue, otherwise normal.
        Vector3 targetFocusPoint;
        float targetRadius;
        float targetHeightOffset;
        if (activeNPC != null)
        {
            targetFocusPoint = (lookAtTransform.position + activeNPC.position) * 0.5f;
            targetRadius = dialogueZoomRadius;
            targetHeightOffset = dialogueHeight;
        }
        else
        {
            targetFocusPoint = lookAtTransform.position;
            targetRadius = orbitRadius;
            targetHeightOffset = height;
        }

        currentFocusPoint = Vector3.Lerp(currentFocusPoint, targetFocusPoint, zoomTransitionSpeed * Time.deltaTime);
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, zoomTransitionSpeed * Time.deltaTime);
        currentTargetHeight = Mathf.Lerp(currentTargetHeight, targetHeightOffset, zoomTransitionSpeed * Time.deltaTime);

        // smooth the cameras vertical position to match the baseline + current height offset
        float targetCameraY = lockedBaseHeight + currentTargetHeight;
        currentCameraY = Mathf.Lerp(currentCameraY, targetCameraY, verticalSmoothSpeed * Time.deltaTime);

        // position camera along the outward vector from the smoothed focus point
        Vector3 targetPos = currentFocusPoint + (lockedOutwardDir * currentRadius);
        targetPos.y = currentCameraY;
        transform.position = targetPos;

        // aim camera using the smoothed baseline height so small jumps inside deadzone don't tilt the camera
        Vector3 lookTarget = currentFocusPoint;
        lookTarget.y = currentCameraY - currentTargetHeight + 1.2f;

        transform.LookAt(lookTarget);
    }

    // sets the dialogue NPC (Noah use these)
    public void SetDialogueTarget(Transform npc)
    {
        activeNPC = npc;
        lockedBaseHeight = (lookAtTransform.position.y + activeNPC.position.y) * 0.5f;
    }

    // removes teh dialogue NPC (Noah use these)
    public void ClearDialogueTarget()
    {
        activeNPC = null;
    }
}