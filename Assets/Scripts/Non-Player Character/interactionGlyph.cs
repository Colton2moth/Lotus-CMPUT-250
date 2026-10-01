using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactionGlyph : MonoBehaviour
{
    // Start is called before the first frame update
    public float upperBound = 0.1f;
    public float lowerBound = -0.1f;
    private bool isActive = false;
    public float bobbingSpeed = 5f;
    private int currentDirection = 1;
    private Vector3 activeScale;
    private Vector3 startingPosition;
    void Start()
    {
        activeScale = transform.localScale;
        startingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            float newOffset = transform.position.y + (bobbingSpeed * Time.deltaTime * currentDirection);
            transform.position = new Vector3(transform.position.x, newOffset, transform.position.z);

            if(transform.position.y < lowerBound + startingPosition.y){ currentDirection = 1;}

            if(transform.position.y > upperBound + startingPosition.y){ currentDirection = -1;}

            if(transform.localScale != activeScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, activeScale, bobbingSpeed * Time.deltaTime);
            }
        }
        else
        {
            if(transform.localScale != Vector3.zero)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, bobbingSpeed * Time.deltaTime);
            }
        }
    }
    public void setActive(bool newState){
         isActive = newState;
    }
}


