using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private InputAction move, jump;
    // Start is called before the first frame update
    void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        playerController.Move(inputVector);

        if (jump != null && jump.WasPressedThisFrame()) {
            playerController.Jump();
        }
    }
}
