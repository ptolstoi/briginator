using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public LevelManager LevelManager;
    public BridgeMeshManager MeshGenerator;
    [Header("Animation")]
    public float AnimationSpeed = 0.5f;
    public float amplitude = 0.5f;
    public float sinSpeed = 1.5f;
    public float sinFrequence = 0.6f;
    public float sinAmplitude = 0.25f;

    private Mesh mesh;
    private List<Vector3> vertices;
    void Start()
    {
        MeshGenerator.GenerateWaterMesh(
            atGameObject: gameObject,
            position: Vector3.down * 2,
            size: new Vector3(
                Mathf.Abs(LevelManager.level.StartLand.x - LevelManager.level.EndLand.x),
                10,
                10
            )
        );

        mesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = new List<Vector3>(mesh.vertices);
    }

    void LateUpdate()
    {
        for (var i = 0; i < vertices.Count; ++i)
        {
            var v = vertices[i];
            if (v.y < -3f)
            {
                continue;
            }
            v.y = -1.5f + (Mathf.PerlinNoise(v.x * 1.5f, v.z + Time.time * AnimationSpeed) - 0.5f) * amplitude +
                    Mathf.Sin(Time.time * sinSpeed + v.z * sinFrequence) * sinAmplitude;

            vertices[i] = v;
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateNormals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponentInParent<Rigidbody2D>().gameObject.SetActive(false);
    }
}
