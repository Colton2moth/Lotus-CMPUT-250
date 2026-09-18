using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerDrowning : MonoBehaviour
{
    [SerializeField] RisingWater water;

    [SerializeField] Transform topOfHead;
    [SerializeField] float timeToDrown = 2.0f;

    [SerializeField] Volume waterPostProcess;
    [SerializeField] float waterPostProcessLerpSpeed = 0.3f;

    float drownTimer;


    [SerializeField] Transform playerResetPosition;

    [SerializeField] PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (topOfHead.transform.position.y < water.transform.position.y)
        {
            drownTimer -= Time.deltaTime;
            if(drownTimer <= 0)
            {
                //TODO: checkpoints set this dynamically
                water.ResetMe(-2.5f);
                
                playerController.teleport(playerResetPosition.position);
            }

            waterPostProcess.weight = Mathf.Lerp(waterPostProcess.weight, 1.0f, waterPostProcessLerpSpeed*Time.deltaTime);
        }
        else
        {
            waterPostProcess.weight = Mathf.Lerp(waterPostProcess.weight, 0.0f, waterPostProcessLerpSpeed*Time.deltaTime);

            drownTimer = timeToDrown;
        }
    }

}
