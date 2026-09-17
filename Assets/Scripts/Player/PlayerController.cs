using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Controls player movement, jump physics, coyote time, jump buffering, etc.
// Also passes motion and directional data to CharacterAnimator.cs
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private CharacterAnimator characterAnimator;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 7f; 
    [SerializeField] private float gravity = -50f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Inertia & Acceleration")]
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float groundDeceleration = 40f;
    [SerializeField] private float airAcceleration = 25f;
    [SerializeField] private float airDeceleration = 6f;

    [Header("Surface Properties")]
    public float surfaceFrictionMultiplier = 1f;

    [Header("Flutter Jump")]
    [SerializeField] private float flutterJumpDuration = 0.8f;
    [SerializeField] private float flutterMaxSpeed = 3.8f;
    [SerializeField] private float flutterAirAcceleration = 5f;
    [SerializeField] private float flutterPower = 9f;
    [SerializeField] private float flutterDownwardMomentumMult = 0.6f;

    private float flutterTimeCounter;
    private bool isFluttering;
    private bool hasFluttered;

    [Header("Movement Tuning")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] public float coyoteTime = 0.2f;
    
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;
    private Vector3 horizontalVelocity;

    public bool canMove = true;

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
     *  - https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Vector3.MoveTowards.html - Vector3.moveTowards specific page.
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
        if (canMove && coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            ExecuteJump();
        }
    }

    // Movement calculations, collisions, animation states, etc.
    public void Move(Vector2 input)
    {
        // Prevents movement when in dialogue
        if (!canMove)
        {
            input = Vector2.zero;
        }

        if (characterController.isGrounded)
        {
            hasFluttered = false;
            isFluttering = false;
        }

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
        else // if in air
        {
            // if player is fluttering then decrement the timer and once it hits 0, stops fluttering
            if (isFluttering)
            {
                flutterTimeCounter -= Time.deltaTime;
                if (flutterTimeCounter <= 0)
                {
                    isFluttering = false;
                }
                else
                {
                    // Divides total time by total duration which gives a ratio between 1.0 to 0.0
                    float progress = flutterTimeCounter / flutterJumpDuration;

                    // at 1.0 (the start) the power of the flutterjump is max at 0.5 it is half and at 0.0 it is 0 and the -2f takes over to slowly lower the player
                    float targetArcSpeed = Mathf.Lerp(-2f, flutterPower, progress);

                    // Pull current velocity toward the arc target instead of overwriting it this allows
                    // allowing downward momentum to resist the lift
                    verticalVelocity = Mathf.MoveTowards(verticalVelocity, targetArcSpeed, 35f * Time.deltaTime);
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
                
        }

        float currentMaxSpeed = isFluttering ? flutterMaxSpeed : maxSpeed;
        Vector3 targetVelocity = moveDirection * currentMaxSpeed;

        float rate;
        if (characterController.isGrounded)
        {
            // if Player is moving then player speed increases by effectiveAcceleration otherwise it decreases by effectiveDeceleration, surfaceFrictionMultiplier changes those (materials)
            float effectiveAcceleration = groundAcceleration * surfaceFrictionMultiplier; 
            float effectiveDeceleration = groundDeceleration * surfaceFrictionMultiplier;
            rate = (input.sqrMagnitude > 0.01f) ? effectiveAcceleration : effectiveDeceleration;
        }
        else
        {
            // if Player is fluttering then use the flutter air acceleration instead of standard air acceleration
            if (isFluttering)
            {
                rate = (input.sqrMagnitude > 0.01f) ? flutterAirAcceleration : airDeceleration;
            }
            else
            {
                rate = (input.sqrMagnitude > 0.01f) ? airAcceleration : airDeceleration;
            }
            
        }

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);

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
        if (!canMove) return;

        // if player isn't on the ground and hasn't yet fluttered this jump, execute the flutter
        if (!characterController.isGrounded && !hasFluttered)
        {
            ExecuteFlutter();
            return;
        }

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

    // Initiates flutter jump physics, timers, and animations
    public void ExecuteFlutter()
    {
        isFluttering = true;
        hasFluttered = true;
        flutterTimeCounter = flutterJumpDuration;

        // Keep a percentage of downwards momentum when fluttering starts
        if (verticalVelocity < 0f)
        {
            verticalVelocity *= flutterDownwardMomentumMult;
        }
        else
        {
            verticalVelocity = 0f;
        }

        // maxes horizontal movement to the flutterMaxSpeed
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, flutterMaxSpeed);

        //if (characterAnimator != null)
        //{
        //    characterAnimator.TriggerFlutter();
        //}
    }

    // Variable jump height.
    public void JumpCancelled()
    {
        if (isFluttering)
        {
            isFluttering = false;
        }

        if (verticalVelocity > 0f)
        {
            verticalVelocity *= 0.5f;
        }
    }

    // Sets momentum that goes into the wall.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Makes it so only steep walls or vertical walls or overhang walls will set perpendicular momentum to 0
        if (hit.normal.y < 0.7f && hit.normal.y > -0.7f) 
        {
            // Dot product to see if theres momentum going into the wall
            if (Vector3.Dot(horizontalVelocity, hit.normal) < 0f) 
            {
                // Remove momentum going into the wall
                horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, hit.normal);
            }
        }

    }
}