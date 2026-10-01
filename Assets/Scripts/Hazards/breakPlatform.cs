using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class breakPlatform : MonoBehaviour
{
    [SerializeField] private float breakTime = 1f;
    [SerializeField] private float resetTime = 1f;

    [SerializeField] CrackBlockJiggler vfx;

    private Collider platformCollider;

    private Coroutine breakCoroutine;

    void Start()
    {


        // Get the collider that is NOT a trigger
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
            {
                platformCollider = col;
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && breakCoroutine == null)
        {
            breakCoroutine = StartCoroutine(BreakPlatform());
        }
    }

    

    private IEnumerator BreakPlatform()
    {
        vfx.JigglePercentInfluence = 1.0f;
        yield return new WaitForSeconds(breakTime);

        vfx.Break();
        platformCollider.enabled = false;

        breakCoroutine = null;

        yield return new WaitForSeconds(resetTime);

        vfx.Reform();
        vfx.JigglePercentInfluence = 0.0f;

        platformCollider.enabled = true;

    }
}