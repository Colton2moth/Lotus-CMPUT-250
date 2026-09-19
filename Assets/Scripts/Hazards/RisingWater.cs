using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingWater : MonoBehaviour
{
    //TODO: make use of allowedToMove by waiting for player to jump/move/finish talking to Lotus 
    public bool allowedToMove = true;

    // distance sampling is relative to this transform
    [SerializeField] Transform playerTarget;

    //curve to sample for how fast to go
    [SerializeField] AnimationCurve waterRubberBandCurve;
    [SerializeField] float curveMaxSpeed = 3f;
    [SerializeField] float curveMinSpeed = 0.7f;

    ///water rises faster when close to maxDistance 
    [SerializeField] float maxDistance = 25;

    ///water rise slower when close to minDistance 
    [SerializeField] float minDistance = 3;

    // Update is called once per frame
    void Update()
    {
        if(!allowedToMove) return;
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


    //We can decide later whether water is responsible for knowing about checkpoints or not, basic implemenation here
    public void ResetMe(float yPosition)
    {
        // allowedToMove = false;
        transform.position = Vector3.up * yPosition;
    }

    // public void StartMe()
    // {
    //     allowedToMove = true;
    // }
}
