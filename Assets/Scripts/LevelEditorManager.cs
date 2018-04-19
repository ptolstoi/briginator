using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class LevelEditorManager : MonoBehaviour
{
    [Header("GridSize")]

    public float gridSize = 0.5f;

    [Header("Wiring")]
    public LevelManager levelManager;

    public Transform cursor;

    float depth = 5;

    private float hexHeight => Mathf.Sqrt(3) / 2 * gridSize;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> indices;

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

    public void ModeChanged(LevelManagerMode mode)
    {
        meshRenderer.enabled = mode == LevelManagerMode.Edit;
        if (mode == LevelManagerMode.Edit)
        {
            RegenerateMesh();
        }
    }

    private void Update()
    {
        var gridPos = GetMouseWorldPosition();

        if (!gridPos.HasValue || levelManager.Mode == LevelManagerMode.Play)
        {
            cursor.gameObject.SetActive(false);
            return;
        }

        cursor.gameObject.SetActive(true);

        cursor.position = gridPos.Value;
    }

    private void RegenerateMesh()
    {
        vertices = new List<Vector3>();
        indices = new List<int>();


        foreach (var part in Generator())
        {
            if (part is Box)
            {
                continue;
            }
            part.AddToBuffers(ref vertices, ref indices);
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private IEnumerable<MeshPart> Generator()
    {
        var levelBounds = levelManager.level.Rect;

        var gridPointSize = gridSize / 8;
        var gridBoxSize = gridSize / 6 / 4;

        yield return new Quad(
            position: Vector3.zero + Vector3.forward * depth,
            up: Vector3.up,
            right: Vector3.right,
            width: gridPointSize * 8,
            height: gridPointSize * 8
        );

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

                var height = levelBounds.height - (levelBounds.min.y + y);

                if (x == levelBounds.min.x)
                {
                    var width = levelBounds.width - (yEven ? gridSize / 2f : 0);

                    var count = 100; //Mathf.Floor(width / gridSize);

                    yield return new Box(
                        gridPoint - Vector3.right * gridSize * count,
                        gridPoint + Vector3.right * gridSize * count,
                        gridBoxSize,
                        gridBoxSize);

                    if (!yEven)
                    {
                        count = 100; //Mathf.Floor(height / hexHeight);
                        yield return new Box(
                            gridPoint - (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                            gridPoint + (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                            gridBoxSize,
                            gridBoxSize);
                    }
                }
                else if (y == levelBounds.min.y)
                {
                    var width = levelBounds.width - (yEven ? gridSize / 2f : 0) - (x - levelBounds.min.x);

                    var count = Math.Min(
                        Mathf.Floor(height / hexHeight),
                        Mathf.Floor(width / (gridSize / 2))
                    );
                    count = 100;

                    yield return new Box(
                        gridPoint - (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                        gridPoint + (Vector3.up * hexHeight + Vector3.right * gridSize / 2) * count,
                        gridBoxSize,
                        gridBoxSize);

                    // width = (x - levelBounds.min.x);

                    // count = Math.Min(
                    //     Mathf.Floor(height / hexHeight),
                    //     Mathf.Floor(width / (gridSize / 2))
                    // );

                    yield return new Box(
                        gridPoint - (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                        gridPoint + (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                        gridBoxSize,
                        gridBoxSize);
                }
                else if (gridPoint.x <= levelBounds.max.x && gridPoint.x + gridSize > levelBounds.max.x)
                {
                    var count = 100; //Mathf.Floor(height / hexHeight);

                    yield return new Box(
                        gridPoint - (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                        gridPoint + (Vector3.up * hexHeight + Vector3.left * gridSize / 2) * count,
                        gridBoxSize,
                        gridBoxSize);
                }

                if (gridPoint.x > levelBounds.max.x)
                {
                    continue;
                }


                yield return new AnchorMesh(gridPoint, gridPointSize, 0.1f, 6);
            }
        }
    }

    Vector3? GetMouseWorldPosition()
    {
        var bounds = levelManager.level.Rect;
        var mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z + depth;

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition) - (Vector3)bounds.min;

        mouseWorldPosition.y = Mathf.Round(mouseWorldPosition.y / hexHeight) * hexHeight;

        var posY = Mathf.RoundToInt(mouseWorldPosition.y / hexHeight);
        if (posY % 2 != 0)
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize - 0.5f) * gridSize + gridSize / 2;
        }
        else
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize) * gridSize;
        }

        mouseWorldPosition = mouseWorldPosition + (Vector3)bounds.min;

        bounds.width += 0.0001f;
        bounds.height += 0.0001f;

        if (bounds.Contains(mouseWorldPosition))
        {
            return mouseWorldPosition;
        }

        return null;
    }
}