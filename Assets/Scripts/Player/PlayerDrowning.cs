using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerDrowning : MonoBehaviour
{
    [SerializeField] RisingWater water;

    [SerializeField] private Transform topOfHead;
    [SerializeField] private float timeToDrown = 2.0f;

    [SerializeField] private float deadlySpeed = -30f;
    [SerializeField] private float requiredDepthForDeath = 4f;
    [SerializeField] private LayerMask groundLayerMask;

    [SerializeField] private Volume waterPostProcess;
    [SerializeField] private float waterPostProcessLerpSpeed = 0.3f;

    private float drownTimer;
    // private bool wasUnderWater;
    private bool isDying = false;


    // [SerializeField] private Transform playerResetPosition;

    [SerializeField] private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDying)
        {
            if (ScreenFadeToBlack.Instance.IsScreenBlack)
            {
                water.ResetMe(-2.5f);
                waterPostProcess.weight = 0f;
                drownTimer = timeToDrown;

                isDying = false;
            }
            return;
        }

        bool isUnderwater = topOfHead.transform.position.y < water.transform.position.y;

        if (isUnderwater)
        {
            bool isDeep = !Physics.Raycast(playerController.transform.position, Vector3.down, requiredDepthForDeath, groundLayerMask);
            if (playerController.currentVerticalVelocity <= deadlySpeed && isDeep)
            {
                Die();
                return;
            }
        }

        if (isUnderwater)
        {
            drownTimer -= Time.deltaTime;
            if (drownTimer <= 0)
            {
                Die();
            }

            waterPostProcess.weight = Mathf.Lerp(waterPostProcess.weight, 1.0f, waterPostProcessLerpSpeed * Time.deltaTime);
        }
        else
        {
            waterPostProcess.weight = Mathf.Lerp(waterPostProcess.weight, 0.0f, waterPostProcessLerpSpeed * Time.deltaTime);

            drownTimer = timeToDrown;
        }
    }

    private void Die()
    {
        if (isDying) return; // Prevent calling this multiple times while already dying
        isDying = true; 
        playerController.Respawn();
    }

}
