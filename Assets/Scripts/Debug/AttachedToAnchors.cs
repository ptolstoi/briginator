using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AttachedToAnchors : MonoBehaviour
{
    public Transform[] Anchors = new Transform[2];
    // private Vector2 lastVelocity = Vector2.zero;
    private new Transform transform;
    // private new Rigidbody2D rigidbody;

    private void Start()
    {
        // rigidbody = GetComponent<Rigidbody2D>();
        transform = base.transform;

        // lastVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        // var acceloration = rigidbody.velocity - lastVelocity;

        // Debug.DrawRay(transform.position, acceloration);
        // Debug.DrawRay(transform.position, rigidbody.angularVelocity * Vector2.right);

        // lastVelocity = rigidbody.velocity;
        // // rigidbody.velocity = Vector2.zero;
        // Debug.Log(acceloration);

        // rigidbody.AddForce(-Physics2D.gravity, ForceMode2D.Force);
        // // UpdatePositionAndScale();
    }

    private void LateUpdate()
    {
        UpdatePositionAndScale();
    }

    void UpdatePositionAndScale()
    {
        if (Anchors.Length != 2 || Anchors.Any(x => x == null))
        {
            return;
        }

        transform.position = Anchors.Average(x => x.position);

        var diff = Anchors[1].position - Anchors[0].position;
        if (diff.x < 0)
        {
            diff = -diff;
        }

        var anchorDistance = diff.magnitude;
        var slopeAngle = Vector3.SignedAngle(diff, Vector3.right, -Vector3.forward);

        transform.localEulerAngles = new Vector3(0, 0, slopeAngle);
        transform.localScale = new Vector3(
            anchorDistance,
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
