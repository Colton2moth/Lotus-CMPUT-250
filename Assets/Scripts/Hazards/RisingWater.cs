using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingWater : MonoBehaviour
{

    // distance sampling is relative to this transform
    [SerializeField] Transform playerTarget;

    //curve to sample for how fast to go
    [SerializeField] AnimationCurve waterRubberBandCurve;
    [SerializeField] float curveMaxSpeed = 1.0f;

    ///water rises faster when close to maxDistance 
    [SerializeField] float maxDistance = 30;

    ///water rise slower when close to minDistance 
    [SerializeField] float minDistance = 3;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up* getCurrentRubberBandSpeed() * Time.deltaTime;
    }

    float getCurrentRubberBandSpeed()
    {
        float distance = playerTarget.position.y - transform.position.y;
        distance = Mathf.Clamp(distance,0, maxDistance);
        float curveSamplePercent = distance/maxDistance;

        return waterRubberBandCurve.Evaluate(curveSamplePercent) * curveMaxSpeed;
    }   
}
