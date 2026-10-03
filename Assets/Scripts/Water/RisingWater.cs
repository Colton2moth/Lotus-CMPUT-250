using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingWater : MonoBehaviour
{
    //TODO: make use of allowedToMove by waiting for player to jump/move/finish talking to Lotus 
    public bool allowedToMove = true;

    // distance sampling is relative to this transform
    [SerializeField] Transform playerTarget;

    [Header("Water Respawn")]
    public float respawnLevel = -2.5f;

    //curve to sample for how fast to go
    [Header("Water Rising Speed")]
    [SerializeField] AnimationCurve waterRubberBandCurve;
    [SerializeField] float curveMaxSpeed = 3f;
    [SerializeField] float curveMinSpeed = 0.7f;
    [SerializeField] float maxDistance = 25;
    [SerializeField] float minDistance = 3;

    private bool isRisingToCheckpoint = false;
    private float targetRiseLevel;
    private float currentRiseSpeed;

    // Update is called once per frame
    void Update()
    {
        if (isRisingToCheckpoint)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetRiseLevel, currentRiseSpeed * Time.deltaTime);
            transform.position = pos;

            // because Mathf.Lerp keeps halving the distance, if its small enough just snap it.
            if (Mathf.Abs(transform.position.y - targetRiseLevel) <= 0.05f)
            {
                pos.y = targetRiseLevel;
                transform.position = pos;

                isRisingToCheckpoint = false;
                allowedToMove = false; // for the start water rising trigger at the start of the actual stage
            }
            return; 

        }
        if (!allowedToMove) return;
        transform.position += Vector3.up* getCurrentRubberBandSpeed() * Time.deltaTime;
    }

    float getCurrentRubberBandSpeed()
    {
        float distance = playerTarget.position.y - transform.position.y;
        distance = Mathf.Clamp(distance,minDistance, maxDistance);
        float curveSamplePercent = (distance - minDistance) / (maxDistance - minDistance);

        // so it scales from min speed to max speed
        float curveWeight = waterRubberBandCurve.Evaluate(curveSamplePercent);
        return Mathf.Lerp(curveMinSpeed, curveMaxSpeed, curveWeight);
    }

    public void RiseToCheckpoint(float targetY, float riseSpeed)
    {
        respawnLevel = targetY;
        targetRiseLevel = targetY;
        currentRiseSpeed = riseSpeed;

        allowedToMove = false; 
        isRisingToCheckpoint = true; 
    }

    //We can decide later whether water is responsible for knowing about checkpoints or not, basic implemenation here
    public void ResetMe(float yPosition)
    {
        isRisingToCheckpoint = false;

        // water is reset at the level of the checkpoint
        Vector3 pos = transform.position;
        pos.y = respawnLevel;
        transform.position = pos;
    }

    public void StartMe()
    {
        allowedToMove = true;
    }

    // public void StartMe()
    // {
    //     allowedToMove = true;
    // }
}
