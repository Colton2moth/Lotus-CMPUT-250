using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    float verticalVelocity;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Add Coyote time, and Jump buffering, Responsivess, Fluidity.
    // Add head hitters (hitting your head on a jump forces you to go down instead of floating there.
    // Better player hitbox (Not capsule so your player doesnt slide on ledges when you are on the edge of your hitbox.
    // Acceleration on the XZ axis: move keys add to velocity on XZ).
    //How do I slow down? Damping. velocity * 0.95; is usually how to do damping
    //How to limit speed? watch youtube video, clamping is not always enough
    //Air movement: air should be lsipperier (less damping).
    

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (characterController.isGrounded && verticalVelocity <= 0f)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f) {
            ExecuteJump();
        }
    }

    public void Move(Vector2 input)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * input.y + camRight * input.x).normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 targetFacing;

            if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
            {
                targetFacing = input.y > 0 ? camForward : -camForward;
            }
            else
            {
                targetFacing = input.x > 0 ? camRight : -camRight;
            }

            Transform target = visualTransform != null ? visualTransform : transform;
            target.rotation = Quaternion.LookRotation(targetFacing);

            if (spriteRenderer != null)
            {
                UpdateSpriteFacing(input);
            }
        }

        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = (moveDirection * movementSpeed) + (Vector3.up * verticalVelocity);
        characterController.Move(motion * Time.deltaTime);
    }

    private void UpdateSpriteFacing(Vector2 input)
    {
        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
        {
            spriteRenderer.flipX = false;
            if (input.y > 0)
            {
                // face back
            }
            else
            {
                // face forward
            }
        }
        else
        {
            spriteRenderer.flipX = (input.x < 0);

            // flip left/right
        }
    }

    public void Jump()
    {
        jumpBufferCounter = jumpBufferTime;
    }

    public void ExecuteJump() {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
    }
}