using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class breakPlatform : MonoBehaviour
{
    [SerializeField] private float breakTime = 1f;
    [SerializeField] private float resetTime = 1f;

    private Renderer platformRenderer;
    private Collider platformCollider;

    private Coroutine breakCoroutine;

    void Start()
    {
        platformRenderer = GetComponent<Renderer>();

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
        yield return new WaitForSeconds(breakTime);

        platformRenderer.enabled = false;
        platformCollider.enabled = false;

        breakCoroutine = null;

        yield return new WaitForSeconds(resetTime);

        platformRenderer.enabled = true;
        platformCollider.enabled = true;
    }
}