using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnchorDistructionWatcher : MonoBehaviour
{

    public AnchoredJoint2D joint;
    public GameObject connectedBlock;
    private new Rigidbody2D rigidbody;
    private Rigidbody2D otherRB;

    private bool bBreakBlock = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnJointBreak2D(Joint2D brokenJoint)
    {
        if (brokenJoint != joint)
        {
            return;
        }

        // we can't delete (BoxCollider)/create blocks during phyisic simulation
        bBreakBlock = true;
        otherRB = joint.connectedBody;

        joint = null;
    }

    private void Update()
    {
        if (!bBreakBlock)
        {
            return;
        }

        var blockTransform = connectedBlock.transform;
        var displToThis = (transform.position - blockTransform.position) / 2;
        displToThis.z = 0;

        var scale = blockTransform.localScale;
        scale.x /= 2;

        var rot = blockTransform.eulerAngles;

        CreateBlock(connectedBlock.name, blockTransform.position + displToThis, rot, scale, rigidbody, true);
        CreateBlock(connectedBlock.name, blockTransform.position - displToThis, rot, scale, otherRB, false);

        DestroyImmediate(connectedBlock);

        bBreakBlock = false;
    }

    private void CreateBlock(string name, Vector3 position, Vector3 rotation, Vector3 localScale, Rigidbody2D attachTo, bool left)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Broken " + name;
        var trnsf = go.transform;
        trnsf.SetParent(connectedBlock.transform.parent);
        trnsf.localScale = localScale;
        trnsf.position = position;
        trnsf.eulerAngles = rotation;
        DestroyImmediate(go.GetComponent<BoxCollider>());

        go.GetComponent<MeshRenderer>().sharedMaterial = connectedBlock.GetComponent<MeshRenderer>().sharedMaterial;

        go.AddComponent<Rigidbody2D>();
        var hinge = go.AddComponent<HingeJoint2D>();
        // hinge.autoConfigureConnectedAnchor = false;
        hinge.connectedBody = attachTo;
        hinge.anchor = (left ? Vector3.left : Vector3.right) * 0.5f;
    }
}