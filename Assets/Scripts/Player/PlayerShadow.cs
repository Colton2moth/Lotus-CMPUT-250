using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerShadow : MonoBehaviour
{
    private DecalProjector decalProjector;

    [Header("Shadow Settings")]
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private float shrinkDistance = 4f;
    [SerializeField] private float baseSize = 0.7f;
    [SerializeField] private float playerRadius = 0.35f;
    [SerializeField] private float castStartOffset = 0.5f;

    public LayerMask groundLayer;

    void Start()
    {
        decalProjector = GetComponent<DecalProjector>();
    }

    void Update()
    {
        Vector3 origin = transform.position + Vector3.up * castStartOffset;
        if (Physics.SphereCast(origin, playerRadius, Vector3.down, out RaycastHit hit, maxDistance + castStartOffset, groundLayer))
        {
            // hit.distance is the spheres travel distance
            float surfaceDist = transform.position.y - hit.point.y;
            float heightPercent = Mathf.Clamp01(surfaceDist / shrinkDistance);
            decalProjector.fadeFactor = Mathf.Lerp(1f, 0.4f, heightPercent);

            // Shrink the width and height of the decal
            float currentSize = Mathf.Lerp(baseSize, baseSize * 0.5f, heightPercent);

            // Clamp projection depth to the hit surface so it cant project past ledges
            float depth = Mathf.Max(surfaceDist, 0.05f) + 0.2f;
            decalProjector.size = new Vector3(currentSize, currentSize, depth);
            decalProjector.pivot = new Vector3(0f, 0f, depth * 0.5f);
        }
        else
        {
            decalProjector.fadeFactor = 0f;
        }
    }
}
