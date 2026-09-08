using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;

// Controls player movement, jump physics, coyote time, jump buffering, etc.
// Also passes motion and directional data to CharacterAnimator.cs
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private CharacterAnimator characterAnimator;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float gravity = -50f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Movement Tuning")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] public float coyoteTime = 0.2f;
    
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;
    private Vector3 horizontalVelocity;

    


    // Add Coyote time, and Jump buffering, Responsiveness, Fluidity.
    // Add head hitters (hitting your head on a jump forces you to go down instead of floating there.
    // Better player hitbox (Not capsule so your player doesnt slide on ledges when you are on the edge of your hitbox.
    // Acceleration on the XZ axis: move keys add to velocity on XZ).
    //How do I slow down? Damping. velocity * 0.95; is usually how to do damping
    //How to limit speed? watch youtube video, clamping is not always enough
    //Air movement: air should be lsipperier (less damping).

    /*  Videos:
     *  https://www.youtube.com/watch?v=XtQMytORBmM&t=240s - Game Maker's Toolkit - Engine basics, Unity hierarchy, component architecture.
     *  https://www.youtube.com/watch?v=T2T82MWbbew&t=296s - Tvtig - CharacterController setup, Input System decoupling, LateUpdate camera follow.
     *  https://www.youtube.com/watch?v=NsSk58un8E0&t - Beans - video for Advanced Movement Shooter Physics. Mostly used for velocity handling and sliding.
     *  https://www.youtube.com/watch?v=z3dequX5g_E - Semikoder - CharacterController grounding and motion pipeline.
     *  https://www.youtube.com/watch?v=SsckrYYxcuM - Dave / GameDevelopment - Slope sliding vectors and normal projections.
     *  https://www.youtube.com/watch?v=K1xZ-rycYY8&t=3s - Bendux - New Input System callbacks, variable jump height.
     *  https://www.youtube.com/watch?v=fJyi7l2tWKo - LlamAcademy - Raycasts, layermasks etc. 
     *  
     *  Documentation:
     *  https://docs.unity3d.com/2022.3/Documentation/Manual/index.html - Unity Documentation.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/index.html - Scripting Documentation.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/CharacterController.html - Character Controller specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Quaternion.html - Quaternion specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SpriteRenderer.html - SpriteRenderer specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/CollisionFlags.html - Collision Flags specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Mathf.html - Mathf specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Physics.html - Physics specific page.
     *  - https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Physics.Raycast.html - Physics.Raycast specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Vector2.html - Vector 2 specific page.
     *  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Vector3.html - Vector 3 specific page.
     *  
     *  prolly go back here to revamp the descriptions but heres the credits and sources for now.
     */

    void Start()
    {
        // Grab the CharacterController component attached to this GameObject
        characterController = GetComponent<CharacterController>();

        // default to main scene camera if none is assigned in the inspector
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Look for CharacterAnimator on this object or child 
        if (characterAnimator == null) 
        {
            characterAnimator = GetComponentInChildren<CharacterAnimator>();
        }
    }

    private void Update()
    {
        // Checks ground and resets coyote timer otherwise count down the timer
        if (characterController.isGrounded && verticalVelocity <= 0f)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // count down jump input buffer timer
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Do a jump if coyote time and jump buffer timer is active
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            ExecuteJump();
        }
    }

    // Movement calculations, collisions, animation states, etc.
    public void Move(Vector2 input)
    {
        // flatten camera direction to ignore pitch/tilt
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // calculate move direction relative to where the camera is facing
        Vector3 moveDirection = (camForward * input.y + camRight * input.x).normalized;

        // headhitting, basically applies -2 vertical velocity (falling down speed) if head touches anything
        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = -2f;
        }

        // if on ground and vertical velocity is less than 0 (which it is if grounded because -2) then apply the -2 sticking force
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else // normal otherwise
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        horizontalVelocity = moveDirection * movementSpeed;

        // feed data into the CharacterAnimator script
        if (characterAnimator != null) 
        {
            // Update facing direction when input is active, 0.01 so player faces last moved direction
            if (input.sqrMagnitude > 0.01f) {

                characterAnimator.UpdateFacing(input);
            }

            // Switch between walking and idle while on the ground
            if (characterController.isGrounded) 
            {
                characterAnimator.SetMoving(input.sqrMagnitude > 0.01f);
            }
        }

        // Combine horizontal motion and vertical velocity, then apply via CharacterController
        Vector3 motion = horizontalVelocity + (Vector3.up * verticalVelocity);
        characterController.Move(motion * Time.deltaTime);
    }

    // buffers jump input, called when player presses jump
    public void Jump()
    {
        jumpBufferCounter = jumpBufferTime;
    }

    // does the jump and triggers jump animation
    public void ExecuteJump()
    {
        verticalVelocity = jumpForce;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;

        characterAnimator.TriggerJump();
    }

    // Variable jump height.
    public void JumpCancelled()
    {
        if (verticalVelocity > 0f) {
            verticalVelocity *= 0.5f;
        }
    }
}