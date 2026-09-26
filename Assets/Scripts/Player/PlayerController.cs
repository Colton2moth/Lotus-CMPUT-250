using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

// Controls player movement, jump physics, coyote time, jump buffering, etc.
// Also passes motion and directional data to CharacterAnimator.cs
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private CharacterAnimator characterAnimator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] OrbitCamera orbitCamera;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float gravity = -50f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float terminalVelocity = -24f;

    [Header("Inertia & Acceleration")]
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float groundDeceleration = 40f;
    [SerializeField] private float airAcceleration = 25f;
    [SerializeField] private float airDeceleration = 6f;

    [Header("Chain Jumping")]
    [SerializeField] private float chainWindow = 0.12f;
    [SerializeField] private float chainSpeedBoost = 1f; // per tier
    [SerializeField] private float chainPowerBoost = 0f; // per tier
    [SerializeField] private float chainMax = 2f;

    private float timeGrounded = 0f;
    private float currentChain = 0f;
    private bool wasGrounded = false;

    [Header("Sliding")]
    [SerializeField] private float slideMaxSpeed = 9f;
    [SerializeField] private float slideAcceleration = 5f;
    [SerializeField] private float slideSteerStrength = 0.4f;

    private bool isTriggerSliding;
    private Vector3 currentSlideDirection;

    [Header("Surface Properties")]
    public float surfaceFrictionMultiplier = 1f;

    [Header("Flutter Jump")]
    [SerializeField] private float flutterJumpDuration = 0.8f;
    [SerializeField] private float flutterMaxSpeed = 2.1f;
    [SerializeField] private float flutterAirAcceleration = 4f;
    [SerializeField] private float flutterPower = 9f;
    [SerializeField] private float flutterDownwardMomentumMult = 0.8f;
    [SerializeField] private float flutterMomentumDecayMult = 0.9f;
    [SerializeField] private float flutterSteerStrength = 2.1f;

    private float flutterTimeCounter;
    private bool isFluttering;
    private bool hasFluttered;

    [Header("Movement Tuning")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float jumpBufferRayCast = 2.2f;
    [SerializeField] public float coyoteTime = 0.2f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer speedTrail;

    private float verticalVelocity;
    private Vector3 horizontalVelocity;

    private float lastGroundedTime;
    // since characterController keeps returning true when hugging a wall. Now player will only be considered grounded if their last grounded time is < 0.05 and isGrounded 
    // => means it recalculates it every time its called
    public bool IsGrounded => characterController.isGrounded && (Time.time - lastGroundedTime < 0.05f);

    public bool canMove = true;

    /*  Videos:
     *  https://www.youtube.com/watch?v=XtQMytORBmM&t=240s - Game Maker's Toolkit - Engine basics, Unity hierarchy, component architecture.
     *  https://www.youtube.com/watch?v=T2T82MWbbew&t=296s - Tvtig - CharacterController setup, Input System decoupling, LateUpdate camera follow.
     *  https://www.youtube.com/watch?v=NsSk58un8E0&t - Beans - video for Advanced Movement Shooter Physics. Mostly used for velocity handling and sliding.
     *  https://www.youtube.com/watch?v=z3dequX5g_E - Semikoder - CharacterController grounding and motion pipeline.
     *  https://www.youtube.com/watch?v=SsckrYYxcuM - Dave / GameDevelopment - Slope sliding vectors and normal projections.
     *  https://www.youtube.com/watch?v=K1xZ-rycYY8&t=3s - Bendux - New Input System callbacks, variable jump height.
     *  https://www.youtube.com/watch?v=fJyi7l2tWKo - LlamAcademy - Raycasts, layermasks etc. 
     *  https://www.youtube.com/watch?v=MOYiVLEnhrw  - Freya Holmér - Math for Game Devs P1 (Her entire video catalogue is especially useful)
     *  https://www.youtube.com/watch?v=XiwEyopOMqg - Freya Holmér - Math for Game Devs P2
     *  https://www.youtube.com/watch?v=1NLekEd770w&t - Freya Holmér -  Math for Game Devs P3
     *  
     *  Forum:
     *  https://discussions.unity.com/t/isgrounded-returns-true-when-colliding-with-wall/931376 - Forum for IsGrounded returns true when colliding with wall
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
        // chain jumping
        UpdateChainTimer();
        //Debug.DrawRay(transform.position, Vector3.down * jumpBufferRayCast, Color.red);

        // Checks ground and resets coyote timer otherwise count down the timer
        if (IsGrounded && verticalVelocity <= 0f)
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
            ExecuteJump(jumpForce);
        }
    }



    // Called by SlideTrigger when entering or exiting a slide volume
    public void SetSliding(bool sliding, Vector3 direction)
    {
        isTriggerSliding = sliding;
        currentSlideDirection = direction;

        if (sliding && isFluttering)
        {
            isFluttering = false;
        }

        if (!sliding)
        {
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeed);
            if (verticalVelocity < -5f)
            {
                verticalVelocity = -2f;
            }
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

        if (IsGrounded)
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

        if (isTriggerSliding)
        {
            float downFactor = currentSlideDirection.y < -0.05f ? currentSlideDirection.y : -0.7f;
            verticalVelocity = downFactor * Mathf.Max(horizontalVelocity.magnitude, slideMaxSpeed);
        }
        else if (IsGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -6f;
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
                    AudioController.Instance.StopFlutterLoop();
                }
                else
                {
                    // Divides total time by total duration which gives a ratio between 1.0 to 0.0
                    float progress = flutterTimeCounter / flutterJumpDuration;

                    // at 1.0 (the start) the power of the flutterjump is max at 0.5 it is half and at 0.0 it is 0 and the -2f takes over to slowly lower the player
                    float targetArcSpeed = Mathf.Lerp(-2f, flutterPower, progress);

                    // Pull current velocity toward the arc target instead of overwriting it this allows
                    // allowing downward momentum to resist the lift
                    verticalVelocity = Mathf.MoveTowards(verticalVelocity, targetArcSpeed, 45f * Time.deltaTime);
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
            }

        }

        if (isTriggerSliding)
        {
            // sliding, gets sliding direction and the player direction and does vector addition. Accelerates sliding speed at the rate of slideAcceleration
            Vector3 SlideDir = new Vector3(currentSlideDirection.x, 0f, currentSlideDirection.z).normalized;
            Vector3 steerVector = moveDirection * (maxSpeed * slideSteerStrength);

            Vector3 targetSlideVelocity = (SlideDir * slideMaxSpeed) + steerVector;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetSlideVelocity, slideAcceleration * Time.deltaTime);
        }
        else if (isFluttering)
        {
            // gets magnitude (speed) of vector
            float currentHorizontalSpeed = horizontalVelocity.magnitude;

            if (currentHorizontalSpeed > flutterMaxSpeed)
            {
                // linearly move the excess speed to the flutter max speed at the rate of flutterMomentumDecayMult * airDeceleration
                float softCapSpeed = Mathf.MoveTowards(currentHorizontalSpeed, flutterMaxSpeed, airDeceleration * flutterMomentumDecayMult * Time.deltaTime);

                if (input.sqrMagnitude > 0.01f)
                {
                    // If player is turning/moving while fluttering then linearly rotate their movement direction
                    Vector3 SteerDirection = Vector3.RotateTowards(horizontalVelocity.normalized, moveDirection, flutterSteerStrength * Time.deltaTime, 0f);
                    horizontalVelocity = SteerDirection * softCapSpeed;
                }
                else
                {
                    // otherwise normal
                    horizontalVelocity = horizontalVelocity.normalized * softCapSpeed;
                }
            }
            else
            {
                // applies flutterAirAcceleration when fluttering and moves at flutterMaxSpeed
                Vector3 targetVelocity = moveDirection * flutterMaxSpeed;
                float rate = (input.sqrMagnitude > 0.01) ? flutterAirAcceleration : airDeceleration;
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);
            }
        }
        else
        {
            float absoluteMaxSpeed = maxSpeed + (currentChain * chainSpeedBoost);
            Vector3 targetVelocity = moveDirection * absoluteMaxSpeed;

            float rate;
            if (IsGrounded)
            {
                // if Player is moving then player speed increases by effectiveAcceleration otherwise it decreases by effectiveDeceleration, surfaceFrictionMultiplier changes those (materials)
                float effectiveAcceleration = groundAcceleration * surfaceFrictionMultiplier;
                float effectiveDeceleration = groundDeceleration * surfaceFrictionMultiplier;
                rate = (input.sqrMagnitude > 0.01f) ? effectiveAcceleration : effectiveDeceleration;
            }
            else
            {
                rate = (input.sqrMagnitude > 0.01f) ? airAcceleration : airDeceleration;
            }

            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);
        }

        // feed data into the CharacterAnimator script
        if (characterAnimator != null)
        {
            // Update facing direction when input is active, 0.01 so player faces last moved direction
            if (input.sqrMagnitude > 0.01f)
            {

                characterAnimator.UpdateFacing(input);
            }

            // Switch between walking and idle while on the ground
            if (IsGrounded)
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

        // So if player is close to ground then we buffer jump instead of flutterjump
        bool nearGround = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, jumpBufferRayCast);

        // if player isn't on the ground and hasn't yet fluttered this jump, execute the flutter
        if (!IsGrounded && !hasFluttered && coyoteTimeCounter <= 0f && !nearGround)
        {
            ExecuteFlutter();
            return;
        }

        jumpBufferCounter = jumpBufferTime;
    }

    // does the jump and triggers jump animation
    public void ExecuteJump(float setJumpForce)
    {
        float finalJumpForce = setJumpForce;

        // only jumping can add to chain
        if (setJumpForce == jumpForce)
        {
            ApplyChainBoost();

            // jump boost for chaining
            finalJumpForce += (currentChain * chainPowerBoost);
        }
        else
        {
            currentChain = 0f;
        }

        float comboPitch = 1f + (currentChain * 0.15f);
        AudioController.Instance.PlayJump(comboPitch);

        verticalVelocity = finalJumpForce;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;

        characterAnimator.TriggerJump();
    }

    // Initiates flutter jump physics, timers, and animations
    public void ExecuteFlutter()
    {
        isFluttering = true;
        hasFluttered = true;
        currentChain = 0f;
        flutterTimeCounter = flutterJumpDuration;

        AudioController.Instance.StartFlutterLoop();

        // Keep a percentage of downwards momentum when fluttering starts
        if (verticalVelocity < 0f)
        {
            verticalVelocity *= flutterDownwardMomentumMult;
        }
        else
        {
            verticalVelocity = 0f;
        }

        // maxes horizontal movement to the flutterMaxSpeed, commented out in favor of soft capping it instead in Move()
        // horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, flutterMaxSpeed);

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
            AudioController.Instance.StopFlutterLoop();
        }

        if (verticalVelocity > 0f)
        {
            verticalVelocity *= 0.5f;
        }
    }

    // Sets momentum that goes into the wall.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isTriggerSliding) return;

        if (hit.normal.y >= 0.7f)
        {
            lastGroundedTime = Time.time;
        }

        // Makes it so only steep walls or vertical walls or overhang walls will set perpendicular momentum to 0
        if (hit.normal.y < 0.7f && hit.normal.y > -0.7f)
        {
            // Dot product to see if theres momentum going into the wall
            if (Vector3.Dot(horizontalVelocity, hit.normal) < 0f)
            {
                // Remove momentum going into the wall
                horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, hit.normal);
            }

            // If falling against a steep wall, push outward slightly to break capsule friction lock
            if (verticalVelocity < 0f)
            {
                horizontalVelocity += hit.normal * (0.2f * Time.deltaTime * 60f);
            }
        }

        if (hit.collider.TryGetComponent<OingyBoingy>(out OingyBoingy boingy))
        {
            ExecuteJump(boingy.boinginess);
        }
    }

    // keeps track of whether the player successfully chains a jump or can chain a jump
    private void UpdateChainTimer()
    {
        if (IsGrounded)
        {
            // reset timer to 0f if player just touched the ground
            if (!wasGrounded)
            {
                timeGrounded = 0f;
            }
            timeGrounded += Time.deltaTime;

            // if time on ground is greater than chain window then reset stacks
            if (timeGrounded > chainWindow)
            {
                currentChain = 0f;
            }
        }
        wasGrounded = IsGrounded;

        // trail only shows up when chain >= 1
        if (speedTrail != null)
        {
            speedTrail.emitting = currentChain > 0f;
        }
    }

    // applies the speed and jumppower boosts of current chain amount.
    private void ApplyChainBoost()
    {
        if (horizontalVelocity.magnitude > (maxSpeed / 2))
        {
            if (timeGrounded <= chainWindow && currentChain < chainMax)
            {
                currentChain++;
            }

            Vector3 moveDirection = horizontalVelocity.normalized;
            horizontalVelocity += moveDirection * (currentChain * chainSpeedBoost);

            float absoluteMaxSpeed = maxSpeed + (chainMax * chainSpeedBoost);
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, absoluteMaxSpeed);
        }
    }
    public void ApplyExternalMovement(Vector3 movement)
    {
        characterController.Move(movement);
    }

    // resets player
    public void teleport(Vector3 position)
    {
        verticalVelocity = 0;
        horizontalVelocity = Vector3.zero;

        surfaceFrictionMultiplier = 1f;
        isTriggerSliding = false;
        isFluttering = false;
        hasFluttered = false;
        orbitCamera.ClearDialogueTarget();


        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;

    }
}