using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;

/**
 * Handles basic kinematic player movement, jumping physics, and input smoothing.
 * 
 * Includes coyote time and jump buffering so jumping feels responsive rather than strict.
 * Movement direction is calculated relative to where the camera is facing on the XZ plane.
 * Also handles air vs ground acceleration and sends facing/walking data to CharacterAnimator.
 */
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private CharacterAnimator characterAnimator;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 7f; // Max Speed Cap now?
    [SerializeField] private float gravity = -50f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Inertia & Acceleration")]
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float groundDeceleration = 40f;
    [SerializeField] private float airAcceleration = 25f;
    [SerializeField] private float airDeceleration = 6f;

    [Header("Movement Tuning")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] public float coyoteTime = 0.1f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float verticalVelocity;
    private Vector3 horizontalVelocity;

    /**
     * Grabs the CharacterController and finds fallback references for camera and animator if left unassigned.
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

    /**
     * Ticks down jump buffer and coyote time timers.
     * If the player recently pressed jump and recently touched the ground, it runs ExecuteJump.
     */
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

    /**
     * Main movement step called by the input handler.
     * - Flattens camera angle to get forward/right directions along the ground.
     * - Accelerates or decelerates horizontal velocity toward target velocity.
     * - Handles gravity and pushes player down if their head hits a ceiling.
     * - Sends movement and facing direction to the animator.
     * @param input 2D input from WASD or left stick.
     */
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
            //if (UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed && verticalVelocity < 0f) 
            //{
            //        verticalVelocity = 4f;
            //}
            //else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        Vector3 targetVelocity = moveDirection * movementSpeed;

        float rate;
        if (characterController.isGrounded)
        {
            // if Player is moving then player speed increases by groundAcceleration otherwise it decreases by groundDeceleration
            rate = (input.sqrMagnitude > 0.01f) ? groundAcceleration : groundDeceleration;
        }
        else
        {
            rate = (input.sqrMagnitude > 0.01f) ? airAcceleration : airDeceleration;
        }

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);

        // feed data into the CharacterAnimator script
        if (characterAnimator != null)
        {
            // Update facing direction when input is active, 0.01 so player faces last moved direction
            if (input.sqrMagnitude > 0.01f)
            {
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

    /**
     * Caches a jump request into the buffer timer. Called as soon as the jump button is hit.
     */
    public void Jump()
    {
        jumpBufferCounter = jumpBufferTime;
    }

    /**
     * Applies vertical jump impulse and clears the timers so you can't double-jump.
     */
    public void ExecuteJump()
    {
        verticalVelocity = jumpForce;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;

        characterAnimator.TriggerJump();
    }

    /**
     * Cuts upward velocity in half if jump key is released early (variable jump height).
     */
    public void JumpCancelled()
    {
        if (verticalVelocity > 0f)
        {
            verticalVelocity *= 0.5f;
        }
    }

    /**
     * Removes horizontal speed moving straight into steep walls/obstacles so the player doesn't stick to them.
     */
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