using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewConnectionManager : MonoBehaviour
{

    public Transform connectionTransform;
    public Transform anchorATransform;
    public Transform anchorBTransform;

    public bool active = false;
    public Vector3 anchorA;
    public Vector3 anchorB;

    private GameObject connectionGameObject;
    private GameObject anchorAGameObject;
    private GameObject anchorBGameObject;

    private void Start()
    {
        connectionGameObject = connectionTransform.gameObject;
        anchorAGameObject = anchorATransform.gameObject;
        anchorBGameObject = anchorBTransform.gameObject;

        connectionGameObject.SetActive(false);
        anchorAGameObject.SetActive(false);
        anchorBGameObject.SetActive(false);
    }

    private void Update()
    {
        connectionGameObject.SetActive(active);
        anchorAGameObject.SetActive(active);
        anchorBGameObject.SetActive(active);

        if (active)
        {
            var diff = anchorA - anchorB;

            transform.position = (anchorA + anchorB) / 2f + Vector3.back * 5;
            connectionTransform.eulerAngles = new Vector3(
                0,
                0,
                Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg
            );
            connectionTransform.localScale = new Vector3(
                diff.magnitude,
                connectionTransform.localScale.y,
                connectionTransform.localScale.z
            );

            anchorATransform.position = anchorA + Vector3.back * 5;
            anchorBTransform.position = anchorB + Vector3.back * 5;
        }
    }
}
