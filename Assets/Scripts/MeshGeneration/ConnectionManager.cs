using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    public AnchoredJoint2D connectionJoint;
    public Connection connection;
    public BridgeMeshManager meshGenerator;
    public GameObject connectedBlock;


    private GameObject mesh;
    private new Rigidbody2D rigidbody;
    private Rigidbody2D otherRigidbody;
    private Transform otherTransform;
    private Transform meshTransform;
    internal LevelManager levelManager;
    private MeshRenderer meshRenderer;
    private Material loadMaterial;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        otherRigidbody = connectionJoint.connectedBody;
        otherTransform = otherRigidbody.transform;

        mesh = new GameObject("Mesh " + connection.Type);
        mesh.layer = gameObject.layer;
        meshTransform = mesh.transform;
        meshTransform.SetParent(levelManager.MeshParent);

        RegenerateMesh();

        meshGenerator.NeedMeshRegeneration += RegenerateMesh;
    }

    private void OnEnable()
    {
        if (meshGenerator != null)
        {
            meshGenerator.NeedMeshRegeneration += RegenerateMesh;
        }
    }

    private void RegenerateMesh()
    {
        meshGenerator.GenerateMeshFor(
            connection: connection,
            atGameObject: mesh,
            useLoadMaterial: meshGenerator.ShowLoad
        );

        meshRenderer = mesh.GetComponent<MeshRenderer>();
        loadMaterial = new Material(meshRenderer.sharedMaterials.Last());
        loadMaterial.name += " ConnectionManager Copy";
        var sharedMats = meshRenderer.sharedMaterials;
        sharedMats[sharedMats.Length - 1] = loadMaterial;
        meshRenderer.sharedMaterials = sharedMats;

    }

    private void LateUpdate()
    {
        if (mesh != null && connectionJoint != null)
        {
            var position = transform.position;
            var otherPosition = otherTransform.position;
            var diff = otherPosition - position;
            var distance = diff.magnitude;

            meshTransform.position = (position + otherPosition) / 2;
            meshTransform.eulerAngles = new Vector3(
                0,
                0,
                Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg
            );
            meshTransform.localScale = new Vector3(distance, 1, 1);

            var load = connectionJoint.reactionForce.sqrMagnitude / connectionJoint.breakForce;
            loadMaterial.color = Color.Lerp(Color.green, Color.red, load);
        }
    }

    private void OnDestroy()
    {
        meshGenerator.NeedMeshRegeneration -= RegenerateMesh;
    }

    private void OnDisable()
    {
        meshGenerator.NeedMeshRegeneration -= RegenerateMesh;
    }



    private void OnJointBreak2D(Joint2D brokenJoint)
    {
        if (brokenJoint != connectionJoint)
        {
            return;
        }

        StartCoroutine(HandleJointBreaking());

        connectionJoint = null;
    }

    private IEnumerator HandleJointBreaking()
    {
        // wait to exit physics callback
        yield return null;

        var displToThis = (transform.position - meshTransform.position) / 2;
        var scale = meshTransform.localScale;
        scale.x /= 2;

        var rot = meshTransform.rotation;

        CreateBlock(connection.Type.ToString(), meshTransform.position + displToThis, rot, scale, rigidbody, true);
        CreateBlock(connection.Type.ToString(), meshTransform.position - displToThis, rot, scale, otherRigidbody, false);

        DestroyImmediate(mesh);
        mesh = null;

        if (connectedBlock != null)
        {
            DestroyImmediate(connectedBlock);
        }

        DestroyImmediate(this);
    }

    private void CreateBlock(string name, Vector3 position, Quaternion rotation, Vector3 localScale, Rigidbody2D attachTo, bool left)
    {
        var go = new GameObject("Broken " + name);
        go.layer = gameObject.layer;
        var trnsf = go.transform;
        trnsf.SetParent(levelManager.MeshParent);
        trnsf.localScale = localScale;
        trnsf.position = position;
        trnsf.rotation = rotation;

        meshGenerator.GenerateMeshFor(
            connection: connection,
            atGameObject: go,
            with: left ? BridgeMeshManager.Parts.Left : BridgeMeshManager.Parts.Right,
            useLoadMaterial: false
        );

        go.AddComponent<Rigidbody2D>();
        var hinge = go.AddComponent<HingeJoint2D>();
        hinge.autoConfigureConnectedAnchor = false;
        hinge.connectedBody = attachTo;
        hinge.anchor = (left ? Vector2.left : Vector2.right) / 2f;
        hinge.connectedAnchor = Vector2.zero;
    }
}
