using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class EditorMeshGenerator : MonoBehaviour
{
    [Header("Wiring")]
    public LevelEditorManager levelEditorManager;

    private Level level => levelEditorManager.levelManager.level;
    private LevelManager levelManager => levelEditorManager.levelManager;

    private List<Vector3> vertices;
    private List<int> indices;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "EditorMesh";

        meshRenderer = gameObject.EnsureComponent<MeshRenderer>();
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        RegenerateMesh();

        meshFilter.sharedMesh = mesh;
    }

    private void OnEnable()
    {
        RegenerateMesh();
    }

    private void OnDisable()
    {
        if (mesh != null)
        {
            meshRenderer.enabled = false;
        }
    }

    public void RegenerateMesh()
    {
        if (mesh == null)
        {
            return;
        }

        vertices = new List<Vector3>();
        indices = new List<int>();

        foreach (var part in Generator())
        {
            part.AddToBuffers(ref vertices, ref indices);
        }

        mesh.SetIndices(null, MeshTopology.Triangles, 0);
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshRenderer.enabled = true;
    }

    private IEnumerable<MeshPart> Generator()
    {
        var levelBounds = level.Rect;
        var gridSize = levelManager.gridSize;
        var hexHeight = levelManager.hexHeight;
        var depth = -levelEditorManager.depth;

        var gridPointSize = gridSize / 8;
        var gridBoxSize = gridSize / 6 / 4;

        // yield return new Quad(
        //     position: Vector3.zero + Vector3.forward * depth,
        //     up: Vector3.up,
        //     right: Vector3.right,
        //     width: gridPointSize * 8,
        //     height: gridPointSize * 8
        // );

        for (var x = levelBounds.min.x; x <= levelBounds.max.x; x += gridSize)
        {
            var yEven = true;
            for (var y = levelBounds.min.y; y <= levelBounds.max.y; y += hexHeight)
            {
                yEven = !yEven;

                var gridPoint = Vector3.up * y + Vector3.right * x + Vector3.forward * depth;

                if (yEven)
                {
                    gridPoint += Vector3.right * gridSize / 2.0f;
                }

                // var height = levelBounds.height - (levelBounds.min.y + y);

                // if (x == levelBounds.min.x)
                // {
                //     var width = levelBounds.width - (yEven ? gridSize / 2f : 0);

                //     var count = 100; //Mathf.Floor(width / gridSize);

                //     yield return new Box(
                //         gridPoint - Vector3.right * gridSize * count,
                //         gridPoint + Vector3.right * gridSize * count,
                //         gridBoxSize,
                //         gridBoxSize);

                //     if (!yEven)
                //     {
                //         count = 100; //Mathf.Floor(height / hexHeight);
                //         yield return new Box(
                //             gridPoint - (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                //             gridPoint + (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                //             gridBoxSize,
                //             gridBoxSize);
                //     }
                // }
                // else if (y == levelBounds.min.y)
                // {
                //     var width = levelBounds.width - (yEven ? gridSize / 2f : 0) - (x - levelBounds.min.x);

                //     var count = Math.Min(
                //         Mathf.Floor(height / hexHeight),
                //         Mathf.Floor(width / (gridSize / 2))
                //     );
                //     count = 100;

                //     yield return new Box(
                //         gridPoint - (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                //         gridPoint + (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                //         gridBoxSize,
                //         gridBoxSize);

                //     // width = (x - levelBounds.min.x);

                //     // count = Math.Min(
                //     //     Mathf.Floor(height / hexHeight),
                //     //     Mathf.Floor(width / (gridSize / 2))
                //     // );

                //     yield return new Box(
                //         gridPoint - (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                //         gridPoint + (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                //         gridBoxSize,
                //         gridBoxSize);
                // }
                // else if (gridPoint.x <= levelBounds.max.x && gridPoint.x + gridSize > levelBounds.max.x)
                // {
                //     var count = 100; //Mathf.Floor(height / hexHeight);

                //     yield return new Box(
                //         gridPoint - (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                //         gridPoint + (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                //         gridBoxSize,
                //         gridBoxSize);
                // }

                if (gridPoint.x > levelBounds.max.x)
                {
                    continue;
                }


                yield return new AnchorMesh(gridPoint, gridPointSize, 0.1f, 6);
            }
        }
    }
}