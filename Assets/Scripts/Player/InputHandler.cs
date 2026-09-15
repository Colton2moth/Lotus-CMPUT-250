using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Reads player inputs through the Unity Input System and feeds them into PlayerController.
 * 
 * Binds the 'Move' and 'Jump' actions, locks the cursor to the game window,
 * and handles jump press/release triggers every frame.
 */
public class InputHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private InputAction move, jump;

    /**
     * Binds input actions from the active Input System asset and locks the cursor.
     */
    void Start()
    {
        // Locate and bind the Move and Jump actions from the active Input System asset
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");

        // Hides cursor and locks it when in game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /**
     * Reads movement vectors and jump button events each frame, passing values to the PlayerController.
     */
    void Update()
    {
        // Read 2D directional input, defaults to 0 if action is missing
        Vector2 inputVector = move != null ? move.ReadValue<Vector2>() : Vector2.zero;

        // send input vector to the player physics controller each frame
        playerController.Move(inputVector);

        if (jump != null)
        {

            // Initial press trigger jump input buffering
            if (jump.WasPressedThisFrame())
            {
                playerController.Jump();
            }

            // Release allows for variable jump heights (see bottom of playerController)
            else if (jump.WasReleasedThisFrame())
            {
                playerController.JumpCancelled();
            }
        }
    }
}