using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCameraFocus : MonoBehaviour
{

    private float xSize;
    private new Camera camera;
    [SerializeField]
    private Camera orthoCam;

    void Start()
    {
        camera = GetComponent<Camera>();
        xSize = Mathf.Abs(transform.position.z) * Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad);
        orthoCam.orthographicSize = xSize;
    }

    // Update is called once per frame
    void Update()
    {
        camera.fieldOfView = Mathf.Atan(xSize / Mathf.Abs(transform.position.z)) * 2 * Mathf.Rad2Deg;
    }
}
