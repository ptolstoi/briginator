using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BridgeMeshManager : MonoBehaviour
{
    public enum Parts
    {
        All,
        Left,
        Right
    }

    public LevelManager LevelManager;
    public Material RoadMaterial;
    public Material WoodMaterial;
    public Material SteelMaterial;

    delegate void GenerateMeshDelegate(ref List<Vector3> vertices, ref Dictionary<Material, List<int>> indices);


    public void GenerateMeshFor(Connection connection, GameObject atGameObject, Parts with = Parts.All)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: connection.ToString(),
            parts: with,
            generate: (ref List<Vector3> vertices, ref Dictionary<Material, List<int>> indices) =>
            {
                foreach (var tuple in MeshPartsForConnection(connection, with))
                {
                    var material = tuple.Item1;
                    var part = tuple.Item2;
                    var indexBuffer = indices.GetOrDefault(material, () => new List<int>());
                    part.AddToBuffers(ref vertices, ref indexBuffer);
                    indices[material] = indexBuffer;
                }
            }
        );
    }

    public void GenerateMeshFor(Anchor anchor, GameObject atGameObject)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: "Anchor",
            parts: Parts.All, //TODO macht kein sinn
            generate: (ref List<Vector3> vertices, ref Dictionary<Material, List<int>> indices) =>
            {
                foreach (var tuple in MeshPartsForAnchor())
                {
                    var material = tuple.Item1;
                    var part = tuple.Item2;
                    var indexBuffer = indices.GetOrDefault(material, () => new List<int>());
                    part.AddToBuffers(ref vertices, ref indexBuffer);
                    indices[material] = indexBuffer;
                }
            }
        );
    }

    private void GenerateMesh(GameObject gameObject, string meshName, Parts parts, GenerateMeshDelegate generate)
    {
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        var meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.sharedMaterials = new[] {
            RoadMaterial,
            WoodMaterial,
            SteelMaterial
         };

        var mesh = new Mesh();
        mesh.name = "Mesh " + meshName + " " + parts;
        mesh.subMeshCount = meshRenderer.sharedMaterials.Length;

        // generate mesh
        var vertices = new List<Vector3>();
        var indices = new Dictionary<Material, List<int>>();

        generate(ref vertices, ref indices);

        mesh.SetVertices(vertices);

        foreach (var pair in indices)
        {
            var idx = Array.IndexOf(meshRenderer.sharedMaterials, pair.Key);

            mesh.SetIndices(pair.Value.ToArray(), MeshTopology.Triangles, idx);
        }


        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    IEnumerable<Tuple<Material, MeshPart>> MeshPartsForConnection(Connection connection, Parts parts)
    {
        var left = Vector3.left / 2;
        var right = Vector3.right / 2;

        var roadHeight = LevelManager.RoadHeight;
        var roadWidth = 2f;

        var steelHeight = 0.25f;
        var steelWidth = steelHeight;

        var forward = Vector3.forward * (roadWidth / 2 - steelHeight / 2);
        var back = Vector3.back * (roadWidth / 2 - steelHeight / 2);

        if (connection.Type == ConnectionType.Road)
        {
            yield return Tuple.Create(
                RoadMaterial,
                new RoadBox(
                    left: left + Vector3.down * (roadHeight / 2),
                    right: right + Vector3.down * (roadHeight / 2),
                    width: roadWidth,
                    height: roadHeight
                ) as MeshPart
            );
        }
        else if (connection.Type == ConnectionType.Steel)
        {
            yield return Tuple.Create(
                SteelMaterial,
                new SteelBox(
                    left: left + forward,
                    right: right + forward,
                    width: steelWidth,
                    height: steelHeight,
                    innerWidth: steelWidth * 0.5f,
                    innerHeight: steelHeight * 0.5f
                ) as MeshPart
            );
            yield return Tuple.Create(
                SteelMaterial,
                new SteelBox(
                    left: left + back,
                    right: right + back,
                    width: steelWidth,
                    height: steelHeight,
                    innerWidth: steelWidth * 0.5f,
                    innerHeight: steelHeight * 0.5f
                ) as MeshPart
            );
        }
        else if (connection.Type == ConnectionType.Wood)
        {
            yield return Tuple.Create(
                WoodMaterial,
                new SteelBox(
                    left: left + forward,
                    right: right + forward,
                    width: steelWidth,
                    height: steelHeight,
                    innerWidth: steelWidth * 0.5f,
                    innerHeight: steelHeight * 0.5f
                ) as MeshPart
            );
            yield return Tuple.Create(
                WoodMaterial,
                new SteelBox(
                    left: left + back,
                    right: right + back,
                    width: steelWidth,
                    height: steelHeight,
                    innerWidth: steelWidth * 0.5f,
                    innerHeight: steelHeight * 0.5f
                ) as MeshPart
            );
        }
    }

    IEnumerable<Tuple<Material, MeshPart>> MeshPartsForAnchor()
    {
        var steelHeight = 0.25f;
        var steelWidth = steelHeight;
        var roadWidth = 2f;
        var forward = Vector3.forward * (roadWidth / 2 - steelHeight / 2);
        var back = Vector3.back * (roadWidth / 2 - steelHeight / 2);

        yield return Tuple.Create(
                RoadMaterial,
                new AnchorMesh(
                    position: Vector3.back * (roadWidth / 2 - steelHeight / 2),
                    radius: steelHeight,
                    depth: steelWidth * 1.05f
                ) as MeshPart
            );
        yield return Tuple.Create(
            RoadMaterial,
            new AnchorMesh(
                position: Vector3.forward * (roadWidth / 2 - steelHeight / 2),
                radius: steelHeight,
                depth: steelWidth * 1.05f
            ) as MeshPart
        );
        yield return Tuple.Create(
            RoadMaterial,
            new SteelBox(
                left: back,
                right: forward,
                width: steelWidth,
                height: steelHeight,
                innerWidth: steelWidth * 0.5f,
                innerHeight: steelHeight * 0.5f
            ) as MeshPart
        );
    }


    IEnumerable<Tuple<Material, MeshPart>> IterateOverParts()
    {
        var solution = new Solution();
        var roadHeight = LevelManager.RoadHeight;
        var roadWidth = 2f;

        var steelHeight = 0.25f;
        var steelWidth = steelHeight;

        foreach (var connection in solution.Connections)
        {
            var anchorA = LevelManager.AnchorId2Rigidbody[connection.IdA].transform;
            var anchorB = LevelManager.AnchorId2Rigidbody[connection.IdB].transform;
            var forward = Vector3.forward * (roadWidth / 2 - steelHeight / 2);
            var back = Vector3.back * (roadWidth / 2 - steelHeight / 2);

            yield return Tuple.Create(
                RoadMaterial,
                new AnchorMesh(
                    position: anchorA.position + Vector3.back * (roadWidth / 2 - steelHeight / 2),
                    radius: steelHeight,
                    depth: steelWidth * 1.05f
                ) as MeshPart
            );
            yield return Tuple.Create(
                RoadMaterial,
                new AnchorMesh(
                    position: anchorB.position + Vector3.back * (roadWidth / 2 - steelHeight / 2),
                    radius: steelHeight,
                    depth: steelWidth * 1.05f
                ) as MeshPart
            );
            yield return Tuple.Create(
                RoadMaterial,
                new AnchorMesh(
                    position: anchorA.position + Vector3.forward * (roadWidth / 2 - steelHeight / 2),
                    radius: steelHeight,
                    depth: steelWidth * 1.05f
                ) as MeshPart
            );
            yield return Tuple.Create(
                RoadMaterial,
                new AnchorMesh(
                    position: anchorB.position + Vector3.forward * (roadWidth / 2 - steelHeight / 2),
                    radius: steelHeight,
                    depth: steelWidth * 1.05f
                ) as MeshPart
            );
            yield return Tuple.Create(
                RoadMaterial,
                new SteelBox(
                    left: anchorB.position + back,
                    right: anchorB.position + forward,
                    width: steelWidth,
                    height: steelHeight,
                    innerWidth: steelWidth * 0.5f,
                    innerHeight: steelHeight * 0.5f
                ) as MeshPart
            );

            if (connection.Type == ConnectionType.Road)
            {
                var left = anchorA.position + Vector3.down * (roadHeight / 2);
                var right = anchorB.position + Vector3.down * (roadHeight / 2);

                yield return Tuple.Create(
                    RoadMaterial,
                    new RoadBox(
                        left: left,
                        right: right,
                        width: roadWidth,
                        height: roadHeight
                    ) as MeshPart
                );
            }
            else if (connection.Type == ConnectionType.Steel)
            {

                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBox(
                        left: anchorA.position + forward,
                        right: anchorB.position + forward,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBox(
                        left: anchorA.position + back,
                        right: anchorB.position + back,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );

            }
            else if (connection.Type == ConnectionType.Wood)
            {
                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBox(
                        left: anchorA.position + Vector3.forward * (roadWidth / 2 - steelHeight / 2),
                        right: anchorB.position + Vector3.forward * (roadWidth / 2 - steelHeight / 2),
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBox(
                        left: anchorA.position + Vector3.back * (roadWidth / 2 - steelHeight / 2),
                        right: anchorB.position + Vector3.back * (roadWidth / 2 - steelHeight / 2),
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );

            }
        }
    }
}
