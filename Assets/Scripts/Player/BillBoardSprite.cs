using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Make the sprite match the camera's horizontal yaw so it stays upright and flat to the screen instead of being locked to one axis
        transform.rotation = Quaternion.Euler(0f, cam.eulerAngles.y, 0f);
    }
}
