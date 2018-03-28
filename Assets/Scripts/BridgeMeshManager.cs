using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeMeshManager : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> indices;

    public LevelManager LevelManager { get; set; }
    public Material RoadMaterial { get; set; }
    public Material WoodMaterial { get; set; }
    public Material SteelMaterial { get; set; }

    private Level Level { get { return LevelManager?.level; } }
    private Solution solution { get { return LevelManager?.solution; } }


    private void Start()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.materials = new[] {
            RoadMaterial,
            // WoodMaterial,
            // SteelMaterial
         };
    }

    private void LateUpdate()
    {
        if (mesh == null)
        {
            vertices = new List<Vector3>();
            indices = new List<int>();

            if (GenerateMesh())
            {
                mesh = new Mesh();
                mesh.name = "Bridge";
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
                mesh.RecalculateNormals();

                meshFilter.mesh = mesh;
            }
        }
        else
        {
            UpdateMesh();

            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
        }
    }

    private bool GenerateMesh()
    {
        foreach (var connection in solution.Connections)
        {
            if (connection.Type == ConnectionType.Road)
            {
                Rigidbody2D rigidbody;
                if (!LevelManager.Connection2Rigidbody.TryGetValue(connection, out rigidbody))
                {
                    return false;
                }
                var anchorA = LevelManager.AnchorId2Rigidbody[connection.IdA].transform;
                var anchorB = LevelManager.AnchorId2Rigidbody[connection.IdB].transform;

                AddOpenBox(anchorA.position, anchorB.position, 0.5f, LevelManager.RoadHeight);
            }
        }

        return true;
    }

    private void UpdateMesh()
    {
        var vertexOffset = 0;
        foreach (var connection in solution.Connections)
        {
            if (connection.Type == ConnectionType.Road)
            {
                var anchorA = LevelManager.AnchorId2Rigidbody[connection.IdA].transform;
                var anchorB = LevelManager.AnchorId2Rigidbody[connection.IdB].transform;

                var v1 = anchorA.position - Vector3.back * 0.5f;
                var v2 = anchorA.position + Vector3.back * 0.5f;

                var v3 = anchorB.position + Vector3.back * 0.5f;
                var v4 = anchorB.position - Vector3.back * 0.5f;

                vertexOffset = UpdateOpenBox(vertexOffset, anchorA.position, anchorB.position, 0.5f, LevelManager.RoadHeight);
            }
        }
    }

    void AddOpenBox(Vector3 left, Vector3 right, float width, float height)
    {
        AddQuad(
            left + Vector3.back * width,
            left - Vector3.back * width,
            right - Vector3.back * width,
            right + Vector3.back * width
        );
        AddQuad(
            left + Vector3.forward * width,
            right + Vector3.forward * width,
            right + Vector3.forward * width + Vector3.down * height,
            left + Vector3.forward * width + Vector3.down * height
        );
    }
    int UpdateOpenBox(int offset, Vector3 left, Vector3 right, float width, float height)
    {
        return UpdateQuad(offset, left + Vector3.back * width,
            left - Vector3.back * width,
            right - Vector3.back * width,
            right + Vector3.back * width) + 6;
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        AddTriangle(v1, v3, v2);
        AddTriangle(v1, v4, v3);
    }

    int UpdateQuad(int offset, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        UpdateTriangle(offset, v1, v3, v2);
        UpdateTriangle(offset + 3, v1, v4, v3);

        return offset + 6;
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var i = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        indices.Add(i++);
        indices.Add(i++);
        indices.Add(i++);
    }

    void UpdateTriangle(int offset, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        vertices[offset] = v1;
        vertices[offset + 1] = v2;
        vertices[offset + 2] = v3;
    }
}
