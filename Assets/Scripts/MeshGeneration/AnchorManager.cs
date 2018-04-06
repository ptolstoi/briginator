using UnityEngine;
using System;
using System.Linq;

public class AnchorManager : MonoBehaviour
{
    public BridgeMeshManager meshGenerator;
    public LevelManager levelManager;
    public GameObject anchorGameObject;
    public Anchor anchor;
    public bool fixedAnchor;

    private GameObject mesh;
    private Transform meshTransform;
    private Transform anchorTransform;

    private void Start()
    {
        mesh = new GameObject("Mesh" + (fixedAnchor ? " Fixed" : "") + " Anchor");
        mesh.layer = gameObject.layer;
        meshTransform = mesh.transform;
        meshTransform.SetParent(levelManager.MeshParent);
        anchorTransform = anchorGameObject.transform;

        var hasRoad = levelManager.solution.Connections.Any(
            conn => (conn.IdA == anchor.Id || conn.IdB == anchor.Id)
                    && conn.Type == ConnectionType.Road
        );

        meshGenerator.GenerateMeshFor(
            anchor: anchor,
            atGameObject: mesh,
            hasRoadConnections: hasRoad
        );
    }

    private void LateUpdate()
    {
        if (mesh != null)
        {
            meshTransform.position = anchorTransform.position;
            meshTransform.rotation = anchorTransform.rotation;
        }
    }
}