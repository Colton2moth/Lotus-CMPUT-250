using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlock : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float stateTimer;
    private Vector3 lastPosition;
    private Vector3 movementDelta;

    [SerializeField] private Vector3 targetDistance = new Vector3(5f, 0f, 0f);
    [SerializeField] private float waitTimeAtStart = 1.5f;
    [SerializeField] private float waitTimeAtEnd = 0.5f;
    [SerializeField] private float moveTime = 1f;

    private PlayerController playerOnBlock;

    private enum MoveState
    {
        WaitingAtStart,
        MovingOut,
        WaitingAtEnd,
        MovingBack
    };

    private MoveState currentState = MoveState.WaitingAtStart;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + targetDistance;
        lastPosition = transform.position;
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        if (currentState == MoveState.WaitingAtStart)
        {
            if (stateTimer >= waitTimeAtStart)
            {
                currentState = MoveState.MovingOut;
                stateTimer = 0f;
            }
        }

        else if (currentState == MoveState.MovingOut)
        {
            float progress = stateTimer / moveTime;

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            if (progress >= 1f)
            {
                transform.position = targetPosition;
                currentState = MoveState.WaitingAtEnd;
                stateTimer = 0f;
            }
        }

        else if (currentState == MoveState.WaitingAtEnd)
        {
            if (stateTimer >= waitTimeAtEnd)
            {
                currentState = MoveState.MovingBack;
                stateTimer = 0f;
            }
        }

        else if (currentState == MoveState.MovingBack)
        {
            float progress = stateTimer / moveTime;

            transform.position = Vector3.Lerp(
                targetPosition,
                startPosition,
                progress
            );

            if (progress >= 1f)
            {
                transform.position = startPosition;
                currentState = MoveState.WaitingAtStart;
                stateTimer = 0f;
            }
        }

        movementDelta = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            playerOnBlock = player;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            if (player == playerOnBlock)
            {
                playerOnBlock = null;
            }
        }
    }

    private void LateUpdate()
    {
        if (playerOnBlock != null)
        {
            playerOnBlock.ApplyExternalMovement(movementDelta);
        }
    }
}